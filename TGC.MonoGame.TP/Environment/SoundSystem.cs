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

            OceanAmbience.Play();
            StormAmbience.Play();
            RainAmbience.Play();

            UpdateAmbienceVolume();
        }

        public void Update(GameTime gameTime)
        {
            if(Inputs.isJustPressed(Microsoft.Xna.Framework.Input.Keys.T))
            {
                PlayRandomThunder();
            }

            UpdateAmbienceVolume();
        }

        private void UpdateAmbienceVolume()
        {
            OceanAmbience.Volume = Math.Clamp(Environment.OceanAmbienceVolume, 0f, 1f);
            StormAmbience.Volume = Math.Clamp(Environment.StormAmbienceVolume, 0f, 1f);
            RainAmbience.Volume = Math.Clamp(Environment.RainAmbienceVolume, 0f, 1f);
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
