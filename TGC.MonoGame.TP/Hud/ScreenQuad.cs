using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Hud
{
    class ScreenQuad
    {
        private GraphicsDevice Graphics;
        private IndexBuffer IndexBuffer;
        private VertexBuffer VertexBuffer;

        public ScreenQuad(GraphicsDevice graphics, Vector3 position, Vector3 size)
        {
            Graphics = graphics;
            CreateVertexBuffer(position, size);
            CreateIndexBuffer();
        }
        private void CreateVertexBuffer(Vector3 position, Vector3 size)
        {
            Vector3 BL = new Vector3(-1, -1, 0);
            Vector3 TL = new Vector3(-1, 1, 0);
            Vector3 BR = new Vector3(1, -1, 0);
            Vector3 TR = new Vector3(1, 1, 0);

            Vector3 screenSize = new Vector3(Graphics.Viewport.Width, Graphics.Viewport.Height, 1);
            Vector3 finalPosition = position / screenSize * 2;
            Vector3 finalSize = (size / screenSize) * 2 + BL;

            TL.Y = finalSize.Y;
            BR.X = finalSize.X;
            TR = finalSize;

            var vertices = new VertexPositionTexture[4];
            vertices[0].Position = (BL + finalPosition);
            vertices[0].TextureCoordinate = new Vector2(0, 1);
            vertices[1].Position = (TL + finalPosition);
            vertices[1].TextureCoordinate = new Vector2(0, 0);
            vertices[2].Position = (BR + finalPosition);
            vertices[2].TextureCoordinate = new Vector2(1, 1);
            vertices[3].Position = (TR + finalPosition);
            vertices[3].TextureCoordinate = new Vector2(1, 0);

            VertexBuffer = new VertexBuffer(Graphics, VertexPositionTexture.VertexDeclaration, 4,
                BufferUsage.WriteOnly);
            VertexBuffer.SetData(vertices);
        }

        private void CreateIndexBuffer()
        {
            var indices = new ushort[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 3;
            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 2;

            IndexBuffer = new IndexBuffer(Graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
        }
        public void Draw(Effect effect)
        {
            Graphics.SetVertexBuffer(VertexBuffer);
            Graphics.Indices = IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }
    }
}
