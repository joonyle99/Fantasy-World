using FMODUnity;
using UnityEngine;

namespace FantasyWorld.World
{
    /// <summary>
    /// 게임플레이 사운드 이벤트 카탈로그. GameplayAudioDirector 가 이걸 보고
    /// 버스 이벤트에 맞는 FMOD 이벤트를 재생한다. 인스펙터에서 EventReference 를 채운다.
    /// </summary>
    [CreateAssetMenu(menuName = "FantasyWorld/Game Sounds", fileName = "GameSounds")]
    public sealed class GameSounds : ScriptableObject
    {
        [Header("Goose")]
        public EventReference Honk;

        [Header("Interaction")]
        public EventReference Grab;
        public EventReference Drop;

        [Header("Objective")]
        public EventReference ObjectiveComplete;
        public EventReference AreaCleared;

        [Header("NPC")]
        public EventReference NpcAlerted;
        public EventReference GooseCaught;
    }
}
