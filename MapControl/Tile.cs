using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapControl
{
    /// <summary>Represents a single map tile.</summary>
    internal sealed class Tile : Image
    {
        private int _tileX;
        private int _tileY;
        private int _zoom;

        /// <summary>Initializes a new instance of the Tile class.</summary>
        /// <param name="zoom">The zoom level for the tile.</param>
        /// <param name="x">The tile index along the X axis.</param>
        /// <param name="y">The tile index along the Y axis.</param>
        public Tile(int zoom, int x, int y)
        {
            _tileX = x;
            _tileY = y;
            _zoom = zoom;
            this.LoadTile();
        }

        /// <summary>Gets or sets the tile index along the X axis.</summary>
        public int TileX
        {
            get
            {
                return _tileX;
            }
            set
            {
                if (value != _tileX)
                {
                    _tileX = value;
                    this.LoadTile();
                }
            }
        }

        /// <summary>Gets or sets the tile index along the Y axis.</summary>
        public int TileY
        {
            get
            {
                return _tileY;
            }
            set
            {
                if (value != _tileY)
                {
                    _tileY = value;
                    this.LoadTile();
                }
            }
        }

        /// <summary>Gets or sets the column index of the tile.</summary>
        internal int Column { get; set; }

        /// <summary>Gets or sets the row index of the tile.</summary>
        internal int Row { get; set; }

        private void LoadTile()
        {
            this.Source = null;
            System.Threading.ThreadPool.QueueUserWorkItem(this.LoadTileInBackground);
        }

        private void LoadTileInBackground(object state)
        {
            ImageSource image = TileGenerator.GetTileImage(_zoom, _tileX, _tileY);
            if (image != null) // We've already set the Source to null before calling this method.
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Source = image;
                }));
            }
        }
    }
}
