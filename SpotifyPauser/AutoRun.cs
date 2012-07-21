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
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;

namespace SpotifyAutoPauser
{
    /* Class to run program on boot */
    internal class AutoRun
    {
        private static RegistryKey GetOptionRegistryKey()
        {
            return Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Spotify-Auto-Pauser");
        }

        private static RegistryKey GetRunRegistryKey()
        {
            return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        }

        internal static bool Run
        {
            get
            {
                RegistryKey option = GetOptionRegistryKey();
                if (option == null) return false;
                bool ret = bool.Parse(option.GetValue("AutoRunOnBoot", true).ToString());
                option.Dispose();
                return ret;
            }
            set
            {
                RegistryKey option = GetOptionRegistryKey();
                option.SetValue("AutoRunOnBoot", value.ToString());
                SetAutoRun(value);
            }
        }

        internal static void AutoRunCheck()
        {
            SetAutoRun(Run);
        }

        private static void SetAutoRun(bool Enabled)
        {
            RegistryKey run = GetRunRegistryKey();
            if (run == null) return;

            if (Run)
            {
                run.SetValue("SpotifyAutoPauser", "\"" + Application.ExecutablePath + "\"");
            }
            else
            {
                run.DeleteValue("SpotifyAutoPauser", false);
            }

            run.Dispose();
        }
    }
}
