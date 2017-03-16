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
        Timer m_BackOnePlay;
        Timer m_BackTwoPlay;
        Timer m_BackgroundOneDelay;
        Timer m_BackgroundTwoDelay;
        Timer m_GameWavePlayTime;
        List<Rock> m_LargeRocks;
        List<Rock> m_MedRocks;
        List<Rock> m_SmallRocks;
        SoundEffect m_RockExplode;
        SoundEffect m_BackgroundOne;
        SoundEffect m_BackgroundTwo;
        Word m_AtariHUD;
        Number m_AtariDate;
        readonly float m_UFOTimerSeedAmount = 10.15f;
        readonly float m_PodTimerSeedAmount = 30;
        readonly float m_BackgroundOneDelaySeed = 1;
        readonly float m_BackgroundTwoDelaySeed = 2;
        int m_UFOCount;
        int m_Wave;
        int m_LargeRockSpawnAmount;
        int m_RockCount;
        bool m_PlayedOne = false;
        bool m_PlayedTwo = false;

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
            m_BackOnePlay = new Timer(this);
            m_BackTwoPlay = new Timer(this);
            m_BackgroundOneDelay = new Timer(this);
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
            m_Player.LoadSounds(Content.Load<SoundEffect>("AsteroidsPlayerFire"),
                Content.Load<SoundEffect>("AsteroidsPlayerExplosion"), Content.Load<SoundEffect>("AsteroidsBonusShip"),
                Content.Load<SoundEffect>("AsteroidsThrust"));

            m_UFO.LoadSounds(Content.Load<SoundEffect>("AsteroidsUFOExplosion"),
                Content.Load<SoundEffect>("AsteroidsUFOShot"), Content.Load<SoundEffect>("AsteroidsUFOLarge"),
                Content.Load<SoundEffect>("AsteroidsUFOSmall"));

            m_RockExplode = Content.Load<SoundEffect>("AsteroidsRockExplosion");
            m_BackgroundOne = Content.Load<SoundEffect>("AsteroidsBackgroundOne");
            m_BackgroundTwo = Content.Load<SoundEffect>("AsteroidsBackgroundTwo");
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
            base.BeginRun();

            m_Player.BeginRun();
            m_Player.Initialize(m_UFO, m_Pod);
            m_Player.GameOver = true;
            m_Player.Active = false;
            m_UFOTimer.Reset();
            m_UFOTimer.Amount = m_UFOTimerSeedAmount;
            m_UFO.Initialize(m_Player, m_Pod);
            m_Pod.Initialize(m_Player);
            m_Pod.BeginRun();
            m_PodTimer.Reset();
            m_PodTimer.Amount = m_PodTimerSeedAmount;
            m_PlayerClear.Radius = 150;
            m_PlayerClear.Moveable = false;
            m_BackOnePlay.Amount = m_BackgroundOne.Duration.Seconds;
            m_BackTwoPlay.Amount = m_BackgroundTwo.Duration.Seconds;
            SpawnLargeRocks(4);
            m_AtariHUD.ProcessWords("ATARI INC", new Vector3(34, (-Serv.WindowHeight * 0.5f) + 20, 0), 5);
            m_AtariDate.ProcessNumber(1979, new Vector3(-34, (-Serv.WindowHeight * 0.5f) + 20, 0), 5);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
#if DEBUG
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif
            if (m_Player.GameOver)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.N))
                {
                    m_Player.GameOver = false;
                    NewGame();
                }
            }
            else if (m_Player.CheckClear)
            {
                if (CheckPlayerClear())
                {
                    m_Player.Spawn = true;
                }
            }
            else
            {
                PlayBackground();
            }

            CountRocks();
            UFOController();
            PodController();
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

        void PlayBackground()
        {
            if (m_BackgroundOneDelay.Seconds > m_BackgroundOneDelay.Amount)
            {
                m_BackgroundOneDelay.Reset();

                if (m_BackgroundOneDelay.Amount > 0.25f)
                    m_BackgroundOneDelay.Amount -= 0.025f;


                if (m_BackOnePlay.Seconds > m_BackOnePlay.Amount && !m_PlayedOne)
                {
                    m_PlayedOne = true;
                    m_PlayedTwo = false;
                    m_BackOnePlay.Reset();
                    m_BackgroundOne.Play(0.5f, 0, 0);
                }
            }

            if (m_BackgroundTwoDelay.Seconds > m_BackgroundTwoDelay.Amount)
            {
                m_BackgroundTwoDelay.Reset();

                if (m_BackgroundTwoDelay.Amount > 0.5f)
                    m_BackgroundTwoDelay.Amount -= 0.025f;

                if (m_BackTwoPlay.Seconds > m_BackTwoPlay.Amount && !m_PlayedTwo)
                {
                    m_PlayedOne = false;
                    m_PlayedTwo = true;
                    m_BackTwoPlay.Reset();
                    m_BackgroundTwo.Play();
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
            m_BackgroundOneDelay.Amount = m_BackgroundOneDelaySeed;
            m_BackgroundTwoDelay.Amount = m_BackgroundTwoDelaySeed;

        }

        void UFOController()
        {
            if (m_UFO.Done || m_UFO.Hit)
            {
                ResetUFO();
            }

            if (m_UFOTimer.Seconds > m_UFOTimer.Amount && !m_UFO.Active)
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

                if (m_PodTimer.Seconds > m_PodTimer.Amount)
                {
                    m_PodTimer.Reset();

                    if (m_Pod.Done)
                        m_Pod.Spawn();
                }
            }
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
                if (m_LargeRockSpawnAmount > 10)
                    m_LargeRockSpawnAmount = 10;

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
                    m_LargeRocks[rock].Player = m_Player;
                    m_LargeRocks[rock].UFO = m_UFO;
                    m_LargeRocks[rock].Spawn();
                    m_LargeRocks[rock].Position = Serv.SetRandomEdge();
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
