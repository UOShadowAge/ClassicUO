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

using System.Linq;
using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Assets;
using ClassicUO.Resources;
using System.Collections.Generic;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Gumps.CharCreation
{
    internal class CreateCharTradeGump : Gump
    {
        public readonly CustomHSliderBar[] _attributeSliders;
        private readonly PlayerMobile _character;
        public readonly CustomCombobox[] _skillsCombobox;
        public readonly CustomHSliderBar[] _skillSliders;
        public readonly List<SkillEntry> _skillList;
        private World _world;


        public CreateCharTradeGump(World world, PlayerMobile character, ProfessionInfo profession) : base(world, 0, 0)
        {
            _world = world;
            _character = character;

            foreach (Skill skill in _character.Skills)
            {
                skill.ValueFixed = 0;
                skill.BaseFixed = 0;
                skill.CapFixed = 0;
                skill.Lock = Lock.Locked;
            }

            bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || 
                string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

            bool unicode = isAsianLang;

            // strength, dexterity, intelligence
            Add
            (
                new Label("Str:", unicode, 1150, font: 0)
                {
                    X = 462, Y = 580
                }
            );

            Add
            (
                new Label("Dex:", unicode, 1150, font: 0)
                {
                    X = 605, Y = 580
                }
            );

            Add
            (
                new Label("Int:", unicode, 1150, font: 0)
                {
                    X = 750, Y = 580
                }
            );

            // sliders for attributes
            _attributeSliders = new CustomHSliderBar[3];

            (var defSkillsValues, var defStatsValues) = ProfessionInfo.GetDefaults(Client.Game.UO.Version);

            Add
            (
                _attributeSliders[0] = new CustomHSliderBar
                (
                    450,
                    605,
                    110,
                    10,
                    60,
                    profession.Name != "Custom" ? profession.StatsVal[0] : defStatsValues[0],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );

            Add
            (
                _attributeSliders[1] = new CustomHSliderBar
                (
                    590,
                    605,
                    110,
                    10,
                    60,
                    profession.Name != "Custom" ? profession.StatsVal[1] : defStatsValues[1],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );

            Add
            (
                _attributeSliders[2] = new CustomHSliderBar
                (
                    735,
                    605,
                    110,
                    10,
                    60,
                    profession.Name != "Advanced" ? profession.StatsVal[2] : defStatsValues[2],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );

            var clientFlags = World.ClientLockedFeatures.Flags;

            _skillList = Client.Game.UO.FileManager.Skills.Skills
                               .Where(s =>
                                          // All standard client versions ignore these skills by defualt
                                          //s.Index != 26 && // MagicResist
                                     s.Name != "Spellweaving" && // Spellweaving
                                     s.Name != "Bushido" &&  // Bushido
                                     s.Name != "Ninjitsu" &&  // Ninjitsu
                                     s.Name != "Mysticism" &&  // Mysticism
                                     s.Name != "Imbuing" &&  // Imbuing
                                     s.Name != "Throwing" &&  // Throwing
                                          (character.Race == RaceType.GARGOYLE || s.Index != 57) // Throwing for gargoyle only
                               )
                               .Where(s =>
                                    clientFlags.HasFlag(LockedFeatureFlags.AOS) ||
                                    (
                                              s.Index != 51 && // Chivlary
                                              s.Index != 50 && // Focus
                                              s.Index != 49    // Necromancy
                                          )
                               )

                               .Where(s =>
                                    clientFlags.HasFlag(LockedFeatureFlags.SE) ||
                                          (
                                              s.Index != 52 && // Bushido
                                              s.Index != 53    // Ninjitsu
                                          )
                               )

                               .Where(s =>
                                    clientFlags.HasFlag(LockedFeatureFlags.SA) ||
                                          (
                                              s.Index != 55 && // Mysticism
                                              s.Index != 56    // Imbuing
                                          )
                               )
                               .ToList();

            // do not include archer if it's a gargoyle
            if (character.Race == RaceType.GARGOYLE)
            {
                var archeryEntry = _skillList.FirstOrDefault(s => s.Index == 31);
                if (archeryEntry != null)
                {
                    _skillList.Remove(archeryEntry);
                }
            }

            var skillNames = _skillList.Select(s => s.Name).ToArray();

            int y = 575;
            _skillSliders = new CustomHSliderBar[CharCreationGump._skillsCount];
            _skillsCombobox = new CustomCombobox[CharCreationGump._skillsCount];

            
            // Skills
            
            // 1
            Add
            (
                _skillsCombobox[0] = new CustomCombobox
                (
                    _world,
                    440,
                    645,
                    195,
                    skillNames,
                    profession.Name != "Advanced" ? profession.SkillDefVal[0, 0] : -1,
                    200,
                    true,
                    $"Skill #1"
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );

            Add
            (
                _skillSliders[0] = new CustomHSliderBar
                (
                    445,
                    645 + 32,
                    190,
                    0,
                    50,
                    profession.Name != "Advanced" ? profession.SkillDefVal[0, 1] : defSkillsValues[0, 1],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true,
                    0,
                    0,
                    true,
                    false,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );
            
            // 2
            Add
            (
                _skillsCombobox[1] = new CustomCombobox
                (
                    _world,
                    655,
                    645,
                    192,
                    skillNames,
                    profession.Name != "Advanced" ? profession.SkillDefVal[1, 0] : -1,
                    200,
                    true,
                    $"Skill #2"
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced",
                    
                }
            );

            Add
            (
                _skillSliders[1] = new CustomHSliderBar
                (
                    660,
                    645 + 32,
                    187,
                    0,
                    50,
                    profession.Name != "Advanced" ? profession.SkillDefVal[1, 1] : defSkillsValues[1, 1],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true,
                    0,
                    0,
                    true,
                    false,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );
            
            // 3
            Add
            (
                _skillsCombobox[2] = new CustomCombobox
                (
                    _world,
                    440,
                    710,
                    195,
                    skillNames,
                    profession.Name != "Advanced" ? profession.SkillDefVal[2, 0] : -1,
                    200,
                    true,
                    $"Skill #3"
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced",
                }
            );

            Add
            (
                _skillSliders[2] = new CustomHSliderBar
                (
                    445,
                    710 + 32,
                    190,
                    0,
                    50,
                    profession.Name != "Advanced" ? profession.SkillDefVal[2, 1] : defSkillsValues[2, 1],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true,
                    0,
                    0,
                    true,
                    false,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );
            
            // 4
            Add
            (
                _skillsCombobox[3] = new CustomCombobox
                (
                    _world,
                    655,
                    710,
                    192,
                    skillNames,
                    profession.Name != "Advanced" ? profession.SkillDefVal[3, 0] : -1,
                    200,
                    true,
                    $"Skill #4"
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );

            Add
            (
                _skillSliders[3] = new CustomHSliderBar
                (
                    660,
                    710 + 32,
                    187,
                    0,
                    50,
                    profession.Name != "Advanced" ? profession.SkillDefVal[3, 1] : defSkillsValues[3, 1],
                    CustomHSliderBarStyle.BlueWidgetNoBar,
                    true,
                    0,
                    0,
                    true,
                    false,
                    true
                )
                {
                    AcceptMouseInput = profession.Name == "Advanced"
                }
            );

            for (int i = 0; i < _attributeSliders.Length; i++)
            {
                for (int j = 0; j < _attributeSliders.Length; j++)
                {
                    if (i != j)
                    {
                        _attributeSliders[i].AddParisSlider(_attributeSliders[j]);
                    }
                }
            }

            for (int i = 0; i < _skillSliders.Length; i++)
            {
                for (int j = 0; j < _skillSliders.Length; j++)
                {
                    if (i != j)
                    {
                        _skillSliders[i].AddParisSlider(_skillSliders[j]);
                    }
                }
            }
        }

        public override void OnButtonClick(int buttonID)
        {
            CharCreationGump charCreationGump = UIManager.GetGump<CharCreationGump>();

            switch ((Buttons) buttonID)
            {
                case Buttons.Next:

                    if (ValidateValues())
                    {
                        for (int i = 0; i < _skillsCombobox.Length; i++)
                        {
                            if (_skillsCombobox[i].SelectedIndex != -1)
                            {
                                Skill skill = _character.Skills[_skillList[_skillsCombobox[i].SelectedIndex].Index];
                                skill.ValueFixed = (ushort) _skillSliders[i].Value;
                                skill.BaseFixed = 0;
                                skill.CapFixed = 0;
                                skill.Lock = Lock.Locked;
                            }
                        }

                        _character.Strength = (ushort) _attributeSliders[0].Value;
                        _character.Intelligence = (ushort) _attributeSliders[1].Value;
                        _character.Dexterity = (ushort) _attributeSliders[2].Value;

                        charCreationGump.SetAttributes(true);
                    }

                    break;
            }

            base.OnButtonClick(buttonID);
        }

        private bool ValidateValues()
        {
            if (_skillsCombobox.Where(s => s.SelectedIndex >= 0).ToList().Count >= 3)
            {
                int duplicated = _skillsCombobox.GroupBy(o => o.SelectedIndex).Count(o => o.Count() > 1);

                if (duplicated > 0)
                {
                    UIManager.GetGump<CharCreationGump>()?.ShowMessage(Client.Game.UO.FileManager.Clilocs.GetString(1080032));

                    return false;
                }
            }
            else
            {
                UIManager.GetGump<CharCreationGump>()?.ShowMessage(Client.Game.UO.Version <= ClientVersion.CV_5090 ? ResGumps.YouMustHaveThreeUniqueSkillsChosen : Client.Game.UO.FileManager.Clilocs.GetString(1080032));

                return false;
            }

            return true;
        }

        private enum Buttons
        {
            Prev,
            Next
        }
    }
}