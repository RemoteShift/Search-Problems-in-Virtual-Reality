using System;

namespace Search.Core
{
    public class MazeTransition : ITransitionFunction
    {
        private readonly bool[,] _mazeWalls;
        private readonly int _width, _height;

        public MazeTransition(bool[,] mazeWalls, int height, int width)
        {
          _mazeWalls = mazeWalls;
          _width = width;
          _height = height;
        }
        
        public IState GetSuccessor(IState state, string action)
        {
            if (state is not GridState gs) return null;
            
            int newRow = gs.Row, newColumn = gs.Column;
            switch (action)
            {
                case "Up":    newRow++; break;
                case "Down":  newRow--; break;
                case "Left":  newColumn--; break;
                case "Right": newColumn++; break;
                default: throw new InvalidOperationException();
            }
            
            if (newRow < 0 || newRow >= _height || newColumn < 0 || newColumn >= _width)
                return null;
            
            return _mazeWalls[newRow, newColumn] ? null : new GridState(newRow, newColumn);
        }
    }
}
