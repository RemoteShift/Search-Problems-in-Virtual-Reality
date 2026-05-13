using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject toolTipsWindow;
    private static bool _firstTime = true;
    
    [Header("Left Canvas Scripts")] 
    [SerializeField] private TitleCanvasUI titleCanvasUI;
    [FormerlySerializedAs("levelSelectorUI")] 
    [SerializeField] private ProblemSelectorUI problemSelectorUI;

    private AnimatableUI _currentOpenCanvas;
    private AnimatableUI _nextCanvas;
    private bool _isTransitioning;

    private void Start()
    {
        if(_firstTime)
        {
            toolTipsWindow.SetActive(true);
            _firstTime = false;
        }
        
        TransitionTo(titleCanvasUI);
    }

    public void LevelSelector()
    {
        TransitionTo(problemSelectorUI);
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