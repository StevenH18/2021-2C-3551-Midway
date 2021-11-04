using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Ships;
using System.Linq;

namespace TGC.MonoGame.TP
{
    public class Ocean
    {
        protected GraphicsDevice Graphics;
        protected ContentManager Content;
        protected Effect Effect;
        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;
        protected Texture2D Albedo;
        protected Texture2D Normal;
        protected Texture2D Noise;
        protected TextureCube SkyBox;
        protected MapEnvironment Environment;
        protected TrailPoint[] Trail;
        protected int MaxTrails = 100;
        protected int CurrentTrail = 0;
        protected float TrailFadeout;
        protected struct TrailPoint {
            public Vector4 Position;
            public float LifeTime;
        }

        public Ocean(GraphicsDevice graphics, ContentManager content, MapEnvironment environment)
        {
            Graphics = graphics;
            Content = content;
            Environment = environment;
        }
        public void Load()
        {
            // Se hace esto para que la densidad represente la cantidad de quads
            Environment.OceanQuads++;

            GenerateMesh();

            Albedo = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Ocean/ocean_albedo");
            Normal = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Ocean/ocean_normal");
            Noise = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Ocean/ocean_noise");
            SkyBox = Content.Load<TextureCube>(TGCGame.ContentFolderTextures + "SkyBoxes/StormySky");

            // Load Shader
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "OceanShader");

