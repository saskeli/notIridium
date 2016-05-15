using System;

namespace notIridium
{
    public class GeoCoordinate
    {
        private readonly double _lat;
        private readonly double _lon;
        private readonly double _alt;
        private Coordinate _coord = null;

        public GeoCoordinate(double lat, double lon, double alt)
        {
            this._lat = lat;
            this._lon = lon;
            this._alt = alt;
        }

        public double Altitude
        {
            get { return _alt; }
        }

        public double GetDistanceTo(GeoCoordinate o)
        {
            double dx = Coord.X - o.Coord.X;
            double dy = Coord.Y - o.Coord.Y;
            double dz = Coord.Z - o.Coord.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public Coordinate Coord
        {
            get
            {
                if (this._coord == null)
                {
                    double z = Math.Sin((Math.PI / 180) * _lat) * _alt;
                    double b = Math.Cos((Math.PI / 180) * _lat) * _alt;
                    double x = Math.Cos((Math.PI / 180) * _lon) * b;
                    double y = Math.Sin((Math.PI / 180) * _lon) * b;
                    _coord = new Coordinate(x, y, z);
                }
                return _coord;
            }
        }

        public int Latitude
        {
            get { return (int)_lat; }
        }

        public int Longitude
        {
            get { return (int) _lon; }
        }
    }
}