using System;

namespace SearchCore
{
    public class GridState : IState
    {
        public readonly int Row;
        public readonly int Column;
        public string id => $"{Row},{Column}";
        public GridState(int row, int column) => (Row, Column) = (row, column);
        public bool Equals(IState other) => other is GridState gs && gs.Row == Row && gs.Column == Column;
        public override bool Equals(object obj) => Equals(obj as IState);
        public override int GetHashCode() => HashCode.Combine(Row, Column);
    }
}
