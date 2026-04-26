using System.Collections;
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
        [Tooltip("Used when a node is being expanded in the next step, " +
                 "to differentiate it from already expanded nodes.")]
        [SerializeField] private Material expandingMat;
        [SerializeField] private Material pathMat;
        public NodeState currentState { get; private set; }
        public string stateId { get; private set; }
        public Vector3 position => transform.position;

        public void Initialize(IState state, Vector3 newPosition)
        {
            stateId = state.id;
            transform.position = newPosition;
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
                NodeState.Expanding => expandingMat,
                NodeState.Path => pathMat,
                _ => defaultMat
            };
        }

        public IEnumerator Blink()
        {
            var initialColor = _renderer.material.color;
            while (true)
            {
                _renderer.material.color = Color.red;
                yield return new WaitForSeconds(0.5f);
                _renderer.material.color = initialColor;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}