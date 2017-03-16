using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    using serv = VectorEngine.Services;

    public class Line : VectorEngine.Vector
    {
        VectorEngine.Timer m_LifeTimer;

        public Line(Game game) : base(game)
        {
            m_LifeTimer = new VectorEngine.Timer(game);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (m_LifeTimer.Seconds > m_LifeTimer.Amount)
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
            Velocity = new Vector3(serv.RandomMinMax(-20, 20), serv.RandomMinMax(-20, 20), 0);
            RotationInRadians = serv.RandomMinMax(0, MathHelper.TwoPi);
            RotationVelocity = serv.RandomMinMax(-6, 6);
            m_LifeTimer.Reset();
            m_LifeTimer.Amount = serv.RandomMinMax(0.1f, 3);
            Active = true;
        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[2];

            pointPosition[0] = new Vector3(0, serv.RandomMinMax(0.5f, 4.75f), 0);
            pointPosition[1] = new Vector3(0, serv.RandomMinMax(-0.5f, -4.75f), 0);

            InitializePoints(pointPosition);
        }
    }
}