using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Effects
{
    public class EffectSystem
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;

        public Effect WaterSplashEffect;
        public BillboardQuad WaterSplashQuad;
        public Texture2D WaterSplashSpriteSheet;
        public Vector3 WaterSplashPosition = new Vector3(0, 250, 1000);
        public Vector2 WaterSplashSize = new Vector2(640, 360);
        public Vector2 WaterSplashSpritePixelSize = new Vector2(640, 360);
        public Vector2 WaterSplashSpriteSheetSize = new Vector2(5760, 5760);
        public int WaterSplashSpriteIndex = 0;
        public int WaterSplashSpriteCount = 135;

        public Effect ExplosionEffect;
        public BillboardQuad ExplosionQuad;
        public Texture2D ExplosionSpriteSheet;
        public Vector3 ExplosionPosition = new Vector3(0, 300, -5000);
        public Vector2 ExplosionSize = new Vector2(640 * 2, 360 * 2);
        public Vector2 ExplosionSpritePixelSize = new Vector2(640, 360);
        public Vector2 ExplosionSpriteSheetSize = new Vector2(9600, 9360);
        public int ExplosionSpriteIndex = 0;
        public int ExplosionSpriteCount = 375;

        public EffectSystem(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;
        } 

        public void Load()
        {
            WaterSplashEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BillboardQuadShader");
            WaterSplashSpriteSheet = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "SpriteSheets/water_splash");
            WaterSplashQuad = new BillboardQuad(Graphics, WaterSplashPosition, WaterSplashSize);

            ExplosionEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BillboardQuadShader");
            ExplosionSpriteSheet = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "SpriteSheets/explosion");
            ExplosionQuad = new BillboardQuad(Graphics, ExplosionPosition, ExplosionSize);
        }

        public void Update(GameTime gameTime)
        {

            ExplosionQuad.Position = ExplosionPosition;

            if (Inputs.isJustPressed(Keys.K)) 
            {
                WaterSplashSpriteIndex = 0;
                ExplosionSpriteIndex = 0;
            }

            Animate(gameTime);
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix proj)
        {
            WaterSplashQuad.SpritePixelSize = WaterSplashSpritePixelSize;
            WaterSplashQuad.SpriteSheetSize = WaterSplashSpriteSheetSize;
            WaterSplashQuad.SpriteIndex = WaterSplashSpriteIndex;

            WaterSplashEffect.Parameters["SpriteSheet"]?.SetValue(WaterSplashSpriteSheet);
            WaterSplashQuad.Draw(gameTime, view, proj, WaterSplashEffect);

            ExplosionQuad.SpritePixelSize = ExplosionSpritePixelSize;
            ExplosionQuad.SpriteSheetSize = ExplosionSpriteSheetSize;
            ExplosionQuad.SpriteIndex = ExplosionSpriteIndex;

            ExplosionEffect.Parameters["SpriteSheet"]?.SetValue(ExplosionSpriteSheet);
            ExplosionQuad.Draw(gameTime, view, proj, ExplosionEffect);
        }

        public void Animate(GameTime gameTime)
        {
            if(WaterSplashSpriteIndex < WaterSplashSpriteCount)
            {
                WaterSplashSpriteIndex++;
            }
            if (ExplosionSpriteIndex < ExplosionSpriteCount)
            {
                ExplosionSpriteIndex++;
            }
        }
    }
}
