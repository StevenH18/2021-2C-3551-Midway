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
        protected VertexBuffer Buffer;
        protected VertexPosition[] Verts;
        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.Graphics = graphics;
            this.Content = content;
        }
        public void Load()
        {
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "OceanShader");

            Verts = new VertexPosition[6]
            {
                new VertexPosition(new Vector3(0f, 0f, 0f)),
                new VertexPosition(new Vector3(1f, 0f, 0f)),
                new VertexPosition(new Vector3(0f, 0f, 1f)),
                new VertexPosition(new Vector3(1f, 0f, 1f)),
                new VertexPosition(new Vector3(0f, 0f, 1f)),
                new VertexPosition(new Vector3(1f, 0f, 0f))
            };

            Buffer = new VertexBuffer(Graphics, VertexPosition.VertexDeclaration, 6, BufferUsage.WriteOnly);

            Buffer.SetData(Verts);
        }
        public void Draw(Matrix view, Matrix proj)
        {
            Effect.Parameters["World"].SetValue(Matrix.CreateScale(20f));
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);

            foreach(EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Graphics.DrawUserPrimitives<VertexPosition>(PrimitiveType.TriangleList, Verts, 0, 2);
            }
        }
    }
}
