using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace DynamicGridTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double x_shape, y_shape, x_canvas, y_canvas;
        Tiles tiles;
        double deltaX;
        double deltaY;
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
           // DynamicGrid.MouseMove += DynamicGrid_MouseMove;
           // DynamicGrid.MouseLeftButtonDown += DynamicGrid_MouseLeftButtonDown;
           // DynamicGrid.MouseLeftButtonUp += DynamicGrid_MouseLeftButtonUp;
            MainCanvas.Loaded += MainCanvas_Loaded;
        }

        private void MainCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            double H = canvas.ActualHeight;
            double W = canvas.ActualWidth;
            bound = new Bound(256, - 256, 256, -256);
            Set_Center();
            FirstFillGrid();
        }

        private void DynamicGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
        }

        UIElement source = null;
        bool captured = false;
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
                CheckLeft(x_shape  - deltaX, y_shape - deltaY);
            }
        }
        private void Set_Center()
        {
            double WDg = DynamicGrid.ActualWidth;
            double HDg = DynamicGrid.ActualHeight;
            double CenterXDg = HDg / 2;
            double CenterYDg = WDg / 2;
            tiles = new Tiles((int)WDg / 256, (int)HDg / 256,DynamicGrid);
            double WMc = MainCanvas.ActualWidth;
            double HMc = MainCanvas.ActualHeight;
            double CenterXMc = HMc / 2;
            double CenterYMc = WMc / 2;
             deltaX = CenterXMc - CenterXDg;
             deltaY = CenterYMc - CenterYDg;
            Canvas.SetLeft(DynamicGrid, deltaX);
            Canvas.SetTop(DynamicGrid, deltaY);
        }
        private void CheckLeft(double x, double y)
        {
            if (x > bound.Right)
            {
                OffsetRight();
            }
            if(x < bound.Left)
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
        private void OffsetRight()
        {
            var _index = DynamicGrid.ColumnDefinitions.IndexOf(DynamicGrid.ColumnDefinitions.Last());
            tiles.DeleteColumn(_index);

            var newcolumn = new ColumnDefinition();
            newcolumn.Width = new GridLength(256);
            tiles.OffsetMatrixColumnRight(newcolumn);
            x_shape -= 256;
        }
        private void OffsetBottom()
        {
            var _index = DynamicGrid.RowDefinitions.IndexOf(DynamicGrid.RowDefinitions.Last());
            tiles.DeleteRow(_index);

            var newrow = new RowDefinition();
            newrow.Height = new GridLength(256);
            tiles.OffsetMatrixColumnBottom(newrow);
            y_shape -= 256;
        }
        private void OffsetLeft()
        {
            var _index  = DynamicGrid.ColumnDefinitions.IndexOf(DynamicGrid.ColumnDefinitions.First());
            tiles.DeleteColumn(_index);
            
            var newcolumn = new ColumnDefinition();
            newcolumn.Width = new GridLength(256);
            tiles.OffsetMatrixColumnLeft(newcolumn);
            x_shape += 256;
        }
        private void OffsetTop()
        {
            var _index = DynamicGrid.RowDefinitions.IndexOf(DynamicGrid.RowDefinitions.First());
            tiles.DeleteRow(_index);

            var newrow = new RowDefinition();
            newrow.Height = new GridLength(256);
            tiles.OffsetMatrixColumnTop(newrow);
            y_shape += 256;
        }
       // private void FirstFillGrid()
       // {
       //     tiles.SetImage(7, 6, 0, 2);
       //     tiles.SetImage(7, 5, 0, 1);
       //     tiles.SetImage(7, 4, 0, 0);
       //     tiles.SetImage(8, 6, 1, 2);
       //     tiles.SetImage(8, 5, 1, 1);
       //     tiles.SetImage(8, 4, 1, 0);
       //     tiles.SetImage(9, 6, 2, 2);
       //     tiles.SetImage(9, 5, 2, 1);
       //     tiles.SetImage(9, 4, 2, 0);
       // }
        private void FirstFillGrid()
        {
            int row = tiles.TileMatrix.GetLength(1);
            int column = tiles.TileMatrix.GetLength(0);
            int MainY = row / 2;
            int MainX = column / 2;
           // tiles.SetImage(8, 5, MainX, MainY);
            for (int x = 0; x < column; x++)
            {
                for (int y = 0; y < row; y++)
                {
                    tiles.SetImage(316902 - MainX + x, 163910 - MainY + y, x, y);
                }
            }
        }
    }
}
