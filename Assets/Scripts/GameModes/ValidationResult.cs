using System.Collections.Generic;
using Search.Visualization;

namespace Search.GameModes
{
    public struct ValidationResult
    {
        public bool Success;
		public string Message;
		public List<NodeVisual> MisplacedNodeVisuals;

		public static ValidationResult Ok()
		{
			return new ValidationResult
			{
				Success = true,
				Message = "OK"
			};
		}
		
		public static ValidationResult Fail(string message)
		{
			return new ValidationResult
			{
				Success = false,
				Message = message,
			};
		}

		public static ValidationResult Fail(string message, List<NodeVisual> misplacedNodeVisuals)
		{
			return new ValidationResult
			{
				Success = false,
				Message = message,
				MisplacedNodeVisuals = misplacedNodeVisuals
			};
		}
    }
}