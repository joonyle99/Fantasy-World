using VContainer;
using UnityEngine;

namespace FantasyWorld.Game
{
    /// <summary>
    /// 씬 전환 중 화면을 덮는 페이드 오버레이. Bootstrapper 프리팹에 상주하며 GameFlow 의 상태를 구독한다.
    /// Loading 진입 시 즉시 덮고, 준비가 끝나도 최소 노출 시간을 채운 뒤에야 부드럽게 걷힌다
    /// (빠른 로드가 한 프레임 깜빡이로 보이지 않도록). 실제 로드가 더 오래 걸리면 그 시간을 그대로 보여준다.
    /// 애니메이션이 끝나면 Update 를 스스로 꺼서 유휴 시엔 매 프레임 돌지 않는다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private float _fadeSpeed = 4f;

        [Tooltip("한 번 뜨면 최소 이 시간(초)은 유지한다. 로드가 더 빠르면 이만큼 붙잡는다.")]
        [SerializeField] private float _minVisibleSeconds = 1.2f;

        [Header("Spinner")]
        [SerializeField] private RectTransform _spinner;
        [SerializeField] private float _spinnerSpeed = 220f;

        private GameFlow _gameFlow;
        private CanvasGroup _group;
        private bool _covering = true;
        private float _shownAt;

        [Inject]
        public void Construct(GameFlow gameFlow)
        {
            _gameFlow = gameFlow;
        }

        // enabled 를 Update 게이트로 쓰므로 구독은 Awake/OnDestroy 에서 (OnEnable 이면 같이 끊긴다)
        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _shownAt = Time.unscaledTime;
            _gameFlow.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            _gameFlow.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState prev, GameState next)
        {
            var wantCover = next == GameState.Loading;

            // 새로 덮기 시작하는 순간을 기록하고 즉시 불투명하게
            if (wantCover && !_covering)
            {
                _shownAt = Time.unscaledTime;
                _group.alpha = 1f;
                _group.blocksRaycasts = true;
            }

            _covering = wantCover;
            enabled = true; // 페이드/홀드 처리할 게 생겼으니 Update 재개
        }

        private void Update()
        {
            // 덮어야 하거나, 아직 최소 노출 시간을 못 채웠으면 계속 덮는다
            var hold = _covering || Time.unscaledTime - _shownAt < _minVisibleSeconds;
            var target = hold ? 1f : 0f;

            _group.alpha = Mathf.MoveTowards(_group.alpha, target, _fadeSpeed * Time.unscaledDeltaTime);
            _group.blocksRaycasts = _group.alpha > 0.01f;

            if (_spinner != null && _group.alpha > 0.01f)
                _spinner.Rotate(0f, 0f, -_spinnerSpeed * Time.unscaledDeltaTime);

            // 완전히 걷혔고 홀드도 끝났으면 더 할 일이 없다 → Update 정지
            if (!hold && _group.alpha <= 0f)
                enabled = false;
        }
    }
}
