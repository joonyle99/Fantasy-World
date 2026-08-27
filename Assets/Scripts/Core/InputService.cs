using System;
using UnityEngine;
using VContainer.Unity;
using UnityEngine.InputSystem;

namespace FantasyWorld.Core
{
    /// <summary>
    /// Input System 액션을 게임 의미 단위(이동, 상호작용, 울음)로 노출한다.
    /// InputActionAsset 은 GameLifetimeScope 에서 RegisterInstance 로 주입된다.
    /// EntryPoint 로 등록되어 컨테이너 생성 시 활성화, 파기 시 해제된다.
    /// </summary>
    public sealed class InputService : IInitializable, IDisposable
    {
        private readonly InputActionMap _playerMap;

        private readonly InputAction _move;
        private readonly InputAction _interact;
        private readonly InputAction _honk;

        /// <summary>이동 입력. -1~1 범위의 2D 벡터 (x: 좌우, y: 전후).</summary>
        public Vector2 MoveInput => _move.ReadValue<Vector2>();

        /// <summary>상호작용 버튼을 누르고 있는지.</summary>
        public bool InteractHeld => _interact.IsPressed();

        /// <summary>울음 입력이 발생한 순간.</summary>
        public event Action HonkPerformed;

        public InputService(InputActionAsset actions)
        {
            _playerMap = actions.FindActionMap("Player", throwIfNotFound: true);
            _move = _playerMap.FindAction("Move", throwIfNotFound: true);
            _interact = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _honk = _playerMap.FindAction("Attack", throwIfNotFound: true);
        }

        public void Initialize()
        {
            _honk.performed += OnHonkPerformed;
            _playerMap.Enable();
        }

        public void Dispose()
        {
            _honk.performed -= OnHonkPerformed;
            _playerMap.Disable();
        }

        private void OnHonkPerformed(InputAction.CallbackContext context)
        {
            HonkPerformed?.Invoke();
        }
    }
}
