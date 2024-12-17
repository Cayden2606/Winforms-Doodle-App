using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Doodle_230833F
{
    public partial class MainForm_230833F : Form
    {
        Bitmap bm;
        Graphics g;
        Pen pen = new Pen(Color.Black, 5);
        SolidBrush brush = new SolidBrush(Color.Black);

        TextureBrush charcoalBrush;
        Bitmap charcoalColoredTexture;
        Bitmap charcoalTexture;

        TextureBrush watercolorBrush;
        Bitmap watercolorColoredTexture;
        Bitmap watercolorTexture;

        Point startP = new Point(0, 0);
        Point endP = new Point(0, 0);
        Random rand = new Random();

        Graphics gShape;
        Pen penShape = new Pen(Color.Black, 5);

        Graphics gOverlay;

        bool flagDraw = false;
        bool flagErase = false;
        bool flagText = false;
        bool flagInitial = false;
        bool flagFill = false;
        bool flagColorPicker = false;
        bool flagShapes = false;
        string strText;

        List<PictureBox> shapePictureBoxes;
        int currentShapeIndex = 0;

        List<PictureBox> colourPictureBoxes;
        int currentColourIndex = 0;

        public MainForm_230833F()
        {
            InitializeComponent();
        }


        private void CaydenMiguelTheseiraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            Clipboard.SetText(attribute.Value.ToString());
        }

        private void MainForm_230833F_Load(object sender, EventArgs e)
        {
            this.Size = new Size(976, 718); 

            bm = new Bitmap(picBoxMain.Width, picBoxMain.Height);
            picBoxMain.Image = bm;

            ToolStripMenuItem.ForeColor = Color.White;
            ToolStripMenuItem.BackColor = Color.FromArgb(26, 32, 49);

            // Add all existing font into a list
            List<string> fonts = new List<string>();
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
            {
                fonts.Add(font.Name);
            }

            comboBoxFonts.Items.AddRange(fonts.ToArray());

            int arialIndex = comboBoxFonts.Items.IndexOf("Arial");
            if (arialIndex != -1)
            {
                comboBoxFonts.SelectedIndex = arialIndex;
            }
            else if (comboBoxFonts.Items.Count > 0)
            {
                comboBoxFonts.SelectedIndex = 0;
            }

            // Add predefined font Sizes
            List<int> fontSizes = new List<int> { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 36, 48, 50, 72 };
            comboBoxSize.Items.AddRange(fontSizes.ConvertAll(size => size.ToString()).ToArray());
            comboBoxSize.SelectedIndex = 4;

            // Custom Brushes
            Image charcoalBrushImage = Properties.Resources.upscaled_trans;
            charcoalBrush = new TextureBrush(charcoalBrushImage);
            charcoalTexture = charcoalBrush.Image as Bitmap;
            Console.WriteLine(charcoalTexture.ToString());

            Image watercolorBrushImage = Properties.Resources.watercolorlight;
            watercolorBrush = new TextureBrush(watercolorBrushImage);
            watercolorTexture = watercolorBrush.Image as Bitmap;

            lblCanvasSize.Text = picBoxMain.Width + " × " + picBoxMain.Height + "px";

            shapePictureBoxes = new List<PictureBox>
            {
                picBoxLineTool,
                picBoxCircleTool,
                picBoxRectangleTool,
                picBoxTriangleTool,
                picBoxRightTriangle,
                picBoxRhombusTool,
                picBoxHexagonTool,
                picBoxPentagonTool,
                picBoxFourPointStarTool,
                picBoxStarTool,
                picBoxSixPointedStarTool,
                picBoxEmojiHappyTool,
                picBoxEmojiSadTool,
                picBoxEmojiPoopTool,
                picBoxEmojiSusTool
            };

            colourPictureBoxes = new List<PictureBox>
            {
                PicBoxBlack,
                PicBoxGrey,
                PicBoxDarkRed,
                PicBoxRed,
                PicBoxOrange,
                PicBoxYellow,
                PicBoxGreen,
                PicBoxBlue,
                PicBoxIndigo,
                PicBoxPurple,
                PicBoxWhite,
                PicBoxLightGrey,
                PicBoxBrown,
                PicBoxRose,
                PicBoxGold,
                PicBoxLightYellow,
                PicBoxLime,
                PicBoxLightTurquiose,
                PicBoxBlueGrey,
                PicBoxLavender
            };
        }

        private void picBoxMain_Click(object sender, EventArgs e)
        {
            // Remove last ICON to show Color (Color seemed more important when you are currently drawing)
            picBoxBrushColor.BackColor = LastColor;
            picBoxBrushColor.Image = null;
        }

        private void ToggleColor(PictureBox picBox, bool flagBool)
        {
            // Aesthetic highlight color when click font styles
            if (flagBool)
            {
                picBox.BackColor = highlightBackColor;
            }
            else
            {
                picBox.BackColor = defaultBackColor;
            }
        }

        bool fontBold = false;
        bool fontItalic = false;
        bool fontUnderline = false;
        bool fontStrikethrough = false;

        private void fontStyles_Click(object sender, EventArgs e)
        {
            sounds("Normal");
            string tag = (sender as PictureBox).Tag.ToString();
            switch (tag)
            {
                case "Bold":
                    fontBold = !fontBold;
                    ToggleColor(picBoxBold, fontBold);
                    break;
                case "Italic":
                    fontItalic = !fontItalic;
                    ToggleColor(picBoxItalic, fontItalic);
                    break;
                case "Underline":
                    fontUnderline = !fontUnderline;
                    ToggleColor(picBoxUnderline, fontUnderline);
                    break;
                case "Strikethrough":
                    fontStrikethrough = !fontStrikethrough;
                    ToggleColor(picBoxStrikethrough, fontStrikethrough);
                    break;
            }
        }

        Point shapeCoordsStart, shapeCoordsEnd;
        private void picBoxMain_MouseDown(object sender, MouseEventArgs e)
        {
            panelBrushOptions.Visible = false;
            panelFilters.Visible = false;
            startP = e.Location;
            Console.WriteLine(flagColorPicker.ToString() + flagText.ToString() + flagFill.ToString());

            if (flagText == false && flagFill == false && flagColorPicker == false && flagShapes == false)
            {
                if (e.Button == MouseButtons.Left)
                {
                    flagDraw = true;
                    // Make a DOT
                    if (flagInitial)
                    {
                        // Draw a dot
                        g = Graphics.FromImage(bm);
                        if (flagErase == false)
                        {
                            // The different available brushes
                            switch (BrushType)
                            {
                                case "Pencil":
                                    endP.X = startP.X + 1;
                                    endP.Y = startP.Y + 1;
                                    g.DrawLine(pen, startP, endP);
                                    break;
                                case "Marker":
                                    g.FillEllipse(brush, startP.X - markerSize / 2, startP.Y - markerSize / 2, markerSize, markerSize);
                                    break;
                                case "Square":
                                    g.FillRectangle(brush, startP.X - squareSize / 2, startP.Y - squareSize / 2, squareSize, squareSize);
                                    break;
                                case "Charcoal":
                                    float angleCharcoal = (float)(rand.NextDouble() * 30 - 15);
                                    GraphicsState stateCharcoal = g.Save();
                                    Matrix transformCharcoal = new Matrix();
                                    transformCharcoal.RotateAt(angleCharcoal, new PointF(startP.X, startP.Y));
                                    g.Transform = transformCharcoal;
                                    g.DrawImage(charcoalColoredTexture, startP.X - charcoalColoredTexture.Width / 2, startP.Y - charcoalColoredTexture.Height / 2);
                                    g.Restore(stateCharcoal);
                                    break;
                                case "Watercolor":
                                    float angleWatercolor = (float)(rand.NextDouble() * 30 - 15);
                                    GraphicsState stateWatercolor = g.Save();
                                    Matrix transformWatercolor = new Matrix();
                                    transformWatercolor.RotateAt(angleWatercolor, new PointF(startP.X, startP.Y));
                                    g.Transform = transformWatercolor;
                                    g.DrawImage(watercolorColoredTexture, startP.X - watercolorColoredTexture.Width / 2, startP.Y - watercolorColoredTexture.Height / 2);
                                    g.Restore(stateWatercolor);
                                    break;
                            }
                        }
                        else
                        {
                            // Eraser tool
                            g.FillEllipse(brush, startP.X - eraserSize / 2, startP.Y - eraserSize / 2, eraserSize, eraserSize);
                        }
                        g.Dispose();
                        picBoxMain.Invalidate();
                    }
                }
            }
            else if (flagText)
            {   
                // Only allow text with correct stuff in it
                if (!flagTextFontError && !flagTextSizeError)
                {
                    strText = txtBoxText.Text;
                    string fontName;
                    if (comboBoxFonts.SelectedItem != null)
                    {
                        fontName = comboBoxFonts.SelectedItem.ToString();
                    }
                    else
                    {
                        // If it fails, get the text input
                        fontName = comboBoxFonts.Text;
                    }

                    int fontSize;

                    // Input validation
                    if (comboBoxSize.SelectedItem != null)
                    {
                        if (int.TryParse(comboBoxSize.SelectedItem.ToString(), out fontSize))
                        {
                            // Successfully parsed the selected item
                        }
                        else
                        {
                            MessageBox.Show("Invalid Font Size Selected. Please select a numeric value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (int.TryParse(comboBoxSize.Text, out fontSize))
                        {
                            // Successfully parsed the text input
                        }
                        else
                        {
                            MessageBox.Show("Invalid Font Size Entered. Please enter a numeric value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Stackable Font Style
                    FontStyle fontStyle = FontStyle.Regular;
                    if (fontBold) fontStyle |= FontStyle.Bold;
                    if (fontUnderline) fontStyle |= FontStyle.Underline;
                    if (fontStrikethrough) fontStyle |= FontStyle.Strikeout;
                    if (fontItalic) fontStyle |= FontStyle.Italic;

                    g = Graphics.FromImage(bm);

                    // Adding text to canvas with colour, anti-alias, style and size
                    Font font = new Font(fontName, fontSize, fontStyle);
                    brush = new SolidBrush(text_color);
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.DrawString(strText, font, brush, startP.X - (fontSize / 2 - strText.Length / 2), startP.Y - fontSize / 2);
                    g.Dispose();
                    picBoxMain.Invalidate();
                }
                else
                {
                    MessageBox.Show("Invalid Font Entered.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    comboBoxFonts.Text = "Arial";
                }

            }
            else if (flagFill)
            {
                Color targetColor = bm.GetPixel(e.X, e.Y);
                FloodFill(bm, e.X, e.Y, targetColor, LastColor);
                picBoxMain.Invalidate();
            }
            else if (flagColorPicker)
            {
                Color pickedColor = bm.GetPixel(e.X, e.Y);
                LastColor = pickedColor;
                picBoxBrushColor.BackColor = pickedColor;
            }
            else if (flagShapes)
            {
                penShape.Color = LastColor;
                shapeCoordsStart = new Point(e.X, e.Y);
            }
        }

        private void FloodFill(Bitmap bmp, int x, int y, Color targetColor, Color replacementColor)
        {
            // End Fill tool if current color is already the color you want to replace
            if (targetColor.ToArgb() == replacementColor.ToArgb())
                return;

            Stack<Point> pixels = new Stack<Point>();
            pixels.Push(new Point(x, y));

            while (pixels.Count > 0)
            {
                Point a = pixels.Pop();

                // check if the point is within the size of the canvas
                if (a.X < bmp.Width && a.X >= 0 && a.Y < bmp.Height && a.Y >= 0)
                {
                    // check if the current pixel has the same color as the target
                    if (bmp.GetPixel(a.X, a.Y) == targetColor)
                    {
                        // make the pixel to this color
                        bmp.SetPixel(a.X, a.Y, replacementColor);

                        // push the neighbouring points onto the stack (Left, Right, Top, Bottom)
                        pixels.Push(new Point(a.X - 1, a.Y));
                        pixels.Push(new Point(a.X + 1, a.Y));
                        pixels.Push(new Point(a.X, a.Y - 1));
                        pixels.Push(new Point(a.X, a.Y + 1));
                    }
                }
            }
        }

        Color text_color = Color.Black;
        private void picBoxColorsText_Click(object sender, EventArgs e)
        {
            // Change colour of text
            sounds("Normal");
            PictureBox clickedBox = sender as PictureBox;

            if (clickedBox != null)
            {
                text_color = clickedBox.BackColor;
            }
        }

        private void ColorDialogs_Click(object sender, EventArgs e)
        {
            sounds("Normal");

            // Color dialogs for drawing, text and filter
            string tag = (sender as PictureBox).Tag.ToString();
            switch (tag)
            {
                case "Edit Brush Colours":
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        Color color = colorDialog.Color;
                        flagText = false;
                        brush.Color = color;
                        pen.Color = color;
                        BrushColor = color;
                        picBoxBrushColor.BackColor = pen.Color;
                        picBoxBrushColor.Image = null;
                        flagErase = false;
                        LastColor = color;
                    }
                    break;
                case "Edit Text Colours":
                    if (colorDialogText.ShowDialog() == DialogResult.OK)
                    {
                        text_color = colorDialogText.Color;
                    }
                    break;
                case "Edit Filters Colours":
                    if (colorDialogFilters.ShowDialog() == DialogResult.OK)
                    {
                        filterColor = colorDialogFilters.Color.ToArgb();
                        btnFilterTransform.PerformClick();
                    }
                    break;
            }
        }

        float pencilSize = 10, markerSize = 10, squareSize = 10, charcoalSize = 10, watercolorSize = 10, eraserSize = 10, shapeSize = 5;
        private void picBoxMain_MouseMove(object sender, MouseEventArgs e)
        {
            // Update the current cursor's position status location text
            lblCoords.Text = e.X.ToString() + " × " + e.Y.ToString() + "px";

            // Need to press brush to enable drawing
            if (flagInitial == true) 
            {
                if (flagDraw == true)
                {
                    endP = e.Location;
                    g = Graphics.FromImage(bm);
                    if (flagErase == false)
                    {
                        // Use the different brush types
                        switch (BrushType)
                        {
                            case "Pencil":
                                g.DrawLine(pen, startP, endP);
                                break;
                            case "Marker":
                                g.FillEllipse(brush, endP.X - markerSize / 2, endP.Y - markerSize / 2, markerSize, markerSize);
                                break;
                            case "Square":
                                g.FillRectangle(brush, endP.X - squareSize / 2, endP.Y - squareSize / 2, squareSize, squareSize);
                                break;
                            case "Charcoal":
                                float angleCharcoal = (float)(rand.NextDouble() * 30 - 15);

                                GraphicsState stateCharcoal = g.Save();

                                Matrix transformCharcoal = new Matrix();
                                transformCharcoal.RotateAt(angleCharcoal, new PointF(endP.X, endP.Y));

                                g.Transform = transformCharcoal;
                                g.DrawImage(charcoalColoredTexture, endP.X - charcoalColoredTexture.Width / 2, endP.Y - charcoalColoredTexture.Height / 2);

                                g.Restore(stateCharcoal);

                                startP = endP;
                                break;
                            case "Watercolor":
                                float angleWatercolor = (float)(rand.NextDouble() * 30 - 15);

                                GraphicsState stateWatercolor = g.Save();

                                Matrix transformWatercolor = new Matrix();
                                transformWatercolor.RotateAt(angleWatercolor, new PointF(endP.X, endP.Y));

                                g.Transform = transformWatercolor;
                                g.DrawImage(watercolorColoredTexture, endP.X - watercolorColoredTexture.Width / 2, endP.Y - watercolorColoredTexture.Height / 2);

                                g.Restore(stateWatercolor);

                                startP = endP;
                                break;

                        }
                    }
                    else
                    {
                        // Eraser
                        g.FillEllipse(brush, endP.X - eraserSize / 2, endP.Y - eraserSize / 2, eraserSize, eraserSize);

                    }

                    g.Dispose();
                    picBoxMain.Invalidate();
                }

                startP = endP;
            }
        }

        // Change Colour of custom brush since it is an image
        private static Bitmap ChangeBitmapColor(Bitmap originalBitmap, Color newColor, int customSize)
        {
            Bitmap newBitmap = new Bitmap(customSize, customSize);

            // Cycle through every pixel of custom brush image
            for (int y = 0; y < originalBitmap.Height; y++)
            {
                for (int x = 0; x < originalBitmap.Width; x++)
                {
                    Color originalColor = originalBitmap.GetPixel(x, y);

                    // Don't change the transparent pixels
                    if (originalColor.A > 0)
                    {
                        // New custom brush color
                        Color changedColor = Color.FromArgb(originalColor.A, newColor.R, newColor.G, newColor.B);

                        // New custom brush size
                        int newX = x * customSize / originalBitmap.Width;
                        int newY = y * customSize / originalBitmap.Height;

                        newBitmap.SetPixel(newX, newY, changedColor);
                    }
                }
            }
            return newBitmap;
        }

        private void picBoxMain_MouseUp(object sender, MouseEventArgs e)
        {
            flagDraw = false;
            shapeCoordsEnd = new Point(e.X, e.Y);

            // The Ending location when drawing using shape tools
            if (flagShapes)
            {
                gShape = Graphics.FromImage(bm);
                switch (shapeType)
                {
                    case "Line":
                        gShape.DrawLine(penShape, shapeCoordsStart, shapeCoordsEnd);
                        break;
                    case "Circle":
                        if (shapeCoordsEnd.X > shapeCoordsStart.X && shapeCoordsEnd.Y > shapeCoordsStart.Y)
                        {
                            gShape.DrawEllipse(penShape, shapeCoordsStart.X, shapeCoordsStart.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }
                        else if (shapeCoordsEnd.X < shapeCoordsStart.X && shapeCoordsEnd.Y < shapeCoordsStart.Y)
                        {
                            gShape.DrawEllipse(penShape, shapeCoordsEnd.X, shapeCoordsEnd.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }
                        else if (shapeCoordsEnd.X > shapeCoordsStart.X && shapeCoordsEnd.Y < shapeCoordsStart.Y)
                        {
                            gShape.DrawEllipse(penShape, shapeCoordsStart.X, shapeCoordsEnd.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }
                        else if (shapeCoordsEnd.X < shapeCoordsStart.X && shapeCoordsEnd.Y > shapeCoordsStart.Y)
                        {
                            gShape.DrawEllipse(penShape, shapeCoordsEnd.X, shapeCoordsStart.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }


                        break;
                    case "Rectangle":
                        if (shapeCoordsEnd.X > shapeCoordsStart.X && shapeCoordsEnd.Y > shapeCoordsStart.Y)
                        {
                            gShape.DrawRectangle(penShape, shapeCoordsStart.X, shapeCoordsStart.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }
                        else if (shapeCoordsEnd.X < shapeCoordsStart.X && shapeCoordsEnd.Y < shapeCoordsStart.Y)
                        {
                            gShape.DrawRectangle(penShape, shapeCoordsEnd.X, shapeCoordsEnd.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }
                        else if (shapeCoordsEnd.X > shapeCoordsStart.X && shapeCoordsEnd.Y < shapeCoordsStart.Y)
                        {
                            gShape.DrawRectangle(penShape, shapeCoordsStart.X, shapeCoordsEnd.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }
                        else if (shapeCoordsEnd.X < shapeCoordsStart.X && shapeCoordsEnd.Y > shapeCoordsStart.Y)
                        {
                            gShape.DrawRectangle(penShape, shapeCoordsEnd.X, shapeCoordsStart.Y, Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y));
                        }

                        break;
                    case "Hexagon":
                        int hexagonCenterX = (shapeCoordsStart.X + shapeCoordsEnd.X) / 2;
                        int hexagonCenterY = (shapeCoordsStart.Y + shapeCoordsEnd.Y) / 2;
                        int hexagonRadius = Math.Min(Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y)) / 2;

                        Point[] hexagonPoints = new Point[6];
                        for (int i = 0; i < 6; i++)
                        {
                            double angle = Math.PI / 3 * i;
                            hexagonPoints[i] = new Point(
                                hexagonCenterX + (int)(hexagonRadius * Math.Cos(angle)),
                                hexagonCenterY + (int)(hexagonRadius * Math.Sin(angle))
                            );
                        }

                        gShape.DrawPolygon(penShape, hexagonPoints);
                        break;
                    case "Triangle":
                        int triangleCenterX = (shapeCoordsStart.X + shapeCoordsEnd.X) / 2;
                        int triangleCenterY = (shapeCoordsStart.Y + shapeCoordsEnd.Y) / 2;
                        int triangleRadius = Math.Min(Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y)) / 2;

                        Point[] trianglePoints = new Point[3];
                        for (int i = 0; i < 3; i++)
                        {
                            double angle = 2 * Math.PI / 3 * i;
                            trianglePoints[i] = new Point(
                                triangleCenterX + (int)(triangleRadius * Math.Cos(angle)),
                                triangleCenterY + (int)(triangleRadius * Math.Sin(angle))
                            );
                        }
                        gShape.DrawPolygon(penShape, trianglePoints);
                        break;
                    case "Pentagon":
                        int pentagonCenterX = (shapeCoordsStart.X + shapeCoordsEnd.X) / 2;
                        int pentagonCenterY = (shapeCoordsStart.Y + shapeCoordsEnd.Y) / 2;
                        int pentagonRadius = Math.Min(Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y)) / 2;

                        Point[] pentagonPoints = new Point[5];
                        for (int i = 0; i < 5; i++)
                        {
                            double angle = 2 * Math.PI / 5 * i;
                            pentagonPoints[i] = new Point(
                                pentagonCenterX + (int)(pentagonRadius * Math.Cos(angle)),
                                pentagonCenterY + (int)(pentagonRadius * Math.Sin(angle))
                            );
                        }
                        gShape.DrawPolygon(penShape, pentagonPoints);
                        break;
                    case "Star":
                        int starCenterX = (shapeCoordsStart.X + shapeCoordsEnd.X) / 2;
                        int starCenterY = (shapeCoordsStart.Y + shapeCoordsEnd.Y) / 2;
                        int starOuterRadius = Math.Min(Math.Abs(shapeCoordsEnd.X - shapeCoordsStart.X), Math.Abs(shapeCoordsEnd.Y - shapeCoordsStart.Y)) / 2;
                        int starInnerRadius = starOuterRadius / 2;

                        Point[] starPoints = new Point[10];
                        for (int i = 0; i < 10; i++)
                        {
                            double angle = Math.PI / 5 * i;
                            int radius = (i % 2 == 0) ? starOuterRadius : starInnerRadius;
                            starPoints[i] = new Point(
                                starCenterX + (int)(radius * Math.Cos(angle)),
                                starCenterY + (int)(radius * Math.Sin(angle))
                            );
                        }
                        gShape.DrawPolygon(penShape, starPoints);
                        break;
                    case "Rhombus":
                        int rhombusCenterX = (shapeCoordsStart.X + shapeCoordsEnd.X) / 2;
                        int rhombusCenterY = (shapeCoordsStart.Y + shapeCoordsEnd.Y) / 2;

                        Point[] rhombusPoints = {
                            new Point(rhombusCenterX, shapeCoordsStart.Y),
                            new Point(shapeCoordsEnd.X, rhombusCenterY),
                            new Point(rhombusCenterX, shapeCoordsEnd.Y),
                            new Point(shapeCoordsStart.X, rhombusCenterY)
                        };
                        gShape.DrawPolygon(penShape, rhombusPoints);
                        break;
                    case "RightTriangle":
                        Point rightAnglePoint = new Point(shapeCoordsStart.X, shapeCoordsEnd.Y); // Point forming the right angle
                        Point[] rightTrianglePoints = { shapeCoordsStart, rightAnglePoint, shapeCoordsEnd };
                        gShape.DrawPolygon(penShape, rightTrianglePoints);
                        break;
                    case "FourPointedStar":
                        int fourStarLeft = Math.Min(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int fourStarRight = Math.Max(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int fourStarTop = Math.Min(shapeCoordsStart.Y, shapeCoordsEnd.Y);
                        int fourStarBottom = Math.Max(shapeCoordsStart.Y, shapeCoordsEnd.Y);

                        int fourStarCenterX = (fourStarLeft + fourStarRight) / 2;
                        int fourStarCenterY = (fourStarTop + fourStarBottom) / 2;
                        int fourStarWidth = fourStarRight - fourStarLeft;
                        int fourStarHeight = fourStarBottom - fourStarTop;

                        Point[] fourStarPoints = new Point[8];

                        fourStarPoints[0] = new Point(fourStarCenterX, fourStarTop); // Top point
                        fourStarPoints[1] = new Point(fourStarCenterX + fourStarWidth / 8, fourStarCenterY - fourStarHeight / 8); // Inner top-right
                        fourStarPoints[2] = new Point(fourStarRight, fourStarCenterY); // Right point
                        fourStarPoints[3] = new Point(fourStarCenterX + fourStarWidth / 8, fourStarCenterY + fourStarHeight / 8); // Inner bottom-right
                        fourStarPoints[4] = new Point(fourStarCenterX, fourStarBottom); // Bottom point
                        fourStarPoints[5] = new Point(fourStarCenterX - fourStarWidth / 8, fourStarCenterY + fourStarHeight / 8); // Inner bottom-left
                        fourStarPoints[6] = new Point(fourStarLeft, fourStarCenterY); // Left point
                        fourStarPoints[7] = new Point(fourStarCenterX - fourStarWidth / 8, fourStarCenterY - fourStarHeight / 8); // Inner top-left

                        gShape.DrawPolygon(penShape, fourStarPoints);
                        break;
                    case "SixPointedStar":
                        int sixStarLeft = Math.Min(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int sixStarRight = Math.Max(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int sixStarTop = Math.Min(shapeCoordsStart.Y, shapeCoordsEnd.Y);
                        int sixStarBottom = Math.Max(shapeCoordsStart.Y, shapeCoordsEnd.Y);

                        int sixStarCenterX = (sixStarLeft + sixStarRight) / 2;
                        int sixStarCenterY = (sixStarTop + sixStarBottom) / 2;
                        int sixStarWidth = sixStarRight - sixStarLeft;
                        int sixStarHeight = sixStarBottom - sixStarTop;

                        // Define the points for the six-pointed star
                        Point[] sixStarpoints = new Point[12];

                        sixStarpoints[0] = new Point(sixStarCenterX, sixStarTop); // Top point
                        sixStarpoints[1] = new Point(sixStarCenterX + sixStarWidth / 8, sixStarCenterY - sixStarHeight / 4); // Inner top-right
                        sixStarpoints[2] = new Point(sixStarCenterX + sixStarWidth / 2, sixStarCenterY - sixStarHeight / 4); // Right top point
                        sixStarpoints[3] = new Point(sixStarCenterX + sixStarWidth / 4, sixStarCenterY); // Inner right
                        sixStarpoints[4] = new Point(sixStarCenterX + sixStarWidth / 2, sixStarCenterY + sixStarHeight / 4); // Right bottom point
                        sixStarpoints[5] = new Point(sixStarCenterX + sixStarWidth / 8, sixStarCenterY + sixStarHeight / 4); // Inner bottom-right
                        sixStarpoints[6] = new Point(sixStarCenterX, sixStarBottom); // Bottom point
                        sixStarpoints[7] = new Point(sixStarCenterX - sixStarWidth / 8, sixStarCenterY + sixStarHeight / 4); // Inner bottom-left
                        sixStarpoints[8] = new Point(sixStarCenterX - sixStarWidth / 2, sixStarCenterY + sixStarHeight / 4); // Left bottom point
                        sixStarpoints[9] = new Point(sixStarCenterX - sixStarWidth / 4, sixStarCenterY); // Inner left
                        sixStarpoints[10] = new Point(sixStarCenterX - sixStarWidth / 2, sixStarCenterY - sixStarHeight / 4); // Left top point
                        sixStarpoints[11] = new Point(sixStarCenterX - sixStarWidth / 8, sixStarCenterY - sixStarHeight / 4); // Inner top-left

                        gShape.DrawPolygon(penShape, sixStarpoints);
                        break;
                    case "Happy":
                        int happyLeft = Math.Min(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int happyRight = Math.Max(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int happyTop = Math.Min(shapeCoordsStart.Y, shapeCoordsEnd.Y);
                        int happyBottom = Math.Max(shapeCoordsStart.Y, shapeCoordsEnd.Y);

                        gShape.DrawImage(Properties.Resources.Happy, new Rectangle(happyLeft, happyTop, happyRight - happyLeft, happyBottom - happyTop));
                        break;
                    case "Sad":
                        int sadLeft = Math.Min(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int sadRight = Math.Max(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int sadTop = Math.Min(shapeCoordsStart.Y, shapeCoordsEnd.Y);
                        int sadBottom = Math.Max(shapeCoordsStart.Y, shapeCoordsEnd.Y);

                        gShape.DrawImage(Properties.Resources.Sad, new Rectangle(sadLeft, sadTop, sadRight - sadLeft, sadBottom - sadTop));
                        break;

                    case "Sus":
                        int susLeft = Math.Min(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int susRight = Math.Max(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int susTop = Math.Min(shapeCoordsStart.Y, shapeCoordsEnd.Y);
                        int susBottom = Math.Max(shapeCoordsStart.Y, shapeCoordsEnd.Y);

                        gShape.DrawImage(Properties.Resources.Sus, new Rectangle(susLeft, susTop, susRight - susLeft, susBottom - susTop));
                        break;

                    case "Poop":
                        int poopLeft = Math.Min(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int poopRight = Math.Max(shapeCoordsStart.X, shapeCoordsEnd.X);
                        int poopTop = Math.Min(shapeCoordsStart.Y, shapeCoordsEnd.Y);
                        int poopBottom = Math.Max(shapeCoordsStart.Y, shapeCoordsEnd.Y);

                        gShape.DrawImage(Properties.Resources.Poop, new Rectangle(poopLeft, poopTop, poopRight - poopLeft, poopBottom - poopTop));
                        break;

                }

                gShape.Dispose();
                picBoxMain.Invalidate();
            }
        }

        private void picBoxColors_Click(object sender, EventArgs e)
        {
            sounds("Normal");
            // Set color of brush
            PictureBox clickedBox = sender as PictureBox;
            if (clickedBox != null)
            {
                Color temp_color = clickedBox.BackColor;

                flagText = false;
                flagErase = false;

                brush.Color = temp_color;
                pen.Color = temp_color;
                BrushColor = temp_color;
                LastColor = temp_color;

                picBoxBrushColor.BackColor = pen.Color;
                picBoxBrushColor.Image = null;
            }
        }

        string BrushType = "Pencil";
        Color BrushColor = Color.Black;
        private void tools_Click(Object sender, EventArgs e)
        {   
            string tag = (sender as PictureBox).Tag.ToString();
            sounds("Normal");
            switch (tag)
            {
                case "Erase":
                    TrackBarDrawSize.Value = int.Parse(eraserSize.ToString());
                    brush = new SolidBrush(picBoxMain.BackColor);
                    picBoxBrushColor.Image = Properties.Resources.EraserTool;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    flagErase = true;
                    flagText = false;
                    flagFill = false;
                    panelText.Visible = false;
                    flagShapes = false;
                    flagColorPicker = false;
                    lblDrawSize.Text = TrackBarDrawSize.Value.ToString();
                    txtBoxDrawSize.Text = TrackBarDrawSize.Value.ToString();
                    aesthetic_Background_Toggle(sender, e);
                    break;
                case "Text":
                    picBoxBrushColor.Image = Properties.Resources.TextTool;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    flagDraw = false;
                    flagText = true;
                    flagColorPicker = false;
                    flagFill = false;
                    flagShapes = false;
                    aesthetic_Background_Toggle(sender, e);
                    panelText.Visible = !panelText.Visible;
                    panelText.BringToFront();
                    break;
                case "Pencil":
                    flagInitial = true;
                    picBoxBrushColor.Image = Properties.Resources.PencilTool;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    flagErase = false;
                    flagText = false;
                    flagFill = false;
                    flagColorPicker = false;
                    flagShapes = false;

                    panelText.Visible = false;
                    brush = new SolidBrush(LastColor);
                    pen.Color = LastColor;

                    Console.WriteLine(LastColor.ToString());
                    Console.WriteLine(charcoalTexture);
                    aesthetic_Background_Toggle(sender, e);

                    // Change size of brushes
                    charcoalColoredTexture = ChangeBitmapColor(charcoalTexture, LastColor, int.Parse(charcoalSize.ToString()));
                    watercolorColoredTexture = ChangeBitmapColor(watercolorTexture, LastColor, int.Parse(watercolorSize.ToString()));

                    switch (BrushType)
                    {
                        case "Pencil":
                            TrackBarDrawSize.Value = int.Parse(pen.Width.ToString());
                            break;
                        case "Marker":
                            TrackBarDrawSize.Value = int.Parse(markerSize.ToString());
                            break;
                        case "Square":
                            TrackBarDrawSize.Value = int.Parse(squareSize.ToString());
                            break;
                        case "Charcoal":
                            TrackBarDrawSize.Value = int.Parse(charcoalSize.ToString());
                            break;
                        case "Watercolor":
                            TrackBarDrawSize.Value = int.Parse(watercolorSize.ToString());
                            break;
                    }
                    // Update the visual size indicator of the respective brush type
                    lblDrawSize.Text = TrackBarDrawSize.Value.ToString();
                    txtBoxDrawSize.Text = TrackBarDrawSize.Value.ToString();

                    break;
                case "Clear":
                    g = Graphics.FromImage(bm);
                    Rectangle rect = picBoxMain.ClientRectangle;
                    g.FillRectangle(new SolidBrush(Color.GhostWhite), rect);
                    g.Dispose();
                    picBoxMain.Invalidate();
                    panelText.Visible = false;
                    flagFill = false;
                    flagColorPicker = false;
                    flagShapes = false;
                    aesthetic_Background_Toggle(sender, e);

                    // Avoid the need to press draw button again to continue drawing
/*                    tools_Click(picBoxPencil, EventArgs.Empty);
*/
                    picBoxBrushColor.Image = Properties.Resources.clear;
                    picBoxBrushColor.BackColor = Color.Transparent;


                    break;
                case "ColorPicker":
                    picBoxBrushColor.Image = Properties.Resources.ColorPickerTool;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    flagFill = false;
                    flagDraw = false;
                    flagErase = false;
                    flagText = false;
                    panelText.Visible = false;
                    flagColorPicker = true;
                    flagShapes = false;
                    aesthetic_Background_Toggle(sender, e);
                    break;

                case "Brush Type":
                    panelBrushOptions.BringToFront();
                    panelBrushOptions.Visible = !panelBrushOptions.Visible;
                    flagInitial = true;
                    flagDraw = false;
                    flagErase = false;
                    flagText = false;
                    brush = new SolidBrush(BrushColor);
                    panelText.Visible = false;
                    flagFill = false;
                    flagColorPicker = false;
                    flagShapes = false;
                    picBoxBrushColor.Image = Properties.Resources.BrushIcon;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    break;
                case "Fill Tool":
                    flagFill = true;
                    flagDraw = false;
                    flagErase = false;
                    flagText = false;
                    panelText.Visible = false;
                    flagColorPicker = false;
                    flagShapes = false;
                    aesthetic_Background_Toggle(sender, e);
                    picBoxBrushColor.Image = Properties.Resources.FillTool;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    break;
                case "Filters":
                    flagFill = false;
                    flagDraw = false;
                    flagErase = false;
                    flagText = false;
                    panelText.Visible = false;
                    flagColorPicker = false;
                    flagShapes = false;
                    panelFilters.Visible = !panelFilters.Visible;
                    picBoxBrushColor.Image = Properties.Resources.filters;
                    picBoxBrushColor.BackColor = Color.Transparent;
                    break;
            }

        }

        string shapeType;
        private void shapes_tool_Click(object sender, EventArgs e)
        {
            sounds("Normal");
            // Get size of shape pen
            TrackBarDrawSize.Value = int.Parse(shapeSize.ToString());

            string tag = (sender as PictureBox).Tag.ToString();
            flagShapes = true;
            flagFill = false;
            flagDraw = false;
            flagErase = false;
            flagText = false;
            panelText.Visible = false;
            flagColorPicker = false;
            picBoxBrushColor.BackColor = Color.Transparent; 
            switch (tag)
            {
                case "LineTool":
                    picBoxBrushColor.Image = Properties.Resources.LineTool;
                    shapeType = "Line";
                    break;
                case "CircleTool":
                    picBoxBrushColor.Image = Properties.Resources.OvalTool;
                    shapeType = "Circle";
                    break;
                case "RectangleTool":
                    picBoxBrushColor.Image = Properties.Resources.RectangleTool;
                    shapeType = "Rectangle";
                    break;
                case "HexagonTool":
                    picBoxBrushColor.Image = Properties.Resources.HexagonTool;
                    shapeType = "Hexagon";
                    break;
                case "TriangleTool":
                    picBoxBrushColor.Image = Properties.Resources.TriangleTool;
                    shapeType = "Triangle";
                    break;
                case "PentagonTool":
                    picBoxBrushColor.Image = Properties.Resources.PentagonTool;
                    shapeType = "Pentagon";
                    break;
                case "StarTool":
                    picBoxBrushColor.Image = Properties.Resources.FivePointStarTool;
                    shapeType = "Star";
                    break;
                case "RhombusTool":
                    picBoxBrushColor.Image = Properties.Resources.DiamondTool;
                    shapeType = "Rhombus";
                    break;
                case "RightTriangleTool":
                    picBoxBrushColor.Image = Properties.Resources.RightTriangleTool;
                    shapeType = "RightTriangle";
                    break;
                case "FourPointedStarTool":
                    picBoxBrushColor.Image = Properties.Resources.FourPointStarTool;
                    shapeType = "FourPointedStar";
                    break;
                case "SixPointedStarTool":
                    picBoxBrushColor.Image = Properties.Resources.SixPointStarTool;
                    shapeType = "SixPointedStar";
                    break;
                case "EmojiHappyTool":
                    picBoxBrushColor.Image = Properties.Resources.Happy;
                    shapeType = "Happy";
                    break;
                case "EmojiSadTool":
                    picBoxBrushColor.Image = Properties.Resources.Sad;
                    shapeType = "Sad";
                    break;
                case "EmojiSusTool":
                    picBoxBrushColor.Image = Properties.Resources.Sus;
                    shapeType = "Sus";
                    break;
                case "EmojiPoopTool":
                    picBoxBrushColor.Image = Properties.Resources.Poop;
                    shapeType = "Poop";
                    break;
            }
            lblDrawSize.Text = TrackBarDrawSize.Value.ToString();
            txtBoxDrawSize.Text = TrackBarDrawSize.Value.ToString();

            aesthetic_Background_Toggle(sender, e);
        }

        private void picBoxSave_Click(object sender, EventArgs e)
        {
            // Save bmp to local drive
            sounds("Control");
            picBoxBrushColor.Image = Properties.Resources.Save;
            picBoxBrushColor.BackColor = Color.Transparent;
            aesthetic_Background_Toggle(sender, e);
            using (SaveFileDialog sfdlg = new SaveFileDialog())
            {
                sfdlg.Title = "Save Dialog";
                sfdlg.Filter = "BMP Image (*.bmp)|*.bmp|PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|All files (*.*)|*.*";
                if (sfdlg.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap bmp = new Bitmap(picBoxMain.Width, picBoxMain.Height))
                    {
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        picBoxMain.DrawToBitmap(bmp, rect);

                        string fileExtension = System.IO.Path.GetExtension(sfdlg.FileName).ToLower();
                        ImageFormat imgFormat = ImageFormat.Bmp;

                        switch (fileExtension)
                        {
                            case ".png":
                                imgFormat = ImageFormat.Png;
                                break;
                            case ".jpg":
                            case ".jpeg":
                                imgFormat = ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                            default:
                                imgFormat = ImageFormat.Bmp;
                                break;
                        }

                        bmp.Save(sfdlg.FileName, imgFormat);
                        MessageBox.Show("File Saved Successfully");
                    }
                }
            }
        }


        Color LastColor = Color.Black;

        private void toolTips_Hover(object sender, EventArgs e)
        {
            // This was before I realised Winforms has tooltips in the toolbox

            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                // Convert the PictureBox location to screen coordinates
                Point pt = pictureBox.PointToScreen(Point.Empty);

                // Convert screen coordinates to form coordinates for the label location
                Point formCoordinates = this.PointToClient(pt);

                // Set the label's location below the PictureBox
                lblToolTips.Location = new Point(formCoordinates.X, formCoordinates.Y + pictureBox.Height);

                // Set the label's text and make it visible
                lblToolTips.Text = pictureBox.Tag.ToString();
                lblToolTips.Visible = true;
            }

        }

        private void toolTips_Leave(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            if (pictureBox != null)
            {
                lblToolTips.Visible = false;
            }
        }


        private void TrackBarDrawSize_Scroll(object sender, EventArgs e)
        {
            // Take the value of trackbar as the size
            if (flagErase)
            {
                eraserSize = TrackBarDrawSize.Value;

            }
            else if (flagShapes)
            {
                shapeSize = TrackBarDrawSize.Value;
                penShape = new Pen(Color.Black, shapeSize);
            }
            else
            {
                switch (BrushType)
                {
                    case "Pencil":

                        pen.Width = TrackBarDrawSize.Value;
                        pencilSize = TrackBarDrawSize.Value;
                        break;
                    case "Marker":

                        markerSize = TrackBarDrawSize.Value;
                        break;
                    case "Square":

                        squareSize = TrackBarDrawSize.Value;
                        break;
                    case "Charcoal":
                        charcoalSize = TrackBarDrawSize.Value;
                        break;
                    case "Watercolor":
                        watercolorSize = TrackBarDrawSize.Value;
                        break;
                }
            }

            lblDrawSize.Text = TrackBarDrawSize.Value.ToString();
            txtBoxDrawSize.Text = TrackBarDrawSize.Value.ToString();

        }

        bool flagTextSizeError = false;

        private void comboBoxSize_TextChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox comboBox = sender as System.Windows.Forms.ComboBox;

            // Validation of text input of combo box
            if (!double.TryParse(comboBox.Text, out _))
            {
                MessageBox.Show("Invalid Font Size Entered. Please enter a numeric value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                flagTextSizeError = true;
            }
            else
            {
                flagTextSizeError = false;
            }
        }

        bool flagTextFontError = false;
        private void comboBoxFonts_TextChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ComboBox comboBox = sender as System.Windows.Forms.ComboBox;

            // Check if the text in the ComboBox is not one of the predefined items
            if (!comboBox.Items.Contains(comboBox.Text))
            {
                // If the text is not valid, show an error message 
                errorProvider.SetError(comboBox, "Invalid entry. Please select an item from the list.");
                flagTextFontError = true;
            }
            else
            {
                // Clear the error if the text is valid
                errorProvider.SetError(comboBox, string.Empty);
                flagTextFontError = false;

            }
        }

        private void txtBoxDrawSize_TextChanged(object sender, EventArgs e)
        {
            // Check if all characters are letters
            if (txtBoxDrawSize.Text.All(char.IsLetter))
            {
                txtBoxDrawSize.Text = "";
                MessageBox.Show("Please enter a numeric value for the pen size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtBoxDrawSize.Text = "1";
                return;
            }

            // Try parsing the input to a float
            if (float.TryParse(txtBoxDrawSize.Text, out float newSize))
            {
                // Ensure the new size is within acceptable range
                if (newSize <= 1000)
                {
                    lblDrawSize.Text = newSize.ToString();

                    if (newSize > TrackBarDrawSize.Maximum)
                    {
                        TrackBarDrawSize.Value = TrackBarDrawSize.Maximum;
                    }
                    else if (newSize < TrackBarDrawSize.Minimum)
                    {
                        TrackBarDrawSize.Value = TrackBarDrawSize.Minimum;
                    }
                    else
                    {
                        TrackBarDrawSize.Value = (int)newSize;
                    }
                }
                else
                {
                    MessageBox.Show("Pen Size too big. Keep it <= 1000.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pen.Width = 1000;
                    lblDrawSize.Text = "1000";
                    txtBoxDrawSize.Text = "1000";
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid numeric value for the pen size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            TrackBarDrawSize_Scroll(sender, e);
        }


        private void picBoxFolder_Click(object sender, EventArgs e)
        {
            sounds("Control");
            panelOpenFile.Visible = !panelOpenFile.Visible;
            flagShapes = false;
            picBoxBrushColor.Image = Properties.Resources.Folder;
            picBoxBrushColor.BackColor = Color.Transparent;
            gOverlay = Graphics.FromImage(bm);
        }

        private void openFile_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            panelOpenFile.Visible = false;

            gOverlay = Graphics.FromImage(bm);
            switch (button.Tag.ToString())
            {
                case "Open File":
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Title = "Select Image File";
                        openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*";

                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                Image img = Image.FromFile(openFileDialog.FileName);
                                gOverlay.DrawImage(img, new Rectangle(0, 0, picBoxMain.Width, picBoxMain.Height));

                                gOverlay.Dispose();
                                picBoxMain.Invalidate();

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error loading image: {ex.Message}");
                            }
                        }
                    }
                    break;
                case "Open Campus.bmp": // load nyp campus image
                    Image imgCampus = Properties.Resources.campus;
                    gOverlay.DrawImage(imgCampus, new Rectangle(0, 0, picBoxMain.Width + 2, picBoxMain.Height + 2));
                    gOverlay.Dispose();
                    picBoxMain.Invalidate();
                    break;
                case "New File": // Same as clear tool
                    g = Graphics.FromImage(bm);
                    Rectangle rect = picBoxMain.ClientRectangle;
                    g.FillRectangle(new SolidBrush(Color.GhostWhite), rect);
                    g.Dispose();
                    picBoxMain.Invalidate();
                    break;
            }


        }

        private void picBoxMain_MouseEnter(object sender, EventArgs e)
        { 
            // Preload the texture with color of custom brush after change size
            charcoalColoredTexture = ChangeBitmapColor(charcoalTexture, LastColor, int.Parse(charcoalSize.ToString()));
            watercolorColoredTexture = ChangeBitmapColor(watercolorTexture, LastColor, int.Parse(watercolorSize.ToString()));

        }


        private void brush_type_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            Console.WriteLine(button.Tag.ToString());
            tools_Click(picBoxPencil, EventArgs.Empty);
            panelBrushOptions.Visible = false;
            flagDraw = false;

            // choose the different brushes
            switch (button.Tag.ToString())
            {
                case "Pencil":
                    BrushType = "Pencil";
                    TrackBarDrawSize.Value = int.Parse(pen.Width.ToString());
                    break;
                case "Marker":
                    TrackBarDrawSize.Value = int.Parse(markerSize.ToString());
                    BrushType = "Marker";
                    break;
                case "Square":
                    BrushType = "Square";
                    TrackBarDrawSize.Value = int.Parse(squareSize.ToString());
                    break;
                case "Charcoal":
                    TrackBarDrawSize.Value = int.Parse(charcoalSize.ToString());
                    BrushType = "Charcoal";
                    break;
                case "Watercolor":
                    TrackBarDrawSize.Value = int.Parse(watercolorSize.ToString());
                    BrushType = "Watercolor";
                    break;
            }

            lblDrawSize.Text = TrackBarDrawSize.Value.ToString();
            txtBoxDrawSize.Text = TrackBarDrawSize.Value.ToString();
            Console.WriteLine(BrushType);
        }

        int filterColor = 0x00FF00FF;
        private void filters_type_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = sender as System.Windows.Forms.Button;
            Console.WriteLine(button.Tag.ToString());
            tools_Click(picBoxPencil, EventArgs.Empty);
            panelFilters.Visible = false;
            flagDraw = false;

            switch (button.Tag.ToString())
            {
                case "GreyScale":
                    if (bm == null)
                        return;

                    for (int y = 0; y < bm.Height; y++)
                    {
                        for (int x = 0; x < bm.Width; x++)
                        {
                            Color originalColor = bm.GetPixel(x, y);
                            int grayValue = (int)((originalColor.R + originalColor.G + originalColor.B) / 3);
                            Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                            bm.SetPixel(x, y, grayColor);
                        }
                    }
                    picBoxMain.Refresh();
                    break;
                case "Luminosity":
                    if (bm == null)
                        return;

                    for (int y = 0; y < bm.Height; y++)
                    {
                        for (int x = 0; x < bm.Width; x++)
                        {
                            Color originalColor = bm.GetPixel(x, y);
                            int grayValue = (int)(0.21 * originalColor.R + 0.72 * originalColor.G + 0.07 * originalColor.B);
                            Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);
                            bm.SetPixel(x, y, grayColor);
                        }
                    }
                    picBoxMain.Refresh();
                    break;
                case "Transform":
                    if (bm == null)
                        return;

                    int pixelcol;
                    for (int y = 0; y < bm.Height; y++)
                    {
                        for (int x = 0; x < bm.Width; x++)
                        {
                            pixelcol = bm.GetPixel(x, y).ToArgb();

                            int alpha = (int)(pixelcol & 0xFF000000);
                            int filteredColor = pixelcol & filterColor;

                            // Combine the alpha channel with the filtered color
                            Color transformedColor = Color.FromArgb(alpha | filteredColor);
                            bm.SetPixel(x, y, transformedColor);
                        }
                    }
                    picBoxMain.Refresh();
                    break;
            }
        }

        private void sounds(string type)
        {
            SoundPlayer player; // Play button sounds
            switch (type)
            {
                case "Delete":
                    System.IO.Stream click_del = Properties.Resources.BACKSPACE_press;
                    player = new SoundPlayer(click_del);
                    player.Play();
                    break;
                case "Normal":
                    System.IO.Stream click_normal = Properties.Resources.ENTER_press;
                    player = new SoundPlayer(click_normal);
                    player.Play();
                    break;
                case "Control":
                    System.IO.Stream click_control = Properties.Resources.SPACE_press;
                    player = new SoundPlayer(click_control);
                    player.Play();
                    break;
                default:
                    return;
            }
            player.Dispose();
        }
        private void MainForm_230833F_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard inputs shortcut

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.N:
                        picBoxFolder_Click(picBoxFolder, EventArgs.Empty);
                        btnNewFile.PerformClick();
                        sounds("Delete");
                        break;
                    case Keys.O:
                        picBoxFolder_Click(picBoxFolder, EventArgs.Empty);
                        btnOpenFile.PerformClick();
                        break;
                    case Keys.C:
                        picBoxFolder_Click(picBoxFolder, EventArgs.Empty);
                        btnOpenCampus.PerformClick();
                        break;
                    case Keys.S:
                        picBoxSave_Click(picBoxSave, EventArgs.Empty);
                        break;
                    case Keys.B:
                        tools_Click(picBoxBrushType, EventArgs.Empty);
                        break;
                }

            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.B:
                        tools_Click(picBoxPencil, EventArgs.Empty);
                        break;
                    case Keys.E:
                        tools_Click(picBoxErase, EventArgs.Empty);
                        break;
                    case Keys.G:
                        tools_Click(picBoxFillTool, EventArgs.Empty);
                        break;
                    case Keys.I:
                        tools_Click(picBoxColorPicker, EventArgs.Empty);
                        break;
                    case Keys.Delete:
                        sounds("Delete");
                        tools_Click(picBoxClear, EventArgs.Empty);
                        break;
                    case Keys.T:
                        tools_Click(picBoxText, EventArgs.Empty);
                        break;
                    case Keys.F:
                        tools_Click(picBoxFilters, EventArgs.Empty);
                        break;
                    case Keys.S:
                        // Cycle through shapes
                        shapes_tool_Click(shapePictureBoxes[currentShapeIndex], EventArgs.Empty);
                        currentShapeIndex = (currentShapeIndex + 1) % shapePictureBoxes.Count;
                        break;
                    case Keys.C:
                        // Cycle through Colours
                        picBoxColors_Click(colourPictureBoxes[currentColourIndex], EventArgs.Empty);
                        currentColourIndex = (currentColourIndex + 1) % colourPictureBoxes.Count;
                        break;
                    case Keys.Back:
                        sounds("Delete");
/*                        tools_Click(picBoxClear, EventArgs.Empty);
*/                        break;
                    case Keys.Space:
                        sounds("Control");
                        break;
                    default:
                        sounds("Normal");
                        break;
                }
            }

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lblShortCuts.Visible = !lblShortCuts.Visible;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        PictureBox selectedPictureBox = null;
        System.Windows.Forms.Button selectedButton = null;
        Color defaultBackColor = Color.Transparent;
        Color highlightBackColor = Color.FromArgb(82, 83, 92);

        private void aesthetic_Background_Toggle(object sender, EventArgs e)
        {

            if (sender is PictureBox clickedPictureBox)
            {
                // If the clicked PictureBox is already selected, deselect it
                if (clickedPictureBox == selectedPictureBox)
                {
                    clickedPictureBox.BackColor = defaultBackColor;
                    selectedPictureBox = null;
                }
                else
                {
                    // Deselect the previously selected PictureBox
                    if (selectedPictureBox != null)
                    {
                        selectedPictureBox.BackColor = defaultBackColor;
                    }

                    // Select the new PictureBox
                    clickedPictureBox.BackColor = highlightBackColor;
                    selectedPictureBox = clickedPictureBox;
                }
            }
            else if (sender is System.Windows.Forms.Button clickedButton)
            {
                // If the clicked Button is already selected, deselect it
                if (clickedButton == selectedButton)
                {
                    clickedButton.BackColor = defaultBackColor;
                    selectedButton = null;
                }
                else
                {
                    // Deselect the previously selected Button
                    if (selectedButton != null)
                    {
                        selectedButton.BackColor = defaultBackColor;
                    }

                    // Select the new Button
                    clickedButton.BackColor = highlightBackColor;
                    selectedButton = clickedButton;
                }
            }
        }

        private void aesthetic_Background_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox clickedPictureBox)
            {
                clickedPictureBox.BackColor = Color.FromArgb(82, 83, 92);
            }
        }

        private void aesthetic_Background_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is PictureBox clickedPictureBox)
            {
                clickedPictureBox.BackColor = defaultBackColor;
            }
        }
    }
}


