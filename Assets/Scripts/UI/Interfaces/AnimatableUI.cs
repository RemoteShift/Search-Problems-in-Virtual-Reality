using DG.Tweening;
using UnityEngine;

public abstract class AnimatableUI : MonoBehaviour
{
    public abstract Sequence PlayOpen();
    public abstract Sequence PlayClose();
}