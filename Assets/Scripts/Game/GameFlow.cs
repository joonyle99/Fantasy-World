using System;
using UnityEngine;
using System.Threading;
using VContainer.Unity;
using FantasyWorld.Core;
using JoonyleGameDevKit;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.Game
{
    /// <summary>
    /// 앱 전역 상태 전환의 주체. 부팅 → 타이틀 → 로딩 → 플레이 ⇄ 일시정지 흐름을 조율하고
    /// SceneLoader / InputService 등 인프라 서비스에 명령을 내린다.
    /// </summary>
    public sealed class GameFlow : IAsyncStartable, IDisposable
    {
        private const string TITLE_SCENE = "Title";
        private const string WORLD_SCENE = "World";

        private readonly SceneLoader _sceneLoader;
        private readonly AudioService _audioService;
        private readonly InputService _inputService;
        private readonly LifetimeScope _scope;

        private readonly GameStateController<GameState> _stateController = new();

        private bool _busy;

        public GameState CurrentState => _stateController.CurrState;

        /// <summary>상태가 바뀌었을 때 (prev, next). 일시정지 메뉴 등 뷰가 구독한다.</summary>
        public event Action<GameState, GameState> StateChanged;

        public GameFlow(SceneLoader sceneLoader, AudioService audioService, InputService inputService, LifetimeScope scope)
        {
            _sceneLoader = sceneLoader;
            _audioService = audioService;
            _inputService = inputService;
            _scope = scope;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _stateController.OnStateChanged += OnStateChanged;
            _inputService.PausePerformed += TogglePause;

            await GoToTitle();
        }

        public void Dispose()
        {
            _stateController.OnStateChanged -= OnStateChanged;
            _inputService.PausePerformed -= TogglePause;
        }

        // ============== 전환 ==============

        /// <summary>타이틀 "시작" 버튼에서 호출. World 씬을 로드하고 플레이 상태로 들어간다.</summary>
        public async UniTask StartGame()
        {
            if (_busy || _stateController.CurrState != GameState.Title)
                return;

            _busy = true;
            _stateController.ChangeState(GameState.Loading);

            // GameLifetimeScope(_scope) 를 부모로 World 씬을 로드한다.
            await _sceneLoader.LoadAsync(WORLD_SCENE, _scope);

            _stateController.ChangeState(GameState.Playing);
            _busy = false;
        }

        /// <summary>일시정지 메뉴 "타이틀로" 버튼에서 호출.</summary>
        public async UniTask ReturnToTitle()
        {
            if (_busy || (_stateController.CurrState != GameState.Playing && _stateController.CurrState != GameState.Paused))
                return;

            SetPaused(false);
            _audioService.StopMusic(allowFadeOut: false);
            await GoToTitle();
        }

        private async UniTask GoToTitle()
        {
            _busy = true;
            _stateController.ChangeState(GameState.Loading);

            await _sceneLoader.LoadAsync(TITLE_SCENE, _scope);

            _stateController.ChangeState(GameState.Title);
            _busy = false;
        }

        // ============== 일시정지 ==============

        /// <summary>Esc 입력 / 메뉴 버튼에서 호출. 플레이 ⇄ 일시정지 토글.</summary>
        public void TogglePause()
        {
            if (_busy)
                return;

            switch (_stateController.CurrState)
            {
                case GameState.Playing:
                    SetPaused(true);
                    break;
                case GameState.Paused:
                    SetPaused(false);
                    break;
            }
        }

        private void SetPaused(bool paused)
        {
            if (paused && _stateController.CurrState != GameState.Playing)
                return;
            if (!paused && _stateController.CurrState != GameState.Paused)
                return;

            Time.timeScale = paused ? 0f : 1f;
            _inputService.SetGameplayInputEnabled(!paused);
            _stateController.ChangeState(paused ? GameState.Paused : GameState.Playing);
        }

        private void OnStateChanged(GameState prev, GameState next) => StateChanged?.Invoke(prev, next);
    }
}
