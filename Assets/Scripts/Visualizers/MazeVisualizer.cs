using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
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


        [HideInInspector] public bool isTreeSearch;

        [HideInInspector] public bool IsAnimating { get; }
        
        public Coroutine blinkingCoroutine { get; set; }

        private void Awake()
        {
            _groundContainer = new GameObject("Grounds");
            _groundContainer.transform.SetParent(transform);
            
            _wallContainer = new GameObject("Walls");
            _wallContainer.transform.SetParent(transform);

            _nodeContainer = new GameObject("Nodes");
            _nodeContainer.transform.SetParent(transform);
        }

        public void Setup(LevelData levelData, SearchProblem problem)
        {
            _levelData = (MazeLevelData) levelData;
            _problem = problem;

            ClearVisuals();

            BuildGroundAndWalls();
            PlaceStartMarker();
            PlaceGoalMarkers();
            PlayerLocomotion.Instance.TeleportTo(_startObject.transform.position, _startObject.transform.rotation);

            PositionCamera();
        }
        
        public void ClearVisuals()
        {
            foreach (var kvp in _nodeVisuals.Where(kvp => kvp.Value))
            {
                Destroy(kvp.Value.gameObject);
            }

            _nodeVisuals.Clear();

            foreach (var obj in _groundObjects.Values.Where(obj => obj))
            {
                Destroy(obj);
            }

            _groundObjects.Clear();
            
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

        public void ClearNodeVisuals()
        {
            foreach (var kvp in _nodeVisuals.Where(kvp => kvp.Value))
            {
                Destroy(kvp.Value.gameObject);
            }

            _nodeVisuals.Clear();
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
                
                var ground = Instantiate(groundTilePrefab, pos, Quaternion.identity, transform);
                ground.transform.SetParent(_groundContainer.transform);
                _groundObjects[new Vector2Int(row, col)] = ground;
                
                if (walls[row, col])
                {
                    var wallPos = new Vector3(col * cellSize, yOffsetWall, row * cellSize);
                    var wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, transform);
                    wall.transform.SetParent(_wallContainer.transform);
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

        private void PositionCamera()
        {
            var width = _levelData.width;
            var height = _levelData.height;
            
            mazeCamera.transform.position = new Vector3((width-1)/2f, 10, (height-1)/2f);
            var cameraComponent = mazeCamera.GetComponent<Camera>();
            
            cameraComponent.orthographicSize = height / 2f;
            
            var halfHeight = cameraComponent.orthographicSize;
            var halfWidth = width / 2f;
            var m = Matrix4x4.Ortho(-halfWidth, halfWidth, -halfHeight, halfHeight, 
                cameraComponent.nearClipPlane, cameraComponent.farClipPlane);
            cameraComponent.projectionMatrix = m;
        }

        public NodeVisual GetOrCreateNodeVisual(IState state)
        {
            var gridState = (GridState)state;
            if (_nodeVisuals.TryGetValue(state.id, out var existing))
            {
                return existing;
            }

            var pos = new Vector3(gridState.Column * cellSize, yOffsetNode, gridState.Row * cellSize);
            var go = Instantiate(nodePrefab, pos, Quaternion.identity, transform);
            go.transform.SetParent(_nodeContainer.transform);
            var visual = go.GetComponentInChildren<NodeVisual>();
            visual.Initialize(state, pos);
            _nodeVisuals[state.id] = visual;
            return visual;
        }
        
        public void BlinkNode(IState state)
        {
            if (state is not GridState)
                return;
            
            if(blinkingCoroutine != null)
                StopCoroutine(blinkingCoroutine);
            
            if (_nodeVisuals.TryGetValue(state.id, out var nodeVisual))
                blinkingCoroutine = StartCoroutine(nodeVisual.Blink());
        }
    }
}