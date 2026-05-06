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

        private SearchProblem _problem;
        private GameObject _nodeContainer;

        private readonly Dictionary<SearchNode, NodeVisual> _nodeVisuals = new();
        private readonly Dictionary<SearchNode, SearchNode> _parentMap = new(); // child → parent
        private readonly Dictionary<SearchNode, List<SearchNode>> _childrenMap = new(); // parent → children

        private readonly Dictionary<SearchNode, float> _subtreeWidth = new();
        private readonly Dictionary<SearchNode, float> _nodeX = new();
        private readonly Dictionary<SearchNode, int> _depth = new();

        public bool IsAnimating { get; }
        private List<NodeVisual> _currentSameStateVisuals = new();

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

        public NodeVisual GetNodeVisual(SearchNode node)
        {
            return _nodeVisuals.GetValueOrDefault(node);
        }

        public NodeVisual CreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var go = Instantiate(nodePrefab, _nodeContainer.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * nodeScale;
            var visual = go.GetComponentInChildren<NodeVisual>();
            visual.Initialize(node.state, node);
            _nodeVisuals[node] = visual;

            // --- Incrementally update parent/child maps ---
            if (parent != null)
            {
                _parentMap[node] = parent;
                if (!_childrenMap.ContainsKey(parent))
                    _childrenMap[parent] = new List<SearchNode>();
                _childrenMap[parent].Add(node);
            }

            // Manual positioning (if parent is manually placed)
            if (parent != null && _nodeVisuals.TryGetValue(parent, out var parentVisual) && parentVisual && parentVisual.manuallyPositioned)
            {
                var childIndex = _childrenMap[parent].Count - 1;   // index of this child
                var xOffset = (childIndex - 0.5f) * horizontalSpacing;
                var childLocalPos = parentVisual.transform.localPosition + new Vector3(xOffset, verticalSpacing, 0f);
                
                var searchController = SearchController.Instance;

                if (searchController.isAutomaticSearch)
                {
                    if (parentVisual)
                    {
                        visual.transform.localPosition = parentVisual.transform.localPosition;
                    }
                    
                    var collide = visual.GetComponent<Collider>();
                    collide.enabled = false;
                    
                    visual.transform.DOLocalMove(childLocalPos, searchController.treeNodeCreationAnimationDuration)
                        .SetEase(Ease.OutCirc).OnComplete(() =>
                        {
                            collide.enabled = true;
                        });
                }
                else
                {
                    visual.transform.localPosition = childLocalPos;
                }
                visual.manuallyPositioned = true;
            }

            LayoutTree();
            return visual;
        }

        public NodeVisual GetOrCreateNodeVisual(SearchNode node, SearchNode parent = null)
        {
            var existing = GetNodeVisual(node);

            if (existing)
            {
                return existing;
            }

            return CreateNodeVisual(node, parent);
        }
        
        /// <summary>
        ///     Call this from your VR grab script when a node is moved.
        /// </summary>
        public void SetNodeManualPosition(NodeVisual visual)
        {
            if (!visual || visual.SearchNode == null)
            {
                return;
            }

            // Mark the visual as manually positioned; TreeVisualizer will read the visual's transform when laying out.
            visual.manuallyPositioned = true;
        }

        public void ResetParent(SearchNode child)
        {
            if (child == null)
            {
                return;
            }

            if (!_nodeVisuals.TryGetValue(child, out var oldVisual) || !oldVisual)
            {
                return;
            }

            // Data has already been updated externally; use the current parent from SearchNode.
            var newParent = child.parent;

            // Detach child from its previous parent relation, if any.
            if (_parentMap.TryGetValue(child, out var previousParent) &&
                _childrenMap.TryGetValue(previousParent, out var previousSiblings))
            {
                previousSiblings.Remove(child);
            }

            if (newParent != null)
            {
                _parentMap[child] = newParent;
                if (!_childrenMap.ContainsKey(newParent))
                {
                    _childrenMap[newParent] = new List<SearchNode>();
                }

                if (!_childrenMap[newParent].Contains(child))
                {
                    _childrenMap[newParent].Add(child);
                }
            }
            else
            {
                _parentMap.Remove(child);
            }

            // Keep the old visual in the scene as historical marker.
            StartCoroutine(DeleteSubTree(oldVisual, subTreeDeletionDelay));

            // Create replacement visual and remap the active reference.
            var go = Instantiate(nodePrefab, _nodeContainer.transform);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * nodeScale;
            var newVisual = go.GetComponentInChildren<NodeVisual>();
            newVisual.Initialize(child.state, child);
            _nodeVisuals[child] = newVisual;

            // New visual starts as not manually positioned.
            newVisual.manuallyPositioned = false;

            // If the new parent is manually positioned, compute a position relative to the parent,
            // store it as manual, and apply it immediately.
            if (newParent != null && _nodeVisuals.TryGetValue(newParent, out var parentVis) && parentVis && parentVis.manuallyPositioned)
            {
                var siblingIndex = _childrenMap[newParent].IndexOf(child);
                if (siblingIndex < 0) siblingIndex = _childrenMap[newParent].Count - 1;
                var xOffset = (siblingIndex - 0.5f) * horizontalSpacing;
                var childLocalPos = parentVis.transform.localPosition + new Vector3(xOffset, verticalSpacing, 0f);
    
                newVisual.manuallyPositioned = true;
                newVisual.transform.localPosition = childLocalPos;
            }

            LayoutTree();
        }

        private IEnumerator DeleteSubTree(NodeVisual visual, float delay)
        {
            var subTree = GetSubTree(visual);
            foreach (var nodeVisual in subTree)
            {
                if (nodeVisual)
                {
                    nodeVisual.SetColor(Color.red);
                }
            }

            yield return new WaitForSeconds(delay);

            var deletedAnyActiveNode = false;
            foreach (var nodeVisual in subTree)
            {
                if (!nodeVisual)
                {
                    continue;
                }

                var node = nodeVisual.SearchNode;

                // Only remove dictionary mappings if this visual is still the active visual for this node.
                if (_nodeVisuals.TryGetValue(node, out var activeVisual) && activeVisual == nodeVisual)
                {
                    if (_parentMap.TryGetValue(node, out var parent) && _childrenMap.TryGetValue(parent, out var siblings))
                    {
                        siblings.Remove(node);
                    }

                    _parentMap.Remove(node);
                    _childrenMap.Remove(node);
                    // No manualPositions dictionary any more - manual flags live on the visuals. Nothing to remove here.
                    _nodeVisuals.Remove(node);
                    deletedAnyActiveNode = true;
                }
                
                Destroy(nodeVisual.gameObject);
            }

            if (deletedAnyActiveNode)
            {
                LayoutTree();
            }
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
                        {
                            stack.Push(childVisual);
                        }
                    }
                }
            }
            return result;
        }

        public List<NodeVisual> GetSubTree(SearchNode root)
        {
            return GetSubTree(_nodeVisuals.GetValueOrDefault(root));
        }
        
        /// <summary>
        ///     Clears all manually placed positions and recomputes the full tree layout.
        /// </summary>
        public void ResetManualPositions()
        {
            // Mark all visuals as not manually positioned so layout will recompute their positions.
            foreach (var v in _nodeVisuals.Values)
            {
                if (v) v.manuallyPositioned = false;
            }
            LayoutTree();
         }

         private void LayoutTree()
         {
             if (_nodeVisuals.Count == 0) return;

             // Clear reusable dictionaries
             _subtreeWidth.Clear();
             _nodeX.Clear();
             _depth.Clear();

             // Find roots (nodes without parent in _parentMap) – iterative, no LINQ
             var roots = new List<SearchNode>();
             foreach (var n in _nodeVisuals.Keys)
                 if (!_parentMap.ContainsKey(n))
                     roots.Add(n);
             if (roots.Count == 0) return;

             // ---------- 1. Compute subtree widths (iterative post-order) ----------
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

             // Process in reverse (post-order)
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

             // ---------- 2. Assign X coordinates (iterative post-order with state machine) ----------
             foreach (var root in roots)
             {
                 // Stack holds (node, leftBound, state)
                 // state 0 = first visit (push children), state 1 = children done (compute parent X)
                 var stackX = new Stack<(SearchNode node, float left, int state)>();
                 stackX.Push((root, 0f, 0));
    
                 while (stackX.Count > 0)
                 {
                     var (n, left, state) = stackX.Pop();
        
                     if (state == 0)
                     {
                         // First time seeing this node
                         if (!_childrenMap.TryGetValue(n, out var children) || children.Count == 0)
                         {
                             // Leaf – assign X directly
                             _nodeX[n] = left;
                             continue;
                         }
            
                         // Push the parent back with state=1 (to compute after children)
                         stackX.Push((n, left, 1));
            
                         // Push children in reverse order so they are processed left‑to‑right
                         float currentX = left;
                         for (int i = children.Count - 1; i >= 0; i--)
                         {
                             var child = children[i];
                             stackX.Push((child, currentX, 0));
                             currentX += _subtreeWidth[child] * horizontalSpacing;
                         }
                     }
                     else // state == 1 – all children have been processed
                     {
                         var children = _childrenMap[n];
                         float firstChildX = _nodeX[children[0]];
                         float lastChildX = _nodeX[children[^1]];
                         _nodeX[n] = (firstChildX + lastChildX) / 2f;
                     }
                 }
             }

             // ---------- 3. Compute depth (use postOrder – now root-first after reversal) ----------
             foreach (var n in postOrder) // postOrder was built root-first, so depth can be assigned top-down
             {
                 if (!_parentMap.TryGetValue(n, out var parent))
                     _depth[n] = 0;
                 else
                     _depth[n] = _depth[parent] + 1;
             }

             // ---------- 4. Center the non‑manual nodes (single pass) ----------
             float minX = float.MaxValue, maxX = float.MinValue;
             bool hasNonManual = false;
             foreach (var kvp in _nodeVisuals)
             {
                 var node = kvp.Key;
                 if (_nodeVisuals.TryGetValue(node, out var vis) && vis && vis.manuallyPositioned) continue;
                 float x = _nodeX[node];
                 if (x < minX) minX = x;
                 if (x > maxX) maxX = x;
                 hasNonManual = true;
             }

             float offsetX = hasNonManual ? -(minX + maxX) / 2f : 0f;

             // ---------- 5. Apply final positions ----------
             foreach (var kvp in _nodeVisuals)
             {
                 var node = kvp.Key;
                 var visual = kvp.Value;
                
                if (visual && visual.manuallyPositioned)
                    ; // keep visual.transform.localPosition as set by the user
                else
                {
                    var target = 
                        new Vector3(_nodeX[node] + offsetX, _depth[node] * verticalSpacing, 0f);
                    
                    var searchController = SearchController.Instance;
                    
                    if (searchController.isAutomaticSearch)
                    {
                        _parentMap.TryGetValue(node, out var parent);
                        if (parent != null)
                            visual.transform.localPosition = _nodeVisuals[parent].transform.localPosition;
                        
                        var collide = visual.GetComponent<Collider>();
                        collide.enabled = false;
                        
                        visual.transform.DOLocalMove(target, searchController.treeNodeCreationAnimationDuration)
                            .SetEase(Ease.OutCirc).OnComplete(() =>
                            {
                                collide.enabled = true;
                            });
                    }
                    else
                    {
                        visual.transform.localPosition = target;
                    }
                }
             }
         }

         public void BlinkNode(SearchNode node, Color color)
         {
             if (_nodeVisuals.TryGetValue(node, out var visual))
             {
                 visual.BlinkNode(color);
             }
         }

         public void BlinkNode(IState state, Color color)
         {
             var visual = _nodeVisuals.Values.FirstOrDefault(v => v.stateId == state.id);
             if (visual)
             {
                 visual.BlinkNode(color);
             }
         }

         public void TryPlaySameState(SearchNode node)
         {
             StopSameStateAnimation(node);

             var newVisuals = _nodeVisuals.Values
                 .Where(v => v.SearchNode.state.Equals(node.state))
                 .ToList();

             foreach (var visual in newVisuals)
             {
                 visual.PlaySameStateAnimation();
                 if (visual != _nodeVisuals[node])
                 {
                     visual.BlinkNode(Color.darkBlue);
                 }
             }

             _currentSameStateVisuals = newVisuals;
         }

         private void StopSameStateAnimation(SearchNode node = null)
         {
             foreach (var nodeVisual in _currentSameStateVisuals)
             {
                 nodeVisual.StopAnimation();
                 if (nodeVisual.SearchNode != node)
                 {
                     nodeVisual.StopBlinking();
                 }
             }

             _currentSameStateVisuals.Clear();
         }
     }
 }

