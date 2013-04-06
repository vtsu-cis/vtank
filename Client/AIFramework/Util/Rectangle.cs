using System;

namespace AIFramework.Util
{
    public class Rectangle
    {
        #region Properties
        public int X  { get; set; }
        public int Y  { get; set; }
        public int Width  { get; set; }
        public int Height { get; set; }
        #endregion

        #region Constructors
        public Rectangle()
            : this(0, 0, 0, 0) { }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        #endregion

        #region Methods
        public bool CollidesWith(Rectangle other)
        {
            decimal left1 = X;
            decimal left2 = other.X;
            decimal right1 = X + Width;
            decimal right2 = other.X + other.Width;
            decimal top1 = Y;
            decimal top2 = other.Y;
            decimal bottom1 = Y + Height;
            decimal bottom2 = other.Y + other.Height;

            if (bottom1 < top2 || top1 > bottom2 || right1 < left2 || left1 > right2)
                return false;

            return true;
        }

        public bool CollidesWith(Circle circle)
        {
            return circle.CollidesWith(this);
        }
        #endregion
    }
}
