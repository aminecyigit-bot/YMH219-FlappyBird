using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace YMH219_FlappyBird
{
    public class PipePair
    {
        public float X { get; set; }
        public int Width { get; } = 74;
        public int GapY { get; private set; }
        public int GapHeight { get; } = 155;
        public bool IsScored { get; set; } = false;

        public PipePair(float startX, int groundY, Random random)
        {
            X = startX;
            RandomizeGap(groundY, random);
        }

        public void RandomizeGap(int groundY, Random random)
        {
            GapY = random.Next(95, groundY - GapHeight - 95);
            IsScored = false;
        }

        public void Move(float speed)
        {
            X -= speed;
        }

        public bool CheckCollision(Bird bird, int groundY)
        {
            RectangleF topRect = new RectangleF(X, 0, Width, GapY);
            RectangleF bottomRect = new RectangleF(X, GapY + GapHeight, Width, groundY - (GapY + GapHeight));
            return bird.Bounds.IntersectsWith(topRect) || bird.Bounds.IntersectsWith(bottomRect);
        }

        public void Draw(Graphics g, int groundY, bool isRecordBroken)
        {
            // Normalde tarihi kahverengi taş/tuğla; rekor kırılınca altın-bronz parıltı
            Color mainColor = isRecordBroken ? Color.FromArgb(184, 134, 11) : Color.FromArgb(115, 74, 48);
            Color lightColor = isRecordBroken ? Color.FromArgb(238, 201, 0) : Color.FromArgb(156, 108, 76);
            Color darkColor = isRecordBroken ? Color.FromArgb(120, 80, 10) : Color.FromArgb(72, 44, 26);

            DrawCisternColumn(g, X, 0, Width, GapY, true, mainColor, lightColor, darkColor);
            DrawCisternColumn(g, X, GapY + GapHeight, Width, groundY - (GapY + GapHeight), false, mainColor, lightColor, darkColor);
        }

        private void DrawCisternColumn(Graphics g, float x, float y, int w, float h, bool isTop, Color main, Color light, Color dark)
        {
            if (h <= 0) return;

            RectangleF shaftRect = new RectangleF(x + 6, y, w - 12, h);
            using (LinearGradientBrush shaftBrush = new LinearGradientBrush(shaftRect, dark, light, LinearGradientMode.Horizontal))
            {
                g.FillRectangle(shaftBrush, shaftRect);
            }

            // Sütun üzerindeki dikey yivler (oymalar)
            using (Pen flutePen = new Pen(Color.FromArgb(90, dark), 2f))
            {
                for (float offset = 16; offset <= w - 16; offset += 10)
                {
                    g.DrawLine(flutePen, x + offset, y, x + offset, y + h);
                }
            }

            // Binbirdirek Sarnıcı kesik piramit sütun başlığı ve bileziği
            int capHeight = 28;
            float capY = isTop ? (y + h - capHeight) : y;
            RectangleF capRect = new RectangleF(x - 4, capY, w + 8, capHeight);

            using (SolidBrush capBrush = new SolidBrush(main))
            using (Pen borderPen = new Pen(dark, 2f))
            {
                g.FillRectangle(capBrush, capRect);
                g.DrawRectangle(borderPen, capRect.X, capRect.Y, capRect.Width, capRect.Height);

                // Sütun bilezik halkası
                float ringY = isTop ? capY - 7 : capY + capHeight;
                g.FillRectangle(capBrush, x + 1, ringY, w - 2, 7);
                g.DrawRectangle(borderPen, x + 1, ringY, w - 2, 7);
            }
        }
    }
}