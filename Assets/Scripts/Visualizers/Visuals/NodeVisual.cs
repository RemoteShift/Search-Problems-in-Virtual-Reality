using System.Collections;
using UnityEngine;
using Search.Core;
using Search.GameModes;

namespace Search.Visualization
{
    public class NodeVisual : MonoBehaviour
    {
        private MeshRenderer _renderer;
        public Color currentOriginalColor;
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
        public bool manuallyPositioned = false;
        private Vector3 initialScale;

        [HideInInspector] public SearchNode SearchNode;
        private Coroutine _blinkingCoroutine;

        // NEW: mark visuals that were just created so visualizers can animate only those
        [HideInInspector] public bool isNew = true;
        
        
        private Animator _anim;
        public void Initialize(IState state, SearchNode searchNode)
        {
            stateId = state.id;
            SearchNode = searchNode;
            initialScale = transform.localScale;
            _renderer = GetComponent<MeshRenderer>();
            _anim = GetComponent<Animator>();
            SetState(NodeState.Default);

            // By default, a newly initialized visual is considered new (animation should run)
            isNew = true;
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
            currentOriginalColor = _renderer.material.color;
        }

        public void BlinkNode(Color color)
        {
            StopBlinking();
            _blinkingCoroutine = StartCoroutine(Blink(color));
        }
        
        public void StopBlinking()
        {
            if (_blinkingCoroutine != null)
                StopCoroutine(_blinkingCoroutine);
            _renderer.material.color = currentOriginalColor;
        }
        
        private IEnumerator Blink(Color color)
        {
            while (true)
            {
                _renderer.material.color = color;
                yield return new WaitForSeconds(0.5f);
                _renderer.material.color = currentOriginalColor;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        public void SetColor(Color color)
        {
            _renderer.material.color = color;
        }
        
        public void PlaySameStateAnimation()
        {
            _anim.enabled = true;

            _anim.CrossFade("sameState", 0f);
        }

        public void StopAnimation()
        {
            transform.localScale = initialScale;
            _anim.enabled = false;
        }
        
        // Events

        public void SubscribeHandleNodeGrabbedQueue(bool isHovering)
        {
            switch (isHovering)
            {
                case true:
                    VRInputHandler.Instance.OnRightGridAndBPressChanged += HandleNodeGrabbedQueue;
                    break;
                case false:
                    if(!PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject)
                        VRInputHandler.Instance.OnRightGridAndBPressChanged -= HandleNodeGrabbedQueue;
                    break;
            }
        }

        [SerializeField] private GameObject queueElementPrefab;
        private GameObject currentlyGrabbedQueueElementObject
        {
            get => PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject;
            set => PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject = value;
        }

        private void HandleNodeGrabbedQueue(bool value)
        {
            if (SearchModeController.Instance.CurrentMode != SearchPlayMode.Solve) return;
            // if (currentState != NodeState.Default) return;
            if (currentlyGrabbedQueueElementObject && value)
                return;

            QueueElementUI currentlyGrabbedQueueElement;
            
            if (!value)
            {
                currentlyGrabbedQueueElement = currentlyGrabbedQueueElementObject?.GetComponent<QueueElementUI>();
                if (!(currentlyGrabbedQueueElement?.isDroppingIntoQueue ?? true))
                {
                    Destroy(currentlyGrabbedQueueElementObject);
                    currentlyGrabbedQueueElementObject = null;
                }
                
                PlayerQueueUI.Instance.GetComponent<Rigidbody>().isKinematic = false;
                
                VRInputHandler.Instance.OnRightGridAndBPressChanged -= HandleNodeGrabbedQueue;
                return;
            }
            
            var parentTransform = PlayerLocomotion.Instance.tempQueueHandAttachmentPoint;
            
            currentlyGrabbedQueueElementObject = Instantiate(queueElementPrefab, parentTransform);
            currentlyGrabbedQueueElementObject.layer = LayerMask.NameToLayer("Default");
            currentlyGrabbedQueueElement = currentlyGrabbedQueueElementObject.GetComponent<QueueElementUI>();
            
            currentlyGrabbedQueueElement.Initialize(this, 
                frontierMat.color); // Frontier node color

            PlayerQueueUI.Instance.GetComponent<Rigidbody>().isKinematic = true;
            
            EdgeManager.Instance.AddEdge("GrabbedNodeVisual", "TempQueueElementUI", 
                transform, currentlyGrabbedQueueElement.transform);
        }
    }
}