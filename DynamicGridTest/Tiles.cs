using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DynamicGridTest
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
        string[,] _indexes { get; }
        Tile tile;

        public Tiles(int columns,int rows,Grid Maingrid)
        {
            TileMatrix = new Image[columns, rows];
            DynamicGrid = Maingrid;
            _indexes = new string[columns, rows];
        }

        public void FillGrid(int column, int row)
        {
            Grid.SetColumn(TileMatrix[column, row],column);
            Grid.SetRow(TileMatrix[column, row], row);
            if (DynamicGrid.Children.IndexOf(TileMatrix[column, row]) != -1)
            {
                int y = DynamicGrid.Children.IndexOf(TileMatrix[column, row]);
            }
            else { DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Add(TileMatrix[column, row]))); }
        }

        public void SetImage(long xnum, long ynum, int column, int row)
        {
            Image image = new Image();
            image.Source = GetPicture(19, xnum, ynum);
            TileMatrix[column, row] = image;
            _indexes[column, row] = "19" + ","+xnum.ToString()+ "," + ynum.ToString();
            FillGrid(column,row);
        }
        public Tile GetTile(int column , int row)
        {
            tile = new Tile();
            List<Char> NonParsedString = _indexes[column, row].ToList();
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
        public Image GetImage(int column, int row){return TileMatrix[column,row];}

        public void DeleteImage(int column, int row)
        {
            DynamicGrid.Dispatcher.BeginInvoke(new Action(()=>DynamicGrid.Children.Remove(TileMatrix[column, row])));
            TileMatrix[column, row] = null;
            _indexes[column, row] = null;
        }

        public void DeleteColumn(int column)
        {
            
            for (int i = TileMatrix.GetLowerBound(1);i<=TileMatrix.GetUpperBound(1);i++)
            {  
                DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Remove(TileMatrix[column, i])));
                TileMatrix[column, i] = null;
                _indexes[column, i] = null;
            }
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.ColumnDefinitions.RemoveAt(column)));
        }

        public void DeleteRow(int row)
        {
            for (int i = TileMatrix.GetLowerBound(0); i <= TileMatrix.GetUpperBound(0); i++)
            {
                DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Remove(TileMatrix[i, row])));
                TileMatrix[i, row] = null;
                _indexes[i, row] = null;
            }
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.RowDefinitions.RemoveAt(row)));
        }

        public void OffsetMatrixColumnRight(ColumnDefinition ncolumn)
        {
            DynamicGrid.IsEnabled = false;
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            for (int r = TileMatrix.GetUpperBound(1); r >= TileMatrix.GetLowerBound(1); r--)
            {
                for (int c = TileMatrix.GetUpperBound(0); c > TileMatrix.GetLowerBound(0); c--)
                {

                    TileMatrix[c, r] = TileMatrix[c - 1, r];
                    _indexes[c, r] = _indexes[c - 1, r];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.ColumnDefinitions.Add(ncolumn)));
            for (int r = TileMatrix.GetUpperBound(1); r >= TileMatrix.GetLowerBound(1); r--)
            {
                Tile t = GetTile(_indexes.GetLowerBound(0), r);
                SetImage(t.x - 1, t.y, _indexes.GetLowerBound(0), r);
            }
            DynamicGrid.IsEnabled = true;
        }

        public void OffsetMatrixColumnBottom(RowDefinition nrow)
        {
            DynamicGrid.IsEnabled = false;
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            for (int c = TileMatrix.GetUpperBound(0); c >= TileMatrix.GetLowerBound(0); c--)
            {
                for (int r = TileMatrix.GetUpperBound(1); r > TileMatrix.GetLowerBound(1); r--)
                {
                    TileMatrix[c, r] = TileMatrix[c, r - 1];
                    _indexes[c, r] = _indexes[c, r - 1];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.RowDefinitions.Add(nrow)));
            for (int c = TileMatrix.GetUpperBound(0); c >= TileMatrix.GetLowerBound(0); c--)
            {
                Tile t = GetTile(c, _indexes.GetLowerBound(1));
                SetImage(t.x, t.y - 1, c, _indexes.GetLowerBound(1));
            }
            DynamicGrid.IsEnabled = true;
        }

        public void OffsetMatrixColumnLeft(ColumnDefinition ncolumn)
        {
            DynamicGrid.IsEnabled = false;
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            for (int r = TileMatrix.GetLowerBound(1); r <= TileMatrix.GetUpperBound(1); r++)
            {
                for (int c = TileMatrix.GetLowerBound(0); c < TileMatrix.GetUpperBound(0); c++)
                {
                    TileMatrix[c, r] = TileMatrix[c + 1, r];
                    _indexes[c, r] = _indexes[c + 1, r];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.ColumnDefinitions.Add(ncolumn)));
            for (int r = TileMatrix.GetLowerBound(1); r <= TileMatrix.GetUpperBound(1); r++)
            {
                Tile t = GetTile(_indexes.GetUpperBound(0), r);
                SetImage(t.x + 1, t.y, _indexes.GetUpperBound(0), r);
            }
            DynamicGrid.IsEnabled = true;
        }
        public void OffsetMatrixColumnTop(RowDefinition nrow)
        {
            DynamicGrid.IsEnabled = false;
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.Children.Clear()));
            for (int c = TileMatrix.GetLowerBound(0); c <= TileMatrix.GetUpperBound(0); c++)
            {
                for (int r = TileMatrix.GetLowerBound(1); r < TileMatrix.GetUpperBound(1); r++)
                {
                    TileMatrix[c, r] = TileMatrix[c, r + 1];
                    _indexes[c, r] = _indexes[c, r + 1];
                    FillGrid(c, r);
                }
            }
            DynamicGrid.Dispatcher.Invoke(new Action(() => DynamicGrid.RowDefinitions.Add(nrow)));
            for (int c = TileMatrix.GetLowerBound(0); c <= TileMatrix.GetUpperBound(0); c++)
            {
                Tile t = GetTile(c, _indexes.GetUpperBound(1));
                SetImage(t.x, t.y + 1, c, _indexes.GetUpperBound(1));
            }
            DynamicGrid.IsEnabled = true;
        }
        private BitmapImage GetPicture(int zoom, long x, long y)
        {
            const string TileFormat = @"http://tile.openstreetmap.org/{0}/{1}/{2}.png";
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, TileFormat, zoom, x, y));
            return new BitmapImage(uri);
        }
    }
    
}
