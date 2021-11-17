using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP
{
    public class Camera
    {
        public Matrix World = Matrix.Identity;
        public Matrix View = Matrix.Identity;
        public Matrix Projection = Matrix.Identity;

        public virtual void Update(GameTime gameTime, Ship ship)
        {

        }
    }
}
