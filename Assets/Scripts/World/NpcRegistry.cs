using System.Collections.Generic;

namespace FantasyWorld.World
{
    /// <summary>
    /// 현재 로드된 NPC 등록/조회, 전역 경보 상태 보관. Npc 가 스스로 등록/해제하고,
    /// 다른 시스템(음악, UI 등)이 AnyAlerted 로 "지금 누가 구스를 쫓고 있나"를 조회한다.
    /// </summary>
    public sealed class NpcRegistry
    {
        private readonly List<Npc> _npcs = new();

        public IReadOnlyList<Npc> Npcs => _npcs;

        /// <summary>Alerted 상태의 NPC 가 하나라도 있는지.</summary>
        public bool AnyAlerted
        {
            get
            {
                foreach (var npc in _npcs)
                {
                    if (npc != null && npc.State == NpcState.Alerted)
                        return true;
                }

                return false;
            }
        }

        public void Register(Npc npc)
        {
            if (!_npcs.Contains(npc))
                _npcs.Add(npc);
        }

        public void Unregister(Npc npc)
        {
            _npcs.Remove(npc);
        }
    }
}
