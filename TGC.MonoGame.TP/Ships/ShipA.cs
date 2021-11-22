using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP
{
    public class ShipA : Ship
    {
        public ShipA(ContentManager content, GraphicsDevice graphics, Gizmos gizmos) : base(content, graphics, gizmos)
        {
            Scale = Matrix.CreateScale(0.015f);
            Rotation = Matrix.CreateRotationX(0) * Matrix.CreateRotationY(0) * Matrix.CreateRotationZ(0);
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }
        public static bool Exists(string path)
        {
            return File.Exists($@"Content\{path}.xnb");
        }
        public override void Load()
        {
            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Ships/ShipA/ShipTest");

            foreach (var mesh in Model.Meshes)
            {
                Effect basicEffect = mesh.Effects[0];
                if (basicEffect.Parameters["Texture"] != null)
                {
                    Texture2D albedo = basicEffect.Parameters["Texture"].GetValueTexture2D();
                    Texture2D metallic = null;
                    Texture2D normal = null;
                    Texture2D roughness = null;

                    if (albedo == null)
                        continue;

                    String albedoName = albedo.Name.Replace("Models\\Ships\\ShipA\\textures\\", "");
                    albedoName = albedoName.Replace("BaseColor_0", "BaseColor");

                    String metallicName = albedoName.Replace("BaseColor", "Metallic");
                    String metallicFile = TGCGame.ContentFolder3D + "Ships/ShipA/textures/" + metallicName;

                    String normalName = albedoName.Replace("BaseColor", "Normal");
                    String normalFile = TGCGame.ContentFolder3D + "Ships/ShipA/textures/" + normalName;

                    String roughnessName = albedoName.Replace("BaseColor", "Roughness");
                    String roughnessFile = TGCGame.ContentFolder3D + "Ships/ShipA/textures/" + roughnessName;


                    if (Exists(metallicFile))
                    {
                        metallic = Content.Load<Texture2D>(metallicFile);
                    }
                    if (Exists(normalFile))
                    {
                        normal = Content.Load<Texture2D>(normalFile);
                    }
                    if (Exists(roughnessFile))
                    {
                        roughness = Content.Load<Texture2D>(roughnessFile);
                    }

                    Ship.TexturesA.Albedos.Add(albedo);
                    Ship.TexturesA.Metallics.Add(metallic);
                    Ship.TexturesA.Normals.Add(normal);
                    Ship.TexturesA.Roughness.Add(roughness);

                }
                else
                {
                    Console.WriteLine("No se pudo cargar ninguna textura para este mesh del barco");
                }
            }

            base.Load();
        }
        public override void Draw(Matrix view, Matrix proj, Matrix cameraWorld, RenderState renderState, MapEnvironment environment)
        {
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            Effect.Parameters["DiffuseColor"]?.SetValue(Color.Gray.ToVector3());

            switch (renderState)
            {
                case RenderState.Default:
                    Effect.CurrentTechnique = Effect.Techniques["BasicColorDrawing"];
                    break;
                case RenderState.HeightMap:
                    Effect.CurrentTechnique = Effect.Techniques["HeightMap"];
                    break;
            }

            var textureIndex = 0;
            foreach (var mesh in Model.Meshes)
            {
                List<Texture2D> albedos = Ship.TexturesA.Albedos;
                List<Texture2D> metallics = Ship.TexturesA.Metallics;
                List<Texture2D> normals = Ship.TexturesA.Normals;
                List<Texture2D> roughness = Ship.TexturesA.Roughness;

                if (textureIndex < Ship.TexturesA.Albedos.Count)
                {
                    Effect.Parameters["AlbedoTexture"]?.SetValue(Ship.TexturesA.Albedos[textureIndex]);
                    Effect.Parameters["MetallicTexture"]?.SetValue(Ship.TexturesA.Metallics[textureIndex]);
                    Effect.Parameters["NormalTexture"]?.SetValue(Ship.TexturesA.Normals[textureIndex]);
                    Effect.Parameters["RoughnessTexture"]?.SetValue(Ship.TexturesA.Roughness[textureIndex]);
                }
                textureIndex++;

                Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));

                Effect.Parameters["TexturedNormals"].SetValue(true);

                Effect.Parameters["AmbientColor"]?.SetValue(new Vector3(1f, 1f, 1f));
                Effect.Parameters["DiffuseColor"]?.SetValue(new Vector3(1f, 1f, 1f));
                Effect.Parameters["SpecularColor"]?.SetValue(environment.SunColor);

                Effect.Parameters["KAmbient"]?.SetValue(0.3f);
                Effect.Parameters["KDiffuse"]?.SetValue(0.3f);
                Effect.Parameters["KSpecular"]?.SetValue(0.2f);

                Effect.Parameters["Shininess"]?.SetValue(128f);
                Effect.Parameters["EyePosition"]?.SetValue(cameraWorld.Translation);

                // Iluminacion
                Effect.Parameters["LightPosition"]?.SetValue(environment.SunPosition);

                var w = mesh.ParentBone.Transform * World;
                Effect.Parameters["World"].SetValue(w);
                mesh.Draw();
            }

            base.Draw(view, proj, cameraWorld, renderState, environment);
        }


    }


}
