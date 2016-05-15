using System.Collections.Generic;
using System.Windows.Documents;

namespace notIridium
{
    internal class Model
    {
        private static readonly char[] Splitter = { ',' };
        public readonly string Seed;
        public readonly Location From;
        public readonly Location To;
        public readonly List<Satellite> Sats = new List<Satellite>();

        public Model(string seed, string route)
        {
            this.Seed = seed;
            string[] r = route.Split(Splitter);
            this.From = new Location(r[1], r[2]);
            this.To = new Location(r[3], r[4]);
        }

        public void AddSat(string s)
        {
            string[] r = s.Split(Splitter);
            Sats.Add(new Satellite(r[0], r[1], r[2], r[3]));
        }
    }
}