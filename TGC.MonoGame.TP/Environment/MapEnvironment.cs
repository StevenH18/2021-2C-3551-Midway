using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public IslandSystem IslandSystem;
        public SoundSystem SoundSystem;

        public Vector3 SunPosition = new Vector3(-7500f, 14000f, -50000f);
        public Vector3 SunColor = new Vector3(0.949f, 0.874f, 0.670f);
        public float SunIntensity = 50000f;
        public float Gravity = 25f;

        // Ocean config
        public int OceanWidth = 100000;
        public int OceanHeight = 100000;
        public int OceanQuads = 512;
        public int OceanTiling = 64;
        public float ShoreWidth = 100;
        public float ShoreSmoothness = 100;
        public RenderTarget2D OceanDepth;
        public RenderTarget2D OceanDepthColor;
        public Vector4[] IslandsPositions = new Vector4[5]
        {
            new Vector4(9000, -20, -500, 3000),
            new Vector4(-1000, 0, -1000, 10000),
            new Vector4(4000, 0, 4000, 5000),
            new Vector4(0, 0, 0, 0),
            new Vector4(0, 0, 0, 0)
        };
        // Waves will be controlled by the WeatherState
        public Vector4 WaveA;
        public Vector4 WaveB;
        public Vector4 WaveC;

        // Rain config
        public float RainParticleHeight = 50;
        public float RainParticleWidth = 1;

        public int RainParticles = 500000;
        public float RainParticleSeparation = 25000;
        public float RainParticleVerticalSeparation = 10000;

        public float RainHeightStart = 5000;
        public float RainHeightEnd = -300;
        public float RainSpeed = 3000;
        public float RainProgress = 0;

        // Thunder config
        private SpriteBatch SpriteBatch;
        private Texture2D WhiteScreen;
        private float TimeUntilThunderSeconds = 10f;
        private float ThunderDurationSeconds = 0.1f;
        private float MaxTimeUntilThunderSeconds = 120f;
        private float MinTimeUntilThunderSeconds = 40f;
        private bool StopThunder = false;
        private bool DrawThunder = false;

        // Ambience config
        public float OceanAmbienceVolume = 0f;
        public float StormAmbienceVolume = 0f;
        public float RainAmbienceVolume = 0f;

        // Weather config
        public Weather WeatherState = Weather.Calm;

        private Dictionary<(Weather, String), Object> WeatherValues = new Dictionary<(Weather, String), object>
        {
            // Weather Calm Values
            { (Weather.Calm, "WaveA"), new Vector4(-1f, -1f, 0.1f, 6000f) },
            { (Weather.Calm, "WaveB"), new Vector4(-1f, -0.6f, 0.1f, 3100f) },
            { (Weather.Calm, "WaveC"), new Vector4(-1f, -0.3f, 0.2f, 1800f) },

            { (Weather.Calm, "RainProgress"), 0f },

            { (Weather.Calm, "OceanAmbienceVolume"), 0.5f },
            { (Weather.Calm, "StormAmbienceVolume"), 0f },
            { (Weather.Calm, "RainAmbienceVolume"), 0f },

            // Weather Rain Values
            { (Weather.Rain, "WaveA"), new Vector4(-1f, -1f, 0.2f, 6000f) },
            { (Weather.Rain, "WaveB"), new Vector4(-1f, -0.6f, 0.2f, 3100f) },
            { (Weather.Rain, "WaveC"), new Vector4(-1f, -0.3f, 0.2f, 1800f) },

            { (Weather.Rain, "RainProgress"), 0.1f },

            { (Weather.Rain, "OceanAmbienceVolume"), 0.5f },
            { (Weather.Rain, "StormAmbienceVolume"), 0f },
            { (Weather.Rain, "RainAmbienceVolume"), 1f },

            // Weather Storm Values
            { (Weather.Storm, "WaveA"), new Vector4(-1f, -1f, 0.5f, 6000f) },
            { (Weather.Storm, "WaveB"), new Vector4(-1f, -0.6f, 0.2f, 3100f) },
            { (Weather.Storm, "WaveC"), new Vector4(-1f, -0.3f, 0.2f, 1800f) },

            { (Weather.Storm, "RainProgress"), 1f },

            { (Weather.Storm, "OceanAmbienceVolume"), 0.5f },
            { (Weather.Storm, "StormAmbienceVolume"), 1f },
            { (Weather.Storm, "RainAmbienceVolume"), 1f }
        };

        private Weather WeatherChangeTo;
        private bool WeatherChanging;
        private float WeatherAnimationStart;
        private float WeatherAnimationLengthSeconds = 10;

        public MapEnvironment(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;

            OceanDepth = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
            OceanDepthColor = new RenderTarget2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            SpriteBatch = new SpriteBatch(graphics);
            WhiteScreen = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "fondo-blanco");

            Ocean = new Ocean(Graphics, Content, this);
            SkyBox = new SkyBox(Graphics, Content, this);
            RainSystem = new RainSystem(Graphics, Content, this);
            IslandSystem = new IslandSystem(Graphics, Content, this);
            SoundSystem = new SoundSystem(Graphics, Content, this);
        }

        public void Load()
        {
            Ocean.Load();
            RainSystem.Load();
            IslandSystem.Load();
            SoundSystem.Load();

            // Initialize all values to the default Weather
            WeatherChangeTo = WeatherState;
            LerpWeatherValues(WeatherState, WeatherChangeTo, 0);
        }

        public void Update(GameTime gameTime)
        {
            if (Inputs.isJustPressed(Microsoft.Xna.Framework.Input.Keys.T))
            {
                SoundSystem.PlayRandomThunder();
            }
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

            KeyboardState kstate = Keyboard.GetState();
            if(kstate.IsKeyDown(Keys.O))
            {
                ShoreWidth += (float)gameTime.ElapsedGameTime.TotalSeconds * 50;
            }
            if (kstate.IsKeyDown(Keys.L))
            {
                ShoreWidth -= (float)gameTime.ElapsedGameTime.TotalSeconds * 50;
            }
            if (kstate.IsKeyDown(Keys.I))
            {
                ShoreSmoothness += (float)gameTime.ElapsedGameTime.TotalSeconds * 50;
            }
            if (kstate.IsKeyDown(Keys.K))
            {
                ShoreSmoothness -= (float)gameTime.ElapsedGameTime.TotalSeconds * 50;
            }

            ThunderEffects(gameTime);
            AnimateWeather(gameTime);
            SoundSystem.Initialize(gameTime);
            SoundSystem.Update(gameTime);
        }

        public void DrawPreTextures(GameTime gameTime, Matrix view, Matrix projection, Matrix world)
        {
            Graphics.SetRenderTarget(OceanDepth);
            Graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            IslandSystem.DrawCameraDepth(view, projection, world, gameTime);

            Graphics.SetRenderTarget(OceanDepthColor);
            Graphics.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            IslandSystem.Draw(view, projection, world);
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
            IslandSystem.Draw(view, projection, world);
            Ocean.Draw(view, projection, world, gameTime);
            SkyBox.Draw(view, projection, world);
            RainSystem.Draw(view, projection, world, gameTime);

            if(DrawThunder)
            {
                SpriteBatch.Begin();
                SpriteBatch.Draw(WhiteScreen, new Rectangle(0, 0, Graphics.Viewport.Width, Graphics.Viewport.Height), Color.White);
                SpriteBatch.End();
            }
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
                if(WeatherChangeTo == Weather.Storm)
                {
                    SoundSystem.PlayRandomThunder();
                }
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
            OceanAmbienceVolume = MathHelper.Lerp(
                (float)WeatherValues[(from, "OceanAmbienceVolume")],
                (float)WeatherValues[(to, "OceanAmbienceVolume")],
                progress);
            StormAmbienceVolume = MathHelper.Lerp(
                (float)WeatherValues[(from, "StormAmbienceVolume")],
                (float)WeatherValues[(to, "StormAmbienceVolume")],
                progress);
            RainAmbienceVolume = MathHelper.Lerp(
                (float)WeatherValues[(from, "RainAmbienceVolume")],
                (float)WeatherValues[(to, "RainAmbienceVolume")],
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
                float progress = (elapsedTime - WeatherAnimationStart) / WeatherAnimationLengthSeconds;
                LerpWeatherValues(WeatherState, WeatherChangeTo, progress);

                if (progress >= 1)
                {
                    WeatherChanging = false;
                    WeatherState = WeatherChangeTo;
                }
            }
        }
        private void ThunderEffects(GameTime gameTime)
        {
            if (WeatherState != Weather.Storm)
                return;

            float seconds = (float)gameTime.TotalGameTime.TotalSeconds;
            float miliseconds = (float)gameTime.TotalGameTime.TotalMilliseconds;

            DrawThunder = false;

            // Draw white screen
            if (seconds % TimeUntilThunderSeconds < ThunderDurationSeconds && miliseconds % 20f < 10f)
            {
                DrawThunder = true;
            }

            if (seconds % TimeUntilThunderSeconds <= ThunderDurationSeconds && StopThunder)
            {
                StopThunder = false;
            }

            if (seconds % TimeUntilThunderSeconds > ThunderDurationSeconds && !StopThunder)
            {
                StopThunder = true;

                Random random = new Random();
                float randomNumber = (float)random.NextDouble();

                ThunderDurationSeconds = 0.1f + randomNumber * 0.2f;
                TimeUntilThunderSeconds = (1 - randomNumber) * MinTimeUntilThunderSeconds + randomNumber * MaxTimeUntilThunderSeconds;

                SoundSystem.PlayRandomThunder();

            }
        }
    }
}
