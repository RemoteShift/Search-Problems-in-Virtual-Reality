namespace Search.Core
{
    public interface IFrontier<T>
    {
        void Enqueue(T node);
        T Dequeue();
        bool IsEmpty { get; }
    }
}
