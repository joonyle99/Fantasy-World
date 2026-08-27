using VContainer.Unity;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 진입점. 구역 목표 세트를 ObjectiveManager 에 등록하고 NPC 를 NpcRegistry 에 연결한다.
    /// </summary>
    public sealed class AreaFlow : IStartable
    {
        private readonly AreaObjectiveSet _objectiveSet;
        private readonly ObjectiveManager _objectiveManager;
        private readonly NpcRegistry _npcRegistry;

        public AreaFlow(AreaObjectiveSet objectiveSet, ObjectiveManager objectiveManager, NpcRegistry npcRegistry)
        {
            _objectiveSet = objectiveSet;
            _objectiveManager = objectiveManager;
            _npcRegistry = npcRegistry;
        }

        void IStartable.Start()
        {
            // TODO: 목표 세트 등록, NPC 등록
        }
    }
}
