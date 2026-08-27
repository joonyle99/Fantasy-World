using VContainer;
using UnityEngine;
using FantasyWorld.Core;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스의 물리 이동·회전·울음 뷰. CharacterController 로 걷고, 카메라 기준으로 입력을 해석한다.
    /// 상호작용 판정 등 게임 로직은 시스템 계층에 위임한다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class GooseController : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private float _turnSpeed = 720f;
        [SerializeField] private float _gravity = -20f;

        [Header("Animation")]
        [SerializeField] private string _stateParam = "State";

        [Header("Interaction")]
        [SerializeField] private Transform _beakPoint;

        /// <summary>부리 위치. InteractionSystem 이 집기 판정·부착 기준으로 쓴다.</summary>
        public Transform BeakPoint => _beakPoint;

        private InputService _inputService;
        private AudioService _audioService;
        private GameplayEventBus _eventBus;

        private CharacterController _controller;
        private Animator _animator;
        private Transform _cameraTransform;
        private float _verticalVelocity;

        [Inject]
        public void Construct(InputService inputService, AudioService audioService, GameplayEventBus eventBus)
        {
            _inputService = inputService;
            _audioService = audioService;
            _eventBus = eventBus;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            _inputService.HonkPerformed += OnHonk;
        }

        private void OnDisable()
        {
            _inputService.HonkPerformed -= OnHonk;
        }

        private void Start()
        {
            _cameraTransform = Camera.main != null ? Camera.main.transform : null;
        }

        private void Update()
        {
            var direction = ResolveMoveDirection(_inputService.MoveInput);

            RotateTowards(direction);
            ApplyMovement(direction);
            UpdateAnimator(direction);
        }

        /// <summary>입력(로컬)을 카메라 기준 월드 방향으로 변환한다. 수평 성분만 사용한다.</summary>
        private Vector3 ResolveMoveDirection(Vector2 input)
        {
            if (_cameraTransform == null)
                return new Vector3(input.x, 0f, input.y);

            var forward = _cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = _cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return right * input.x + forward * input.y;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            var target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime);
        }

        private void ApplyMovement(Vector3 direction)
        {
            // 접지 상태면 아래로 살짝 눌러 붙여 계단·경사에서 뜨지 않게 한다
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += _gravity * Time.deltaTime;

            var velocity = direction * _moveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateAnimator(Vector3 direction)
        {
            if (_animator == null)
                return;

            _animator.SetFloat(_stateParam, Mathf.Clamp01(direction.magnitude));
        }

        private void OnHonk()
        {
            _eventBus.Publish(new GooseHonked(transform.position));

            // TODO: 울음 애니메이션 + _audioService 로 FMOD 이벤트 재생
        }
    }
}
