using System.Collections.Generic;
using System.Linq;
using Search.Core;
using Search.Levels;
using UnityEngine;
using UnityEngine.Serialization;

namespace Search.Visualization
{
    public class MazeVisualizer : MonoBehaviour, ISearchListener
    {
        [Header("Prefabs")] [SerializeField] private GameObject groundTilePrefab; // flat tile for walkable cells
        [SerializeField] private GameObject wallPrefab; // cube for walls
        [SerializeField] private GameObject startMarkerPrefab; // e.g., green cylinder
        [SerializeField] private GameObject goalMarkerPrefab; // e.g., red cylinder
        [SerializeField] private GameObject nodePrefab; // sphere for search nodes

        [Header("Visual Settings")] [SerializeField]
        private float cellSize = 1f;

        [SerializeField] private float yOffsetGround;
        [SerializeField] private float yOffsetWall = 0.5f;
        [SerializeField] private float yOffsetMarker = 0.1f;
        [SerializeField] private float yOffsetNode = 0.2f;

        // Internal data
        private MazeLevelData _levelData;
        private SearchProblem _problem;
        private readonly Dictionary<string, NodeVisual> _nodeVisuals = new();
        private readonly Dictionary<Vector2Int, GameObject> _wallObjects = new();
        private GameObject _startObject;
        private readonly List<GameObject> _goalObjects = new();

        /// <summary>
        ///     Call this after loading a MazeLevelData to build the static maze.
        /// </summary>
        public void Setup(MazeLevelData levelData, SearchProblem problem)
        {
            _levelData = levelData;
            _problem = problem;

            // Clear previous visuals
            ClearVisuals();

            // Build static maze
            BuildGroundAndWalls();
            PlaceStartMarker();
            PlaceGoalMarkers();
        }

        private void ClearVisuals()
        {
            // Destroy all dynamically created objects
            foreach (var kvp in _nodeVisuals.Where(kvp => kvp.Value))
            {
                Destroy(kvp.Value.gameObject);
            }

            _nodeVisuals.Clear();

            foreach (var obj in _wallObjects.Values.Where(obj => obj))
            {
                Destroy(obj);
            }

            _wallObjects.Clear();

            if (_startObject)
            {
                Destroy(_startObject);
            }

            foreach (var obj in _goalObjects.Where(obj => obj))
            {
                Destroy(obj);
            }

            _goalObjects.Clear();
        }

        private void BuildGroundAndWalls()
        {
            var walls = _levelData.GetWalls2D();
            var height = _levelData.height;
            var width = _levelData.width;

            for (var row = 0; row < height; row++)
            for (var col = 0; col < width; col++)
            {
                var pos = new Vector3(col * cellSize, yOffsetGround, row * cellSize);

                // Ground tile (always place)
                Instantiate(groundTilePrefab, pos, Quaternion.identity, transform);

                // Wall if needed
                if (walls[row, col])
                {
                    var wallPos = new Vector3(col * cellSize, yOffsetWall, row * cellSize);
                    var wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, transform);
                    _wallObjects[new Vector2Int(row, col)] = wall;
                }
            }
        }

        private void PlaceStartMarker()
        {
            var start = _levelData.start;
            var pos = new Vector3(start.y * cellSize, yOffsetMarker, start.x * cellSize);
            _startObject = Instantiate(startMarkerPrefab, pos, Quaternion.identity, transform);
        }

        private void PlaceGoalMarkers()
        {
            var goals = _levelData.goals;
            foreach (var goal in goals)
            {
                var pos = new Vector3(goal.y * cellSize, yOffsetMarker, goal.x * cellSize);
                var marker = Instantiate(goalMarkerPrefab, pos, Quaternion.identity, transform);
                _goalObjects.Add(marker);
            }
        }
        
        private NodeVisual GetOrCreateNodeVisual(GridState state)
        {
            if (_nodeVisuals.TryGetValue(state.id, out var existing))
            {
                return existing;
            }

            var pos = new Vector3(state.Column * cellSize, yOffsetNode, state.Row * cellSize);
            var go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            var visual = go.GetComponent<NodeVisual>(); // or get from prefab
            visual.Initialize(state, pos);
            _nodeVisuals[state.id] = visual;
            return visual;
        }

        // ---------- ISearchListener implementation ----------
        public void OnNodeExpanded(SearchNode node)
        {
            if (node.state is GridState gs)
            {
                GetOrCreateNodeVisual(gs).SetState(NodeState.Expanded);
            }
        }

        public void OnNodeGenerated(SearchNode node)
        {
            if (node.state is GridState gs)
            {
                GetOrCreateNodeVisual(gs).SetState(NodeState.Frontier);
            }
        }

        public void OnFrontierUpdated(IReadOnlyList<SearchNode> frontier)
        {
            // Reset all frontier visuals first
            foreach (var visual in _nodeVisuals.Values.Where(visual => visual.currentState == NodeState.Frontier))
            {
                visual.SetState(NodeState.Default);
            }

            // Mark current frontier nodes
            foreach (var node in frontier)
            {
                if (node.state is GridState gs)
                {
                    GetOrCreateNodeVisual(gs).SetState(NodeState.Frontier);
                }
            }
        }

        public void OnSolutionFound(SearchNode solution)
        {
            var current = solution;
            while (current != null)
            {
                if (current.state is GridState gs)
                {
                    GetOrCreateNodeVisual(gs).SetState(NodeState.Path);
                }

                current = current.parent;
            }
        }

        public void OnSearchComplete(SearchResult result)
        {
            Debug.Log($"Search completed. Success: {result.success}, Expanded: {result.nodesExpanded}");
        }
    }
}