using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rotator_Interface
{
    public partial class Form1 : Form
    {
        float uhol = 0f;
        float? ciel = null;
        SerialDriver SerialController = new SerialDriver();

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SerialController.ChangeDegree += novy => {
                if (InvokeRequired) Invoke(new Action(() => { uhol = novy; pictureBox1.Invalidate(); }));
                else { uhol = novy; pictureBox1.Invalidate(); }
            };
            try { SerialController.Otvor(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            pictureBox1.Paint += Kreslenie;
            pictureBox1.MouseClick += Klik;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SerialController.Zatvor();
            base.OnFormClosing(e);
        }

        void Klik(object s, MouseEventArgs e)
        {
            float dx = e.X - pictureBox1.Width / 2f;
            float dy = e.Y - pictureBox1.Height / 2f;
            if (Math.Sqrt(dx * dx + dy * dy) > 200) return;
            double az = Math.Atan2(dy, dx) * 180.0 / Math.PI + 90.0; // jeej realne uplatnenie chujovin zo školy :D
            ciel = (float)(((az % 360) + 360) % 360);
            SerialController.IdiNa(ciel.Value);
            pictureBox1.Invalidate();
        }

        void Kreslenie(object s, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = pictureBox1.Width / 2f - 6;
            float cy = pictureBox1.Height / 2f - 1;
            float R = 185f;

            if (ciel.HasValue)
            {
                PointF tp = BodNaKruznici(cx, cy, ciel.Value, R * 0.92f);
                using (Pen p = new Pen(Color.DodgerBlue, 2f))
                {
                    p.DashStyle = DashStyle.Dash;
                    g.DrawLine(p, cx, cy, tp.X, tp.Y);
                }
                g.FillEllipse(Brushes.DodgerBlue, tp.X - 5, tp.Y - 5, 10, 10);
            }

            PointF hrot = BodNaKruznici(cx, cy, uhol, R * 0.88f);
            PointF chvost = BodNaKruznici(cx, cy, uhol + 180, R * 0.15f);
            using (Pen p = new Pen(Color.OrangeRed, 2.5f))
                g.DrawLine(p, chvost, hrot);

            float rad = (float)((uhol - 90) * Math.PI / 180.0);
            PointF[] sipka = {
                hrot,
                new PointF(hrot.X - (float)Math.Cos(rad - 0.35) * 13, hrot.Y - (float)Math.Sin(rad - 0.35) * 13),
                new PointF(hrot.X - (float)Math.Cos(rad + 0.35) * 13, hrot.Y - (float)Math.Sin(rad + 0.35) * 13),
            };
            g.FillPolygon(Brushes.OrangeRed, sipka);
            g.FillEllipse(Brushes.Black, cx - 5, cy - 5, 10, 10);

            using (Font f = new Font("Segoe UI", 9f))
                g.DrawString(string.Format("Uhol: {0:F1}°   Ciel: {1}", uhol, ciel.HasValue ? ciel.Value.ToString("F1") + "°" : "---"),
                    f, Brushes.DimGray, 5, pictureBox1.Height - 20);
        }

        PointF BodNaKruznici(float cx, float cy, float az, float r)
        {
            double rad = (az - 90.0) * Math.PI / 180.0;
            return new PointF(cx + (float)(Math.Cos(rad) * r), cy + (float)(Math.Sin(rad) * r));
        }
    }
}