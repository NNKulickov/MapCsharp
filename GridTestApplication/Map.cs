using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MapProject.Utilities;

namespace MapProject
{
    public class Map : Grid
    {
        private string Source;
        private int Zoom = 0;
       
        private Point MapCentreInDegrees;
        private Point MapPositionOnParent;
        private Label DebugLatitudeLabel;
        private Label DebugLongitudeLabel;
        private Label DebugZoomLabel;
        private Label DebugDegreesCentreXLabel;
        private Label DebugDegreesCentreYLabel;
        private Point MapPixelCentre;
        private Vector TotalOffset = new Vector();
        private Vector StartPixelParentPosition;
        private bool MapCaptured;
        private bool UpDateMapFlag = false;

        public MapIcons mapIcons = new MapIcons();
        public static readonly double MercatorLatitudeLimit = 85.0511;
        public void SetSource(string source) { Source = source; }
        public void SetZoom(int zoom) { Zoom = zoom; }
        public string GetSource() { return Source; }
        public int GetZoom() { return Zoom; }
        public Point GetMapCentreInDegrees() { return MapCentreInDegrees; }
        public Point GetPixelMapCentre() { return MapPixelCentre; }
        public Point GetPixelParentCentre() { return new Point(((Panel)Parent).ActualWidth / 2, ((Panel)Parent).ActualHeight / 2); }
        public Point GetMapPositionOnParent() { return MapPositionOnParent; }
        private double GetTileLongitudeDegreesResolution() { return 180.0 / Math.Pow(2, Zoom); }
        private void SetClip() { ((Panel)Parent).ClipToBounds = true; }

        public Map(int zoom)
        {
            SetZoom(zoom);
            Width = 256;
            Height = 256;
        }

        public void ConfigureMap()
        {
            SubscribeToMouseEvents();
            SubscribeToParentSizeChangedEvent();
            SetClip();
            SubscribeToKeyDownEvent();
            Dispatcher.Invoke(new Action(() => mapIcons.InitialElementsOn(this)));
            //Debug

            SetupDebugOutput();
        }

        private void SetMapPositionOnParent(Point position)
        {
            MapPositionOnParent = position;
            PlaceMapOnParent();
        }

        private void PlaceMapOnParent()
        {
            Canvas.SetTop(this, (int)MapPositionOnParent.Y);
            Canvas.SetLeft(this, (int)MapPositionOnParent.X);
        }

        private void SubscribeToMouseEvents()
        {
            ((Panel)Parent).MouseMove += ParentMouseMove;
            ((Panel)Parent).MouseLeftButtonDown += MapMouseLeftButtonDown;
            ((Panel)Parent).MouseWheel += MapMouseWheel;
            ((Panel)Parent).MouseLeftButtonUp += MapMouseLeftButtonUp;
        }

        private void SubscribeToParentSizeChangedEvent()
        {
            ((Panel)Parent).SizeChanged += ParentSizeChanged;
        }

