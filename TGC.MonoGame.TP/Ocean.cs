using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace TGC.MonoGame.TP
{
    public class Ocean
    {
        protected GraphicsDevice Graphics;
        protected ContentManager Content;
        protected Effect Effect;
        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;
        private int GridWidth = 64;
        private int GridHeigt = 64;
        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.Graphics = graphics;
            this.Content = content;
        }
        public void Load()
        {
            // Load Vertices
            VertexPosition[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(Graphics, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            ushort[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(Graphics, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);

            // Load Shader
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "OceanShader");
        }
        public void Draw(Matrix view, Matrix proj, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalSeconds;
            Graphics.Indices = IndexBuffer;
            Graphics.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            Effect.Parameters["Time"].SetValue(deltaTime);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                var triangles = GridWidth * GridHeigt * 2;
                Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles);
            }
        }
        private VertexPosition[] CalculateVertices()
        {
            var vertices = new VertexPosition[GridWidth * GridHeigt];

            int vertIndex = 0;
            for (int y = 0; y < GridHeigt; ++y)
            {
                for (int x = 0; x < GridWidth; ++x)
                {
                    var position = new Vector3(x, 0, y);
                    vertices[vertIndex++] = new VertexPosition(position);
                }
            }

            return vertices;
        }
        private ushort[] CalculateIndices()
        {
            var indices = new ushort[(GridHeigt - 1) * (GridWidth - 1) * 6];

            int indicesIndex = 0;
            for (int y = 0; y < GridHeigt - 1; ++y)
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
