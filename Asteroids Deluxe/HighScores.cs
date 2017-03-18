using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace Asteroids_Deluxe
{
    using Serv = VectorEngine.Services;
    using Timer = VectorEngine.Timer;

    struct HighScoreListLines
    {
        Number m_Rank;
        Number m_Score;
        Word m_Name;

        public Number Rank
        {
            get
            {
                return m_Rank;
            }

            set
            {
                m_Rank = value;
            }
        }

        public Number Score
        {
            get
            {
                return m_Score;
            }

            set
            {
                m_Score = value;
            }
        }

        public Word Name
        {
            get
            {
                return m_Name;
            }

            set
            {
                m_Name = value;
            }
        }
    }

    struct HighScoreList
    {
        int m_Score;
        string m_Name;

        public int Score
        {
            get
            {
                return m_Score;
            }

            set
            {
                m_Score = value;
            }
        }

        public string Name
        {
            get
            {
                return m_Name;
            }

            set
            {
                m_Name = value;
            }
        }
    }

    public class HighScores : GameComponent
    {
        HighScoreList[] m_HighScoreData = new HighScoreList[10];
        HighScoreListLines[] m_HighScoreDisplay = new HighScoreListLines[10];
        Word m_GameOverHUD;
        Word m_CoinPlayHUD;
        Word m_HighScoreHUD;
        Word m_PushStartHUD;
        Word[] m_EnterInitials = new Word[4];
        Word m_NewHighScoreLetters;
        Number[] m_CoinPlayOnes = new Number[2];
        Timer m_HighScoreTimer;
        Timer m_GameOverTimer;
        Timer m_PushStartTimer;
        FileStream m_FileStream;
        char[] m_HighScoreSelectedLetters = new char[3];
        string m_FileName = "Score.sav";
        string m_DataRead = "";
        string[] m_EnterYourInitials = new string[4];
        int m_NewHighScoreRank;
        int m_HighScore;
        int m_HighScoreSelector;
        bool m_IsThereANewHighScore = false;
        bool m_GameOver = true;
        bool m_KeyLeftDown = false;
        bool m_KeyRightDown = false;
        bool m_KeyNextDown = false;
        bool m_GameOverDisplayed = false;

        public int HighScore
        {
            get
            {
                return m_HighScore;
            }
        }

        public HighScores(Game game) : base(game)
        {
            game.Components.Add(this);

            for (int i = 0; i < 10; i++)
            {
                m_HighScoreDisplay[i].Name = new Word(game);
                m_HighScoreDisplay[i].Score = new Number(game);
                m_HighScoreDisplay[i].Rank = new Number(game);
            }

            for (int i = 0; i < 4; i++)
            {
                m_EnterInitials[i] = new Word(game);
            }

            for (int i = 0; i < 2; i++)
            {
                m_CoinPlayOnes[i] = new Number(game);
            }

            m_HighScoreHUD = new Word(game);
            m_GameOverHUD = new Word(game);
            m_PushStartHUD = new Word(game);
            m_CoinPlayHUD = new Word(game);
            m_NewHighScoreLetters = new Word(game);
            m_HighScoreTimer = new Timer(game);
            m_GameOverTimer = new Timer(game);
            m_PushStartTimer = new Timer(game);
        }

        public override void Initialize()
        {
            m_HighScoreTimer.Amount = 15;
            m_GameOverTimer.Amount = 5;
            m_PushStartTimer.Amount = 0.666f;
            m_EnterYourInitials[0] = "YOUR SCORE IS ONE OF THE TEN BEST";
            m_EnterYourInitials[1] = "PLEASE ENTER YOUR INITIALS";
            m_EnterYourInitials[2] = "PUSH ROTATE TO SELECT LETTER";
            m_EnterYourInitials[3] = "PUSH SHIELD WHEN LETTER IS CURRECT";
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (m_GameOver)
            {
                if (!m_IsThereANewHighScore)
                {
                    if (m_HighScoreData[0].Score > 0)
                    {
                        if (m_HighScoreTimer.Seconds > m_HighScoreTimer.Amount)
                        {
                            HideScoreList();
                            m_GameOverDisplayed = true;
                            m_GameOverHUD.ShowWords();
                            m_CoinPlayHUD.ShowWords();
                            m_CoinPlayOnes[0].ShowNumbers();
                            m_CoinPlayOnes[1].ShowNumbers();
                            m_HighScoreTimer.Reset();
                            m_GameOverTimer.Reset();
                        }
                        else if (m_GameOverTimer.Seconds > m_GameOverTimer.Amount)
                        {
                            ShowScoreList();
                            m_GameOverDisplayed = false;
                            m_PushStartHUD.HideWords();
                            m_GameOverHUD.HideWords();
                            m_CoinPlayHUD.HideWords();
                            m_CoinPlayOnes[0].HideNumbers();
                            m_CoinPlayOnes[1].HideNumbers();
                            m_GameOverTimer.Reset();
                        }
                    }
                    else
                    {
                        HideScoreList();
                        m_GameOverDisplayed = true;
                        m_GameOverHUD.ShowWords();
                        m_CoinPlayHUD.ShowWords();
                        m_CoinPlayOnes[0].ShowNumbers();
                        m_CoinPlayOnes[1].ShowNumbers();
                    }

                    if (m_GameOverDisplayed)
                    {
                        if (m_PushStartTimer.Seconds > m_PushStartTimer.Amount)
                        {
                            m_PushStartTimer.Reset();

                            if (m_PushStartHUD.Active)
                            {
                                m_PushStartHUD.HideWords();
                            }
                            else
                            {
                                m_PushStartHUD.ShowWords();
                            }
                        }
                    }
                }
                else
                {
                    NewHighScore();
                }
            }
        }

        public void BeginRun()
        {
            if (ReadFile())
            {
                DataDecode();
                UpdateHighScoreList();
            }

            m_HighScoreHUD.ProcessWords("HIGH SCORES", new Vector3(0, VectorEngine.Services.WindowHeight * 0.5f - 140, 0), 12);
            m_GameOverHUD.ProcessWords("GAME OVER", new Vector3(0, (Serv.WindowHeight * 0.5f) * 0.3666f, 0), 14);
            m_GameOverHUD.HideWords();
            m_PushStartHUD.ProcessWords("PUSH START", new Vector3(0, (Serv.WindowHeight * 0.5f) * 0.666f, 0), 12);
            m_PushStartHUD.HideWords();
            float coinsY = (-Serv.WindowHeight * 0.5f) * 0.4666f;
            m_CoinPlayHUD.ProcessWords("COIN   PLAY", new Vector3(0, coinsY, 0), 12);
            m_CoinPlayHUD.HideWords();
            m_CoinPlayOnes[0].ProcessNumber(1, new Vector3(-200, coinsY, 0), 12);
            m_CoinPlayOnes[1].ProcessNumber(1, new Vector3(-10, coinsY, 0), 12);
            m_CoinPlayOnes[0].HideNumbers();
            m_CoinPlayOnes[1].HideNumbers();

            for (int i = 0; i < 4; i++)
            {
                m_EnterInitials[i].ProcessWords(m_EnterYourInitials[i], new Vector3(0, Serv.WindowHeight * 0.5f - 200 - i * 40, 0), 12);
                m_EnterInitials[i].HideWords();
            }
        }

        public void HideScoreList()
        {
            for (int i = 0; i < 10; i++)
            {
                m_HighScoreDisplay[i].Name.HideWords();
                m_HighScoreDisplay[i].Rank.HideNumbers();
                m_HighScoreDisplay[i].Score.HideNumbers();
            }

            m_HighScoreHUD.HideWords();
        }

        public void ShowScoreList()
        {
            for (int i = 0; i < 10; i++)
            {
                m_HighScoreDisplay[i].Name.ShowWords();
                m_HighScoreDisplay[i].Score.ShowNumbers();
                m_HighScoreDisplay[i].Rank.ShowNumbers();
            }

            m_HighScoreHUD.ShowWords();
        }

        public void NewGame()
        {
            m_GameOver = false;
            m_GameOverDisplayed = false;
            HideScoreList();
            m_GameOverHUD.HideWords();
            m_CoinPlayHUD.HideWords();
            m_CoinPlayOnes[0].HideNumbers();
            m_CoinPlayOnes[1].HideNumbers();
            m_PushStartHUD.HideWords();
            m_HighScoreSelectedLetters = "___".ToCharArray();
        }

        public void GameOver(int score)
        {
            ShowScoreList();

            for (int rank = 0; rank < 10; rank++)
            {
                if (score > m_HighScoreData[rank].Score)
                {
                    if (rank < 9)
                    {
                        HighScoreList[] oldScores = new HighScoreList[10];

                        for (int oldranks = rank; oldranks < 10; oldranks++)
                        {
                            oldScores[oldranks].Score = m_HighScoreData[oldranks].Score;
                            oldScores[oldranks].Name = m_HighScoreData[oldranks].Name;
                        }

                        for (int oldranks = rank; oldranks < 9; oldranks++)
                        {
                            m_HighScoreData[oldranks + 1].Score = oldScores[oldranks].Score;
                            m_HighScoreData[oldranks + 1].Name = oldScores[oldranks].Name;
                        }
                    }

                    m_HighScoreData[rank].Score = score;
                    m_HighScoreData[rank].Name = "AAA";
                    WriteFile();
                    m_IsThereANewHighScore = true;
                    m_GameOver = true;
                    m_HighScoreSelector = 0;
                    m_NewHighScoreRank = rank;
                    HideScoreList();
                    m_GameOverHUD.HideWords();
                    UpdateLetterSelected();

                    for (int i = 0; i < 4; i++)
                    {
                        m_EnterInitials[i].ShowWords();
                    }

                    break;
                }
            }
        }

        void NewHighScore()
        {
            if (!m_KeyRightDown)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    m_KeyRightDown = true;
                    m_HighScoreSelectedLetters[m_HighScoreSelector]++;

                    if (m_HighScoreSelectedLetters[m_HighScoreSelector] > 95)
                        m_HighScoreSelectedLetters[m_HighScoreSelector] = (char)65;

                    if (m_HighScoreSelectedLetters[m_HighScoreSelector] > 90)
                        m_HighScoreSelectedLetters[m_HighScoreSelector] = (char)95;

                    UpdateLetterSelected();
                }
            }

            if (!m_KeyLeftDown)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                {
                    m_KeyLeftDown = true;
                    m_HighScoreSelectedLetters[m_HighScoreSelector]--;

                    if (m_HighScoreSelectedLetters[m_HighScoreSelector] == 94)
                        m_HighScoreSelectedLetters[m_HighScoreSelector] = (char)90;

                    if (m_HighScoreSelectedLetters[m_HighScoreSelector] < 65)
                        m_HighScoreSelectedLetters[m_HighScoreSelector] = (char)95;

                    UpdateLetterSelected();
                }
            }

            if (!m_KeyNextDown)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    m_KeyNextDown = true;
                    m_HighScoreSelector++;

                    UpdateLetterSelected();

                    if (m_HighScoreSelector > 2)
                    {
                        string name = "";

                        for (int i = 0; i < 3; i++)
                        {
                            name += m_HighScoreSelectedLetters[i];
                        }

                        m_IsThereANewHighScore = false;
                        m_HighScoreData[m_NewHighScoreRank].Name = name;
                        UpdateHighScoreList();
                        m_NewHighScoreLetters.HideWords();
                        WriteFile();
                        UpdateHighScoreList();

                        for (int i = 0; i < 4; i++)
                        {
                            m_EnterInitials[i].HideWords();
                        }
                    }
                }
            }

            if (Keyboard.GetState().IsKeyUp(Keys.Right) && m_KeyRightDown)
                m_KeyRightDown = false;

            if (Keyboard.GetState().IsKeyUp(Keys.Left) && m_KeyLeftDown)
                m_KeyLeftDown = false;

            if (Keyboard.GetState().IsKeyUp(Keys.Down) && m_KeyNextDown)
                m_KeyNextDown = false;
        }

        void UpdateLetterSelected()
        {
            string name = "";
            Vector3 lettersPosition = new Vector3(0, -Serv.WindowHeight * 0.5f + 180, 0);

            for (int i = 0; i < 3; i++)
            {
                name += m_HighScoreSelectedLetters[i];
            }

            m_NewHighScoreLetters.ProcessWords(name, lettersPosition, 12);

        }

        void UpdateHighScoreList()
        {
            Vector3 listPosition = new Vector3(20, Serv.WindowHeight * 0.5f - 180, 0);

            for (int i = 0; i < 10; i++)
            {
                if (m_HighScoreData[i].Score > 0)
                {
                    m_HighScoreDisplay[i].Rank.ProcessNumber(i + 1, new Vector3(listPosition.X - 150,
                        listPosition.Y - i * 25, 0), 10);
                    m_HighScoreDisplay[i].Score.ProcessNumber(m_HighScoreData[i].Score, new Vector3(listPosition.X + 15,
                        listPosition.Y - i * 25, 0), 10);
                    m_HighScoreDisplay[i].Name.ProcessWords(m_HighScoreData[i].Name, new Vector3(listPosition.X + 80,
                        listPosition.Y - i * 25, 0), 10);

                    if (m_HighScoreData[i].Score > m_HighScore)
                        m_HighScore = m_HighScoreData[i].Score;
                }
            }
        }

        void DataDecode()
        {
            int scoreRank = 0;
            int letter = 0;
            bool isLetter = true;
            string fromNumber = "";

            foreach (char DataChar in m_DataRead)
            {
                if (DataChar.ToString() == "*")
                    break;

                if (DataChar.ToString() == "'\0'")
                    break;

                if (isLetter)
                {
                    letter++;
                    m_HighScoreData[scoreRank].Name += DataChar;

                    if (letter == 3)
                        isLetter = false;
                }
                else
                {
                    if (DataChar.ToString() == ":")
                    {
                        m_HighScoreData[scoreRank].Score = int.Parse(fromNumber);

                        scoreRank++;

                        if (scoreRank > 9)
                            break;

                        letter = 0;
                        fromNumber = "";
                        isLetter = true;
                    }
                    else
                    {
                        fromNumber += DataChar.ToString();
                    }
                }
            }
        }

        bool ReadFile()
        {
            if (File.Exists(m_FileName))
            {
                m_FileStream = new FileStream(m_FileName, FileMode.Open, FileAccess.Read);

                byte[] dataByte = new byte[1024];
                UTF8Encoding bufferUTF8 = new UTF8Encoding(true);

                while (m_FileStream.Read(dataByte, 0, dataByte.Length) > 0)
                {
                    m_DataRead += bufferUTF8.GetString(dataByte, 0, dataByte.Length);
                }

                Close();
            }
            else
                return false;

            return true;
        }

        void WriteFile()
        {
            m_FileStream = new FileStream(m_FileName, FileMode.OpenOrCreate, FileAccess.Write);

            for (int i = 0; i < 10; i++)
            {
                if (m_HighScoreData[i].Score > 0)
                {
                    byte[] name = new UTF8Encoding(true).GetBytes(m_HighScoreData[i].Name);
                    m_FileStream.Write(name, 0, name.Length);

                    byte[] score = new UTF8Encoding(true).GetBytes(m_HighScoreData[i].Score.ToString());
                    m_FileStream.Write(score, 0, score.Length);

                    byte[] marker = new UTF8Encoding(true).GetBytes(":");
                    m_FileStream.Write(marker, 0, marker.Length);
                }
            }

            Close();
        }

        void Close()
        {
            m_FileStream.Flush();
            m_FileStream.Close();
            m_FileStream.Dispose();
        }
    }
}
