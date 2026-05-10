using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using Search.Controllers;
using Search.Core;
using Search.Levels;
using UnityEngine;

namespace Search.Visualization
{
    public class MazeVisualizer : MonoBehaviour, IVisualizer
    {
        [Foldout("Prefabs")] 
        [SerializeField] private GameObject groundTilePrefab;
        [Foldout("Prefabs")] 
        [SerializeField] private GameObject wallPrefab;
        [Foldout("Prefabs")] 
        [SerializeField] private GameObject startMarkerPrefab;
        [Foldout("Prefabs")] 
        [SerializeField] private GameObject goalMarkerPrefab;
        [Foldout("Prefabs")] 
        [SerializeField] private GameObject nodePrefab;
        
        [Header("General Settings")]
        [SerializeField] private GameObject mazeCamera;

        [Foldout("Visual Settings")] 
        [SerializeField] private float cellSize = 1f;
        [Foldout("Visual Settings")] 
        [SerializeField] private float yOffsetGround;
        [Foldout("Visual Settings")] 
        [SerializeField] private float yOffsetWall = 0.5f;
        [Foldout("Visual Settings")]
        [SerializeField] private float yOffsetMarker = 0.1f;
        [Foldout("Visual Settings")] 
        [SerializeField] private float yOffsetNode = 0.2f;
        
        private MazeLevelData _levelData;
        private SearchProblem _problem;
        private readonly Dictionary<string, NodeVisual> _nodeVisuals = new();
        private GameObject _nodeContainer;
        private readonly Dictionary<Vector2Int, GameObject> _groundObjects = new();
        private GameObject _groundContainer;
        private readonly Dictionary<Vector2Int, GameObject> _wallObjects = new();
        private GameObject _wallContainer;
        private GameObject _startObject;
        private readonly List<GameObject> _goalObjects = new();
        
        private NodeVisual _sameStateVisual;

        private void Awake()
        {
            _groundContainer = CreateContainer("Grounds");
            _wallContainer = CreateContainer("Walls");
            _nodeContainer = CreateContainer("Nodes");
        }

        private GameObject CreateContainer(string namee)
        {
            var container = new GameObject(namee);
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
            return container;
        }

        public void Setup(LevelData levelData, SearchProblem problem)
        {
            _levelData = (MazeLevelData) levelData;
            _problem = problem;

            ClearVisuals();
            BuildGroundAndWalls();
            PlaceStartMarker();
            PlaceGoalMarkers();
            PositionCamera();
        }

        public void ClearVisuals()
        {
            foreach (var kvp in _nodeVisuals.Where(kvp => kvp.Value))
                Destroy(kvp.Value.gameObject);

            _nodeVisuals.Clear();

            foreach (var obj in _groundObjects.Values.Where(obj => obj))
                Destroy(obj);

            _groundObjects.Clear();

            foreach (var obj in _wallObjects.Values.Where(obj => obj))
                Destroy(obj);

            _wallObjects.Clear();

            if (_startObject)
                Destroy(_startObject);

            foreach (var obj in _goalObjects.Where(obj => obj))
                Destroy(obj);

            _sameStateVisual = null;
            _goalObjects.Clear();
        }

        public void ClearNodeVisuals()
        {
            foreach (var kvp in _nodeVisuals.Where(kvp => kvp.Value))
                Destroy(kvp.Value.gameObject);

            _nodeVisuals.Clear();
            _sameStateVisual = null;
        }

        private void BuildGroundAndWalls()
        {
            var walls = _levelData.GetWalls2D();
            var height = _levelData.rows;
            var width = _levelData.columns;

            for (var row = 0; row < height; row++)
            for (var col = 0; col < width; col++)
            {
                var pos = new Vector3(col * cellSize, yOffsetGround, row * cellSize);

                var ground = Instantiate(groundTilePrefab, _groundContainer.transform);
                ground.transform.localPosition = pos;
                ground.transform.localRotation = Quaternion.identity;
                _groundObjects[new Vector2Int(row, col)] = ground;

                if (walls[row, col])
                {
                    var wallPos = new Vector3(col * cellSize, yOffsetWall, row * cellSize);
                    var wall = Instantiate(wallPrefab, _wallContainer.transform);
                    wall.transform.localPosition = wallPos;
                    wall.transform.localRotation = Quaternion.identity;
                    _wallObjects[new Vector2Int(row, col)] = wall;
                }
            }
        }

