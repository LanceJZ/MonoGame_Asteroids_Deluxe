using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    public class PlayerShip : VectorEngine.Vector
    {
        PlayerWing m_Wing;

        public PlayerShip(Game game) : base(game)
        {
            m_Wing = new PlayerWing(game);
            AddChild(m_Wing, true, true);
        }

        public override void Initialize()
        {
            Active = false;
            Moveable = false;
            base.Initialize();
        }

        public override void BeginRun()
        {
            base.BeginRun();
            Radius = m_Wing.Radius;
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
        }
    }
}
