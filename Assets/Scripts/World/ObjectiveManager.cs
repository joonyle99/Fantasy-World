using VContainer.Unity;
using FantasyWorld.Core;

namespace FantasyWorld.World
{
    /// <summary>
    /// 전체 목표 진행을 추적한다. 구역이 넘겨준 목표 세트를 받아 완료를 판정하고,
    /// 구역 클리어 시 다음 구역 언락 및 진행도 저장을 트리거한다. To-Do UI 의 데이터 소스.
    /// </summary>
    public sealed class ObjectiveManager : IStartable
    {
        private readonly GameplayEventBus _eventBus;
        private readonly SaveSystem _saveSystem;

        public ObjectiveManager(GameplayEventBus eventBus, SaveSystem saveSystem)
        {
            _eventBus = eventBus;
            _saveSystem = saveSystem;
        }

        void IStartable.Start()
        {
            // TODO: 게임플레이 이벤트 구독 → 목표 완료 판정
        }

        // TODO: RegisterAreaObjectives / IsAreaComplete
    }
}
