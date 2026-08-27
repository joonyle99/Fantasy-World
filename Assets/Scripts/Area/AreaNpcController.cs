using VContainer;
using UnityEngine;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 NPC 디렉터. 구역 로드 시 씬의 모든 Npc 를 찾아 의존성을 주입(Initialize)한다.
    /// AreaFlow 가 ObjectiveZone 을 연결하는 것과 같은 패턴.
    /// </summary>
    public sealed class AreaNpcController : MonoBehaviour
    {
        private GooseController _goose;
        private NpcRegistry _npcRegistry;
        private GameplayEventBus _eventBus;

        [Inject]
        public void Construct(GooseController goose, NpcRegistry npcRegistry, GameplayEventBus eventBus)
        {
            _goose = goose;
            _npcRegistry = npcRegistry;
            _eventBus = eventBus;
        }

        private void Start()
        {
            foreach (var npc in FindObjectsByType<Npc>(FindObjectsSortMode.None))
                npc.Initialize(_goose, _npcRegistry, _eventBus);
        }
    }
}
