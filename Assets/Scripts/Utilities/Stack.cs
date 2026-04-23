using System.Collections.Generic;
using Search.Core;
using System.Linq;

namespace Search.Utils
{
    public class Stack<T> : IFrontier<T>
    {
        private readonly System.Collections.Generic.Stack<T> _stack = new();

        public void Add(T node) => _stack.Push(node);
        public void AddRange(IEnumerable<T> nodes)
        {
            foreach (var node in nodes)
            {
                _stack.Push(node);
            }
        }

        public bool Contains(T node) => _stack.Contains(node);
        
        public T Peek() => _stack.Peek();

        public T Remove() => _stack.Pop();

        public bool IsEmpty => _stack.Count == 0;
        
        public int Count => _stack.Count;
        
        public void Clear() => _stack.Clear();
        
        public IReadOnlyList<T> ToList() => _stack.ToArray();
    }
}
