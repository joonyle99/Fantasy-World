using System;
using System.Threading;
using VContainer.Unity;
using FantasyWorld.Core;
using JoonyleGameDevKit;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.Game
{
    /// <summary>
    /// 앱 전역 상태 전환의 주체. 부팅 → 로딩 → 플레이 흐름을 조율하고
    /// SceneLoader / SaveSystem 등 인프라 서비스에 명령을 내린다.
    /// </summary>
    public sealed class GameFlow : IAsyncStartable, IDisposable
    {
        private readonly SceneLoader _sceneLoader;
        private readonly SaveSystem _saveSystem;
        private readonly AudioService _audioService;
        private readonly LifetimeScope _scope;

        private readonly GameStateController<GameState> _stateController = new();

        public GameState CurrentState => _stateController.CurrState;

        public GameFlow(SceneLoader sceneLoader, SaveSystem saveSystem, AudioService audioService, LifetimeScope scope)
        {
            _sceneLoader = sceneLoader;
            _saveSystem = saveSystem;
            _audioService = audioService;
            _scope = scope;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _stateController.ChangeState(GameState.Loading);

            // GameLifetimeScope(_scope) 를 부모로 World 씬을 로드한다.
            // Bootstrap 씬은 교체되지만 이 스코프는 DontDestroyOnLoad 로 유지된다.
            await _sceneLoader.LoadAsync("World", _scope);

            _stateController.ChangeState(GameState.Playing);
        }

        public void Dispose()
        {
            // TODO: 구독 해제
        }
    }
}
