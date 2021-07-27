using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace MapProject.Utilities
{
    public static class MapFilling
    {
        public static void InitialFill(this Map map)
        {
            Point PixelStepper = map.GetMapPositionOnParent();
            PixelStepper.Offset(128,128);
            map.Dispatcher.BeginInvoke( new Action(() =>{
                for (int i = 0; i < map.RowDefinitions.Count; i++)
                {
                    FillColumn(map, PixelStepper, i);
                    PixelStepper.Offset(0, 256);
                }
            }));
            
        }
        public static void AddTilesOnColumn(this Map map,bool IsLeft)
        {
            Point PixelStepper = map.GetMapPositionOnParent();
            
            if (IsLeft)
            {
                PixelStepper.Offset(128, 128);
                FillRow(map, PixelStepper, 0);
            }    
            else
            {
                PixelStepper.Offset((map.ColumnDefinitions.Count -1) * 256, 128);
                FillRow(map, PixelStepper, map.ColumnDefinitions.Count - 1);
            }    
        }
        public static void AddTilesOnRow(this Map map, bool IsTop)
        {
            Point PixelStepper = map.GetMapPositionOnParent();
            if (IsTop)
            {
                PixelStepper.Offset(128, 128);
                FillColumn(map, PixelStepper, 0);
            }
            else
            {
                PixelStepper.Offset( 128, (map.RowDefinitions.Count)*256);
                FillColumn(map, PixelStepper, map.RowDefinitions.Count - 1);
            }
        }
        private static void FillRow(Map map, Point RowStepper, int Column)
        {
            for (int i = 0; i <= map.RowDefinitions.Count - 1; i++)
            {
                Point DegreesCoordinates = RowStepper.ToWorldPosition(map);
                AddImage(map, Column, i, DegreesCoordinates, map.GetZoom());
                RowStepper.Offset(0, 256);
            }
        }
        private static void FillColumn(Map map, Point RowStepper, int Row)
        {
            for (int i = 0; i <= map.ColumnDefinitions.Count - 1;i++)
            {
                Point DegreesCoordinates = RowStepper.ToWorldPosition(map);
                AddImage(map, i, Row, DegreesCoordinates, map.GetZoom());
                RowStepper.Offset(256, 0);
            }
        }
        private static void AddImage(Grid map, int column, int row, Point DegreesCoordinates, int zoom)
        {
            UIElement content = GetContent(DegreesCoordinates, zoom);
            map.Children.Add(content);
            Grid.SetColumn(content, column);
            Grid.SetRow(content, row);
        }
        private static UIElement GetContent(Point DegreesCoordinates, int zoom)
        {
            if (DegreesCoordinates.X >= 180)
                DegreesCoordinates.X = -DegreesCoordinates.X;
            System.Windows.Controls.Image image = new System.Windows.Controls.Image()
            { Source = GetTile(DegreesCoordinates, zoom)};
            return image;
        }
        private static Border GetInitialBorder()
        {
            return new Border()
            {
                Width = 256,
                Height = 256,
                BorderBrush = new SolidColorBrush(Colors.Red),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Colors.White)
            };
        }

        private static BitmapImage GetTile(Point DegreesCoordinates, int zoom)
        {
            Point Tile = DegreesCoordinates.WorldToTilePos(zoom);
            return 
                GetValidatedTile(Tile, GetCachePath(Tile, zoom), zoom);
        }

        private static BitmapImage GetValidatedTile(Point Tile,string path, int zoom)
        {
            Uri uri = null;
            BitmapImage image;
            try { image = new BitmapImage(new Uri(Path.GetFullPath(path), UriKind.Absolute)); }
            catch (Exception e)
            {
                image = GetAndSaveWebPicture(ref uri, Tile, zoom);
            }
            return image;
        }

        private static BitmapImage GetAndSaveWebPicture(ref Uri uri, Point Tile, int zoom)
        {
            uri = new Uri(GetWebPath(Tile, zoom));
            BitmapImage image = new BitmapImage(uri);
            image.DownloadCompleted += ImageDownloadCompleted;
            return image;
        }
        private static void ImageDownloadCompleted(object sender, EventArgs e)
        {
            BitmapImage image = (BitmapImage)sender;
            string path = image.UriSource.GetLeftPart(UriPartial.Path);
            path = path.Remove(0, image.UriSource.GetLeftPart(UriPartial.Authority).Count() + 1);
            int[] tile = UriParser(path);
            BitmapImage ImageToSave = image.Clone();
            SaveImage(ImageToSave, tile[0], tile[1], tile[2]);
        }

        private static string GetWebPath(Point Tile, int zoom)
        {
            string TileFormat = @"https://tile.openstreetmap.org/{0}/{1}/{2}.png";
            return string.Format(CultureInfo.InvariantCulture, TileFormat, zoom, (int)Tile.X, (int)Tile.Y);
        }

        private static string GetCachePath(Point Tile, int zoom)
        {
            string LocalRelativePath = @"Images/Tiles/";
            string TileFormat = LocalRelativePath + @"{0}/{1}/{2}.png";
            return string.Format(TileFormat, zoom, (int)Tile.X, (int)Tile.Y);
        }

        private static Object mutex = new Object();
        private static void SaveImage(BitmapImage bitmapImage,int zoom,int x,int y)
        {
            string localFileName = string.Format(@"Images/Tiles/{0}/{1}/{2}.png",zoom,x,y);
            Directory.CreateDirectory(string.Format(@"Images/Tiles/{0}/{1}",zoom,x));
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            lock(mutex)
            {
                using (var fileStream = new FileStream(localFileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private static int[] UriParser(string uri)
        {
            char[] s = uri.ToCharArray();
            int zoom = GetZoom(ref s);
            while (zoom == -1) zoom = GetZoom(ref s);
            int x = GetTileCoordinate(ref s,'/');
            int y = GetTileCoordinate(ref s,'.');
            return new int[3] { zoom, x, y };
        }

        private static int GetZoom(ref char[] s)
        {
            int zoom = -1;
            try { zoom = Int32.Parse(String.Join("", s.TakeWhile(t => t != '/'))); }
            catch (FormatException e) { }
            s = s.SkipWhile(t => t != '/').ToArray();
            s = s.Skip(1).ToArray();
            return zoom;
        }

        private static int GetTileCoordinate(ref char[] s,char StopSymbol)
        {
            int result = Int32.Parse(String.Join("", s.TakeWhile(t => t != StopSymbol)));

            s = s.SkipWhile(t => t != StopSymbol).ToArray();
            s = s.Skip(1).ToArray();
            return result;
        }
    }
}
