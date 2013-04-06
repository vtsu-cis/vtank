using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.util.game
{
    /// <summary>
    /// The TracerBullet class's job is to attach what looks like a trailing path to
    /// projectiles. This is especially used for bullets that are difficult to see, such
    /// as a machine gun's bullets.
    /// </summary>
    public class TracerBullet
    {
        #region Members

        private static int[] indicies = new int[2] { 0, 1 };
        //private static BasicEffect effect;
        private static VertexDeclaration declaration;
        private readonly static int length = 18;

        private VertexPositionColor[] pointList = new VertexPositionColor[2];
        private Vector3 colorf;

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the position of the tracer path.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the angle of the tracer path.
        /// </summary>
        public double Angle
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the scale of the tracer path.
        /// </summary>
        public float Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Get or set the color of the tracer path.
        /// </summary>
        public Color Color
        {
            get;
            set;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new tracer bullet with all of the properties of the origianl bullet.
        /// Uses a default trail color of white.
        /// </summary>
        /// <param name="_position">Position of the tracer bullet.</param>
        /// <param name="_angle">Angle in which the tracer bullet moves.</param>
        /// <param name="_scale">Scale of the bullet (i.e. thickness).</param>
        public TracerBullet(ref Vector3 _position, double _angle, float _scale)
            : this(ref _position, _angle, _scale, new Color(1.0f, 0.85f, 0.6f, 1.0f))
        {
        }

        /// <summary>
        /// Create a new tracer bullet with all of the properties of the origianl bullet.
        /// </summary>
        /// <param name="_position">Position of the tracer bullet.</param>
        /// <param name="_angle">Angle in which the tracer bullet moves.</param>
        /// <param name="_scale">Scale of the bullet (i.e. thickness).</param>
        /// <param name="_color">Color of the trail.</param>
        public TracerBullet(ref Vector3 _position, double _angle, float _scale, Color _color)
        {
            Position = _position;
            Angle    = _angle;
            Scale    = _scale;
            Color    = _color;

            Initialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs line list calculations and other necessary initializations.
        /// </summary>
        private void Initialize()
        {
            colorf = Color.ToVector3();
            
            pointList[0] = new VertexPositionColor(
                new Vector3(Position.X, Position.Y, Position.Z), Color);
            pointList[1] = new VertexPositionColor(
                new Vector3(Position.X - (float)(Math.Cos(Angle) * length),
                            Position.Y - (float)(Math.Sin(Angle) * length), 
                            Position.Z), Color);
        }

        /// <summary>
        /// Update the tracer bullet, which primarily updates the vertices.
        /// </summary>
        public void Update(Vector3 position)
        {
            Position = position;

            pointList[0].Position.X = Position.X;
            pointList[0].Position.Y = Position.Y;
            pointList[0].Position.Z = Position.Z;
            
            pointList[1].Position.X = Position.X - (float)(Math.Cos(Angle) * length);
            pointList[1].Position.Y = Position.Y - (float)(Math.Sin(Angle) * length);
            pointList[1].Position.Z = Position.Z;
        }

        /// <summary>
        /// Draw the tracer bullet.
        /// </summary>
        public void Draw()
        {
            /*GraphicsDevice device       = Program.GameObject.GraphicsDevice;
            device.VertexDeclaration    = declaration;

            effect.DiffuseColor = colorf;

            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                    pointList, 0, 2, indicies, 0, 1);

                pass.End();
            }

            effect.End();*/
        }

        #endregion
    }
}
