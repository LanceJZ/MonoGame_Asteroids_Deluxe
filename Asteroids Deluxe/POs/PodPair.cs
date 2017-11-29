using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Asteroids_Deluxe.VectorEngine;

namespace Asteroids_Deluxe
{
    public class PodPair : PositionedObject
    {
        Pod[] m_Pods = new Pod[2];
        Player m_Player;
        UFO m_UFO;
        SoundEffect m_Explode;
        float m_RotateMagnitude = MathHelper.PiOver2;
        int m_Score = 100;
        bool m_NewWave;

        public bool NewWave
        {
            set
            {
                m_NewWave = value;
            }
        }

        public Pod[] Pods
        {
            get { return m_Pods; }
            set { m_Pods = value; }
        }

        public PodPair(Game game) : base(game)
        {
            for (int i = 0; i < 2; i++)
            {
                m_Pods[i] = new Pod(game);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < 2; i++)
            {
                AddChild(m_Pods[i], false, false);
            }
        }

        public void Initialize(Player player, UFO ufo)
        {
            m_Player = player;
            m_UFO = ufo;
            Active = false;
        }

        public void LoadSounds(SoundEffect explode)
        {
            m_Explode = explode;
        }

        public override void BeginRun()
        {
            base.BeginRun();

            foreach (Pod pod in m_Pods)
            {
                pod.Moveable = false;
                pod.Initialize(m_Player, m_UFO);
                pod.LoadSounds(m_Explode);
            }

            m_Pods[1].ReletiveRotation = (float)Math.PI;
            m_Pods[1].ReletivePosition.X = -11.7f;
            m_Pods[0].ReletivePosition.X = 11.7f;
            Radius = m_Pods[0].Radius * 2;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Active & Moveable)
            {
                if (m_Player.Active && !m_Player.Hit && !m_NewWave)
                    RotationVelocity = Services.AimAtTarget(Position, m_Player.Position,
                        RotationInRadians, m_RotateMagnitude);
                else
                {
                    RotationVelocity = 0;

                    if (m_UFO.Active)
                    {
                        RotationVelocity = Services.AimAtTarget(Position, m_UFO.Position,
                            RotationInRadians, m_RotateMagnitude);
                    }
                }

                if (m_NewWave)
                {
                    if (Position.X > Services.WindowWidth * 0.5f || Position.X < -Services.WindowWidth * 0.5f
                        || Position.Y > Services.WindowHeight * 0.5f || Position.Y < -Services.WindowHeight * 0.5f)
                    {
                        Active = false;

                        for (int i = 0; i < 2; i++)
                        {
                            m_Pods[i].Active = false;
                        }
                    }

                    RotationVelocity = 0;
                }

                Velocity = Services.VelocityFromAngle(RotationInRadians, 70);
                CheckCollision();
            }
        }

        void CheckCollision()
        {
            foreach (Pod pod in m_Pods)
            {
                if (pod.CheckPlayerCollision())
                {
                    if (m_Player.Shield.Active)
                    {
                        m_Player.ShieldHit(pod.Position, Velocity);
                    }
                    else
                    {
                        m_Player.SetScore(m_Score);
                        SplitAppart();
                    }
                }

                if (pod.CheckPlayerShotCollision())
                {
                    m_Player.SetScore(m_Score);
                    SplitAppart();
                }

                if (pod.CheckUFOCollision() || pod.CheckUFOShotCollision())
                {
                    SplitAppart();
                }
            }
        }

        void SplitAppart()
        {
            if (!m_Player.GameOver)
                m_Explode.Play(0.25f, 0.5f, 0);

            foreach (Pod pod in m_Pods)
            {
                pod.Moveable = true;
            }

            Active = false;
            Moveable = false;
        }
    }
}
