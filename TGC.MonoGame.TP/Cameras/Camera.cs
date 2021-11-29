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

        public Matrix WorldTranslation = Matrix.Identity;
        public Matrix WorldRotation = Matrix.Identity;
        public Matrix ViewTranslation = Matrix.Identity;
        public Matrix ViewRotation = Matrix.Identity;

        public virtual void Update(GameTime gameTime, Ship ship, TGCGame game)
        {

        }
    }
}
