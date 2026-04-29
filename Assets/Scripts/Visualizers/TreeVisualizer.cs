using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Search.Core;
using Search.Levels;
using UnityEngine;

namespace Search.Visualization
{
    public class TreeVisualizer : MonoBehaviour, IVisualizer
    {
        [Foldout("Prefabs")] [SerializeField] private GameObject nodePrefab;

        [Foldout("Visual Settings")] [SerializeField]
        private float verticalSpacing = 2f;

        [Foldout("Visual Settings")] [SerializeField]
        private float horizontalSpacing = 1.5f;

        [Foldout("Visual Settings")] [SerializeField]
        private float nodeScale = 1f;

        private SearchProblem _problem;
        private GameObject _nodeContainer;

        private readonly Dictionary<SearchNode, NodeVisual> _nodeVisuals = new();
        private readonly Dictionary<SearchNode, SearchNode> _parentMap = new(); // child → parent
        private readonly Dictionary<SearchNode, List<SearchNode>> _childrenMap = new(); // parent → children

        private readonly Dictionary<SearchNode, Vector3> _manualPositions = new();

        public bool IsAnimating { get; }
        private List<NodeVisual> _currentSameStateVisuals = new();

        public Coroutine blinkingCoroutine { get; set; }

        private void Awake()
        {
            _nodeContainer = CreateContainer("Nodes");
        }

        private GameObject CreateContainer(string name)
        {
            var container = new GameObject(name);
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
            return container;
        }

        public void Setup(LevelData levelData, SearchProblem problem)
        {
            _problem = problem;
            ClearVisuals();
        }

        public void ClearVisuals() => ClearNodeVisuals();

        public void ClearNodeVisuals()
        {
            foreach (var visual in _nodeVisuals.Values.Where(visual => visual))
            {
                Destroy(visual.gameObject);
            }

            _nodeVisuals.Clear();
            _parentMap.Clear();
            _childrenMap.Clear();
            
            StopSameStateAnimation();
        }

        public NodeVisual GetNodeVisual(SearchNode node)
        {
            if (_nodeVisuals.TryGetValue(node, out var existing))
                return existing;
            return null;
        }

        public NodeVisual CreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            // Removed the manual parent/child maps – LayoutTree will rebuild from node.parent
            var go = Instantiate(nodePrefab, _nodeContainer.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * nodeScale;
            var visual = go.GetComponentInChildren<NodeVisual>();
            visual.Initialize(node.state, node, Vector3.zero);
            _nodeVisuals[node] = visual;

            if (parent != null && _manualPositions.TryGetValue(parent, out var parentManualPos))
            {
                // Determine child index for this parent
                var childIndex = _childrenMap.TryGetValue(parent, out var value) ? value.Count : 0;

                // Compute offset: spread children horizontally from parent's position
                var xOffset = (childIndex - 0.5f) * horizontalSpacing; // 0.5 centers the first child
                var childLocalPos = parentManualPos + new Vector3(xOffset, verticalSpacing, 0f);

                visual.transform.localPosition = childLocalPos;
                _manualPositions[node] = childLocalPos; // mark as manually placed
            }

            LayoutTree();
            return visual;
        }

        public NodeVisual GetOrCreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var existing = GetNodeVisual(node);

            if (existing)
                return existing;

            return CreateNodeVisual(node, parent);
        }

        /// <summary>
        /// <summary>
        /// Call this from your VR grab script when a node is moved.
        /// </summary>
        public void SetNodeManualPosition(NodeVisual visual)
        {
            if (!visual || visual.SearchNode == null) return;
            _manualPositions[visual.SearchNode] = visual.transform.localPosition;
        }

        /// <summary>
        /// Clears all manually placed positions and recomputes the full tree layout.
        /// </summary>
        public void ResetManualPositions()
        {
            _manualPositions.Clear();
            LayoutTree();
        }

