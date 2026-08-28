using UnityEngine;
using VContainer.Unity;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 NPC 디렉터. 구역 로드 시 씬의 모든 Npc 를 찾아 의존성을 주입(Initialize)한다.
    /// AreaFlow 가 ObjectiveTrigger 를 연결하는 것과 같은 패턴.
    /// </summary>
    public sealed class AreaNpcController : IStartable
    {
        private readonly GooseController _goose;
        private readonly NpcRegistry _npcRegistry;
        private readonly GameplayEventBus _eventBus;

        public AreaNpcController(GooseController goose, NpcRegistry npcRegistry, GameplayEventBus eventBus)
        {
            _goose = goose;
            _npcRegistry = npcRegistry;
            _eventBus = eventBus;
        }

        void IStartable.Start()
        {
            foreach (var npc in Object.FindObjectsByType<Npc>(FindObjectsSortMode.None))
                npc.Initialize(_goose, _npcRegistry, _eventBus);
        }
    }
}
