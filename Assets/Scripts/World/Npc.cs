using UnityEngine;

namespace FantasyWorld.World
{
    public enum NpcState
    {
        Idle,
        Alerted,
        Returning,
    }

    /// <summary>
    /// 한 NPC. 시야(거리 + 각도 + 시선 차단)로 구스를 인지하고 상태 머신으로 반응한다.
    /// Idle → (구스 목격 / 근처 울음) → Alerted(추격) → 시야 상실 → Returning(귀가) → Idle.
    /// 이동은 지금은 단순 MoveTowards. 나중에 NavMeshAgent 로 교체.
    /// </summary>
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
        [SerializeField] private float _homeArriveDistance = 0.2f;

        private GooseController _goose;
        private NpcRegistry _registry;
        private GameplayEventBus _eventBus;

        private NpcState _state = NpcState.Idle;
        private Vector3 _homePosition;
        private float _lostSightTimer;
        private bool _caughtThisChase;
        private bool _initialized;

        public NpcState State => _state;

        /// <summary>AreaNpcController 가 구역 로드 시 호출한다.</summary>
        public void Initialize(GooseController goose, NpcRegistry registry, GameplayEventBus eventBus)
        {
            _goose = goose;
            _registry = registry;
            _eventBus = eventBus;
            _homePosition = transform.position;

            _registry.Register(this);
            _eventBus.Subscribe<GooseHonked>(OnHonk);
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (!_initialized)
                return;

            _registry.Unregister(this);
            _eventBus.Unsubscribe<GooseHonked>(OnHonk);
        }

        private void Update()
        {
            if (!_initialized)
                return;

            var canSeeGoose = CanSeeGoose();

            switch (_state)
            {
                case NpcState.Idle:
                    if (canSeeGoose)
                        SetState(NpcState.Alerted);
                    break;

                case NpcState.Alerted:
                    Chase(canSeeGoose);
                    break;

                case NpcState.Returning:
                    ReturnHome(canSeeGoose);
                    break;
            }
        }

        private void Chase(bool canSeeGoose)
        {
            MoveToward(_goose.transform.position);

            // 이번 추격에서 한 번만 발행 (매 프레임 스팸 방지)
            if (!_caughtThisChase && Distance2D(_goose.transform.position) < _catchDistance)
            {
                _caughtThisChase = true;
                _eventBus.Publish(new GooseCaught(this));
            }

            if (canSeeGoose)
            {
                _lostSightTimer = 0f;
                return;
            }

            _lostSightTimer += Time.deltaTime;
            if (_lostSightTimer >= _loseSightGrace)
                SetState(NpcState.Returning);
        }

        private void ReturnHome(bool canSeeGoose)
        {
            if (canSeeGoose)
            {
                SetState(NpcState.Alerted);
                return;
            }

            MoveToward(_homePosition);

            if (Distance2D(_homePosition) < _homeArriveDistance)
                SetState(NpcState.Idle);
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

        private void MoveToward(Vector3 target)
        {
            var flatTarget = new Vector3(target.x, transform.position.y, target.z);
            transform.position = Vector3.MoveTowards(transform.position, flatTarget, _moveSpeed * Time.deltaTime);

            var direction = flatTarget - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        private float Distance2D(Vector3 target)
        {
            var delta = target - transform.position;
            delta.y = 0f;
            return delta.magnitude;
        }

        private void OnHonk(GooseHonked honk)
        {
            if (_state != NpcState.Idle)
                return;

            if (Vector3.Distance(transform.position, honk.Position) < _viewDistance * 1.5f)
                SetState(NpcState.Alerted);
        }

        private void SetState(NpcState next)
        {
            if (_state == next)
                return;

            if (next == NpcState.Alerted)
            {
                _lostSightTimer = 0f;
                _caughtThisChase = false;
            }

            _state = next;
            _eventBus.Publish(new NpcStateChanged(this, next));
        }
    }
}
