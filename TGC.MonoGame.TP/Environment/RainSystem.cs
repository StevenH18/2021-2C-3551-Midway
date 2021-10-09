using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP
{
    public class RainSystem
    {
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;
        protected MapEnvironment Environment;

        public RainParticle[] RainParticles;

        public RainSystem(GraphicsDevice graphics, ContentManager content, MapEnvironment environment)
        {
            GraphicsDevice = graphics;
            Content = content;
            Environment = environment;
        }
        public void Load()
        {
            RainParticles = new RainParticle[Environment.RainParticles];
            for(var i = 0; i < Environment.RainParticles; i++)
            {
                Random random = new Random();

                Vector3 offset = Vector3.Zero;
                offset.X = (float)random.NextDouble() * Environment.RainParticleSeparation;
                offset.Z = (float)random.NextDouble() * Environment.RainParticleSeparation;
                float timeOffset = (float)random.NextDouble() * Environment.RainParticleVerticalSeparation;

                RainParticles[i] = new RainParticle(GraphicsDevice, Content, Environment.RainParticleWidth, Environment.RainParticleHeight, offset, timeOffset);
                RainParticles[i].Load();
            }
        }
        public void Draw(Matrix view, Matrix proj, Matrix cameraWorld, GameTime gameTime)
        {
            for (var i = 0; i < Environment.RainParticles; i++)
            {
                bool skip = ((float)i / (float)Environment.RainParticles) >= Environment.RainProgress;
                RainParticles[i].Draw(view, proj, cameraWorld, Environment.RainParticleSeparation, Environment.RainHeightStart, Environment.RainHeightEnd, Environment.RainSpeed, skip, gameTime);
            }
        }
    }
}
