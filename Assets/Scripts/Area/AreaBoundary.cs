using VContainer;
using UnityEngine;
using FantasyWorld.World;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 경계 트리거. 구스가 들어오면 다음 구역으로 전환한다.
    /// _requiresClear 면 현재 구역의 필수 목표가 모두 끝나야 통과할 수 있다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class AreaBoundary : MonoBehaviour
    {
        [SerializeField] private string _nextAreaScene;
        [Tooltip("체크하면 현재 구역이 클리어되기 전에는 통과 불가.")]
        [SerializeField] private bool _requiresClear = true;

        private AreaTransition _areaTransition;
        private ObjectiveManager _objectiveManager;

        [Inject]
        public void Construct(AreaTransition areaTransition, ObjectiveManager objectiveManager)
        {
            _areaTransition = areaTransition;
            _objectiveManager = objectiveManager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrEmpty(_nextAreaScene))
                return;

            if (!other.TryGetComponent(out GooseController _))
                return;

            if (_requiresClear && !_objectiveManager.CurrentAreaCleared)
                return;

            _areaTransition.GoTo(_nextAreaScene).Forget();
        }
    }
}
