using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Hud
{
    class HudController
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;

        private SpriteBatch SpriteBatch;

        private Texture2D RadarTexture;
        private Texture2D RadarLineTexture;
        private int RadarSize = 300;
        private int RadarPadding = 50;
        private Effect RadarEffect;

        public HudController(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;

            SpriteBatch = new SpriteBatch(graphics);
        }
        public void Load()
        {
            RadarTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/radar");
            RadarLineTexture = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "Hud/radar_line");
            RadarEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "RadarShader");
        }
        public void Update(GameTime gameTime)
        {
            
        }
        public void Draw(GameTime gameTime, Ship[] ships, Matrix cameraMatrix)
        {
            DrawRadar(gameTime, ships, cameraMatrix);
        }
        private void DrawRadar(GameTime gameTime, Ship[] ships, Matrix cameraMatrix)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Vector3 radarPosition = new Vector3(RadarPadding, RadarPadding, 0);
            Vector3 radarSize = new Vector3(RadarSize, RadarSize, 0);

            ScreenQuad radar = new ScreenQuad(Graphics, radarPosition, radarSize);

            Vector3[] shipPositions = ships.Select(ship => ship.Position).ToArray();

            RadarEffect.Parameters["RadarTexture"]?.SetValue(RadarTexture);
            RadarEffect.Parameters["RadarLineTexture"]?.SetValue(RadarLineTexture);
            RadarEffect.Parameters["CameraPosition"]?.SetValue(cameraMatrix.Translation);
            RadarEffect.Parameters["CameraForward"]?.SetValue(cameraMatrix.Forward);
            RadarEffect.Parameters["RadarRange"]?.SetValue(9000f);

            RadarEffect.Parameters["ShipPositions"]?.SetValue(shipPositions);

            RadarEffect.Parameters["Time"]?.SetValue(time);

            radar.Draw(RadarEffect);
        }
    }
}
