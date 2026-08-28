using System;
using FMODUnity;
using VContainer.Unity;
using FantasyWorld.Core;
using FantasyWorld.Game;

namespace FantasyWorld.World
{
    /// <summary>
    /// 게임플레이 사운드 배선을 한곳에 모은다. World 세션 시작 시 전역 음악을 켜고,
    /// GameState 가 Playing 일 때만 현재 구역의 앰비언스를 흘린다(로딩/타이틀에선 끔).
    /// 그 외엔 GameplayEventBus 의 "순간"들을 듣고 해당 SFX 를 재생한다.
    /// (프레임 단위 사운드 — 발소리 등 — 은 버스를 안 타므로 각 컴포넌트가 직접 처리)
    /// </summary>
    public sealed class GameplayAudioDirector : IStartable, IDisposable
    {
        private readonly GameplayEventBus _eventBus;
        private readonly AudioService _audioService;
        private readonly GameSounds _sounds;
        private readonly GameFlow _gameFlow;

        private EventReference _areaAmbience; // 비어 있으면(default) 앰비언스 없음

        public GameplayAudioDirector(GameplayEventBus eventBus, AudioService audioService,
            GameSounds sounds, GameFlow gameFlow)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _sounds = sounds;
            _gameFlow = gameFlow;
        }

        void IStartable.Start()
        {
            // 전역 음악 — 구역이 바뀌어도 유지된다 (같은 이벤트면 재시작 안 함)
            _audioService.PlayMusic(_sounds.Music);

            _gameFlow.StateChanged += OnGameStateChanged;

            _eventBus.Subscribe<GooseHonked>(OnHonked);
            _eventBus.Subscribe<GooseGrabbed>(OnGrabbed);
            _eventBus.Subscribe<GooseDropped>(OnDropped);
            _eventBus.Subscribe<ObjectiveCompleted>(OnObjectiveCompleted);
            _eventBus.Subscribe<AreaCleared>(OnAreaCleared);
            _eventBus.Subscribe<NpcStateChanged>(OnNpcStateChanged);
            _eventBus.Subscribe<GooseCaught>(OnGooseCaught);
        }

        public void Dispose()
        {
            _gameFlow.StateChanged -= OnGameStateChanged;
            _audioService.StopAmbience(allowFadeOut: false);

            _eventBus.Unsubscribe<GooseHonked>(OnHonked);
            _eventBus.Unsubscribe<GooseGrabbed>(OnGrabbed);
            _eventBus.Unsubscribe<GooseDropped>(OnDropped);
            _eventBus.Unsubscribe<ObjectiveCompleted>(OnObjectiveCompleted);
            _eventBus.Unsubscribe<AreaCleared>(OnAreaCleared);
            _eventBus.Unsubscribe<NpcStateChanged>(OnNpcStateChanged);
            _eventBus.Unsubscribe<GooseCaught>(OnGooseCaught);
        }

        // ============== 구역 앰비언스 ==============

        /// <summary>구역 스코프(AreaAmbience)가 로드 시 자기 구역의 앰비언스를 알려준다.</summary>
        public void SetAreaAmbience(EventReference ambience)
        {
            _areaAmbience = ambience;

            // 이미 플레이 중이면 즉시 반영, 아니면 Playing 전환 때 켜진다
            if (_gameFlow.CurrentState == GameState.Playing)
                _audioService.PlayAmbience(_areaAmbience);
        }

        private void OnGameStateChanged(GameState prev, GameState next)
        {
            switch (next)
            {
                case GameState.Playing:
                    _audioService.PlayAmbience(_areaAmbience);
                    break;
                case GameState.Loading:
                case GameState.Title:
                    _audioService.StopAmbience();
                    break;
            }
        }

        // ============== 버스 이벤트 → SFX ==============

        private void OnHonked(GooseHonked e) => _audioService.PlayOneShot(_sounds.Honk, e.Position);

        private void OnGrabbed(GooseGrabbed e) => _audioService.PlayOneShot(_sounds.Grab, e.Target.transform.position);

        private void OnDropped(GooseDropped e) => _audioService.PlayOneShot(_sounds.Drop, e.Position);

        private void OnObjectiveCompleted(ObjectiveCompleted e) => _audioService.PlayOneShot(_sounds.ObjectiveComplete);

        private void OnAreaCleared(AreaCleared e) => _audioService.PlayOneShot(_sounds.AreaCleared);

        private void OnNpcStateChanged(NpcStateChanged e)
        {
            if (e.State == NpcState.Alerted)
                _audioService.PlayOneShot(_sounds.NpcAlerted, e.Npc.transform.position);
        }

        private void OnGooseCaught(GooseCaught e) => _audioService.PlayOneShot(_sounds.GooseCaught, e.By.transform.position);
    }
}
