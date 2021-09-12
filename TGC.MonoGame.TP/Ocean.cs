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
        private int Width = 3000;
        private int Height = 3000;
        // Aca se puede cambiar cuantos vertices tiene la mesh (2x2 seria un quad)
        private int GridWidth = 128;
        private int GridHeight = 128;
        // Parametrizacion de las olas
        public float Amplitude = 20f;
        public float Speed = 100f;
        public float WaveLength = 500f;
        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.GraphicsDevice = graphics;
            this.Content = content;
        }
        public void Load()
        {
            var rasterizer = new RasterizerState();
            rasterizer.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizer;

            // Creo vertices en base al GridWidth y GridHeight
            VertexPosition[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(GraphicsDevice, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            ushort[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);

            // Load Shader
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "OceanShader");
        }
        public void Draw(Matrix view, Matrix proj, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalSeconds;
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            // Le paso el tiempo para simular las olas
            Effect.Parameters["Time"]?.SetValue(deltaTime);
            // Parametros de las olas
            Effect.Parameters["Amplitude"]?.SetValue(Amplitude);
            Effect.Parameters["Speed"]?.SetValue(Speed);
            Effect.Parameters["WaveLength"]?.SetValue(WaveLength);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = GridWidth * GridHeight * 2;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles);
            }
        }
        // Retorna una altura dada una posicion
        // Esta funcion debe mantenerse igual a la del shader para poder saber la altura de
        // una ola en un punto determinado
        public float CalculateWaveHeightFromPosition(Vector3 position, GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float k = 2.0f * (float)Math.PI / WaveLength;
            return Amplitude * (float)Math.Sin(k * (position.X - time * Speed));
        }
        private VertexPosition[] CalculateVertices()
        {
            var vertices = new VertexPosition[GridWidth * GridHeight];

            int vertIndex = 0;
            for (float y = 0; y < GridHeight; ++y)
            {
                for (float x = 0; x < GridWidth; ++x)
                {
                    var position = new Vector3(x / GridWidth * Width, 0, y / GridHeight * Height);
                    vertices[vertIndex++] = new VertexPosition(position);
                }
            }

            return vertices;
        }
        private ushort[] CalculateIndices()
        {
            var indices = new ushort[(GridHeight - 1) * (GridWidth - 1) * 6];

            int indicesIndex = 0;
            for (int y = 0; y < GridHeight - 1; ++y)
            {
                for (int x = 0; x < GridWidth - 1; ++x)
                {
                    int start = y * GridWidth + x;
                    indices[indicesIndex++] = (ushort)start;
                    indices[indicesIndex++] = (ushort)(start + 1);
                    indices[indicesIndex++] = (ushort)(start + GridWidth);
                    indices[indicesIndex++] = (ushort)(start + 1);
                    indices[indicesIndex++] = (ushort)(start + 1 + GridWidth);
                    indices[indicesIndex++] = (ushort)(start + GridWidth);
                }
            }

            return indices;
        }
    }
}
