using System.Collections;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AsyncOperation = UnityEngine.AsyncOperation;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;

    private Canvas _loadingCanvas;

    private void Start()
    {
        if (!LoadingManager.Instance)
        {
            Debug.LogError("LoadingManager instance not found!");
            return;
        }

        _loadingCanvas = GetComponent<Canvas>();
        _loadingCanvas.worldCamera = PlayerLocomotion.Instance.GetComponentInChildren<Camera>();
        
        StartCoroutine(LoadTargetSceneAsync());
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        var targetString = LoadingManager.Instance.targetSceneString;
        var targetInt = LoadingManager.Instance.targetSceneIndex;

        AsyncOperation asyncLoad;

        if (!string.IsNullOrEmpty(targetString))
            asyncLoad = SceneManager.LoadSceneAsync(targetString);
        else if (targetInt >= 0 && targetInt != LoadingManager.Instance.loadingSceneIndex)
            asyncLoad = SceneManager.LoadSceneAsync(targetInt);
        else
            throw new InvalidEnumArgumentException("Invalid target scene in LoadingManager");

        if (asyncLoad == null) yield break;

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            var progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            UpdateProgressUI(progress);
            yield return null;
        }

        UpdateProgressUI(1f);
        yield return new WaitForSeconds(0.2f);

        // Activate the target scene
        asyncLoad.allowSceneActivation = true;
    }

    private void UpdateProgressUI(float progress)
    {
        if (progressSlider)
            progressSlider.value = progress;
        if (progressText)
            progressText.text = (progress * 100).ToString("F0") + "%";
    }
}