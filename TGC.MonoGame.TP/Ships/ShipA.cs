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

            foreach (var mesh in Model.Meshes)
            {
                Effect basicEffect = mesh.Effects[0];
                if (basicEffect.Parameters["Texture"] != null)
                {
                    Ship.TexturesA.Albedos.Add(basicEffect.Parameters["Texture"].GetValueTexture2D());
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
                List<Texture2D> albedos = Ship.TexturesA.Albedos;

                if (textureIndex <= Ship.TexturesA.Albedos.Count)
                {
                    Effect.Parameters["DiffuseMap"].SetValue(Ship.TexturesA.Albedos[textureIndex]);
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
