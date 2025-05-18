using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace EasyTransition
{

    public class TransitionManager : MonoBehaviour
    {
        [SerializeField] private GameObject transitionTemplate;

        private bool runningTransition;

        public UnityAction onTransitionBegin;
        public UnityAction onTransitionCutPointReached;
        public UnityAction onTransitionEnd;

        private static TransitionManager instance;

        private void Awake()
        {
            instance = this;
        }

        public static TransitionManager Instance()
        {
            if (instance == null)
                Debug.LogError("You tried to access the instance before it exists.");

            return instance;
        }

        /// <summary>
        /// Starts a transition without loading a new level.
        /// </summary>
        /// <param name="transition">The settings of the transition you want to use.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(startDelay, transition));
        }

        /// <summary>
        /// Loads the new Scene with a transition.
        /// </summary>
        /// <param name="sceneName">The name of the scene you want to load.</param>
        /// <param name="transition">The settings of the transition you want to use to load you new scene.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(string sceneName, TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(sceneName, startDelay, transition));
        }

        /// <summary>
        /// Loads the new Scene with a transition.
        /// </summary>
        /// <param name="sceneIndex">The index of the scene you want to load.</param>
        /// <param name="transition">The settings of the transition you want to use to load you new scene.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(int sceneIndex, TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(sceneIndex, startDelay, transition));
        }

        /// <summary>
        /// Gets the index of a scene from its name.
        /// </summary>
        /// <param name="sceneName">The name of the scene you want to get the index of.</param>
        int GetSceneIndex(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).buildIndex;
        }

        IEnumerator Timer(string sceneName, float startDelay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();


            SceneManager.LoadScene(sceneName);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();
        }

        IEnumerator Timer(int sceneIndex, float startDelay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            SceneManager.LoadScene(sceneIndex);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();
        }

        IEnumerator Timer(float delay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(delay);

            onTransitionBegin?.Invoke();

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            template.GetComponent<Transition>().OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();

            runningTransition = false;
        }

        /// <summary>
        /// 非同步場景加載過渡協程。
        /// </summary>
        /// 
        public IEnumerator TransitionAndLoadSceneAsync(
        string sceneName,
        float startDelay,
        TransitionSettings transitionSettings,
        Action onSceneLoadedAndBeforeTransitionEnds = null
        )
        {
            runningTransition = true;

            // 1. 過場開始前的延遲
            yield return new WaitForSecondsRealtime(startDelay);

            // 2. 觸發過場動畫開始
            onTransitionBegin?.Invoke();
            Debug.Log("Transition Begin");

            // 3. 建立並播放過場動畫（前半段）
            GameObject template = Instantiate(transitionTemplate);
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            // 等待前半段過場動畫結束
            yield return new WaitForSecondsRealtime(transitionTime);

            // 4. 這裡算是切換點（cut point）
            onTransitionCutPointReached?.Invoke();
            Debug.Log("Transition Cut Point Reached");

            // 5. 開始非同步加載場景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad == null)
            {
                Debug.LogError("Failed to start async scene loading.");
                runningTransition = false;
                yield break;
            }

            // 等待場景加載完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            Debug.Log($"Scene '{sceneName}' Loaded Successfully.");

            // 6. 場景已載入，但過場尚未結束；先執行外部指定的回調(初始化)
            onSceneLoadedAndBeforeTransitionEnds?.Invoke();

            // 如果你的初始化 (EnterState) 還有更長的異步等待，可以考慮在這裡再多加幾個 yield

            // 7. 等待設定的過場結束時間
            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            // 8. 過場正式結束
            onTransitionEnd?.Invoke();
            Debug.Log("Transition End");

            runningTransition = false;
        }
        public IEnumerator TransitionAndLoadSceneAsync(string sceneName, float startDelay, TransitionSettings transitionSettings, UnityAction onComplete = null)
        {
            // 開始過渡前的延遲
            yield return new WaitForSecondsRealtime(startDelay);

            // 啟動過渡動畫
            onTransitionBegin?.Invoke();
            Debug.Log("Transition Begin");

            GameObject template = Instantiate(transitionTemplate);
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            // 等待過渡動畫時間
            yield return new WaitForSecondsRealtime(transitionTime);

            // 過渡中切換點事件
            onTransitionCutPointReached?.Invoke();
            Debug.Log("Transition Cut Point Reached");

            // 開始非同步加載場景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            if (asyncLoad == null)
            {
                Debug.LogError("Failed to start async scene loading.");
                runningTransition = false;
                yield break;
            }

            // 等待場景加載完成
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Debug.Log("Scene Loaded Successfully");

            // 呼叫加載完成後的回調
            onComplete?.Invoke();

            // 過渡動畫結束
            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);
            onTransitionEnd?.Invoke();
            Debug.Log("Transition End");

            runningTransition = false;
        }
        /*
        public void Transition(string sceneName, TransitionSettings transition, float startDelay, UnityAction onComplete)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assign a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(TransitionAndLoadScene(sceneName, startDelay, transition, onComplete));
        }

        private IEnumerator TransitionAndLoadScene(string sceneName, float startDelay, TransitionSettings transitionSettings, UnityAction onComplete)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();
            Debug.Log("onTransitionBegin");

            // 实例化过渡模板
            GameObject template = Instantiate(transitionTemplate);
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();
            Debug.Log("onTransitionCutPointReached");

            // 加载场景
            if (string.IsNullOrEmpty(sceneName) || SceneManager.GetSceneByName(sceneName) == null)
            {
                Debug.LogError($"Scene {sceneName} does not exist or is not added to the build settings.");
                runningTransition = false;
                yield break;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            if (asyncLoad == null)
            {
                Debug.LogError("Failed to start async loading.");
                runningTransition = false;
                yield break;
            }
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
            {
                Debug.Log($"Loading progress: {asyncLoad.progress}");
                yield return null;
            }

            // 手动激活场景
            asyncLoad.allowSceneActivation = true;


            Debug.Log("Scene loaded successfully. Invoking onComplete callback.");
            onComplete?.Invoke();

            // 等待销毁时间
            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            onTransitionEnd?.Invoke();

            runningTransition = false;
        }

        */
    }

}
