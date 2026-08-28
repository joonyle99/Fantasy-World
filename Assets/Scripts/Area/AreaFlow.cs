using System;
using UnityEngine;
using VContainer.Unity;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 진입점. 구역 목표 세트를 ObjectiveManager 에 등록하고,
    /// 씬의 모든 ObjectiveTrigger(존 진입 · 울음 · NPC 상태 등)를 목표 판정에 연결한다.
    /// </summary>
    public sealed class AreaFlow : IStartable, IDisposable
    {
        private readonly AreaObjectiveSet _objectiveSet;
        private readonly ObjectiveManager _objectiveManager;
        private readonly GameplayEventBus _eventBus;
        private readonly GooseController _goose;

        private ObjectiveTrigger[] _triggers = Array.Empty<ObjectiveTrigger>();

        public AreaFlow(AreaObjectiveSet objectiveSet, ObjectiveManager objectiveManager,
            GameplayEventBus eventBus, GooseController goose)
        {
            _objectiveSet = objectiveSet;
            _objectiveManager = objectiveManager;
            _eventBus = eventBus;
            _goose = goose;
        }

        void IStartable.Start()
        {
            _objectiveManager.SetActiveArea(_objectiveSet);

            var context = new ObjectiveContext(_eventBus, _goose);

            _triggers = UnityEngine.Object.FindObjectsByType<ObjectiveTrigger>(FindObjectsSortMode.None);
            foreach (var trigger in _triggers)
            {
                trigger.Bind(context);
                trigger.ConditionMet += OnConditionMet;
            }
        }

        public void Dispose()
        {
            foreach (var trigger in _triggers)
            {
                if (trigger == null)
                    continue;

                trigger.ConditionMet -= OnConditionMet;
                trigger.Unbind();
            }
        }

        private void OnConditionMet(string completionKey)
        {
            _objectiveManager.NotifyCondition(completionKey);
        }
    }
}
