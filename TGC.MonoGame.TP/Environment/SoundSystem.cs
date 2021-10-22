using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Environment
{
    public class SoundSystem
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;
        private MapEnvironment Environment;
        private List<SoundEffect> ThunderSounds;
        private SoundEffectInstance OceanAmbience;
        private SoundEffectInstance StormAmbience;
        private SoundEffectInstance RainAmbience;
        private float VolumeEaseInSeconds = 5f;

        public SoundSystem(GraphicsDevice graphics, ContentManager content, MapEnvironment environment)
        {
            Graphics = graphics;
            Content = content;
            Environment = environment;
        }

        public void Load()
        {
            ThunderSounds = new List<SoundEffect>();

            ThunderSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/thunder1"));
            ThunderSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/thunder2"));
            ThunderSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/thunder3"));
            ThunderSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/thunder4"));
            ThunderSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/thunder5"));

            SoundEffect oceanAmbience = Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/ocean_ambience");
            SoundEffect stormAmbience = Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/storm_ambience");
            SoundEffect rainAmbience = Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Ambience/rain_ambience");

            OceanAmbience = oceanAmbience.CreateInstance();
            StormAmbience = stormAmbience.CreateInstance();
            RainAmbience = rainAmbience.CreateInstance();

            OceanAmbience.IsLooped = true;
            StormAmbience.IsLooped = true;
            RainAmbience.IsLooped = true;

            // Comienzo con volumen en 0 para que no te rompa los timpanos
            OceanAmbience.Volume = 0f;
            StormAmbience.Volume = 0f;
            RainAmbience.Volume = 0f;

            OceanAmbience.Play();
            StormAmbience.Play();
            RainAmbience.Play();

        }

        public void Update(GameTime gameTime)
        {
            UpdateAmbienceVolume(gameTime);
        }

        private void UpdateAmbienceVolume(GameTime gameTime)
        {
            // Lentamente subo el volumen maximo hasta 1, asi el sonido no es muy impactante
            float maxVolumeSmooth = (float)Math.Clamp(gameTime.TotalGameTime.TotalSeconds / VolumeEaseInSeconds, 0f, 1f);

            OceanAmbience.Volume = Math.Clamp(Environment.OceanAmbienceVolume, 0f, maxVolumeSmooth);
            StormAmbience.Volume = Math.Clamp(Environment.StormAmbienceVolume, 0f, maxVolumeSmooth);
            RainAmbience.Volume = Math.Clamp(Environment.RainAmbienceVolume, 0f, maxVolumeSmooth);
        }

        public void PlayRandomThunder()
        {
            if (ThunderSounds == null)
                return;

            int randomIndex = (new Random()).Next(0, ThunderSounds.Count);
            if(ThunderSounds.Count > 0 && randomIndex <= ThunderSounds.Count)
            {
                ThunderSounds[randomIndex].Play();
            }
        }
    }
}
