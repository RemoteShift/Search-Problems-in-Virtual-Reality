namespace Search.Core
{
   public interface IState
   {
       string id { get; }
       bool Equals(IState other);
   } 
}

