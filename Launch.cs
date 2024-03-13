using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
[assembly: AssemblyTitle("QRCode Generator")]
[assembly: AssemblyProduct("QRCode Generator")]
[assembly: AssemblyCopyright("Copyright (C) 2024 lalaki.cn")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
namespace QRCodeGenerator
{
    class Launch : Form
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();
        Bitmap customIcon;
        public Launch()
        {
            Icon = SystemIcons.Application;
            Height = (int)(Screen.PrimaryScreen.WorkingArea.Height * 0.7);
            Width = (int)(Height * 0.6);
            Text = "QRCode Generator";
            TopMost = true;
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Resize += (_, __) =>
            {
                if (WindowState == FormWindowState.Maximized) WindowState = FormWindowState.Normal;
            };
            const int titlePadding = 32;
            Paint += (_, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(Color.HotPink, 7f))
                    g.DrawLine(pen, Width, 0, 0, 0);
                using (var border = new Pen(Color.FromArgb(55, 0, 0, 0)))
                    g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
                var font = new Font("", 16f);
                g.DrawString("QRCode Generator", font, Brushes.Black, 14, titlePadding);
            };
            MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Tag = new Point(-e.X, -e.Y);
                    Cursor = Cursors.SizeAll;
                }
            };
            MouseMove += (_, __) =>
            {
                if (Cursor == Cursors.SizeAll && Tag is Point pos1)
                {
                    Point pos = MousePosition;
                    pos.Offset(pos1.X, pos1.Y);
                    Location = pos;
                }
            };
            MouseUp += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    Cursor = Cursors.Default;
            };
            const int inputHeight = 80;
            const int padding = 50;
            var close = new Label()
            {
                Font = new Font("", 22f),
                Height = 32,
                Width = 32,
                Top = 25,
                Text = "×"
            };
            close.MouseHover += (__, _) => close.ForeColor = Color.IndianRed;
            close.MouseLeave += (__, _) => close.ForeColor = DefaultForeColor;
            close.Left = Width - close.Width - 20;
            close.Click += (__, _) => { Hide(); Application.Exit(); };
            Controls.Add(close);
            var input = new TextBox()
            {
                Multiline = true
            };
            Controls.Add(input);
            var pic = new PictureBox() { Width = Width - padding, Height = Width - padding, SizeMode = PictureBoxSizeMode.Zoom, Top = padding + titlePadding, Left = padding / 2 };
            Controls.Add(pic);
            input.TextChanged += (_, __) =>
            {
                var text = input.Text.Trim();
                if (string.IsNullOrEmpty(text)) return;
                var bw = new ZXing.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = Width,
                        Height = Width,
                        Margin = 0,
                        PureBarcode = false,
                    }
                };
                bw.Options.Hints.Add(ZXing.EncodeHintType.CHARACTER_SET, "UTF-8");
                bw.Options.Hints.Add(ZXing.EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.Q);
                Bitmap bm = null;
                try { bm = bw.Write(text); }
                catch (ZXing.WriterException e1)
                {
                    MessageBox.Show(e1.Message);
                    return;
                }
                if (customIcon != null)
                {
                    var icon = customIcon;
                    const int side = 80;
                    var resizedIcon = new Bitmap(side, side);
                    using (Graphics graphics = Graphics.FromImage(resizedIcon))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(icon, 0, 0, resizedIcon.Width, resizedIcon.Height);
                    }
                    var g = Graphics.FromImage(bm);
                    g.DrawImage(resizedIcon, (bm.Width - resizedIcon.Width) / 2, (bm.Height - resizedIcon.Height) / 2, resizedIcon.Width, resizedIcon.Height);
                }
                pic.Image = bm;
            };
            input.Bounds = new Rectangle(padding / 2, pic.Top + pic.Height + padding, Width - padding, inputHeight);
            Height = input.Top + input.Height + titlePadding - 10;
            pic.ContextMenuStrip = new ContextMenuStrip();
            var copy = new ToolStripMenuItem
            {
                Text = "Copy",
            };
            copy.Click += (_, __) =>
            {
                if (pic.Image != null) Clipboard.SetImage(pic.Image);
            };
            var save = new ToolStripMenuItem
            {
                Text = "Save",
            };
            save.Click += (_, __) =>
            {
                if (pic.Image != null)
                {
                    var saveDialog = new SaveFileDialog()
                    {
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        FileName = "QRCode_1.png",
                        Filter = "PNG (*.png)|*.png"
                    };
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        pic.Image.Save(saveDialog.FileName, ImageFormat.Png);
                    }
                }
            };
            var changeIcon = new ToolStripMenuItem
            {
                Text = "Change Icon",
            };
            changeIcon.Click += (_, __) =>
            {
                var imgSelect = new OpenFileDialog()
                {
                    Multiselect = false,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Filter = "Image file (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
                };
                if (imgSelect.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        customIcon = new Bitmap(imgSelect.FileName);
                        input.AppendText(" ");
                    }
                    catch (ArgumentException e1)
                    {
                        MessageBox.Show(e1.Message);
                    }
                }
            };
            pic.ContextMenuStrip.Items.Add(copy);
            pic.ContextMenuStrip.Items.Add(changeIcon);
            pic.ContextMenuStrip.Items.Add(save);
            input.AppendText("Hello, I am lalakii");
        }

        [STAThread]
        static void Main()
        {
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var main = Assembly.GetExecutingAssembly();
            foreach (var dll in main.GetManifestResourceNames())
            {
                using (var dllStream = main.GetManifestResourceStream(dll))
                using (var ms = new MemoryStream())
                {
                    dllStream.CopyTo(ms);
                    AppDomain.CurrentDomain.AssemblyResolve += (__, _) => Assembly.Load(ms.ToArray());
                }
            }
            Application.Run(new Launch());
        }
    }
}