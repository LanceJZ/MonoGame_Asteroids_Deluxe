using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;
    using Timer = VectorEngine.Timer;
    using PO = VectorEngine.PositionedObject;
    /// <summary>
    /// After 40,000 points only small UFOs spawn.
    /// A steadily decreasing timer that shortens intervals between saucer spawns on each UFO.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager m_GraphicsDM;
        Player m_Player;
        PO m_PlayerClear;
        UFO m_UFO;
        PodGroup m_Pod;
        Timer m_UFOTimer;
        Timer m_PodTimer;
        Timer m_BackGroundPlay;
        Timer m_BackTwoPlay;
        Timer m_BackgroundDelay;
        Timer m_BackgroundTwoDelay;
        Timer m_GameWavePlayTime;
        List<Rock> m_LargeRocks;
        List<Rock> m_MedRocks;
        List<Rock> m_SmallRocks;
        SoundEffect m_RockExplode;
        SoundEffect m_Background;
        Word m_AtariHUD;
        Number m_AtariDate;
        KeyboardState m_KeyState, m_KeyStateOld;
        readonly float m_UFOTimerSeedAmount = 10.15f;
        readonly float m_PodTimerSeedAmount = 30;
        readonly float m_BackgroundOneDelaySeed = 1;
        readonly float m_BackgroundTwoDelaySeed = 2;
        int m_UFOCount;
        int m_Wave;
        int m_LargeRockSpawnAmount;
        int m_RockCount;
        bool m_PlayedBack = false;
        //bool m_PlayedTwo = false;
        bool m_Paused = false;

        public Game()
        {
            Vector2 screenSize = new Vector2();
            m_GraphicsDM = new GraphicsDeviceManager(this);
            m_GraphicsDM.IsFullScreen = false;
            m_GraphicsDM.SynchronizeWithVerticalRetrace = true;
            m_GraphicsDM.GraphicsProfile = GraphicsProfile.HiDef;
            screenSize.X = m_GraphicsDM.PreferredBackBufferWidth = 1200;
            screenSize.Y = m_GraphicsDM.PreferredBackBufferHeight = 900;
            m_GraphicsDM.PreferMultiSampling = true; //Error in MonoGame 3.6 for DirectX, fixed for next version.
            m_GraphicsDM.PreparingDeviceSettings += SetMultiSampling;
            m_GraphicsDM.ApplyChanges();
            IsFixedTimeStep = false;
            m_Player = new Player(this);
            m_PlayerClear = new PO(this);
            m_UFO = new UFO(this);
            m_Pod = new PodGroup(this);
            m_UFOTimer = new Timer(this);
            m_PodTimer = new Timer(this);
            m_BackGroundPlay = new Timer(this);
            m_BackTwoPlay = new Timer(this);
            m_BackgroundDelay = new Timer(this);
            m_BackgroundTwoDelay = new Timer(this);
            m_GameWavePlayTime = new Timer(this);
            m_AtariHUD = new Word(this);
            m_AtariDate = new Number(this);
            m_LargeRocks = new List<Rock>();
            m_MedRocks = new List<Rock>();
            m_SmallRocks = new List<Rock>();
            Content.RootDirectory = "Content";
        }

        private void SetMultiSampling(object sender, PreparingDeviceSettingsEventArgs eventArgs)
        {
            PresentationParameters PresentParm = eventArgs.GraphicsDeviceInformation.PresentationParameters;
            PresentParm.MultiSampleCount = 4;
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Serv.Initialize(m_GraphicsDM, this);

            // The number determines how good our antialiasing works.
            // Possible values are 2,4,8,16,32, but not all work on all computers.
            // 4 is safe, and 8 is too in almost all cases
            // Higher numbers mean lower framerates

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            m_Player.LoadSounds(Content.Load<SoundEffect>("AsteroidsDeluxePlayerFire"),
                Content.Load<SoundEffect>("AsteroidsPlayerExplosion"), Content.Load<SoundEffect>("AsteroidsDeluxeBonusShip"),
                Content.Load<SoundEffect>("AsteroidsDeluxePlayerThrust"), Content.Load<SoundEffect>("AsteroidsDeluxePlayerStart"),
                Content.Load<SoundEffect>("AsteroidsDeluxeShield"));

            m_UFO.LoadSounds(Content.Load<SoundEffect>("AsteroidsUFOExplosion"),
                Content.Load<SoundEffect>("AsteroidsUFOShot"), Content.Load<SoundEffect>("AsteroidsUFOLarge"),
                Content.Load<SoundEffect>("AsteroidsUFOSmall"));

            m_RockExplode = Content.Load<SoundEffect>("AsteroidsRockExplosion");
            m_Background = Content.Load<SoundEffect>("AsteroidsDeluxeBackground");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Executed after initialization is complete
        /// </summary>
        protected override void BeginRun()
        {
            m_Player.BeginRun();
            m_Player.Initialize(m_UFO, m_Pod);
            m_Player.GameOver = true;
            m_Player.Active = false;
            m_UFOTimer.Amount = m_UFOTimerSeedAmount;
            m_UFO.Initialize(m_Player);
            m_Pod.Initialize(m_Player, m_UFO);
            m_Pod.BeginRun();
            m_PodTimer.Amount = m_PodTimerSeedAmount;
            m_PlayerClear.Radius = 150;
            m_PlayerClear.Moveable = false;
            m_BackGroundPlay.Amount = m_Background.Duration.Seconds;
            SpawnLargeRocks(4);
            m_AtariHUD.ProcessWords("ATARI INC", new Vector3(34, (-Serv.WindowHeight * 0.5f) + 20, 0), 5);
            m_AtariDate.ProcessNumber(1980, new Vector3(-34, (-Serv.WindowHeight * 0.5f) + 20, 0), 5);

            base.BeginRun();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            m_KeyState = Keyboard.GetState();
            KeyInput();
            m_KeyStateOld = m_KeyState;

            if (m_Paused)
                return;

            if (m_Player.CheckClear)
            {
                if (CheckPlayerClear())
                {
                    m_Player.Spawn = true;
                }
            }
            else if (!m_Player.GameOver)
            {
                PlayBackground();
            }

            CountRocks();
            UFOController();
            PodController();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(new Vector3(0.01666f, 0, 0.1f)));

            base.Draw(gameTime);
        }

        void KeyInput()
        {
#if DEBUG
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (m_KeyState.IsKeyDown(Keys.K) && !m_KeyStateOld.IsKeyDown(Keys.K))
            {
                m_Pod.Reset();
                m_Pod.Spawn();
            }
#endif
            if (m_KeyState.IsKeyDown(Keys.P) && !m_KeyStateOld.IsKeyDown(Keys.P))
            {
                m_Paused = !m_Paused;
            }

            if (m_KeyState.IsKeyDown(Keys.N) && !m_KeyStateOld.IsKeyDown(Keys.N) && m_Player.GameOver)
            {
                m_Player.GameOver = false;
                NewGame();
            }
        }

        void PlayBackground()
        {
            if (m_BackgroundDelay.Expired)
            {
                m_BackgroundDelay.Reset();

                if (m_BackgroundDelay.Amount > 0.25f)
                    m_BackgroundDelay.Amount -= 0.025f;


                if (m_BackGroundPlay.Expired && !m_PlayedBack)
                {
                    m_PlayedBack = true;
                    m_BackGroundPlay.Reset();
                    m_Background.Play(0.25f, 0, 0);
                }
            }
        }

        bool CheckPlayerClear()
        {
            foreach (Rock rock in m_LargeRocks)
            {
                if (rock.Active)
                {
                    if (m_PlayerClear.CirclesIntersect(rock.Position, rock.Radius))
                        return false;
                }
            }

            foreach (Rock rock in m_MedRocks)
            {
                if (rock.Active)
                {
                    if (m_PlayerClear.CirclesIntersect(rock.Position, rock.Radius))
                        return false;
                }
            }

            foreach (Rock rock in m_SmallRocks)
            {
                if (rock.Active)
                {
                    if (m_PlayerClear.CirclesIntersect(rock.Position, rock.Radius))
                        return false;
                }
            }

            if (m_UFO.Active)
            {
                if (m_PlayerClear.CirclesIntersect(m_UFO.Position, m_UFO.Radius))
                    return false;
            }

            if (m_UFO.Shot.Active)
                return false;

            if (m_Pod.Active)
            {
                if (m_PlayerClear.CirclesIntersect(m_Pod.Position, m_Pod.Radius))
                    return false;
            }
            else
            {
                for (int pair = 0; pair < 3; pair++)
                {
                    if (m_Pod.PodPair[pair].Active)
                    {
                        if (m_PlayerClear.CirclesIntersect(m_Pod.PodPair[pair].Position, m_Pod.PodPair[pair].Radius))
                            return false;
                    }
                    else
                    {
                        for (int pod = 0; pod < 2; pod++)
                        {
                            if (m_Pod.PodPair[pair].Pods[pod].Active)
                            {
                                if (m_PlayerClear.CirclesIntersect(m_Pod.PodPair[pair].Pods[pod].Position,
                                    m_Pod.PodPair[pair].Pods[pod].Radius))
                                    return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        void NewGame()
        {
            m_Player.NewGame();
            ResetUFO();
            ResetPod();

            for (int i = 0; i < m_LargeRocks.Count; i++)
            {
                m_LargeRocks[i].Active = false;
            }

            for (int i = 0; i < m_MedRocks.Count; i++)
            {
                m_MedRocks[i].Active = false;
            }

            for (int i = 0; i < m_SmallRocks.Count; i++)
            {
                m_SmallRocks[i].Active = false;
            }

            m_Wave = 0;
            m_UFOCount = 0;
            SpawnLargeRocks(m_LargeRockSpawnAmount = 4);
            m_BackgroundDelay.Amount = m_BackgroundOneDelaySeed;
            m_BackgroundTwoDelay.Amount = m_BackgroundTwoDelaySeed;

        }

        void UFOController()
        {
            if (m_UFO.Done || m_UFO.Hit)
            {
                ResetUFO();
            }

            if (m_UFOTimer.Expired && !m_UFO.Active)
            {
                m_UFOTimer.Amount = Serv.RandomMinMax(m_UFOTimerSeedAmount * 0.5f,
                    m_UFOTimerSeedAmount + (m_UFOTimerSeedAmount - m_Wave));
                m_UFO.Spawn(m_UFOCount, m_Wave);
                m_UFOCount++;
            }
        }

        void PodController()
        {
            if (m_RockCount < 4 && m_Wave > 1)
            {
                if (!m_Pod.Done)
                    m_PodTimer.Reset();

                if (m_PodTimer.Expired)
                {
                    m_PodTimer.Reset();

                    if (m_Pod.Done)
                    {
                        m_Pod.Spawn();
                    }
                }
            }
            else
                m_PodTimer.Reset();
        }

        void ResetUFO()
        {
            m_UFOTimer.Reset();
            m_UFO.Active = false;
            m_UFO.Done = false;
            m_UFO.Hit = false;
        }

        void ResetPod()
        {
            m_PodTimer.Reset();
            m_Pod.Reset();
        }

        void CountRocks()
        {
            m_RockCount = 0;

            foreach (Rock rock in m_LargeRocks)
            {
                if (rock.Active)
                {
                    m_RockCount++;

                    if (rock.Hit)
                    {
                        SpawnMedRocks(rock.Position);
                        rock.Active = false;
                        rock.Hit = false;
                    }
                }

            }

            foreach (Rock rock in m_MedRocks)
            {
                if (rock.Active)
                {
                    m_RockCount++;

                    if (rock.Hit)
                    {
                        SpawnSmallRocks(rock.Position);
                        rock.Active = false;
                        rock.Hit = false;
                    }
                }

            }

            foreach (Rock rock in m_SmallRocks)
            {
                if (rock.Active)
                {
                    m_RockCount++;

                    if (rock.Hit)
                    {
                        rock.Active = false;
                        rock.Hit = false;
                    }
                }
            }

            if (m_RockCount == 0)
            {
                if (m_LargeRockSpawnAmount > 12)
                    m_LargeRockSpawnAmount = 12;

                SpawnLargeRocks(m_LargeRockSpawnAmount += 2);
                m_Wave++;
            }
        }

        void SpawnLargeRocks(int count)
        {
            m_GameWavePlayTime.Reset();
            m_Wave++;
            m_Pod.NewWave();

            for (int i = 0; i < count; i++)
            {
                bool spawnNewRock = true;

                foreach (Rock rock in m_LargeRocks)
                {
                    if (!rock.Active && !rock.ExplosionActive)
                    {
                        spawnNewRock = false;
                        rock.Spawn();
                        rock.Position = Serv.SetRandomEdge();
                        break;
                    }
                }

                if (spawnNewRock)
                {
                    int rock = m_LargeRocks.Count;
                    m_LargeRocks.Add(new Rock(this));
                    m_LargeRocks[rock].Spawn(m_Player, m_UFO);
                    m_LargeRocks[rock].LoadSound(m_RockExplode);
                }
            }
        }

        void SpawnMedRocks(Vector3 position)
        {
            for (int i = 0; i < 2; i++)
            {
                bool spawnNewRock = true;

                foreach (Rock rock in m_MedRocks)
                {
                    if (!rock.Active && !rock.ExplosionActive)
                    {
                        spawnNewRock = false;
                        rock.Spawn(position);
                        break;
                    }
                }

                if (spawnNewRock)
                {
                    int rock = m_MedRocks.Count;
                    m_MedRocks.Add(new Rock(this));
                    m_MedRocks[rock].Spawn(position, 0.5f, 150, 50, m_Player, m_UFO);
                    m_MedRocks[rock].LoadSound(m_RockExplode);
                }
            }
        }

        void SpawnSmallRocks(Vector3 position)
        {
            for (int i = 0; i < 2; i++)
            {
                bool spawnNewRock = true;

                foreach (Rock rock in m_SmallRocks)
                {
                    if (!rock.Active && !rock.ExplosionActive)
                    {
                        spawnNewRock = false;
                        rock.Spawn(position);
                        break;
                    }
                }

                if (spawnNewRock)
                {
                    int rock = m_SmallRocks.Count;
                    m_SmallRocks.Add(new Rock(this));
                    m_SmallRocks[rock].Spawn(position, 0.25f, 300, 100, m_Player, m_UFO);
                    m_SmallRocks[rock].LoadSound(m_RockExplode);
                }
            }

        }
    }
}
