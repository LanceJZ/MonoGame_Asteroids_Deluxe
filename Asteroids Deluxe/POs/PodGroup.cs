using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Asteroids_Deluxe.VectorEngine;

namespace Asteroids_Deluxe
{
    public class PodGroup : PositionedObject
    {
        PodPair[] m_PodPair = new PodPair[3];
        Player m_Player;
        UFO m_UFO;
        SoundEffect m_Spawn;
        SoundEffect m_Explode;
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

            foreach (PodPair pair in m_PodPair)
            {
                AddChild(pair, false, false);
            }
        }

        public void Initialize(Player player, UFO ufo)
        {
            m_Player = player;
            m_UFO = ufo;
            Active = false;
        }

        public void LoadSounds(SoundEffect spawn, SoundEffect explode)
        {
            m_Spawn = spawn;
            m_Explode = explode;
        }

        public override void BeginRun()
        {
            base.BeginRun();

            foreach (PodPair pair in m_PodPair)
            {
                pair.Moveable = false;
                pair.Initialize(m_Player, m_UFO);
                pair.LoadSounds(m_Explode);
                pair.BeginRun();
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
                    if (Position.X > Services.WindowWidth * 0.5f || Position.X < -Services.WindowWidth * 0.5f
                        || Position.Y > Services.WindowHeight * 0.5f || Position.Y < -Services.WindowHeight * 0.5f)
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
                foreach (PodPair pair in m_PodPair)
                {
                    if (pair.Active)
                        return;

                    foreach (Pod pod in pair.Pods)
                    {
                        if (pod.Active)
                            return;
                    }
                }

                Reset();
            }
        }

        public void NewWave()
        {
            m_NewWave = true;

            foreach (PodPair pair in m_PodPair)
            {
                pair.NewWave = true;

                foreach (Pod pod in pair.Pods)
                {
                    pod.NewWave = true;
                }
            }
        }

        public void Spawn()
        {
            if (!m_Player.GameOver)
                m_Spawn.Play();
            Velocity = Services.SetVelocityFromAngle(20);
            Position = Services.SetRandomEdge();
            Active = true;
            m_NewWave = false;
            m_Done = false;

            foreach (PodPair pair in m_PodPair)
            {
                pair.Active = true;
                pair.NewWave = false;

                foreach (Pod pod in pair.Pods)
                {
                    pod.Active = true;
                    pod.NewWave = false;
                }
            }
        }

        public void Reset()
        {
            Active = false;
            m_NewWave = false;
            m_Done = true;

            foreach (PodPair pair in m_PodPair)
            {
                pair.Active = false;
                pair.Moveable = false;

                foreach (Pod pod in pair.Pods)
                {
                    pod.Active = false;
                    pod.Moveable = false;
                }
            }
        }

        public void SplitAppart()
        {
            if (!m_Player.GameOver)
                m_Explode.Play(0.5f, 0, 0);
            Active = false;

            foreach (PodPair pair in m_PodPair)
            {
                pair.Moveable = true;
            }
        }
    }
}
