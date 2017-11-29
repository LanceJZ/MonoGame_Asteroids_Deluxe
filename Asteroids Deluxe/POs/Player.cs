using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;
    using Timer = VectorEngine.Timer;

    public class Player : VectorEngine.PositionedObject
    {
        Game m_Game;
        Timer m_FlameTimer;
        Timer m_ThrustTimer;
        PlayerFlame m_Flame;
        PlayerShip m_Ship;
        Shield m_Shield;
        List<PlayerShip> m_ShipLives;
        Shot[] m_Shots;
        UFO m_UFO;
        PodGroup m_PodGroup;
        SoundEffect m_FireShot;
        SoundEffect m_Explode;
        SoundEffect m_Bonus;
        SoundEffect m_Thrust;
        SoundEffect m_Start;
        SoundEffect m_ShieldOn;
        LineExplode m_Explosion;
        Number m_ScoreHUD;
        Number m_HiScoreHUD;
        HighScores m_HighScoreList;
        KeyboardState m_KeyState, m_KeyStateOld;
        float m_ShieldPower;
        int m_Score;
        int m_HiScore;
        int m_NextBonusLife;
        int m_BaseBonusLife = 5000;
        int m_Lives;
        bool m_Exploding = false;
        bool m_Spawn = true;
        bool m_CheckClear = false;
        bool m_GameOver = false;
        bool m_ShieldTest = false;
        bool m_ShieldSoundPlayed = false;

        public Shot[] Shots
        {
            get { return m_Shots; }

            set { m_Shots = value; }
        }

        public Shield Shield
        {
            get { return m_Shield; }
        }

        public bool Exploding
        {
            get { return m_Exploding; }
        }

        public bool CheckClear
        {
            get {  return m_CheckClear; }
        }

        public bool Spawn
        {
            set { m_Spawn = value; }
        }

        public bool GameOver
        {
            get { return m_GameOver; }

            set { m_GameOver = value; }
        }

        public Player(Game game) : base(game)
        {
            m_Game = game;
            m_FlameTimer = new Timer(game);
            m_ThrustTimer = new Timer(game);
            m_Flame = new PlayerFlame(game);
            m_Ship = new PlayerShip(game);
            m_Shield = new Shield(game);
            m_Shots = new Shot[4];
            m_ScoreHUD = new Number(game);
            m_ScoreHUD.Moveable = false;
            m_HiScoreHUD = new Number(game);
            m_HiScoreHUD.Moveable = false;
            m_ShipLives = new List<PlayerShip>();
            m_Explosion = new LineExplode(game);
            m_HighScoreList = new HighScores(game);

            AddChild(m_Flame, false, true);
            AddChild(m_Ship, true, true);
            AddChild(m_Shield, false, true);

            for (int i = 0; i < 4; i++)
            {
                MakeShipLives(i);
            }

            for (int i = 0; i < 4; i++)
            {
                Shots[i] = new Shot(game);
            }
        }

        public override void Initialize()
        {
            m_FlameTimer.Amount = 0.01666f;

            base.Initialize();
        }

        public void Initialize(UFO ufo, PodGroup podGroup)
        {
            m_UFO = ufo;
            m_PodGroup = podGroup;
        }

        /// <summary>
        /// Executed after initialization is complete
        /// </summary>
        public override void BeginRun()
        {
            Active = false;
            m_Flame.Active = false;
            m_Ship.BeginRun();
            m_Shield.BeginRun();
            Radius = m_Ship.Radius;
            ShipLivesDisplay();
            m_HighScoreList.BeginRun();
            m_HiScore = m_HighScoreList.HighScore;
            m_HiScoreHUD.ProcessNumber(m_HiScore, new Vector3(0, 440, 0), 8);
            m_ScoreHUD.ProcessNumber(m_Score, new Vector3(-400, 440, 0), 10);
        }

        public override void Update(GameTime gameTime)
        {
            if (m_Exploding)
            {
                if (!m_Explosion.Active)
                {
                    m_Exploding = false;
                    m_CheckClear = true;
                }
            }
            else if (m_Spawn)
            {
                m_CheckClear = false;

                if (!GameOver)
                {
                    Active = true;
                    m_Spawn = false;
                    m_Start.Play(0.1f, 0, 0);
                }
            }

            if (Active)
            {
                if (Hit)
                {
                    LostLife();
                }

                if (!m_ShieldTest)
                {
                    if (m_Shield.Active && m_ShieldPower > 0)
                        m_ShieldPower -= 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    else if (m_ShieldPower < 100 && !m_Shield.Active)
                        m_ShieldPower += 1 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                m_KeyState = Keyboard.GetState();
                KeyInput();
                m_KeyStateOld = m_KeyState;
                CheckBorders();
                CheckCollision();
                base.Update(gameTime);
            }
        }

        public void SetScore(int add)
        {
            m_Score += add;

            m_ScoreHUD.ProcessNumber(m_Score, new Vector3(-400, 440, 0), 10);

            if (m_HiScore < m_Score)
            {
                m_HiScore = m_Score;
                m_HiScoreHUD.ProcessNumber(m_HiScore, new Vector3(0, 440, 0), 8);
            }

            if (m_Score > m_NextBonusLife)
            {
                m_Bonus.Play(0.15f, 0, 0);
                m_Lives++;
                m_NextBonusLife += 10000;
                ShipLivesDisplay();
            }
        }

        public void NewGame()
        {
            m_Lives = 4;
            m_Score = 0;
            m_NextBonusLife = m_BaseBonusLife;
            Active = true;
            ResetShip();
            SetScore(0);
            m_HighScoreList.NewGame();
            ShipLivesDisplay();
        }

        public void LoadSounds(SoundEffect fireshot, SoundEffect explode, SoundEffect bonus, SoundEffect thurst,
            SoundEffect start, SoundEffect shield)
        {
            m_ShieldOn = shield;
            m_Start = start;
            m_FireShot = fireshot;
            m_Explode = explode;
            m_Bonus = bonus;
            m_Thrust = thurst;
            m_ThrustTimer.Amount = (float)m_Thrust.Duration.TotalSeconds;
        }

        public void ShieldHit(Vector3 position, Vector3 velocity)
        {
            Acceleration = Vector3.Zero;
            Velocity = (Velocity * 0.1f) * -1;
            Velocity += velocity * 0.95f;
            Velocity += Serv.VelocityFromAngle(Serv.AngleFromVectors(position, Position), 75);

            if (!m_ShieldTest)
                m_ShieldPower -= 20;
        }

        public void ShieldHit()
        {
            if (!m_ShieldTest)
                m_ShieldPower -= 40;
        }

        void CheckCollision()
        {
            if (m_PodGroup.Active)
            {
                if (m_Shield.Active)
                {
                    if (m_PodGroup.CirclesIntersect(m_Shield.Position, m_Shield.Radius))
                    {
                        ShieldHit(m_PodGroup.Position, m_PodGroup.Velocity);
                    }
                }
                else if (CirclesIntersect(m_PodGroup.Position, m_PodGroup.Radius))
                {
                    Hit = true;
                    m_PodGroup.SplitAppart();
                    SetScore(m_PodGroup.Score);
                }

            }

            foreach (Shot shot in m_Shots)
            {
                if (shot.Active)
                {
                    if (m_PodGroup.Active)
                    {
                        if (shot.CirclesIntersect(m_PodGroup.Position, m_PodGroup.Radius))
                        {
                            shot.Active = false;
                            m_PodGroup.SplitAppart();
                            SetScore(m_PodGroup.Score);
                            break;
                        }
                    }

                    if (m_UFO.Active)
                    {
                        if (shot.CirclesIntersect(m_UFO.Position, m_UFO.Radius))
                        {
                            shot.Active = false;
                            m_UFO.Active = false;
                            m_UFO.Explode();
                            SetScore(m_UFO.Points);
                        }
                    }
                }
            }
        }

        void Explode()
        {
            m_Explode.Play(0.5f, 0, 0);
            m_Explosion.Spawn(Position, Radius);
            m_Exploding = true;
        }

        void LostLife()
        {
            Explode();
            m_Lives--;
            Active = false;
            Hit = false;
            m_Flame.Active = false;
            m_Shield.Active = false;
            m_Spawn = false;

            if (m_Lives < 1)
            {
                GameOver = true;
                m_HighScoreList.GameOver(m_Score);
                m_HiScore = m_HighScoreList.HighScore;
            }

            ShipLivesDisplay();
            ResetShip();
        }

        void ShipLivesDisplay() //TODO: Fix wings appearing in middle of screen on new ship life.
        {
            for (int i = 0; i < m_ShipLives.Count; i++)
            {
                m_ShipLives[i].Active = false;
            }

            if (m_Lives > m_ShipLives.Count)
            {
                MakeShipLives(m_Lives - 1);
            }

            for (int i = 0; i < m_Lives; i++)
            {
                m_ShipLives[i].Active = true;
            }
        }

        void MakeShipLives(int count)
        {
            m_ShipLives.Add(new PlayerShip(m_Game));
            m_ShipLives[count].Position = new Vector3((-count * 16) + -400, 400, 0);
            m_ShipLives[count].RotationInRadians = (float)Math.PI * 0.5f;
            m_ShipLives[count].Scale = 0.5f;
        }

        void ResetShip()
        {
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            Acceleration = Vector3.Zero;
            RotationInRadians = Serv.RandomMinMax(0, (float)MathHelper.TwoPi);
            m_ShieldPower = 100;
        }

        void ThrustOn()
        {
            float maxPerSecond = 820;
            float thrustAmount = 400;

            if (m_FlameTimer.Expired)
            {
                m_FlameTimer.Reset();

                if (Active)
                {
                    m_Flame.Active = !m_Flame.Active;
                }
                else
                    m_Flame.Active = false;
            }

            if (m_ThrustTimer.Expired)
            {
                m_ThrustTimer.Reset();

                m_Thrust.Play(0.33f, 0, 0);
            }

            if (Math.Abs(Velocity.X) + Math.Abs(Velocity.Y) < maxPerSecond)
            {
                Acceleration = Serv.SetVelocity(RotationInRadians, thrustAmount);
            }
            else
            {
                ThrustOff();
            }
        }

        void ThrustOff()
        {
            float Deceration = 0.25f;
            Acceleration = -Velocity * Deceration;
        }

        void FireShot()
        {
            for (int shotCount = 0; shotCount < 4; shotCount++)
            {
                if (!Shots[shotCount].Active)
                {
                    m_FireShot.Play(0.15f, 0, 0);

                    float speed = 500;
                    Vector3 offset = Serv.SetVelocity(RotationInRadians, 11);
                    Vector3 direction = Serv.SetVelocity(RotationInRadians, speed);

                    Shots[shotCount].Spawn(Position + offset, Velocity * 0.75f + direction, 1.55f);
                    break;
                }
            }
        }

        void ShieldOn()
        {
            if (Active)
            {
                if (m_ShieldPower > 0)
                {
                    m_Shield.Active = true; //TODO: Finish shield functionality.

                    if (!m_ShieldSoundPlayed && !m_ShieldTest)
                    {
                        m_ShieldOn.Play(0.75f, 0, 0);
                        m_ShieldSoundPlayed = true;
                    }
                }
                else
                {
                    m_Shield.Active = false;
                    m_ShieldSoundPlayed = false;
                }
            }
        }

        void ShieldOff()
        {
            m_Shield.Active = false;
            m_ShieldSoundPlayed = false;
        }

        void KeyInput()
        {
            float rotationAmound = MathHelper.Pi;

            if (m_KeyState.IsKeyDown(Keys.W) || m_KeyState.IsKeyDown(Keys.Up))
            {
                ThrustOn();
            }
            else
            {
                m_Flame.Active = false;
                ThrustOff();
            }

            if (m_KeyState.IsKeyDown(Keys.A) || m_KeyState.IsKeyDown(Keys.Left))
            {
                RotationVelocity = rotationAmound;
            }
            else if (m_KeyState.IsKeyDown(Keys.D) || m_KeyState.IsKeyDown(Keys.Right))
            {
                RotationVelocity = -rotationAmound;
            }
            else
                RotationVelocity = 0;

            if (!m_KeyStateOld.IsKeyDown(Keys.LeftControl) && !m_KeyStateOld.IsKeyDown(Keys.Space))
            {
                if (m_KeyState.IsKeyDown(Keys.LeftControl) || m_KeyState.IsKeyDown(Keys.Space))
                {
                    FireShot();
                }
            }

            if (m_KeyState.IsKeyDown(Keys.Down) || m_KeyState.IsKeyDown(Keys.RightControl)
                || m_KeyState.IsKeyDown(Keys.RightShift))
            {
                ShieldOn();
            }
            else
            {
                ShieldOff();
            }
#if DEBUG
            if (m_KeyState.IsKeyDown(Keys.L) && !m_KeyStateOld.IsKeyDown(Keys.L))
            {
                m_Lives++;
                ShipLivesDisplay();
            }

            if (m_KeyState.IsKeyDown(Keys.S) && !m_KeyStateOld.IsKeyDown(Keys.S))
            {
                m_ShieldTest = !m_ShieldTest;
            }

            if (m_ShieldTest)
            {
                ShieldOn();
            }
#endif
        }
    }
}
