using System;
using System.Collections.Generic;
using System.Text;

namespace MinotaurPathfinder
{
    public enum SquareContent
    {
        Starting,
        Empty,
        Wall
    };

    public struct MetaCompleteSquare
    {
        public CompleteSquare Square;
        public int X;
        public int Y;

        public MetaCompleteSquare(CompleteSquare _square, int x, int y)
        {
            Square = _square;
            X = x;
            Y = y;
        }
    }

    public struct CompleteSquare
    {
        public SquareContent ContentCode;
        public int DistanceSteps;
        public bool IsPath;
        public int X;
        public int Y;

        public CompleteSquare(int x, int y, int distanceSteps, bool isPath, SquareContent content)
        {
            X = x;
            Y = y;
            DistanceSteps = distanceSteps;
            IsPath = isPath;
            ContentCode = content;
        }
    }
}
