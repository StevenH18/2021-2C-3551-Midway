using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Environment
{
    public enum Weather
    {
        Calm, Rain, Storm
    }
    public class MapEnvironment
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;

        public Ocean Ocean;
        public RainSystem RainSystem;
        public SkyBox SkyBox;
        public Islands Islands;

        public Vector3 SunPosition = new Vector3(0000f, 7000f, -30000f);
        public float Gravity = 50f;

        // Ocean config
        public int OceanWidth = 50000;
        public int OceanHeight = 50000;
        public int OceanQuads = 256;
        public int OcealTiling = 64;
        public Vector4[] IslandsPositions = new Vector4[5]
        {
            new Vector4(0,0,0,1500),
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,0)
        };
        // Waves will be controlled by the WeatherState
        public Vector4 WaveA;
        public Vector4 WaveB;
        public Vector4 WaveC;
        // Ocean colors
        public Vector3 OceanAmbientColor = new Vector3(0.129f, 0.145f, 0.160f);
        public Vector3 OceanDiffuseColor = new Vector3(0.113f, 0.145f, 0.184f);
        public Vector3 OceanSpecularColor = new Vector3(0.949f, 0.874f, 0.670f);

        // Rain config
        public float RainParticleHeight = 50;
        public float RainParticleWidth = 2;

        public int RainParticles = 5000;
        public float RainParticleSeparation = 6000;
        public float RainParticleVerticalSeparation = 2000;

        public float RainHeightStart = 3000;
        public float RainHeightEnd = -500;
        public float RainSpeed = 1500;
        public float RainProgress = 0;

        // Weather config
        public Weather WeatherState = Weather.Calm;

        private Dictionary<(Weather, String), Object> WeatherValues = new Dictionary<(Weather, String), object>
        {
            // Weather Calm Values
            { (Weather.Calm, "WaveA"), new Vector4(-1f, -1f, 0f, 6000f) },
            { (Weather.Calm, "WaveB"), new Vector4(-1f, -0.6f, 0.01f, 3100f) },
            { (Weather.Calm, "WaveC"), new Vector4(-1f, -0.3f, 0.05f, 1800f) },
            { (Weather.Calm, "RainProgress"), 0f },

            // Weather Storm Values
            { (Weather.Rain, "WaveA"), new Vector4(-1f, -1f, 0.05f, 6000f) },
            { (Weather.Rain, "WaveB"), new Vector4(-1f, -0.6f, 0.05f, 3100f) },
            { (Weather.Rain, "WaveC"), new Vector4(-1f, -0.3f, 0.05f, 1800f) },
            { (Weather.Rain, "RainProgress"), 0.2f },

            // Weather Storm Values
            { (Weather.Storm, "WaveA"), new Vector4(-1f, -1f, 0.2f, 6000f) },
            { (Weather.Storm, "WaveB"), new Vector4(-1f, -0.6f, 0.1f, 3100f) },
            { (Weather.Storm, "WaveC"), new Vector4(-1f, -0.3f, 0.1f, 1800f) },
            { (Weather.Storm, "RainProgress"), 1f }
        };

        private Weather WeatherChangeTo;
        private bool WeatherChanging;
        private float WeatherAnimationStart;
        private float WeatherAnimationDuration = 1;

        public MapEnvironment(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;

            Ocean = new Ocean(Graphics, Content, this);
            SkyBox = new SkyBox(Graphics, Content, this);
            RainSystem = new RainSystem(Graphics, Content, this);
            Islands = new Islands(Graphics, Content);
        }

        public void Load()
        {
            Ocean.Load();
            RainSystem.Load();
            Islands.Load();

            // Initialize all values to the default Weather
            LerpWeatherValues(WeatherState, WeatherChangeTo, 0);
        }

        public void Update(GameTime gameTime)
        {
            if (Inputs.isJustPressed(Microsoft.Xna.Framework.Input.Keys.P)) 
            {
                if(WeatherState == Weather.Calm)
                {
                    ChangeWeather(Weather.Rain);
                }
                else if (WeatherState == Weather.Rain)
                {
                    ChangeWeather(Weather.Storm);
                }
                else
                {
                    ChangeWeather(Weather.Calm);
                }
            }

            AnimateWeather(gameTime);
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

        /// <summary>
        ///     Funcion para cambiar el clima, el clima no cambiara durante el cambio de clima a otro
        /// </summary>
        /// <param name="weather"></param>
        /// <returns></returns>
        public bool ChangeWeather(Weather weather)
        {
            if (!WeatherChanging)
            {
                WeatherChangeTo = weather;
            }
            return !WeatherChanging;
        }

        private void LerpWeatherValues(Weather from, Weather to, float progress)
        {
            WaveA = Vector4.Lerp(
                (Vector4)WeatherValues[(from, "WaveA")],
                (Vector4)WeatherValues[(to, "WaveA")],
                progress);
            WaveB = Vector4.Lerp(
                (Vector4)WeatherValues[(from, "WaveB")],
                (Vector4)WeatherValues[(to, "WaveB")],
                progress);
            WaveC = Vector4.Lerp(
                (Vector4)WeatherValues[(from, "WaveC")],
                (Vector4)WeatherValues[(to, "WaveC")],
                progress);
            RainProgress = MathHelper.Lerp(
                (float)WeatherValues[(from, "RainProgress")],
                (float)WeatherValues[(to, "RainProgress")],
                progress);
        }

        private void AnimateWeather(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.TotalGameTime.TotalSeconds;

            // Start animating
            if (WeatherChangeTo != WeatherState && !WeatherChanging)
            {
                WeatherChanging = true;
                WeatherAnimationStart = elapsedTime;
            }
            if(WeatherChanging)
            {
                float progress = (elapsedTime - WeatherAnimationStart) / WeatherAnimationDuration;
                LerpWeatherValues(WeatherState, WeatherChangeTo, progress);

                if (progress >= 1)
                {
                    WeatherChanging = false;
                    WeatherState = WeatherChangeTo;
                }
            }
        }
    }
}
