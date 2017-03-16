using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace Asteroids_Deluxe
{
    public class PlayerWing : VectorEngine.Vector
    {

        public PlayerWing(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void BeginRun()
        {
            base.BeginRun();

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[6];

            pointPosition[0] = new Vector3(-6.44f, 14.63f, 0);//Top wing tip.
            pointPosition[1] = new Vector3(1.76f, 0, 0);//Front of wing middle of ship.
            pointPosition[2] = new Vector3(-6.44f, -14.63f, 0);//Bottom wing tip.
            pointPosition[3] = new Vector3(-11.7f, -3.65f, 0);//Connect to bottom back tip.
            pointPosition[4] = new Vector3(-11.7f, 3.65f, 0);//Connect to top back tip.
            pointPosition[5] = new Vector3(-6.44f, 14.63f, 0);//Top wing tip.

            InitializePoints(pointPosition);

            Radius = 14.63f;
        }
    }
}
