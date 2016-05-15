using System;
using System.Collections.Generic;
using System.Globalization;

namespace notIridium
{
    public class Satellite : IPoint
    {
        public readonly string Id;
        private readonly GeoCoordinate _coord;
        private readonly LinkedList<Connection> _links = new LinkedList<Connection>();

        public Satellite(string id, string x, string y, string z)
        {
            this.Id = id;
            this._coord = new GeoCoordinate(Double.Parse(x, CultureInfo.InvariantCulture), 
                Double.Parse(y, CultureInfo.InvariantCulture), 
                (Double.Parse(z, CultureInfo.InvariantCulture) + 6371) * 1000);
        }

        public GeoCoordinate Coord
        {
            get { return _coord; }
        }

        public LinkedList<Connection> Links
        {
            get { return _links; }
        }

        public void AddLink(IPoint target, int weight)
        {
            _links.AddLast(new Connection(target, weight));
        }
    }
}