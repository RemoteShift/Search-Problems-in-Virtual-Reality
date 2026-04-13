using System;
using System.Collections.Generic;
using System.Linq;
using Search.Core;
using UnityEngine;

namespace Search.Levels
{
    [CreateAssetMenu(fileName = "MazeLevel", menuName = "Levels/Maze")]
    public class MazeLevelData : LevelData
    {
        public int width;
        public int height;
        public Vector2Int start;
        public Vector2Int[] goals;
        public List<Vector2Int> wallsPositions; // only cells where there are walls, the rest is considered empty

        private bool[,] walls;

        public override SearchProblem CreateSearchProblem()
        {
            var actions = new List<string> { "Up", "Down", "Left", "Right" };

            var walls = GetWalls2D();
            
            ITransitionFunction transitionFunc = new MazeTransition(walls, height, width);
            IStepCostFunction stepCostFunc = new MazeStep();
            
            var initialState = new GridState(start.x, start.y);

            return new SearchProblem(initialState, GoalTest, actions, transitionFunc, stepCostFunc, HeuristicFunction);

            bool GoalTest(IState state)
            {
                return state is GridState gs && goals.Any(goal => gs.Row == goal.x && gs.Column == goal.y);
            }

            float HeuristicFunction(IState state)
            {
                if (state is not GridState gs) return 0f;
                return goals.Min(goal => Mathf.Abs(gs.Row - goal.x) + Mathf.Abs(gs.Column - goal.y));
            }
        }
        
        public bool[,] GetWalls2D()
        {
            if (walls != null)
            {
                return walls;
            }
            walls = new bool[height, width];
            foreach (var wall in wallsPositions.Where(pos => pos.x >= 0 && pos.x < height && 
                                                            pos.y >= 0 && pos.y < width))
            {
                walls[wall.x, wall.y] = true;
            }
            return walls;
        }
    }
}
