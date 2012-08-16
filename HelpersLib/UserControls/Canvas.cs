﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (C) 2012 ShareX Developers

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HelpersLib
{
    public class Canvas : UserControl
    {
        public delegate void DrawEventHandler(Graphics g);
        public event DrawEventHandler Draw;

        public int Interval { get; set; }

        private Timer timer;
        private bool needPaint;
        private Bitmap canvas;

        public Canvas()
        {
            Interval = 100;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void Start()
        {
            Stop();

            timer = new Timer();
            timer.Interval = Interval;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            base.Dispose(disposing);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            needPaint = true;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (needPaint)
            {
                OnDraw(e.Graphics);
                needPaint = false;
            }
        }

        protected void OnDraw(Graphics g)
        {
            if (Draw != null)
            {
                Draw(g);
            }
        }
    }
}