using System.Collections.Generic;
using Priority_Queue;

namespace notIridium
{
    internal class Edge : FastPriorityQueueNode
    {
        public readonly List<IPoint> Path;
        public readonly IPoint Target;
        public readonly int Weight;

        public Edge(List<IPoint> points, IPoint target, int i)
        {
            Path = points;
            Target = target;
            Weight = i;
        }
    }
}