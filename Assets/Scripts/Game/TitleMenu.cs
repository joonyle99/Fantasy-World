using VContainer;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace FantasyWorld.Game
{
    /// <summary>
    /// 타이틀 화면 메뉴. 시작 / 종료 버튼을 GameFlow 에 연결한다.
    /// </summary>
    public sealed class TitleMenu : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        private GameFlow _gameFlow;

        [Inject]
        public void Construct(GameFlow gameFlow)
        {
            _gameFlow = gameFlow;
        }

        private void OnEnable()
        {
            _startButton.onClick.AddListener(OnStartClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnStartClicked()
        {
            _startButton.interactable = false;
            _gameFlow.StartGame().Forget();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
