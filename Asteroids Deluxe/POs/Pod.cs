using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;

    public class Pod : VectorEngine.Vector
    {
        Player m_Player;
        UFO m_UFO;
        float m_RotateMagnitude = MathHelper.PiOver2;
        int m_Score = 200;
        bool m_NewWave;

        public bool NewWave
        {
            set
            {
                m_NewWave = value;
            }
        }

        public Pod(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

        }

        public void Initialize(Player player, UFO ufo)
        {
            m_Player = player;
            m_UFO = ufo;
            Active = false;
        }

        public override void BeginRun()
        {
            base.BeginRun();

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active & Moveable)
            {
                if (m_Player.Active && !m_Player.Hit && !m_NewWave)
                    RotationVelocity = Serv.AimAtTarget(Position, m_Player.Position, RotationInRadians, m_RotateMagnitude);
                else
                {
                    RotationVelocity = 0;

                    if (m_UFO.Active)
                    {
                        RotationVelocity = Serv.AimAtTarget(Position, m_UFO.Position, RotationInRadians, m_RotateMagnitude);
                    }
                }

                if (m_NewWave)
                {
                    if (Position.X > Serv.WindowWidth * 0.5f || Position.X < -Serv.WindowWidth * 0.5f
                        || Position.Y > Serv.WindowHeight * 0.5f || Position.Y < -Serv.WindowHeight * 0.5f)
                    {
                        Active = false;
                    }

                    RotationVelocity = 0;
                }

                Velocity = Serv.SetVelocityFromAngle(RotationInRadians, 70);
                CheckCollision();
            }
        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[5];

            pointPosition[0] = new Vector3(-11.7f, 14.04f, 0);//Top back tip.
            pointPosition[1] = new Vector3(11.7f, 0, 0);//Nose pointing to the right of screen.
            pointPosition[2] = new Vector3(-11.7f, -14.04f, 0);//Bottom Back tip.
            pointPosition[3] = new Vector3(-4.68f, 0, 0);//Back inside indent.
            pointPosition[4] = new Vector3(-11.7f, 14.04f, 0);//Top Back Tip.

            Radius = InitializePoints(pointPosition);
        }

        public bool CheckPlayerShotCollision()
        {
            for (int shot = 0; shot < 4; shot++)
            {
                if (m_Player.Shots[shot].Active)
                {
                    if (CirclesIntersect(m_Player.Shots[shot].Position, m_Player.Shots[shot].Radius))
                    {
                        m_Player.Shots[shot].Active = false;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CheckPlayerCollision()
        {
            if (m_Player.Active && !m_Player.Hit)
            {
                if (m_Player.Shield.Active)
                {
                    if (CirclesIntersect(m_Player.Position, m_Player.Shield.Radius))
                    {
                        return true;
                    }
                }
                else if (CirclesIntersect(m_Player.Position, m_Player.Radius))
                {
                    m_Player.Hit = true;
                    return true;
                }
            }

            return false;
        }

        public bool CheckUFOCollision()
        {
            if (m_UFO.Active)
            {
                if (CirclesIntersect(m_UFO.Position, m_UFO.Radius))
                {
                    m_UFO.Explode();
                    return true;
                }
            }

            return false;
        }

        public bool CheckUFOShotCollision()
        {
            if (m_UFO.Shot.Active)
            {
                if (CirclesIntersect(m_UFO.Shot.Position, m_UFO.Shot.Radius))
                {
                    m_UFO.Shot.Active = false;
                    return true;
                }
            }

            return false;
        }

        void CheckCollision()
        {
            if (CheckPlayerCollision())
            {
                if (m_Player.Shield.Active)
                {
                    m_Player.ShieldHit(Position, Velocity);
                }
                else
                {
                    m_Player.SetScore(m_Score);
                    Active = false;
                    Moveable = false;
                }
            }

            if (CheckPlayerShotCollision())
            {
                m_Player.SetScore(m_Score);
                Active = false;
                Moveable = false;
            }

            if (CheckUFOCollision() || CheckUFOShotCollision())
            {
                Active = false;
                Moveable = false;
            }
        }
    }
}