        private void PlaceStartMarker()
        {
            var start = _levelData.start;
            var pos = new Vector3(start.y * cellSize, yOffsetMarker, start.x * cellSize);
            _startObject = Instantiate(startMarkerPrefab, transform);
            _startObject.transform.localPosition = pos;
            _startObject.transform.localRotation = Quaternion.identity;
        }

        private void PlaceGoalMarkers()
        {
            var goals = _levelData.goals;
            foreach (var goal in goals)
            {
                var pos = new Vector3(goal.y * cellSize, yOffsetMarker, goal.x * cellSize);
                var marker = Instantiate(goalMarkerPrefab, transform);
                marker.transform.localPosition = pos;
                marker.transform.localRotation = Quaternion.identity;
                _goalObjects.Add(marker);
            }
        }

        private void PositionCamera()
        {
            var width = _levelData.columns;
            var height = _levelData.rows;

            mazeCamera.transform.SetParent(transform);
            mazeCamera.transform.localPosition = new Vector3((width - 1) / 2f, 10f, (height - 1) / 2f);
            mazeCamera.transform.localRotation = Quaternion.Euler(90, 0, 0);

            var cameraComponent = mazeCamera.GetComponent<Camera>();
            cameraComponent.orthographicSize = height / 2f;

            var halfHeight = cameraComponent.orthographicSize;
            var halfWidth = width / 2f;
            var m = Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight,
                cameraComponent.nearClipPlane, cameraComponent.farClipPlane);
            cameraComponent.projectionMatrix = m;
        }

        public NodeVisual GetNodeVisual(SearchNode node) => _nodeVisuals.GetValueOrDefault(node.state.id);

        public NodeVisual CreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var state = node.state;
            var gridState = (GridState)state;
            var pos = new Vector3(gridState.Column * cellSize, yOffsetNode, gridState.Row * cellSize);
            var go = Instantiate(nodePrefab, _nodeContainer.transform);
            var searchController = SearchController.Instance;
            var visual = go.GetComponentInChildren<NodeVisual>();
            visual.Initialize(state, node);
            _nodeVisuals[state.id] = visual;

            if (searchController.isAutomaticSearch)
            {
                var startPos = pos;

                if (parent != null && _nodeVisuals.TryGetValue(parent.state.id, out var parentVisual) && parentVisual)
                    startPos = parentVisual.transform.localPosition;

                visual.transform.localPosition = startPos;

                var collide = go.GetComponentInChildren<Collider>();
                if (collide) collide.enabled = false;

                visual.isNew = true;
                visual.transform.DOLocalMove(pos, searchController.problemNodeCreationAnimationDuration)
                    .SetEase(Ease.OutCirc).OnComplete(() =>
                    {
                        if (collide) collide.enabled = true;
                        visual.isNew = false;
                    }).SetId("Search").timeScale = searchController.searchTimeScale;
            }
            else
            {   visual.transform.localPosition = pos;
                visual.isNew = false;
            }

            return visual;
        }

        public NodeVisual GetOrCreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var existing = GetNodeVisual(node);
            return existing ? existing : CreateNodeVisual(node, parent);
        }

        public void BlinkNode(IState state, Color color)
        {
            if (state is not GridState)
                return;

            if (_nodeVisuals.TryGetValue(state.id, out var nodeVisual))
                nodeVisual.BlinkNode(color);
        }

        public void TryPlaySameState(SearchNode node)
        {
            if (node.state is not GridState)
                return;

            StopSameStateAnimation(node);
            
            if (_nodeVisuals.TryGetValue(node.state.id, out var nodeVisual))
            {
                nodeVisual.PlaySameStateAnimation();
                _sameStateVisual = nodeVisual;
            }
        }

        private void StopSameStateAnimation(SearchNode node)
        {
            if (_sameStateVisual is null)
                return;
            
            _sameStateVisual.StopAnimation();
            
            if(!_sameStateVisual.SearchNode.state.Equals(node.state))
                _sameStateVisual.StopBlinking();
        }
    }
}
