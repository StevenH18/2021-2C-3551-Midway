using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Ships
{
    public abstract class Ship
    {
        protected ContentManager Content;
        protected Matrix World { get; set; }
        protected Model Model { get; set; }
        protected Effect Effect { get; set; }
        protected Matrix Scale;
        public Matrix Rotation;
        public Vector3 Position;

        public Ship(ContentManager content)
        {
            this.Content = content;
        }

        public void Load()
        {
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }
        }
        public void Update(GameTime gameTime)
        {
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }

        public void Draw(Matrix view, Matrix proj, Color color)
        {

            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            Effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());

            foreach (var mesh in Model.Meshes)
            {

                var w = mesh.ParentBone.Transform * World;

                Effect.Parameters["World"].SetValue(w);

                mesh.Draw();
            }

        }

    }
}
