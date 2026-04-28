using Search.Core;
using Search.Levels;

namespace Search.Visualization
{
    public interface IVisualizer
    {
        void Setup(LevelData levelData, SearchProblem problem);
        void ClearVisuals();
        
        bool IsAnimating { get; }

        #region Node Visuals
        void ClearNodeVisuals();
        NodeVisual GetOrCreateNodeVisual(SearchNode node, SearchNode parent = null);
        void BlinkNode(IState state);
        #endregion
    }
}

