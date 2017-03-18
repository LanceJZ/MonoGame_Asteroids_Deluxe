using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;
    using Timer = VectorEngine.Timer;
    /// <summary>
    /// As the player's score increases, the angle range of the shots from the small saucer diminishes
    /// until the saucer fires extremely accurately.
    /// The small saucer will fire immediately when spawned. (Revision 3 of original arcade.)
    ///
    /// For Deluxe version Large UFO aims at player 1 out of 3 shots. Small UFO aims at player 3 out of 4.
    /// </summary>
    public class UFO : VectorEngine.Vector
    {
        Player m_Player;
        Shot m_Shot;
        Explode m_Explosion;
        Timer m_ShotTimer;
        Timer m_VectorTimer;
        Timer m_LargeTimer;
        Timer m_SmallTimer;
        SoundEffect m_Explode;
        SoundEffect m_Large;
        SoundEffect m_Small;
        SoundEffect m_FireShot;
        float m_Speed = 66;
        float m_Radius;
        int m_Points;
        int m_PlayerScore;
        bool m_SmallSoucer;
        bool m_Done;

        public int PlayerScore { set { m_PlayerScore = value; } }

        public bool Done
        {
            get
            {
                return m_Done;
            }

            set
            {
                m_Done = value;
            }
        }

        public Shot Shot
        {
            get
            {
                return m_Shot;
            }

            set
            {
                m_Shot = value;
            }
        }

        public UFO(Game game) : base(game)
        {
            m_ShotTimer = new Timer(game);
            m_VectorTimer = new Timer(game);
            m_LargeTimer = new Timer(game);
            m_SmallTimer = new Timer(game);
            m_Shot = new Shot(game);
            m_Explosion = new Explode(game);
        }

        public void Initialize(Player player)
        {
            m_Player = player;
            Active = false;
            m_ShotTimer.Amount = 2.75f;
            m_VectorTimer.Amount = 3.15f;
        }

        public override void Initialize()
        {
            base.Initialize();
            Position.X = Serv.WindowWidth;
        }

        public void LoadSounds(SoundEffect explode, SoundEffect shot, SoundEffect large, SoundEffect small)
        {
            m_Explode = explode;
            m_FireShot = shot;
            m_Large = large;
            m_Small = small;

            m_LargeTimer.Amount = (float)m_Large.Duration.TotalSeconds;
            m_SmallTimer.Amount = (float)m_Small.Duration.TotalSeconds;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active)
            {
                if (!m_Player.GameOver)
                {
                    if (m_SmallSoucer)
                    {
                        if (m_SmallTimer.Seconds > m_SmallTimer.Amount)
                        {
                            m_SmallTimer.Reset();
                            m_Small.Play(0.5f, 0, 0);
                        }
                    }
                    else
                    {
                        if (m_LargeTimer.Seconds > m_LargeTimer.Amount)
                        {
                            m_LargeTimer.Reset();
                            m_Large.Play(0.5f, 0, 0);
                        }
                    }
                }

                if (Position.X > Serv.WindowWidth * 0.5f || Position.X < -Serv.WindowWidth * 0.5f)
                {
                    Done = true;
                }

                CheckBorders();
                TimeToChangeVectorYet();
                TimeToShotYet();
            }

            CheckColusion();
        }

        public void Spawn(int SpawnCount, int Wave)
        {
            Active = true;
            Done = false;
            Hit = false;
            m_ShotTimer.Reset();
            m_VectorTimer.Reset();

            float spawnPercent = (float)(Math.Pow(0.915, (SpawnCount) / (Wave + 1)));

            if (Serv.RandomMinMax(0, 99) < (m_PlayerScore / 400) + (spawnPercent * 100) || m_PlayerScore > 20000)
            {
                m_SmallSoucer = false;
                m_Points = 200;
                Scale = 1;
            }
            else
            {
                m_SmallSoucer = true;
                m_Points = 1000;
                Scale = 0.5f;
            }

            Radius = m_Radius * Scale;
            Position.Y = Serv.RandomMinMax(-Serv.WindowHeight * 0.25f, Serv.WindowHeight * 0.25f);

            if (Serv.RandomMinMax(0, 10) > 5)
            {
                Position.X = -Serv.WindowWidth * 0.5f + 1;
                Velocity.X = m_Speed;
            }
            else
            {
                Position.X = Serv.WindowWidth * 0.5f - 1;
                Velocity.X = -m_Speed;
            }
        }

        public void Explode()
        {
            if (!m_Player.GameOver)
                m_Explode.Play();

            m_Explosion.Spawn(Position, Radius);
            Hit = true;
        }

        void CheckColusion()
        {
            if (m_Player.Active)
            {
                if (Active)
                {
                    if (m_Player.Shield.Active)
                    {
                        if (CirclesIntersect(m_Player.Position, m_Player.Shield.Radius))
                        {
                            m_Player.ShieldHit(Position, Velocity);
                        }
                    }
                    else if (CirclesIntersect(m_Player.Position, m_Player.Radius))
                    {
                        Explode();
                        m_Player.Hit = true;
                        m_Player.SetScore(m_Points);
                    }
                }

                if (m_Shot.Active)
                {
                    if (m_Player.Shield.Active)
                    {
                        if (m_Shot.CirclesIntersect(m_Player.Position, m_Player.Shield.Radius))
                        {
                            m_Player.ShieldHit();
                            m_Shot.Active = false;
                        }
                    }
                    else if (m_Shot.CirclesIntersect(m_Player.Position, m_Player.Radius))
                    {
                        m_Shot.Active = false;
                        m_Player.Hit = true;
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
        }

        void TimeToShotYet()
        {
            if (m_ShotTimer.Seconds > m_ShotTimer.Amount)
            {
                FireShot();
            }
        }

        void TimeToChangeVectorYet()
        {
            if (m_VectorTimer.Seconds > m_VectorTimer.Amount)
            {
                ChangeVector();
            }
        }

        void FireShot()
        {
            if (!m_Player.GameOver)
                m_FireShot.Play(0.4f, 0, 0);

            m_ShotTimer.Reset();
            float speed = 400;
            float rad = 0;

            //Adjust accuracy according to score. By the time the score reaches 30,000, percent = 0.
            float percent = 0.25f - (m_PlayerScore * 0.00001f);

            if (!m_SmallSoucer)
            {
                if (Serv.RandomMinMax(0, 4) > 3)
                    rad = Serv.RandomRadian();
                else
                    rad = Serv.AngleFromVectors(Position, m_Player.Position) + Serv.RandomMinMax(-percent, percent);

            }
            else
            {
                if (percent < 0)
                    percent = 0;

                rad = Serv.AngleFromVectors(Position, m_Player.Position) + Serv.RandomMinMax(-percent, percent);
            }

            Vector3 dir = Serv.SetVelocity(rad, speed);
            Vector3 offset = Serv.SetVelocity(rad, Radius);
            m_Shot.Spawn(Position + offset, dir + Velocity * 0.25f, 1.15f);
        }

        void ChangeVector()
        {
            m_VectorTimer.Reset();
            float vChange = Serv.RandomMinMax(0, 9);

            if (vChange < 5)
            {
                if ((int)Velocity.Y == 0 && vChange < 2.5)
                    Velocity.Y = m_Speed;
                else if ((int)Velocity.Y == 0)
                    Velocity.Y = -m_Speed;
                else
                    Velocity.Y = 0;
            }
        }

        protected override void InitializeLineMesh()
        {
            Vector3[] pointPosition = new Vector3[12];

            pointPosition[0] = new Vector3(8.2f, 4.7f, 0);// Upper left
            pointPosition[1] = new Vector3(22.3f, -4.7f, 0);// Lower inside Left
            pointPosition[2] = new Vector3(9.4f, -12.9f, 0);// Bottom left
            pointPosition[3] = new Vector3(-9.4f, -12.9f, 0);// Bottom right
            pointPosition[4] = new Vector3(-22.3f, -4.7f, 0);// Upper right
            pointPosition[5] = new Vector3(-8.2f, 4.7f, 0);// Lower inside right
            pointPosition[6] = new Vector3(-3.5f, 12.9f, 0);// Right Top
            pointPosition[7] = new Vector3(3.5f, 12.9f, 0);// Left Top
            pointPosition[8] = new Vector3(8.2f, 4.7f, 0); // Upper inside right
            pointPosition[9] = new Vector3(-8.2f, 4.7f, 0);// Upper inside left
            pointPosition[10] = new Vector3(-22.3f, -4.7f, 0);// Lower inside left
            pointPosition[11] = new Vector3(22.3f, -4.7f, 0);// Lower inside Right

            m_Radius = InitializePoints(pointPosition);
        }
    }
}
