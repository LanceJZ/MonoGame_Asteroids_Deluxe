using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    public class PlayerShip : VectorEngine.Vector
    {
        public PlayerShip(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void BeginRun()
        {
            base.BeginRun();
            Moveable = false;
        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[6];

            pointPosition[0] = new Vector3(-13.5f, 8.2f, 0);//Top back tip. (10.17 * Xenko numbers.)
            pointPosition[1] = new Vector3(13.5f, 0, 0);//Nose pointing to the right of screen.
            pointPosition[2] = new Vector3(-13.5f, -8.2f, 0);//Bottom back tip.
            pointPosition[3] = new Vector3(-10.6f, -4.7f, 0);//Bottom inside back.
            pointPosition[4] = new Vector3(-10.6f, 4.7f, 0);//Top inside back.
            pointPosition[5] = new Vector3(-13.5f, 8.2f, 0);//Top Back Tip.

            InitializePoints(pointPosition);

            Radius = 13.5f;
        }
    }
}
