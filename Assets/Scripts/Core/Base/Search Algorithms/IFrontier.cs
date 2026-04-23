using System.Collections.Generic;

namespace Search.Core
{
    public interface IFrontier<T>
    {
        void Add(T node);
        void AddRange(IEnumerable<T> nodes);
        bool Contains(T node);
        T Peek();
        T Remove();
        bool IsEmpty { get; }
        int Count { get; }
        void Clear();
        IReadOnlyList<T> ToList();
    }
}
