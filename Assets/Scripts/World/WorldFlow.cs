using System.Threading;
using VContainer.Unity;
using FantasyWorld.Core;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.World
{
    /// <summary>
    /// World 세션 진입점. 전역 음악을 켜고 첫 구역을 로드한다.
    /// </summary>
    public sealed class WorldFlow : IAsyncStartable
    {
        private readonly SceneLoader _sceneLoader;
        private readonly AudioService _audioService;
        private readonly GameSounds _sounds;
        private readonly LifetimeScope _scope;

        public WorldFlow(SceneLoader sceneLoader, AudioService audioService, GameSounds sounds, LifetimeScope scope)
        {
            _sceneLoader = sceneLoader;
            _audioService = audioService;
            _sounds = sounds;
            _scope = scope;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            // 전역 음악 — 구역이 바뀌어도 유지된다 (같은 이벤트면 재시작 안 함)
            _audioService.PlayMusic(_sounds.Music);

            // WorldLifetimeScope(_scope) 를 부모로 첫 구역을 Additive 로드
            await _sceneLoader.LoadAdditiveAsync("Area_Garden", _scope, setActive: true);
        }
    }
}
