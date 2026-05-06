using System;
using Search.Levels;
using UnityEngine;

namespace Search.Visualization
{
    public class NodeVisualPasser : MonoBehaviour
    {
        public void SetNodeManualPosition()
        {
            var nodeVisual = GetComponentInChildren<NodeVisual>();
            if (!nodeVisual) return;
    
            var treeVisualizer = LevelManager.Instance?.TreeVisualizer;
            treeVisualizer?.SetNodeManualPosition(nodeVisual);
        }

        public void EnableAndPassNodeVisualStatsUI()
        {
            NodeStatsUI.Instance.EnableAndUpdateNodeStatsUI(GetComponentInChildren<NodeVisual>());
        }

        public void DisableNodeStatsUI()
        {
            NodeStatsUI.Instance.DisableNodeStatsUI();
        }
    }
}

