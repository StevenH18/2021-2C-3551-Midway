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
        protected VertexPositionColor[] Verts;
        public Ocean(GraphicsDevice graphics, ContentManager content)
        {
            this.Graphics = graphics;
            this.Content = content;
        }
        public void Load()
        {
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BasicShader");

            Verts = new VertexPositionColor[3]
            {
                new VertexPositionColor(Vector3.Up, Color.Red),
                new VertexPositionColor(Vector3.Down, Color.Green),
                new VertexPositionColor(Vector3.Left, Color.Blue)
            };

            Buffer = new VertexBuffer(Graphics, VertexPositionColor.VertexDeclaration, 3, BufferUsage.WriteOnly);

            Buffer.SetData(Verts);
        }
        public void Draw(Matrix view, Matrix proj)
        {
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);

            foreach(EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, Verts, 0, 1);
            }
        }
    }
}
