using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace TileMap
{
    class Tile
    {
        public int z { get; set; }
        public long x { get; set; }
        public long y { get; set; }
    }
    class Tiles
    {
        public Image[,] TileMatrix { get; set; }
        Grid DynamicGrid;
        string[,] indeces { get; }
        Tile tile;
        int zoom;

        public Tiles(int columns,int rows,Grid Maingrid,int zoom)
        {
            TileMatrix = new Image[columns, rows];
            DynamicGrid = Maingrid;
            DynamicGrid.ColumnDefinitions.Clear();
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.Children.Clear();
            this.zoom = zoom;
            indeces = new string[columns, rows];
        }

        private void FillGrid(int column, int row)
        {
            Grid.SetColumn(TileMatrix[column, row],column);
            Grid.SetRow(TileMatrix[column, row], row);

            if(DynamicGrid.Children.IndexOf(TileMatrix[column, row]) != -1)
            {
                int y = DynamicGrid.Children.IndexOf(TileMatrix[column, row]);
            }
            else { DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Add(TileMatrix[column, row]))); }
            
        }
        private void FillGrid(int column, int row,Grid grid)
        {
            Grid.SetColumn(TileMatrix[column, row], column);
            Grid.SetRow(TileMatrix[column, row], row);
            Image im = new Image();
            
            if(TileMatrix[column, row].Source.Clone() != null)
            {
                im.Source = TileMatrix[column, row].Source.Clone();
                grid.Children.Add(im);
            }
            

        }

        public void SetImage(long xnum, long ynum,int zoom, int column, int row)
        {
            Image image = new Image();
            image.Source = GetPicture(zoom, xnum, ynum);
            TileMatrix[column, row] = image;
            indeces[column, row] = zoom + "," + xnum.ToString()+ "," + ynum.ToString();
            FillGrid(column,row);
        }

        public void SetImage(long xnum, long ynum, int zoom, int column, int row,Grid grid)
        {
            Image image = new Image();
            image.Source = GetPicture(zoom, xnum, ynum);
            TileMatrix[column, row] = image;
            indeces[column, row] = zoom + "," + xnum.ToString() + "," + ynum.ToString();
            FillGrid(column, row,grid);
        }

        public Tile GetTile(int column , int row)
        {
            tile = new Tile();
            List<Char> NonParsedString = indeces[column, row].ToList();
            //Parse zoom
            string StrZoom = String.Join("", NonParsedString.TakeWhile(x => x != ','));
            NonParsedString = NonParsedString.SkipWhile(x => x != ',').ToList();
            NonParsedString.Remove(',');
            //Parse x(column)
            string StrX = String.Join("", NonParsedString.TakeWhile(x => x != ','));
            NonParsedString = NonParsedString.SkipWhile(x => x != ',').ToList();
            NonParsedString.Remove(',');
            //Parse y(row)
            string StrY = String.Join("", NonParsedString.TakeWhile(x => x != ','));
            NonParsedString = NonParsedString.SkipWhile(x => x == ',').ToList();
            //Fill Tile object
            tile.z = Int32.Parse(StrZoom);
            tile.x= Int32.Parse(StrX);
            tile.y = Int32.Parse(StrY);
            return tile;
        }
        public Tile GetTile(Image image)
        {
            
            tile = new Tile();
            IEnumerable<Char> s ="";
            long x = 0;
            long y = 0;
            if (String.Join("", image.Source.ToString().Take(4)) == "http")
            {
                s = image.Source.ToString().Skip(8);
                s = s.SkipWhile(t => t != '/');
                s = s.Skip(1);
                s = s.SkipWhile(t => t != '/');
                s = s.Skip(1);
                x = long.Parse(String.Join("", s.TakeWhile(t => t != '/')));
                s = s.SkipWhile(t => t != '/');
                s = s.Skip(1);
                y = long.Parse(String.Join("", s.TakeWhile(t => t != '.')));
            }
            else
            {
                s = image.Source.ToString().Skip(Path.GetFullPath("_Cache").Length + 8);
                s = s.SkipWhile(t => t != '/');
                s = s.Skip(1);
                s = s.SkipWhile(t => t != '/');
                s = s.Skip(1);
                x = long.Parse(String.Join("", s.TakeWhile(t => t != ',')));
                s = s.SkipWhile(t => t != ',');
                s = s.Skip(1);
                y = long.Parse(String.Join("", s.TakeWhile(t => t != '.')));
            }
            
            tile.z = zoom;
            tile.x = x;
            tile.y = y;
            return tile;
        }
        public Image GetImage(int column, int row){return TileMatrix[column,row];}
        public Point GetIndexces(Tile tile)
        {
            Point p = new Point();
            for (int r = indeces.GetUpperBound(1); r >= indeces.GetLowerBound(1); r--)
            {
                for (int c = indeces.GetUpperBound(0); c > indeces.GetLowerBound(0); c--)
                {
                    var currT = GetTile(c, r);
                    if ((currT.x == tile.x) && (currT.y == tile.y)&& (currT.z == tile.z))
                    {
                        p.X = c;
                        p.Y = r;
                    }
                }
            }
            return p;
        }
        public void DeleteImage(int column, int row)
        {
            DynamicGrid.Dispatcher.Invoke(new Action(()=>DynamicGrid.Children.Remove(TileMatrix[column, row])));
            TileMatrix[column, row] = null;
            indeces[column, row] = null;
        }

        public void DeleteColumn(int column)
        {
            
            for (int i = TileMatrix.GetLowerBound(1);i<=TileMatrix.GetUpperBound(1);i++)
            {  
                TileMatrix[column, i] = null;
                indeces[column, i] = null;
            }
            DynamicGrid.ColumnDefinitions.RemoveAt(column);
        }

        public void DeleteRow(int row)
        {
            
            for (int i = TileMatrix.GetLowerBound(0); i <= TileMatrix.GetUpperBound(0); i++)
            {
                TileMatrix[i, row] = null;
                indeces[i, row] = null;
            }
            
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.RowDefinitions.RemoveAt(row)));
        }

        public void OffsetMatrixColumnRight(ColumnDefinition column)
        {
            //System.Threading.Thread.Sleep(5000);
            
            for (int r = TileMatrix.GetUpperBound(1); r >= TileMatrix.GetLowerBound(1); r--)  
            {
                
            for (int c = TileMatrix.GetUpperBound(0); c > TileMatrix.GetLowerBound(0); c--)
            {
                TileMatrix[c, r] = TileMatrix[c - 1, r];
                indeces[c, r] = indeces[c - 1, r];
                FillGrid(c, r);
            }
            }
            DynamicGrid.ColumnDefinitions.Add(column);
            for (int r = TileMatrix.GetUpperBound(1); r >= TileMatrix.GetLowerBound(1); r--)
            {
              Tile t = GetTile(indeces.GetLowerBound(0), r);
              SetImage(t.x - 1, t.y,zoom, indeces.GetLowerBound(0), r);
            }

        }

        public void OffsetMatrixColumnBottom(RowDefinition nrow)
        {
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            Grid grid = new Grid();
            for (int c = TileMatrix.GetUpperBound(0); c >= TileMatrix.GetLowerBound(0); c--)
            {
                for (int r = TileMatrix.GetUpperBound(1); r > TileMatrix.GetLowerBound(1); r--)
                {
                    TileMatrix[c, r] = TileMatrix[c, r-1];
                    indeces[c, r] = indeces[c, r-1];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.RowDefinitions.Add(nrow);
            for (int c = TileMatrix.GetUpperBound(0); c >= TileMatrix.GetLowerBound(0); c--)
            {
                Tile t = GetTile(c , indeces.GetLowerBound(1));
                SetImage(t.x , t.y - 1, zoom, c , indeces.GetLowerBound(1));
            }
        }

        public void OffsetMatrixColumnLeft(ColumnDefinition ncolumn)
        {
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            for (int r = TileMatrix.GetLowerBound(1); r <= TileMatrix.GetUpperBound(1); r++)
            {
                for (int c = TileMatrix.GetLowerBound(0); c < TileMatrix.GetUpperBound(0); c++)
                {
                    TileMatrix[c, r] = TileMatrix[c + 1, r];
                    indeces[c, r] = indeces[c + 1, r];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.ColumnDefinitions.Add(ncolumn);
            for (int r = TileMatrix.GetLowerBound(1); r <= TileMatrix.GetUpperBound(1); r++)
            {
                Tile t = GetTile(indeces.GetUpperBound(0), r);
                SetImage(t.x + 1, t.y, zoom, indeces.GetUpperBound(0), r);
            }
        }
        public void OffsetMatrixColumnTop(RowDefinition nrow)
        {
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            for (int c = TileMatrix.GetLowerBound(0); c <= TileMatrix.GetUpperBound(0); c++)
            {
                for (int r = TileMatrix.GetLowerBound(1); r < TileMatrix.GetUpperBound(1); r++)
                {
                    TileMatrix[c, r] = TileMatrix[c, r + 1];
                    indeces[c, r] = indeces[c, r + 1];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.RowDefinitions.Add(nrow);
            for (int c = TileMatrix.GetLowerBound(0); c <= TileMatrix.GetUpperBound(0); c++)
            {
                Tile t = GetTile(c, indeces.GetUpperBound(1));
                SetImage(t.x, t.y + 1, zoom, c, indeces.GetUpperBound(1));
            }
        }
        private  BitmapImage GetPicture(int zoom, long x, long y)
        {
            Directory.CreateDirectory("_Cache");
            if (File.Exists($@"_Cache\{zoom}\{ x},{ y}.png"))
            {
                var abs = Path.GetFullPath($@"_Cache\{zoom}\{x},{y}.png");
                Uri uri = new Uri(abs, UriKind.Absolute); 
                return new BitmapImage(uri);
            }
            else              
            {
                string path = $@"_Cache\{ zoom}";
                Directory.CreateDirectory(path);
                var image =  DownloadPicture(zoom, x, y);
                return image;
            }
        }
        private BitmapImage DownloadPicture(int zoom, long x, long y)
        {
            const string TileFormat = @"http://tile.openstreetmap.org/{0}/{1}/{2}.png";
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, TileFormat, zoom, x, y));
            var Image = new BitmapImage(uri);
            Image.Changed += Image_Changed;
            Image.DownloadFailed += ImageDownloadFailed;
            return Image;
        }

        private void ImageDownloadFailed(object sender, ExceptionEventArgs e)
        {
            //BitmapImage image = (BitmapImage)sender;
            //Image_Changed(sender, null);
        }

        private void Image_Changed(object sender, EventArgs e)
        {
            BitmapImage Image = (BitmapImage)sender;
            var s = Image.ToString().Skip(8);
            s = s.SkipWhile(t => t != '/');
            s = s.Skip(1);
            s = s.SkipWhile(t => t != '/');
            s = s.Skip(1);
            var x = long.Parse(String.Join("", s.TakeWhile(t => t != '/')));
            s = s.SkipWhile(t => t != '/');
            s = s.Skip(1);
            var y = long.Parse(String.Join("", s.TakeWhile(t => t != '.')));
            string localFileName = $@"_Cache\{zoom}\{x},{y}.png";
            if (!File.Exists(localFileName))
            {
                BitmapImage saveImage = Image.Clone();
                SavePic(saveImage, zoom, x, y);
            }
        }
        private void SavePic(BitmapImage bitmapImage, int zoom, long x, long y)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            string localFileName = $@"_Cache\{zoom}\{x},{y}.png";
            using (var fileStream = new System.IO.FileStream(localFileName, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            WebClient web = (WebClient)sender;
            Console.WriteLine(e.Error);
            web.Dispose();
        }

        private void CreateRequest(int zoom, long x, long y)
        {
            string path = $@"_Cache\{ zoom}";
            const string q = "\"";
            if (Directory.Exists(path))
            {
                string JsonFormat;
                if(File.Exists(@"../Request.json"))
                {
                    string[] currentText = File.ReadAllLines(@"../Request.json");
                    string LastStr = currentText.Last().Remove(currentText.Last().Count() -1 );
                    JsonFormat = LastStr + ",\n{" + $"{q}z{q}:{q}{zoom}{q},\n{q}x{q}:{q}{x}{q},\n{q}y{q}:{q}{y}{q}" + "}]";
                    currentText[currentText.Count() - 1] = JsonFormat;
                    File.WriteAllLines(@"../Request.json", currentText);
                }
                else
                {
                    JsonFormat = "[{" + $"{q}z{q}:{q}{zoom}{q},\n{q}x{q}:{q}{x}{q},\n{q}y{q}:{q}{y}{q}" + "}]";
                    File.AppendAllText(@"../Request.json", JsonFormat);
                }
                
            }
            else
            {
                Directory.CreateDirectory($@"_Cache\{ zoom}");     //Directory ..\_Cache\{ zoom} not exist
                string JsonFormat;
                if (File.Exists(@"../Request.json"))
                {
                    string[] currentText = File.ReadAllLines(@"../Request.json");
                    string LastStr = currentText.Last().Remove(currentText.Last().Count() - 1);
                    JsonFormat = LastStr + ",\n{" + $"{q}z{q}:{q}{zoom}{q},\n{q}x{q}:{q}{x}{q},\n{q}y{q}:{q}{y}{q}" + "}]";
                    currentText[currentText.Count() - 1] = JsonFormat;
                    File.WriteAllLines(@"../Request.json", currentText);
                }
                else
                {
                    JsonFormat = "[{" + $"{q}z{q}:{q}{zoom}{q},\n{q}x{q}:{q}{x}{q},\n{q}y{q}:{q}{y}{q}" + "}]";
                    File.AppendAllText(@"../Request.json", JsonFormat);
                }
            }
        }
    }
    
}
