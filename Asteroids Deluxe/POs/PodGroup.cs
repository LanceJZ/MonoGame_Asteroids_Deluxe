using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;

    public class PodGroup : VectorEngine.PositionedObject
    {
        PodPair[] m_PodPair = new PodPair[3];
        Player m_Player;
        UFO m_UFO;
        int m_Score = 50;
        bool m_NewWave = false;
        bool m_Done = false;

        public PodPair[] PodPair
        {
            get { return m_PodPair; }
            set { m_PodPair = value; }
        }

        public int Score
        {
            get { return m_Score; }
        }

        public bool Done
        {
            get { return m_Done; }
        }

        public PodGroup(Game game) : base(game)
        {
            for (int i = 0; i < 3; i++)
            {
                m_PodPair[i] = new PodPair(game);
            }

        }

        public override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < 3; i++)
            {
                AddChild(m_PodPair[i], false, false);
            }
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

            for (int i = 0; i < 3; i++)
            {
                m_PodPair[i].Moveable = false;
                m_PodPair[i].Initialize(m_Player, m_UFO);
                m_PodPair[i].BeginRun();
            }

            Radius = 25.68f;

            m_PodPair[1].ReletiveRotation = MathHelper.Pi * 0.33333f;
            m_PodPair[2].ReletiveRotation = -MathHelper.Pi * 0.33333f;
            m_PodPair[0].ReletivePosition = new Vector3(0, 9.666f, 0);
            m_PodPair[1].ReletivePosition = new Vector3(11.7f, -9.666f, 0);
            m_PodPair[2].ReletivePosition = new Vector3(-11.7f, -9.666f, 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active)
            {
                if (m_NewWave)
                {
                    if (Position.X > Serv.WindowWidth * 0.5f || Position.X < -Serv.WindowWidth * 0.5f
                        || Position.Y > Serv.WindowHeight * 0.5f || Position.Y < -Serv.WindowHeight * 0.5f)
                    {
                        Active = false;
                    }
                }
                else
                {
                    CheckBorders();
                }
            }

            if (!m_Done)
            {
                for (int pair = 0; pair < 3; pair++)
                {
                    if (m_PodPair[pair].Active)
                        return;

                    for (int pod = 0; pod < 2; pod++)
                    {
                        if (m_PodPair[pair].Pods[pod].Active)
                            return;
                    }
                }

                Reset();
            }
        }

        public void NewWave()
        {
            m_NewWave = true;

            for (int pair = 0; pair < 3; pair++)
            {
                m_PodPair[pair].NewWave = true;

                for (int pod = 0; pod < 2; pod++)
                {
                    m_PodPair[pair].Pods[pod].NewWave = true;
                }
            }
        }

        public void Spawn()
        {
            Velocity = Serv.SetVelocityFromAngle(20);
            Position = Serv.SetRandomEdge();
            Active = true;
            m_NewWave = false;
            m_Done = false;

            for (int pair = 0; pair < 3; pair++)
            {
                m_PodPair[pair].Active = true;
                m_PodPair[pair].NewWave = false;

                for (int pod = 0; pod < 2; pod++)
                {
                    m_PodPair[pair].Pods[pod].Active = true;
                    m_PodPair[pair].Pods[pod].NewWave = false;
                }
            }
        }

        public void Reset()
        {
            Active = false;
            m_NewWave = false;
            m_Done = true;

            for (int pair = 0; pair < 3; pair++)
            {
                m_PodPair[pair].Active = false;
                m_PodPair[pair].Moveable = false;

                for (int pod = 0; pod < 2; pod++)
                {
                    m_PodPair[pair].Pods[pod].Active = false;
                    m_PodPair[pair].Pods[pod].Moveable = false;
                }
            }
        }

        public void SplitAppart()
        {
            Active = false;

            for (int i = 0; i < 3; i++)
            {
                m_PodPair[i].Moveable = true;
            }
        }
    }
}
