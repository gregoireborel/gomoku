using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gomoku
{
    class MenuButton
    {
        public Texture2D   _texture, _overText;
        public Vector2     _position;
        bool                _overlay;
        public float       _sizeX, _sizeY;

        public MenuButton() { _position.X = 0; _position.Y = 0; }

        public      MenuButton(Texture2D img, Texture2D overlay, float sizeX, float sizeY)
        {
            _texture = img;
            _overText = overlay;
            _position.X = 0; _position.Y = 0;
            _overlay = false;
            _sizeX = sizeX;
            _sizeY = sizeY;
        }

        public bool         Update(float mouseX, float mouseY)
        {
            if ((mouseX >= _position.X && mouseY >= _position.Y) &&
                (mouseX <= (_position.X + _sizeX)) && mouseY <= (_position.Y + _sizeY))
                _overlay = true;
            else
                _overlay = false;
            return _overlay;
        }

        public Texture2D    Draw()
        {
            if (_overlay)
                return _overText;
            else
                return _texture;
        }
    }
}
