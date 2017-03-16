using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;

    public class Pod : VectorEngine.Vector
    {
        Player m_Player;
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

        public void Initialize(Player player)
        {
            m_Player = player;
            Active = false;
        }

        public override void BeginRun()
        {
            base.BeginRun();

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Moveable && Active)
            {
                if (m_Player.Active && !m_Player.Hit)
                    RotationVelocity = Serv.AimAtTarget(Position, m_Player.Position, RotationInRadians);
                else
                    RotationVelocity = 0;

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

            InitializePoints(pointPosition);

            Radius = 14.04f;

        }

        void CheckCollision()
        {
            if (CheckPlayerCollision() || CheckPlayerShotCollision())
            {
                m_Player.SetScore(m_Score);
                Active = false;
                Moveable = false;
            }
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
                if (CirclesIntersect(m_Player.Position, m_Player.Radius))
                {
                    m_Player.Hit = true;
                    return true;
                }
            }

            return false;
        }
    }
}
