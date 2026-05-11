using DG.Tweening;
using NaughtyAttributes;
using Search.Controllers;
using Search.Levels;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectorUI : AnimatableUI
{
    private LevelManager _levelManager;
    private SearchController _searchController;
    
    #region Initial Rect Mask Padding Settings

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private RectMask2D rectMask;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float left;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float right;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float top;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float bottom;

    #endregion

    public override Sequence PlayOpen()
    {
        #region Animation

        var seq = DOTween.Sequence().SetId("UI");
        
        #region Animate Rect Mask Padding Open
        
        rectMask.padding = new Vector4(left, bottom, right, top);
        
        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    0,
                    rectMask.padding.y,
                    rectMask.padding.z,
                    rectMask.padding.w
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );

        seq.AppendInterval(0.1f);
        
        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    0,
                    0,
                    rectMask.padding.z,
                    0
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );
        
        #endregion

        // Add any OnComplete here
        
        return seq;

        #endregion
    }

    public override Sequence PlayClose()
    {
        #region Animation

        var seq = DOTween.Sequence().SetId("UI");

        #region Animate Rect Mask Padding Close

        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    rectMask.padding.x,
                    bottom,
                    rectMask.padding.z,
                    top
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );
        
        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    left,
                    bottom,
                    rectMask.padding.z,
                    top
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );

        #endregion

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        return seq;

        #endregion
    }
}
