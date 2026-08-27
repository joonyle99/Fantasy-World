using System;
using UnityEngine;
using VContainer.Unity;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 진입점. 구역 목표 세트를 ObjectiveManager 에 등록하고,
    /// 씬의 ObjectiveZone 들을 목표 판정에 연결한다.
    /// </summary>
    public sealed class AreaFlow : IStartable, IDisposable
    {
        private readonly AreaObjectiveSet _objectiveSet;
        private readonly ObjectiveManager _objectiveManager;

        private ObjectiveZone[] _zones = Array.Empty<ObjectiveZone>();

        public AreaFlow(AreaObjectiveSet objectiveSet, ObjectiveManager objectiveManager)
        {
            _objectiveSet = objectiveSet;
            _objectiveManager = objectiveManager;
        }

        void IStartable.Start()
        {
            _objectiveManager.SetActiveArea(_objectiveSet);

            _zones = UnityEngine.Object.FindObjectsByType<ObjectiveZone>(FindObjectsSortMode.None);
            foreach (var zone in _zones)
                zone.ConditionMet += OnZoneConditionMet;
        }

        public void Dispose()
        {
            foreach (var zone in _zones)
            {
                if (zone != null)
                    zone.ConditionMet -= OnZoneConditionMet;
            }
        }

        private void OnZoneConditionMet(string completionKey)
        {
            _objectiveManager.NotifyCondition(completionKey);
        }
    }
}
