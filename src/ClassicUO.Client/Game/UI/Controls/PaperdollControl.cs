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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps.Login;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Point = Microsoft.Xna.Framework.Point;

namespace ClassicUO.Game.UI.Controls
{
    internal class PaperdollControl : Control
    {
        private static readonly Layer[] _layerOrder =
        {
            Layer.Cloak, Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
            Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Waist, Layer.Necklace,
            Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
        };

        private static readonly Layer[] _layerOrder_quiver_fix =
        {
            Layer.Shirt, Layer.Pants, Layer.Shoes, Layer.Legs, Layer.Arms, Layer.Torso, Layer.Tunic,
            Layer.Ring, Layer.Bracelet, Layer.Face, Layer.Gloves, Layer.Skirt, Layer.Robe, Layer.Cloak, Layer.Waist,
            Layer.Necklace,
            Layer.Hair, Layer.Beard, Layer.Earrings, Layer.Helmet, Layer.OneHanded, Layer.TwoHanded, Layer.Talisman
        };

        private readonly PaperDollGump _paperDollGump;


        private bool _updateUI;
        private int _width;
        private int _height;
        private Character _character;
        private World _world;

        public PaperdollControl(World world, int x, int y, uint serial, Character character, PaperDollGump paperDollGump, int width, int height)
        {
            _world = world;
            X = x;
            Y = y;
            AcceptMouseInput = false;
            LocalSerial = serial;
            _updateUI = true;
            _width = width;
            _height = height;
            _character = character;
        }

        public bool HasFakeItem { get; private set; }


        public override void Update()
        {
            base.Update();

            if (_updateUI)
            {
                UpdateUI();

                _updateUI = false;
            }
        }

        public void SetFakeItem(bool value)
        {
            _updateUI = HasFakeItem && !value || !HasFakeItem && value;
            HasFakeItem = value;
        }

