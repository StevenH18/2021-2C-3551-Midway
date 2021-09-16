using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace TGC.MonoGame.TP
{
    public class Ocean
    {
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;
        protected Effect Effect;
        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;
        // Aca se puede cambiar el tamaño de la mesh
        private int Width = 10000;
        private int Height = 10000;
        // Aca se puede cambiar que tan densa es la mesh (Density = 8 => 8x8 quads)
        private int Density = 128;
        // Direccion del oleaje
        public Vector2 Direction = new Vector2(6f, 0f);
        // Gravedad de las olas (afecta la velocidad)
        public float Gravity = 9.8f;
        // Inclinacion de las olas (Debe estar entre 0.0f y 1.0f)
        public float Steepness = 0.1f;
        // Separacion entre olas
        public float WaveLength = 2000f;
        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.GraphicsDevice = graphics;
            this.Content = content;
        }
        public void Load()
        {
            // Se hace esto para que la densidad represente la cantidad de quads
            Density++;

            var rasterizer = new RasterizerState();
            rasterizer.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizer;

            // Creo vertices en base al GridWidth y GridHeight
            VertexPosition[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(GraphicsDevice, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            uint[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);

            // Load Shader
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "OceanShader");
        }
        public void Draw(Matrix view, Matrix proj, GameTime gameTime)
        {
            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            // Le paso el tiempo para simular las olas
            Effect.Parameters["Time"]?.SetValue(time);
            // Parametros de las olas
            Effect.Parameters["Direction"]?.SetValue(Direction);
            Effect.Parameters["Gravity"]?.SetValue(Gravity);
            Effect.Parameters["Steepness"]?.SetValue(Steepness);
            Effect.Parameters["WaveLength"]?.SetValue(WaveLength);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = Density * Density * 2;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles);
            }
        }
        /// <summary>
        /// Dada una posicion de un objeto
        /// Devuelve primero la normal y luego la posicion en la ola
        /// 
        /// Esta funcion debe mantenerse igual a la del shader
        /// </summary>
        public (Vector3, Vector3) WaveNormalPosition(Vector3 position, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Codigo casi igual al shader
            Vector3 p = position;
            float k = 2.0f * MathF.PI / WaveLength;
            Vector2 d = Vector2.Normalize(Direction);
            float c = MathF.Sqrt(Gravity / k);
            float f = k * (Vector2.Dot(d, new Vector2(p.X, p.Z)) - time * c);
            float a = Steepness / k;
            p.X += d.X * a * MathF.Cos(f);
            p.Y = a * MathF.Sin(f);
            p.Z += d.Y * a * MathF.Cos(f);

            // Se calcula derivando p
            Vector3 tangent = new Vector3(
                1 - d.X * d.X * (Steepness * MathF.Sin(f)),
                d.X * (Steepness * MathF.Cos(f)),
                -d.X * d.Y * (Steepness * MathF.Sin(f))
            );
            Vector3 binormal = new Vector3(
                -d.X * d.Y * (Steepness * MathF.Sin(f)),
                d.Y * (Steepness * MathF.Cos(f)),
                1 - d.Y * d.Y * (Steepness * MathF.Sin(f))
            );

            Vector3 normal = Vector3.Normalize(Vector3.Cross(binormal, tangent));
            position = p;

            return (normal, position);
        }
        /// <summary>
        /// Crea una grilla de vertices que representa un plano para enviar al VertexBuffer
        /// </summary>
        private VertexPosition[] CalculateVertices()
        {
            var vertices = new VertexPosition[Density * Density];

            int vertIndex = 0;
            for (float y = 0; y < Density; ++y)
            {
                for (float x = 0; x < Density; ++x)
                {
                    var position = new Vector3(x / Density * Width, 0, y / Density * Height);
                    vertices[vertIndex++] = new VertexPosition(position);
                }
            }

            return vertices;
        }
        /// <summary>
        /// Crea los indices de los quads (2 triangulos) para enviarlos al IndexBuffer
        /// </summary>
        private uint[] CalculateIndices()
        {
            var indices = new uint[(Density - 1) * (Density - 1) * 6];

            int indicesIndex = 0;
            for (int y = 0; y < Density - 1; ++y)
            {
                for (int x = 0; x < Density - 1; ++x)
                {
                    int start = y * Density + x;
                    indices[indicesIndex++] = (uint)start;
                    indices[indicesIndex++] = (uint)(start + 1);
                    indices[indicesIndex++] = (uint)(start + Density);
                    indices[indicesIndex++] = (uint)(start + 1);
                    indices[indicesIndex++] = (uint)(start + 1 + Density);
                    indices[indicesIndex++] = (uint)(start + Density);
                }
            }

            return indices;
        }
    }
}
