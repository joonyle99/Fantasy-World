using VContainer;
using VContainer.Unity;

namespace FantasyWorld.Game
{
    /// <summary>
    /// 타이틀 씬의 컴포지션 루트. GameLifetimeScope 를 부모로 두며(SceneLoader 가 EnqueueParent),
    /// 씬의 TitleMenu 를 찾아 GameFlow 를 주입한다. 씬이 교체되면 스코프째 정리된다.
    /// </summary>
    public sealed class TitleLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<TitleMenu>();
        }
    }
}
