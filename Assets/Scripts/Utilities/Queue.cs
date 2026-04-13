using Search.Core;

namespace Search.Utils
{
    public class Queue<T> : IFrontier<T>
    {
        private readonly Queue<T> _queue = new();

        public void Enqueue(T node) => _queue.Enqueue(node);

        public T Dequeue() => _queue.Dequeue();

        public bool IsEmpty => _queue.IsEmpty;
    }
}
