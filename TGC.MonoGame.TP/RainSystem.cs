using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP
{
    class RainSystem
    {
        protected GraphicsDevice GraphicsDevice;
        protected ContentManager Content;

        public RainParticle[] RainParticles;
        private float ParticleHeight = 50;
        private float ParticleWidth = 1;

        private int MaxParticles = 5000;
        private float ParticleSeparation = 8000;
        private float ParticleVerticalSeparation = 1000;

        private float HeightStart = 3000;
        private float HeightEnd = -500;
        private float Speed = 1500;

        public RainSystem(GraphicsDevice graphics, ContentManager content)
        {
            GraphicsDevice = graphics;
            Content = content;
        }
        public void Load()
        {
            RainParticles = new RainParticle[MaxParticles];
            for(var i = 0; i < MaxParticles; i++)
            {
                Random random = new Random();

                Vector3 offset = Vector3.Zero;
                offset.X = (float)random.NextDouble() * ParticleSeparation;
                offset.Z = (float)random.NextDouble() * ParticleSeparation;
                float timeOffset = (float)random.NextDouble() * ParticleVerticalSeparation;

                RainParticles[i] = new RainParticle(GraphicsDevice, Content, ParticleWidth, ParticleHeight, offset, timeOffset);
                RainParticles[i].Load();
            }
        }
        public void Draw(Matrix view, Matrix proj, Matrix cameraWorld, GameTime gameTime)
        {
            for (var i = 0; i < MaxParticles; i++)
            {
                RainParticles[i].Draw(view, proj, cameraWorld, ParticleSeparation, HeightStart, HeightEnd, Speed, gameTime);
            }
        }
    }
}