        private void LayoutTree()
        {
            if (_nodeVisuals.Count == 0) return;

            // Rebuild parent/child maps from SearchNode.parent
            _parentMap.Clear();
            _childrenMap.Clear();
            foreach (var node in _nodeVisuals.Keys.Where(node => node.parent != null))
            {
                _parentMap[node] = node.parent;
                if (!_childrenMap.ContainsKey(node.parent))
                    _childrenMap[node.parent] = new List<SearchNode>();
                _childrenMap[node.parent].Add(node);
            }

            // Find roots
            var roots = _nodeVisuals.Keys.Where(n => !_parentMap.ContainsKey(n)).ToList();
            if (roots.Count == 0) return;

            // ---------- 1. Subtree width ----------
            var subtreeWidth = new Dictionary<SearchNode, float>();

            float ComputeWidth(SearchNode n)
            {
                if (subtreeWidth.TryGetValue(n, out var cached)) return cached;
                if (!_childrenMap.TryGetValue(n, out var children) || children.Count == 0)
                {
                    subtreeWidth[n] = 1f;
                    return 1f;
                }

                float total = 0f;
                foreach (var child in children)
                    total += ComputeWidth(child);
                subtreeWidth[n] = total;
                return total;
            }

            foreach (var root in roots)
                ComputeWidth(root);

            // ---------- 2. X coordinates (parent centered) ----------
            var nodeX = new Dictionary<SearchNode, float>();

            void AssignX(SearchNode n, float leftBound)
            {
                if (!_childrenMap.TryGetValue(n, out var children) || children.Count == 0)
                {
                    nodeX[n] = leftBound;
                    return;
                }

                float currentX = leftBound;
                foreach (var child in children)
                {
                    AssignX(child, currentX);
                    currentX += subtreeWidth[child] * horizontalSpacing;
                }

                float firstChildX = nodeX[children[0]];
                float lastChildX = nodeX[children[^1]];
                nodeX[n] = (firstChildX + lastChildX) / 2f;
            }

            foreach (var root in roots)
                AssignX(root, 0f);

            // ---------- 3. Depth (Y coordinate) ----------
            var depth = new Dictionary<SearchNode, int>();

            int GetDepth(SearchNode n)
            {
                if (depth.TryGetValue(n, out var d)) return d;
                if (!_parentMap.TryGetValue(n, out var parent))
                {
                    depth[n] = 0;
                    return 0;
                }

                d = GetDepth(parent) + 1;
                depth[n] = d;
                return d;
            }

            foreach (var node in _nodeVisuals.Keys)
                GetDepth(node);

            // ---------- 4. Compute raw positions (root y=0, children above) ----------
            var computedPositions = new Dictionary<SearchNode, Vector3>();
            foreach (var kvp in _nodeVisuals)
            {
                var node = kvp.Key;
                float x = nodeX.GetValueOrDefault(node, 0f);
                float y = depth.GetValueOrDefault(node, 0) * verticalSpacing;
                computedPositions[node] = new Vector3(x, y, 0f);
            }

            // ---------- 5. Find min/max X only for NON‑manual nodes ----------
            float minX = float.MaxValue, maxX = float.MinValue;
            bool hasNonManual = false;
            foreach (var kvp in _nodeVisuals)
            {
                var node = kvp.Key;
                if (_manualPositions.ContainsKey(node)) continue; // skip manual nodes
                float x = computedPositions[node].x;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                hasNonManual = true;
            }

            float offsetX = 0f;
            if (hasNonManual)
            {
                float centerX = (minX + maxX) / 2f;
                offsetX = -centerX; // shift so that the non‑manual tree is centered at x=0
            }

            // ---------- 6. Apply final positions (manual nodes are NOT moved) ----------
            foreach (var kvp in _nodeVisuals)
            {
                var node = kvp.Key;
                var visual = kvp.Value;

                if (_manualPositions.TryGetValue(node, out var manualPos))
                {
                    // Node was manually placed – keep its position, do not apply auto layout
                    visual.transform.localPosition = manualPos;
                }
                else
                {
                    Vector3 rawPos = computedPositions[node];
                    visual.transform.localPosition = new Vector3(rawPos.x + offsetX, rawPos.y, 0f);
                }
            }
        }

        public void BlinkNode(SearchNode node)
        {
            if (_nodeVisuals.TryGetValue(node, out var visual))
            {
                if (blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);
                blinkingCoroutine = StartCoroutine(visual.Blink());
            }
        }

        public void BlinkNode(IState state)
        {
            var visual = _nodeVisuals.Values.FirstOrDefault(v => v.stateId == state.id);
            if (visual)
            {
                if (blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);
                blinkingCoroutine = StartCoroutine(visual.Blink());
            }
        }

        public void TryPlaySameState(IState state)
        {
            StopSameStateAnimation();
            
            var newVisuals = _nodeVisuals.Values
                .Where(v => v.SearchNode.state.Equals(state))
                .ToList();

            foreach (var visual in newVisuals)
            {
                visual.PlaySameStateAnimation();
            }

            _currentSameStateVisuals = newVisuals;
        }

        private void StopSameStateAnimation()
        {
            foreach (var nodeVisual in _currentSameStateVisuals)
            {
                nodeVisual.StopAnimation();
            }

            _currentSameStateVisuals.Clear();
        }
    }
}