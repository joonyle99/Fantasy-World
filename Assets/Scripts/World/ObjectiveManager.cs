using System.Collections.Generic;
using FantasyWorld.Core;

namespace FantasyWorld.World
{
    /// <summary>
    /// 현재 구역의 목표 진행을 추적한다. 조건 키가 달성되면 해당 목표를 완료 처리하고,
    /// 필수 목표가 모두 끝나면 구역 클리어를 알린다. To-Do UI 의 데이터 소스이기도 하다.
    /// </summary>
    public sealed class ObjectiveManager
    {
        private readonly GameplayEventBus _eventBus;
        private readonly SaveSystem _saveSystem;

        private readonly HashSet<string> _completedKeys = new();

        private AreaObjectiveSet _activeArea;
        private bool _areaCleared;

        public AreaObjectiveSet ActiveArea => _activeArea;

        public ObjectiveManager(GameplayEventBus eventBus, SaveSystem saveSystem)
        {
            _eventBus = eventBus;
            _saveSystem = saveSystem;
        }

        /// <summary>구역이 로드될 때 AreaFlow 가 호출한다.</summary>
        public void SetActiveArea(AreaObjectiveSet area)
        {
            _activeArea = area;
            _areaCleared = false;
            _completedKeys.Clear();
        }

        public bool IsCompleted(string completionKey) => _completedKeys.Contains(completionKey);

        /// <summary>조건 키가 달성됐음을 알린다 (ObjectiveZone 등이 트리거).</summary>
        public void NotifyCondition(string completionKey)
        {
            if (_activeArea == null || _areaCleared)
                return;

            if (!_completedKeys.Add(completionKey))
                return;

            foreach (var objective in _activeArea.Objectives)
            {
                if (objective.CompletionKey == completionKey)
                    _eventBus.Publish(new ObjectiveCompleted(objective.Description));
            }

            if (AllRequiredCompleted())
            {
                _areaCleared = true;
                _eventBus.Publish(new AreaCleared(_activeArea.AreaName));

                // TODO: _saveSystem 에 구역 클리어 기록 + 다음 구역 언락
            }
        }

        private bool AllRequiredCompleted()
        {
            foreach (var objective in _activeArea.Objectives)
            {
                if (!objective.Optional && !_completedKeys.Contains(objective.CompletionKey))
                    return false;
            }

            return true;
        }
    }
}
