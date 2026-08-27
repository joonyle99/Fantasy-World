using System.Threading;
using VContainer.Unity;
using FantasyWorld.Core;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.World
{
    /// <summary>
    /// World 세션 진입점. 코어 시스템을 연결하고 첫 구역을 로드한다.
    /// </summary>
    public sealed class WorldFlow : IAsyncStartable
    {
        private readonly SceneLoader _sceneLoader;
        private readonly ObjectiveManager _objectiveManager;
        private readonly NpcRegistry _npcRegistry;
        private readonly LifetimeScope _scope;

        public WorldFlow(SceneLoader sceneLoader, ObjectiveManager objectiveManager, NpcRegistry npcRegistry, LifetimeScope scope)
        {
            _sceneLoader = sceneLoader;
            _objectiveManager = objectiveManager;
            _npcRegistry = npcRegistry;
            _scope = scope;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            // WorldLifetimeScope(_scope) 를 부모로 첫 구역을 Additive 로드한다.
            await _sceneLoader.LoadAdditiveAsync("Area_Garden", _scope, setActive: true);
        }
    }
}
