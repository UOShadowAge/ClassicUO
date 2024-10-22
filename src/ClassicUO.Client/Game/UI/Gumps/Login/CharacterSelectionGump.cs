#region license

// Copyright (c) 2024, andreakarasho
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
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Resources;
using ClassicUO.Utility;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class CharacterSelectionGump : Gump
    {
        private const ushort SELECTED_COLOR = 0x0021;
        private const ushort NORMAL_COLOR = 0x47E;
        public uint _selectedCharacter;
        private StatusGumpModernControl _statusGump;

        private static readonly CharacterPosition[] _CHARACTER_POSITIONS = 
        {
            new CharacterPosition() { X = 112, Y = 400 },
            new CharacterPosition() { X = 397, Y = 280 },
            new CharacterPosition() { X = 675, Y = 280 },
            new CharacterPosition() { X = 987, Y = 352 },
            new CharacterPosition() { X = 350, Y = 542 },
            new CharacterPosition() { X = 825, Y = 508 }
        };
        
        internal class CharacterPosition
        {
            public int X { get; set; }
            public int Y { get; set; }
            
        }

        public CharacterSelectionGump(World world) : base(world, 0, 0)
        {
            CanCloseWithRightClick = false;

            int yOffset = 150;
            int yBonus = 0;

            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            string lastAccName = LastAccountManager.GetLastAccountSafe(LoginScene.Username, world.ServerName);
            string lastCharName = LastCharacterManager.GetLastCharacter(LoginScene.Username, world.ServerName, lastAccName);
            string lastSelected = loginScene.Characters.FirstOrDefault(o => o.Name == lastCharName)?.Name;

            LockedFeatureFlags f = world.ClientLockedFeatures.Flags;
            CharacterListFlags ff = world.ClientFeatures.Flags;

            if (Client.Game.UO.Version >= ClientVersion.CV_6040 || Client.Game.UO.Version >= ClientVersion.CV_5020 && loginScene.Characters.Length > 5)
            {
                yOffset = 116;
                yBonus = 45;
            }

            if (!string.IsNullOrEmpty(lastSelected))
            {
                _selectedCharacter = (uint) Array.IndexOf(loginScene.Characters, loginScene.Characters.FirstOrDefault(c => c.Name == lastSelected));
            }
            else if (loginScene.Characters.Length > 0)
            {
                _selectedCharacter = 0;
            }

            int v = 0;
            int i = 0;
            foreach (var ctr in loginScene.Characters.OrderByDescending(c => !string.IsNullOrEmpty(c.Name)))
            {
                if (!string.IsNullOrEmpty(ctr.Name))
                {
                    v++;

                    if (v > world.ClientFeatures.MaxChars)
                    {
                        break;
                    }

                    if (world.ClientLockedFeatures.Flags != 0 && !world.ClientLockedFeatures.Flags.HasFlag(LockedFeatureFlags.SeventhCharacterSlot))
                    {
                        if (v == 6 && !world.ClientLockedFeatures.Flags.HasFlag(LockedFeatureFlags.SixthCharacterSlot))
                        {
                            break;
                        }
                    }

                    var realIdx = Array.IndexOf(loginScene.Characters, ctr);

                    Add
                    (
                        new CharacterEntryGump((uint)realIdx, ctr.Name, SelectCharacter, LoginCharacter)
                        {
                            X = _CHARACTER_POSITIONS[i].X - (i >= 4 ? 30 : 20),
                            Y = _CHARACTER_POSITIONS[i].Y - 25,
                            Hue = realIdx == _selectedCharacter ? SELECTED_COLOR : NORMAL_COLOR,
                            Width = i >= 4 ? 200 : 250
                        }, 1
                    );

                    Add(
                        new GumpPic((i == 0 || i == 1 || i == 4) ? _CHARACTER_POSITIONS[i].X : _CHARACTER_POSITIONS[i].X + 68 , _CHARACTER_POSITIONS[i].Y + 125, (i == 0 || i == 1 || i == 4) ?  (ushort)166 : (ushort)167, 0)
                        {
                                Alpha = 0.4f,
                                Width = 170,
                                Height = 200
                        }, 1
                    );
 
                    if (ctr.Player != null)
                    {
                        Add
                        (
                            new PaperdollControl(world, _CHARACTER_POSITIONS[i].X, _CHARACTER_POSITIONS[i].Y, ctr.Player, ctr, null, i >= 4 ? 300 : 350, i >= 4 ? 300 : 350)
                            {
                                AcceptMouseInput = false,
                            }, 1
                        );
                    }
                }

                i++;
            }

            if (CanCreateChar(loginScene))
            {
                Add
                (
                    new Button((int) Buttons.New, 165, 165, 165)
                    {
                        X = 559, Y = 793, ButtonAction = ButtonAction.Activate
                    },
                    1
                );
            }
            
            
            Add
            (
                new Button((int) Buttons.Delete, 186, 187, 188)
                {
                    X = 23,
                    Y = 41,
                    ButtonAction = ButtonAction.Activate
                },
                1
            );
            
            Add
            (
                new Button((int) Buttons.Prev, 180, 181, 182)
                {
                    X = 32, Y = 895, ButtonAction = ButtonAction.Activate
                },
                1
            );
            
            Add
            (
                new Button((int) Buttons.Next, 183, 184, 185)
                {
                    X = 1165, Y = 894, ButtonAction = ButtonAction.Activate
                },
                1
            );
            
            AcceptKeyboardInput = true;
            ChangePage(1);
        }

        private bool CanCreateChar(LoginScene scene)
        {
            if (scene.Characters != null)
            {
                int empty = scene.Characters.Count(c => string.IsNullOrEmpty(c.Name));

                if (empty >= 0 && scene.Characters.Length - empty < World.ClientFeatures.MaxChars)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
        {
            if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
            {
                LoginCharacter(_selectedCharacter);
            }
        }

        public override void OnButtonClick(int buttonID)
        {
                LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            switch ((Buttons) buttonID)
            {
                case Buttons.Delete:
                    DeleteCharacter(loginScene);

                    break;

                case Buttons.New when CanCreateChar(loginScene):
                    loginScene.StartCharCreation();

                    break;

                case Buttons.Next:
                    LoginCharacter(_selectedCharacter);

                    break;

                case Buttons.Prev:
                    loginScene.StepBack();

                    break;
            }

            base.OnButtonClick(buttonID);
        }

        private void DeleteCharacter(LoginScene loginScene)
        {
            string charName = loginScene.Characters[_selectedCharacter].Name;

            if (!string.IsNullOrEmpty(charName))
            {
                LoadingGump existing = Children.OfType<LoadingGump>().FirstOrDefault();

                if (existing != null)
                {
                    Remove(existing);
                }

                Add
                (
                    new LoadingGump
                    (
                        World,
                        string.Format(ResGumps.PermanentlyDelete0, charName),
                        LoginButtons.OK | LoginButtons.Cancel,
                        buttonID =>
                        {
                            if (buttonID == (int) LoginButtons.OK)
                            {
                                loginScene.DeleteCharacter(_selectedCharacter);
                            }
                            else
                            {
                                ChangePage(1);
                            }
                        }
                    ),
                    2
                );

                ChangePage(2);
            }
        }

        private void SelectCharacter(uint index)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();
            _selectedCharacter = index;

            foreach (CharacterEntryGump characterGump in FindControls<CharacterEntryGump>())
            {
                if (!string.IsNullOrEmpty(loginScene.Characters[index].Name))
                {
                    characterGump.Hue = characterGump.CharacterIndex == index ? SELECTED_COLOR : NORMAL_COLOR;
                }
            }
        }

        private void LoginCharacter(uint index)
        {
            LoginScene loginScene = Client.Game.GetScene<LoginScene>();

            if (loginScene.Characters.Length > index && !string.IsNullOrEmpty(loginScene.Characters[index].Name))
            {
                loginScene.SelectCharacter(index);
            }
        }

        private enum Buttons
        {
            New,
            Delete,
            Next,
            Prev
        }

        private class CharacterEntryGump : Control
        {
            private readonly Label _label;
            private readonly Action<uint> _loginFn;
            private readonly Action<uint> _selectedFn;

            public CharacterEntryGump(uint index, string character, Action<uint> selectedFn, Action<uint> loginFn)
            {
                CharacterIndex = index;
                _selectedFn = selectedFn;
                _loginFn = loginFn;

                // Bg
                Add
                (
                    new GumpPic(0, 7, 162, 0)
                    {
                        Width = 263,
                        Height = 40,
                        Alpha = 0.75f
                    }
                );

                // Char Name
                Add
                (
                    _label = new Label(character, false, NORMAL_COLOR, 270, 0, align: TEXT_ALIGN_TYPE.TS_CENTER)
                    {
                        X = 0,
                        Y = 16
                    }
                );

                AcceptMouseInput = true;
            }

            public CharacterEntryGump(uint index, Action<uint> selectedFn)
            {
                _selectedFn = selectedFn;

                // Bg
                Add
                (
                    new GumpPic(0, 7, 162, 0)
                    {
                        Width = 263, Height = 40, Alpha = 0.75f
                    }
                );

                // Only here so an error isn't thrown.  Won't show up though.
                _label = new Label("", false, NORMAL_COLOR, 270, 3, align: TEXT_ALIGN_TYPE.TS_LEFT)
                {
                    X = 10,
                    Y = 10
                };

                    // Bg
                    Add
                    (
                        new ResizePic(164)
                        {
                            X = 15,
                            Y = 10,
                            Width = 239,
                            Height = 36,
                            Alpha = 0.75f
                        }
                    );
                

                AcceptMouseInput = true;
            }

            public uint CharacterIndex { get; }

            public ushort Hue
            {
                get => _label.Hue;
                set => _label.Hue = value;
            }

            protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    _loginFn(CharacterIndex);

                    return true;
                }

                return false;
            }


            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    _selectedFn(CharacterIndex);
                }
            }
        }
    }
}