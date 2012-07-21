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
using System.Xml;
using System.Xml.XPath;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace SpotifyAutoPauser
{
    /* Rudimentary auto-updater */
    internal class Updater
    {
        private static Tuple<Version, string> GetVersion()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://code.google.com/feeds/p/spotify-auto-pauser/downloads/basic");
            WebResponse resp = req.GetResponse();

            XPathNavigator xml = new XPathDocument(resp.GetResponseStream()).CreateNavigator();
            XmlNamespaceManager mgr = new XmlNamespaceManager(xml.NameTable);
            mgr.AddNamespace("x", "http://www.w3.org/2005/Atom");
            string url = xml.SelectSingleNode("/x:feed/x:entry/x:link", mgr).GetAttribute("href", "");
            
            Regex reg = new Regex("v([0-9]{1,2}).([0-9]{1,2}).([0-9]{1,2})");
            if (reg.IsMatch(url))
            {
                Match m = reg.Match(url);
                Version v = new Version(m.Groups[0].ToString().Substring(1) + ".0");
                return new Tuple<Version, String>(v, url);
            }
            
            return null;
        }

        internal static bool Check()
        {
            Tuple<Version, string> update = GetVersion();
            if (update.Item1 != null && update.Item1.CompareTo(Assembly.GetExecutingAssembly().GetName().Version) > 0)
            {
                if (MessageBox.Show("There is a new version of Spotify Auto Pauser available, would you like to download it now?", "Spotify Auto Pauser Update", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Process.Start(update.Item2);
                    /*HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://code.google.com/feeds/p/spotify-auto-pauser/downloads/basic");
                    WebResponse resp = req.GetResponse();
                    string fname = Path.ChangeExtension(Path.GetTempFileName(), ".msi");
                    using (StreamWriter sw = new StreamWriter(fname))
                    {
                        sw.Write(resp.GetResponseStream());
                    }

                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = fname;
                        p.StartInfo.Arguments = "/qb";
                        p.Start();
                    }
                    Application.Exit();*/
                }
            }

            return true;
        }
    }
}
