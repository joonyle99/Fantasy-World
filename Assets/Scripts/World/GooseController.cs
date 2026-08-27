using VContainer;
using UnityEngine;
using FantasyWorld.Core;

namespace FantasyWorld.World
{
    /// <summary>
    /// 구스의 물리 이동, 목 뻗기, 울음 등 물리·연출 뷰. 로직 판정은 시스템 계층에 위임한다.
    /// </summary>
    public sealed class GooseController : MonoBehaviour
    {
        private InputService _inputService;
        private AudioService _audioService;

        [Inject]
        public void Construct(InputService inputService, AudioService audioService)
        {
            _inputService = inputService;
            _audioService = audioService;
        }

        // TODO: FixedUpdate 이동, Honk, 목 뻗기
    }
}