        public void UpdateUI()
        {
            if (IsDisposed)
            {
                return;
            }

            Character character = _character;

            
            if (character.Player == null || character.Player.IsDestroyed)
            {
                Dispose();

                return;
            }

            if (character.Player != null && (character.Player.Name == "" || character.Items == null))
            {
                return;
            }

            Clear();

            // Add the base gump - the semi-naked paper doll.
            ushort body;
            ushort hue = character.Hue;

            if (character.Player.Graphic == 0x0191 || character.Player.Graphic == 0x0193)
            {
                body = 0x000D;
            }
            else if (character.Player.Graphic == 0x025D)
            {
                body = 0x000E;
            }
            else if (character.Player.Graphic == 0x025E)
            {
                body = 0x000F;
            }
            else if (character.Player.Graphic == 0x029A || character.Player.Graphic == 0x02B6)
            {
                body = 0x029A;
            }
            else if (character.Player.Graphic == 0x029B || character.Player.Graphic == 0x02B7)
            {
                body = 0x0299;
            }
            else if (character.Player.Graphic == 0x04E5)
            {
                body = 0xC835;
            }
            else if (character.Player.Graphic == 0x03DB)
            {
                body = 0x000C;
                hue = 0x03EA;
            }
            else if (character.Player.IsFemale)
            {
                body = 0x000D;
            }
            else
            {
                body = 0x000C;
            }

            // body
            Add
            (
                new GumpPic(0, 0, body, hue)
                {
                    IsPartialHue = true,
                    Width = _width,
                    Height = _height
                }
            );


            if (character.Player.Graphic == 0x03DB)
            {
                Add
                (
                    new GumpPic(0, 0, 0xC72B, character.Player.Hue)
                    {
                        AcceptMouseInput = true,
                        IsPartialHue = true,
                        Width = _width,
                        Height = _height
                    }
                );
            }

            // equipment
            Item equipItem = character.Player.FindItemByLayer(Layer.Cloak);
            Item arms = character.Player.FindItemByLayer(Layer.Arms);

            bool switch_arms_with_torso = false;

            if (arms != null)
            {
                switch_arms_with_torso = arms.Graphic == 0x1410 || arms.Graphic == 0x1417;
            }
            else if (
                HasFakeItem
                && Client.Game.UO.GameCursor.ItemHold.Enabled
                && !Client.Game.UO.GameCursor.ItemHold.IsFixedPosition
                && (byte)Layer.Arms == Client.Game.UO.GameCursor.ItemHold.ItemData.Layer
            )
            {
                switch_arms_with_torso =
                    Client.Game.UO.GameCursor.ItemHold.Graphic == 0x1410
                    || Client.Game.UO.GameCursor.ItemHold.Graphic == 0x1417;
            }

            Layer[] layers;

            if (equipItem != null)
            {
                layers = equipItem.ItemData.IsContainer ? _layerOrder_quiver_fix : _layerOrder;
            }
            else if (
                HasFakeItem
                && Client.Game.UO.GameCursor.ItemHold.Enabled
                && !Client.Game.UO.GameCursor.ItemHold.IsFixedPosition
                && (byte)Layer.Cloak == Client.Game.UO.GameCursor.ItemHold.ItemData.Layer
            )
            {
                layers = Client.Game.UO.GameCursor.ItemHold.ItemData.IsContainer
                    ? _layerOrder_quiver_fix
                    : _layerOrder;
            }
            else
            {
                layers = _layerOrder;
            }


            for (int i = 0; i < layers.Length; i++)
            {
                Layer layer = layers[i];

                if (switch_arms_with_torso)
                {
                    if (layer == Layer.Arms)
                    {
                        layer = Layer.Torso;
                    }
                    else if (layer == Layer.Torso)
                    {
                        layer = Layer.Arms;
                    }
                }

                equipItem = character.Items.FirstOrDefault(i => i.Layer == layer);

                if (equipItem != null)
                {
                    ushort id = GetAnimID(character.Player.Graphic, equipItem.ItemData.AnimID, character.Player.IsFemale);

                    Add
                    (
                        new GumpPicEquipment
                        (
                            _world,
                            _paperDollGump,
                            equipItem.Serial,
                            0,
                            0,
                            id,
                            (ushort) (equipItem.Hue & 0x3FFF),
                            layer
                        )
                        {
                            AcceptMouseInput = true,
                            IsPartialHue = equipItem.ItemData.IsPartialHue,
                            CanLift = _world.InGame && !_world.Player.IsDead && layer != Layer.Beard && layer != Layer.Hair && (_paperDollGump.CanLift || LocalSerial == _world.Player),
                            Alpha = layer == Layer.Helmet ? 0.75f : 1,
                            Width = _width,
                            Height = _height
                        }
                    );
                }
            }


            equipItem = character.Items.FirstOrDefault(i => i.Layer == Layer.Backpack);

            if (equipItem != null && equipItem.ItemData.AnimID != 0)
            {
                ushort backpackGraphic = (ushort) (equipItem.ItemData.AnimID + Constants.MALE_GUMP_OFFSET);

                var gump = Client.Game.UO.Gumps;

                    switch (ProfileManager.CurrentProfile.BackpackStyle)
                    {
                        case 1:
                            if (gump.GetGump(0x777B).Texture != null)
                            {
                                backpackGraphic = 0x777B; // Suede Backpack
                            }

                            break;
                        case 2:
                            if (gump.GetGump(0x777C).Texture != null)
                            {
                                backpackGraphic = 0x777C; // Polar Bear Backpack
                            }

                            break;
                        case 3:
                            if (gump.GetGump(0x777D).Texture != null)
                            {
                                backpackGraphic = 0x777D; // Ghoul Skin Backpack
                            }

                            break;
                        default:
                            if (gump.GetGump(0xC4F6).Texture != null)
                            {
                                backpackGraphic = 0xC4F6; // Default Backpack
                            }

                            break;
                    }


                    int bx = 0;

                if (_world.ClientFeatures.PaperdollBooks)
                {
                    bx = 6;
                }

                Add
                (
                    new GumpPicEquipment
                    (
                        _world,
                        _paperDollGump,
                        equipItem.Serial,
                        -bx,
                        0,
                        backpackGraphic,
                        (ushort) (equipItem.Hue & 0x3FFF),
                        Layer.Backpack
                    )
                    {
                        AcceptMouseInput = true,
                        Width = _width,
                        Height = _height,
                    }
                );
            }
        }

