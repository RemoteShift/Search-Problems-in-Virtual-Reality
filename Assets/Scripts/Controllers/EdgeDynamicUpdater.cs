using UnityEngine;

namespace Search.Visualization
{
    public class EdgeDynamicUpdater : MonoBehaviour
    {
        private LineRenderer _lr;
        private Transform _from;
        private Transform _to;
    
        public void Initialize(Transform from, Transform to)
        {
            _from = from;
            _to = to;
            _lr = GetComponent<LineRenderer>();
        }

        private void LateUpdate()
        {
            if (_from && _to)
                _lr.SetPositions(new[] { _from.position, _to.position });
        }
    }
}