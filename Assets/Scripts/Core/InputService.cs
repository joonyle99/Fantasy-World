using System;
using UnityEngine;
using VContainer.Unity;
using UnityEngine.InputSystem;

namespace FantasyWorld.Core
{
    /// <summary>
    /// Player / UI 액션맵을 타입 있는 멤버로 노출하는 facade. 입력 도메인 수준(이동, 상호작용,
    /// 주 행동, 일시정지)까지만 안다 — "울음" 같은 게임 의미 해석은 게임 레이어의 몫이다.
    /// InputActionAsset 은 GameLifetimeScope 에서 RegisterInstance 로 주입된다.
    /// EntryPoint 로 등록되어 컨테이너 생성 시 활성화, 파기 시 해제된다.
    /// </summary>
    public sealed class InputService : IInitializable, IDisposable
    {
        private readonly InputActionMap _playerMap;

        private readonly InputAction _move;
        private readonly InputAction _interact;
        private readonly InputAction _attack;

        // 일시정지는 Player 맵이 꺼져 있어도(로딩/타이틀) 받을 수 있어야 하므로
        // UI 맵의 Cancel 액션 하나만 독립적으로 켠다.
        private readonly InputAction _pause;

        /// <summary>이동 입력. -1~1 범위의 2D 벡터 (x: 좌우, y: 전후).</summary>
        public Vector2 MoveInput => _move.ReadValue<Vector2>();

        /// <summary>상호작용 버튼을 누르고 있는지.</summary>
        public bool InteractHeld => _interact.IsPressed();

        /// <summary>주 행동(Attack) 입력이 발생한 순간. 게임 레이어가 울음 등으로 해석한다.</summary>
        public event Action AttackPerformed;

        /// <summary>일시정지 토글 입력이 발생한 순간 (Esc / 게임패드 Start).</summary>
        public event Action PausePerformed;

        public InputService(InputActionAsset actions)
        {
            _playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            _move = _playerMap.FindAction("Move", throwIfNotFound: true);
            _interact = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _attack = _playerMap.FindAction("Attack", throwIfNotFound: true);

            var uiMap = actions.FindActionMap("UI", throwIfNotFound: true);
            _pause = uiMap.FindAction("Cancel", throwIfNotFound: true);
        }

        public void Initialize()
        {
            _attack.performed += OnAttackPerformed;
            _pause.performed += OnPausePerformed;

            _playerMap.Enable();
            _pause.Enable();
        }

        public void Dispose()
        {
            _attack.performed -= OnAttackPerformed;
            _pause.performed -= OnPausePerformed;

            _playerMap.Disable();
            _pause.Disable();
        }

        /// <summary>게임플레이 이동/상호작용 입력을 끊는다 (일시정지 중).</summary>
        public void SetGameplayInputEnabled(bool enabled)
        {
            if (enabled)
                _playerMap.Enable();
            else
                _playerMap.Disable();
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackPerformed?.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            PausePerformed?.Invoke();
        }
    }
}
