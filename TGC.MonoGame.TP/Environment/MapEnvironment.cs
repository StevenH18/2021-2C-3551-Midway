using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Environment
{
    public class MapEnvironment
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;

        public Ocean Ocean;
        public RainSystem RainSystem;
        public SkyBox SkyBox;
        public Islands Islands;

        public Vector3 SunPosition = new Vector3(5000f, 5000f, -30000f);
        public float Gravity = 9.8f;

        // Ocean config
        public int OceanWidth = 50000;
        public int OceanHeight = 50000;
        public int OceanDensity = 256;
        public Vector4[] IslandsPositions = new Vector4[5]
        {
            new Vector4(5000,0,5000,1500),
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,0)
        };
        public Vector4 WaveA = new Vector4(-1f, -1f, 0.05f, 6000f);
        public Vector4 WaveB = new Vector4(-1f, -0.6f, 0.05f, 3100f);
        public Vector4 WaveC = new Vector4(-1f, -0.3f, 0.05f, 1800f);
        public Vector3 OceanAmbientColor = new Vector3(0.243f, 0.313f, 0.356f);
        public Vector3 OceanDiffuseColor = new Vector3(0.407f, 0.552f, 0.576f);
        public Vector3 OceanSpecularColor = new Vector3(0.894f, 0.941f, 0.776f);

        // Rain config
        public float RainParticleHeight = 25;
        public float RainParticleWidth = 2;

        public int RainParticles = 5000;
        public float RainParticleSeparation = 6000;
        public float RainParticleVerticalSeparation = 100;

        public float RainHeightStart = 3000;
        public float RainHeightEnd = -500;
        public float RainSpeed = 1000;

        public MapEnvironment(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;

            Ocean = new Ocean(Graphics, Content, this);
            SkyBox = new SkyBox(Graphics, Content);
            RainSystem = new RainSystem(Graphics, Content, this);
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
            Ocean.Draw(view, projection, world, gameTime);
            RainSystem.Draw(view, projection, world, gameTime);
            SkyBox.Draw(view, projection, world);
            Islands.Draw(view, projection);
        }
    }
}
