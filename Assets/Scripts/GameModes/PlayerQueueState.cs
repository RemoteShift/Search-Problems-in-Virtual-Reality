using System.Collections.Generic;
using System.Linq;
using Search.Visualization;

namespace Search.GameModes
{
    public class PlayerQueueState
    {
        private readonly List<NodeVisual> _playerFrontier = new();
        private readonly List<NodeVisual> _expectedFrontier = new();
        private readonly List<NodeVisual> _deltaNodes = new();

        public IReadOnlyList<NodeVisual> playerFrontier => _playerFrontier;
        public IReadOnlyList<NodeVisual> expectedFrontier => _expectedFrontier;
        public IReadOnlyList<NodeVisual> deltaNodes => _deltaNodes;

        public void Clear()
        {
            _playerFrontier.Clear();
            _expectedFrontier.Clear();
            _deltaNodes.Clear();
        }

        public void ClearPlayerFrontier()
        {
            _playerFrontier.Clear();
            _deltaNodes.Clear();
        }

        public void SetExpectedFrontier(IReadOnlyList<NodeVisual> frontier)
        {
            _expectedFrontier.Clear();
            if (frontier != null)
                _expectedFrontier.AddRange(frontier.Where(v => v));
        }

        public void AddToExpectedFrontier(IReadOnlyList<NodeVisual> nodeVisuals)
        {
            if(nodeVisuals != null)
                _expectedFrontier.AddRange(nodeVisuals.Where(v => v));
        }

        public void SetDeltaNodes(IReadOnlyList<NodeVisual> DeltaNodes)
        {
            _deltaNodes.Clear();
            if (DeltaNodes != null)
                _deltaNodes.AddRange(DeltaNodes.Where(v => v));
        }

        public bool AddDeltaNodeToFrontier(NodeVisual nodeVisual, int index = -1)
        {
            if (!nodeVisual)
                return false;

            if (!_deltaNodes.Contains(nodeVisual))
                return false;

            if (!AddToPlayerFrontier(nodeVisual, index))
                return false;
            
            _deltaNodes.Remove(nodeVisual);

            return true;
        }

        public bool AddToPlayerFrontier(NodeVisual nodeVisual, int index = -1)
        {
            if (!nodeVisual)
                return false;

            if (_playerFrontier.Contains(nodeVisual))
                return false;

            if (index < 0 || index > _playerFrontier.Count)
                _playerFrontier.Add(nodeVisual);
            else
                _playerFrontier.Insert(index, nodeVisual);

            return true;
        }
        
        public bool RemoveFromPlayerFrontier(NodeVisual nodeVisual)
        {
            if (!nodeVisual)
                return false;

            if (!_playerFrontier.Remove(nodeVisual))
                return false;
            
            PlayerQueueUI.Instance.RemoveNode(nodeVisual);
            
            return _playerFrontier.Remove(nodeVisual);
        }

        public bool RemoveFromExpectedFrontier(NodeVisual nodeVisual)
        {
            if (!nodeVisual)
                return false;
            
            return _expectedFrontier.Remove(nodeVisual);
        }
        
        public bool MoveFrontierNode(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _playerFrontier.Count)
                return false;

            if (toIndex < 0 || toIndex >= _playerFrontier.Count)
                return false;

            if (fromIndex == toIndex)
                return true;

            var node = _playerFrontier[fromIndex];
            _playerFrontier.RemoveAt(fromIndex);
            _playerFrontier.Insert(toIndex, node);
            return true;
        }

        public ValidationResult ValidateExactMatch()
        {
            if (_playerFrontier.Count != _expectedFrontier.Count)
            {
                return ValidationResult.Fail(
                    $"Count mismatch. Player has {_playerFrontier.Count}, expected {_expectedFrontier.Count}.");
            }

            bool failed = false;
            string failureMessage = "";
            List<NodeVisual> misplacedNodes = new();
            
            for (var i = 0; i < _expectedFrontier.Count; i++)
            {
                var expected = _expectedFrontier[i];
                var actual = _playerFrontier[i];

                if (!ReferenceEquals(expected, actual))
                {
                    var expectedName = expected ? expected.stateId : "<null>";
                    var actualName = actual ? actual.stateId : "<null>";
                    failed = true;
                    failureMessage += $"\nMismatch at position {i}. Expected {expectedName}, got {actualName}.";
                    misplacedNodes.Add(actual);
                }
            }

            if (failed)
            {
                return ValidationResult.Fail(failureMessage, misplacedNodes);
            }
            
            return ValidationResult.Ok();
        }
    }
}