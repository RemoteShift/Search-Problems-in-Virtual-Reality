using System;

namespace SearchCore
{
    public class MazeTransition : ITransitionFunction
    {
        private readonly bool[,] _mazeWalls;
        public MazeTransition(bool[,] mazeWalls) => _mazeWalls = mazeWalls;
        
        public IState GetSuccessor(IState state, string action)
        {
            if (state is not GridState gs) return null;
            return action switch
            {
                "Up" => !_mazeWalls[gs.X, gs.Y + 1] ? new GridState(gs.X, gs.Y + 1) : null,
                "Down" => !_mazeWalls[gs.X, gs.Y - 1] ? new GridState(gs.X, gs.Y - 1) : null,
                "Left" => !_mazeWalls[gs.X - 1, gs.Y] ? new GridState(gs.X - 1, gs.Y) : null,
                "Right" => !_mazeWalls[gs.X + 1, gs.Y] ? new GridState(gs.X + 1, gs.Y) : null,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
