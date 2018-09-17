using System;

namespace AIFramework.Util
{
    public class Circle
    {
        #region Properties
        public double X { get; set; }
        public double Y { get; set; }
        public float Radius { get; set; }
        #endregion

        #region Constructors
        public Circle()
            : this(0, 0, 0) { }

        public Circle(double x, double y, float radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }
        #endregion

        #region Methods
        public bool CollidesWith(Rectangle rect)
        {
            Circle circle = this;
            double half_width = rect.Width / 2.0;
            double half_height = rect.Height / 2.0;

            double distance_x = Math.Abs(circle.X - rect.X - half_width);
            double distance_y = Math.Abs(circle.Y - rect.Y - half_height);

            // Quick rule-out checks.
            if (distance_x > half_width + Radius || distance_y > half_height + Radius)
                return false;

            if (distance_x <= half_width || distance_y <= half_height)
                return true;

            // Measure the distance between the corner and the circle.
            return Math.Sqrt(Math.Pow(distance_x - half_width, 2) +
                Math.Pow(distance_y - half_height, 2)) <=Radius;
        }
        #endregion
    }
}
