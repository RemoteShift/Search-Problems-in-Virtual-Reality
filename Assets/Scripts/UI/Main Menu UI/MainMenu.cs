using DG.Tweening;
using Search.Levels;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LeftControllerUI leftControllerUI;

    [Header("Left Canvas Scripts")] 
    [SerializeField] private TitleCanvasUI titleCanvasUI;
    [SerializeField] private LevelSelectorUI levelSelectorUI;

    private AnimatableUI _currentOpenCanvas;
    private AnimatableUI _nextCanvas;
    private bool _isTransitioning;

    private void Start()
    {
        TransitionTo(titleCanvasUI);
    }

    public void LevelSelector()
    {
        TransitionTo(levelSelectorUI);
        /*LoadingManager.Instance.LoadScene(sceneIndex: 2, onComplete:() =>
         { leftControllerUI.isUIToggleable = true; 
         LevelManager.Instance.isMainMenu = false; 
         LevelManager.Instance.Initialize(); 
         LevelManager.Instance.StartSearch(); 
         Destroy(gameObject); });*/
    }

    private void TransitionTo(AnimatableUI targetCanvas)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        _nextCanvas = targetCanvas;

        var sameCanvas = (_currentOpenCanvas == _nextCanvas);
        
        var seq = DOTween.Sequence().SetId("UI");
        
        if (_currentOpenCanvas)
        {
            seq.Append(_currentOpenCanvas.PlayClose());
        }
        
        if (!sameCanvas)
        {
            seq.AppendCallback(() =>
            {
                _nextCanvas.gameObject.SetActive(true);
            });

            seq.Append(_nextCanvas.PlayOpen());

            seq.OnComplete(() =>
            {
                _currentOpenCanvas = _nextCanvas;
                _nextCanvas = null;
                _isTransitioning = false;
            });
        }
        else
        {
            seq.AppendCallback(() =>
            {
                titleCanvasUI.gameObject.SetActive(true);
            });

            seq.Append(titleCanvasUI.PlayOpen());

            seq.OnComplete(() =>
            {
                _currentOpenCanvas = titleCanvasUI;
                _nextCanvas = null;
                _isTransitioning = false;
            });
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}