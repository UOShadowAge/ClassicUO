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

using System.IO;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps.Login
{
    internal class LoginBackground : Gump
    {
        private static Texture2D _imageTexture;

        private static Texture2D ImageTexture
        {
            get
            {
                if (_imageTexture == null || _imageTexture.IsDisposed)
                {
                    using var stream = new MemoryStream(Loader.GetLoginBackgroundImage().ToArray());
                    _imageTexture = Texture2D.FromStream(Client.Game.GraphicsDevice, stream);
                }

                return _imageTexture;
            }
        }

        private static Texture2D _overlayTexture;

        private static Texture2D OverlayTexture
        {
            get
            {
                if (_overlayTexture == null || _overlayTexture.IsDisposed)
                {
                    using var stream = new MemoryStream(Loader.GetLoginBackgroundImage().ToArray());
                    _overlayTexture = Texture2D.FromStream(Client.Game.GraphicsDevice, stream);
                }

                return _overlayTexture;
            }
        }

        private Rectangle _imageSource, _imageDest;
        private Rectangle _overlaySource, _overlayDest;

        private float _animTime;

        public LoginBackground(World world) : base(world, 0, 0)
        {
            Width = 1280;
            Height = 960;

            CanCloseWithEsc = false;
            CanCloseWithRightClick = false;
            AcceptKeyboardInput = false;

            LayerOrder = UILayer.Under;

            _imageSource.Width = Width;
            _imageSource.Height = Height;
            _imageDest.Width = Width;
            _imageDest.Height = Height;

            _overlaySource.Width = Width;
            _overlaySource.Height = Height;
            _overlayDest.Width = Width;
            _overlayDest.Height = Height;
            
            // Quit Button
            Add
            (
                new Button(0, 1482, 1481, 1480)
                {
                    X = 1197,
                    Y = 41,
                    ButtonAction = ButtonAction.Activate,
                    AcceptKeyboardInput = false
                }
            );
        }

        public override void Update()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Update();

            if (_animTime < Time.Ticks)
            {
                _animTime = Time.Ticks + 60f;

                if (_overlayTexture != null)
                {
                    _overlaySource.X = _overlaySource.Right % _overlayTexture.Width;
                }
            }
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            Vector3 hueVector = Vector3.UnitZ;

            var imageDest = _imageDest;
            var overlayDest = _overlayDest;

            imageDest.Offset(x, y);
            overlayDest.Offset(x, y);

            batcher.DrawTiled(ImageTexture, imageDest, _imageSource, hueVector);

            hueVector.Z = RandomHelper.GetValue(70, 80) / 100.0f;

            batcher.DrawTiled(OverlayTexture, overlayDest, _overlaySource, hueVector);

            return base.Draw(batcher, x, y);
        }

        public override void OnButtonClick(int buttonID)
        {
            Client.Game.Exit();
        }
    }
}