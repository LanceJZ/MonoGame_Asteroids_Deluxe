#region Using
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Asteroids_Deluxe.VectorEngine
{
    public sealed class Services : DrawableGameComponent
    {
        #region Fields
        private static Services m_Instance = null;
        private static GraphicsDeviceManager m_GraphicsDM;
        private static BasicEffect m_BasicEffect;
        private static Random m_RandomNumber;
        private static Matrix m_ViewMatrix;
        private static Matrix m_ProjectionMatrix;
        private static Matrix m_WorldMatrix;
        private static Vector2 m_ScreenSize;
        private static List<IDrawComponent> m_DrawableComponents;
        #endregion
        #region Properties
        /// <summary>
        /// This is used to get the Services Instance
        /// Instead of using the mInstance this will do the check to see if the Instance is valid
        /// where ever you use it. It is also private so it will only get used inside the engine services.
        /// </summary>
        private static Services Instance
        {
            get
            {
                //Make sure the Instance is valid
                if (m_Instance != null)
                {
                    return m_Instance;
                }

                throw new InvalidOperationException("The Engine Services have not been started!");
            }
        }

        public static GraphicsDeviceManager GraphicsDM
        {
            get { return m_GraphicsDM; }
        }

        public static Random RandomNumber
        {
            get { return m_RandomNumber; }
        }

        public static Matrix ViewMatrix
        {
            get { return m_ViewMatrix; }
        }

        public static Matrix ProjectionMatrix
        {
            get { return m_ProjectionMatrix; }
        }

        public static void AddDrawableComponent(IDrawComponent drawableComponent)
        {
            m_DrawableComponents.Add(drawableComponent);

        }

        /// <summary>
        /// Get a random float between min and max
        /// </summary>
        /// <param name="min">the minimum random value</param>
        /// <param name="max">the maximum random value</param>
        /// <returns>float</returns>
        public static float RandomMinMax(float min, float max)
        {
            return min + (float)RandomNumber.NextDouble() * (max - min);
        }

        /// <summary>
        /// Returns the window size in pixels, of the height.
        /// </summary>
        /// <returns>int</returns>
        public static int WindowHeight { get { return m_GraphicsDM.PreferredBackBufferHeight; } }

        /// <summary>
        /// Returns the window size in pixels, of the width.
        /// </summary>
        /// <returns>int</returns>
        public static int WindowWidth { get { return m_GraphicsDM.PreferredBackBufferWidth; } }

        public static Vector2 ScreenSize { get { return m_ScreenSize; } set { m_ScreenSize = value; } }

        public static Matrix WorldMatrix
        {
            get
            {
                return m_WorldMatrix;
            }

            set
            {
                m_WorldMatrix = value;
            }
        }

        public static BasicEffect BasicEffect
        {
            get
            {
                return m_BasicEffect;
            }

            set
            {
                m_BasicEffect = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// This is the constructor for the Services
        /// You will note that it is private that means that only the Services can only create itself.
        /// </summary>
        private Services(Game game) : base(game)
        {
            game.Components.Add(this);
        }
        #endregion

        #region Public Methods

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);


            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            foreach (IDrawComponent drawable in m_DrawableComponents)
            {
                drawable.Draw(gameTime);
            }

        }
        /// <summary>
        /// This is used to start up Panther Engine Services.
        /// It makes sure that it has not already been started if it has been it will throw and exception
        /// to let the user know.
        ///
        /// You pass in the game class so you can get information needed.
        /// </summary>
        /// <param name="graphics">Reference to the graphic device.</param>
        public static void Initialize(GraphicsDeviceManager graphics, Game game)
        {
            //First make sure there is not already an instance started
            if (m_Instance == null)
            {
                m_GraphicsDM = graphics;
                m_ScreenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                //Create the Engine Services
                m_Instance = new Services(game);
                m_RandomNumber = new Random(DateTime.Now.Millisecond);
                //Set View Matrix and Projection Matrix
                m_ViewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1f), Vector3.Zero, Vector3.UnitY);
                m_ProjectionMatrix = Matrix.CreateOrthographic(m_ScreenSize.X, m_ScreenSize.Y, 1, 2);
                BasicEffect = new BasicEffect(graphics.GraphicsDevice);
                BasicEffect.VertexColorEnabled = true;
                BasicEffect.View = m_ViewMatrix;
                BasicEffect.Projection = m_ProjectionMatrix;
                BasicEffect.World = WorldMatrix;
                m_WorldMatrix = Matrix.CreateTranslation(0, 0, 0);
                m_DrawableComponents = new List<IDrawComponent>();

                return;
            }

            throw new Exception("The Engine Services have already been started.");
        }
        /// <summary>
        /// Returns a Vector3 direction of travel from angle and magnitude.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="magnitude"></param>
        /// <returns>Vector3</returns>
        public static Vector3 SetVelocity(float angle, float magnitude)
        {
            Vector3 Vector = new Vector3(0);
            Vector.Y = (float)(Math.Sin(angle) * magnitude);
            Vector.X = (float)(Math.Cos(angle) * magnitude);
            return Vector;
        }

        /// <summary>
        /// Returns a float of the angle in radians derived from two Vector3 passed into it, using only the X and Y.
        /// </summary>
        /// <param name="origin">Vector3 of origin</param>
        /// <param name="target">Vector3 of target</param>
        /// <returns>Float</returns>
        public static float AngleFromVectors(Vector3 origin, Vector3 target)
        {
            return (float)(Math.Atan2(target.Y - origin.Y, target.X - origin.X));
        }

        public static float RandomRadian()
        {
            return RandomMinMax(0, (float)Math.PI * 2);
        }

        public static Vector3 SetRandomVelocity(float speed)
        {
            float ang = RandomRadian();
            float amt = RandomMinMax(speed * 0.15f, speed);
            return VelocityFromAngle(ang, amt);
        }

        public static Vector3 SetRandomVelocity(float speed, float radianDirection)
        {
            float amt = RandomMinMax(speed * 0.15f, speed);
            return VelocityFromAngle(radianDirection, amt);
        }

        public static Vector3 VelocityFromAngle(float rotation, float magnitude)
        {
            return new Vector3((float)Math.Cos(rotation) * magnitude, (float)Math.Sin(rotation) * magnitude, 0);
        }

        public static Vector3 SetVelocityFromAngle(float magnitude)
        {
            float ang = RandomRadian();
            return new Vector3((float)Math.Cos(ang) * magnitude, (float)Math.Sin(ang) * magnitude, 0);
        }

        public static Vector3 SetRandomEdge()
        {
            return new Vector3(WindowWidth * 0.5f, RandomMinMax(-WindowHeight * 0.45f, WindowHeight * 0.45f), 0);
        }

        public static float AimAtTarget(Vector3 origin, Vector3 target, float facingAngle, float magnitude)
        {
            float turnVelocity = 0;
            float targetAngle = AngleFromVectors(origin, target);
            float targetLessFacing = targetAngle - facingAngle;
            float facingLessTarget = facingAngle - targetAngle;

            if (Math.Abs(targetLessFacing) > Math.PI)
            {
                if (facingAngle > targetAngle)
                {
                    facingLessTarget = ((MathHelper.TwoPi - facingAngle) + targetAngle) * -1;
                }
                else
                {
                    facingLessTarget = (MathHelper.TwoPi - targetAngle) + facingAngle;
                }
            }

            if (facingLessTarget > 0)
            {
                turnVelocity = -magnitude;
            }
            else
            {
                turnVelocity = magnitude;
            }

            return turnVelocity;
        }
        #endregion
    }
}
