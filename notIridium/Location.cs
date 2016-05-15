using System;
using System.Collections.Generic;
using System.Globalization;

namespace notIridium
{
    public class Location : IPoint
    {
        private readonly GeoCoordinate _coord;
        private readonly LinkedList<Connection> _links = new LinkedList<Connection>();

        public Location(string x, string y)
        {
            double yd = Double.Parse(y, CultureInfo.InvariantCulture);
            double xd = Double.Parse(x, CultureInfo.InvariantCulture);
            this._coord = new GeoCoordinate(xd, yd, 6371000d);
        }

        public void AddLink(IPoint target, int weight)
        {
            _links.AddLast(new Connection(target, weight));
        }

        public GeoCoordinate Coord
        {
            get { return _coord; }
        }

        public LinkedList<Connection> Links
        {
            get { return _links; }
        }
    }
}