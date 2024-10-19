#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Scenes;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers
{
    [JsonSerializable(typeof(LastAccountInfo))]
    [JsonSerializable(typeof(List<LastAccountInfo>))]
    sealed partial class LastAccountJsonContext : JsonSerializerContext { }

    public static class LastAccountManager
    {
        private static readonly string _lastAccountFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles");
        private static readonly string _lastAccountFile = Path.Combine(_lastAccountFilePath, "lastaccount.json");

        private static List<LastAccountInfo> LastAccounts { get; set; }

        private static string LastAccountNameOverride { get; set; }

        public static void Load()
        {
            LastAccounts = new List<LastAccountInfo>();

            if (!File.Exists(_lastAccountFile))
            {
                ConfigurationResolver.Save(LastAccounts, _lastAccountFile, LastAccountJsonContext.Default);
            }

            LastAccounts = ConfigurationResolver.Load<List<LastAccountInfo>>(_lastAccountFile, LastAccountJsonContext.Default);

            // safety check
            if (LastAccounts == null)
            {
                LastAccounts = new List<LastAccountInfo>();
            }
        }

        public static void Save(string login, string server, string name)
        {
            LastAccountInfo lastAcc = LastAccounts.FirstOrDefault(c => c.LoginName == login && c.ServerName == server);

            // Check to see if they passed in -lastaccountname but picked another account, clear override then
            if (!string.IsNullOrEmpty(LastAccountNameOverride) && !LastAccountNameOverride.Equals(name))
            {
                LastAccountNameOverride = string.Empty;
            }

            if (lastAcc != null)
            {
                lastAcc.LastAccountName = name;
            }
            else
            {
                LastAccounts.Add(new LastAccountInfo
                {
                    LoginName = login,
                    ServerName = server,
                    LastAccountName = name,
                });
            }

            ConfigurationResolver.Save(LastAccounts, _lastAccountFile, LastAccountJsonContext.Default);
        }

        public static string GetLastAccountSafe(string login, string server)
        {
            string lastAccName = GetLastAccount(login, server);

            // we got here by skipping the account selection (normal login flow)
            if (string.IsNullOrWhiteSpace(lastAccName))
            {
                lastAccName = LoginScene.Username;
            }

            return lastAccName;
        }

        public static string GetLastAccount(string login, string server)
        {
            if (LastAccounts == null)
            {
                Load();
            }

            // If they passed in a -lastaccountname param, ignore json value, use that value instead
            if (!string.IsNullOrEmpty(LastAccountNameOverride))
            {
                return LastAccountNameOverride;
            }

            LastAccountInfo lastAcc = LastAccounts.FirstOrDefault(c => c.LoginName == login && c.ServerName == server);

            return lastAcc != null ? lastAcc.LastAccountName : string.Empty;
        }
        
        public static void OverrideLastAccount(string name)
        {
            LastAccountNameOverride = name;
        }
    }

    public class LastAccountInfo
    {
        public string LoginName { get; set; }
        public string ServerName { get; set; }
        public string LastAccountName { get; set; }
    }
}