using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP
{
    public class IslandSystem
    {
        protected ContentManager Content;
        protected GraphicsDevice Graphics;
        protected MapEnvironment Environment;
        protected Texture2D Noise;
        private Island[] Islands;

        public List<BoundingSphere> IslandColliders;
        public List<BoundingSphere> CollidersDebug;
        private int CurrentCollider;
        private float Size = 200f;
        private Gizmos Gizmos;

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

        public IslandSystem(GraphicsDevice graphics, ContentManager content, MapEnvironment environment, Gizmos gizmos)
        {
            Content = content;
            Graphics = graphics;
            Environment = environment;
            Gizmos = gizmos;

            IslandColliders = new List<BoundingSphere>();
            CollidersDebug = new List<BoundingSphere>();

            // First island
            IslandColliders.Add(new BoundingSphere(new Vector3(4633.1313f, 0f, 2269.684f), 591.6674f));
            IslandColliders.Add(new BoundingSphere(new Vector3(3817.7393f, 0f, 2072.0244f), 658.3344f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2977.9517f, 0f, 2141.2925f), 408.3336f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5159.9854f, 0f, 2248.9004f), 300.00012f));
            IslandColliders.Add(new BoundingSphere(new Vector3(4328.7305f, 0f, 1771.5061f), 300.00012f));
            IslandColliders.Add(new BoundingSphere(new Vector3(3238.6533f, 0f, 1860.2358f), 300.00012f));
            IslandColliders.Add(new BoundingSphere(new Vector3(3321.6719f, 0f, 2295.138f), 300.00012f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2666.6953f, 0f, 2473.0334f), 158.33328f));
            IslandColliders.Add(new BoundingSphere(new Vector3(4197.5786f, 0f, 2583.9937f), 266.66675f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2163.7515f, 0f, 4568.728f), 658.3344f));
            IslandColliders.Add(new BoundingSphere(new Vector3(3030.4185f, 0f, 5601.606f), 791.6684f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2773.0134f, 0f, 4925.0264f), 591.6674f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2494.562f, 0f, 6018.6104f), 358.33353f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2342.888f, 0f, 5360.9004f), 358.33353f));
            IslandColliders.Add(new BoundingSphere(new Vector3(3273.2131f, 0f, 5004.0034f), 358.33353f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2096.3186f, 0f, 3753.492f), 191.66666f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5538.5244f, 0f, 4361.1562f), 533.3338f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5562.518f, 0f, 4014.6543f), 458.33365f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5868.376f, 0f, 4678.7188f), 366.66687f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5929.5254f, 0f, 3713.6982f), 83.333145f));

            // Second island
            IslandColliders.Add(new BoundingSphere(new Vector3(8192.272f, 0f, -1657.0726f), 791.6684f));
            IslandColliders.Add(new BoundingSphere(new Vector3(8769.7705f, 0f, 951.01733f), 750.00146f));
            IslandColliders.Add(new BoundingSphere(new Vector3(9754.327f, 0f, 341.73407f), 1075.003f));
            IslandColliders.Add(new BoundingSphere(new Vector3(9868.172f, 0f, -921.95905f), 1075.003f));
            IslandColliders.Add(new BoundingSphere(new Vector3(9119.634f, 0f, -1842.2719f), 625.00085f));
            IslandColliders.Add(new BoundingSphere(new Vector3(8650.573f, 0f, -1011.5747f), 333.33344f));
            IslandColliders.Add(new BoundingSphere(new Vector3(10677.303f, 0f, -115.393845f), 333.33344f));
            IslandColliders.Add(new BoundingSphere(new Vector3(7847.114f, 0f, -1800.5543f), 616.6675f));

            // Third island
            IslandColliders.Add(new BoundingSphere(new Vector3(-335.09457f, 0f, -6017.0073f), 149.99994f));
            IslandColliders.Add(new BoundingSphere(new Vector3(319.57053f, 0f, -4642.47f), 149.99994f));
            IslandColliders.Add(new BoundingSphere(new Vector3(930.2899f, 0f, -7107.311f), 266.66675f));
            IslandColliders.Add(new BoundingSphere(new Vector3(1381.2517f, 0f, -6792.485f), 425.00027f));
            IslandColliders.Add(new BoundingSphere(new Vector3(1825.1113f, 0f, -6301.6055f), 383.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2144.1687f, 0f, -5817.176f), 333.3335f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2436.3418f, 0f, -5153.123f), 416.66693f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2558.826f, 0f, -3675.0264f), 816.6685f));
            IslandColliders.Add(new BoundingSphere(new Vector3(2466.3035f, 0f, -4617.3486f), 533.3338f));
            IslandColliders.Add(new BoundingSphere(new Vector3(3594.0706f, 0f, -3836.5881f), 575.0007f));
            IslandColliders.Add(new BoundingSphere(new Vector3(4610.0527f, 0f, -4025.47f), 575.0007f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5367.2773f, 0f, -4617.8955f), 575.0007f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5674.1265f, 0f, -5341.181f), 550.00055f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5914.4805f, 0f, -6195.362f), 383.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5847.887f, 0f, -5830.3374f), 383.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5729.1523f, 0f, -4962.2256f), 383.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(5161.835f, 0f, -4105.3037f), 383.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(4397.0645f, 0f, -3676.681f), 383.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2705.6868f, 0f, -3885.9326f), 766.6683f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-3970.5942f, 0f, -3830.979f), 541.6671f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4953.167f, 0f, -3815.8865f), 541.6671f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5482.7266f, 0f, -4532.924f), 541.6671f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5490.4126f, 0f, -5490.091f), 616.6675f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4837.403f, 0f, -6041.8267f), 466.66693f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4478.373f, 0f, -6591.126f), 358.33347f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4502.493f, 0f, -7131.3975f), 300.00006f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4462.1665f, 0f, -7659.9805f), 300.00006f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4318.509f, 0f, -8041.522f), 183.33325f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-6177.84f, 0f, -5274.0645f), 450.00024f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-6705.281f, 0f, -4986.2734f), 450.00024f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-8039.219f, 0f, -4967.3325f), 450.00024f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7402.808f, 0f, -4918.0425f), 458.3336f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7973.934f, 0f, -4357.603f), 458.3336f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7937.442f, 0f, -3684.9375f), 458.3336f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7955.646f, 0f, -3242.0227f), 458.3336f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7052.696f, 0f, -3308.2212f), 608.3341f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-6392.3853f, 0f, -3107.031f), 608.3341f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5681.511f, 0f, -2733.7485f), 608.3341f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5149.7344f, 0f, -2365.8079f), 608.3341f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4561.273f, 0f, -1927.8062f), 533.33374f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4114.653f, 0f, -1238.4777f), 533.33374f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4713.9707f, 0f, -554.34814f), 533.33374f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5383.791f, 0f, -345.15997f), 533.33374f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7232.9717f, 0f, -41.691254f), 433.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-6273.8496f, 0f, 77.313446f), 575.0006f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7703.43f, 0f, -41.86061f), 199.99994f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5704.466f, 0f, 497.82867f), 433.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5686.8687f, 0f, 1129.1057f), 433.33356f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5717.8047f, 0f, 1767.4926f), 300.00006f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5862.3564f, 0f, 2313.7534f), 441.6669f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-6535.6016f, 0f, 2880.53f), 583.334f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7080.9355f, 0f, 4228.604f), 966.6692f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-7047.364f, 0f, 3114.8262f), 233.33331f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-6155.651f, 0f, 4648.2793f), 583.334f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-5148.8535f, 0f, 4580.7007f), 583.334f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-4019.2214f, 0f, 4625.468f), 583.334f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2936.1265f, 0f, 5717.549f), 383.3335f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-3235.3457f, 0f, 4978.0303f), 625.00085f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2757.2712f, 0f, 4145.827f), 625.00085f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1313.1068f, 0f, 3282.318f), 416.66687f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2152.2166f, 0f, 3385.8245f), 558.33386f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1993.7468f, 0f, 2772.6296f), 558.33386f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2720.1929f, 0f, 2236.0073f), 558.33386f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1982.6775f, 0f, 1384.1527f), 383.3335f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2498.5107f, 0f, 1621.2046f), 383.3335f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2150.9778f, 0f, 673.9155f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1929.5884f, 0f, 280.5583f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1856.0371f, 0f, -258.08722f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1779.2905f, 0f, -812.08484f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-1995.2971f, 0f, -1336.9868f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2152.3467f, 0f, -1668.1285f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2689.7488f, 0f, -2392.1646f), 691.66785f));
            IslandColliders.Add(new BoundingSphere(new Vector3(-2547.0122f, 0f, -3073.3884f), 433.33356f));

            CollidersDebug.Add(new BoundingSphere(Vector3.Zero, 200f));
        }

        public static bool Exists(string path)
        {
            return File.Exists($@"Content\{path}.xnb");
        }

        public void Load()
        {
            for(var i = 0; i < IslandColliders.Count; i++)
            {
                IslandColliders[i] = new BoundingSphere(IslandColliders[i].Center, IslandColliders[i].Radius * 0.9f);
            }

            Noise = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Ocean/ocean_noise");
            Islands = new Island[Environment.IslandsPositions.Length];

            for(var i = 0; i < Environment.IslandsPositions.Length; i++)
            {
                /*
                if(i == 1)
                {
                    Islands[i].Model = null;
                    continue;
                }
                */

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
        public void Update(GameTime gameTime, Camera camera)
        {
            /*
            float miliseconds = (float)gameTime.TotalGameTime.Milliseconds;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 colliderPosition = camera.World.Translation + camera.World.Forward * 1000;
            colliderPosition.Y = 0;

            CollidersDebug[CurrentCollider] = new BoundingSphere(colliderPosition, Size);

            if(Inputs.isJustPressed(Keys.Tab))
            {
                CollidersDebug.Add(new BoundingSphere(colliderPosition, Size));
                CurrentCollider++;
            }
            if(Keyboard.GetState().IsKeyDown(Keys.Add))
            {
                Size += deltaTime * 500;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Subtract))
            {
                Size -= deltaTime * 500;
            }

            if (miliseconds % 50000 == 0)
            {
                Debug.WriteLine("--------------------------------");

                foreach (BoundingSphere collider in CollidersDebug)
                {
                    string x = collider.Center.X.ToString().Replace(",", ".");
                    string y = collider.Center.Y.ToString().Replace(",", ".");
                    string z = collider.Center.Z.ToString().Replace(",", ".");
                    string radius = collider.Radius.ToString().Replace(",", ".");

                    Debug.WriteLine("IslandColliders.Add(new BoundingSphere(new Vector3("+x+"f, "+y+"f, "+z+"f), "+radius+ "f));");
                }

                Debug.WriteLine("--------------------------------");
            }
            */
        }
        public void DrawCameraDepth(Matrix view, Matrix proj, Matrix cameraWorld, GameTime gameTime)
        {
            var time = (float)gameTime.TotalGameTime.TotalSeconds;

            foreach (Island island in Islands)
            {
                if (island.Model == null)
                    continue;

                island.Effect.CurrentTechnique = island.Effect.Techniques["DepthPass"];
                island.Effect.Parameters["View"]?.SetValue(view);
                island.Effect.Parameters["Projection"]?.SetValue(proj);
                island.Effect.Parameters["ShoreWidth"]?.SetValue(Environment.ShoreWidth);
                island.Effect.Parameters["ShoreSmoothness"]?.SetValue(Environment.ShoreSmoothness);
                island.Effect.Parameters["NoiseTexture"]?.SetValue(Noise);
                island.Effect.Parameters["Time"]?.SetValue(time);

                foreach (var mesh in island.Model.Meshes)
                {
                    var islandWorld = mesh.ParentBone.Transform * Matrix.CreateTranslation(island.Position);

                    island.Effect.Parameters["World"]?.SetValue(islandWorld);

                    mesh.Draw();
                }
            }
        }
        public void Draw(Matrix view, Matrix proj, Matrix cameraWorld, RenderState renderState)
        {
            foreach (Island island in Islands)
            {
                if (island.Model == null)
                    continue;

                switch (renderState)
                {
                    case RenderState.Default:
                        island.Effect.CurrentTechnique = island.Effect.Techniques["BasicColorDrawing"];
                        break;
                    case RenderState.HeightMap:
                        island.Effect.CurrentTechnique = island.Effect.Techniques["HeightMap"];
                        break;
                }

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


            foreach(BoundingSphere collider in IslandColliders)
            {
                Gizmos.DrawSphere(collider.Center, Vector3.One * collider.Radius, Color.Yellow);
            }
            /*
            foreach(BoundingSphere collider in CollidersDebug)
            {
                Gizmos.DrawSphere(collider.Center, Vector3.One * collider.Radius, Color.Purple);
            }
            */
        }
    }
}
