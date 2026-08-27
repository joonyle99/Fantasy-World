using TMPro;
using VContainer;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace FantasyWorld.World
{
    /// <summary>
    /// 화면 구석의 목표 체크리스트 HUD. GameplayEventBus 를 구독해 구역 로드 시 목록을 만들고,
    /// 목표 완료 / 구역 클리어를 반영해 다시 그린다. World 씬에 상주한다.
    /// </summary>
    public sealed class ObjectiveHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _listText;

        private GameplayEventBus _eventBus;

        private readonly List<ObjectiveDefinition> _objectives = new();
        private readonly HashSet<string> _completed = new();

        private string _areaName;
        private bool _areaCleared;

        [Inject]
        public void Construct(GameplayEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void OnEnable()
        {
            _eventBus.Subscribe<AreaObjectivesLoaded>(OnAreaLoaded);
            _eventBus.Subscribe<ObjectiveCompleted>(OnObjectiveCompleted);
            _eventBus.Subscribe<AreaCleared>(OnAreaCleared);
        }

        private void OnDisable()
        {
            _eventBus.Unsubscribe<AreaObjectivesLoaded>(OnAreaLoaded);
            _eventBus.Unsubscribe<ObjectiveCompleted>(OnObjectiveCompleted);
            _eventBus.Unsubscribe<AreaCleared>(OnAreaCleared);
        }

        private void OnAreaLoaded(AreaObjectivesLoaded loaded)
        {
            _objectives.Clear();
            _completed.Clear();
            _areaCleared = false;
            _areaName = loaded.Area.AreaName;

            foreach (var objective in loaded.Area.Objectives)
                _objectives.Add(objective);

            Redraw();
        }

        private void OnObjectiveCompleted(ObjectiveCompleted completed)
        {
            _completed.Add(completed.CompletionKey);
            Redraw();
        }

        private void OnAreaCleared(AreaCleared cleared)
        {
            _areaCleared = true;
            Redraw();
        }

        private void Redraw()
        {
            if (_titleText != null)
                _titleText.text = _areaCleared ? _areaName + "  (cleared)" : _areaName;

            var builder = new StringBuilder();
            foreach (var objective in _objectives)
            {
                builder.Append(_completed.Contains(objective.CompletionKey) ? "[x] " : "[ ] ");
                builder.Append(objective.Description);
                if (objective.Optional)
                    builder.Append("  (optional)");
                builder.Append('\n');
            }

            if (_listText != null)
                _listText.text = builder.ToString();
        }
    }
}
