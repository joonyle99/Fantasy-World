using VContainer.Unity;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스의 집기/물기/놓기 상호작용 처리. 대상 탐지와 물리 조인트 부착·해제를 관리하고,
    /// 상호작용 결과를 GameplayEventBus 로 알린다.
    /// </summary>
    public sealed class InteractionSystem : ITickable
    {
        private readonly GameplayEventBus _eventBus;

        public InteractionSystem(GameplayEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        void ITickable.Tick()
        {
            // TODO: 상호작용 대상 탐지 및 집기/놓기 처리
        }
    }
}
