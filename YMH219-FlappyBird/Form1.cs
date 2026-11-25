using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YMH219_FlappyBird
{
    public partial class Form1 : Form
    {
        private Bird _bird;
        private List<PipePair> _pipes;
        private GameManager _gameManager;
        private ComboBox _cmbBirdSelect;
        private Random _random = new Random();

        private List<PointF> _clouds = new List<PointF>();
        private List<PointF> _rainDrops = new List<PointF>();
        private List<Point> _stars = new List<Point>();

        private const int GroundHeight = 85;
        private const int PipeSpacing = 250;
        private int GroundY => this.ClientSize.Height - GroundHeight;
        private float _groundScroll = 0f;

        public Form1()
        {
            InitializeComponent();
            SetupEnvironment();
        }

        private void SetupEnvironment()
        {
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.ClientSize = new Size(960, 640);

            _gameManager = new GameManager();
            _bird = new Bird(170, GroundY / 2f);

            // Sağ üst kuş seçici
            _cmbBirdSelect = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(this.ClientSize.Width - 205, 16),
                Width = 185,
                TabStop = false
            };
            _cmbBirdSelect.Items.AddRange(new object[]
            {
                "Sarı Kanarya",
                "Kızıl Kardinal",
                "Mavi Alakarga",
                "Zümrüt Papağan"
            });
            _cmbBirdSelect.SelectedIndex = 0;
            _cmbBirdSelect.SelectedIndexChanged += (s, e) =>
            {
                _bird.SelectedType = (BirdType)_cmbBirdSelect.SelectedIndex;
                this.Focus();
                this.Invalidate();
            };
            this.Controls.Add(_cmbBirdSelect);

            PopulateSkyDetails();
            InitializePipes();

            // Pencere tam ekran yapılırsa sağ taraf boş kalmasın diye yeniden doldur
            this.Resize += (s, e) =>
            {
                _cmbBirdSelect.Location = new Point(this.ClientSize.Width - 205, 16);
                PopulateSkyDetails();
                if (!_gameManager.IsRunning)
                {
                    _bird.Reset(170, GroundY / 2f);
                    InitializePipes();
                }
                this.Invalidate();
            };

            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;
            this.KeyDown += OnKeyDown;
            this.Paint += OnPaintScene;
        }

        private void PopulateSkyDetails()
        {
            int w = Math.Max(this.ClientSize.Width, 1200);
            _clouds.Clear();
            _stars.Clear();
            _rainDrops.Clear();

            for (int x = 40; x < w + 200; x += 210)
                _clouds.Add(new PointF(x, _random.Next(40, 190)));

            for (int i = 0; i < 75; i++)
                _stars.Add(new Point(_random.Next(0, w), _random.Next(10, 380)));

            for (int i = 0; i < 110; i++)
                _rainDrops.Add(new PointF(_random.Next(0, w), _random.Next(0, 700)));
        }

        private void InitializePipes()
        {
            _pipes = new List<PipePair>();
            int screenWidth = Math.Max(this.ClientSize.Width, 1200);
            int pipeCount = (screenWidth / PipeSpacing) + 3;

            // Ekranın tamamını dolduracak sayıda sütun çiftini önceden yerleştir
            for (int i = 0; i < pipeCount; i++)
            {
                _pipes.Add(new PipePair(440 + i * PipeSpacing, GroundY, _random));
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!_gameManager.IsRunning) return;

            _bird.Update();
            _groundScroll = (_groundScroll + _gameManager.Speed) % 120f;

            for (int i = 0; i < _clouds.Count; i++)
            {
                float nx = _clouds[i].X - (_gameManager.Speed * 0.25f);
                if (nx < -120) nx = this.ClientSize.Width + 60;
                _clouds[i] = new PointF(nx, _clouds[i].Y);
            }

            if (_gameManager.CurrentWeather == WeatherState.Yagmurlu)
            {
                for (int i = 0; i < _rainDrops.Count; i++)
                {
                    float rx = _rainDrops[i].X - 3f;
                    float ry = _rainDrops[i].Y + 11f;
                    if (ry > GroundY) { ry = 0; rx = _random.Next(0, this.ClientSize.Width); }
                    if (rx < 0) rx = this.ClientSize.Width;
                    _rainDrops[i] = new PointF(rx, ry);
                }
            }

            float rightmostX = 0;
            foreach (var p in _pipes)
                if (p.X > rightmostX) rightmostX = p.X;

            foreach (var pipe in _pipes)
            {
                pipe.Move(_gameManager.Speed);

                if (!pipe.IsScored && pipe.X + pipe.Width < _bird.X)
                {
                    pipe.IsScored = true;
                    _gameManager.AddScore();
                }

                if (pipe.X + pipe.Width < 0)
                {
                    pipe.X = rightmostX + PipeSpacing;
                    pipe.RandomizeGap(GroundY, _random);
                }

                if (pipe.CheckCollision(_bird, GroundY))
                {
                    EndGame();
                    return;
                }
            }

            if (_bird.Y + _bird.Height >= GroundY || _bird.Y <= 0)
            {
                EndGame();
                return;
            }

            this.Invalidate();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
                if (!_gameManager.IsRunning)
                {
                    StartNewGame();
                }
                else
                {
                    _bird.Flap();
                }
            }
        }

        private void StartNewGame()
        {
            _cmbBirdSelect.Enabled = false;
            _bird.Reset(170, GroundY / 2f);
            InitializePipes();
            _gameManager.StartGame();
            gameTimer.Start();
            this.Invalidate();
        }

        private void EndGame()
        {
            gameTimer.Stop();
            _gameManager.EndGame();
            _cmbBirdSelect.Enabled = true;
            this.Invalidate();
        }

        private void OnPaintScene(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawSkyAndWeather(g);

            foreach (var pipe in _pipes)
                pipe.Draw(g, GroundY, _gameManager.IsRecordBroken);

            DrawNatureGround(g);
            _bird.Draw(g);
            DrawHudAndOverlays(g);
        }

        private void DrawSkyAndWeather(Graphics g)
        {
            Color topSky = _gameManager.CurrentWeather switch
            {
                WeatherState.Yagmurlu => Color.FromArgb(95, 115, 135),
                WeatherState.Gece => Color.FromArgb(18, 26, 56),
                _ => Color.FromArgb(92, 184, 242)
            };

            Color bottomSky = _gameManager.CurrentWeather switch
            {
                WeatherState.Yagmurlu => Color.FromArgb(165, 180, 195),
                WeatherState.Gece => Color.FromArgb(46, 64, 112),
                _ => Color.FromArgb(196, 234, 255)
            };

            using (LinearGradientBrush skyBrush = new LinearGradientBrush(this.ClientRectangle, topSky, bottomSky, LinearGradientMode.Vertical))
            {
                g.FillRectangle(skyBrush, this.ClientRectangle);
            }

            int sunX = this.ClientSize.Width - 310;
            if (_gameManager.CurrentWeather == WeatherState.Gece)
            {
                foreach (var star in _stars)
                    g.FillEllipse(Brushes.LightYellow, star.X, star.Y, 3, 3);

                g.FillEllipse(Brushes.LemonChiffon, sunX, 55, 52, 52);
            }
            else if (_gameManager.CurrentWeather == WeatherState.Gunesli)
            {
                using (SolidBrush sunGlow = new SolidBrush(Color.FromArgb(70, Color.Gold)))
                    g.FillEllipse(sunGlow, sunX - 15, 40, 82, 82);
                g.FillEllipse(Brushes.Gold, sunX, 55, 52, 52);
            }

            Color cloudColor = _gameManager.CurrentWeather == WeatherState.Yagmurlu
                ? Color.FromArgb(200, 210, 220)
                : Color.FromArgb(230, 255, 255, 255);

            using (SolidBrush cloudBrush = new SolidBrush(cloudColor))
            {
                foreach (var c in _clouds)
                {
                    g.FillEllipse(cloudBrush, c.X, c.Y, 65, 34);
                    g.FillEllipse(cloudBrush, c.X + 22, c.Y - 12, 55, 40);
                    g.FillEllipse(cloudBrush, c.X + 48, c.Y + 2, 52, 30);
                }
            }

            if (_gameManager.CurrentWeather == WeatherState.Yagmurlu)
            {
                using (Pen rainPen = new Pen(Color.FromArgb(170, 210, 235, 255), 1.5f))
                {
                    foreach (var r in _rainDrops)
                        g.DrawLine(rainPen, r.X, r.Y, r.X - 3, r.Y + 10);
                }
            }
        }

        private void DrawNatureGround(Graphics g)
        {
            using (SolidBrush soilBrush = new SolidBrush(Color.FromArgb(101, 67, 33)))
                g.FillRectangle(soilBrush, 0, GroundY + 16, this.ClientSize.Width, GroundHeight - 16);

            using (SolidBrush grassBrush = new SolidBrush(Color.FromArgb(56, 142, 60)))
                g.FillRectangle(grassBrush, 0, GroundY, this.ClientSize.Width, 18);

            for (float x = -_groundScroll; x < this.ClientSize.Width + 120; x += 120)
            {
                g.FillRectangle(Brushes.SaddleBrown, x + 20, GroundY - 26, 8, 26);
                g.FillEllipse(Brushes.ForestGreen, x + 7, GroundY - 48, 34, 28);

                g.DrawLine(Pens.DarkGreen, x + 65, GroundY, x + 65, GroundY - 12);
                g.FillEllipse(Brushes.Crimson, x + 61, GroundY - 16, 8, 8);

                g.DrawLine(Pens.DarkGreen, x + 92, GroundY, x + 92, GroundY - 10);
                g.FillEllipse(Brushes.Gold, x + 88, GroundY - 14, 8, 8);
            }
        }

        private void DrawHudAndOverlays(Graphics g)
        {
            using (SolidBrush panelBrush = new SolidBrush(Color.FromArgb(145, 20, 20, 20)))
                g.FillRoundedRectangle(panelBrush, 16, 12, 430, 44, 12);

            using (Font hudFont = new Font("Segoe UI", 12f, FontStyle.Bold))
            {
                string weatherName = _gameManager.CurrentWeather switch
                {
                    WeatherState.Yagmurlu => "Yağmurlu",
                    WeatherState.Gece => "Gece",
                    _ => "Güneşli"
                };

                string hudText = $"Skor: {_gameManager.Score}   |   Rekor: {_gameManager.HighScore}   |   Hava: {weatherName}";
                g.DrawString(hudText, hudFont, Brushes.White, 28, 22);
            }

            if (!_gameManager.IsRunning)
            {
                Rectangle card = new Rectangle(this.ClientSize.Width / 2 - 220, this.ClientSize.Height / 2 - 75, 440, 145);
                using (SolidBrush cardBrush = new SolidBrush(Color.FromArgb(215, 28, 24, 22)))
                using (Pen goldPen = new Pen(Color.Goldenrod, 2.5f))
                {
                    g.FillRoundedRectangle(cardBrush, card.X, card.Y, card.Width, card.Height, 18);
                    g.DrawRoundedRectangle(goldPen, card.X, card.Y, card.Width, card.Height, 18);
                }

                using (Font titleFont = new Font("Segoe UI", 17f, FontStyle.Bold))
                using (Font subFont = new Font("Segoe UI", 11.5f, FontStyle.Regular))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
                    if (_gameManager.IsGameOver)
                    {
                        string recordNote = _gameManager.IsRecordBroken ? " (YENİ REKOR!)" : "";
                        g.DrawString($"OYUN BİTTİ! Skor: {_gameManager.Score}{recordNote}", titleFont, Brushes.Gold, this.ClientSize.Width / 2f, card.Y + 26, sf);
                        g.DrawString("Tekrar başlamak için SPACE tuşuna basın", subFont, Brushes.WhiteSmoke, this.ClientSize.Width / 2f, card.Y + 78, sf);
                    }
                    else
                    {
                        g.DrawString("FLAPPY BIRD", titleFont, Brushes.Gold, this.ClientSize.Width / 2f, card.Y + 26, sf);
                        g.DrawString("Zıplamak ve başlamak için SPACE tuşuna basın", subFont, Brushes.WhiteSmoke, this.ClientSize.Width / 2f, card.Y + 78, sf);
                    }
                }
            }
        }
    }

    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using GraphicsPath path = CreateRoundedPath(x, y, width, height, radius);
            g.FillPath(brush, path);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using GraphicsPath path = CreateRoundedPath(x, y, width, height, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundedPath(float x, float y, float width, float height, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + width - d, y, d, d, 270, 90);
            path.AddArc(x + width - d, y + height - d, d, d, 0, 90);
            path.AddArc(x, y + height - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}