using System;
using System.IO;
using System.Media;

namespace YMH219_FlappyBird
{
    public enum WeatherState
    {
        Gunesli,
        Yagmurlu,
        Gece
    }

    public class GameManager
    {
        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public float Speed { get; private set; } = 4.5f;
        public bool IsRunning { get; private set; } = false;
        public bool IsGameOver { get; private set; } = false;
        public bool IsRecordBroken { get; private set; } = false;
        public WeatherState CurrentWeather { get; private set; } = WeatherState.Gunesli;

        private readonly string _highScoreFile = "highscore.txt";

        public GameManager()
        {
            LoadHighScore();
        }

        public void StartGame()
        {
            Score = 0;
            Speed = 4.5f;
            IsRunning = true;
            IsGameOver = false;
            IsRecordBroken = false;
            CurrentWeather = WeatherState.Gunesli;
        }

        public void AddScore()
        {
            Score++;
            SystemSounds.Asterisk.Play();

            if (Score > HighScore)
            {
                HighScore = Score;
                IsRecordBroken = true;
                SaveHighScore();
            }

            if (Score % 4 == 0 && Speed < 9.5f)
            {
                Speed += 0.4f;
            }

            // Her 5 puanda dinamik hava olayı değişimi
            int cycle = (Score / 5) % 3;
            CurrentWeather = cycle switch
            {
                1 => WeatherState.Yagmurlu,
                2 => WeatherState.Gece,
                _ => WeatherState.Gunesli
            };
        }

        public void EndGame()
        {
            IsRunning = false;
            IsGameOver = true;
            SystemSounds.Hand.Play();
        }

        private void LoadHighScore()
        {
            try
            {
                if (File.Exists(_highScoreFile) && int.TryParse(File.ReadAllText(_highScoreFile), out int val))
                    HighScore = val;
            }
            catch { HighScore = 0; }
        }

        private void SaveHighScore()
        {
            try { File.WriteAllText(_highScoreFile, HighScore.ToString()); }
            catch { }
        }
    }
}