using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GridTestApplication.Utilities
{
    struct SizeEventArgs
    {
        public Size ParentPrevious { get; set; }
        public Size ParentCurrent { get; set; }
        public double _width;
        public double _height;
        public int zoom { get; set; } 
    }
    class SizeAdaptor
    {
        public double _width { get; set; }
        public double _height { get; set; }
        public int zoom { get; set; }
        SizeAdaptor(ref double Width,ref double Height,int Zoom)
        {
            _width = Width;
            _height = Height;
            zoom = Zoom;
        }
        public static void SetMapSize(SizeEventArgs eventArgs,Map map)
        {
            SizeAdaptor adaptor = new SizeAdaptor(ref eventArgs._width,ref eventArgs._height,eventArgs.zoom);
            adaptor.UpdateSize(eventArgs.ParentPrevious,eventArgs.ParentCurrent);
            map.Width = adaptor._width;
            map.Height =  adaptor._height;
        }
        private void UpdateSize(Size PreviousParentSize, Size NewParentSize)
        {
            if ((PreviousParentSize.Width * PreviousParentSize.Height != 0) && (NewParentSize.Height != PreviousParentSize.Height))
                ChangeSize(PreviousParentSize, NewParentSize);
            else InitialSize(NewParentSize);
        }
        private void ChangeSize(in Size PreviousParentSize, in Size NewParentSize)
        {
            double WidthCoefficient = NewParentSize.Width / PreviousParentSize.Width;
            double HeightCoefficient = NewParentSize.Height / PreviousParentSize.Height;
            ScaleMap(WidthCoefficient, HeightCoefficient, NewParentSize);
        }
        private void InitialSize(Size Parent)
        {
            _width = Parent.Width + 1536;
            double DesiredHeight = 256 * Math.Pow(2, zoom);
            if (DesiredHeight > Parent.Height + 1024) _height = Parent.Height + 1024;
            else _height = DesiredHeight;
        }
        private void ScaleMap(double WidthCoefficient, double HeightCoefficient,Size Parent)
        {
            _width *= WidthCoefficient;
            ValidateAndSetHeight(_height * HeightCoefficient,Parent);
        }
        private void ValidateAndSetHeight(double NewHeight,Size Parent)
        {
            if (NewHeight > Parent.Height + 1024) _height = Parent.Height + 1024;
            else if (NewHeight < 256) _height = 256;
            else _height = NewHeight;
        }
    }
}
