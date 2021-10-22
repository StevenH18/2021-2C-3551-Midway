using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP
{
    public class IslandSystem
    {
        protected ContentManager Content;
        protected MapEnvironment Environment;
        private Island[] Islands;

        struct Island
        {
            public Model Model;
            public Effect Effect;
            public List<Texture2D> Textures;
            public Vector3 Position;
        }

        public IslandSystem(GraphicsDevice graphics, ContentManager content, MapEnvironment environment)
        {
            Content = content;
            Environment = environment;
        }
        public void Load()
        {
            Islands = new Island[Environment.IslandsPositions.Length];
            for(var i = 0; i < Environment.IslandsPositions.Length; i++)
            {
                try
                {
                    Islands[i].Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Environment/Island" + (i + 1) + "/Island" + (i + 1));
                    Islands[i].Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "IslandShader");
                    Islands[i].Textures = new List<Texture2D>();

                    Vector4 position = Environment.IslandsPositions[i];
                    Islands[i].Position = new Vector3(position.X, position.Y, position.Z);

                    foreach (var mesh in Islands[i].Model.Meshes)
                    {
                        foreach (var meshPart in mesh.MeshParts)
                        {
                            Islands[i].Textures.Add(meshPart.Effect.Parameters["Texture"].GetValueTexture2D());
                            meshPart.Effect = Islands[i].Effect;
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        public void Draw(Matrix view, Matrix proj)
        {
            foreach(Island island in Islands)
            {
                if (island.Model == null)
                    continue;

                island.Effect.Parameters["View"]?.SetValue(view);
                island.Effect.Parameters["Projection"]?.SetValue(proj);

                var textureIndex = 0;

                foreach (var mesh in island.Model.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var world = mesh.ParentBone.Transform * Matrix.CreateTranslation(island.Position);

                        island.Effect.Parameters["DiffuseMap"]?.SetValue(island.Textures[textureIndex]);
                        island.Effect.Parameters["World"]?.SetValue(world);

                        mesh.Draw();
                        textureIndex++;
                    }
                }
            }
        }
    }
}
