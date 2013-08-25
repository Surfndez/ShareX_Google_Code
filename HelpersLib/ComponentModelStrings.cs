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
using System.Linq;
using System.Text;

namespace HelpersLib
{
    public static class ComponentModelStrings
    {
        public const string AfterCaptureClipboard = "After capture / clipboard";
        public const string AfterCaptureClipboard_ClipboardContentFormat = "Clipboard content format after uploading. Supported variables: $url, $shorturl, $thumbnailurl, $deletionurl, $folderpath, $foldername, $filepath, $filename and other variables such as %y-%m-%d etc.";
        public const string Interaction = "Interaction";
        public const string Interaction_DisableNotifications = "Disable notifications";
        public const string UploadText = "Upload text";
        public const string UploadText_TextFileExtension = "File extension when saving text to the local hard disk.";
        public const string UploadText_TextFormat = "Text format e.g. csharp, cpp, etc.";
    }
}