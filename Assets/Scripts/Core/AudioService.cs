using System;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;

namespace FantasyWorld.Core
{
    /// <summary>
    /// FMOD 재생 래퍼. 게임플레이 코드는 FMOD API 를 직접 부르지 않고 이 서비스만 의존한다.
    /// 무엇을 재생할지(EventReference)는 호출자가 넘긴다 — 게임플레이 사운드는
    /// GameSounds(SO) 에, 구역 앰비언스는 각 Area 스코프에 두고 디렉터가 라우팅한다.
    /// 넘어온 참조가 비어 있으면 조용히 무시하므로, FMOD 뱅크가 아직 없어도 안전하다.
    /// </summary>
    public sealed class AudioService : IDisposable
    {
        private readonly LoopChannel _music = new();
        private readonly LoopChannel _ambience = new();

        // ============== 일회성 SFX ==============

        /// <summary>월드 위치에서 1회 재생 (3D).</summary>
        public void PlayOneShot(EventReference sound, Vector3 position)
        {
            if (sound.IsNull)
                return;

            RuntimeManager.PlayOneShot(sound, position);
        }

        /// <summary>2D 로 1회 재생 (UI 클릭음 등).</summary>
        public void PlayOneShot(EventReference sound)
        {
            if (sound.IsNull)
                return;

            RuntimeManager.PlayOneShot(sound);
        }

        /// <summary>오브젝트에 붙여 1회 재생 — 소리가 오브젝트를 따라간다.</summary>
        public void PlayOneShotAttached(EventReference sound, GameObject source)
        {
            if (sound.IsNull || source == null)
                return;

            RuntimeManager.PlayOneShotAttached(sound, source);
        }

        // ============== 음악 (전역, 하나만 유지) ==============

        public void PlayMusic(EventReference music) => _music.Play(music);

        public void StopMusic(bool allowFadeOut = true) => _music.Stop(allowFadeOut);

        /// <summary>현재 음악 이벤트의 로컬 파라미터 설정 (예: 긴장도).</summary>
        public void SetMusicParameter(string parameterName, float value) => _music.SetParameter(parameterName, value);

        // ============== 구역 앰비언스 (하나만 유지) ==============

        /// <summary>현재 구역의 앰비언스 베드를 재생한다. 다른 구역으로 바뀌면 이전 것과 교차 페이드된다.</summary>
        public void PlayAmbience(EventReference ambience) => _ambience.Play(ambience);

        public void StopAmbience(bool allowFadeOut = true) => _ambience.Stop(allowFadeOut);

        // ============== 전역 파라미터 ==============

        /// <summary>여러 이벤트가 공유하는 FMOD 전역 파라미터 (예: "Alert" 로 전체 긴장도 조절).</summary>
        public void SetGlobalParameter(string parameterName, float value)
        {
            RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
        }

        public void Dispose()
        {
            _music.Stop(allowFadeOut: false);
            _ambience.Stop(allowFadeOut: false);
        }

        /// <summary>
        /// 한 번에 하나만 흐르는 루프 베드(음악·앰비언스). 같은 이벤트 재요청은 무시하고,
        /// 다른 이벤트로 바뀌면 이전 것을 페이드아웃시키며 새 것으로 교체한다.
        /// </summary>
        private sealed class LoopChannel
        {
            private EventInstance _instance;
            private EventReference _current;
            private bool _playing;

            public void Play(EventReference sound)
            {
                if (sound.IsNull)
                {
                    Stop();
                    return;
                }

                // 같은 이벤트가 아직 실제로 흐르고 있으면 재시작하지 않는다
                // (구역 로드 때마다 처음으로 튀는 것 방지 — 단, 자연 종료된 경우엔 다시 건다)
                if (_current.Guid.Equals(sound.Guid) && IsActuallyPlaying())
                    return;

                Stop(allowFadeOut: true);

                _instance = RuntimeManager.CreateInstance(sound);
                _instance.start();
                _current = sound;
                _playing = true;
            }

            public void Stop(bool allowFadeOut = true)
            {
                if (!_playing)
                    return;

                _instance.stop(allowFadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                _instance.release();
                _playing = false;
            }

            public void SetParameter(string parameterName, float value)
            {
                if (_playing)
                    _instance.setParameterByName(parameterName, value);
            }

            /// <summary>루프가 아닌 이벤트가 자연 종료됐을 수 있으므로 실제 재생 상태를 확인한다.</summary>
            private bool IsActuallyPlaying()
            {
                if (!_playing)
                    return false;

                if (_instance.getPlaybackState(out var state) != FMOD.RESULT.OK)
                    return false;

                return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
            }
        }
    }
}
