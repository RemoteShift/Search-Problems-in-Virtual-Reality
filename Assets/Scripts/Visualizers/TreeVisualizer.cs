using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using DG.Tweening;
using Search.Controllers;
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
        
        [Foldout("Visual Settings")] [SerializeField]
        private float subTreeDeletionDelay = 1f;
        
        private GameObject _nodeContainer;

        private readonly Dictionary<SearchNode, NodeVisual> _nodeVisuals = new();
        private readonly Dictionary<SearchNode, SearchNode> _parentMap = new();
        private readonly Dictionary<SearchNode, List<SearchNode>> _childrenMap = new();

        private readonly Dictionary<SearchNode, float> _subtreeWidth = new();
        private readonly Dictionary<SearchNode, float> _nodeX = new();
        private readonly Dictionary<SearchNode, int> _depth = new();
        
        private List<NodeVisual> _currentSameStateVisuals = new();

        private void Awake()
        {
            _nodeContainer = CreateContainer("Nodes");
        }

        private GameObject CreateContainer(string containerName)
        {
            var container = new GameObject(containerName);
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
            return container;
        }

        public void Setup(LevelData levelData, SearchProblem problem)
        {
            ClearVisuals();
        }

        public void ClearVisuals()
        {
            StopAllCoroutines();
            ClearNodeVisuals();
        }

        public void ClearNodeVisuals()
        {
            foreach (var visual in _nodeVisuals.Values.Where(visual => visual)) Destroy(visual.gameObject);
            _nodeVisuals.Clear();
            _currentSameStateVisuals.Clear();
            _parentMap.Clear();
            _childrenMap.Clear();
            _subtreeWidth.Clear();
            _nodeX.Clear();
            _depth.Clear();
            StopSameStateAnimation();
        }

        public NodeVisual GetNodeVisual(SearchNode node) => _nodeVisuals.GetValueOrDefault(node);

        public NodeVisual CreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var go = Instantiate(nodePrefab, _nodeContainer.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * nodeScale;

            var visual = go.GetComponentInChildren<NodeVisual>();
            visual.Initialize(node.state, node);
            _nodeVisuals[node] = visual;

            if (parent != null)
            {
                _parentMap[node] = parent;
                if (!_childrenMap.ContainsKey(parent))
                    _childrenMap[parent] = new List<SearchNode>();
                _childrenMap[parent].Add(node);
            }

            LayoutTree();
            
            if (parent != null
                && _nodeVisuals.TryGetValue(parent, out var parentVisual)
                && parentVisual.manuallyPositioned)
            {
                visual.manuallyPositioned = true;
            }

            return visual;
        }

        public NodeVisual GetOrCreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var existing = GetNodeVisual(node);
            return existing ? existing : CreateNodeVisual(node, parent);
        }

        public void SetNodeManualPosition(NodeVisual visual)
        {
            if (!visual || visual.SearchNode == null)
                return;

            visual.manuallyPositioned = true;
            visual.transform.localPosition = visual.transform.localPosition;
        }

        public void ResetParent(SearchNode child)
        {
            if (child == null) return;
            if (!_nodeVisuals.TryGetValue(child, out var oldVisual) || !oldVisual) return;

            var newParent = child.parent;

            // 1. Capture the entire subtree (including the old root) while maps are still intact
            var subtree = GetSubTree(oldVisual);

            // 2. Remove the old child from its previous parent and from dictionaries
            if (_parentMap.TryGetValue(child, out var previousParent) &&
                _childrenMap.TryGetValue(previousParent, out var siblings))
            {
                siblings.Remove(child);
            }
            _parentMap.Remove(child);
            _childrenMap.Remove(child);
            _nodeVisuals.Remove(child);   // old visual is no longer the "active" one

            // 3. Attach to the new parent (if it exists)
            if (newParent != null)
            {
                _parentMap[child] = newParent;
                if (!_childrenMap.ContainsKey(newParent))
                    _childrenMap[newParent] = new List<SearchNode>();
                if (!_childrenMap[newParent].Contains(child))
                    _childrenMap[newParent].Add(child);
            }

            // 4. Create the replacement visual and put it into the dictionary
            var go = Instantiate(nodePrefab, _nodeContainer.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * nodeScale;

            var newVisual = go.GetComponentInChildren<NodeVisual>();
            newVisual.Initialize(child.state, child);
            newVisual.manuallyPositioned = false;
            _nodeVisuals[child] = newVisual;

            // 5. Start the old subtree deletion (with delay, turning red, then destroying)
            StartCoroutine(DeleteSubTree(subtree, subTreeDeletionDelay));

            // 6. Recalculate layout (the new visual will be placed, old ones are still visible until deleted)
            LayoutTree();
        }

        private IEnumerator DeleteSubTree(List<NodeVisual> subTree, float delay)
        {
            // Colour them red
            foreach (var visual in subTree)
            {
                if (visual) visual.SetColor(Color.red);
            }

            // Wait safely, respecting DOTween.timeScale and avoiding division by zero
            float timer = 0f;
            while (timer < delay)
            {
                if (DOTween.timeScale > 0f)
                {
                    timer += Time.unscaledDeltaTime * DOTween.timeScale;
                }
                yield return null;
            }

            bool anyMapChanges = false;
            foreach (var visual in subTree)
            {
                if (!visual) continue;
                var node = visual.SearchNode;
                if (node == null) continue;

                // Only remove from maps if this visual is still the official one
                if (_nodeVisuals.TryGetValue(node, out var activeVisual) && activeVisual == visual)
                {
                    if (_parentMap.TryGetValue(node, out var parent) && _childrenMap.TryGetValue(parent, out var siblings))
                        siblings.Remove(node);
                    _parentMap.Remove(node);
                    _childrenMap.Remove(node);
                    _nodeVisuals.Remove(node);
                    anyMapChanges = true;
                }

                // Kill any running tweens and destroy the GameObject
                visual.transform.DOKill();
                Destroy(visual.gameObject);
            }

            if (anyMapChanges)
                LayoutTree();
        }

        private List<NodeVisual> GetSubTree(NodeVisual rootVisual)
        {
            var result = new List<NodeVisual>();
            var stack = new Stack<NodeVisual>();
            stack.Push(rootVisual);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                result.Add(current);

                var currentNode = current.SearchNode;
                if (_childrenMap.TryGetValue(currentNode, out var children))
                {
                    foreach (var child in children)
                    {
                        if (_nodeVisuals.TryGetValue(child, out var childVisual) && childVisual)
                            stack.Push(childVisual);
                    }
                }
            }

            return result;
        }

        public List<NodeVisual> GetSubTree(SearchNode root) => GetSubTree(_nodeVisuals.GetValueOrDefault(root));

        public void ResetManualPositions()
        {
            foreach (var v in _nodeVisuals.Values)
            {
                if (v) v.manuallyPositioned = false;
            }

            LayoutTree();
        }

        private void LayoutTree()
        {
            if (_nodeVisuals.Count == 0)
                return;

            _subtreeWidth.Clear();
            _nodeX.Clear();
            _depth.Clear();

            var roots = new List<SearchNode>();
            foreach (var n in _nodeVisuals.Keys)
                if (!_parentMap.ContainsKey(n))
                    roots.Add(n);

            if (roots.Count == 0)
                return;

            var postOrder = new List<SearchNode>();
            var stack = new Stack<SearchNode>();
            foreach (var r in roots) stack.Push(r);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                postOrder.Add(n);
                if (_childrenMap.TryGetValue(n, out var children))
                    foreach (var c in children)
                        stack.Push(c);
            }

            for (int i = postOrder.Count - 1; i >= 0; i--)
            {
                var n = postOrder[i];
                if (!_childrenMap.TryGetValue(n, out var children) || children.Count == 0)
                    _subtreeWidth[n] = 1f;
                else
                {
                    float total = 0f;
                    foreach (var c in children) total += _subtreeWidth[c];
                    _subtreeWidth[n] = total;
                }
            }

            foreach (var root in roots)
            {
                var stackX = new Stack<(SearchNode node, float left, int state)>();
                stackX.Push((root, 0f, 0));

                while (stackX.Count > 0)
                {
                    var (n, left, state) = stackX.Pop();

                    if (state == 0)
                    {
                        if (!_childrenMap.TryGetValue(n, out var children) || children.Count == 0)
                        {
                            _nodeX[n] = left;
                            continue;
                        }

                        stackX.Push((n, left, 1));

                        float currentX = left;
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                            var child = children[i];
                            stackX.Push((child, currentX, 0));
                            currentX += _subtreeWidth[child] * horizontalSpacing;
                        }
                    }
                    else
                    {
                        var children = _childrenMap[n];
                        float firstChildX = _nodeX[children[0]];
                        float lastChildX = _nodeX[children[^1]];
                        _nodeX[n] = (firstChildX + lastChildX) / 2f;
                    }
                }
            }

            foreach (var n in postOrder)
            {
                if (!_parentMap.TryGetValue(n, out var parent))
                    _depth[n] = 0;
                else
                    _depth[n] = _depth[parent] + 1;
            }

            float minX = float.MaxValue, maxX = float.MinValue;
            bool hasNonManual = false;

            foreach (var kvp in _nodeVisuals)
            {
                var node = kvp.Key;
                if (_nodeVisuals.TryGetValue(node, out var vis) && vis && vis.manuallyPositioned)
                    continue;

                float x = _nodeX[node];
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                hasNonManual = true;
            }

            float offsetX = hasNonManual ? -(minX + maxX) / 2f : 0f;

            foreach (var kvp in _nodeVisuals)
            {
                var node = kvp.Key;
                var visual = kvp.Value;

                if (visual && visual.manuallyPositioned)
                    continue;

                var target = new Vector3(_nodeX[node] + offsetX, _depth[node] * verticalSpacing, 0f);
                
                if (_parentMap.TryGetValue(node, out var parent)
                    && _nodeVisuals.TryGetValue(parent, out var parentVisual)
                    && parentVisual.manuallyPositioned)
                {
                    if (_nodeX.TryGetValue(parent, out float parentX) && _depth.TryGetValue(parent, out int parentDepth))
                    {
                        var parentDefaultPos = new Vector3(parentX + offsetX, parentDepth * verticalSpacing, 0f);
                        var parentActualPos = parentVisual.transform.localPosition;
                        target += parentActualPos - parentDefaultPos;
                    }
                }

                var searchController = SearchController.Instance;

                if (searchController.isAutomaticSearch)
                {
                    _parentMap.TryGetValue(node, out var parentForAnim);
                    if (visual.isNew && parentForAnim != null && _nodeVisuals.TryGetValue(parentForAnim, out var parentVisForAnim) && parentVisForAnim)
                    {
                        visual.transform.localPosition = parentVisForAnim.transform.localPosition;
                    }

                    var collide = visual.GetComponent<Collider>();
                    if (collide) collide.enabled = false;

                    if (visual.isNew)
                    {
                        visual.transform.DOLocalMove(target, searchController.treeNodeCreationAnimationDuration)
                            .SetEase(Ease.OutCirc).OnComplete(() =>
                            {
                                if (collide) collide.enabled = true;
                                visual.isNew = false;
                            }).SetId("Search");
                    }
                    else
                    {
                        visual.transform.localPosition = target;
                        if (collide) collide.enabled = true;
                    }
                }
                else
                {
                    visual.transform.localPosition = target;
                }
            }
        }

        private void StopSameStateAnimation(NodeVisual nodeVisual = null)
        {
            foreach (var visual in _currentSameStateVisuals)
            {
                if (!visual)
                    continue;

                visual.StopAnimation();
                if (nodeVisual == null || visual != nodeVisual)
                    visual.StopBlinking();
            }

            _currentSameStateVisuals.Clear();
        }

        public void BlinkNode(SearchNode node, Color color)
        {
            if (_nodeVisuals.TryGetValue(node, out var visual))
                visual.BlinkNode(color);
        }

        public void BlinkNode(IState state, Color color)
        {
            var visual = _nodeVisuals.Values.FirstOrDefault(v => v.stateId == state.id);
            if (visual)
                visual.BlinkNode(color);
        }

        public void TryPlaySameState(SearchNode node)
        {
            _nodeVisuals.TryGetValue(node, out var nodeVisual);
            StopSameStateAnimation(nodeVisual);

            var newVisuals = _nodeVisuals.Values
                .Where(v => v.SearchNode.state.Equals(node.state))
                .ToList();

            foreach (var visual in newVisuals)
            {
                visual.PlaySameStateAnimation();
                if (visual != _nodeVisuals[node])
                    visual.BlinkNode(Color.darkBlue);
            }

            _currentSameStateVisuals = newVisuals;
        }
    }
}
