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
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls
{
    internal class CustomCombobox : Control
    {
        private readonly byte _font;
        private readonly string[] _items;
        private readonly Label _label;
        private readonly int _maxHeight;
        private int _selectedIndex;
        private World _world;

        public CustomCombobox
        (
            World world,
            int x,
            int y,
            int width,
            string[] items,
            int selected = -1,
            int maxHeight = 200,
            bool showArrow = false,
            string emptyString = "",
            byte font = 3
        )
        {
            _world = world;
            X = x;
            Y = y;
            Width = width;
            Height = 34;
            SelectedIndex = selected;
            _font = font;
            _items = items;
            _maxHeight = maxHeight;

            Add
            (
                new GumpPicTiled(177)
                {
                    Width = width, Height = Height,
                    Alpha = .15f
                }
            );

            string initialText = selected > -1 ? items[selected] : emptyString;

            bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || 
                string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

            bool unicode = isAsianLang;
            byte font1 = (byte)(isAsianLang ? 1 : _font);

            Add
            (
                _label = new Label(initialText, unicode, 1150, font: font1)
                {
                    X = 12, Y = 5
                }
            );

            if (showArrow)
            {
                Add(new GumpPic(width - 16, 6, 0x00FC, 0)
                {
                    Alpha = .50f
                });
            }
        }


        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;

                if (_items != null)
                {
                    _label.Text = _items[value];

                    OnOptionSelected?.Invoke(this, value);
                }
            }
        }


        public event EventHandler<int> OnOptionSelected;


        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (batcher.ClipBegin(x, y, Width, Height))
            {
                base.Draw(batcher, x, y);
                batcher.ClipEnd();
            }

            return true;
        }


        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            if (button != MouseButtonType.Left)
            {
                return;
            }

            int comboY = ScreenCoordinateY + Offset.Y;

            if (comboY < 0)
            {
                comboY = 0;
            }
            else if (comboY + _maxHeight > Client.Game.Window.ClientBounds.Height)
            {
                comboY = Client.Game.Window.ClientBounds.Height - _maxHeight;
            }

            if (!AcceptMouseInput)
                return;

            UIManager.Add
            (
                new CustomComboboxGump
                (
                    _world,
                    ScreenCoordinateX,
                    comboY,
                    Width,
                    _maxHeight,
                    _items,
                    _font,
                    this
                )
            );

            base.OnMouseUp(x, y, button);
        }

        private class CustomComboboxGump : Gump
        {
            private const int ELEMENT_HEIGHT = 15;


            private readonly CustomCombobox _combobox;

            public CustomComboboxGump
            (
                World world,
                int x,
                int y,
                int width,
                int maxHeight,
                string[] items,
                byte font,
                CustomCombobox combobox
            ) : base(world, 0, 0)
            {
                CanMove = false;
                AcceptMouseInput = true;
                X = x;
                Y = y;

                IsModal = true;
                LayerOrder = UILayer.Over;
                ModalClickOutsideAreaClosesThisControl = true;

                _combobox = combobox;

                GumpPicTiled background;
                Add(background = new GumpPicTiled(161));
                background.AcceptMouseInput = false;

                HoveredLabel[] labels = new HoveredLabel[items.Length];

                bool isAsianLang = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || 
                    string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                    string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0;

                bool unicode = isAsianLang;
                byte font1 = (byte)(isAsianLang ? 1 : 3);

                for (int i = 0; i < items.Length; i++)
                {
                    string item = items[i];

                    if (item == null)
                    {
                        item = string.Empty;
                    }

                    HoveredLabel label = new HoveredLabel
                    (
                        item,
                        unicode,
                        1150,
                        1152,
                        1150,
                        font: font1
                    )
                    {
                        X = 5,
                        Y = i * ELEMENT_HEIGHT + 4,
                        DrawBackgroundCurrentIndex = true,
                        IsVisible = item.Length != 0,
                        Tag = i,
                        Alpha = .75f
                    };

                    label.MouseUp += LabelOnMouseUp;

                    labels[i] = label;
                }

                int totalHeight = Math.Min(maxHeight, labels.Max(o => o.Y + o.Height));
                int maxWidth = Math.Max(width, labels.Max(o => o.X + o.Width));

                ScrollArea area = new ScrollArea
                (
                    0,
                    0,
                    maxWidth + 18,
                    totalHeight + 4,
                    true
                );

                foreach (HoveredLabel label in labels)
                {
                    label.Width = maxWidth;
                    area.Add(label);
                }

                Add(area);

                background.Width = maxWidth;
                background.Height = totalHeight;
            }


            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                if (batcher.ClipBegin(x, y, Width, Height))
                {
                    base.Draw(batcher, x, y);

                    batcher.ClipEnd();
                }

                return true;
            }

            private void LabelOnMouseUp(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtonType.Left)
                {
                    _combobox.SelectedIndex = (int) ((Label) sender).Tag;

                    Dispose();
                }
            }
        }
    }
}