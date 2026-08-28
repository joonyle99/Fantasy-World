using VContainer;
using UnityEngine;
using UnityEngine.UI;
using FantasyWorld.Game;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.World
{
    /// <summary>
    /// World 씬에 상주하는 일시정지 메뉴. GameFlow 의 상태 전환을 구독해 패널을 켜고 끄며,
    /// 재개 / 타이틀로 버튼을 GameFlow 에 연결한다. 입력·timeScale 은 GameFlow 가 관리한다.
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _titleButton;

        private GameFlow _gameFlow;

        [Inject]
        public void Construct(GameFlow gameFlow)
        {
            _gameFlow = gameFlow;
        }

        private void OnEnable()
        {
            _panel.SetActive(false);

            _gameFlow.StateChanged += OnStateChanged;
            _resumeButton.onClick.AddListener(OnResumeClicked);
            _titleButton.onClick.AddListener(OnTitleClicked);
        }

        private void OnDisable()
        {
            _gameFlow.StateChanged -= OnStateChanged;
            _resumeButton.onClick.RemoveListener(OnResumeClicked);
            _titleButton.onClick.RemoveListener(OnTitleClicked);
        }

        private void OnStateChanged(GameState prev, GameState next)
        {
            _panel.SetActive(next == GameState.Paused);
        }

        private void OnResumeClicked() => _gameFlow.TogglePause();

        private void OnTitleClicked()
        {
            _titleButton.interactable = false;
            _gameFlow.ReturnToTitle().Forget();
        }
    }
}
