using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipB : Ship
    {
        public ShipB(ContentManager content, Ocean ocean, Color color) : base(content, ocean, color)
        {
            Scale = Matrix.CreateScale(0.15f);
            Rotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(0) * Matrix.CreateRotationZ(0);
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }
        public override void Load()
        {
            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Ships/ShipB/ShipB");

            foreach (var mesh in Model.Meshes)
            {
                Effect basicEffect = mesh.Effects[0];
                if (basicEffect.Parameters["Texture"] != null)
                {
                    Ship.TexturesB.Albedos.Add(basicEffect.Parameters["Texture"].GetValueTexture2D());
                }
                else
                {
                    Console.WriteLine("No se pudo cargar ninguna textura para este mesh del barco");
                }
            }

            base.Load();
        }
        public override void Draw(Matrix view, Matrix proj)
        {
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            Effect.Parameters["DiffuseColor"].SetValue(Color.ToVector3());

            var textureIndex = 0;
            foreach (var mesh in Model.Meshes)
            {
                List<Texture2D> albedos = Ship.TexturesB.Albedos;

                if (textureIndex <= Ship.TexturesB.Albedos.Count)
                {
                    Effect.Parameters["DiffuseMap"].SetValue(Ship.TexturesB.Albedos[textureIndex]);
                }
                textureIndex++;

                var w = mesh.ParentBone.Transform * World;
                Effect.Parameters["World"].SetValue(w);
                mesh.Draw();
            }

            base.Draw(view, proj);
        }

    }
}
