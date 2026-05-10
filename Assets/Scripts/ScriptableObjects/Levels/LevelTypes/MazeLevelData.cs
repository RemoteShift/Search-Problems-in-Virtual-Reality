using System;
using System.Collections.Generic;
using System.Linq;
using Search.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Search.Levels
{
    [CreateAssetMenu(fileName = "MazeLevel", menuName = "Levels/Maze")]
    public class MazeLevelData : LevelData
    {
        [FormerlySerializedAs("width")] public int columns;
        [FormerlySerializedAs("height")] public int rows;
        public Vector2Int start;
        public override IState startState { get; set; }
        public Vector2Int[] goals;
        public override List<IState> goalStates { get; set; }
        public List<Vector2Int> wallsPositions; // only cells where there are walls, the rest is considered empty

        private bool[,] walls;

        public override SearchProblem CreateSearchProblem()
        {
            var actions = new List<string> { "Up", "Down", "Left", "Right" };

            var walls2D = GetWalls2D();
            
            ITransitionFunction transitionFunc = new MazeTransition(walls2D, rows, columns);
            IStepCostFunction stepCostFunc = new MazeStep();
            
            var initialState = new GridState(start.x, start.y);
            startState = initialState;

            goalStates = goals.Select(g => new GridState(g.x, g.y)).Cast<IState>().ToList();
            
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
            walls = new bool[rows, columns];
            foreach (var wall in wallsPositions.Where(pos => pos.x >= 0 && pos.x < rows && 
                                                            pos.y >= 0 && pos.y < columns))
            {
                walls[wall.x, wall.y] = true;
            }
            return walls;
        }
    }
}
