using System;
using Search.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    public int loadingSceneIndex = 1;

    public string targetSceneString = null;
    public int targetSceneIndex = -1;
    private Action _onTargetSceneLoaded;
    private bool _isLoading;

    public void LoadScene(string sceneName = null, int sceneIndex = -1, Action onComplete = null)
    {
        if (_isLoading)
        {
            Debug.LogWarning("Already loading a scene. Ignoring new request.");
            return;
        }

        targetSceneString = sceneName ?? null;
        targetSceneIndex = sceneIndex >= 0 ? sceneIndex : -1;

        if (targetSceneIndex == -1 && string.IsNullOrEmpty(targetSceneString))
            throw new ArgumentException("Either sceneName or sceneIndex must be provided.");

        _onTargetSceneLoaded = onComplete;
        _isLoading = true;

        // Listen for any scene load
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Load the loading scene (this triggers OnSceneLoaded once for loading scene)
        SceneManager.LoadScene(loadingSceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If this is the loading scene itself, ignore it and wait for the target
        if (scene.buildIndex == loadingSceneIndex)
            return;

        // Check if this is the target scene (by name or index)
        var isTarget = (!string.IsNullOrEmpty(targetSceneString) && scene.name.Equals(targetSceneString)) ||
                       (targetSceneIndex >= 0 && scene.buildIndex == targetSceneIndex);

        if (!isTarget) return;

        // Target scene is now fully loaded
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _isLoading = false;

        // Invoke the callback and clear it
        _onTargetSceneLoaded?.Invoke();
        _onTargetSceneLoaded = null;
    }
}