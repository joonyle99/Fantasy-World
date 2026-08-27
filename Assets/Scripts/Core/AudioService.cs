using System;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;

namespace FantasyWorld.Core
{
    /// <summary>
    /// FMOD 재생 래퍼. 게임플레이 코드는 FMOD API 를 직접 부르지 않고 이 서비스만 의존한다.
    /// 무엇을 재생할지(EventReference)는 호출자가 넘긴다 — 게임플레이 사운드는
    /// GameSounds(SO) 에 모아두고 GameplayAudioDirector 가 라우팅한다.
    /// 넘어온 참조가 비어 있으면 조용히 무시하므로, FMOD 뱅크가 아직 없어도 안전하다.
    /// </summary>
    public sealed class AudioService : IDisposable
    {
        private EventInstance _music;
        private bool _musicPlaying;

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

        // ============== 음악 (하나만 유지) ==============

        public void PlayMusic(EventReference music)
        {
            if (music.IsNull)
                return;

            StopMusic(allowFadeOut: false);

            _music = RuntimeManager.CreateInstance(music);
            _music.start();
            _musicPlaying = true;
        }

        public void StopMusic(bool allowFadeOut = true)
        {
            if (!_musicPlaying)
                return;

            _music.stop(allowFadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            _music.release();
            _musicPlaying = false;
        }

        /// <summary>현재 음악 이벤트의 로컬 파라미터 설정 (예: 긴장도).</summary>
        public void SetMusicParameter(string parameterName, float value)
        {
            if (_musicPlaying)
                _music.setParameterByName(parameterName, value);
        }

        // ============== 전역 파라미터 ==============

        /// <summary>여러 이벤트가 공유하는 FMOD 전역 파라미터 (예: "Alert" 로 전체 긴장도 조절).</summary>
        public void SetGlobalParameter(string parameterName, float value)
        {
            RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
        }

        public void Dispose()
        {
            StopMusic(allowFadeOut: false);
        }
    }
}
