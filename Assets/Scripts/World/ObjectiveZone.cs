using System;
using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 트리거 볼륨. 지정한 Grabbable 이 들어오면 완료 조건 키를 알린다.
    /// AreaFlow 가 씬의 모든 존을 찾아 ObjectiveManager 에 연결한다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class ObjectiveZone : MonoBehaviour
    {
        [SerializeField] private string _completionKey;
        [Tooltip("허용할 Grabbable Id 목록. 비어 있으면 모든 Grabbable 을 받는다.")]
        [SerializeField] private string[] _acceptedIds = Array.Empty<string>();

        /// <summary>조건이 달성됐을 때 완료 키를 전달한다.</summary>
        public event Action<string> ConditionMet;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Grabbable grabbable))
                return;

            if (_acceptedIds.Length > 0 && Array.IndexOf(_acceptedIds, grabbable.Id) < 0)
                return;

            ConditionMet?.Invoke(_completionKey);
        }
    }
}
