using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Hud
{
    class HudController
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;

        // Radar config
        private Texture2D RadarTexture;
        private Texture2D RadarLineTexture;
        private Texture2D RadarMaskTexture;
        private int RadarSize = 250;
        private int RadarPadding = 30;
        private float RadarRange = 30000f;
        private Effect RadarEffect;

        // Crosshair config
        private Texture2D CrosshairTexture;
        private Effect CrosshairEffect;

        // Weather alert
        private SpriteBatch WeatherAlertSprite;
        private SpriteFont WeatherSpriteFont;

        // Ship Health config
        private Texture2D HealthTexture;
        private Effect HealthEffect;
        private int HealthSize = 250;
        private int HealthPadding = 30;

        public bool ShowHud = true;

        public HudController(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;
        }
        public void Load()
        {
            RadarTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/radar");
            RadarLineTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/radar_line");
            RadarMaskTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/radar_mask");
            RadarEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "RadarShader");

            CrosshairTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/crosshair");
            CrosshairEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "CrosshairShader");

            WeatherAlertSprite = new SpriteBatch(Graphics);
            WeatherSpriteFont = Content.Load<SpriteFont>("Fonts/WeatherHud");

            HealthTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/ship_health");
            HealthEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "ShipHealthShader");
        }
        public void Update(GameTime gameTime)
        {

        }
        public void Draw(GameTime gameTime, Ship[] ships, Camera activeCamera, MapEnvironment environment, bool drawCrosshair)
        {
            if (ships[0].Health <= 0 || !ShowHud)
                return;

            DrawRadar(gameTime, ships, activeCamera.World);
            DrawCrosshair(drawCrosshair);
            DrawWeatherAlert(gameTime, environment);
            DrawShipHealth((ShipPlayer)ships[0]);
        }
        private void DrawRadar(GameTime gameTime, Ship[] ships, Matrix cameraMatrix)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Vector3 radarPosition = new Vector3(RadarPadding, RadarPadding, 0);
            Vector3 radarSize = new Vector3(RadarSize, RadarSize, 0);

            ScreenQuad radar = new ScreenQuad(Graphics, radarPosition, radarSize);

            Vector3[] shipPositions = ships.Select(ship => ship.Position).ToArray();
            int[] shipsDestroyed = ships.Select(ship => ship.Destroyed ? 1 : 0).ToArray();

            RadarEffect.Parameters["RadarTexture"]?.SetValue(RadarTexture);
            RadarEffect.Parameters["RadarLineTexture"]?.SetValue(RadarLineTexture);
            RadarEffect.Parameters["RadarMaskTexture"]?.SetValue(RadarMaskTexture);
            RadarEffect.Parameters["RadarRange"]?.SetValue(RadarRange);
            RadarEffect.Parameters["ShipPositions"]?.SetValue(shipPositions);
            RadarEffect.Parameters["ShipsDestroyed"]?.SetValue(shipsDestroyed);
            RadarEffect.Parameters["CameraPosition"]?.SetValue(cameraMatrix.Translation);
            RadarEffect.Parameters["CameraForward"]?.SetValue(cameraMatrix.Forward);

            RadarEffect.Parameters["Time"]?.SetValue(time);

            radar.Draw(RadarEffect);
        }
        private void DrawCrosshair(bool drawCrosshair)
        {
            Vector3 crosshairPosition = new Vector3(0, (Graphics.Viewport.Height - 400) / 2, 0);
            Vector3 crosshairSize = new Vector3(Graphics.Viewport.Width, 400, 0);

            ScreenQuad crosshair = new ScreenQuad(Graphics, crosshairPosition, crosshairSize);
            CrosshairEffect.Parameters["CrosshairTexture"]?.SetValue(CrosshairTexture);

            if (drawCrosshair)
                crosshair.Draw(CrosshairEffect);
        }
        private void DrawWeatherAlert(GameTime gameTime, MapEnvironment environment)
        {
            string alerta = "ALERTA DE TORMENTA";
            Vector2 weatherAlertPosition = new Vector2((Graphics.Viewport.Width - WeatherSpriteFont.MeasureString(alerta).X) / 2, 100);
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            if (environment.WeatherChangeTo == Weather.Storm && environment.WeatherChanging && time % 2 < 1)
            {
                WeatherAlertSprite.Begin();
                WeatherAlertSprite.DrawString(WeatherSpriteFont, alerta, weatherAlertPosition, Color.OrangeRed);
                WeatherAlertSprite.End();
            }
        }
        private void DrawShipHealth(ShipPlayer shipPlayer)
        {
            HealthEffect.Parameters["PlayerHealth"]?.SetValue(shipPlayer.Health);
            HealthEffect.Parameters["PlayerMaxHealth"]?.SetValue(shipPlayer.MaxHealth);
            HealthEffect.Parameters["HealthTexture"]?.SetValue(HealthTexture);

            Vector3 healthPosition = new Vector3(HealthPadding, 250, 0);
            Vector3 healthSize = new Vector3(HealthSize, HealthSize, 0);

            ScreenQuad health = new ScreenQuad(Graphics, healthPosition, healthSize);

            health.Draw(HealthEffect);
        }
    }
}
