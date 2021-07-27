using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Image = System.Windows.Controls.Image;
using System.Windows.Media.Animation;

namespace TileMap
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int zoom;
        double x_shape, y_shape, x_canvas, y_canvas;
        double deltaX = 0;
        double deltaY = 0;
        Tiles tiles;
        UIElement source = null;
        bool captured = false;

        public struct Bound
        {
            public double Top, Left, Right, Bottom;
            public Bound(double top, double left, double right, double bottom)
            {
                Top = top;
                Left = left;
                Right = right;
                Bottom = bottom;
            }
        }
        Bound bound;

        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += Main_SizeChanged;
            MainCanvas.SizeChanged += MainCanvas_SizeChanged;
            DynamicGrid.MouseLeftButtonDown += DynamicGrid_MouseLeftButtonDown;
            DynamicGrid.MouseMove += DynamicGrid_MouseMove;
            DynamicGrid.MouseLeftButtonUp += DynamicGrid_MouseLeftButtonUp;
            DynamicGrid.MouseEnter += DynamicGrid_MouseEnter;
            DynamicGrid.MouseLeave += DynamicGrid_MouseLeave;
        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainCanvas.Height = Main.ActualHeight - 512;
            MainCanvas.Width = Main.ActualWidth - 512;
            MainCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            MainCanvas.VerticalAlignment = VerticalAlignment.Center;
            LoadGrid();
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawMap();
        }

        private void DynamicGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(MainCanvas).X;
            y_shape = Canvas.GetTop(source);
            y_canvas = e.GetPosition(MainCanvas).Y;
        }

        private void DynamicGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (captured)
            {
                double x = e.GetPosition(MainCanvas).X;
                double y = e.GetPosition(MainCanvas).Y;
                x_shape += x - x_canvas;
                y_shape += y - y_canvas;
                Canvas.SetLeft(source, x_shape);
                Canvas.SetTop(source, y_shape);
                x_canvas = x;
                y_canvas = y;
                //For debug.............................
                Label_REL_X.Content = x_shape;
                Label_REL_Y.Content = y_shape;
                //..........................................
                CheckLeft(x_shape - deltaX, y_shape - deltaY);
            }
        }

        private void DynamicGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
        }

        private void DynamicGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            SetImageEvent();
        }

        private void DynamicGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            UnsetImageEvent();
        }

        private void LoadGrid()
        {
            double W = Main.ActualWidth;
            double H = Main.ActualHeight;
            int Hcount = (int)(H / 256);
            int Wcount = (int)(W / 256);
            Hcount += 4;
            Wcount += 4;
            zoom = 4;
            if (IsEven(Hcount))
            {
                Hcount += 1;
            }
            if (IsEven(Wcount))
            {
                Wcount += 1;
            }
            tiles = new Tiles((int)Wcount, (int)Hcount, DynamicGrid, zoom);
            for (int x = 0; x < Wcount; x++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(256);
                DynamicGrid.ColumnDefinitions.Add(column);
            }
            for (int y = 0; y < Hcount; y++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(256);
                DynamicGrid.RowDefinitions.Add(row);
            }
        }

        private void DrawMap()
        {
            Set_Center();
            bound = new Bound(256, -256, 256, -256);
            FirstFillGrid();
        }

        private void CheckLeft(double x, double y)
        {
            if (x > bound.Right)
            {
                OffsetRight();
            }
            if (x < bound.Left)
            {
                OffsetLeft();
            }
            if (y > bound.Top)
            {
                OffsetBottom();

            }
            if (y < bound.Bottom)
            {
                OffsetTop();
            }
        }

        private void Set_Center()
        {
            double WDg = DynamicGrid.ActualWidth;
            double HDg = DynamicGrid.ActualHeight;
            double CenterXDg = WDg / 2;
            double CenterYDg = HDg / 2;
            double WMc = MainCanvas.Width;
            double HMc = MainCanvas.Height;
            double CenterXMc = WMc / 2;
            double CenterYMc = HMc / 2;
            deltaX = CenterXMc - CenterXDg;
            deltaY = CenterYMc - CenterYDg;
            Canvas.SetLeft(DynamicGrid, deltaX);
            Canvas.SetTop(DynamicGrid, deltaY);
            //For debug.............................
            Canvas.SetLeft(Label_X, 200);
            Canvas.SetTop(Label_X, 200);
            Canvas.SetLeft(Label_Y, 400);
            Canvas.SetTop(Label_Y, 200);
            Canvas.SetLeft(Label_MAX_X, 200);
            Canvas.SetTop(Label_MAX_X, 400);
            Canvas.SetLeft(Label_MAX_Y, 400);
            Canvas.SetTop(Label_MAX_Y, 400);
            Canvas.SetLeft(Label_REL_X, 200);
            Canvas.SetTop(Label_REL_X, 600);
            Canvas.SetLeft(Label_REL_Y, 400);
            Canvas.SetTop(Label_REL_Y, 600);
        }

        private void FirstFillGrid()
        {
            int row = tiles.TileMatrix.GetLength(1);
            int column = tiles.TileMatrix.GetLength(0);
            int MainY = row / 2;
            int MainX = column / 2;
            for (int x = 0; x < column; x++)
            {
                for (int y = 0; y < row; y++)
                {
                    tiles.SetImage(8 - MainX + x, 5 - MainY + y, zoom, x, y);
                }
            }
        }

        private void OffsetRight()
        {
            UnsetImageEvent();
            var _index = DynamicGrid.ColumnDefinitions.IndexOf(DynamicGrid.ColumnDefinitions.Last());
            tiles.DeleteColumn(_index);
            var newcolumn = new ColumnDefinition();
            newcolumn.Width = new GridLength(256);
            tiles.OffsetMatrixColumnRight(newcolumn);
            x_shape -= 256;
            SetImageEvent();
        }

        private void OffsetBottom()
        {
            UnsetImageEvent();

            var _index = DynamicGrid.RowDefinitions.IndexOf(DynamicGrid.RowDefinitions.Last());
            tiles.DeleteRow(_index);
            var newrow = new RowDefinition();
            newrow.Height = new GridLength(256);
            tiles.OffsetMatrixColumnBottom(newrow);
            y_shape -= 256;
            SetImageEvent();
        }

        private void OffsetLeft()
        {
            UnsetImageEvent();

            var _index = DynamicGrid.ColumnDefinitions.IndexOf(DynamicGrid.ColumnDefinitions.First());
            tiles.DeleteColumn(_index);
            var newcolumn = new ColumnDefinition();
            newcolumn.Width = new GridLength(256);
            tiles.OffsetMatrixColumnLeft(newcolumn);
            x_shape += 256;
            SetImageEvent();
        }

        private void OffsetTop()
        {
            UnsetImageEvent();

            var _index = DynamicGrid.RowDefinitions.IndexOf(DynamicGrid.RowDefinitions.First());
            tiles.DeleteRow(_index);
            var newrow = new RowDefinition();
            newrow.Height = new GridLength(256);
            tiles.OffsetMatrixColumnTop(newrow);
            y_shape += 256;
            SetImageEvent();
        }

        private void SetImageEvent()
        {
            foreach (var im in DynamicGrid.Children)
            {
                if (im != null)
                {
                    Image image = (Image)im;
                    image.MouseMove += Image_MouseMove;
                    image.MouseWheel += Image_MouseWheel;
                }
            }
        }

        private void UnsetImageEvent()
        {
            foreach (var im in DynamicGrid.Children)
            {
                if (im != null)
                {
                    Image image = (Image)im;
                    image.MouseMove -= Image_MouseMove;
                    image.MouseWheel -= Image_MouseWheel;
                }
            }
        }

        private bool IsEven(int a)
        {
            if (a % 2 == 0) { return true; }
            else { return false; }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            Point offset = e.GetPosition(image);
            Tile tile = tiles.GetTile(image);
            double RelevateCoord_X = offset.X / 256;
            double RelevateCoord_Y = offset.Y / 256;
            Point p = TileToWorldPos(tile.x + RelevateCoord_X, tile.y + RelevateCoord_Y, tile.z);
            //For debug.....................................
            Label_X.Content = p.X;
            Label_Y.Content = p.Y;
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            DynamicGrid.IsEnabled = false;
            Image image = (Image)sender;
            zoom += e.Delta / 120;
            if (zoom > 19)
            {
                zoom = 19;
                return;
            }
            else if (zoom < 0)
            {
                zoom = 0;
                return;
            }
            Point offset = e.GetPosition(image);
            Tile tile = tiles.GetTile(image);
            Point p = CalcZoom(offset,tile);
            if(e.Delta>0)
            {
                ZoomedInFillFrid(p);
                OffsetMap(p,offset);
            }
            else
            {
                ZoomedOutFillFrid(p);
                OffsetMap(p, offset);
            }
            DynamicGrid.IsEnabled = true;
        }

        public Point WorldToTilePos(double lon, double lat, int zoom)
        {
            Point p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
               1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        public Point TileToWorldPos(double tile_x, double tile_y, int zoom)
        {
            Point p = new Point();
            double n = Math.Pow(2.0, zoom);
            p.X = tile_x * 360.0 / n - 180.0;
            p.Y = Math.Atan(Math.Sinh(Math.PI - tile_y * 2.0 * Math.PI / n)) * 180.0 / Math.PI;
            return p;
        }

        private Point CalcZoom(Point offset, Tile tile)
        {
            DynamicGrid.IsEnabled = false;
            DynamicGrid.Children.Clear();
            double MaxDegree_X = 360 / (1 << (zoom));
            double MaxDegree_Y = 170.1022 / (1 << (zoom));
            double RelevateCoord_X = offset.X / 256;
            double RelevateCoord_Y = offset.Y / 256;
            Point ZoomPoint = TileToWorldPos(tile.x + RelevateCoord_X, tile.y + RelevateCoord_Y, tile.z);
            offset.X = MaxDegree_X * RelevateCoord_X;
            offset.Y = MaxDegree_Y * RelevateCoord_Y;
            Label_MAX_X.Content = offset.X;
            Label_MAX_Y.Content = offset.Y;
            Label_X.Content = ZoomPoint.X;
            Label_Y.Content = ZoomPoint.Y;
            return ZoomPoint;
        }

        private void ZoomedInFillFrid(Point ToZoom)
        {
            UnsetImageEvent();
            int Wcount = tiles.TileMatrix.GetLength(0);
            int Hcount = tiles.TileMatrix.GetLength(1);
            Point ZoomedT = WorldToTilePos(ToZoom.X, ToZoom.Y, zoom - 1);
            Tile PreviousTile = new Tile() { x = (int)ZoomedT.X, y = (int)ZoomedT.Y, z = zoom - 1 };

            Point indexces = tiles.GetIndexces(PreviousTile);

            tiles = new Tiles((int)Wcount, (int)Hcount, DynamicGrid, zoom);
            for (int x = 0; x < Wcount; x++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(256);
                DynamicGrid.ColumnDefinitions.Add(column);
            }
            for (int y = 0; y < Hcount; y++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(256);
                DynamicGrid.RowDefinitions.Add(row);
            }
            Point NewTilePoint = WorldToTilePos(ToZoom.X, ToZoom.Y, zoom);
            Tile NewTile = new Tile() { x = (long)(NewTilePoint.X), y = (long)(NewTilePoint.Y), z = zoom };
            tiles.SetImage(NewTile.x, NewTile.y, zoom, (int)indexces.X, (int)indexces.Y);

            for (int x = 0; x < Wcount; x++)
            {
                for (int y = 0; y < Hcount; y++)
                {
                    tiles.SetImage(NewTile.x - (int)indexces.X + x, NewTile.y - (int)indexces.Y + y, zoom, x, y);
                }
            }
            SetImageEvent();
        }

        private void ZoomedOutFillFrid(Point ToZoom)
        {
            UnsetImageEvent();
            int Wcount = tiles.TileMatrix.GetLength(0);
            int Hcount = tiles.TileMatrix.GetLength(1);
            Point ZoomedT = WorldToTilePos(ToZoom.X, ToZoom.Y, zoom + 1);
            Tile PreviousTile = new Tile() { x = (int)ZoomedT.X, y = (int)ZoomedT.Y, z = zoom + 1 };

            Point indexces = tiles.GetIndexces(PreviousTile);

            tiles = new Tiles((int)Wcount, (int)Hcount, DynamicGrid, zoom);
            for (int x = 0; x < Wcount; x++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(256);
                DynamicGrid.ColumnDefinitions.Add(column);
            }
            for (int y = 0; y < Hcount; y++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(256);
                DynamicGrid.RowDefinitions.Add(row);
            }
            Point NewTile = WorldToTilePos(ToZoom.X, ToZoom.Y, zoom);
            tiles.SetImage((int)NewTile.X, (int)NewTile.Y, zoom, (int)indexces.X, (int)indexces.Y);
            for (int x = 0; x < Wcount; x++)
            {
                for (int y = 0; y < Hcount; y++)
                {
                    tiles.SetImage((int)NewTile.X - (int)indexces.X + x, (int)NewTile.Y - (int)indexces.Y + y, zoom, x, y);
                }
            }
            SetImageEvent();
        }

        private void OffsetMap(Point ScaledPoint,Point cursor)
        {
            Point p = new Point();
            double k = 1 / Math.Cos(Math.PI * ScaledPoint.Y / 180);
            Point zoomedTile = WorldToTilePos(ScaledPoint.X,ScaledPoint.Y , zoom);
            Point MainPoint = TileToWorldPos((int)zoomedTile.X, (int)zoomedTile.Y,zoom);
            p.X = ScaledPoint.X - MainPoint.X;
            p.Y = ScaledPoint.Y - MainPoint.Y;
            Point MaxDegree = new Point() { X = 360/ Math.Pow(2, zoom) , Y = 170.1022 /Math.Pow(2, zoom) };
            Point pixel = new Point() { X = 256 * p.X / MaxDegree.X, Y= 256*(p.Y / (MaxDegree.Y * k)) };
            double t = Canvas.GetTop(DynamicGrid);
            double x = Canvas.GetLeft(DynamicGrid) - pixel.X + cursor.X;
            double y = Canvas.GetTop(DynamicGrid) + pixel.Y + cursor.Y;
            Canvas.SetLeft(DynamicGrid, x);
            Canvas.SetTop(DynamicGrid, y );
        }
    }
}
