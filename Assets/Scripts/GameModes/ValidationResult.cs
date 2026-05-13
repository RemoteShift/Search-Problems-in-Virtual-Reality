namespace Search.GameModes
{
    public struct ValidationResult
    {
        public bool Success;
		public string Message;

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
				Message = message
			};
		}
    }
}