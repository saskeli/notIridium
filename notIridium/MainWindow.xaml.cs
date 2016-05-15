using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Priority_Queue;

namespace notIridium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly int[] Weights = { 0, 1, 2};
        private readonly Regex _seedRGX = new Regex(@"(?<=#SEED: ).*\d");
        private readonly Regex _satRGX = new Regex(@"SAT.*\d");
        private readonly Regex _routeRGX = new Regex(@"ROUTE.*\d");
        private Model _model = null;

        public MainWindow()
        {
            InitializeComponent();
            WeightBox.ItemsSource = Weights;
            WeightBox.SelectedIndex = 1;
            _model = GenerateGraph(defaultData.Data);
            SeedBox.Text = _model.Seed;
            PopulateCanvas();
            FindPath();
            WeightBox.SelectionChanged += WeightBox_SelectionChanged;
        }

        private void WeightBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FindPath();
        }

        private void DoButton_OnClick(object sender, RoutedEventArgs e)
        {
            string s = GetData();
            _model = GenerateGraph(s);
            SeedBox.Text = _model.Seed;
            PopulateCanvas();
            FindPath();
        }

        private void FindPath()
        {
            ClearHighlights();
            HashSet<IPoint> visited = new HashSet<IPoint>();
            FastPriorityQueue<Edge> pq = new FastPriorityQueue<Edge>(400);
            foreach (Connection c in _model.From.Links)
            {
                int d = (int) Math.Pow(c.Weight, WeightBox.SelectedIndex);
                pq.Enqueue(new Edge(new List<IPoint>(), c.Target, d), d);
            }
            while (pq.Count > 0)
            {
                Edge e = pq.Dequeue();
                if (visited.Contains(e.Target)) continue;
                if (e.Target == _model.To)
                {
                    Highlight(e.Path);
                    return;
                }
                visited.Add(e.Target);
                foreach (Connection c in e.Target.Links)
                {
                    if (visited.Contains(c.Target)) continue;
                    int d = e.Weight + ((int) Math.Pow(c.Weight, WeightBox.SelectedIndex));
                    List<IPoint> p = new List<IPoint>(e.Path);
                    p.Add(e.Target);
                    pq.Enqueue(new Edge(p, c.Target, d), d);
                }
            }
            PathBox.Text = "No path found.";
        }

        private void ClearHighlights()
        {
            List<Line> lo = new List<Line>();
            foreach (object c in SatCanvas.Children)
            {
                if (c is Line && ((Line)c).StrokeThickness == 4) lo.Add((Line)c);
            }
            foreach (Line line in lo)
            {
                SatCanvas.Children.Remove(line);
            }
        }

        private void Highlight(List<IPoint> path)
        {
            List<string> sl = new List<string>();
            LinkPath(_model.From, path[0], -1);
            for (int i = 0; i < path.Count - 1; i++)
            {
                LinkPath(path[i], path[i + 1], -1);
                sl.Add(((Satellite)path[i]).Id);
            }
            LinkPath(path[path.Count - 1], _model.To, -1);
            PathBox.Text = String.Join(",", sl);
        }

        private string GetData()
        {
            WebRequest r = WebRequest.Create("https://space-fast-track.herokuapp.com/generate");
            using (WebResponse res = r.GetResponse())
            {
                Stream dStream = res.GetResponseStream();
                using (StreamReader reader = new StreamReader(dStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private Model GenerateGraph(string data)
        {
            Model m = Parse(data);
            GenerateLinks(m);
            return m;
        }

        private void GenerateLinks(Model m)
        {
            EndPointAccess(m);
            for (int i = 0; i < m.Sats.Count; i++)
            {
                for (int j = i + 1; j < m.Sats.Count; j++)
                {
                    int weight = Visible(m.Sats[i], m.Sats[j]);
                    if (weight != 0)
                    {
                        m.Sats[i].AddLink(m.Sats[j], weight);
                        m.Sats[j].AddLink(m.Sats[i], weight);
                    }
                }
            }
        }

        private void EndPointAccess(Model model)
        {
            double fromClosestDistance = Double.PositiveInfinity;
            IPoint fromClosestSat = null;
            List<IPoint> fromAccessible = new List<IPoint>();
            double toClosestDistance = Double.PositiveInfinity;
            IPoint toClosestSat = null;
            List<IPoint> toAccessible = new List<IPoint>();
            foreach (Satellite sat in model.Sats)
            {
                double distance = Distance(model.From, sat);
                if (distance < Double.PositiveInfinity)
                {
                    if (distance < fromClosestDistance)
                    {
                        fromClosestDistance = distance;
                        fromClosestSat = sat;
                    }
                    fromAccessible.Add(sat);
                }
                distance = Distance(model.To, sat);
                if (distance < Double.PositiveInfinity)
                {
                    if (distance < toClosestDistance)
                    {
                        toClosestDistance = distance;
                        toClosestSat = sat;
                    }
                    toAccessible.Add(sat);
                }
            }
            if (fromClosestSat != null)
            {
                model.From.AddLink(fromClosestSat, 1);
                fromClosestSat.AddLink(model.From, 1);
                foreach (IPoint point in fromAccessible)
                {
                    if (!point.Equals(fromClosestSat))
                    {
                        model.From.AddLink(point, 3);
                        point.AddLink(model.From, 3);
                    }
                }
            }
            if (toClosestSat != null)
            {
                model.To.AddLink(toClosestSat, 1);
                toClosestSat.AddLink(model.To, 1);
                foreach (IPoint point in toAccessible)
                {
                    if (!point.Equals(toClosestSat))
                    {
                        model.To.AddLink(point, 3);
                        point.AddLink(model.To, 3);
                    }
                }
            }
        }

        private double Distance(IPoint a, IPoint b)
        {
            if (Visible(a, b) > 0)
            {
                return a.Coord.GetDistanceTo(b.Coord);
            }
            return Double.PositiveInfinity;
        }

        private int Visible(IPoint a1, IPoint b1)
        {
            IPoint a = a1.Coord.Altitude > b1.Coord.Altitude ? a1 : b1;
            IPoint b = a1 == a ? b1 : a1;
            double la = a.Coord.Altitude;
            double lb = a.Coord.GetDistanceTo(b.Coord);
            double lc = b.Coord.Altitude;
            double aa = Math.Acos((Math.Pow(la, 2) + Math.Pow(lb, 2) - Math.Pow(lc, 2)) / (2 * la * lb));
            double minH = la * Math.Sin(aa);
            double kLen = la * Math.Cos(aa);
            if (kLen >= lb) return 1;
            if (minH >= 6471000) return 1;
            if (minH >= 6371000) return 3;
            return 0;
        }

        private Model Parse(string s)
        {
            Model m = new Model(_seedRGX.Match(s).Value, _routeRGX.Match(s).Value);
            foreach (Match match in _satRGX.Matches(s))
            {
                m.AddSat(match.Value);
            }
            return m;
        }

        private void PopulateCanvas()
        {
            SatCanvas.Children.Clear();
            FromPoly();
            ToPoly();
            foreach (Satellite s in _model.Sats)
            {
                SatPoly(s);
            }
        }

        private void SatPoly(Satellite s)
        {
            Ellipse sat = new Ellipse();
            sat.Width = 6;
            sat.Height = 6;
            sat.Fill = Brushes.Black;
            sat.ToolTip = "ID: " + s.Id + "\nlat: " + s.Coord.Latitude + "\nlon: " + s.Coord.Longitude + "\nalt: " +
                          (int)((s.Coord.Altitude - 6371000) / 1000);
            Canvas.SetTop(sat, (s.Coord.Latitude * -1 + 90) * 2 - 3);
            Canvas.SetLeft(sat, (s.Coord.Longitude + 180) * 2 - 3);
            SatCanvas.Children.Add(sat);
            foreach (Connection c in s.Links)
            {
                if (c.Target.GetType() == s.GetType() && 
                    String.Compare(s.Id, ((Satellite)c.Target).Id, StringComparison.Ordinal) > 0)
                {
                    LinkPath(s, c.Target, c.Weight);
                }
            }
        }

        private void ToPoly()
        {
            foreach (Connection c in _model.To.Links)
            {
                LinkPath(_model.To, c.Target, c.Weight);
            }
            Polygon to = new Polygon();
            to.Points.Add(new Point(-5, -5));
            to.Points.Add(new Point(5, -5));
            to.Points.Add(new Point(0, 5));
            to.Fill = Brushes.Blue;
            to.ToolTip = "lat: " + _model.To.Coord.Latitude + "\nlon: " + _model.To.Coord.Longitude + "\nalt: " +
                          (int)((_model.To.Coord.Altitude - 6371000) / 1000);
            Canvas.SetTop(to, (_model.To.Coord.Latitude * -1 + 90) * 2);
            Canvas.SetLeft(to, (_model.To.Coord.Longitude + 180) * 2);
            SatCanvas.Children.Add(to);
        }

        private void FromPoly()
        {
            foreach (Connection c in _model.From.Links)
            {
                LinkPath(_model.From, c.Target, c.Weight);
            }
            Polygon fm = new Polygon();
            fm.Points.Add(new Point(-5, 5));
            fm.Points.Add(new Point(0, -5));
            fm.Points.Add(new Point(5, 5));
            fm.Fill = Brushes.Blue;
            fm.ToolTip = "lat: " + _model.From.Coord.Latitude + "\nlon: " + _model.From.Coord.Longitude + "\nalt: " +
                          (int)((_model.From.Coord.Altitude - 6371000) / 1000);
            Canvas.SetTop(fm, (_model.From.Coord.Latitude * -1 + 90) * 2);
            Canvas.SetLeft(fm, (_model.From.Coord.Longitude + 180) * 2);
            SatCanvas.Children.Add(fm);
        }

        private void LinkPath(IPoint from, IPoint to, int weight)
        {
            Line link = new Line();
            link.X1 = (from.Coord.Longitude + 180) * 2;
            link.X2 = (to.Coord.Longitude + 180) * 2;
            link.Y1 = (from.Coord.Latitude * -1 + 90) * 2;
            link.Y2 = (to.Coord.Latitude * -1 + 90) * 2;
            if (weight > 1) link.Stroke = Brushes.Red;
            else if (weight == -1)
            {
                link.Stroke = new SolidColorBrush(Brushes.Aquamarine.Color);
                link.Stroke.Opacity = 0.5;
                link.StrokeThickness = 4;
            }
            else link.Stroke = Brushes.Black;
            SatCanvas.Children.Add(link);
        }
    }
}
