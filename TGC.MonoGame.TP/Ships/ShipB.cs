using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipB : Ship
    {
        public ShipB(ContentManager content, GraphicsDevice graphics, Gizmos gizmos) : base(content, graphics, gizmos)
        {
            Scale = Matrix.CreateScale(0.2f);
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
        public override void Draw(Matrix view, Matrix proj, Matrix cameraWorld, RenderState renderState, MapEnvironment environment)
        {
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            Effect.Parameters["DiffuseColor"]?.SetValue(Color.Black.ToVector3());

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
                List<Texture2D> albedos = Ship.TexturesB.Albedos;

                if (textureIndex <= Ship.TexturesB.Albedos.Count)
                {
                    Effect.Parameters["AlbedoTexture"]?.SetValue(Ship.TexturesB.Albedos[textureIndex]);
                }
                textureIndex++;

                Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(World)));

                Effect.Parameters["TexturedNormals"].SetValue(false);

                Effect.Parameters["AmbientColor"]?.SetValue(new Vector3(0.2f, 0.2f, 0.2f));
                Effect.Parameters["DiffuseColor"]?.SetValue(environment.SunColor);
                Effect.Parameters["SpecularColor"]?.SetValue(environment.SunColor);

                Effect.Parameters["KAmbient"]?.SetValue(0.2f);
                Effect.Parameters["KDiffuse"]?.SetValue(0.2f);
                Effect.Parameters["KSpecular"]?.SetValue(0f);

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
