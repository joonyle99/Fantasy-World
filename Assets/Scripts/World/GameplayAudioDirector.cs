using System;
using VContainer.Unity;
using FantasyWorld.Core;

namespace FantasyWorld.World
{
    /// <summary>
    /// 게임플레이 사운드 배선을 한곳에 모은다. World 세션 시작 시 전역 음악을 켜고,
    /// 이후 GameplayEventBus 의 "순간"들을 듣고 해당 SFX 를 재생한다.
    /// (프레임 단위 사운드 — 발소리 등 — 은 버스를 안 타므로 각 컴포넌트가 직접 처리)
    /// </summary>
    public sealed class GameplayAudioDirector : IStartable, IDisposable
    {
        private readonly GameplayEventBus _eventBus;
        private readonly AudioService _audioService;
        private readonly GameSounds _sounds;

        public GameplayAudioDirector(GameplayEventBus eventBus, AudioService audioService, GameSounds sounds)
        {
            _eventBus = eventBus;
            _audioService = audioService;
            _sounds = sounds;
        }

        void IStartable.Start()
        {
            // 전역 음악 — 구역이 바뀌어도 유지된다 (같은 이벤트면 재시작 안 함)
            _audioService.PlayMusic(_sounds.Music);

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
            _eventBus.Unsubscribe<GooseHonked>(OnHonked);
            _eventBus.Unsubscribe<GooseGrabbed>(OnGrabbed);
            _eventBus.Unsubscribe<GooseDropped>(OnDropped);
            _eventBus.Unsubscribe<ObjectiveCompleted>(OnObjectiveCompleted);
            _eventBus.Unsubscribe<AreaCleared>(OnAreaCleared);
            _eventBus.Unsubscribe<NpcStateChanged>(OnNpcStateChanged);
            _eventBus.Unsubscribe<GooseCaught>(OnGooseCaught);
        }

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
