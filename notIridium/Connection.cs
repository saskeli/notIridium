namespace notIridium
{
    public class Connection
    {
        public readonly IPoint Target;
        public readonly int Weight;
        public Connection(IPoint target, int weight)
        {
            this.Target = target;
            this.Weight = weight;
        }
    }
}