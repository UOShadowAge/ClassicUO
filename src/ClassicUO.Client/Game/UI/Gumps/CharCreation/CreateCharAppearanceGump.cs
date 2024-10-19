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
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Game.Scenes;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps.CharCreation
{
    internal class CreateCharAppearanceGump : Gump
    {
        struct CharacterInfo
        {
            public bool IsFemale;
            public RaceType Race;
            public bool IsHardcore;
            public bool IsSoloSelfFound;
        }

        private PlayerMobile _character;
        private CharacterInfo _characterInfo;
        private readonly Button _humanRadio, _elfRadio, _gargoyleRadio;
        private readonly Button _hairSelect, _beardSelect, _shirtSelect, _pantsSelect;
        private readonly Button _maleRadio, _femaleRadio;
        private readonly Checkbox _hardcoreCheckbox, _soloSelfFoundCheckbox;
        private ProgressSlider _hairSlider, _beardSlider, _skinSlider;
        private CustomColorTablePicker _colorTablePicker;
        private readonly StbTextBox _nameTextBox;
        private PaperDollInteractable _paperDoll;
        private ProfessionInfoGump[] _professionInfoGumps = new ProfessionInfoGump[10]; // Stores all profession info   
        private readonly Button _CreateButton;
        private GumpPic _selectedProfessionGump;
        private CreateCharTradeGump _createCharTradeGump;
        private readonly Dictionary<Layer, Tuple<int, ushort>> CurrentColorOption = new Dictionary<Layer, Tuple<int, ushort>>();
        public Dictionary<Layer, int> CurrentOption = new Dictionary<Layer, int>
        {
            { Layer.Hair, 1 },
            { Layer.Beard, 0 },
            { Layer.Invalid, 0 }
        };

        public CreateCharAppearanceGump() : base(0, 0)
        {
            // Background
            Add
            (
                new GumpPic(0, 0, 159, 0)
                {
                    Height = 960,
                    Width = 1280,
                    AcceptKeyboardInput = false
                }
            );

            // Male / Female Button Background

            // // Male
            // Add
            // (
            //     new GumpPic(480, 325, 170, 0)
            //     {
            //         AcceptKeyboardInput = false
            //     }
            // );
            //
            // // Female
            // Add
            // (
            //     new GumpPic(600, 325, 172, 0)
            //     {
            //         AcceptKeyboardInput = false
            //     }
            // );
            //
            // Add
            // (
            //     new GumpPic(800, 325, 173, 0)
            //     {
            //         AcceptKeyboardInput = false
            //     }
            // );

            // Add
            // (
            //     new ResizePic(0x0E10)
            //     {
            //         X = 82, Y = 125, Width = 151, Height = 310
            //     },
            //     1
            // );

            // Add(new GumpPic(280, 53, 0x0709, 0), 1);
            // Add(new GumpPic(240, 73, 0x070A, 0), 1);
            //
            // Add
            // (
            //     new GumpPicTiled
            //     (
            //         248,
            //         73,
            //         215,
            //         16,
            //         0x070B
            //     ),
            //     1
            // );

            // Add(new GumpPic(463, 73, 0x070C, 0), 1);
            // Add(new GumpPic(238, 98, 0x0708, 0), 1);
            //
            // Add
            // (
            //     new ResizePic(0x0E10)
            //     {
            //         X = 475, Y = 125, Width = 151, Height = 310
            //     },
            //     1
            // );

            // // Male/Female Radios
            // Add
            // (
            //     _maleRadio = new Button((int)Buttons.MaleButton, 0x0768, 0x0767)
            //     {
            //          X = 160, Y = 455, ButtonAction = ButtonAction.Activate
            //     },
            //     1
            // );

            Add
            (
                _femaleRadio = new Button((int)Buttons.FemaleButton, 172, 173)
                {
                    X = 781,
                    Y = 300,
                    ButtonAction = ButtonAction.Activate,
                    Alpha = 0
                }, 1
            );

            Add
            (
                _maleRadio = new Button((int)Buttons.MaleButton, 172, 173)
                {
                    IsClicked = true,
                    X = 460,
                    Y = 300,
                    ButtonAction = ButtonAction.Activate,
                    Alpha = 0.45f
                }, 1
            );

            Add
            (
                _hairSelect = new Button((int)Buttons.HairButton, 175, 175)
                {
                    X = 896,
                    Y = 475,
                    ButtonAction = ButtonAction.Activate,
                    Alpha = .55f,
                    Width = 64,
                    Height = 54
                }, 1
            );

            Add
            (
                _beardSelect = new Button((int)Buttons.BeardButton, 175, 175)
                {
                    X = 963,
                    Y = 475,
                    ButtonAction = ButtonAction.Activate,
                    Alpha = 0,
                    Width = 64,
                    Height = 54
                }, 1
            );

            Add
            (
                _shirtSelect = new Button((int)Buttons.ShirtButton, 175, 175)
                {
                    X = 1029,
                    Y = 475,
                    ButtonAction = ButtonAction.Activate,
                    Alpha = 0,
                    Width = 64,
                    Height = 54
                }, 1
            );
            
            Add
            (
                _pantsSelect = new Button((int)Buttons.PantsButton, 175, 175)
                {
                    X = 1097,
                    Y = 475,
                    ButtonAction = ButtonAction.Activate,
                    Alpha = 0,
                    Width = 64,
                    Height = 54
                }, 1
            );


            Add
            (
                _hardcoreCheckbox = new Checkbox(150, 151, "")
                {
                    X = 224,
                    Y = 894
                }, 1
            );
            
            Add
            (
                _soloSelfFoundCheckbox = new Checkbox(148, 149, "")
                {
                    X = 1033,
                    Y = 893
                }, 1
            );

            Add
            (
                _CreateButton = new Button((int)Buttons.Next, 171, 171, 171)
                {
                    X = 559,
                    Y = 793,
                    ButtonAction = ButtonAction.Activate
                }, 1
            );

            _CreateButton.Alpha = _CreateButton.IsEnabled ? 1 : 0.25f;


            Add
            (
                _nameTextBox = new StbTextBox(0, 16, 200, false, hue: 33, style: FontStyle.Fixed, align: TEXT_ALIGN_TYPE.TS_CENTER)
                {
                    X = 546,
                    Y = 234,
                    Width = 200,
                    Height = 22
                    //ValidationRules = (uint) (TEXT_ENTRY_RULES.LETTER | TEXT_ENTRY_RULES.SPACE)
                }, 1
            );

            // // Races
            // Add
            // (
            //     _humanRadio = new Button((int)Buttons.HumanButton, 0x0768, 0x0767)
            //     {
            //         X = 180, Y = 435, ButtonAction = ButtonAction.Activate
            //     },
            //     1
            // );

            //Add
            //(
            //    new Button((int) Buttons.HumanButton, 0x0702, 0x0704, 0x0703)
            //    {
            //        X = 200, Y = 435, ButtonAction = ButtonAction.Activate
            //    },
            //    1
            //);

            //Add
            //(
            //    _elfRadio = new Button((int)Buttons.ElfButton, 0x0768, 0x0767, 0x0768)
            //    {
            //        X = 180, Y = 455, ButtonAction = ButtonAction.Activate
            //    },
            //    1
            //);

            //Add
            //(
            //    new Button((int) Buttons.ElfButton, 0x0705, 0x0707, 0x0706)
            //    {
            //        X = 200, Y = 455, ButtonAction = ButtonAction.Activate
            //    },
            //    1
            //);

            //if (Client.Version >= ClientVersion.CV_60144)
            //{
            //    Add
            //    (
            //        _gargoyleRadio = new Button((int)Buttons.GargoyleButton, 0x0768, 0x0767)
            //        {
            //            X = 60, Y = 435, ButtonAction = ButtonAction.Activate
            //        },
            //        1
            //    );

            //    Add
            //    (
            //        new Button((int) Buttons.GargoyleButton, 0x07D3, 0x07D5, 0x07D4)
            //        {
            //            X = 80, Y = 435, ButtonAction = ButtonAction.Activate
            //        },
            //        1
            //    );
            //}

            Add
            (
                new Button((int)Buttons.Prev, 180, 181, 182)
                {
                    X = 32,
                    Y = 895,
                    ButtonAction = ButtonAction.Activate
                }, 1
            );

            // _maleRadio.IsClicked = true;
            //_humanRadio.IsClicked = true;
            _characterInfo.IsFemale = false;
            _characterInfo.Race = RaceType.HUMAN;

            _characterInfo.IsHardcore = _hardcoreCheckbox.IsChecked;
            _characterInfo.IsSoloSelfFound = _soloSelfFoundCheckbox.IsChecked;

            CreateCharacter(_characterInfo.IsFemale, _characterInfo.Race, _characterInfo.IsHardcore, _characterInfo.IsSoloSelfFound);
            HandleRaceChanged();
            HandleProfessions();
        }

        private void HandleProfessions()
        {
            var professions = new List<ProfessionInfo>(ProfessionLoader.Instance.Professions.Keys);
            
            for (int i = 0; i < professions.Count; i++)
            {
                if (_professionInfoGumps[i] == null)
                {
                    Add
                    (
                        _professionInfoGumps[i] = new ProfessionInfoGump(professions[i])
                        {
                            X = 144,
                            Y = 136 + 69 * i,

                            Selected = SelectProfession
                        }
                    );
                }
                else
                {
                    if (_character != null && _character.Profession == professions[i])
                    {

                        if (_selectedProfessionGump != null)
                        {
                            _selectedProfessionGump.X = 135;
                            _selectedProfessionGump.Y = 133 + 69 * i;
                        }
                        else
                        {
                            Add(_selectedProfessionGump = new GumpPic(135, 133 + 69 * i, 179, 0));
                        }
                    }
                }
            }
        }
        
        public void SelectProfession(ProfessionInfo info)
        {
            if (info.Type == ProfessionLoader.PROF_TYPE.CATEGORY && ProfessionLoader.Instance.Professions.TryGetValue(info, out List<ProfessionInfo> list) && list != null)
            {
                Parent.Add(new CreateCharProfessionGump(info));
                Parent.Remove(this);
            }
            else
            {
                CharCreationGump charCreationGump = UIManager.GetGump<CharCreationGump>();
                charCreationGump.SetCharacter(_character);

                charCreationGump?.SetProfession(info);
                _character.Profession = info;
                DisplayProfessionInfo(info);
            }

            HandleProfessions();
        }

        public void DisplayProfessionInfo(ProfessionInfo info)
        {
            if (_createCharTradeGump == null)
            {
                Add( _createCharTradeGump = new CreateCharTradeGump(_character, info));
            }
            else
            {
                _createCharTradeGump.Clear();
                Add( _createCharTradeGump = new CreateCharTradeGump(_character, info));
            }
        }

        private void CreateCharacter(bool isFemale, RaceType race, bool isHardcore = false, bool isSoloSelfFound = false)
        {
            if (_character == null)
            {
                _character = new PlayerMobile(1);
                World.Mobiles.Add(_character);
            }

            LinkedObject first = _character.Items;

            while (first != null)
            {
                LinkedObject next = first.Next;

                World.RemoveItem((Item)first, true);

                first = next;
            }

            _character.Clear();
            _character.Race = race;
            _character.IsFemale = isFemale;
            _character.IsHardcore = isHardcore;
            _character.IsSoloSelfFound = isSoloSelfFound;
            
            if (_paperDoll != null)
            {
                Remove(_paperDoll);
            }
            
            Add
            (
                _paperDoll = new PaperDollInteractable(518, 235, _character, null, 375, 350)
                {
                    AcceptMouseInput = false,
                }, 1
            );

            _paperDoll.RequestUpdate();

            if (isFemale)
            {
                _character.Flags |= Flags.Female;
            }
            else
            {
                _character.Flags &= ~Flags.Female;
            }
            
            // Skin
            ushort[] pallet = CharacterCreationValues.GetSkinPallet(race);
            var defaultSkinIndex = RandomHelper.GetValue(0, pallet.Length - 1);
            CurrentOption[Layer.Invalid] = defaultSkinIndex;
            CurrentColorOption[Layer.Invalid] = new Tuple<int, ushort>(defaultSkinIndex, pallet[defaultSkinIndex]);
            _character.Hue = pallet[defaultSkinIndex];

            Add(_skinSlider = new ProgressSlider(this, 448, 77, pallet.Length, Layer.Invalid, race, _character, CurrentOption[Layer.Invalid]));
            _skinSlider.OnOptionSelected += Skin_OnOptionSelected;


            // Hair
            CharacterCreationValues.ComboContent content = CharacterCreationValues.GetHairComboContent(_characterInfo.IsFemale, race);
            var randomHair = RandomHelper.GetValue(0, content.Labels.Length - 1);
            pallet = CharacterCreationValues.GetCustomHairPallet();
            var randomHairHue = RandomHelper.GetValue(0, pallet.Length - 1);

            
            CurrentColorOption[Layer.Hair] = new Tuple<int, ushort>(randomHairHue, pallet[randomHairHue]);
            CurrentOption[Layer.Hair] = randomHair;
            
            Add(_hairSlider = new ProgressSlider(this, 448, 109, content.Labels.Length, Layer.Hair, race, _character, CurrentOption[Layer.Hair]));
            _hairSlider.OnOptionSelected += Hair_OnOptionSelected;
            AddCustomColorPickerTable(918, 575, pallet, Layer.Hair, 11, 8);

            // Beard
            if (!_characterInfo.IsFemale && race != RaceType.ELF)
            {
                content = CharacterCreationValues.GetFacialHairComboContent(race);
                var randomBeard = RandomHelper.GetValue(0, content.Labels.Length - 1);
                var randomBeardHue = RandomHelper.GetValue(0, pallet.Length - 1);

                CurrentColorOption[Layer.Beard] = new Tuple<int, ushort>(randomBeardHue, pallet[randomBeardHue]);
                CurrentOption[Layer.Beard] = randomBeard;
                
                Add(_beardSlider = new ProgressSlider(this, 448, 142, content.Labels.Length, Layer.Beard, race, _character, CurrentOption[Layer.Beard]));
                _beardSlider.OnOptionSelected += Facial_OnOptionSelected;
            }
            
            // Shirt
            pallet = CharacterCreationValues.GetCustomClothesPallet();
            var randomShirt = RandomHelper.GetValue(0, pallet.Length - 1);
            CurrentOption[Layer.Shirt] = randomShirt;
            CurrentColorOption[Layer.Shirt] = new Tuple<int, ushort>(randomShirt, pallet[randomShirt]);
            
            // Pants
            pallet = CharacterCreationValues.GetCustomClothesPallet();
            var randomPants = RandomHelper.GetValue(0, pallet.Length - 1);
            CurrentOption[Layer.Pants] = randomPants;
            CurrentColorOption[Layer.Pants] = new Tuple<int, ushort>(randomPants, pallet[randomPants]);


            switch (race)
            {
                case RaceType.GARGOYLE:
                    _character.Graphic = isFemale ? (ushort)0x029B : (ushort)0x029A;

                    Item it = CreateItem(0x4001, CurrentColorOption[Layer.Shirt].Item2, Layer.Robe);

                    _character.PushToBack(it);

                    break;

                case RaceType.ELF when isFemale:
                    _character.Graphic = 0x025E;
                    it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                    _character.PushToBack(it);

                    it = CreateItem(0x1531, CurrentColorOption[Layer.Pants].Item2, Layer.Skirt);

                    _character.PushToBack(it);

                    it = CreateItem(0x1518, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                    _character.PushToBack(it);

                    break;

                case RaceType.ELF:
                    _character.Graphic = 0x025D;
                    it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                    _character.PushToBack(it);

                    it = CreateItem(0x152F, CurrentColorOption[Layer.Pants].Item2, Layer.Pants);

                    _character.PushToBack(it);

                    it = CreateItem(0x1518, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                    _character.PushToBack(it);

                    break;

                default:

                {
                    if (isFemale)
                    {
                        _character.Graphic = 0x0191;
                        it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                        _character.PushToBack(it);

                        it = CreateItem(0x1531, CurrentColorOption[Layer.Pants].Item2, Layer.Skirt);

                        _character.PushToBack(it);

                        it = CreateItem(0x1EFD, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                        _character.PushToBack(it);
                    }
                    else
                    {
                        _character.Graphic = 0x0190;
                        it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                        _character.PushToBack(it);

                        it = CreateItem(0x1539, CurrentColorOption[Layer.Pants].Item2, Layer.Pants);

                        _character.PushToBack(it);

                        it = CreateItem(0x1EFD, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                        _character.PushToBack(it);
                    }

                    break;
                }
            }
            
            Layer layer;
            
            if (!_characterInfo.IsFemale && race != RaceType.ELF)
            {
                layer = Layer.Beard;
                content = CharacterCreationValues.GetFacialHairComboContent(race);

                Item beard = CreateItem(content.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);

                _character.PushToBack(beard);
            }

            layer = Layer.Hair;
            content = CharacterCreationValues.GetHairComboContent(_characterInfo.IsFemale, race);

            Item hair = CreateItem(content.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);

            _character.PushToBack(hair);
        }
        
        private void UpdateCharacter(bool isFemale, RaceType race, bool isHardcore = false, bool isSoloSelfFound = false)
        {
            if (_character == null)
            {
                return;
            }

            _character.Race = race;
            _character.IsFemale = isFemale;
            _character.IsHardcore = isHardcore;
            _character.IsSoloSelfFound = isSoloSelfFound;

            if (isFemale)
            {
                _character.Flags |= Flags.Female;
            }
            else
            {
                _character.Flags &= ~Flags.Female;
            }
            
            LinkedObject first = _character.Items;

            while (first != null)
            {
                LinkedObject next = first.Next;

                World.RemoveItem((Item)first, true);

                first = next;
            }


            switch (race)
            {
                case RaceType.GARGOYLE:
                    _character.Graphic = isFemale ? (ushort)0x029B : (ushort)0x029A;

                    Item it = CreateItem(0x4001, CurrentColorOption[Layer.Shirt].Item2, Layer.Robe);

                    _character.PushToBack(it);

                    break;

                case RaceType.ELF when isFemale:
                    _character.Graphic = 0x025E;
                    it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                    _character.PushToBack(it);

                    it = CreateItem(0x1531, CurrentColorOption[Layer.Pants].Item2, Layer.Skirt);

                    _character.PushToBack(it);

                    it = CreateItem(0x1518, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                    _character.PushToBack(it);

                    break;

                case RaceType.ELF:
                    _character.Graphic = 0x025D;
                    it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                    _character.PushToBack(it);

                    it = CreateItem(0x152F, CurrentColorOption[Layer.Pants].Item2, Layer.Pants);

                    _character.PushToBack(it);

                    it = CreateItem(0x1518, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                    _character.PushToBack(it);

                    break;

                default:

                {
                    if (isFemale)
                    {
                        _character.Graphic = 0x0191;
                        it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                        _character.PushToBack(it);

                        it = CreateItem(0x1531, CurrentColorOption[Layer.Pants].Item2, Layer.Skirt);

                        _character.PushToBack(it);

                        it = CreateItem(0x1EFD, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                        _character.PushToBack(it);
                    }
                    else
                    {
                        _character.Graphic = 0x0190;
                        it = CreateItem(0x1710, 0x0384, Layer.Shoes);
                        _character.PushToBack(it);

                        it = CreateItem(0x1539, CurrentColorOption[Layer.Pants].Item2, Layer.Pants);

                        _character.PushToBack(it);

                        it = CreateItem(0x1EFD, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);

                        _character.PushToBack(it);
                    }

                    break;
                }
            }
            
            Layer layer;
            CharacterCreationValues.ComboContent content;
            
            if (!_characterInfo.IsFemale && race != RaceType.ELF)
            {
                layer = Layer.Beard;
                content = CharacterCreationValues.GetFacialHairComboContent(race);

                Item beard = CreateItem(content.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);

                _character.PushToBack(beard);
            }

            layer = Layer.Hair;
            content = CharacterCreationValues.GetHairComboContent(_characterInfo.IsFemale, race);

            Item hair = CreateItem(content.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);

            _character.PushToBack(hair);
        }

        private void HandleRaceChanged()
        {
            RaceType race = _characterInfo.Race;
            CharacterListFlags flags = World.ClientFeatures.Flags;
            LockedFeatureFlags locks = World.ClientLockedFeatures.Flags;

            bool allowElf = (flags & CharacterListFlags.CLF_ELVEN_RACE) != 0 && locks.HasFlag(LockedFeatureFlags.ML);
            bool allowGarg = locks.HasFlag(LockedFeatureFlags.SA);

            if (race == RaceType.ELF && !allowElf)
            {
                _CreateButton.IsEnabled = false;
            }
            else if (race == RaceType.GARGOYLE && !allowGarg)
            {
                _CreateButton.IsEnabled = false;
            }
            else
            {
                _CreateButton.IsEnabled = true;
            }
        }
        
        private void AddCustomColorPickerTable(int x, int y, ushort[] pallet, Layer layer, int rows, int columns)
        {
            Add
            (
                _colorTablePicker = new CustomColorTablePicker(this, x, y, 28, 25, columns, rows, pallet, Layer.Hair)
            );

            if (!CurrentColorOption.ContainsKey(layer))
            {
                CurrentColorOption[layer] = new Tuple<int, ushort>(0, _colorTablePicker.SelectedHue);
            }

            _colorTablePicker.ColorSelected += ColorPicker_ColorSelected;
        }

        private void AddCustomColorPicker(int x, int y, ushort[] pallet, Layer layer, int clilocLabel, int rows, int columns)
        {
            CustomColorPicker colorPicker;

            Add
            (
                colorPicker = new CustomColorPicker(layer, clilocLabel, pallet, rows, columns)
                {
                    X = x,
                    Y = y,
                    IsVisible = false
                }, 1
            );

            if (!CurrentColorOption.ContainsKey(layer))
            {
                CurrentColorOption[layer] = new Tuple<int, ushort>(0, colorPicker.HueSelected);
            }
            else
            {
                colorPicker.SetSelectedIndex(CurrentColorOption[layer].Item1);
            }

            colorPicker.ColorSelected += ColorPicker_ColorSelected;
        }

        public void ColorPicker_ColorSelected(object sender, ColorSelectedEventArgs e)
        {
            if (e.SelectedIndex == 0xFFFF)
            {
                return;
            }

            CurrentColorOption[e.Layer] = new Tuple<int, ushort>(e.SelectedIndex, e.SelectedHue);

            if (e.Layer != Layer.Invalid)
            {
                Item item;

                if (_character.Race == RaceType.GARGOYLE && e.Layer == Layer.Shirt)
                {
                    item = _character.FindItemByLayer(Layer.Robe);
                }
                else
                {
                    item = _character.FindItemByLayer(_characterInfo.IsFemale && e.Layer == Layer.Pants ? Layer.Skirt : e.Layer);
                }

                if (item != null)
                {
                    item.Hue = e.SelectedHue;
                }
            }
            else
            {
                _character.Hue = e.SelectedHue;
            }

            _paperDoll.RequestUpdate();
        }

        private void Facial_OnOptionSelected(object sender, int e)
        {
            RaceType race = _characterInfo.Race;
            CharacterCreationValues.ComboContent content = CharacterCreationValues.GetFacialHairComboContent(race);
            CurrentOption[Layer.Beard] = e;
            UpdateHair(race, Layer.Beard, CurrentColorOption[Layer.Beard].Item2, content);
            _paperDoll.RequestUpdate();
        }

        private void Hair_OnOptionSelected(object sender, int e)
        {
            RaceType race = _characterInfo.Race;
            CharacterCreationValues.ComboContent content = CharacterCreationValues.GetHairComboContent(_characterInfo.IsFemale, race);
            CurrentOption[Layer.Hair] = e;
            UpdateHair(race, Layer.Hair, CurrentColorOption[Layer.Hair].Item2, content);
            _paperDoll.RequestUpdate();
        }

        private void Skin_OnOptionSelected(object sender, int e)
        {
            RaceType race = _characterInfo.Race;
            ushort[] pallet = CharacterCreationValues.GetSkinPallet(race);
            CurrentOption[Layer.Invalid] = e;
            _character.Hue = pallet[e];
            _paperDoll.RequestUpdate();
        }

        private void UpdateHair(RaceType race, Layer layer, ushort hue, CharacterCreationValues.ComboContent content)
        {
            if (layer == Layer.Hair)
            {
                Item hair = CreateItem(content.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);
                _character.PushToBack(hair);
            }
            else if (layer == Layer.Beard)
            {
                if (!_characterInfo.IsFemale && race != RaceType.ELF)
                {
                    Item beard = CreateItem(content.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);
                    _character.PushToBack(beard);
                }
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            CharCreationGump charCreationGump = UIManager.GetGump<CharCreationGump>();
            ushort[] pallet = null;
            
            switch ((Buttons)buttonID)
            {
                case Buttons.FemaleButton:
                    _femaleRadio.IsClicked = true;
                    _maleRadio.IsClicked = false;
                    _characterInfo.IsFemale = true;
                    _femaleRadio.Alpha = 0.45f;
                    _maleRadio.Alpha = 0;
                    // CurrentOption[Layer.Hair] = 2;
                    CurrentOption[Layer.Beard] = 0;
                    _beardSlider.SelectedIndex = CurrentOption[Layer.Beard];
                    _hairSlider.SelectedIndex = CurrentOption[Layer.Hair];
                    _beardSlider.IsVisible = false;
                    _beardSlider.ResetXPosition();
                    // _hairSlider.ResetXPosition();

                    UpdateCharacter(_femaleRadio.IsClicked, _characterInfo.Race, _characterInfo.IsHardcore, _characterInfo.IsSoloSelfFound);

                    break;

                case Buttons.MaleButton:
                    _maleRadio.IsClicked = true;
                    _femaleRadio.IsClicked = false;
                    _characterInfo.IsFemale = false;
                    _maleRadio.Alpha = 0.45f;
                    _femaleRadio.Alpha = 0;
                    // CurrentOption[Layer.Hair] = 2;
                    // CurrentOption[Layer.Beard] = 0;
                    _beardSlider.IsVisible = true;
                    _beardSlider.SelectedIndex = CurrentOption[Layer.Beard];
                    _hairSlider.SelectedIndex = CurrentOption[Layer.Hair];
                    // _beardSlider.ResetXPosition();
                    // _hairSlider.ResetXPosition();

                    UpdateCharacter(_femaleRadio.IsClicked, _characterInfo.Race, _characterInfo.IsHardcore, _characterInfo.IsSoloSelfFound);
                    
                    break;

                case Buttons.HumanButton:

                    _characterInfo.Race = RaceType.HUMAN;

                    if (!_humanRadio.IsClicked)
                    {
                        _humanRadio.IsClicked = true;

                        if (_elfRadio != null)
                        {
                            _elfRadio.IsClicked = false;
                        }

                        if (_gargoyleRadio != null)
                        {
                            _gargoyleRadio.IsClicked = false;
                        }

                        HandleRaceChanged();
                    }

                    break;

                case Buttons.ElfButton:

                    _characterInfo.Race = RaceType.ELF;

                    if (!_elfRadio.IsClicked)
                    {
                        _elfRadio.IsClicked = true;
                        _humanRadio.IsClicked = false;

                        if (_gargoyleRadio != null)
                        {
                            _gargoyleRadio.IsClicked = false;
                        }

                        HandleRaceChanged();
                    }

                    break;

                case Buttons.GargoyleButton:

                    _characterInfo.Race = RaceType.GARGOYLE;

                    if (!_gargoyleRadio.IsClicked)
                    {
                        _gargoyleRadio.IsClicked = true;
                        _elfRadio.IsClicked = false;
                        _humanRadio.IsClicked = false;

                        HandleRaceChanged();
                    }

                    break;

                case Buttons.Next:
                    _character.Name = _nameTextBox.Text;
                    _character.IsHardcore = _hardcoreCheckbox.IsChecked;
                    _character.IsSoloSelfFound = _soloSelfFoundCheckbox.IsChecked;

                    if (ValidateCharacter(_character))
                    {
                        charCreationGump.SetCharacter(_character);
                        
                        if (_createCharTradeGump != null && ValidateValues(_createCharTradeGump))
                        {
                            for (int i = 0; i < _createCharTradeGump._skillsCombobox.Length; i++)
                            {
                                if (_createCharTradeGump._skillsCombobox[i].SelectedIndex != -1)
                                {
                                    Skill skill = _character.Skills[_createCharTradeGump._skillList[_createCharTradeGump._skillsCombobox[i].SelectedIndex].Index];
                                    skill.ValueFixed = (ushort) _createCharTradeGump._skillSliders[i].Value;
                                    skill.BaseFixed = 0;
                                    skill.CapFixed = 0;
                                    skill.Lock = Lock.Locked;
                                }
                            }

                            _character.Strength = (ushort) _createCharTradeGump._attributeSliders[0].Value;
                            _character.Intelligence = (ushort) _createCharTradeGump._attributeSliders[1].Value;
                            _character.Dexterity = (ushort) _createCharTradeGump._attributeSliders[2].Value;

                            charCreationGump.SetAttributes(true);
                            // charCreationGump.SetStep(CharCreationGump.CharCreationStep.ChooseCity)
                        }
                    }

                    break;

                case Buttons.Prev:
                    charCreationGump.StepBack();

                    break;

                case Buttons.HairButton:
                    _hairSelect.Alpha = .55f;
                    _beardSelect.Alpha = 0;
                    _shirtSelect.Alpha = 0;
                    _pantsSelect.Alpha = 0;
                    pallet = CharacterCreationValues.GetCustomHairPallet();
                    _colorTablePicker.ChangePallete(pallet);
                    _colorTablePicker._layer = Layer.Hair;
                    // _colorTablePicker.ResetSelectedIndex();
                    _colorTablePicker.DeferredSelection();


                    break;

                case Buttons.BeardButton:
                    _beardSelect.Alpha = .55f;
                    _hairSelect.Alpha = 0;
                    _shirtSelect.Alpha = 0;
                    _pantsSelect.Alpha = 0;
                    pallet = CharacterCreationValues.GetCustomHairPallet();
                    _colorTablePicker.ChangePallete(pallet);
                    _colorTablePicker._layer = Layer.Beard;
                    // _colorTablePicker.ResetSelectedIndex();
                    _colorTablePicker.DeferredSelection();


                    break;

                case Buttons.ShirtButton:
                    _shirtSelect.Alpha = .55f;
                    _pantsSelect.Alpha = 0;
                    _beardSelect.Alpha = 0;
                    _hairSelect.Alpha = 0;
                    pallet = CharacterCreationValues.GetCustomClothesPallet();
                    _colorTablePicker.ChangePallete(pallet);
                    _colorTablePicker._layer = Layer.Shirt;
                    // _colorTablePicker.ResetSelectedIndex();
                    _colorTablePicker.DeferredSelection();


                    break;
                
                case Buttons.PantsButton:
                    _pantsSelect.Alpha = .55f;
                    _shirtSelect.Alpha = 0;
                    _beardSelect.Alpha = 0;
                    _hairSelect.Alpha = 0;
                    pallet = CharacterCreationValues.GetCustomClothesPallet();
                    _colorTablePicker.ChangePallete(pallet);
                    _colorTablePicker._layer = Layer.Pants;
                    // _colorTablePicker.ResetSelectedIndex();
                    _colorTablePicker.DeferredSelection();


                    break;
            }

            base.OnButtonClick(buttonID);
        }
        
        private bool ValidateValues(CreateCharTradeGump charTradeGump)
        {
            if (charTradeGump._skillsCombobox.Where(s => s.SelectedIndex >= 0).ToList().Count >= 3)
            {
                int duplicated = charTradeGump._skillsCombobox.GroupBy(o => o.SelectedIndex).Count(o => o.Count() > 1);

                if (duplicated > 0)
                {
                    UIManager.GetGump<CharCreationGump>()?.ShowMessage(ClilocLoader.Instance.GetString(1080032));

                    return false;
                }
            }
            else
            {
                UIManager.GetGump<CharCreationGump>()?.ShowMessage(Client.Version <= ClientVersion.CV_5090 ? ResGumps.YouMustHaveThreeUniqueSkillsChosen : ClilocLoader.Instance.GetString(1080032));

                return false;
            }

            return true;
        }

        private bool ValidateCharacter(PlayerMobile character)
        {
            int invalid = Validate(character.Name);

            if (invalid > 0)
            {
                UIManager.GetGump<CharCreationGump>()?.ShowMessage(ClilocLoader.Instance.GetString(invalid));

                return false;
            }

            return true;
        }

        public static int Validate(string name)
        {
            return Validate(name, 2, 16, true, false, true, 1, _SpaceDashPeriodQuote, Client.Version >= ClientVersion.CV_5020 ? _Disallowed : new string[] { }, _StartDisallowed);
        }

        public static int Validate
        (
            string name, int minLength, int maxLength, bool allowLetters, bool allowDigits, bool noExceptionsAtStart, int maxExceptions, char[] exceptions, string[] disallowed,
            string[] startDisallowed
        )
        {
            if (string.IsNullOrEmpty(name) || name.Length < minLength)
                return 3000612;
            else if (name.Length > maxLength)
                return 3000611;

            int exceptCount = 0;

            name = name.ToLowerInvariant();

            if (!allowLetters || !allowDigits || (exceptions.Length > 0 && (noExceptionsAtStart || maxExceptions < int.MaxValue)))
            {
                for (int i = 0; i < name.Length; ++i)
                {
                    char c = name[i];

                    if (c >= 'a' && c <= 'z')
                    {
                        exceptCount = 0;
                    }
                    else if (c >= '0' && c <= '9')
                    {
                        if (!allowDigits)
                            return 3000611;

                        exceptCount = 0;
                    }
                    else
                    {
                        bool except = false;

                        for (int j = 0; !except && j < exceptions.Length; ++j)
                            if (c == exceptions[j])
                                except = true;

                        if (!except || (i == 0 && noExceptionsAtStart))
                            return 3000611;

                        if (exceptCount++ == maxExceptions)
                            return 3000611;
                    }
                }
            }

            for (int i = 0; i < disallowed.Length; ++i)
            {
                int indexOf = name.IndexOf(disallowed[i]);

                if (indexOf == -1)
                    continue;

                bool badPrefix = (indexOf == 0);

                for (int j = 0; !badPrefix && j < exceptions.Length; ++j)
                    badPrefix = (name[indexOf - 1] == exceptions[j]);

                if (!badPrefix)
                    continue;

                bool badSuffix = ((indexOf + disallowed[i].Length) >= name.Length);

                for (int j = 0; !badSuffix && j < exceptions.Length; ++j)
                    badSuffix = (name[indexOf + disallowed[i].Length] == exceptions[j]);

                if (badSuffix)
                    return 3000611;
            }

            for (int i = 0; i < startDisallowed.Length; ++i)
            {
                if (name.StartsWith(startDisallowed[i]))
                    return 3000611;
            }

            return 0;
        }

        private static readonly char[] _SpaceDashPeriodQuote = new char[] { ' ', '-', '.', '\'' };

        private static string[] _StartDisallowed = new string[] { "seer", "counselor", "gm", "admin", "lady", "lord" };

        private static readonly string[] _Disallowed = new string[]
        {
            "jigaboo", "chigaboo", "wop", "kyke", "kike", "tit", "spic", "prick", "piss", "lezbo", "lesbo", "felatio", "dyke", "dildo", "chinc", "chink", "cunnilingus", "cum",
            "cocksucker", "cock", "clitoris", "clit", "ass", "hitler", "penis", "nigga", "nigger", "klit", "kunt", "jiz", "jism", "jerkoff", "jackoff", "goddamn", "fag",
            "blowjob", "bitch", "asshole", "dick", "pussy", "snatch", "cunt", "twat", "shit", "fuck", "tailor", "smith", "scholar", "rogue", "novice", "neophyte", "merchant",
            "medium", "master", "mage", "lb", "journeyman", "grandmaster", "fisherman", "expert", "chef", "carpenter", "british", "blackthorne", "blackthorn", "beggar",
            "archer", "apprentice", "adept", "gamemaster", "frozen", "squelched", "invulnerable", "osi", "origin"
        };

        private Item CreateItem(int id, ushort hue, Layer layer)
        {
            Item existsItem = _character.FindItemByLayer(layer);

            if (existsItem != null)
            {
                World.RemoveItem(existsItem, true);
                _character.Remove(existsItem);
            }

            if (id == 0)
            {
                return null;
            }

            // This is a workaround to avoid to see naked guy
            // We are simulating server objects into World.Items map.
            Item item = World.GetOrCreateItem(0x4000_0000 + (uint)layer); // use layer as unique Serial
            _character.Remove(item);
            item.Graphic = (ushort)id;
            item.Hue = hue;
            item.Layer = layer;
            item.Container = _character;
            //

            if (item.Layer is Layer.Hair || item.Layer is Layer.Beard)
                Console.WriteLine($@"{item.Layer.ToString()}: {CurrentColorOption[layer].Item2.ToString()}");


            return item;
        }

        private enum Buttons
        {
            MaleButton = 1,
            FemaleButton = 2,
            HumanButton = 3,
            ElfButton = 4,
            GargoyleButton = 5,
            Prev = 6,
            Next = 7,
            HairButton = 8,
            BeardButton = 9,
            ShirtButton = 10,
            PantsButton = 11
        }

        public class ColorSelectedEventArgs : EventArgs
        {
            public ColorSelectedEventArgs(Layer layer, ushort[] pallet, int selectedIndex)
            {
                Layer = layer;
                Pallet = pallet;
                SelectedIndex = selectedIndex;
            }

            public Layer Layer { get; }

            private ushort[] Pallet { get; }

            public int SelectedIndex { get; }

            public ushort SelectedHue => Pallet != null && SelectedIndex >= 0 && SelectedIndex < Pallet.Length ? Pallet[SelectedIndex] : (ushort)0xFFFF;
        }

        private class CustomColorPicker : Control
        {
            //private readonly ColorBox _box;
            private readonly int _cellH;
            private readonly int _cellW;
            private readonly ColorBox _colorPicker;
            private ColorPickerBox _colorPickerBox;
            private readonly int _columns, _rows;
            private int _lastSelectedIndex;
            private readonly Layer _layer;
            private readonly ushort[] _pallet;

            public CustomColorPicker(Layer layer, int label, ushort[] pallet, int rows, int columns)
            {
                Width = 121;
                Height = 25;
                _cellW = 125 / columns;
                _cellH = 280 / rows;
                _columns = columns;
                _rows = rows;
                _layer = layer;
                _pallet = pallet;

                bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                                   string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare
                                       (Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

                bool unicode = isAsianLang;
                byte font = (byte)(isAsianLang ? 3 : 9);
                ushort hue = (ushort)(isAsianLang ? 0xFFFF : 0);

                Add
                (
                    new Label(ClilocLoader.Instance.GetString(label), unicode, hue, font: font)
                    {
                        X = 0,
                        Y = 0
                    }
                );

                Add
                (
                    _colorPicker = new ColorBox(121, 23, (ushort)((pallet?[0] ?? 1) + 1))
                    {
                        X = 1,
                        Y = 15
                    }
                );

                _colorPicker.MouseUp += ColorPicker_MouseClick;
            }

            public ushort HueSelected => _colorPicker.Hue;

            public event EventHandler<ColorSelectedEventArgs> ColorSelected;

            public void SetSelectedIndex(int index)
            {
                if (_colorPickerBox != null)
                {
                    _colorPickerBox.SelectedIndex = index;

                    SetCurrentHue();
                }
            }

            private void SetCurrentHue()
            {
                _colorPicker.Hue = _colorPickerBox.SelectedHue;
                _lastSelectedIndex = _colorPickerBox.SelectedIndex;

                _colorPickerBox.Dispose();
            }

            private void ColorPickerBoxOnMouseUp(object sender, MouseEventArgs e)
            {
                int column = e.X / _cellW;
                int row = e.Y / _cellH;
                int selectedIndex = row * _columns + column;

                if (selectedIndex >= 0 && selectedIndex < _colorPickerBox.Hues.Length)
                {
                    ColorSelected?.Invoke(this, new ColorSelectedEventArgs(_layer, _colorPickerBox.Hues, selectedIndex));
                    SetCurrentHue();
                }
            }

            private void ColorPicker_MouseClick(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtonType.Left)
                {
                    _colorPickerBox?.Dispose();
                    _colorPickerBox = null;

                    if (_colorPickerBox == null)
                    {
                        _colorPickerBox = new ColorPickerBox(489, 141, _rows, _columns, _cellW, _cellH, _pallet)
                        {
                            IsModal = true,
                            LayerOrder = UILayer.Over,
                            ModalClickOutsideAreaClosesThisControl = true,
                            ShowLivePreview = false,
                            SelectedIndex = _lastSelectedIndex
                        };

                        UIManager.Add(_colorPickerBox);

                        _colorPickerBox.ColorSelectedIndex += ColorPickerBoxOnColorSelectedIndex;
                        _colorPickerBox.MouseUp += ColorPickerBoxOnMouseUp;
                    }
                }
            }

            private void ColorPickerBoxOnColorSelectedIndex(object sender, EventArgs e)
            {
                ColorSelected?.Invoke(this, new ColorSelectedEventArgs(_layer, _colorPickerBox.Hues, _colorPickerBox.SelectedIndex));
            }
        }

        internal class ProgressSlider : Control
        {
            private CreateCharAppearanceGump _parent;
            private GumpPic _progressSlider;
            private Button _nextButton, _previousButton;
            private int _selectedIndex;
            private Layer _layer;
            private RaceType _race;
            private PlayerMobile _character;
            private int _length;
            private bool _updateUI;
            private int _startingX;


            public ProgressSlider(CreateCharAppearanceGump parent, int x, int y, int length, Layer layer, RaceType race, PlayerMobile character, int selectedIndex)
            {
                _parent = parent;
                X = x;
                Y = y;
                SelectedIndex = selectedIndex;
                _race = race;
                _character = character;
                _layer = layer;
                _length = length;
                _startingX = x;


                Add
                (
                    _previousButton = new Button((int)Buttons.PrevButton, 157, 157, 157)
                    {
                        X = X,
                        Y = Y,
                        ButtonAction = ButtonAction.Activate
                    }
                );

                _previousButton.Alpha = SelectedIndex > 0 ? 1 : 0;

                if (SelectedIndex < _length)
                {
                    Add
                    (
                        _nextButton = new Button((int)Buttons.NextButton, 158, 158, 158)
                        {
                            X = X + 239,
                            Y = Y,
                            ButtonAction = ButtonAction.Activate
                        }
                    );
                }

                _nextButton.Alpha = SelectedIndex < _length ? 1 : 0;

                double cap = _layer == Layer.Beard ? 215 : _layer == Layer.Hair ? 212 : 202;
                var xPosition = Math.Max(1, (int)Math.Floor(cap * SelectedIndex / _length));

                Add
                (
                    _progressSlider = new GumpPic(X + 32 + xPosition, _layer == Layer.Beard ? Y - 2 : Y - 1, 177, 0)
                    {
                        Height = _layer == Layer.Beard ? 30 : 29,
                        Alpha = 0.45f,
                        Width = (int)Math.Round((double)(Math.Max(0, Math.Min(100, ((double)1 / (double)_length) * 100))), MidpointRounding.ToEven)
                    }
                );
            }

            public void ResetXPosition()
            {
                _progressSlider.X = _startingX + 32;
            }

            private enum Buttons
            {
                PrevButton = 20,
                NextButton = 21,
            }

            public override void OnButtonClick(int buttonID)
            {
                switch ((Buttons)buttonID)
                {
                    case Buttons.PrevButton:
                    {
                        if (SelectedIndex > 0)
                        {
                            SelectedIndex--;
                        }
                        else
                        {
                            return;
                        }

                        RequestUpdate();

                        break;
                    }

                    case Buttons.NextButton:
                    {
                        if (SelectedIndex < _length)
                        {
                            SelectedIndex++;
                        }
                        else
                        {
                            return;
                        }

                        RequestUpdate();

                        break;
                    }
                }

                base.OnButtonClick(buttonID);
            }


            public override void Update()
            {
                base.Update();

                if (_updateUI)
                {
                    UpdateUI();

                    _updateUI = false;
                }
            }

            public void RequestUpdate()
            {
                _updateUI = true;
            }

            private void UpdateUI()
            {
                if (IsDisposed)
                {
                    return;
                }

                double cap = _layer == Layer.Beard ? 215 : _layer == Layer.Hair ? 212 : 202;
                var xPosition = Math.Max(1, (int)Math.Floor(cap * SelectedIndex / _length));

                if (SelectedIndex > 0)
                {
                    _previousButton.IsEnabled = true;
                    _previousButton.Alpha = 1;
                }
                else
                {
                    _previousButton.IsEnabled = false;
                    _previousButton.Alpha = 0;
                }

                if (SelectedIndex < _length - 1)
                {
                    _nextButton.IsEnabled = true;
                    _nextButton.Alpha = 1;
                }
                else
                {
                    _nextButton.IsEnabled = false;
                    _nextButton.Alpha = 0;
                }

                _progressSlider.X = _startingX + 32 + xPosition;
            }

            public event EventHandler<int> OnOptionSelected;

            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    _selectedIndex = value;

                    OnOptionSelected?.Invoke(this, value);
                }
            }
        }

        internal class CustomColorTablePicker : Gump
        {
            private readonly int _cellWidth, _cellHeight, _columns, _rows;
            private ushort[] _hues;
            public Layer _layer;
            private int _selectedIndex = -1;
            private Rectangle _selectedBox = Rectangle.Empty;
            private CreateCharAppearanceGump _parent;


            public event EventHandler<ColorSelectedEventArgs> ColorSelected;

            public CustomColorTablePicker(CreateCharAppearanceGump parent, int x, int y, int cellWidth, int cellHeight, int columns, int rows, ushort[] hues, Layer layer) : base(0, 0)
            {
                _parent = parent;
                X = x;
                Y = y;
                _cellWidth = cellWidth;
                _cellHeight = cellHeight;
                _columns = columns;
                _rows = rows;
                _hues = hues;
                _layer = layer;

                Width = _columns * _cellWidth;
                Height = _rows * _cellHeight;

                AcceptMouseInput = true;

                // Defer the selection action to a method called after initialization
                DeferredSelection();
            }
            
            public void DeferredSelection()
            {
                _selectedIndex = _parent.CurrentColorOption[_layer].Item1;
                
                if (_selectedIndex >= 0 && _selectedIndex < _hues.Length)
                {
                    int column = _selectedIndex % _columns;
                    int row = _selectedIndex / _columns;

                    // Set the selected box and invoke the event
                    _selectedBox = new Rectangle(X + column * _cellWidth, Y + row * _cellHeight, _cellWidth, _cellHeight);
        
                    // Fire the event as if it was selected by the user
                    // SelectColor();
                }
            }

            private void SelectColor()
            {
                if (_selectedIndex == 0xFFFF)
                {
                    return;
                }
                
                if (_layer != Layer.Invalid)
                {
                    Item item;

                    if (_parent._character.Race == RaceType.GARGOYLE && _layer == Layer.Shirt)
                    {
                        item = _parent._character.FindItemByLayer(Layer.Robe);
                    }
                    else
                    {
                        item = _parent._character.FindItemByLayer(_parent._characterInfo.IsFemale && _layer == Layer.Pants ? Layer.Skirt : _layer);
                    }

                    if (item != null)
                    {
                        item.Hue = _parent.CurrentColorOption[_layer].Item2;
                        Console.WriteLine($@"{item.Layer.ToString()}: {_parent.CurrentColorOption[_layer].Item2.ToString()}");
                    }
                }

                _parent._paperDoll.RequestUpdate();
            }

            // Store and expose the selected hue (derived from the selected index and palette)
            public ushort SelectedHue => (_selectedIndex >= 0 && _selectedIndex < _hues.Length) ? _hues[_selectedIndex] : (ushort)0xFFFF;
            
            

            // Store and expose the selected index
            public int SelectedIndex => _selectedIndex;

            // Handle mouse clicks to select a color
            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    int column = x / _cellWidth;
                    int row = y / _cellHeight;
                    int selectedIndex = row * _columns + column;

                    if (selectedIndex >= 0 && selectedIndex < _hues.Length)
                    {
                        _selectedIndex = selectedIndex;
                        _selectedBox = new Rectangle(X + column * _cellWidth, Y + row * _cellHeight, _cellWidth, _cellHeight);

                        ColorSelected?.Invoke(this, new ColorSelectedEventArgs(_layer, _hues, _selectedIndex));
                    }
                }
            }
            
            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                for (int row = 0; row < _rows; row++)
                {
                    for (int column = 0; column < _columns; column++)
                    {
                        int hueIndex = row * _columns + column;

                        if (hueIndex >= _hues.Length)
                            break;

                        ushort hue = _hues[hueIndex];
                        Rectangle box = new Rectangle(x + column * _cellWidth, y + row * _cellHeight, _cellWidth, _cellHeight);

                        batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), box, ShaderHueTranslator.GetHueVector(hue, false, .75f));
                    }
                }
                
                // Draw the outline last, after all the boxes are drawn, so it doesn't get overwritten
                if (_selectedBox != Rectangle.Empty)
                {
                    DrawOutline(batcher, _selectedBox, Color.Gold, 3); // Draw gold outline
                }

                return true;
            }
            
            private void DrawOutline(UltimaBatcher2D batcher, Rectangle box, Color outlineColor, int thickness)
            {
                // Define the color vector for the gold color
                Vector3 goldColor = ShaderHueTranslator.GetHueVector(1710, false, 1);

                // Draw the top edge
                batcher.Draw(SolidColorTextureCache.GetTexture(Color.White),
                             new Rectangle(box.X - thickness, box.Y - thickness, box.Width + thickness * 2, thickness),
                             goldColor);

                // Draw the bottom edge
                batcher.Draw(SolidColorTextureCache.GetTexture(Color.White),
                             new Rectangle(box.X - thickness, box.Y + box.Height, box.Width + thickness * 2, thickness),
                             goldColor);

                // Draw the left edge
                batcher.Draw(SolidColorTextureCache.GetTexture(Color.White),
                             new Rectangle(box.X - thickness, box.Y - thickness, thickness, box.Height + thickness * 2),
                             goldColor);

                // Draw the right edge
                batcher.Draw(SolidColorTextureCache.GetTexture(Color.White),
                             new Rectangle(box.X + box.Width, box.Y - thickness, thickness, box.Height + thickness * 2),
                             goldColor);
            }

            public void ChangePallete(ushort[] pallet)
            {
                _hues = pallet;
            }

            public void ResetSelectedIndex()
            {
                _selectedBox = new Rectangle();
                _selectedIndex = -1;
            }
        }

    }
}