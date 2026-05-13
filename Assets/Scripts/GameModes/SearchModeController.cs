using System.Collections.Generic;
using System.Linq;
using Search.Controllers;
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

        public UnityEvent onModeChanged = new();
        public UnityEvent onValidationSuccess = new();
        public UnityEvent<string> onValidationFailure = new();

        private readonly List<NodeVisual> _authoritativeFrontier = new();
        private readonly List<NodeVisual> _latestDelta = new();

        public IReadOnlyList<NodeVisual> AuthoritativeFrontier => _authoritativeFrontier;
        public IReadOnlyList<NodeVisual> LatestDelta => _latestDelta;

        public void SetMode(SearchPlayMode mode)
        {
            if (currentMode == mode)
                return;

            currentMode = mode;
            ResetSolveState();
            onModeChanged.Invoke();
        }

        public void NotifyFrontierChanged(IReadOnlyList<NodeVisual> frontier)
        {
            _authoritativeFrontier.Clear();
            if (frontier != null)
                _authoritativeFrontier.AddRange(frontier.Where(v => v));

            PlayerQueueState.SetExpectedFrontier(_authoritativeFrontier);
        }

        public void NotifyDeltaGenerated(IReadOnlyList<NodeVisual> deltaNodes)
        {
            _latestDelta.Clear();
            if (deltaNodes != null)
                _latestDelta.AddRange(deltaNodes.Where(v => v));

            PlayerQueueState.SetDeltaNodes(_latestDelta);
        }

        public bool TryAddDeltaNodeToPlayerFrontier(NodeVisual nodeVisual)
        {
            return PlayerQueueState.AddDeltaNodeToFrontier(nodeVisual);
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