using UnityEngine;
using Search.Core;

namespace Search.Visualization
{
    public class NodeVisual : MonoBehaviour
    {
        private MeshRenderer _renderer;
        [SerializeField] private Material defaultMat;
        [SerializeField] private Material frontierMat;
        [SerializeField] private Material expandedMat;
        [SerializeField] private Material pathMat;
        public NodeState currentState { get; private set; }
        public string stateId { get; private set; }

        public void Initialize(IState state, Vector3 position)
        {
            stateId = state.id;
            transform.position = position;
            _renderer = GetComponent<MeshRenderer>();
            SetState(NodeState.Default);
        }

        public void SetState(NodeState newState)
        {
            currentState = newState;
            _renderer.material = newState switch
            {
                NodeState.Default => defaultMat,
                NodeState.Frontier => frontierMat,
                NodeState.Expanded => expandedMat,
                NodeState.Path => pathMat,
                _ => defaultMat
            };
        }
    }
}