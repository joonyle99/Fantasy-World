namespace FantasyWorld.World
{
    /// <summary>
    /// ObjectiveTrigger.Bind 로 전달되는 구역 런타임 참조 묶음.
    /// 이벤트 기반 트리거(HonkNearbyObjective, NpcStateObjective 등)가 사용하고,
    /// ObjectiveZone 처럼 필요 없는 트리거는 무시한다.
    /// </summary>
    public readonly struct ObjectiveContext
    {
        public readonly GameplayEventBus EventBus;
        public readonly GooseController Goose;

        public ObjectiveContext(GameplayEventBus eventBus, GooseController goose)
        {
            EventBus = eventBus;
            Goose = goose;
        }
    }
}
