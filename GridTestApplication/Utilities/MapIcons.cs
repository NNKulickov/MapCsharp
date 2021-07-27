using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapProject.Utilities
{
    public class MapIcons
    {
        private int LegendTopCoordinate = 0;
        private struct LegendValues
        {
            public UIElement textUIElement;
            public UIElement shapeUIElement;
            public SolidColorBrush color;
        }
        private struct TriangleTrack
        {
            public Point coordinate;
            public double angle;
        }
        private struct IconValues
        {
            public string Type;
            public Point Coordinates;
            public UIElement uIElement;
        }
        private struct Designation
        {
            public string Name;
            public IconValues iconValues;
        }

        private struct Track
        {
            public List<Point> coordinates;
            public Dictionary<TriangleTrack, UIElement> triangle;
            public Dictionary<TrackLine, UIElement> lines;
        }

        private struct TrackLine
        {
            public Point startCoordinates;
            public Point endCoordinates;
        }

        private Dictionary<string, IconValues> Icons;
        private Dictionary<string, Track> Tracks;
        private Dictionary<string, LegendValues> Legends;

        public MapIcons()
        {
            Icons = new Dictionary<string, IconValues>();
            Tracks = new Dictionary<string, Track>();
            Legends = new Dictionary<string, LegendValues>();
        }

        public void DeleteIconOf(string name)
        {
            Icons.Remove(name);
        }

        public void DeleteTrackOf(string name)
        {
            Tracks.Remove(name);
        }

        public void Add(Map map ,Point coordinates,string type,string name)
        {
            SetIconOnMap(map, new Designation()
            {
                Name = name,
                iconValues = new IconValues()
                {
                    Coordinates = coordinates,
                    Type = type
                }
            });
        }

        public void AddTrackOn(Map map, Dictionary<DateTime, Point> coordinates, string name, 
            SolidColorBrush color)
        {
            AddTrack(map, coordinates, name, color);
            AddLegend(name, color);
        }

        public void InitialElementsOn(Map map)
        {
            DeleteAllIconsChild(map);
            InitialIconsOn(map);
            InitialTracksOn(map);
            InitialLegensOn(map);
        }

        public void Update(Map map)
        {
            map.Dispatcher.BeginInvoke(new Action(()=>UpdateIcons(map)));
            map.Dispatcher.BeginInvoke(new Action(() => UpdateTracks(map)));
            map.Dispatcher.BeginInvoke(new Action(() => UpdateLegends(map)));
        }

        private void InitialIconsOn(Map map)
        {
            foreach (var IconCoordinates in Icons.Keys)
            {
                IconValues element;
                if (Icons.TryGetValue(IconCoordinates, out element))
                {
                    UIElement image = element.uIElement;
                    AddElementToParentChild(map, image);
                }
            }
        }

        private void InitialTracksOn(Map map)
        {
            foreach (string name in Tracks.Keys)
            {
                if (Tracks.TryGetValue(name, out Track track))
                {
                    AddElementsToParentChild(map, track.lines.Values.ToList<UIElement>());
                    AddElementsToParentChild(map, track.triangle.Values.ToList<UIElement>());
                   
                }                   
            }
        }

        private void InitialLegensOn(Map map)
        {
            foreach (string name in Legends.Keys)
            {
                LegendValues element;
                if (Legends.TryGetValue(name, out element))
                {
                    SetLegendOnMap(map, element.textUIElement, element.shapeUIElement);
                    AddElementToParentChild(map, element.textUIElement);
                    AddElementToParentChild(map, element.shapeUIElement);
                }
            }
        }

        private void UpdateIcons(Map map)
        {
            foreach (var IconCoordinates in Icons.Keys)
            {
                IconValues element;
                if (Icons.TryGetValue(IconCoordinates, out element))
                {
                    SetIconPosition(map,element.Coordinates, element.uIElement);
                }
            }
        }

        private void UpdateTracks(Map map)
        {
            foreach (string name in Tracks.Keys)
            {
                if (Tracks.TryGetValue(name, out Track track))
                {
                    SetTrackOnMap(map,ref track.triangle,ref track.lines);
                }
            }
        }

        private void UpdateLegends(Map map)
        {
            foreach(string name in Legends.Keys)
            {
                if (Legends.TryGetValue(name, out LegendValues element))
                {
                    SetLegendOnMap(map, element.textUIElement,element.shapeUIElement);
                }
            }
        }

        private void AddLegend(string name, SolidColorBrush color)
        {
            LegendTopCoordinate += 50;
            Label TextLegend = GetTextLegend(name);
            Ellipse ellipse = GetColorShapeLegend(color);
            Legends.Add(name, new LegendValues() { color = color, textUIElement = TextLegend, shapeUIElement = ellipse });
        } 

        private Label GetTextLegend(string name)
        {
            Label TextLegend = new Label() { Width = 100, Height = 30, Content = name };
            Canvas.SetTop(TextLegend, LegendTopCoordinate);
            return TextLegend;
        }

        private Ellipse GetColorShapeLegend(SolidColorBrush color)
        {
            Ellipse ellipse = new Ellipse(){Width = 10, Height = 10, Fill = color };
            Canvas.SetTop(ellipse, LegendTopCoordinate + 5);
            return ellipse;
        }

        private void SetLegendOnMap(Map map,UIElement TextLegend,UIElement Shape)
        {
            Canvas.SetLeft(TextLegend, ((Panel)map.Parent).Width - 100);
            Canvas.SetTop(TextLegend, Canvas.GetTop(TextLegend));
            Canvas.SetLeft(Shape, ((Panel)map.Parent).Width - 120);
            Canvas.SetTop(Shape, Canvas.GetTop(Shape) );
        } 

        private void SetTrackOnMap(Map map, ref Dictionary<TriangleTrack, UIElement> circles,
            ref Dictionary<TrackLine,UIElement> lines)
        {
            List<Line> linesShape = new List<Line>();
            SetLineOnMap(map, ref lines, ref linesShape);
            SetTriangleOnMap(map,ref circles,linesShape);
        }

        private void SetLineOnMap(Map map,ref Dictionary<TrackLine, UIElement> lines,ref List<Line> linesShape)
        {
            foreach (var lineCoordinates in lines.Keys)
            {
                UIElement line = null;
                if (lines.TryGetValue(lineCoordinates, out line))
                {
                    SetLinePosition(map, lineCoordinates, line);
                    linesShape.Add((Line)line);
                }
            }
        }
        private void SetTriangleOnMap(Map map, ref Dictionary<TriangleTrack, UIElement> circles, List<Line> linesShape)
        {
            int i = 0;
            foreach (var triangleCoordinates in circles.Keys)
            {
                if (circles.TryGetValue(triangleCoordinates, out UIElement triangle))
                {
                    RotateTriangleWithLine(linesShape.ElementAt(i++), (Polygon)triangle);
                    SetTrianglePosition(map, triangleCoordinates.coordinate, triangle);
                }
            }
        }
        private void AddTrack(Map map, Dictionary<DateTime, Point> Coordinates , string name, System.Windows.Media.SolidColorBrush color)
        {
            Dictionary<TriangleTrack, UIElement> _triangles = new Dictionary<TriangleTrack, UIElement>();
            Dictionary<TrackLine, UIElement> _lines = new Dictionary<TrackLine, UIElement>();
            foreach (DateTime date in Coordinates.Keys.ToList())
            {
                if(Coordinates.TryGetValue(date, out Point coordinates))
                {
                    AddLineToTrack(map, coordinates, Coordinates.Values.ToList(), ref _lines, color);
                    //_circles.Add(coordinates, GetTrackCircle(map, coordinates,color));
                    AddTriangleToTrack(map, date, Coordinates, ref _triangles, color);
                }
                
            }
            Tracks.Add(name, new Track() { triangle = _triangles, coordinates = Coordinates.Values.ToList(), lines = _lines});
        }

        private void AddLineToTrack(Map map,Point coordinates, List<Point> CoordinatesList,
            ref Dictionary<TrackLine, UIElement> _lines, SolidColorBrush color)
        {
            if(CoordinatesList.IndexOf(coordinates)!= CoordinatesList.Count - 1 )
            {
                TrackLine trackLine = GetTrackLineCoordinates(coordinates, CoordinatesList);
                if(!_lines.ContainsKey(trackLine))
                _lines.Add(trackLine, GetTrackLineShape(map,trackLine,color));
            }
        }

        private void AddTriangleToTrack(Map map, DateTime date, Dictionary<DateTime, Point> CoordinatesList,
            ref Dictionary<TriangleTrack, UIElement> _triangles, SolidColorBrush color)
        {
            if (CoordinatesList.Keys.ToList().IndexOf(date) != CoordinatesList.Count - 1)
            {
                CoordinatesList.TryGetValue(date, out Point coordinates);
                TriangleTrack track = new TriangleTrack() { angle = 0, coordinate = coordinates };
                if(!_triangles.ContainsKey(track))
                    _triangles.Add(track, GetTriangle(map, coordinates, color, date));
            }
        }

        private TrackLine GetTrackLineCoordinates(Point currentCoordinates,List<Point> CoordinatesList)
        {
            int currentIndex = CoordinatesList.IndexOf(currentCoordinates);
            Point NextCoordinate = CoordinatesList.ElementAt(currentIndex + 1);
            return new TrackLine() { startCoordinates = currentCoordinates, endCoordinates = NextCoordinate };
        }

        private UIElement GetTrackLineShape(Map map, TrackLine trackLine, SolidColorBrush color)
        {
            Line line = new Line()
            {
                Stroke = color,
                StrokeThickness = 2,
                X1 = 0,Y1 = 0,
            };
            SetLinePosition(map, trackLine, line);
            return line;
        }

        private UIElement GetTriangle(Map map,Point coordinates,SolidColorBrush color, DateTime date)
        {
            Polygon poly = new Polygon();
            poly.Points = GetTrianglePoints();
            poly.Fill = color;
            AddDate(ref poly, date);
            SetTrianglePosition(map, coordinates,poly);
            return poly;
        }

        private void AddDate(ref Polygon poly, DateTime date)
        {
            ContextMenu context = new ContextMenu();
            MenuItem menuItem = new MenuItem() { Header = date.ToString("dd/MM/yyyy HH:mm:ss") };
            context.Items.Add(menuItem);
            poly.ContextMenu = context;
        }
        private void RotateTriangleWithLine(Line line,Polygon poly)
        {
            Point p1 = poly.Points.ElementAt(2);
            Point p2 = poly.Points.ElementAt(3);
            CheckAndRotateTriangle(line,p1,p2,ref poly);
        }

        private void CheckAndRotateTriangle(Line line,Point p1,Point p2, ref Polygon poly)
        {
            double currentangle = Vector.AngleBetween(GetCurrentVector(p1, p2), GetSourceVector());
            double angleToRotate = GetRotatedAngleFrom(line);
            double deltaAngle = currentangle - angleToRotate;
            if (deltaAngle != 0)
                RotateTriangle(ref poly, deltaAngle);
        }
        private Vector GetCurrentVector(Point p1,Point p2)
        {
            return new Vector() { X = p1.X - p2.X, Y = p1.Y - p2.Y };
        }
        private Vector GetSourceVector()
        {
            Point leftNode = new Point(6, -2);
            Point rightNode = new Point(-6, -2);
            return new Vector() { X = leftNode.X - rightNode.X, Y = leftNode.Y - rightNode.Y };
        }
        private double GetRotatedAngleFrom(Line line)
        {
            if (DirectionUp(line.Y2))
                return GetAngleUpDirection(line);
            else
                return GetAngleDownDirection(line);
        }

        private double GetAngleDownDirection(Line line)
        {
            if (DirectionLeft(line.X2))
                return GetAngleDownLeftDirection(line);
            else
                return GetAngleDownRightDirection(line);
        }

        private double GetAngleUpDirection(Line line)
        {
            if (DirectionLeft(line.X2))
                return GetAngleUpLeftDirection(line);
            else
                return GetAngleUpRightDirection(line);
        }

        private double GetAngleUpLeftDirection(Line line)
        {
            double sine = line.Y2 / GetHypotenuse(line.X2, line.Y2);
            double angleRad = Math.Sign(sine) * Math.Abs(Math.Acos(sine));
            double angleDegree = angleRad * 180 / Math.PI;
            return angleDegree;
        }

        private double GetAngleUpRightDirection(Line line)
        {
            double sine = line.Y2 / GetHypotenuse(line.X2, line.Y2);
            double angleRad = Math.Acos(sine);
            double angleDegree = angleRad * 180 / Math.PI;
            return angleDegree;
        }

        private double GetAngleDownLeftDirection(Line line)
        {
            double sine = line.X2 / GetHypotenuse(line.X2, line.Y2);
            double angleRad = Math.Asin(sine);
            double angleDegree = angleRad * 180 / Math.PI;
            return angleDegree;
        }

        private double GetAngleDownRightDirection(Line line)
        {
            double sine = line.X2 / GetHypotenuse(line.X2, line.Y2);
            double angleRad = Math.Asin(sine);
            double angleDegree = angleRad * 180 / Math.PI;
            return angleDegree;
        }

        private static double GetHypotenuse(double cathetus1,double cathetus2)
        {
            return Math.Sqrt(Math.Pow(cathetus1, 2) + Math.Pow(cathetus2, 2));
        }

        private bool DirectionUp(double cathetusY)
        {
            if(cathetusY >= 0)
                return false;
            else
                return true;
        }

        private bool DirectionLeft(double cathetusX)
        {
            if (cathetusX >= 0)
                return false;
            else
                return true;
        }

        private void RotateTriangle(ref Polygon poly, double angle)
        {
            Point p0 = poly.Points.ElementAt(1);
            Point p1 = poly.Points.ElementAt(2);
            Point p2 = poly.Points.ElementAt(3);
            RotatePoints(ref p0,ref p1,ref p2,angle);
            poly.Points = new PointCollection() {  new Point(0,0),p0, p1, p2 ,p0 };
        }

        private void RotatePoints(ref Point p0, ref Point p1, ref Point p2,double angle)
        {
            Matrix MainMatrix = new Matrix() { M11 = p1.X, M12 = p1.Y, M21 = p2.X, M22 = p2.Y };
            Matrix SubMatrix = new Matrix() { M11 = p0.X, M12 = p0.Y, M21 = p0.X, M22 = p0.Y };
            RotateMatrixes(ref MainMatrix, ref SubMatrix, angle);
            p0 = new Point(SubMatrix.M11, SubMatrix.M12);
            p1 = new Point(MainMatrix.M11, MainMatrix.M12);
            p2 = new Point(MainMatrix.M21, MainMatrix.M22);
        }

        private void RotateMatrixes(ref Matrix m1, ref Matrix m2,double angle)
        {
            m1.Rotate(angle);
            m2.Rotate(angle);
        }
        private UIElement GetTrackCircle(Map map, Point coordinates, SolidColorBrush color)
        {
            Ellipse ellipse = new Ellipse()
            {
                Width = 20,
                Height = 20,
                Fill = color
            };
            SetIconPosition(map, coordinates, ellipse);
            return ellipse;
        }

        private void SetIconOnMap(Map map,Designation designation)
        {
            designation.iconValues.uIElement = GetPictureFromName(designation.iconValues.Type);
            SetIconPosition(map, designation.iconValues.Coordinates, designation.iconValues.uIElement);
            if(!Icons.ContainsKey(designation.Name))
                Icons.Add(designation.Name, designation.iconValues);
        }

        private void SetIconPosition(Map map, Point coordinates, UIElement icon)
        {
            Point pixels = GetPixelPositionOnParent(map,
                        new Size(icon.DesiredSize.Width, icon.DesiredSize.Height), coordinates);
            PlaceImageOnMap(icon, pixels);
        }

        private PointCollection GetTrianglePoints()
        {
            PointCollection points = new PointCollection();
            Point[] initPoints = InitialTrianglePoints();
            points.Add(new Point(0, 0));
            points.Add(initPoints[2]);
            points.Add(initPoints[0]);
            points.Add(initPoints[1]);
            points.Add(initPoints[0]);
            return points;
        }

        private Point[] InitialTrianglePoints()
        {
            Point[] points = new Point[3] 
            { 
                new Point(6, -2),
                new Point(-6, -2),
                new Point(0, 10)
            };
            return points;
        }

        private void SetTrianglePosition(Map map, Point coordinates, UIElement triangle)
        {
            Point pixels = GetPixelPositionOnParent(map,
                        new Size(0, 0), coordinates);
            PlaceImageOnMap(triangle, pixels);
        }

        private void SetLinePosition(Map map, TrackLine coordinates, UIElement icon)
        {
            Point pixels = GetPixelPositionOnParent(map,
                        new Size(0, 0), coordinates.startCoordinates);
            Point Delta = GetDifferenceInPixelsFrom(map,coordinates);
            ((Line)icon).X2 = Delta.X;
            ((Line)icon).Y2 = Delta.Y;
            PlaceImageOnMap(icon, pixels);
        }

        private Point GetDifferenceInPixelsFrom(Map map,TrackLine degrees)
        {
            TrackLine pixels = GetPixelTrackLineOnParent(map, new Size(1, 1), degrees);
            return new Point(pixels.endCoordinates.X - pixels.startCoordinates.X,
                  pixels.startCoordinates.Y - pixels.endCoordinates.Y);
        }

        private void AddElementToParentChild(Map map, UIElement Element)
        {
            ((Panel)map.Parent).Children.Add(Element);
        }

        private void AddElementsToParentChild(Map map, List<UIElement> Elements)
        {
            foreach(var element in Elements)
            {
                ((Panel)map.Parent).Children.Add(element);
            }
        }

        private void DeleteAllIconsChild(Map map)
        {
            int ChildCount = ((Panel)map.Parent).Children.Count;
            if (ChildCount > 11)
                ((Panel)map.Parent).Children.RemoveRange(12, ChildCount - 12);
        }

        private TrackLine GetPixelTrackLineOnParent(Map map, Size pictureSize, TrackLine trackLine)
        {
            return new TrackLine()
            {
                startCoordinates = GetPixelPositionOnParent(map, pictureSize, trackLine.startCoordinates),
                endCoordinates = GetPixelPositionOnParent(map, pictureSize, trackLine.endCoordinates),
            };
        }

        private Point GetPixelPositionOnParent(Map map, Size pictureSize, Point coordinates)
        {
            Point pixels = coordinates.ToPixels(map.GetZoom());
            GetPixelRelevateParent(map, pictureSize,ref pixels);
            return pixels;
        }

        private void GetPixelRelevateParent(Map map, Size pictureSize, ref Point pixels)
        {
            Point CentrePixels = map.GetMapCentreInDegrees().ToPixels(map.GetZoom());
            pixels.Offset(-CentrePixels.X, -CentrePixels.Y);
            pixels.Offset(map.GetPixelParentCentre().X, -map.GetPixelParentCentre().Y);
            pixels.Offset(-pictureSize.Width / 2, pictureSize.Height / 2);
        }

        private void PlaceImageOnMap(UIElement element ,Point pixels)
        {
            Canvas.SetTop(element, -pixels.Y);
            Canvas.SetLeft(element, pixels.X);
        }

        private Image GetPictureFromName(string pictureType)
        {
            string path = @"Icons\" + pictureType + ".png";
            string debugfullpath = System.IO.Path.GetFullPath(path);
            BitmapImage image = new BitmapImage(new Uri(debugfullpath, UriKind.Absolute));
            return new Image() { Source = image,Width = 50,Height = 50};
        }
    }
}
