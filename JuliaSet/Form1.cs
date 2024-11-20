using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace JuliaSet
{
	public partial class Form1 : Form
	{
        private bool SaveImage = false;
        private PictureBox pictureBox1;
        private int w = 800;
        private int h = 800;
        private int width8K = 8192; // 8K width
        private int height8K = 8192; // 8K height
        private float scale = 1.0f;
        private int maxiter = 100;
        private float n = 2.0F;
        private double cR = 0.58;
        private double cTheta = 66;
        private int colorSelect = 1;
        private ComplexNumber u = new ComplexNumber(0, 0);
        public Form1()
		{
			InitializeComponent();
			this.Size = new Size(w, h);
			pictureBox1 = new PictureBox();
			pictureBox1.Size = new Size(w, h);
			pictureBox1.Location = new Point((this.ClientSize.Width - w) / 2, (this.ClientSize.Height - h) / 2);
			this.Controls.Add(pictureBox1);
			this.KeyDown += Form1_KeyDown;
			Julia_Set(w, h);
		}
		void Julia_Set(int width, int height)
        {
            UpdateFormTitle();
            ComplexNumber c = new ComplexNumber(cR * Math.Cos(cTheta * Math.PI / 180), cR * Math.Sin(cTheta * Math.PI / 180));
            var calculatedPoints = Enumerable.Range(0, width * height).AsParallel().Select(xy =>
            {
                int i = maxiter;
                int y = xy / width;
                int x = xy % width;
                ComplexNumber z = new ComplexNumber(2.0 * (x - width / 2) / (scale * width / 2.0), 2.0 * (height / 2 - y) / (scale * height / 2.0));
                z += u;
                while (z.Norm() < 4.0 && i > 0)
                {
                    if (n == 2.0F)
                        z = z * z + c;
                    else
                        z = ComplexNumber.pow(z, n) + c;
                    i--;
                }
                return new CalculatedPoint { x = x, y = y, i = i };
            });

            var bitmap = new Bitmap(width, height);
            foreach (CalculatedPoint cp in calculatedPoints)
            {
                bitmap.SetPixel(cp.x, cp.y, setColor((double)cp.i / maxiter));
            }
            if (SaveImage)
            {

                bitmap.SetResolution(300, 300); // Set the resolution to 300 DPI
                bitmap.Save($"N={n}_R={cR}_Theta={cTheta}_Iteration={maxiter}_Scale={scale}_x={u.Re}_y={u.Im}.png");
                SaveImage = false;
            }
            else pictureBox1.Image = bitmap;
        }
		private Color setColor(double aspect)
		{
			int r, g, b;
			double hue, sat, light;
			if (colorSelect == 1)
			{
                hue = lerp(240, 180, Math.Sin(Math.PI*aspect));
                sat = lerp(0.5, 1, (1 + Math.Cos(Math.PI * aspect)) / 2);
                light = lerp(0.1, 1, (1 + Math.Cos(Math.PI * aspect)) / 2);
                Color c = HSLtoRGB(hue, sat, light);
				r = c.R;
				g = c.G;
				b = c.B;
            }
            else if (colorSelect == 2)
			{
                hue = (aspect * 360);
                sat = lerp(1, 0.5, aspect);
                light = lerp(1, 0, aspect);
                Color c = HSLtoRGB(hue, sat, light);
                r = c.R;
                g = c.G;
                b = c.B;
            }
			else if (colorSelect == 3)
			{
                hue = (lerp(0, 360, Math.Sin(Math.PI * aspect)) + 240) %360;
                sat = lerp(0.5, 1, (1 + Math.Cos(Math.PI * aspect)) / 2);
                light = lerp(1, 0.5, Math.Sin(Math.PI * aspect / 2));
                Color c = HSLtoRGB(hue, sat, light);
                r = c.R;
                g = c.G;
                b = c.B;
            }
			else if (colorSelect == 4)
			{
                hue = (lerp(0, 360, Math.Sin(Math.PI * aspect)) + 60) % 360;
                sat = lerp(0, 1, (1 + Math.Cos(Math.PI * aspect)) / 2);
                light = lerp(0, 1, Math.Sin(Math.PI * aspect / 2));
                Color c = HSLtoRGB(hue, sat, light);
                r = c.R;
                g = c.G;
                b = c.B;
            } else
			{
                r = (int)((aspect) * 255);
                g = (int)((aspect) * 255);
                b = (int)((aspect) * 255);
            }
			return Color.FromArgb(r,g,b);
		}
		double lerp(double a, double b, double amount)
		{
			return a * (1 - amount) + b * amount;
		}
		Color HSLtoRGB(double h, double s, double l)
		{
			if (s == 0) return Color.FromArgb(0, 0, 0);
			h = h % 360;
            double c, x, m;
            c = (1 - Math.Abs(2 * l - 1)) * s;
			h = h / 60;
            x = c * (1 - Math.Abs(h % 2 - 1));
            m = l - c / 2;
            double r_p, g_p, b_p;
            if (h >= 0 && h <= 1)
            {
                r_p = c;
                g_p = x;
                b_p = 0;
            }
            else if (h >= 1 && h <= 2)
            {
                r_p = x;
                g_p = c;
                b_p = 0;
            }
            else if (h >= 2 && h <= 3)
            {
                r_p = 0;
                g_p = c;
                b_p = x;
            }
            else if (h >= 3 && h <= 4)
            {
                r_p = 0;
                g_p = x;
                b_p = c;
            }
            else if (h >= 4 && h <= 5)
            {
                r_p = x;
                g_p = 0;
                b_p = c;
            }
            else
            {
                r_p = c;
                g_p = 0;
                b_p = x;
            }
			return Color.FromArgb((int)((r_p + m) * 255), (int)((g_p + m) * 255), (int)((b_p + m) * 255));
        }
		private void UpdateFormTitle()
		{
			this.Text = $"Julia Set (N = {n}, R = {cR}, Theta = {cTheta}, Iteration = {maxiter}), Scale = {scale}, x = {u.Re}, y = {u.Im}";
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int digit = (int)Math.Floor(Math.Log10(scale) + 1.0);
            if (e.KeyCode == Keys.Up && u.Im < 2)
            {
                u.Im += Math.Pow(10, -digit);
                u.Im = (float)Math.Round(u.Im * Math.Pow(10, digit)) / Math.Pow(10, digit);
            }
            else if (e.KeyCode == Keys.Down && u.Im > -2)
            {
                u.Im -= Math.Pow(10, -digit);
                u.Im = (float)Math.Round(u.Im * Math.Pow(10, digit)) / Math.Pow(10, digit);
            }
            else if (e.KeyCode == Keys.Left && u.Im > -2)
            {
                u.Re -= Math.Pow(10, -digit);
                u.Re = (float)Math.Round(u.Re * Math.Pow(10, digit)) / Math.Pow(10, digit);
            }
            else if (e.KeyCode == Keys.Right && u.Im < 2)
            {
                u.Re += Math.Pow(10, -digit);
                u.Re = (float)Math.Round(u.Re * Math.Pow(10, digit)) / Math.Pow(10, digit);
            }
            if (e.KeyCode == Keys.Oemplus)
                scale += (float)Math.Pow(10, digit - 1) / 2;
            else if (e.KeyCode == Keys.OemMinus && scale > 1)
                scale -= (float)Math.Pow(10, digit - 1) / 2;
            else if (e.KeyCode == Keys.W)
            {
                n += 0.1F;
                n = (float)Math.Round(n * 10) / 10;
            }
            else if (e.KeyCode == Keys.S)
            {
                n -= 0.1F;
                n = (float)Math.Round(n * 10) / 10;
            }
            else if (e.KeyCode == Keys.A && maxiter > 10)
                maxiter -= 10;
            else if (e.KeyCode == Keys.D)
                maxiter += 10;
            else if (e.KeyCode == Keys.R)
            {
                scale = 1;
                u.Re = 0;
                u.Im = 0;
            }
            else if (e.KeyCode == Keys.Q)
            {
                cR -= 0.01;
                cR = Math.Round(cR * 100) / 100;
            }
            else if (e.KeyCode == Keys.E)
            {
                cR += 0.01;
                cR = Math.Round(cR * 100) / 100;
            }
            else if (e.KeyCode == Keys.Z)
            {
                cTheta--;
                cTheta = cTheta % 360;
            }
            else if (e.KeyCode == Keys.C)
            {
                cTheta++;
                cTheta = cTheta % 360;
            }
            else if (e.KeyCode == Keys.D1)
                colorSelect = 1;
            else if (e.KeyCode == Keys.D2)
                colorSelect = 2;
            else if (e.KeyCode == Keys.D3)
                colorSelect = 3;
            else if (e.KeyCode == Keys.D4)
                colorSelect = 4;
            else if (e.KeyCode == Keys.D5)
                colorSelect = 5;
            Julia_Set(w, h);
            if (e.KeyCode == Keys.Space)
            {
                SaveImage = true;
                Julia_Set(width8K, height8K);
            }
        }
        public struct CalculatedPoint
        {
            public int x;
            public int y;
            public int i;
        }
        public struct ComplexNumber
        {
            public double Re;
            public double Im;
            public ComplexNumber(double re, double im)
            {
                this.Re = re;
                this.Im = im;
            }
            public static ComplexNumber operator +(ComplexNumber x, ComplexNumber y)
            {
                return new ComplexNumber(x.Re + y.Re, x.Im + y.Im);
            }
            public static ComplexNumber operator *(ComplexNumber x, ComplexNumber y)
            {
                return new ComplexNumber(x.Re * y.Re - x.Im * y.Im,
                    x.Re * y.Im + x.Im * y.Re);
            }
            public static ComplexNumber pow(ComplexNumber x, double n)
            {
                double size = Math.Pow(x.Norm(), n / 2);
                double ang = n * Math.Atan2(x.Im, x.Re);
                return new ComplexNumber(size * Math.Cos(ang), size * Math.Sin(ang));
            }
            public double Norm()
            {
                return Re * Re + Im * Im;
            }
        }
    }
}
