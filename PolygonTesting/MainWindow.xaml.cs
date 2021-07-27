using System;
using System.Collections.Generic;
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

namespace PolygonTesting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Polygon myPolygon = new Polygon();
            myPolygon.Fill = System.Windows.Media.Brushes.LightSeaGreen;
            System.Windows.Point Point1 = new System.Windows.Point(0, 0);
            System.Windows.Point Point12 = new System.Windows.Point(0, 20);
            System.Windows.Point Point2 = new System.Windows.Point( 10, -30);
            System.Windows.Point Point3 = new System.Windows.Point(-10, -30);
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(Point1);
            myPointCollection.Add(Point12);
            myPointCollection.Add(Point2);
            myPointCollection.Add(Point3);
            myPointCollection.Add(Point12);
            myPolygon.Points = myPointCollection;
            Canvas.SetLeft(myPolygon, 100);
            Canvas.SetTop(myPolygon, 50);
            Mc.Children.Add(myPolygon);
            RotateTriangle(ref myPolygon, 30);
        }

        private void RotateTriangle(ref Polygon poly ,double angle)
        {
            Point Main = poly.Points.ElementAt(0);
            Point SubMain = poly.Points.ElementAt(1);
            Point p1 = poly.Points.ElementAt(2);
            Point p2 = poly.Points.ElementAt(3);
            Matrix matrix1 = new Matrix();
            matrix1.M11 = p1.X;
            matrix1.M12 = p1.Y;
            matrix1.M21 = p2.X;
            matrix1.M22 = p2.Y;
            Matrix matrix2 = new Matrix() {M11 = SubMain.X,M12 = SubMain.Y, M21 = SubMain.X, M22 = SubMain.Y };
            matrix2.Rotate(angle);
            matrix1.Rotate(angle);
            SubMain = new Point(matrix2.M11, matrix2.M12);
            poly.Points = new PointCollection() {Main,SubMain,
                new Point(matrix1.M11, matrix1.M12), new Point(matrix1.M21, matrix1.M22),SubMain };
        }
    }
}
