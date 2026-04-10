using System;

namespace SearchCore
{
    public class GridState : IState
    {
        public readonly int X;
        public readonly int Y;
        public string id => $"{X},{Y}";
        public GridState(int x, int y) => (X, Y) = (x, y);
        public bool Equals(IState other) => other is GridState gs && gs.X == X && gs.Y == Y;
        public override bool Equals(object obj) => Equals(obj as IState);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}