        private void ParentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize(e.PreviousSize, e.NewSize);
            InitializeStartPosition();
            Dispatcher.Invoke(new Action(() => { FillMapWithTiles(); }));
            Dispatcher.BeginInvoke(new Action(() => mapIcons.Update(this)));
        }
        //-
        //-
        //-
        //-
        //-
        //Mouse events handler description 
        private void MapMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point DesiredDegrees = GetLimitedMousePixelCoordinates().ToWorldPosition(this);
            UpdateZoom(e.Delta);
            SetMapCentreWithCursorPosition(DesiredDegrees);
            InitializeStartPosition();
            Dispatcher.BeginInvoke(new Action(() => mapIcons.Update(this)));
            FillMapWithTiles();
        }

        private void MapMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            MapCaptured = false;
        }

        private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            MapCaptured = true;
            StartPixelParentPosition = (Vector)e.GetPosition((Panel)Parent);
        }

        private void ParentMouseMove(object sender, MouseEventArgs e)
        {
            if (MapCaptured)
                DragHandler(e.GetPosition((Panel)Parent));
            Point MouseDegrees = GetLimitedMousePixelCoordinates().ToWorldPosition(this);
            LimitDegrees(ref MouseDegrees);
            ShowDebugOutput(MouseDegrees);     
        }
        //-
        //-
        //-
        //-
        //-
        private void InitializeStartPosition()
        {
            ResetOffsetVariables();
            UpdateHeight();
            SetCenterMap();
            SetView(MapCentreInDegrees.Y, MapCentreInDegrees.X, Zoom);
        }

        public void SetView(double desiredLatitude = 0, double desiredLongitude = 0, int zoom = 0)
        {
            UpdateMapCentreInDegrees(new Point(desiredLongitude, desiredLatitude));
            SetZoom(zoom);
            UpdateReferenceSystem();
            this.AutosizeColumnsAndRows();
        }

        private void UpdateReferenceSystem()
        {
            Point DegreesDistanceToCentre = GetTopLeftTileCoordinate().ToWorldPosition(this);
            if ((!Double.IsNaN(DegreesDistanceToCentre.X)) || (!Double.IsNaN(DegreesDistanceToCentre.Y)))
            {
                Vector PixelDistance = TransformDegrees(DegreesDistanceToCentre).ToPixels(Zoom);
                InitialPlaceMap(PixelDistance);
            }
        }

        private void InitialPlaceMap(Vector PixelDistance)
        {
            HorizontalReturn(PixelDistance.X);
            VerticalReturn(-PixelDistance.Y);
            UpdateView(PixelDistance.X, -PixelDistance.Y);
            PlaceMapOnParent();
        }

        private void ResetOffsetVariables()
        {
            TotalOffset = new Vector(0, 0);
            MapPositionOnParent = new Point(0, 0);
        }

        private void DragHandler(Point MousePosition)
        {
            Vector OffsetCoordinates = new Vector(MousePosition.X - StartPixelParentPosition.X, MousePosition.Y - StartPixelParentPosition.Y);
            Dispatcher.BeginInvoke(new Action(() =>ExecuteVerticalDrag(OffsetCoordinates.Y)));
            Dispatcher.BeginInvoke(new Action(() => ExecuteHorizontalDrag(OffsetCoordinates.X)));
            Dispatcher.BeginInvoke(new Action(() => mapIcons.Update(this)));
            StartPixelParentPosition = (Vector)MousePosition;
        }

        private void ExecuteVerticalDrag(double _yPixel)
        {
            if (_yPixel > 0)
            {
                for (int i = 1; i <= Math.Floor(_yPixel); i++)
                {
                    MoveMapOnePixelUp();
                }
            }
            else
            {
                for (int i = 1; i <= -Math.Floor(_yPixel); i++)
                {
                    MoveMapOnePixelDown();
                }
            }
        }

        private void ExecuteHorizontalDrag(double _xPixel)
        {
            if (_xPixel < 0)
            {
                for (int i = 1; i <= -Math.Floor(_xPixel); i++)
                {
                    MoveMapOnePixelRight();
                }
            }
            else
            {
                for (int i = 1; i <= Math.Floor(_xPixel); i++)
                {
                    MoveMapOnePixelLeft();
                }
            }
        }
        //-
        //-
        //-
        //-
        //-
        //-
        //Map Size changing
        private void UpdateSize(Size PreviousParentSize, Size NewParentSize)
        {
            if (PreviousParentSize.Width * PreviousParentSize.Height != 0) ChangeSize(PreviousParentSize, NewParentSize);
            else InitialSize();
            this.AutosizeColumnsAndRows();
        }
        private void ChangeSize(in Size PreviousParentSize, in Size NewParentSize)
        {
            double WidthCoefficient = NewParentSize.Width / PreviousParentSize.Width;
            double HeightCoefficient = NewParentSize.Height / PreviousParentSize.Height;
            ScaleMap(WidthCoefficient, HeightCoefficient);
        }
        private void InitialSize()
        {
            Width = ((Panel)Parent).ActualWidth + 1536;
            double DesiredHeight = 256 * Math.Pow(2, Zoom);
            ValidateAndSetHeight(DesiredHeight);
        }
        private void ScaleMap(double WidthCoefficient, double HeightCoefficient)
        {
            Width *= WidthCoefficient;
            ValidateAndSetHeight(Height * HeightCoefficient);
        }
        private void ValidateAndSetHeight(double NewHeight)
        {
            if (NewHeight > ((Panel)Parent).ActualHeight + 1280) Height = ((int)((((Panel)Parent).ActualHeight + 1280) /256))*256;
            else if (NewHeight < 256) Height = 256;
            else Height = ((int)(NewHeight/256))*256;
        }
        //-
        //-
        //-
        //-
        //-
        //Map centering description
        private void SetCenterMap()
        {
            Point PixelOffsetMapCentreRelativeParent = new Point()
            {
                X = (((Panel)Parent).ActualWidth - Width) / 2,
                Y = (((Panel)Parent).ActualHeight - Height) / 2
            };
            SetMapPositionOnParent(PixelOffsetMapCentreRelativeParent);
            MapPixelCentre = new Point(Width / 2, Height / 2);
        }
        //-
        //-
        //-
        //-
        //-
        //Update Referense System description
        private Vector TransformDegrees(Point DegreesDistance)
        {
            Vector DegreesDistanceConversion =
                new Vector()
                {
                    X = GetTranformedLongitude(DegreesDistance.X, GetTileLongitudeDegreesResolution()),
                    Y = DegreesDistance.Y
                };
            if (Zoom == 0)
                DegreesDistanceConversion.Y = 0;
            return DegreesDistanceConversion;
        }
        private double GetTranformedLongitude(double _xCoordinate, double _xResolution)
        {
            if (_xCoordinate > 0) return GetPositiveTransformationLongitude(_xCoordinate, _xResolution);
            else return GetNegativeTransformationLongitude(_xCoordinate);
        }
        private void UpdateMapCentreInDegrees(Point DesiredPoint)
        {
            LimitDegrees(ref DesiredPoint);
            MapCentreInDegrees = DesiredPoint;
        }
        private static void LimitDegrees(ref Point point)
        {
            if (point.X > 180) point.X = 180;
            else if (point.X < -180) point.X = -180;
            if (point.Y > MercatorLatitudeLimit) point.Y = MercatorLatitudeLimit;
            else if (point.Y < -MercatorLatitudeLimit) point.Y = -MercatorLatitudeLimit;
        }
        private Point GetTopLeftTileCoordinate()
        {
            Point MapPos = MapPositionOnParent;
            double MapYTop = ((int)(MapPixelCentre.Y / 256)) * 256;
            double YCentre = MapPos.Y + MapYTop;
            return new Point((int)(MapPixelCentre.X / 256) * 256, YCentre);
        }
        //-
        //-
        //-
        //-
        //-
        //Key Down Event Handler description
        private void SubscribeToKeyDownEvent()
        {
            Window.GetWindow(this).KeyDown += MapKeyDown;
        }
        private void MapKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                MoveMapOnePixelLeft();
            }
            else if (e.Key == Key.Right)
            {
                MoveMapOnePixelRight();
            }
            else if (e.Key == Key.Up)
            {
                MoveMapOnePixelUp();
            }
            else if (e.Key == Key.Down)
            {
                MoveMapOnePixelDown();
            }
        }
        //-
        //-
        //-
        //-
        //-
        //Map's offset description
        private readonly object balanceLock = new object();
        private void MoveMapOnePixelDown()
        {
            ShiftPosition(0, -1);
            ShiftReferenceSystem(0, 1);
            if (UpDateMapFlag)
            {
                this.AddTilesOnRow(false);
                UpDateMapFlag = false;
            }
            ShowDebugOutput(GetLimitedMousePixelCoordinates().ToWorldPosition(this));
        }
        private void MoveMapOnePixelUp()
        {
            ShiftPosition(0, 1);
            ShiftReferenceSystem(0, -1);
            if (UpDateMapFlag)
            {
                this.AddTilesOnRow(true);
                UpDateMapFlag = false;
            }
            ShowDebugOutput(GetLimitedMousePixelCoordinates().ToWorldPosition(this));
        }
        delegate Point GetNewCentre(ref Point DegreesCentre);
        private void MoveMapOnePixelLeft()
        {
            ShiftPosition(1, 0);
            ShiftReferenceSystem(-1, 0);
            if (UpDateMapFlag)
            {
                this.AddTilesOnColumn(true);
                UpDateMapFlag = false;
            }
            ShowDebugOutput(GetLimitedMousePixelCoordinates().ToWorldPosition(this));
        }
        private void MoveMapOnePixelRight()
        {
            ShiftPosition(-1, 0);
            ShiftReferenceSystem(1, 0);
            if (UpDateMapFlag)
            {
                this.AddTilesOnColumn(false);
                UpDateMapFlag = false;
            }
            ShowDebugOutput(GetLimitedMousePixelCoordinates().ToWorldPosition(this));
        }

        private void ShiftReferenceSystem(double dx, double dy)
        {
            double invertDy = -dy;
            CheckVerticalBorder(ref invertDy);
            Point OffsetPoint = GetPixelParentCentre();
            OffsetPoint.Offset(dx, -invertDy);
            MapCentreInDegrees = new Point(OffsetPoint.ToWorldPosition(this).X, 
                OffsetPoint.ToWorldPosition(this).Y);
        }

        private void ShiftPosition(double dx, double dy)
        {
            ValidateAndUpdateOffset(dx, dy);
            PlaceMapOnParent();
        }

        private void ValidateAndUpdateOffset(double dx, double dy)
        {
            Vector PreviousOffset = TotalOffset;
            UpdateTotalOffsetAndCheckBorder(dx, ref dy);
            UpdatePosition(dx, dy);
            ReturnMapOnInitial(PreviousOffset);
        }

        private void ReturnMapOnInitial(Vector PreviousOffset)
        {
            UpdateValidatedHorizontalOffset(PreviousOffset);
            UpdateValidatedVerticalOffset(PreviousOffset);
        }

        private void UpdateValidatedHorizontalOffset(Vector PreviousOffset)
        {
            if (TotalOffset.X - PreviousOffset.X >= 256)
            {
                UpDateMapFlag = true;
                UpdateView(-256, 0);
            }
            else if (TotalOffset.X - PreviousOffset.X <= -256)
            {
                UpDateMapFlag = true;
                UpdateView(256, 0);
            }
        }

        private void UpdateValidatedVerticalOffset(Vector PreviousOffset)
        {
            if (Math.Abs(TotalOffset.Y - PreviousOffset.Y) >= 256)
            {
                UpDateMapFlag = true;
                UpdateView(0, -Math.Sign(TotalOffset.Y - PreviousOffset.Y) * 256);
            }
        }

        private void UpdateTotalOffsetAndCheckBorder(in double dx, ref double dy)
        {
            CheckVerticalBorder(ref dy);
            if(!ValidateMapAndParentSizes()) TotalOffset += new Vector(dx, dy);
            else TotalOffset += new Vector(dx, 0);
        }

        private bool ValidateMapAndParentSizes()
        {
            if ((Math.Abs(Height/2 - ((Panel)Parent).ActualHeight / 2) < 45)&&(Zoom < 2)) return true;
             return false;
        }

        private void CheckVerticalBorder(ref double y)
        {
            if (CheckTopVerticalBorder() && y > 0) y = 0;
            else if (CheckBottomVerticalBorder() && y < 0) y = 0;
        }

        private bool CheckTopVerticalBorder()
        {
            Point TopParentCetnrePixelCoordinate = new Point(GetPixelParentCentre().X, 0);
            double max = TopParentCetnrePixelCoordinate.ToWorldPosition(this).Y;
            if (max > MercatorLatitudeLimit) return true;
            else return false;
        }

        private bool CheckBottomVerticalBorder()
        {
            Point TopParentCetnrePixelCoordinate = new Point(GetPixelParentCentre().X, ((Panel)Parent).ActualHeight);
            double max = TopParentCetnrePixelCoordinate.ToWorldPosition(this).Y;
            if (max < -MercatorLatitudeLimit) return true;
            else return false;
        }

        private void HorizontalReturn(double _xValue)
        {
            if (Math.Abs(_xValue) >= 256)
            {
                int SkipedTileCount = (int)Math.Abs(Math.Floor(_xValue / 256));
                UpdateView(-Math.Sign(_xValue) * 256 * (SkipedTileCount+1), 0);
            }
            TotalOffset.X = Math.Floor(Math.Sign(_xValue) *
                (Math.Abs(_xValue) - Math.Abs(256 * Math.Floor(_xValue / 256))));
        }

        private void VerticalReturn(double _yValue)
        {
            if (Math.Abs(_yValue) >= 128)
            {
                int SkipedTileCount = (int)Math.Abs(Math.Floor(_yValue / 256));
                UpdateView(0, -Math.Sign(_yValue) * 256 * (SkipedTileCount));

            }
            TotalOffset.Y = Math.Floor(-Math.Sign(_yValue) *
                (Math.Abs(_yValue) - Math.Abs(256 * Math.Floor(_yValue / 256))));
        }

        private void UpdateView(double x, double y)
        {
            UpdatePosition(-x, -y);
        }

        private void UpdatePosition(double dx, double dy)
        {
            if(Zoom > 1)
                this.UpdateGrid(ref TotalOffset);
            MapPositionOnParent.Offset(dx, dy);
        }
        //-
        //-
        //-
        //-
        //-
        //Transform function description
        static double GetPositiveTransformationLongitude(double x, double _resolution) { 
            return _resolution + (_resolution + x) % 180.0; }
        static double GetNegativeTransformationLongitude(double x) { return (180.0 + x) % 180.0; }
        //-
        //-
        //-
        //-
        //-
        //Change zoom description
        private void UpdateZoom(int MouseOffset)
        {
            if (MouseOffset > 0) Zoom++;
            else Zoom--;
            LimitZoom();
        }

        private void LimitZoom()
        {
            if (Zoom > 19) Zoom = 19;
            else if (Zoom < 0) Zoom = 0;
        }

        private void UpdateHeight()
        {
            InitialSize();
            this.AutosizeColumnsAndRows();
        }
        /// <summary>
        /// Sets the map center with the desired coordinates on cursor when the zoom has changed
        /// </summary>
        /// <param name="DesiredDegrees"></param>
        /// <param name="IsIncrease"></param>
        private void SetMapCentreWithCursorPosition(Point DesiredDegrees)
        {
            LimitDegrees(ref DesiredDegrees);
            if (Zoom == 0)
                MapCentreInDegrees = new Point(DesiredDegrees.X/2, 0);
            else
                if (Zoom + 1 != 21) SetCentreOfDesiredDegrees(DesiredDegrees);
        }

        private void SetCentreOfDesiredDegrees(Point CursorDegrees)
        {
            Vector DeltaPixels = (Vector)GetLimitedMousePixelCoordinates() - (Vector)GetPixelParentCentre();
            MapCentreInDegrees = new Point()
            {
                X = CursorDegrees.X - DeltaPixels.X * GetTileLongitudeDegreesResolution() / 128,
                Y = PointExtension.GetVerticalCoordinate(Zoom, CursorDegrees.Y, DeltaPixels.Y)
            };
            ValidateMapCentre();
        }

        private void ValidateMapCentre()
        {
            double _difference = 0;
            if (Math.Abs(MapCentreInDegrees.X) > 180)
            {
                _difference = Math.Sign(MapCentreInDegrees.X) * (Math.Abs(MapCentreInDegrees.X) - 180);
                MapCentreInDegrees.X = -Math.Sign(MapCentreInDegrees.X) * 180;
            }
            MapCentreInDegrees.X += _difference;
        }
        //-
        //-
        //-
        //-
        //-
        private Point GetLimitedMousePixelCoordinates()
        {
            Point MousePixelCoordinates = Mouse.GetPosition((Panel)Parent);
            MousePixelCoordinates.LimitTo(new Size(((Panel)Parent).ActualWidth, ((Panel)Parent).ActualHeight));
            return MousePixelCoordinates;
        }

        public void AddIcon(Point coordinates,string type,string name)
        {
            mapIcons.Add(this, coordinates, type,name);
            Dispatcher.Invoke(new Action(() => mapIcons.InitialElementsOn(this)));
        }

        public void DeleteIconOf(string name)
        {
            mapIcons.DeleteIconOf(name);
        }

        public void AddTrack(Dictionary<DateTime, Point> coordinates, string name)
        {
            Random rand = new Random();
            SolidColorBrush color = new SolidColorBrush(new Color() 
            {
                R = (byte)rand.Next(0, 128),
                ScA = 64,
                G = (byte)rand.Next(0, 128),
                B = (byte)rand.Next(0, 128)
              
            });
            Dispatcher.Invoke(
                new Action(() => mapIcons.AddTrackOn(this, coordinates, name, color)));
            Dispatcher.Invoke(new Action(() => mapIcons.InitialElementsOn(this)));
        }

        

        public void DeleteTrackOf(string name)
        {
            mapIcons.DeleteTrackOf(name);
        }

        public void FillMapWithTiles()
        {
            Children.Clear();
            Dispatcher.Invoke(new Action(() => {this.InitialFill(); }));
        }

        private List<Label> DebugAddLabels()
        {
            List<Label> labels = new List<Label>();
            Label latitude = new Label();
            Label longitude = new Label();
            Label zoomLabel = new Label();
            Label DegreesCentreXLabel = new Label();
            Label DegreesCentreYLabel = new Label();
            Label TextLatitude = new Label();
            Label TextLongitude = new Label();
            Label TextZoomLabel = new Label();
            Label TextDegreesCentreLabel = new Label();
            Label TextDegreesCentreXLabel = new Label();
            Label TextDegreesCentreYLabel = new Label();

            TextLatitude.Width = 100;
            TextLongitude.Width = 100;
            TextZoomLabel.Width = 100;
            TextDegreesCentreLabel.Width = 100;
            TextDegreesCentreYLabel.Width = 100;
            TextDegreesCentreXLabel.Width = 100;
            latitude.Width = 100;
            longitude.Width = 100;
            zoomLabel.Width = 100;
            DegreesCentreXLabel.Width = 100;
            DegreesCentreYLabel.Width = 100;
            TextLatitude.Content = "Latitude";
            TextLongitude.Content = "Longitude";
            TextZoomLabel.Content = "Zoom";
            TextDegreesCentreLabel.Content = "DegreesCentre";
            TextDegreesCentreYLabel.Content = "Y";
            TextDegreesCentreXLabel.Content = "X";

            Canvas.SetLeft(latitude, 50);
            Canvas.SetTop(latitude, 50);
            Canvas.SetLeft(longitude, 150);
            Canvas.SetTop(longitude, 50);
            Canvas.SetLeft(zoomLabel, 250);
            Canvas.SetTop(zoomLabel, 50);
            Canvas.SetLeft(DegreesCentreXLabel, 350);
            Canvas.SetTop(DegreesCentreXLabel, 50);
            Canvas.SetLeft(DegreesCentreYLabel, 450);
            Canvas.SetTop(DegreesCentreYLabel, 50);
            Canvas.SetLeft(TextLatitude, 50);
            Canvas.SetTop(TextLatitude, 10);
            Canvas.SetLeft(TextLongitude, 150);
            Canvas.SetTop(TextLongitude, 10);
            Canvas.SetLeft(TextDegreesCentreLabel, 350);
            Canvas.SetTop(TextDegreesCentreLabel, 10);
            Canvas.SetLeft(TextDegreesCentreXLabel, 350);
            Canvas.SetTop(TextDegreesCentreXLabel, 25);
            Canvas.SetLeft(TextDegreesCentreYLabel, 450);
            Canvas.SetTop(TextDegreesCentreYLabel, 25);
            Canvas.SetLeft(TextZoomLabel, 250);
            Canvas.SetTop(TextZoomLabel, 10);




            ((Panel)Parent).Children.Add(latitude);
            ((Panel)Parent).Children.Add(longitude);
            ((Panel)Parent).Children.Add(zoomLabel);
            ((Panel)Parent).Children.Add(DegreesCentreXLabel);
            ((Panel)Parent).Children.Add(DegreesCentreYLabel);
            ((Panel)Parent).Children.Add(TextDegreesCentreLabel);
            ((Panel)Parent).Children.Add(TextDegreesCentreXLabel);
            ((Panel)Parent).Children.Add(TextDegreesCentreYLabel);
            ((Panel)Parent).Children.Add(TextLatitude);
            ((Panel)Parent).Children.Add(TextLongitude);
            ((Panel)Parent).Children.Add(TextZoomLabel);
            labels.Add(latitude);
            labels.Add(longitude);
            labels.Add(zoomLabel);
            labels.Add(DegreesCentreXLabel);
            labels.Add(DegreesCentreYLabel);
            return labels;
        }

        private void SetupDebugOutput()
        {
            List<Label> DebugLabels = DebugAddLabels();
            DebugLatitudeLabel = DebugLabels[0];
            DebugLongitudeLabel = DebugLabels[1];
            DebugZoomLabel = DebugLabels[2];
            DebugDegreesCentreXLabel = DebugLabels[3];
            DebugDegreesCentreYLabel = DebugLabels[4];
        }

        private void ShowDebugOutput(Point WorldCoordinates)
        {
            DebugLatitudeLabel.Content = WorldCoordinates.Y;
            DebugLongitudeLabel.Content = WorldCoordinates.X;
            DebugZoomLabel.Content = Zoom;
            DebugDegreesCentreXLabel.Content = MapCentreInDegrees.X;
            DebugDegreesCentreYLabel.Content = MapCentreInDegrees.Y;
        }
    }
}
