using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;

    class Rock : VectorEngine.Vector
    {
        Player m_Player;
        UFO m_UFO;
        Explode m_Explosion;
        SoundEffect m_Explode;
        int m_Points = 20;
        float m_Speed = 75;

        public bool ExplosionDone
        {
            get
            {
                return m_Explosion.Active;
            }
        }

        public Rock(Game game) : base(game)
        {
            m_Explosion = new Explode(game);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (Active)
            {
                base.Update(gameTime);
                CheckBorders();
                CheckCollusions();
            }
        }

        public void LoadSound(SoundEffect explode)
        {
            m_Explode = explode;
        }

        protected override void InitializeLineMesh()
        {
            int rockType = (int)Serv.RandomMinMax(0, 3.99f);

            switch (rockType)
            {
                case 0:
                    RockOne();
                    break;
                case 1:
                    RockTwo();
                    break;
                case 2:
                    RockThree();
                    break;
                case 3:
                    RockFour();
                    break;
            }
        }

        void CheckCollusions()
        {
            if (m_Player.Active)
            {
                if (!m_Player.Shield.Active)
                {
                    if (CirclesIntersect(m_Player.Position, m_Player.Radius))
                    {
                        Explode();
                        m_Player.Hit = true;
                        m_Player.SetScore(m_Points);
                    }
                }
                else
                {
                    if (CirclesIntersect(m_Player.Shield.Position, m_Player.Shield.Radius))
                    {
                        m_Player.ShieldHit(Position, Velocity);
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (m_Player.Shots[i].Active)
                {
                    if (CirclesIntersect(m_Player.Shots[i].Position, m_Player.Shots[i].Radius))
                    {
                        Explode();
                        m_Player.Shots[i].Active = false;
                        m_Player.SetScore(m_Points);
                    }
                }
            }

            if (m_UFO.Active)
            {
                if (CirclesIntersect(m_UFO.Position, m_UFO.Radius))
                {
                    Explode();
                    m_UFO.Explode();
                }

                if (m_UFO.Shot.Active)
                {
                    if (CirclesIntersect(m_UFO.Shot.Position, m_UFO.Shot.Radius))
                    {
                        Explode();
                        m_UFO.Shot.Active = false;
                    }
                }
            }
        }

        public void Spawn(Vector3 position, float scale, float speed, int points, Player player, UFO ufo)
        {
            Scale = scale;
            Radius = Radius * scale;
            m_Points = points;
            m_Speed = speed;
            m_Player = player;
            m_UFO = ufo;
            Spawn(position);
        }

        public void Spawn(Vector3 position)
        {
            Position = position;
            Spawn();
        }

        public void Spawn(Player player, UFO ufo)
        {
            m_Player = player;
            m_UFO = ufo;
            Position = Serv.SetRandomEdge();
            Spawn();
        }

        public void Spawn()
        {
            Active = true;
            Velocity = Serv.SetRandomVelocity(m_Speed);
            RotationVelocity = Serv.RandomMinMax(-MathHelper.PiOver4, MathHelper.PiOver4);
        }

        void Explode()
        {
            if (!m_Player.GameOver)
                m_Explode.Play(0.15f, 0, 0);

            m_Explosion.Spawn(Position, Radius);
            Hit = true;
        }

        void RockOne()
        {
            Vector3[] pointPosition = new Vector3[13];

            //Rock One
            pointPosition[0] = new Vector3(-34, 17.6f, 0);
            pointPosition[1] = new Vector3(-17.6f, 35.2f, 0);
            pointPosition[2] = new Vector3(0, 25.8f, 0);
            pointPosition[3] = new Vector3(17.6f, 35.2f, 0);
            pointPosition[4] = new Vector3(24, 17.6f, 0);
            pointPosition[5] = new Vector3(17.6f, 8.2f, 0);
            pointPosition[6] = new Vector3(34, -8.2f, 0);
            pointPosition[7] = new Vector3(17.6f, -35.2f, 0);
            pointPosition[8] = new Vector3(-8.2f, -21, 0);
            pointPosition[9] = new Vector3(-17.6f, -35.2f, 0);
            pointPosition[10] = new Vector3(-34, -17.6f, 0);
            pointPosition[11] = new Vector3(-24.7f, 0, 0);
            pointPosition[12] = new Vector3(-34, 17.6f, 0);

            Radius = InitializePoints(pointPosition);
        }

        void RockTwo()
        {
            Vector3[] pointPosition = new Vector3[11];

            //Rock One
            pointPosition[0] = new Vector3(-34, 17.6f, 0);
            pointPosition[1] = new Vector3(-16.4f, 34, 0);
            pointPosition[2] = new Vector3(0, 17.6f, 0);
            pointPosition[3] = new Vector3(17.6f, 34, 0);
            pointPosition[4] = new Vector3(34, 17.6f, 0);
            pointPosition[5] = new Vector3(25.8f, 0, 0);
            pointPosition[6] = new Vector3(34, -17.6f, 0);
            pointPosition[7] = new Vector3(8.2f, -34, 0);
            pointPosition[8] = new Vector3(-16.4f, -34, 0);
            pointPosition[9] = new Vector3(-34, -16.4f, 0);
            pointPosition[10] = new Vector3(-34, 17.6f, 0);

            Radius = InitializePoints(pointPosition);
        }

        void RockThree()
        {
            Vector3[] pointPosition = new Vector3[13];

            //Rock One
            pointPosition[0] = new Vector3(-34, 17.6f, 0);
            pointPosition[1] = new Vector3(-8.2f, 17.6f, 0);
            pointPosition[2] = new Vector3(-18.8f, 34, 0);
            pointPosition[3] = new Vector3(9.4f, 34, 0);
            pointPosition[4] = new Vector3(34, 17.6f, 0);
            pointPosition[5] = new Vector3(34, 9.4f, 0);
            pointPosition[6] = new Vector3(9.4f, 0, 0);
            pointPosition[7] = new Vector3(34, -16.4f, 0);
            pointPosition[8] = new Vector3(16.4f, -32.9f, 0);
            pointPosition[9] = new Vector3(8.2f, -24.7f, 0);
            pointPosition[10] = new Vector3(-17.6f, -24, 0);
            pointPosition[11] = new Vector3(-34, -9.4f, 0);
            pointPosition[12] = new Vector3(-34, 17.6f, 0);

            Radius = InitializePoints(pointPosition);
        }

        void RockFour()
        {
            Vector3[] pointPosition = new Vector3[13];

            //Rock One
            pointPosition[0] = new Vector3(-34, 9.4f, 0);
            pointPosition[1] = new Vector3(-7, 34, 0);
            pointPosition[2] = new Vector3(17.6f, 34, 0);
            pointPosition[3] = new Vector3(35.2f, 8.2f, 0);
            pointPosition[4] = new Vector3(35.2f, -8.2f, 0);
            pointPosition[5] = new Vector3(18.8f, -34, 0);
            pointPosition[6] = new Vector3(16.4f, -34, 0);
            pointPosition[7] = new Vector3(0, -34, 0);
            pointPosition[8] = new Vector3(0, -9.4f, 0);
            pointPosition[9] = new Vector3(-16.4f, -32.9f, 0);
            pointPosition[10] = new Vector3(-34, -8.2f, 0);
            pointPosition[11] = new Vector3(-17.6f, 0, 0);
            pointPosition[12] = new Vector3(-34, 9.4f, 0);

            Radius = InitializePoints(pointPosition);
        }
    }
}
