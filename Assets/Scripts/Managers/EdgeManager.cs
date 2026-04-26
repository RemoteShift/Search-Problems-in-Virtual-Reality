using System;
using System.Collections.Generic;
using Search.Utils;
using UnityEngine;

namespace Search.Visualization
{
    public class EdgeManager : Singleton<EdgeManager>
    {
        public Material edgeMaterial;
        public float edgeWidth;
        
        private readonly Dictionary<(string, string), GameObject> _edges = new();
    
        public void AddEdge(string fromId, string toId, Transform from, Transform to)
        {
            // Normalize so (A,B) and (B,A) both use the same key
            var key = (string.Compare(fromId, toId, StringComparison.Ordinal) < 0) ? 
                (fromId, toId) : (toId, fromId);
    
            if (_edges.ContainsKey(key))
                return;
            
            var edgeObj = new GameObject($"Edge_{fromId}_{toId}");
            edgeObj.transform.SetParent(transform);
            
            var lr = edgeObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new[] { from.position, to.position });
            lr.material = edgeMaterial;
            lr.startWidth = edgeWidth;
            lr.endWidth = edgeWidth;
            lr.useWorldSpace = true;

            var updater = edgeObj.AddComponent<EdgeDynamicUpdater>();
            updater.Initialize(from, to);
            _edges[key] = edgeObj;
        }

        public void RemoveEdge(string fromId, string toId)
        {
            var key = (string.Compare(fromId, toId, StringComparison.Ordinal) < 0) ? 
                (fromId, toId) : (toId, fromId);
            
            var edgeObject = _edges.GetValueOrDefault(key);
            
            if (!edgeObject)
                return;
            
            Destroy(edgeObject);
            _edges.Remove(key);
        }
    
        public void ClearEdges()
        {
            foreach (var go in _edges.Values) Destroy(go);
            _edges.Clear();
        }
    }
}