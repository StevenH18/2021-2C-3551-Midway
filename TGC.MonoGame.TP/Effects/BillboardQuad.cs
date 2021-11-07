using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Effects
{
    public class BillboardQuad
    {
        private GraphicsDevice Graphics;

        private VertexBuffer VertexBuffer;
        private IndexBuffer IndexBuffer;

        public Texture2D SpriteSheet;
        public Effect Effect;

        public Vector3 Position;
        public float Width;
        public float Height;
        public Vector2 PixelSize;
        public Vector2 SpriteSheetSize;
        public Vector2 SpriteOffset = Vector2.Zero;
        public int SpriteIndex = 0;
        public int SpriteCount;

        public BillboardQuad(GraphicsDevice graphics, Texture2D spriteSheet, Effect effect, Vector2 size, Vector2 pixelSize, Vector2 spriteSheetSize, int spriteCount)
        {
            Graphics = graphics;

            SpriteSheet = spriteSheet;
            Effect = effect;
            Width = size.X;
            Height = size.Y;
            PixelSize = pixelSize;
            SpriteSheetSize = spriteSheetSize;
            SpriteIndex = spriteCount;
            SpriteCount = spriteCount;

            InitializeGeometry();
        }
        public void Play()
        {
            SpriteIndex = 0;
        }
        public void Update(GameTime gameTime)
        {
            if (SpriteIndex < SpriteCount)
            {
                SpriteIndex++;
            }
        }
        public void Draw(GameTime gameTime, Matrix view, Matrix proj, Effect effect)
        {
            if (SpriteIndex >= SpriteCount)
                return;

            var time = (float)gameTime.TotalGameTime.TotalSeconds;

            SpriteOffset = new Vector2(SpriteIndex % ((SpriteSheetSize.X) / PixelSize.X), MathF.Floor(SpriteIndex / (SpriteSheetSize.X / PixelSize.X)));
            SpriteOffset = SpriteOffset * PixelSize;

            effect.Parameters["World"]?.SetValue(Matrix.CreateTranslation(Position));
            effect.Parameters["View"]?.SetValue(view);
            effect.Parameters["Projection"]?.SetValue(proj);
            effect.Parameters["Time"]?.SetValue(time);
            effect.Parameters["SpritePixelSize"]?.SetValue(PixelSize);
            effect.Parameters["SpriteSheetSize"]?.SetValue(SpriteSheetSize);
            effect.Parameters["SpriteOffset"]?.SetValue(SpriteOffset);

            Graphics.Indices = IndexBuffer;
            Graphics.SetVertexBuffer(VertexBuffer);

            effect.CurrentTechnique.Passes[0].Apply();

            var triangles = 4;

            Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles);
        }

        private void InitializeGeometry()
        {
            // Creo vertices en base al GridWidth y GridHeight
            VertexPositionTexture[] vertices = CalculateVertices();

            VertexBuffer = new VertexBuffer(Graphics, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.None);

            VertexBuffer.SetData(vertices);

            // Load Indices
            ushort[] indices = CalculateIndices();

            IndexBuffer = new IndexBuffer(Graphics, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);

            IndexBuffer.SetData(indices);
        }

        private VertexPositionTexture[] CalculateVertices()
        {
            var vertices = new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-Width / 2,  Height / 2, 0f), new Vector2(0,0)),
                new VertexPositionTexture(new Vector3( Width / 2,  Height / 2, 0f), new Vector2(1,0)),
                new VertexPositionTexture(new Vector3(-Width / 2, -Height / 2, 0f), new Vector2(0,1)),
                new VertexPositionTexture(new Vector3( Width / 2, -Height / 2, 0f), new Vector2(1,1))
            };

            return vertices;
        }
        /// <summary>
        /// Crea los indices de los quads (2 triangulos) para enviarlos al IndexBuffer
        /// </summary>
        private ushort[] CalculateIndices()
        {
            var indices = new ushort[6]{
                0, 1, 2,
                1, 3, 2
            };

            return indices;
        }
    }
}
