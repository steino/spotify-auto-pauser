/*
 * Copyright (c) 2012, Paul Berruti
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpotifyAutoPauser
{
    static class Program
    {
        /* Main Application */

        private static NotifyIcon _trayIcon = new NotifyIcon();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Auto-run check
            AutoRun.AutoRunCheck();

            // Check for updates
            Updater.Check();

            // Setup the session handler
            _ss = new SessionSwitchEventHandler(SessionSwitch);
            SystemEvents.SessionSwitch += _ss;

            // Setup the tray icon and context menu
            ContextMenu cm = new ContextMenu();
            MenuItem lob = new MenuItem("Launch on Boot", new EventHandler(LaunchOnBootClick));
            lob.Checked = AutoRun.Run;
            cm.MenuItems.Add(lob);
            cm.MenuItems.Add(new MenuItem("Quit", new EventHandler(QuitClick)));

            _trayIcon.Icon = Properties.Resources.MainIcon;
            _trayIcon.Text = "Spotify Auto Pauser";
            _trayIcon.ContextMenu = cm;
            _trayIcon.Visible = true;
            _trayIcon.MouseClick += new MouseEventHandler(TrayClick);

            // Wait until the user exits
            Application.Run();
        }

        /* Clicking the tray icon */
        private static void TrayClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetSpotifyState(!IsSpotifyPlaying());
            }
        }

        /* Clicking the launch on boot option */
        private static void LaunchOnBootClick(object sender, EventArgs e)
        {
            AutoRun.Run = !AutoRun.Run;
            ((MenuItem)sender).Checked = AutoRun.Run;
        }

        /* Clicking the quit button */
        private static void QuitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }



        /* Session Lock/Unlock */

        private static SessionSwitchEventHandler _ss;

        private static void SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _wasPlaying = IsSpotifyPlaying();
                SetSpotifyState(false);
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                SetSpotifyState(_wasPlaying);
            }
        }



        /* Spotify */

        private static bool _wasPlaying = false;

        private static Process[] GetSpotifyProcesses()
        {
            return Process.GetProcessesByName("spotify");
        }

        private static bool IsSpotifyRunning()
        {
            Process[] procs = GetSpotifyProcesses();
            if (procs.Length > 0) return true;
            return false;
        }

        private static string GetSpotifyTitle()
        {
            if (!IsSpotifyRunning()) return null;
            Process[] procs = GetSpotifyProcesses();

            // I'm only expecting there to be one...
            return procs[0].MainWindowTitle;
        }

        private static bool IsSpotifyPlaying()
        {
            string title = GetSpotifyTitle();
            if (title == null || title.ToUpper() == "SPOTIFY") return false;
            return true;
        }

        private static IntPtr GetSpotifyHwnd()
        {
            if (!IsSpotifyRunning()) return IntPtr.Zero;
            Process[] procs = GetSpotifyProcesses();
            return procs[0].Handle;
        }

        private static void SetSpotifyState(bool Play)
        {
            int attempts = 0;

            // Try up to 5 times then bail in case something's weird, multiple tries are needed in case of weird timing issues
            // Play && !IsPlaying ==> If we should be playing and we aren't yet, keep trying
            // !Play && IsPlaying ==> If we shouldn't be playing and we are still, keep trying
            while (attempts < 5 && ((Play && !IsSpotifyPlaying()) || (!Play && IsSpotifyPlaying())))
            {
                SendMessage(GetSpotifyHwnd(), WM_APPCOMMAND, IntPtr.Zero, new IntPtr((long)SpotifyAction.PlayPause));
                System.Threading.Thread.Sleep(150);
            }
        }

        /* From: http://stackoverflow.com/questions/8459162/user32-api-custom-postmessage-for-automatisation */
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_APPCOMMAND = 0x0319;

        private enum SpotifyAction : long
        {
            PlayPause = 917504,
            Mute = 524288,
            VolumeDown = 589824,
            VolumeUp = 655360,
            Stop = 851968,
            PreviousTrack = 786432,
            NextTrack = 720896
        }
    }
}