        public void RequestUpdate()
        {
            _updateUI = true;
        }

        protected static ushort GetAnimID(ushort graphic, ushort animID, bool isfemale)
        {
            int offset = isfemale ? Constants.FEMALE_GUMP_OFFSET : Constants.MALE_GUMP_OFFSET;

            if (Client.Game.UO.Version >= ClientVersion.CV_7000 && animID == 0x03CA                          // graphic for dead shroud
                                                                && (graphic == 0x02B7 || graphic == 0x02B6)) // dead gargoyle graphics
            {
                animID = 0x0223;
            }

            Client.Game.UO.Animations.ConvertBodyIfNeeded(ref graphic);

            if (Client.Game.UO.FileManager.Animations.EquipConversions.TryGetValue(graphic, out Dictionary<ushort, EquipConvData> dict))
            {
                if (dict.TryGetValue(animID, out EquipConvData data))
                {
                    if (data.Gump > Constants.MALE_GUMP_OFFSET)
                    {
                        animID = (ushort)(data.Gump >= Constants.FEMALE_GUMP_OFFSET ? data.Gump - Constants.FEMALE_GUMP_OFFSET : data.Gump - Constants.MALE_GUMP_OFFSET);
                    }
                    else
                    {
                        animID = data.Gump;
                    }
                }
            }
            if (
                animID + offset > GumpsLoader.MAX_GUMP_DATA_INDEX_COUNT
                || Client.Game.UO.Gumps.GetGump((ushort)(animID + offset)).Texture == null
            )
            {
                // inverse
                offset = isfemale ? Constants.MALE_GUMP_OFFSET : Constants.FEMALE_GUMP_OFFSET;
            }

            if (Client.Game.UO.Gumps.GetGump((ushort)(animID + offset)).Texture == null)
            {
                Log.Error(
                    $"Texture not found in paperdoll: gump_graphic: {(ushort)(animID + offset)}"
                );
            }

            return (ushort)(animID + offset);
        }

        private class GumpPicEquipment : GumpPic
        {
            private readonly Layer _layer;
            private readonly Gump _gump;
            private readonly World _world;

            public GumpPicEquipment
            (
                World world,
                Gump gump,
                uint serial,
                int x,
                int y,
                ushort graphic,
                ushort hue,
                Layer layer
            ) : base(x, y, graphic, hue)
            {
                _world = world;
                _gump = gump;
                LocalSerial = serial;
                CanMove = false;
                _layer = layer;

                if (SerialHelper.IsValid(serial) && _world.InGame)
                {
                    SetTooltip(serial);
                }
            }

            public bool CanLift { get; set; }

            protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                if (button != MouseButtonType.Left)
                {
                    return false;
                }

                // this check is necessary to avoid crashes during character creation
                if (_world.InGame)
                {
                    GameActions.DoubleClick(_world, LocalSerial);
                }

                return true;
            }

            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                SelectedObject.Object = _world.Get(LocalSerial);
                base.OnMouseUp(x, y, button);
            }

            public override void Update()
            {
                base.Update();

                if (_world.InGame)
                {
                    if (
                        CanLift
                        && !Client.Game.UO.GameCursor.ItemHold.Enabled
                        && Mouse.LButtonPressed
                        && UIManager.LastControlMouseDown(MouseButtonType.Left) == this
                        && (
                            Mouse.LastLeftButtonClickTime != 0xFFFF_FFFF
                            && Mouse.LastLeftButtonClickTime != 0
                            && Mouse.LastLeftButtonClickTime + Mouse.MOUSE_DELAY_DOUBLE_CLICK
                            < Time.Ticks
                            || Mouse.LDragOffset != Point.Zero
                        )
                    )
                    {
                        GameActions.PickUp(_world, LocalSerial, 0, 0);

                        if (_layer == Layer.OneHanded || _layer == Layer.TwoHanded)
                        {
                            _world.Player.UpdateAbilities();
                        }
                    }
                    else if (MouseIsOver)
                    {
                        SelectedObject.Object = _world.Get(LocalSerial);
                    }
                }
            }

            protected override void OnMouseOver(int x, int y)
            {
                SelectedObject.Object = _world.Get(LocalSerial);
            }
        }
    }
}