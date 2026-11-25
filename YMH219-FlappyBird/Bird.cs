using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace YMH219_FlappyBird
{
    public enum BirdType
    {
        SariKanarya,
        KizilKardinal,
        MaviAlakarga,
        ZumrutPapagan
    }

    public class Bird
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; } = 44;
        public int Height { get; } = 32;
        public float Velocity { get; private set; } = 0f;
        public BirdType SelectedType { get; set; } = BirdType.SariKanarya;

        // Daha dengeli ve akıcı uçuş hassasiyeti
        private const float Gravity = 0.42f;
        private const float JumpImpulse = -8.2f;
        private const float MaxFallSpeed = 9.5f;
        private int _wingCounter = 0;

        public Bird(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        public void Update()
        {
            Velocity += Gravity;
            if (Velocity > MaxFallSpeed) Velocity = MaxFallSpeed;
            Y += Velocity;
            _wingCounter++;
        }

        public void Flap()
        {
            Velocity = JumpImpulse;
        }

        public void Reset(float startX, float startY)
        {
            X = startX;
            Y = startY;
            Velocity = 0f;
        }

        public RectangleF Bounds => new RectangleF(X + 5, Y + 5, Width - 10, Height - 10);

        public void Draw(Graphics g)
        {
            GraphicsState state = g.Save();
            float angle = Math.Clamp(Velocity * 2.8f, -22f, 38f);
            g.TranslateTransform(X + Width / 2f, Y + Height / 2f);
            g.RotateTransform(angle);
            g.TranslateTransform(-(X + Width / 2f), -(Y + Height / 2f));

            float wingOffset = (_wingCounter % 18 < 9) ? -3f : 4f;

            switch (SelectedType)
            {
                case BirdType.KizilKardinal:
                    DrawCardinal(g, wingOffset);
                    break;
                case BirdType.MaviAlakarga:
                    DrawBlueJay(g, wingOffset);
                    break;
                case BirdType.ZumrutPapagan:
                    DrawParrot(g, wingOffset);
                    break;
                default:
                    DrawCanary(g, wingOffset);
                    break;
            }

            g.Restore(state);
        }

        // 1. Sarı Kanarya: Zarif oval gövde ve küçük kuyruk
        private void DrawCanary(Graphics g, float wingOffset)
        {
            // Kuyruk
            PointF[] tail = { new PointF(X + 4, Y + 14), new PointF(X - 10, Y + 9), new PointF(X - 8, Y + 21) };
            g.FillPolygon(Brushes.Goldenrod, tail);

            // Gövde
            g.FillEllipse(Brushes.Gold, X, Y, Width, Height);
            g.DrawEllipse(new Pen(Color.DarkGoldenrod, 1.8f), X, Y, Width, Height);
            g.FillEllipse(Brushes.LightYellow, X + 8, Y + 15, Width - 16, Height - 17);

            // Kanat
            g.FillEllipse(Brushes.Orange, X + 9, Y + 10 + wingOffset, 18, 10);

            // Göz ve ince gaga
            g.FillEllipse(Brushes.White, X + 26, Y + 6, 9, 9);
            g.FillEllipse(Brushes.Black, X + 30, Y + 8, 4, 4);
            PointF[] beak = { new PointF(X + Width - 1, Y + 13), new PointF(X + Width + 10, Y + 16), new PointF(X + Width - 1, Y + 20) };
            g.FillPolygon(Brushes.DarkOrange, beak);
        }

        // 2. Kızıl Kardinal: Tepesinde belirgin sivri taç (ibik) ve siyah yüz maskesi
        private void DrawCardinal(Graphics g, float wingOffset)
        {
            // Tepe tacı (ibik)
            PointF[] crest = { new PointF(X + 14, Y + 3), new PointF(X + 8, Y - 11), new PointF(X + 26, Y + 2) };
            g.FillPolygon(Brushes.Crimson, crest);

            // Uzun kuyruk
            PointF[] tail = { new PointF(X + 2, Y + 15), new PointF(X - 15, Y + 12), new PointF(X - 13, Y + 23) };
            g.FillPolygon(Brushes.DarkRed, tail);

            // Gövde
            g.FillEllipse(Brushes.Crimson, X, Y, Width, Height);
            g.DrawEllipse(new Pen(Color.DarkRed, 1.8f), X, Y, Width, Height);

            // Siyah göz maskesi
            g.FillEllipse(Brushes.Black, X + 24, Y + 6, 15, 14);

            // Kanat
            g.FillEllipse(Brushes.Firebrick, X + 8, Y + 11 + wingOffset, 20, 11);

            // Göz ve kalın konik gaga
            g.FillEllipse(Brushes.White, X + 27, Y + 7, 8, 8);
            g.FillEllipse(Brushes.Black, X + 30, Y + 9, 4, 4);
            PointF[] beak = { new PointF(X + Width - 2, Y + 11), new PointF(X + Width + 11, Y + 16), new PointF(X + Width - 2, Y + 22) };
            g.FillPolygon(Brushes.OrangeRed, beak);
        }

        // 3. Mavi Alakarga: Aerodinamik uzun gövde, sivri kafa tacı ve çift renkli uzun kuyruk
        private void DrawBlueJay(Graphics g, float wingOffset)
        {
            // Sivri kuyruk
            PointF[] tail = { new PointF(X + 2, Y + 14), new PointF(X - 18, Y + 10), new PointF(X - 16, Y + 20) };
            g.FillPolygon(Brushes.RoyalBlue, tail);

            // Geriye yatık kafa tacı
            PointF[] crest = { new PointF(X + 12, Y + 4), new PointF(X + 4, Y - 8), new PointF(X + 24, Y + 2) };
            g.FillPolygon(Brushes.DodgerBlue, crest);

            // İnce uzun gövde
            g.FillEllipse(Brushes.DodgerBlue, X - 2, Y + 2, Width + 4, Height - 4);
            g.FillEllipse(Brushes.WhiteSmoke, X + 8, Y + 15, Width - 12, Height - 17);

            // Beyaz çizgili mavi kanat
            g.FillEllipse(Brushes.MidnightBlue, X + 8, Y + 10 + wingOffset, 21, 10);
            g.DrawLine(new Pen(Color.White, 2f), X + 12, Y + 15 + wingOffset, X + 24, Y + 15 + wingOffset);

            // Göz ve uzun sivri gaga
            g.FillEllipse(Brushes.White, X + 28, Y + 7, 9, 9);
            g.FillEllipse(Brushes.Black, X + 31, Y + 9, 4, 4);
            PointF[] beak = { new PointF(X + Width + 1, Y + 13), new PointF(X + Width + 14, Y + 16), new PointF(X + Width + 1, Y + 19) };
            g.FillPolygon(Brushes.DimGray, beak);
        }

        // 4. Zümrüt Papağan: İri kafa, uzun çift kuyruk tüyü ve kıvrık papağan gagası
        private void DrawParrot(Graphics g, float wingOffset)
        {
            // Uzun tropik kuyruk
            PointF[] tail1 = { new PointF(X + 3, Y + 16), new PointF(X - 20, Y + 14), new PointF(X - 18, Y + 22) };
            PointF[] tail2 = { new PointF(X + 3, Y + 19), new PointF(X - 16, Y + 24), new PointF(X - 12, Y + 28) };
            g.FillPolygon(Brushes.Crimson, tail1);
            g.FillPolygon(Brushes.Gold, tail2);

            // Gövde ve iri papağan başı
            g.FillEllipse(Brushes.MediumSeaGreen, X, Y + 3, Width - 4, Height - 3);
            g.FillEllipse(Brushes.LimeGreen, X + 18, Y - 2, 24, 24);

            // Renkli kanat
            g.FillEllipse(Brushes.SeaGreen, X + 6, Y + 11 + wingOffset, 19, 12);
            g.FillEllipse(Brushes.Gold, X + 10, Y + 13 + wingOffset, 10, 6);

            // Göz halkası ve kıvrık kanca gaga
            g.FillEllipse(Brushes.White, X + 26, Y + 4, 11, 11);
            g.FillEllipse(Brushes.Black, X + 30, Y + 7, 5, 5);

            PointF[] hookedBeak =
            {
                new PointF(X + Width - 3, Y + 9),
                new PointF(X + Width + 11, Y + 13),
                new PointF(X + Width + 8, Y + 23),
                new PointF(X + Width + 2, Y + 18),
                new PointF(X + Width - 3, Y + 18)
            };
            g.FillPolygon(Brushes.DarkOrange, hookedBeak);
        }
    }
}