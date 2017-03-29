using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    using serv = VectorEngine.Services;

    public class Dot : VectorEngine.Vector
    {
        VectorEngine.Timer m_LifeTimer;

        public Dot(Game game) : base(game)
        {
            m_LifeTimer = new VectorEngine.Timer(game);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (m_LifeTimer.Expired)
                Active = false;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void Spawn(Vector3 position, float radius)
        {
            Position = position + new Vector3(serv.RandomMinMax(-radius * 0.5f, radius * 0.5f),
                serv.RandomMinMax(-radius * 0.5f, radius * 0.5f), 0);
            Velocity = new Vector3(serv.RandomMinMax(-16, 16), serv.RandomMinMax(-16, 16), 0);
            m_LifeTimer.Amount = serv.RandomMinMax(0.1f, 1);
            Active = true;
        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[4];

            pointPosition[0] = new Vector3(-0.25f, 0.25f, 0);
            pointPosition[1] = new Vector3(0.25f, -0.25f, 0);
            pointPosition[2] = new Vector3(0.25f, 0.25f, 0);
            pointPosition[3] = new Vector3(-0.25f, -0.25f, 0);

            InitializePoints(pointPosition);
        }
    }
}
