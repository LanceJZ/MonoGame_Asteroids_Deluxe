using Microsoft.Xna.Framework;
using System;

namespace Asteroids_Deluxe
{
    public class Shield : VectorEngine.Vector
    {

        public Shield(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

        }

        public override void BeginRun()
        {
            base.BeginRun();

            Active = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[9];

            float radius = 20.48f;
            float rotation = MathHelper.Pi / 8;

            for (int i = 0; i < 8; i++)
            {
                pointPosition[i] = new Vector3(radius * (float)Math.Cos(2 * MathHelper.Pi * i / 8 + rotation),
                    radius * (float)Math.Sin(2 * MathHelper.Pi * i / 8 + rotation), 0);
            }

            pointPosition[8] = pointPosition[0];

            Radius = InitializePoints(pointPosition);
        }
    }
}
