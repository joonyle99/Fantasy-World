using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// NPC 가 지정한 상태에 도달하면 완료. "경비를 화나게 만들기" 같은 목표.
    /// _target 을 비우면 아무 NPC, 지정하면 그 NPC 만 판정한다.
    /// </summary>
    public sealed class NpcStateObjective : ObjectiveTrigger
    {
        [SerializeField] private NpcState _targetState = NpcState.Alerted;
        [Tooltip("특정 NPC 만 판정하려면 지정. 비우면 아무 NPC.")]
        [SerializeField] private Npc _target;

        private GameplayEventBus _eventBus;

        public override void Bind(ObjectiveContext context)
        {
            _eventBus = context.EventBus;
            _eventBus.Subscribe<NpcStateChanged>(OnNpcStateChanged);
        }

        public override void Unbind()
        {
            _eventBus?.Unsubscribe<NpcStateChanged>(OnNpcStateChanged);
        }

        private void OnNpcStateChanged(NpcStateChanged changed)
        {
            if (changed.State != _targetState)
                return;

            if (_target != null && changed.Npc != _target)
                return;

            Complete();
        }
    }
}
