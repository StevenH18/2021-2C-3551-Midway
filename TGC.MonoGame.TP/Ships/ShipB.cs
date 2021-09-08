﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipB : Ship
    {

        public ShipB(ContentManager content) : base(content)
        {
            Scale = Matrix.CreateScale(0.15f);
            Rotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(((float)Math.PI) / 2) * Matrix.CreateRotationZ(0);
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }

        public new void Load()
        {
            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Ships/ShipB/ShipB");
            base.Load();
        }

    }
}
