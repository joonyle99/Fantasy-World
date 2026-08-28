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
    /// 한 NPC. 시야(거리 + 각도 + 시선 차단)와 근처 울음으로 구스를 인지하고,
    /// JoonyleGameDevKit 의 StateMachine{T} 로 반응한다.
    /// Idle → (구스 목격 / 근처 울음) → Alerted(추격) → 시야 상실 → Returning(귀가) → Idle.
    /// 이동은 NavMeshAgent 가, 걷기/뛰기 애니메이션은 실제 이동 속력에 블렌드된다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public sealed class Npc : MonoBehaviour
    {
        private const string SPEED_PARAM = "Speed";
        private const float HONK_ALERT_MEMORY = 0.5f;

        [Header("Perception")]
        [SerializeField] private float _viewDistance = 6f;
        [SerializeField] private float _viewAngle = 100f;
        [SerializeField] private LayerMask _sightBlockers;
        [SerializeField] private float _eyeHeight = 1.6f;
        [SerializeField] private float _honkHearingRadius = 9f;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 1.8f;
        [SerializeField] private float _chaseSpeed = 3.6f;
        [SerializeField] private float _catchDistance = 1f;
        [SerializeField] private float _loseSightGrace = 2f;

        [Header("Animation")]
        [Tooltip("이동 속력을 Speed 파라미터에 반영할 때의 완충 시간(초)")]
        [SerializeField] private float _locomotionDamp = 0.12f;

        private GooseController _goose;
        private NpcRegistry _registry;
        private GameplayEventBus _eventBus;
        private NavMeshAgent _agent;
        private Animator _animator;
        private int _speedHash;

        private StateMachine<Npc> _stateMachine;
        private readonly IdleState _idleState = new();
        private readonly AlertedState _alertedState = new();
        private readonly ReturningState _returningState = new();

        private Vector3 _homePosition;
        private NpcState _currentState = NpcState.Idle;
        private float _honkHeardAt = float.NegativeInfinity;

        public NpcState State => _currentState;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _speedHash = Animator.StringToHash(SPEED_PARAM);
        }

        /// <summary>AreaNpcController 가 구역 로드 시 호출한다.</summary>
        public void Initialize(GooseController goose, NpcRegistry registry, GameplayEventBus eventBus)
        {
            _goose = goose;
            _registry = registry;
            _eventBus = eventBus;
            _homePosition = transform.position;

            _registry.Register(this);
            _eventBus.Subscribe<GooseHonked>(OnHonk);

            BuildStateMachine();
        }

        private void BuildStateMachine()
        {
            _stateMachine = new StateMachine<Npc>(this);
            _stateMachine.AddState(_idleState);
            _stateMachine.AddState(_alertedState);
            _stateMachine.AddState(_returningState);

            _stateMachine.AddTransition<IdleState, AlertedState>(ShouldNotice);
            _stateMachine.AddTransition<AlertedState, ReturningState>(() => _alertedState.LostSightExpired);
            _stateMachine.AddTransition<ReturningState, AlertedState>(ShouldNotice);
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
            if (_stateMachine == null)
                return;

            _stateMachine.Update(Time.deltaTime);

            SyncPublicState();
            DriveLocomotionAnimation();
        }

        // ============== 인지 ==============

        /// <summary>구스를 알아챌 조건 — 직접 목격했거나 방금 근처에서 울음을 들었다.</summary>
        private bool ShouldNotice()
        {
            return CanSeeGoose() || HeardHonkRecently();
        }

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

        private bool HeardHonkRecently()
        {
            return Time.time - _honkHeardAt < HONK_ALERT_MEMORY;
        }

        private void OnHonk(GooseHonked honk)
        {
            if (Vector3.Distance(transform.position, honk.Position) <= _honkHearingRadius)
                _honkHeardAt = Time.time;
        }

        // ============== 이동 (상태가 호출) ==============

        private void BeginChase()
        {
            if (!_agent.isOnNavMesh)
                return;

            _agent.speed = _chaseSpeed;
            _agent.isStopped = false;
        }

        /// <summary>구스는 계속 움직이므로 매 프레임 목적지를 갱신한다.</summary>
        private void UpdateChaseTarget()
        {
            if (_agent.isOnNavMesh)
                _agent.SetDestination(_goose.transform.position);
        }

        private void BeginReturnHome()
        {
            if (!_agent.isOnNavMesh)
                return;

            _agent.speed = _patrolSpeed;
            _agent.isStopped = false;
            _agent.SetDestination(_homePosition); // 집은 고정 목적지 — 한 번만 지정
        }

        private void Halt()
        {
            if (_agent.isOnNavMesh)
                _agent.isStopped = true;
        }

        /// <summary>현재 목적지에 (정지 거리 이내로) 도착했는지.</summary>
        private bool HasReachedDestination()
        {
            return _agent.isOnNavMesh
                && !_agent.pathPending
                && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f;
        }

        private bool IsGooseWithinReach()
        {
            var delta = _goose.transform.position - transform.position;
            delta.y = 0f;
            return delta.magnitude < _catchDistance;
        }

        // ============== 애니메이션 / 상태 공표 ==============

        private void DriveLocomotionAnimation()
        {
            // 실제 이동 속력을 그대로 블렌드 트리에 넘긴다 (Idle ↔ Walk ↔ Jog)
            _animator.SetFloat(_speedHash, _agent.velocity.magnitude, _locomotionDamp, Time.deltaTime);
        }

        /// <summary>상태 인스턴스 변화를 공개 enum 으로 옮기고, 바뀌었을 때만 이벤트를 낸다.</summary>
        private void SyncPublicState()
        {
            var mapped = MapState(_stateMachine.CurrState);
            if (mapped == _currentState)
                return;

            _currentState = mapped;
            _eventBus.Publish(new NpcStateChanged(this, mapped));
        }

        private NpcState MapState(StateBase<Npc> state)
        {
            if (state == _alertedState)
                return NpcState.Alerted;

            if (state == _returningState)
                return NpcState.Returning;

            return NpcState.Idle;
        }

        private void ReportCaughtGoose()
        {
            _eventBus.Publish(new GooseCaught(this));
        }

        // ============== 상태 ==============

        private sealed class IdleState : StateBase<Npc>
        {
            public override void Enter(Npc owner) => owner.Halt();
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
                owner.BeginChase();
            }

            public override void Update(Npc owner, float deltaTime)
            {
                owner.UpdateChaseTarget();

                // 이번 추격에서 한 번만 발행
                if (!_caught && owner.IsGooseWithinReach())
                {
                    _caught = true;
                    owner.ReportCaughtGoose();
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
                owner.BeginReturnHome();
            }

            public override void Update(Npc owner, float deltaTime)
            {
                if (owner.HasReachedDestination())
                    ArrivedHome = true;
            }

            public override void Exit(Npc owner) { }
        }
    }
}
