using System;
using UnityEngine;
using System.Threading;
using VContainer.Unity;
using FantasyWorld.Core;
using JoonyleGameDevKit;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

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
        private UniTaskCompletionSource _worldReady;

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

            // 부팅 시 이미 타이틀 씬이 열려 있으면(빌드 0번 씬) 다시 로드하지 않는다.
            if (SceneManager.GetActiveScene().name == TITLE_SCENE)
            {
                _stateController.ChangeState(GameState.Title);
                return;
            }

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

            _worldReady = new UniTaskCompletionSource();

            // GameLifetimeScope(_scope) 를 부모로 World 씬을 로드한다.
            await _sceneLoader.LoadAsync(WORLD_SCENE, _scope);

            // WorldFlow 가 첫 구역 로드까지 마쳤다고 알릴 때까지 기다린다 —
            // 바닥 없는 World 만 뜬 화면이 노출되지 않도록. 로딩 오버레이는 이 동안 유지된다.
            await _worldReady.Task;

            _stateController.ChangeState(GameState.Playing);
            _busy = false;
        }

        /// <summary>
        /// WorldFlow 가 첫 구역 로드까지 마치면 호출한다. StartGame 이 이 신호를 받고 Playing 으로 전환한다.
        /// (asmdef 를 분리하면 World→Game 참조 대신 인터페이스로 뒤집는다)
        /// </summary>
        public void NotifyWorldReady()
        {
            _worldReady?.TrySetResult();
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
