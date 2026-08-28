using UnityEngine;
using UnityEngine.AI;
using JoonyleGameDevKit;

namespace FantasyWorld.World
{
    public enum NpcState
    {
        Idle,
        Alerted,
        Returning,
    }

    /// <summary>
    /// 한 NPC. 시야(거리 + 각도 + 시선 차단)로 구스를 인지하고,
    /// JoonyleGameDevKit 의 StateMachine{T} 로 반응한다.
    /// Idle → (구스 목격 / 근처 울음) → Alerted(추격) → 시야 상실 → Returning(귀가) → Idle.
    /// 이동은 NavMeshAgent 로 처리한다 — 프랍·건물을 우회한다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Npc : MonoBehaviour
    {
        [Header("Perception")]
        [SerializeField] private float _viewDistance = 6f;
        [SerializeField] private float _viewAngle = 100f;
        [SerializeField] private LayerMask _sightBlockers;
        [SerializeField] private float _eyeHeight = 1.4f;

        [Header("Behaviour")]
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _catchDistance = 1f;
        [SerializeField] private float _loseSightGrace = 2f;

        private GooseController _goose;
        private NpcRegistry _registry;
        private GameplayEventBus _eventBus;
        private NavMeshAgent _agent;

        private StateMachine<Npc> _stateMachine;
        private readonly IdleState _idleState = new();
        private readonly AlertedState _alertedState = new();
        private readonly ReturningState _returningState = new();

        private Vector3 _homePosition;
        private NpcState _currentState = NpcState.Idle;

        public NpcState State => _currentState;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>AreaNpcController 가 구역 로드 시 호출한다.</summary>
        public void Initialize(GooseController goose, NpcRegistry registry, GameplayEventBus eventBus)
        {
            _goose = goose;
            _registry = registry;
            _eventBus = eventBus;
            _homePosition = transform.position;

            _agent.speed = _moveSpeed;

            _registry.Register(this);
            _eventBus.Subscribe<GooseHonked>(OnHonk);

            _stateMachine = new StateMachine<Npc>(this);
            _stateMachine.AddState(_idleState);
            _stateMachine.AddState(_alertedState);
            _stateMachine.AddState(_returningState);

            _stateMachine.AddTransition<IdleState, AlertedState>(CanSeeGoose);
            _stateMachine.AddTransition<AlertedState, ReturningState>(() => _alertedState.LostSightExpired);
            _stateMachine.AddTransition<ReturningState, AlertedState>(CanSeeGoose);
            _stateMachine.AddTransition<ReturningState, IdleState>(() => _returningState.ArrivedHome);

            _stateMachine.ChangeState<IdleState>();
        }

        private void OnDestroy()
        {
            if (_stateMachine == null)
                return;

            _registry.Unregister(this);
            _eventBus.Unsubscribe<GooseHonked>(OnHonk);
        }

        private void Update()
        {
            _stateMachine?.Update(Time.deltaTime);
        }

        // ============== 인지 / 이동 ==============

        private bool CanSeeGoose()
        {
            var toGoose = _goose.transform.position - transform.position;

            if (toGoose.sqrMagnitude > _viewDistance * _viewDistance)
                return false;

            if (Vector3.Angle(transform.forward, toGoose) > _viewAngle * 0.5f)
                return false;

            var eye = transform.position + Vector3.up * _eyeHeight;
            if (Physics.Raycast(eye, toGoose.normalized, out var hit, _viewDistance, _sightBlockers))
            {
                if (hit.transform.root != _goose.transform.root)
                    return false;
            }

            return true;
        }

        /// <summary>목적지를 향해 이동을 지시한다. 경로는 NavMeshAgent 가 계산한다.</summary>
        private void MoveTo(Vector3 target)
        {
            if (!_agent.isOnNavMesh)
                return;

            _agent.isStopped = false;
            _agent.SetDestination(target);
        }

        private void StopMoving()
        {
            if (_agent.isOnNavMesh)
                _agent.isStopped = true;
        }

        /// <summary>현재 목적지에 (정지 거리 이내로) 도착했는지.</summary>
        private bool HasArrived()
        {
            return _agent.isOnNavMesh
                && !_agent.pathPending
                && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
        }

        private float Distance2D(Vector3 target)
        {
            var delta = target - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void OnHonk(GooseHonked honk)
        {
            if (_stateMachine.CurrState != _idleState)
                return;

            if (Vector3.Distance(transform.position, honk.Position) < _viewDistance * 1.5f)
                _stateMachine.ChangeState<AlertedState>();
        }

        private void RaiseStateChanged(NpcState state)
        {
            _currentState = state;
            _eventBus.Publish(new NpcStateChanged(this, state));
        }

        // ============== 상태 ==============

        private sealed class IdleState : StateBase<Npc>
        {
            public override void Enter(Npc owner)
            {
                owner.StopMoving();
                owner.RaiseStateChanged(NpcState.Idle);
            }

            public override void Update(Npc owner, float deltaTime) { }
            public override void Exit(Npc owner) { }
        }

        private sealed class AlertedState : StateBase<Npc>
        {
            /// <summary>시야를 놓친 지 유예 시간이 지났는지 (Returning 으로의 전이 조건).</summary>
            public bool LostSightExpired { get; private set; }

            private float _lostSightTimer;
            private bool _caught;

            public override void Enter(Npc owner)
            {
                _lostSightTimer = 0f;
                LostSightExpired = false;
                _caught = false;
                owner.RaiseStateChanged(NpcState.Alerted);
            }

            public override void Update(Npc owner, float deltaTime)
            {
                // 구스는 움직이므로 매 프레임 목적지 갱신
                owner.MoveTo(owner._goose.transform.position);

                // 이번 추격에서 한 번만 발행
                if (!_caught && owner.Distance2D(owner._goose.transform.position) < owner._catchDistance)
                {
                    _caught = true;
                    owner._eventBus.Publish(new GooseCaught(owner));
                }

                if (owner.CanSeeGoose())
                {
                    _lostSightTimer = 0f;
                    return;
                }

                _lostSightTimer += deltaTime;
                if (_lostSightTimer >= owner._loseSightGrace)
                    LostSightExpired = true;
            }

            public override void Exit(Npc owner) { }
        }

        private sealed class ReturningState : StateBase<Npc>
        {
            /// <summary>집에 도착했는지 (Idle 로의 전이 조건).</summary>
            public bool ArrivedHome { get; private set; }

            public override void Enter(Npc owner)
            {
                ArrivedHome = false;
                owner.MoveTo(owner._homePosition); // 집은 고정 목적지 — 한 번만 지정
                owner.RaiseStateChanged(NpcState.Returning);
            }

            public override void Update(Npc owner, float deltaTime)
            {
                if (owner.HasArrived())
                    ArrivedHome = true;
            }

            public override void Exit(Npc owner) { }
        }
    }
}
