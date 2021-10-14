using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP
{
    public class Islands
    {
        protected ContentManager Content;
        protected Model Model { get; set; }
        protected Effect Effect { get; set; }
        protected Matrix Scale;
        public Matrix Rotation;
        public Vector3 Position = new Vector3(-6000f, 0f, -6000f);
        protected Matrix World { get; set; }

        public Islands(GraphicsDevice graphics, ContentManager content)
        {
            Content = content;
            Scale = Matrix.CreateScale(1);
            Rotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(0) * Matrix.CreateRotationZ(0);
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }
        public void Load()
        {
            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Environment/Island");
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
        public void Draw(Matrix view, Matrix proj)
        {
            Effect.Parameters["View"]?.SetValue(view);
            Effect.Parameters["Projection"]?.SetValue(proj);
            Effect.Parameters["DiffuseColor"]?.SetValue(new Vector3(0.167f, 0.409f, 0.219f));

            foreach (var mesh in Model.Meshes)
            {
                var w = mesh.ParentBone.Transform * World;

                Effect.Parameters["World"]?.SetValue(w);

                mesh.Draw();
            }

        }
    }
}
