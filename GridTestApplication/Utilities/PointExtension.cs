using System;
using System.Windows;

namespace MapProject.Utilities
{
    public static class PointExtension
    {
        public static Point ToWorldPosition(this Point MousePixelCoordinates, Map map)
        {
            Point pixelOffset = GetPixelOffset(MousePixelCoordinates, map.GetPixelParentCentre());
            return ValidatedDegreeCoordinates(new Point()
            {
                Y = GetVerticalCoordinate(map.GetZoom(), map.GetMapCentreInDegrees().Y, pixelOffset.Y),
                X = map.GetMapCentreInDegrees().X + pixelOffset.X * GetHorizontalResolution(map.GetZoom())
            });
        }
        public static Point ToPixels(this Point point, int zoom)
        {
            point.Y = GetLatitudePixelOf(point.Y, zoom);
            point.X /= GetHorizontalResolution(zoom);
            return point;
        }
        public static Vector ToPixels(this Vector vector, int zoom)
        {
            vector.Y = GetLatitudePixelOf(vector.Y,zoom);
            vector.X /= GetHorizontalResolution(zoom);
            return vector;
        }
        public static void LimitTo(this ref Point MousePixelCoordinates, Size MapSize)
        {
            MousePixelCoordinates.Y = CheckYBorder(MousePixelCoordinates.Y, MapSize.Height);
            MousePixelCoordinates.X = CheckXBorder(MousePixelCoordinates.X, MapSize.Width);
        }
        public static Point TileToWorldPos( double tile_x, double tile_y, int zoom)
        {
            Point p = new Point();
            double n = Math.Pow(2.0, zoom);
            p.X = tile_x * 360 / n - 180.0;
            p.Y = Math.Atan(Math.Sinh(Math.PI - tile_y * 2.0 * Math.PI / n)) * 180.0 / Math.PI;
            return p;
        }
        public static Point WorldToTilePos(this Point worldPos, int zoom)
        {
            Point p = new Point();
            p.X = (int)((worldPos.X + 180.0) / 360.0 * (1 << zoom));
            p.Y = (int)Math.Floor((1 - Math.Log(Math.Tan(worldPos.Y*Math.PI/180) +
                1 / Math.Cos(worldPos.Y * Math.PI / 180)) / Math.PI) / 2 * (1 << zoom));
            return p;
        }
        private static Point ValidatedDegreeCoordinates(Point DegreeCoordinates)
        {
            if ((Math.Abs(DegreeCoordinates.X) < 180) &&
                (Math.Abs(DegreeCoordinates.Y) < Map.MercatorLatitudeLimit )) return DegreeCoordinates;
            else return new Point(ValidatedDegreeLongitude(DegreeCoordinates.X), DegreeCoordinates.Y); // Remove ValidatedDegreeLatitude(DegreeCoordinates.Y)
        }
        private static double ValidatedDegreeLongitude(double LongitudeInDegrees)
        {
            if (LongitudeInDegrees < 180 && LongitudeInDegrees > -180) return LongitudeInDegrees;
            else if (LongitudeInDegrees > 180) return GetPositiveLongitude(LongitudeInDegrees);
            else return GetNegativeLongitude(LongitudeInDegrees);
        }
        private static double GetPositiveLongitude(double LonditudeInDegrees) //Longitude > 180 
        {
            return (LonditudeInDegrees +180) % 360 - 180;
        }
        private static double GetNegativeLongitude(double LonditudeInDegrees) //Longitude < -180 
        {
            return (LonditudeInDegrees + 180) % 360 + 180;
        }
        public static double GetVerticalCoordinate(int zoom,double CurrentLatitudeInCentre,double PixelMouseOffset)
        {
            PixelMouseOffset += GetLatitudePixelOf(CurrentLatitudeInCentre, zoom);
            double Radians =  Math.PI * (PixelMouseOffset / Math.Pow(2.0,zoom + 7));
            double DegreesAtan = Math.Atan(Math.Exp(Radians)) * 360 / ( 2 * Math.PI);
            double Latitude = 2 * DegreesAtan - 90;
            return Latitude;
        }
        private static double GetLatitudePixelOf(double x,int zoom)
        {
            double Radians = Math.Log((1 + Math.Sin(ToRadians(x))) / (1 - Math.Sin(ToRadians(x))))/2;
            double coefficient = Radians / Math.PI ;
            double Pixels = coefficient * Math.Pow(2.0, zoom + 7);
            return Pixels;
        }
        private static double ToRadians(double x)
        {
            return x * 2 * Math.PI / 360;
        }
        private static double GetHorizontalResolution(int zoom)
        {
            return 360 / ( Math.Pow(2.0, zoom + 8));
        }
        private static Point GetPixelOffset(Point MousePixelCoordinates, Point ParentCenterPixelCoordinates)
        {
            double DebugCentreX = MousePixelCoordinates.X - ParentCenterPixelCoordinates.X;
            double DebugCentreY = MousePixelCoordinates.Y - ParentCenterPixelCoordinates.Y;
            return new Point()
            {
                X = DebugCentreX,
                Y = -DebugCentreY
            };
        }
        private static double CheckXBorder(double X, double Width)
        {
            if (X < 0) return 0;
            else
            {
                if (X > Width) return Width;
                else return X;
            }
        }
        private static double CheckYBorder(double Y, double Height)
        {
            if (Y < 0) return 0;
            else
            {
                if (Y > Height) return Height;
                else return Y;
            }
        }
        public static Point GetDiferenceBetween(this Point Point1, Point Point2)
        {
            return new Point(Point1.X - Point2.X,Point1.Y - Point2.Y);
        }
    }
}
