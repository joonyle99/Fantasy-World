using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

// ============================================================
//  LifetimeScope.EnqueueParent 패턴
// ============================================================
//
//  씬을 로드하면 그 씬의 LifetimeScope 가 Awake 에서 자기 컨테이너를 빌드한다.
//  이때 부모를 어디서 찾을지 미리 알려줘야 한다.
//
//  EnqueueParent(parent) 는 "지금부터 새로 Awake 하는 LifetimeScope 는
//  parent 를 부모로 삼아라"를 스택에 쌓고, IDisposable 을 돌려준다.
//  Dispose 시점(=using 블록 종료)에 스택에서 빠진다.
//
//      using (LifetimeScope.EnqueueParent(parent))
//      {
//          await SceneManager.LoadSceneAsync(name, ...);   // 이 안에서 새 스코프 Awake
//      }                                                    // Awake 끝난 뒤 해제
//
//  parent 는 새 씬의 스코프가 살아있는 동안 함께 살아있어야 한다.
//    - Game -> World : parent = GameLifetimeScope (DontDestroyOnLoad)
//    - World -> Area : parent = WorldLifetimeScope (World 씬이 상주)
//  호출자는 자기 LifetimeScope 를 주입받아(생성자 파라미터에 LifetimeScope) 넘긴다.
//
// ============================================================

namespace FantasyWorld.Core
{
    /// <summary>
    /// 씬 로드/언로드 담당. 게임플레이 씬을 로드할 때 상위 LifetimeScope 를 부모로 물려,
    /// 새 씬의 스코프가 그 부모의 등록물을 주입받게 한다.
    /// </summary>
    public sealed class SceneLoader
    {
        /// <summary>
        /// 현재 씬을 새 씬으로 교체한다. parent 는 씬 전환에도 살아있어야 한다(예: GameLifetimeScope).
        /// </summary>
        public async UniTask LoadAsync(string sceneName, LifetimeScope parent)
        {
            using (LifetimeScope.EnqueueParent(parent))
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                if (operation == null)
                {
                    Debug.LogError($"[SceneLoader] 씬을 로드할 수 없습니다: {sceneName} (Build Settings 확인)");
                    return;
                }

                await operation;
            }
        }

        /// <summary>
        /// 씬을 Additive 로 얹는다. 구역(Area) 스트리밍에 쓴다.
        /// </summary>
        public async UniTask LoadAdditiveAsync(string sceneName, LifetimeScope parent, bool setActive = false)
        {
            using (LifetimeScope.EnqueueParent(parent))
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (operation == null)
                {
                    Debug.LogError($"[SceneLoader] 씬을 로드할 수 없습니다: {sceneName} (Build Settings 확인)");
                    return;
                }

                await operation;
            }

            if (setActive)
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid())
                {
                    SceneManager.SetActiveScene(scene);
                }
            }
        }

        /// <summary>
        /// Additive 로 얹은 씬을 내린다. 해당 씬의 LifetimeScope 도 함께 파괴되어 구독/리소스가 정리된다.
        /// </summary>
        public async UniTask UnloadAsync(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            await SceneManager.UnloadSceneAsync(scene);
        }
    }
}
