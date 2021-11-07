using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP
{
    public abstract class Camera
    {
        public Matrix World = Matrix.Identity;
        public Matrix View = Matrix.Identity;
        public Matrix Projection = Matrix.Identity;

        public abstract void Update(GameTime gameTime, Ship ship);
    }
}
