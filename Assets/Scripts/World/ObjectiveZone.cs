using System;
using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 트리거 볼륨. 지정한 Grabbable 이 들어오면 완료 조건을 알린다.
    /// _acceptedIds 가 비어 있으면 모든 Grabbable 을 받는다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class ObjectiveZone : ObjectiveTrigger
    {
        [Tooltip("허용할 Grabbable Id 목록. 비어 있으면 모든 Grabbable 을 받는다.")]
        [SerializeField] private string[] _acceptedIds = Array.Empty<string>();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Grabbable grabbable))
                return;

            if (_acceptedIds.Length > 0 && Array.IndexOf(_acceptedIds, grabbable.Id) < 0)
                return;

            Complete();
        }
    }
}
