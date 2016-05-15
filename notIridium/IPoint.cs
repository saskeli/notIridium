using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace notIridium
{
    public interface IPoint
    {
        GeoCoordinate Coord { get; }
        LinkedList<Connection> Links { get; }

        void AddLink(IPoint target, int weight);
    }
}
