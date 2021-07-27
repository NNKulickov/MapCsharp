using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace MapProject
{
    public partial class MainWindow : Window
    {
        Map map;
        public MainWindow()
        {
            InitializeComponent();
            map = new Map(8);
            MC.Children.Add(map);
            map.ConfigureMap();
            map.AddIcon(new Point(37.6173, 55.7558),"car", "0‎");
            Dictionary<DateTime, Point> trackPoints = new Dictionary<DateTime, Point>();
            Dictionary <DateTime, Point> trackPoints2 = GetPoints();
            //System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            //long unixTime = 15200523;
            //dtDateTime = dtDateTime.AddSeconds(unixTime);
            //trackPoints.Add(dtDateTime, new Point(0,0));
            //dtDateTime = dtDateTime.AddSeconds(100);
            //trackPoints.Add(dtDateTime, new Point(-10, 40));
            //dtDateTime = dtDateTime.AddSeconds(100);
            //trackPoints.Add(dtDateTime, new Point(37.6173, 55.7558));
            map.SetView(55.7558, 37.6173, 8);
            map.AddTrack(trackPoints2, "Путь");

        }

        private void Main_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MC.Height = ActualHeight - 256;
            MC.Width = ActualWidth - 256;
            MC.HorizontalAlignment = HorizontalAlignment.Center;
            MC.VerticalAlignment = VerticalAlignment.Center;
        }
        private Dictionary<DateTime, Point> GetPoints()
        {
            Dictionary<DateTime, Point> data = new Dictionary<DateTime, Point>();
            string path = Path.GetFullPath(@"test_route.csv");
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    Point p = new Point(Double.Parse(values[1]), Double.Parse(values[2]));
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    long unixTime = long.Parse(values[0]);
                    dtDateTime = dtDateTime.AddSeconds(unixTime);
                    if ((!data.ContainsValue(p)) && (!data.ContainsKey(dtDateTime)))
                        data.Add(dtDateTime, p);
                }
            }
            return data;
        }
    }
}
