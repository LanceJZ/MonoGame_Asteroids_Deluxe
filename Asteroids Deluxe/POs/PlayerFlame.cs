using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    public class PlayerFlame : VectorEngine.Vector
    {
        public PlayerFlame(Game game) : base(game)
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
            Vector3[] pointPosition = new Vector3[4];

            pointPosition[0] = new Vector3(-9, -4, 0);//Bottom inside back.
            pointPosition[1] = new Vector3(-17.5f, 0, 0);//Tip of flame.
            pointPosition[2] = new Vector3(-9, 4, 0);//Top inside back.
            pointPosition[3] = new Vector3(-17.5f, 0, 0);//Tip of flame.

            InitializePoints(pointPosition);
        }
    }
}
