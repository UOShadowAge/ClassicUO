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
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class AccountSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x0021;
        private const ushort NORMAL_COLOR = 0x034F;
        private const ushort BANNED_COLOR = 0x0384;
        private uint _selectedAccount;

        public AccountSelectionGump() : base(0, 0)
        {
            CanCloseWithRightClick = false;

            int posInList = 0;
            int yOffset = 150;
            int yBonus = 0;
            int listTitleY = 106;

            LoginScene loginScene = Client.Game.GetScene<LoginScene>();
            
            string lastAccName = LastAccountManager.GetLastAccount(LoginScene.Username, World.ServerName);
            string lastSelected = loginScene.Accounts.FirstOrDefault(o => o == lastAccName);

            if (loginScene.Accounts.Length > 5)
            {
                listTitleY = 96;
                yOffset = 125;
                yBonus = 45;
            }

            if (!string.IsNullOrEmpty(lastSelected))
            {
                _selectedAccount = (uint) Array.IndexOf(loginScene.Accounts, lastSelected);
            }
            else if (loginScene.Accounts.Length > 0)
            {
                _selectedAccount = 0;
            }

            Add
            (
                new ResizePic(0x0A28)
                {
                    X = 160, Y = 70, Width = 408, Height = 343 + yBonus
                },
                1
            );
            
            bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || 
                string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

            bool unicode = isAsianLang;
            byte font = (byte)(isAsianLang ? 1 : 2);
            ushort hue = (ushort)(isAsianLang ? 0xFFFF : 0x0386);

            Add
            (
                new Label("Account Selection", unicode, hue, font: font)
                {
                    X = 267, Y = listTitleY
                },
                1
            );
            
            for (int i = 0, valid = 0; i < loginScene.Accounts.Length; i++)
            {
                string account = loginScene.Accounts[i];

                if (!string.IsNullOrEmpty(account))
                {
                    valid++;

                    bool isBanned = account.EndsWith(" *");

                    if (isBanned)
                    {
                        account = account.Substring(0, account.Length - 2);
                    }

                    ushort color = isBanned ? BANNED_COLOR : i == _selectedAccount ? SELECTED_COLOR : NORMAL_COLOR;

                    Add
                    (
                        new AccountEntryGump((uint) i, isBanned, account, SelectAccount, LoginAccount)
                        {
                            X = 224,
                            Y = yOffset + posInList * 40,
                            Hue = color
                        },
                        1
                    );

                    posInList++;
                }
            }

            Add
            (
                new Button((int) Buttons.Prev, 0x15A1, 0x15A3, 0x15A2)
                {
                    X = 586, Y = 445, ButtonAction = ButtonAction.Activate
                },
                1
            );

            Add
            (
                new Button((int) Buttons.Next, 0x15A4, 0x15A6, 0x15A5)
                {
                    X = 610, Y = 445, ButtonAction = ButtonAction.Activate
                },
                1
            );

            AcceptKeyboardInput = true;
            ChangePage(1);
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginAccount(_selectedAccount);
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            switch ((Buttons) buttonID)
            {
                case Buttons.Next:
                    LoginAccount(_selectedAccount);

                    break;

                case Buttons.Prev:
                    loginScene.StepBack();

                    break;
            }

            base.OnButtonClick(buttonID);
        }

        private void SelectAccount(uint index)
        {
            _selectedAccount = index;

            foreach (AccountEntryGump accountGump in FindControls<AccountEntryGump>())
            {
                accountGump.Hue = accountGump.AccountBanned ? BANNED_COLOR : accountGump.AccountIndex == index ? SELECTED_COLOR : NORMAL_COLOR;
            }
        }

        private void LoginAccount(uint index)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (loginScene.Accounts.Length > index && !string.IsNullOrEmpty(loginScene.Accounts[index]))
            {
                loginScene.SelectAccount(index);
            }
        }

        private enum Buttons
        {
            Next,
            Prev
        }

        private class AccountEntryGump : Control
        {
            private readonly Label _label;
            private readonly Action<uint> _loginFn;
            private readonly Action<uint> _selectedFn;

            public uint AccountIndex { get; }
            public bool AccountBanned { get; }

            public ushort Hue
            {
                get => _label.Hue;
                set => _label.Hue = value;
            }

            public AccountEntryGump(uint index, bool banned, string account, Action<uint> selectedFn, Action<uint> loginFn)
            {
                AccountIndex = index;
                AccountBanned = banned;

                _selectedFn = selectedFn;
                _loginFn = loginFn;

                ushort bgColor = 0;

                if (banned)
                {
                    bgColor = BANNED_COLOR;
                }

                // Bg
                Add
                (
                    new ResizePic(0x0BB8)
                    {
                        X = 0, Y = 0, Width = 280, Height = 30, Hue = bgColor,
                    }
                );

                // Account Name
                Add
                (
                    _label = new Label
                    (
                        account,
                        false,
                        NORMAL_COLOR,
                        270,
                        5,
                        align: TEXT_ALIGN_TYPE.TS_CENTER
                    )
                    {
                        X = 0
                    }
                );

                AcceptMouseInput = !AccountBanned;
            }

            protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                if (!AccountBanned && button == MouseButtonType.Left)
                {
                    _loginFn(AccountIndex);

                    return true;
                }

                return false;
            }


            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (!AccountBanned && button == MouseButtonType.Left)
                {
                    _selectedFn(AccountIndex);
                }
            }
        }
    }
}