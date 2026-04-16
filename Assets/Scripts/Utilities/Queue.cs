using System.Collections.Generic;
using Search.Core;

namespace Search.Utils
{
    public class Queue<T> : IFrontier<T>
    {
        private readonly System.Collections.Generic.Queue<T> _queue = new();

        public void Add(T node) => _queue.Enqueue(node);
        public void AddRange(IEnumerable<T> nodes)
        {
            foreach (var node in nodes)
            {
                _queue.Enqueue(node);
            }
        }

        public T Remove() => _queue.Dequeue();

        public bool IsEmpty => _queue.Count == 0;
    }
}
