using System.Collections.Generic;
using System.Linq;
using Search.Controllers;
using Search.Core;
using Search.Levels;
using Search.Visualization;
using Search.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Search.GameModes
{
    public class SearchModeController : Singleton<SearchModeController>
    {
        [SerializeField] private SearchPlayMode currentMode = SearchPlayMode.Observe;

        public SearchPlayMode CurrentMode => currentMode;

        public readonly PlayerQueueState PlayerQueueState = new();

        public UnityEvent<SearchPlayMode> onModeChanged = new();
        public UnityEvent onValidationSuccess = new();
        public UnityEvent<string> onValidationFailure = new();

        private readonly List<NodeVisual> _authoritativeFrontier = new();
        private readonly List<NodeVisual> _latestDelta = new();

        public IReadOnlyList<NodeVisual> authoritativeFrontier => _authoritativeFrontier;
        public IReadOnlyList<NodeVisual> latestDelta => _latestDelta;
        
        public void SetMode(SearchPlayMode mode)
        {
            if (currentMode == mode)
                return;

            currentMode = mode;
            onModeChanged.Invoke(mode);
            
            SearchController.Instance.ClearState();
        }

        public void NotifyFrontierChanged(IReadOnlyList<NodeVisual> frontier)
        {
            _authoritativeFrontier.Clear();
            if (frontier != null)
                _authoritativeFrontier.AddRange(frontier.Where(v => v));

            PlayerQueueState.SetExpectedFrontier(_authoritativeFrontier);
        }

        public void NotifyNodesAddedToFrontier(IReadOnlyList<NodeVisual> nodeVisuals, 
            IQueuingFunction queuingFunction = null)
        {
            if(nodeVisuals != null)
                _authoritativeFrontier.AddRange(nodeVisuals.Where(v => v));
            
            PlayerQueueState.AddToExpectedFrontier(nodeVisuals, queuingFunction);
        }

        public void NotifyDeltaGenerated(IReadOnlyList<NodeVisual> deltaNodes)
        {
            _latestDelta.Clear();
            if (deltaNodes != null)
                _latestDelta.AddRange(deltaNodes.Where(v => v));

            PlayerQueueState.SetDeltaNodes(_latestDelta);
        }

        public void NotifyNodeExpanded(NodeVisual expandedNodeVisual)
        {
            if (!expandedNodeVisual)
                return;
            
            PlayerQueueState.RemoveFromExpectedFrontier(expandedNodeVisual);
            PlayerQueueState.RemoveFromPlayerFrontier(expandedNodeVisual);
        }

        public bool TryAddDeltaNodeToPlayerFrontier(NodeVisual nodeVisual, int index = -1)
        {
            return PlayerQueueState.AddDeltaNodeToFrontier(nodeVisual, index);
        }

        public void PlayerRequestsAdvance()
        {
            if (currentMode == SearchPlayMode.Observe)
            {
                SearchController.Instance.AdvanceStep();
                return;
            }
            
            var validation = PlayerQueueState.ValidateExactMatch();
            if (!validation.Success)
            {
                onValidationFailure.Invoke(validation.Message);

                if (validation.MisplacedNodeVisuals != null)
                {
                    PlayerQueueUI.Instance.ShowMisplacedElements(validation.MisplacedNodeVisuals);
                }
                
                return;
            }

            onValidationSuccess.Invoke();
            SearchController.Instance.AdvanceStep();
        }

        public void ResetSolveState()
        {
            PlayerQueueState.Clear();
            _authoritativeFrontier.Clear();
            _latestDelta.Clear();
        }
    }
}