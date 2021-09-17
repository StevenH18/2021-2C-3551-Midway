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
        public int Width = 10000;
        public int Height = 10000;
        // Aca se puede cambiar que tan densa es la mesh (Density = 8 => 8x8 quads)
        private int Density = 128;
        // Gravedad de las olas (afecta la velocidad)
        public float Gravity = 9.8f;

        // Para organizar mejor multiples olas enviamos todos los parametros de una
        // ola en un Vector4(DirX, DirY, Steepness, WaveLength)
        // asi es mas facil enviarlo al shader
        public Vector4 WaveA = new Vector4(1f, 0.3f, 0.3f, 6000f);
        public Vector4 WaveB = new Vector4(1f, -0.2f, 0.5f, 3000f);
        public Vector4 WaveC = new Vector4(1f, 0f, 0.1f, 1000f);

        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.GraphicsDevice = graphics;
            this.Content = content;
        }
        public void Load()
        {
            // Se hace esto para que la densidad represente la cantidad de quads
            Density++;

            GenerateMesh();
            var rasterizer = new RasterizerState();
            rasterizer.FillMode = FillMode.WireFrame;
            rasterizer.CullMode = CullMode.None;
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
            Effect.Parameters["Gravity"]?.SetValue(Gravity);
            Effect.Parameters["WaveA"]?.SetValue(WaveA);
            Effect.Parameters["WaveB"]?.SetValue(WaveB);
            Effect.Parameters["WaveC"]?.SetValue(WaveC);

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
        Vector3 CalculateWave(Vector4 wave, Vector3 vertex, ref Vector3 tangent, ref Vector3 binormal, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Vector2 direction = new Vector2(wave.X, wave.Y);
            float steepness = wave.Z;
            float wavelength = wave.W;

            Vector3 p = vertex;
            float k = 2.0f * MathF.PI / wavelength;
            Vector2 d = Vector2.Normalize(direction);
            float c = MathF.Sqrt(Gravity / k);
            float f = k * (Vector2.Dot(d, new Vector2(p.X, p.Z)) - time * c);
            float a = steepness / k;

            // Calculamos la normal para poder usarla en un futuro con iluminacion

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

            position += CalculateWave(WaveA, position, ref tangent, ref binormal, gameTime);
            position += CalculateWave(WaveB, position, ref tangent, ref binormal, gameTime);
            position += CalculateWave(WaveC, position, ref tangent, ref binormal, gameTime);

            Vector3 normal = Vector3.Normalize(Vector3.Cross(binormal, tangent));

            return (normal, position);
        }

        public void GenerateMesh()
        {
            // Creo vertices en base al GridWidth y GridHeight
            VertexPosition[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(GraphicsDevice, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            uint[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);
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
