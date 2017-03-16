﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;

    public class PodPair : VectorEngine.PositionedObject
    {
        Pod[] m_Pods = new Pod[2];
        Player m_Player;
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

        public void Initialize(Player player)
        {
            m_Player = player;
            Active = false;
        }

        public override void BeginRun()
        {
            base.BeginRun();

            for (int i = 0; i < 2; i++)
            {
                m_Pods[i].Moveable = false;
                m_Pods[i].Initialize(m_Player);
            }

            m_Pods[1].ReletiveRotation = (float)Math.PI;
            m_Pods[1].ReletivePosition.X = -11.7f;
            m_Pods[0].ReletivePosition.X = 11.7f;
            Radius = m_Pods[0].Radius * 2;
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

        void CheckCollision()
        {
            for (int i = 0; i < 2; i++)
            {
                if (m_Pods[i].CheckPlayerCollision() || m_Pods[i].CheckPlayerShotCollision())
                {
                    m_Player.SetScore(m_Score);
                    SplitAppart();
                }
            }
        }

        void SplitAppart()
        {
            for (int i = 0; i < 2; i++)
            {
                m_Pods[i].Moveable = true;
            }

            Active = false;
            Moveable = false;
        }
    }
}