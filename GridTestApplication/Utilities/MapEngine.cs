using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MapProject.Utilities
{
    public static class MapEngine
    {
        enum OffsetVerticalDirection : byte
        {
            TopBoundary = 1,
            BottomBoundary = 2,
            InBoundary = 0
        }
        enum OffsetHorizontalDirection : byte
        {
            LeftBoundary = 1,
            RightBoundary = 2,
            InBoundary = 0
        }
        public static void AutosizeColumnsAndRows(this Grid grid)
        {
            int ColumnsToAdd = MatchColumns(grid.ColumnDefinitions.Count, (int)grid.Width / 256) +2;
            int RowsToAdd = MatchRows(grid.RowDefinitions.Count, (int)grid.Height / 256);
            grid.AddColumnsAndRows(ColumnsToAdd, RowsToAdd);
        }
        public static void UpdateGrid(this Grid grid, ref Vector Offset)
        {
            Offset.Y = grid.RestructVertical(Offset.Y);
            Offset.X = grid.RestructHorizontal(Offset.X);
        }
        private static double RestructVertical(this Grid grid,double offset_Y)
        {
            switch (CheckVerticalBound(offset_Y))
            {
                case OffsetVerticalDirection.InBoundary:
                    break;
                case OffsetVerticalDirection.TopBoundary:
                    grid.RestructFromBottom();
                    offset_Y = 0;
                    break;
                case OffsetVerticalDirection.BottomBoundary:
                    grid.RestructFromTop();
                    offset_Y = 0;
                    break;
            }
            return offset_Y;
        }
        private static double RestructHorizontal(this Grid grid,double offset_X)
        {
            switch (CheckHorizontalBound(offset_X))
            {
                case OffsetHorizontalDirection.InBoundary:
                    break;
                case OffsetHorizontalDirection.LeftBoundary:
                    grid.RestructFromLeft();
                    offset_X = 0;
                    break;
                case OffsetHorizontalDirection.RightBoundary:
                    grid.RestructFromRight();
                    offset_X = 0;
                    break;
            }
            return offset_X;
        }
        private static OffsetHorizontalDirection CheckHorizontalBound(in double offset_X)
        {
            OffsetHorizontalDirection     _HorizontalOffset = 0;
            if      (offset_X < -256)     _HorizontalOffset += 1;
            else if (offset_X > 256)      _HorizontalOffset += 2;
            return _HorizontalOffset;
        }
        private static OffsetVerticalDirection CheckVerticalBound(in double offset_Y)
        {
            OffsetVerticalDirection       _VerticalOffset = 0;
            if (offset_Y < -256)          _VerticalOffset += 2;
            else if (offset_Y > 256)      _VerticalOffset += 1;
            return _VerticalOffset;
        }
        public static void AddColumnsAndRows(this Grid grid, double ColumnsCount, double RowsCount)
        {
            if (ColumnsCount > 0) grid.AddColumns((int)ColumnsCount); else grid.DeleteColumns(-(int)ColumnsCount);
            if (RowsCount > 0) grid.AddRows((int)RowsCount); else grid.DeleteRows(-(int)RowsCount);
        }
        private static void AddColumns(this Grid grid,int count)
        {
            for (int i = 0; i < count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(256)
                });
                
            }
        }
        private static void AddRows(this Grid grid, int count)
        {
            for (int i = 0; i < count; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(256)
                });
            }
        }
        private static void DeleteColumns(this Grid grid, int count)
        {

            if (grid.ColumnDefinitions.Count() != 0) for (int i = 0; i < count; i++)
            {
                grid.ColumnDefinitions.RemoveAt(grid.ColumnDefinitions.Count() - 1);
            }
        }
        private static void DeleteRows(this Grid grid,int count)
        {
            for (int i = 0; i < count; i++)
            {
                grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count() - 1);
               
            }
        }
        public static void SetColumns(this Grid grid, Collection<UIElement> elements, int rangeFrom)
        {
            foreach (UIElement element in elements)
            {
                Grid.SetColumn(element, rangeFrom++ - 1);
            }
        }
        public static void SetRows(this Grid grid, Collection<UIElement> elements, int rangeFrom)
        {
            foreach (UIElement element in elements)
            {
                Grid.SetRow(element, rangeFrom++ - 1);
            }
        }
        public static void RestructFromLeft(this Grid grid)//delete from left, add to right
        {
            foreach (UIElement child in grid.Children.Cast<UIElement>().ToList())
            {
                if (Grid.GetColumn(child) == 0)
                {
                    grid.Children.Remove(child);
                }
                else
                {
                    Grid.SetColumn(child, Grid.GetColumn(child)-1 );
                }
            }
        }
        public static void RestructFromRight(this Grid grid)//delete from right, add to left
        {
            foreach (UIElement child in grid.Children.Cast<UIElement>().ToList())
            {
                if (Grid.GetColumn(child) == grid.ColumnDefinitions.Count - 1)
                {
                    grid.Children.Remove(child);
                }
                else
                {
                    Grid.SetColumn(child, Grid.GetColumn(child) + 1);
                }

            }
        }
        public static void RestructFromTop(this Grid grid)//delete from top, add to bottom
        {
            foreach (UIElement child in grid.Children.Cast<UIElement>().ToList())
            {
                if (Grid.GetRow(child) == 0)
                {
                    grid.Children.Remove(child);
                }
                else
                {
                    Grid.SetRow(child, Grid.GetRow(child) - 1);
                }

            }
        }
        public static void RestructFromBottom(this Grid grid)//delete from bottom, add to top
        {
            foreach (UIElement child in grid.Children.Cast<UIElement>().ToList())
            {
                if (Grid.GetRow(child) == grid.RowDefinitions.Count - 1)
                {
                    grid.Children.Remove(child);
                }
                else
                {
                    Grid.SetRow(child, Grid.GetRow(child) + 1);
                }
            }
        }
        public static int MatchColumns(int CurrentColumns,int NeddedColumns)
        {
            return NeddedColumns - CurrentColumns;
        }
        public static int MatchRows(int CurrentRows, int NeddedRows)
        {
            return NeddedRows - CurrentRows;
        }
    }
}
