using System;
using System.Collections.Generic;
using Search.Core;

namespace Search.Utils
{
    #region Summary

    /// <summary>
    /// A binary-heap priority queue with index map so individual items can be removed or repositioned in O(log n).
    /// Comparison: comparison(a,b) &lt; 0 means 'a' has higher priority than 'b'.
    /// </summary>

    #endregion
    public class PriorityQueue<T> : IFrontier<T>
    {
        private readonly List<T> _heap = new();
        private readonly Dictionary<T,int> _indexMap = new();
        private readonly Comparison<T> _comparison;

        public PriorityQueue(Comparison<T> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        public int Count => _heap.Count;

        public bool IsEmpty => Count == 0;

        public void Clear()
        {
            _heap.Clear();
            _indexMap.Clear();
        }
        
        public void Add(T item)
        {
            _heap.Add(item);
            int idx = _heap.Count - 1;
            _indexMap[item] = idx;
            HeapifyUp(idx);
        }
        
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public T Remove()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("PriorityQueue is empty");
            var root = _heap[0];
            var lastIndex = _heap.Count - 1;

            if (lastIndex == 0)
            {
                _heap.RemoveAt(0);
                _indexMap.Remove(root);
                return root;
            }

            var last = _heap[lastIndex];
            _heap[0] = last;
            _indexMap[last] = 0;

            _heap.RemoveAt(lastIndex);
            _indexMap.Remove(root);

            HeapifyDown(0);
            return root;
        }

        public T Peek()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("PriorityQueue is empty");
            return _heap[0];
        }

        public bool Contains(T item) => _indexMap.ContainsKey(item);

        #region Summary

        /// <summary>
        /// Remove a specific item from the queue. Returns true if removed.
        /// O(log n).
        /// </summary>

        #endregion
        public bool Remove(T item)
        {
            if (!_indexMap.TryGetValue(item, out var idx)) return false;

            var lastIndex = _heap.Count - 1;
            var last = _heap[lastIndex];

            // Remove mapping for the item being removed
            _indexMap.Remove(item);

            if (idx == lastIndex)
            {
                // removing last element
                _heap.RemoveAt(lastIndex);
                return true;
            }

            // Move last into idx, pop last, then fix heap
            _heap[idx] = last;
            _indexMap[last] = idx;
            _heap.RemoveAt(lastIndex);

            // Try to restore heap property by sifting up or down
            if (!HeapifyUp(idx))
            {
                HeapifyDown(idx);
            }

            return true;
        }

        #region Summary

        /// <summary>
        /// If the item's priority/key has changed, call this to reposition it in the heap.
        /// Returns true if item was present and repositioned. O(log n).
        /// </summary>

        #endregion
        public bool TryUpdate(T item)
        {
            if (!_indexMap.TryGetValue(item, out var idx)) return false;
            // Try HeapifyUp first (if priority increased), otherwise HeapifyDown
            if (!HeapifyUp(idx))
            {
                HeapifyDown(idx);
            }
            return true;
        }

        private bool HeapifyUp(int index)
        {
            bool moved = false;
            while (index > 0)
            {
                int parent = (index - 1) >> 1;
                if (_comparison(_heap[index], _heap[parent]) >= 0) break;
                Swap(index, parent);
                index = parent;
                moved = true;
            }
            return moved;
        }

        private void HeapifyDown(int index)
        {
            int last = _heap.Count - 1;
            while (true)
            {
                int left = (index << 1) + 1;
                int right = left + 1;
                int best = index;

                if (left <= last && _comparison(_heap[left], _heap[best]) < 0)
                    best = left;
                if (right <= last && _comparison(_heap[right], _heap[best]) < 0)
                    best = right;

                if (best == index) break;

                Swap(index, best);
                index = best;
            }
        }

        private void Swap(int i, int j)
        {
            (_heap[i], _heap[j]) = (_heap[j], _heap[i]);

            _indexMap[_heap[i]] = i;
            _indexMap[_heap[j]] = j;
        }
    }
}
