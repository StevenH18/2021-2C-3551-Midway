using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TGC.MonoGame.TP
{
    public class Ocean
    {
        protected GraphicsDevice Graphics;
        protected ContentManager Content;
        protected Effect Effect;
        protected VertexBuffer VertexBuffer;
        protected IndexBuffer IndexBuffer;
        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.Graphics = graphics;
            this.Content = content;
        }
        public void Load()
        {
            VertexBuffer = new VertexBuffer(Graphics, VertexPosition.VertexDeclaration, 4, BufferUsage.None);

            VertexPosition[] verts = new VertexPosition[]
            {
                new VertexPosition(new Vector3(0f, 0f, 0f)),
                new VertexPosition(new Vector3(1f, 0f, 0f)),
                new VertexPosition(new Vector3(0f, 0f, 1f)),
                new VertexPosition(new Vector3(1f, 0f, 1f))
            };

            VertexBuffer.SetData(verts);

            IndexBuffer = new IndexBuffer(Graphics, IndexElementSize.SixteenBits, 6, BufferUsage.None);

            ushort[] indices = new ushort[]
            {
                0, 1, 2,
                3, 2, 1
            };

            IndexBuffer.SetData(indices);

            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "OceanShader");
        }
        public void Draw(Matrix view, Matrix proj)
        {
            Graphics.Indices = IndexBuffer;
            Graphics.SetVertexBuffer(VertexBuffer);

            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            }
        }
    }
}
