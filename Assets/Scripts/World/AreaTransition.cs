using VContainer.Unity;
using FantasyWorld.Core;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.World
{
    /// <summary>
    /// 활성 구역(Area) 씬 하나를 유지하며 Unload → Load 로 갈아끼운다.
    /// WorldFlow 가 첫 구역을, AreaBoundary 가 다음 구역 전환을 요청한다.
    /// World 스코프 싱글턴이라 구역이 바뀌어도 살아있다.
    /// </summary>
    public sealed class AreaTransition
    {
        private readonly SceneLoader _sceneLoader;
        private readonly LifetimeScope _worldScope;

        private string _currentArea;
        private bool _transitioning;

        /// <summary>현재 로드된 구역 씬 이름. 아직 없으면 null.</summary>
        public string CurrentArea => _currentArea;

        /// <summary>전환이 진행 중인지.</summary>
        public bool IsTransitioning => _transitioning;

        public AreaTransition(SceneLoader sceneLoader, LifetimeScope worldScope)
        {
            _sceneLoader = sceneLoader;
            _worldScope = worldScope;
        }

        /// <summary>지정 구역으로 전환한다. 이미 그 구역이거나 전환 중이면 무시한다.</summary>
        public async UniTask GoTo(string areaScene)
        {
            if (_transitioning || areaScene == _currentArea)
                return;

            _transitioning = true;

            var previous = _currentArea;

            // 새 구역을 먼저 얹고 나서 이전 구역을 내린다 — 그 사이 바닥이 사라지지 않게.
            await _sceneLoader.LoadAdditiveAsync(areaScene, _worldScope, setActive: true);
            _currentArea = areaScene;

            if (!string.IsNullOrEmpty(previous))
                await _sceneLoader.UnloadAsync(previous);

            _transitioning = false;
        }
    }
}
