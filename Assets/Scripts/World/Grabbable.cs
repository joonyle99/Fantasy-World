using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스가 부리로 집을 수 있는 오브젝트 표식. 집는 동안 물리를 멈추고 부리에 붙는다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Grabbable : MonoBehaviour
    {
        [SerializeField] private string _id;

        /// <summary>목표 판정 등에서 이 오브젝트를 식별하는 키.</summary>
        public string Id => _id;

        public bool IsHeld { get; private set; }

        private Rigidbody _body;
        private Transform _originalParent;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        public void AttachTo(Transform holder)
        {
            IsHeld = true;
            _originalParent = transform.parent;

            _body.isKinematic = true;
            _body.interpolation = RigidbodyInterpolation.None;
            transform.SetParent(holder, worldPositionStays: true);
        }

        public void Release(Vector3 releaseVelocity)
        {
            IsHeld = false;
            transform.SetParent(_originalParent, worldPositionStays: true);

            _body.isKinematic = false;
            _body.interpolation = RigidbodyInterpolation.Interpolate;
            _body.linearVelocity = releaseVelocity;
        }
    }
}
