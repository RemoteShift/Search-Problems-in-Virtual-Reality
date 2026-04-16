using System.Collections.Generic;

namespace Search.Core
{
    public interface IFrontier<T>
    {
        void Add(T node);
        void AddRange(IEnumerable<T> nodes);
        T Remove();
        bool IsEmpty { get; }
    }
}
