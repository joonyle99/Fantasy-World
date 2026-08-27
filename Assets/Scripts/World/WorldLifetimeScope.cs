using VContainer;
using UnityEngine;
using VContainer.Unity;

// ============================================================
//  2계층 스코프 — 1계층에서 추가되는 개념
// ============================================================
//
//  [ 부모 스코프 ]
//    - 이 스코프는 GameLifetimeScope 를 부모로 둔다.
//    - 부모 연결은 SceneLoader 가 게임플레이 씬을 로드하기 직전
//      LifetimeScope.EnqueueParent(gameScope) 로 지정한다. (SceneLoader 참고)
//    - 덕분에 여기 없는 타입(SceneLoader 등)은 부모 컨테이너에서 주입된다.
//
//  [ builder.RegisterComponentInHierarchy<T>() ]
//    - MonoBehaviour 는 new 로 못 만든다. 대신 "로드된 씬에 이미 배치된
//      T 컴포넌트를 찾아" 컨테이너에 등록하고, 그 인스턴스에 의존성을 주입한다.
//    - 주입은 [Inject] 붙은 메서드로 받는다. (GooseController 참고)
//    - World 씬에 실제로 그 컴포넌트가 있어야 한다. 없으면 resolve 시 예외.
//
// ============================================================

namespace FantasyWorld.World
{
    /// <summary>
    /// 상주 World 씬의 컴포지션 루트. GameLifetimeScope 를 부모로 두며,
    /// 게임 세션 동안 유지되는 게임플레이 코어 시스템을 등록한다.
    /// </summary>
    public sealed class WorldLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSounds _gameSounds;

        protected override void Configure(IContainerBuilder builder)
        {
            // 씬에 배치된 뷰 컴포넌트 — 씬에서 찾아 주입
            builder.RegisterComponentInHierarchy<GooseController>();
            builder.RegisterComponentInHierarchy<CameraDirector>();
            builder.RegisterComponentInHierarchy<ObjectiveHudView>();

            // 데이터 에셋
            builder.RegisterInstance(_gameSounds);

            // 상태만 들고 있는 순수 시스템 — 요청 시 생성
            builder.Register<GameplayEventBus>(Lifetime.Singleton);
            builder.Register<NpcRegistry>(Lifetime.Singleton);
            builder.Register<ObjectiveManager>(Lifetime.Singleton);

            // 매 프레임 / 이벤트 콜백이 필요한 시스템 — 빌드 시 즉시 생성
            builder.RegisterEntryPoint<InteractionSystem>().AsSelf();
            builder.RegisterEntryPoint<GameplayAudioDirector>();

            // World 세션 흐름의 주체
            builder.RegisterEntryPoint<WorldFlow>();
        }
    }
}
