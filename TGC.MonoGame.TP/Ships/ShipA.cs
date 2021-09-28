using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP
{
    public class ShipA : Ship
    {
        public ShipA(ContentManager content,Ocean ocean, Color color) : base(content, ocean, color)
        {
            Scale = Matrix.CreateScale(0.015f);
            Rotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(0) * Matrix.CreateRotationZ(0);
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }

        public override void Load()
        {
            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Ships/ShipA/Ship");
            base.Load();

        }


    }


}
