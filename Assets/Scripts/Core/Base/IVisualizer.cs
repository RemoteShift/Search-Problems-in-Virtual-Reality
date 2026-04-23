using System.Collections;
using Search.Core;
using Search.Levels;
using UnityEngine;

namespace Search.Visualization
{
    public interface IVisualizer
    {
        void Setup(LevelData levelData, SearchProblem problem);
        void ClearVisuals();
        
        bool IsAnimating { get; }

        #region Node Visuals
        void ClearNodeVisuals();
        NodeVisual GetOrCreateNodeVisual(IState state);
        void BlinkNode(IState state);
        #endregion
    }
}

