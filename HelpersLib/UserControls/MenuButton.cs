﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (C) 2008-2013 ShareX Developers

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HelpersLib
{
    public class MenuButton : Button
    {
        [DefaultValue(null)]
        public ContextMenuStrip Menu { get; set; }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);

            if (Menu != null && mevent.Button == MouseButtons.Left)
            {
                Menu.Show(this, mevent.Location);
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            int arrowX = ClientRectangle.Width - 14;
            int arrowY = ClientRectangle.Height / 2 - 1;

            Brush brush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow;
            Point[] arrows = new Point[] { new Point(arrowX, arrowY), new Point(arrowX + 7, arrowY), new Point(arrowX + 3, arrowY + 4) };
            pevent.Graphics.FillPolygon(brush, arrows);
        }
    }
}