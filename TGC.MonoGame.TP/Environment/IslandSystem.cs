using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP
{
    public class IslandSystem
    {
        protected ContentManager Content;
        protected GraphicsDevice Graphics;
        protected MapEnvironment Environment;
        private Island[] Islands;

        struct Island
        {
            public Model Model;
            public Effect Effect;
            public Texture2D Diffuse;
            public Texture2D Normal;
            public Texture2D Roughness;
            public Texture2D Ao;
            public TextureCube SkyBox;
            public Vector3 Position;
        }

        public IslandSystem(GraphicsDevice graphics, ContentManager content, MapEnvironment environment)
        {
            Content = content;
            Graphics = graphics;
            Environment = environment;
        }

        public static bool Exists(string path)
        {
            return File.Exists($@"Content\{path}.xnb");
        }

        public void Load()
        {
            Islands = new Island[Environment.IslandsPositions.Length];

            for(var i = 0; i < Environment.IslandsPositions.Length; i++)
            {
                try
                {
                    string islandContent = "Environment/Island" + (i + 1) + "/Island" + (i + 1);
                    Islands[i].Model = Content.Load<Model>(TGCGame.ContentFolder3D + islandContent);

                    if(Exists(TGCGame.ContentFolder3D + islandContent + "_Color"))
                        Islands[i].Diffuse = Content.Load<Texture2D>(TGCGame.ContentFolder3D + islandContent + "_Color");

                    if (Exists(TGCGame.ContentFolder3D + islandContent + "_Normal"))
                        Islands[i].Normal = Content.Load<Texture2D>(TGCGame.ContentFolder3D + islandContent + "_Normal");

                    if (Exists(TGCGame.ContentFolder3D + islandContent + "_Color"))
                        Islands[i].Roughness = Content.Load<Texture2D>(TGCGame.ContentFolder3D + islandContent + "_Reflectance");

                    if (Exists(TGCGame.ContentFolder3D + islandContent + "_Alpha"))
                        Islands[i].Ao = Content.Load<Texture2D>(TGCGame.ContentFolder3D + islandContent + "_Alpha");

                    Islands[i].SkyBox = Content.Load<TextureCube>(TGCGame.ContentFolderTextures + "SkyBoxes/StormySky");
                    Islands[i].Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "IslandShader");

                    Vector4 position = Environment.IslandsPositions[i];
                    Islands[i].Position = new Vector3(position.X, position.Y, position.Z);

                    foreach (var mesh in Islands[i].Model.Meshes)
                    {
                        foreach (var meshPart in mesh.MeshParts)
                        {
                            meshPart.Effect = Islands[i].Effect;
                        }
                    }
                }
                catch (Exception e)
                {
                    Islands[i].Model = null;
                }
            }
        }
        public void DrawCameraDepth(Matrix view, Matrix proj, Matrix cameraWorld)
        {
            foreach (Island island in Islands)
            {
                if (island.Model == null)
                    continue;

                island.Effect.CurrentTechnique = island.Effect.Techniques["DepthPass"];
                island.Effect.Parameters["View"]?.SetValue(view);
                island.Effect.Parameters["Projection"]?.SetValue(proj);
                island.Effect.Parameters["ShoreWidth"]?.SetValue(Environment.ShoreWidth);
                island.Effect.Parameters["ShoreSmoothness"]?.SetValue(Environment.ShoreSmoothness);

                foreach (var mesh in island.Model.Meshes)
                {
                    var islandWorld = mesh.ParentBone.Transform * Matrix.CreateTranslation(island.Position);

                    island.Effect.Parameters["World"]?.SetValue(islandWorld);

                    mesh.Draw();
                }
            }
        }
        public void Draw(Matrix view, Matrix proj, Matrix cameraWorld)
        {
            foreach (Island island in Islands)
            {
                if (island.Model == null)
                    continue;

                island.Effect.CurrentTechnique = island.Effect.Techniques["BasicColorDrawing"];

                island.Effect.Parameters["View"]?.SetValue(view);
                island.Effect.Parameters["Projection"]?.SetValue(proj);
                island.Effect.Parameters["AlbedoTexture"]?.SetValue(island.Diffuse);
                island.Effect.Parameters["NormalTexture"]?.SetValue(island.Normal);
                island.Effect.Parameters["RoughnessTexture"]?.SetValue(island.Roughness);
                island.Effect.Parameters["AoTexture"]?.SetValue(island.Ao);
                island.Effect.Parameters["EnvironmentMap"]?.SetValue(island.SkyBox);

                island.Effect.Parameters["EyePosition"]?.SetValue(cameraWorld.Translation);

                // Iluminacion
                island.Effect.Parameters["LightPositions"]?.SetValue(new Vector3[] {
                    Environment.SunPosition
                });
                island.Effect.Parameters["LightColors"]?.SetValue(new Vector3[] {
                    Environment.SunColor * Environment.SunIntensity
                });

                foreach (var mesh in island.Model.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        var islandWorld = mesh.ParentBone.Transform * Matrix.CreateTranslation(island.Position);

                        island.Effect.Parameters["World"]?.SetValue(islandWorld);
                        island.Effect.Parameters["InverseTransposeMatrix"]?.SetValue(Matrix.Transpose(Matrix.Invert(islandWorld)));

                        mesh.Draw();
                    }
                }
            }
        }
    }
}
