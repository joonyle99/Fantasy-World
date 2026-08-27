using VContainer;
using UnityEngine;
using FantasyWorld.World;

namespace FantasyWorld.Area
{
    /// <summary>
    /// 구역 NPC 배치·순찰·반응 디렉터. 구역 활성 동안 NpcRegistry 에 자신의 NPC를 등록한다.
    /// </summary>
    public sealed class AreaNpcController : MonoBehaviour
    {
        private NpcRegistry _npcRegistry;
        private GameplayEventBus _eventBus;

        [Inject]
        public void Construct(NpcRegistry npcRegistry, GameplayEventBus eventBus)
        {
            _npcRegistry = npcRegistry;
            _eventBus = eventBus;
        }

        // TODO: NPC 스폰/등록, 순찰 경로 할당
    }
}