            Trail = new TrailPoint[MaxTrails];
        }
        public void Update(GameTime gameTime, Ship[] ships)
        {
            UpdateTrails(gameTime, ships);
        }
        public void Draw(Matrix view, Matrix proj, Matrix world, GameTime gameTime)
        {
            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            Graphics.Indices = IndexBuffer;
            Graphics.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            // Le paso el tiempo para simular las olas
            Effect.Parameters["Time"]?.SetValue(time);
            // Parametros de las olas
            Effect.Parameters["Gravity"].SetValue(Environment.Gravity);
            Effect.Parameters["WaveA"].SetValue(Environment.WaveA);
            Effect.Parameters["WaveB"].SetValue(Environment.WaveB);
            Effect.Parameters["WaveC"].SetValue(Environment.WaveC);
            // Islas
            Effect.Parameters["Islands"].SetValue(Environment.IslandsPositions);
            // Iluminacion
            Effect.Parameters["LightPositions"]?.SetValue(new Vector3[] {
                Environment.SunPosition
            });
            Effect.Parameters["LightColors"]?.SetValue(new Vector3[] {
                Environment.SunColor * Environment.SunIntensity
            });
            Effect.Parameters["EyePosition"]?.SetValue(world.Translation);
            // SkyBox
            Effect.Parameters["EnvironmentMap"]?.SetValue(SkyBox);
            // Textura
            Effect.Parameters["AlbedoTexture"]?.SetValue(Albedo);
            Effect.Parameters["NormalTexture"]?.SetValue(Normal);
            Effect.Parameters["NormalIntensity"]?.SetValue(1f);
            Effect.Parameters["NoiseTexture"]?.SetValue(Noise);
            Effect.Parameters["DepthTexture"]?.SetValue(Environment.OceanDepth);
            Effect.Parameters["DepthColorTexture"]?.SetValue(Environment.OceanDepthColor);
            // Ship foam
            Effect.Parameters["TrailPositions"]?.SetValue(Trail.Select(trail => trail.Position).ToArray());
            Effect.Parameters["TrailLifeTimes"]?.SetValue(Trail.Select(trail => trail.LifeTime).ToArray());
            Effect.Parameters["TrailFadeout"]?.SetValue(TrailFadeout);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = Environment.OceanQuads * Environment.OceanQuads * 2;
                Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles);
            }
        }
        void UpdateTrails(GameTime gameTime, Ship[] ships)
        {
            float seconds = (float)gameTime.TotalGameTime.TotalSeconds;
            float miliseconds = (float)gameTime.TotalGameTime.Milliseconds;
            float delay = 10;

            if(MathF.Abs(ships[0].speed) > 0)
                TrailFadeout = seconds + 1;

            if(miliseconds % delay == 0)
            {
                Vector3 trailPosition = ships[0].World.Translation + ships[0].Rotation.Forward * MathF.Sign(ships[0].speed) * 100;
                float trailSize = 50 * MathF.Abs(ships[0].speed / ships[0].Maxspeed);

                Trail[CurrentTrail].Position = new Vector4(trailPosition.X, trailPosition.Y, trailPosition.Z, trailSize);
                Trail[CurrentTrail].LifeTime = seconds;

                CurrentTrail++;
                if (CurrentTrail >= Trail.Length)
                    CurrentTrail = 0;
            }
        }
        float ClosenessToIsland(Vector3 position)
        {
            float previousDistance = 0;

            for (int i = 0; i < Environment.IslandsPositions.Length; i++)
            {
                Vector3 Island = new Vector3(Environment.IslandsPositions[i].X, Environment.IslandsPositions[i].Y, Environment.IslandsPositions[i].Z);
                previousDistance = MathF.Max(previousDistance, Math.Clamp(Environment.IslandsPositions[i].W / (position - Island).Length(), 0f, 1f));
            }

            return previousDistance;
        }
        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        /// <summary>
        /// Dada una posicion de un objeto
        /// Devuelve primero la normal y luego la posicion en la ola
        /// 
        /// Esta funcion debe mantenerse igual a la del shader
        /// </summary>
        Vector3 CalculateWave(Vector4 wave, Vector3 vertex, ref Vector3 tangent, ref Vector3 binormal, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Vector2 direction = new Vector2(wave.X, wave.Y);
            float steepness = wave.Z;
            float wavelength = wave.W;

            steepness = Lerp(steepness, steepness * 0.05f, ClosenessToIsland(vertex));

            Vector3 p = vertex;
            float k = 2.0f * MathF.PI / wavelength;
            Vector2 d = Vector2.Normalize(direction);
            float c = MathF.Sqrt(Environment.Gravity / k);
            float f = k * (Vector2.Dot(d, new Vector2(p.X, p.Z)) - time * c);
            float a = steepness / k;

            tangent += new Vector3(
                - d.X * d.X * (steepness * MathF.Sin(f)),
                d.X * (steepness * MathF.Cos(f)),
                -d.X * d.Y * (steepness * MathF.Sin(f))
            );
            binormal += new Vector3(
                -d.X * d.Y * (steepness * MathF.Sin(f)),
                d.Y * (steepness * MathF.Cos(f)),
                - d.Y * d.Y * (steepness * MathF.Sin(f))
            );

            return new Vector3(
                d.X * a * MathF.Cos(f),
                a * MathF.Sin(f),
                d.Y * a * MathF.Cos(f)
            );
        }
        public (Vector3, Vector3) WaveNormalPosition(Vector3 position, GameTime gameTime)
        {
            // Se calcula derivando p
            Vector3 tangent = new Vector3(1, 0, 0);
            Vector3 binormal = new Vector3(0, 0, 1);

            position += CalculateWave(Environment.WaveA, position, ref tangent, ref binormal, gameTime);
            position += CalculateWave(Environment.WaveB, position, ref tangent, ref binormal, gameTime);
            position += CalculateWave(Environment.WaveC, position, ref tangent, ref binormal, gameTime);

            Vector3 normal = Vector3.Normalize(Vector3.Cross(binormal, tangent));

            return (normal, position);
        }

        public void GenerateMesh()
        {
            // Creo vertices en base al GridWidth y GridHeight
            VertexPositionTexture[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(Graphics, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            uint[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(Graphics, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);
        }

        /// <summary>
        /// Crea una grilla de vertices que representa un plano para enviar al VertexBuffer
        /// </summary>
        private VertexPositionTexture[] CalculateVertices() // TODO cambiar vertexPosition a VertexPositionTexture
        {
            var vertices = new VertexPositionTexture[Environment.OceanQuads * Environment.OceanQuads];

            int vertIndex = 0;
            for (float y = 0; y < Environment.OceanQuads; ++y)
            {
                for (float x = 0; x < Environment.OceanQuads; ++x)
                {
                    var position = new Vector3(x / Environment.OceanQuads * Environment.OceanWidth - Environment.OceanWidth / 2, 0, y / Environment.OceanQuads * Environment.OceanHeight - Environment.OceanHeight / 2);
                    var uv = new Vector2(x / Environment.OceanQuads * Environment.OceanTiling, y / Environment.OceanQuads * Environment.OceanTiling);
                    vertices[vertIndex++] = new VertexPositionTexture(position, uv);
                }
            }

            return vertices;
        }
        /// <summary>
        /// Crea los indices de los quads (2 triangulos) para enviarlos al IndexBuffer
        /// </summary>
        private uint[] CalculateIndices()
        {
            var indices = new uint[(Environment.OceanQuads - 1) * (Environment.OceanQuads - 1) * 6];

            int indicesIndex = 0;
            for (int y = 0; y < Environment.OceanQuads - 1; ++y)
            {
                for (int x = 0; x < Environment.OceanQuads - 1; ++x)
                {
                    int start = y * Environment.OceanQuads + x;
                    indices[indicesIndex++] = (uint)start;
                    indices[indicesIndex++] = (uint)(start + 1);
                    indices[indicesIndex++] = (uint)(start + Environment.OceanQuads);
                    indices[indicesIndex++] = (uint)(start + 1);
                    indices[indicesIndex++] = (uint)(start + 1 + Environment.OceanQuads);
                    indices[indicesIndex++] = (uint)(start + Environment.OceanQuads);
                }
            }

            return indices;
        }
    }
}
