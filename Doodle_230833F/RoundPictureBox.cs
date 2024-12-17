using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Doodle_230833F
{
    internal class RoundPictureBox: PictureBox
    {
        public RoundPictureBox()
        {
            this.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            GraphicsPath grpath =  new GraphicsPath();
            grpath.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
            this.Region = new System.Drawing.Region(grpath);
            base.OnPaint(pe);
        }
    }
}
