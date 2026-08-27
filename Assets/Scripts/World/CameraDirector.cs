using VContainer;
using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스를 뒤에서 고정 각도로 따라가는 카메라. 위치는 부드럽게 추적하고 항상 구스를 바라본다.
    /// </summary>
    public sealed class CameraDirector : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 4f, -7f);
        [SerializeField] private float _followSmoothTime = 0.2f;
        [SerializeField] private float _lookHeight = 1f;

        private GooseController _goose;
        private Vector3 _followVelocity;

        [Inject]
        public void Construct(GooseController goose)
        {
            _goose = goose;
        }

        private void LateUpdate()
        {
            if (_goose == null)
                return;

            var target = _goose.transform.position + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref _followVelocity, _followSmoothTime);
            transform.LookAt(_goose.transform.position + Vector3.up * _lookHeight);
        }
    }
}
