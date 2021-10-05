using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Environment
{
    class MapEnvironment
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;

        public Ocean Ocean;
        public RainSystem RainSystem;
        public SkyBox SkyBox;
        public Islands Islands;

        public MapEnvironment(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;

            Ocean = new Ocean(Graphics, Content);
            SkyBox = new SkyBox(Graphics, Content);
            RainSystem = new RainSystem(Graphics, Content);
            Islands = new Islands(Graphics, Content);
        }

        public void Load()
        {
            Ocean.Load();
            RainSystem.Load();
            Islands.Load();
        }

        public void Update(GameTime gameTime)
        {
            
        }

        /// <summary>
        ///     Se debe enviar matriz de vista, proyeccion y mundo de la camara
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        /// <param name="world"></param>
        public void Draw(GameTime gameTime, Matrix view, Matrix projection, Matrix world)
        {
            Ocean.Draw(view, projection, gameTime);
            RainSystem.Draw(view, projection, world, gameTime);
            SkyBox.Draw(view, projection, world);
            Islands.Draw(view, projection);
        }
    }
}
