using Search.Culling;
using UnityEngine;

namespace Search.Visualization
{
    public class EdgeDynamicUpdater : MonoBehaviour
    {
        private LineRenderer _lr;
        private Transform _from;
        private Transform _to;
    
        private CanvasCulling _cullingScript;
        
        public void Initialize(Transform from, Transform to)
        {
            _from = from;
            _to = to;
            _lr = GetComponent<LineRenderer>();
            _cullingScript = GetComponentInChildren<CanvasCulling>();
            if(!_cullingScript)
                Debug.LogWarning("DistanceCulling script not found on edge label. Distance-based culling will not work.");
        }

        private void LateUpdate()
        {
            if (!_from || !_to)
            {
                EdgeManager.Instance.RemoveEdge(gameObject);
                return;
            }
            
            if(!_from.gameObject.activeSelf ||  !_to.gameObject.activeSelf)
                _lr.enabled = false;
            else
                _lr.enabled = true;
            
            if (_from && _to)
                _lr.SetPositions(new[] { _from.position, _to.position });
            
            transform.position = (_from.position + _to.position) * 0.5f;
            _cullingScript.maxDistance = Mathf.Max(Vector3.Distance(_from.position, _to.position), 4f, 10f);
        }
    }
}