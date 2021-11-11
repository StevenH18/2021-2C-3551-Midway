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

        public Vector2 WaterSplashSize = new Vector2(1280, 720);
        public Vector2 WaterSplashSpritePixelSize = new Vector2(1280, 720);
        public Vector2 WaterSplashSpriteSheetSize = new Vector2(10240, 10800);
        public int WaterSplashSpriteCount = 120;

        public Vector2 ExplosionSize = new Vector2(1280, 720);
        public Vector2 ExplosionSpritePixelSize = new Vector2(640, 360);
        public Vector2 ExplosionSpriteSheetSize = new Vector2(9600, 9360);
        public int ExplosionSpriteCount = 375;

        public Vector2 HitSize = new Vector2(640, 360);
        public Vector2 HitSpritePixelSize = new Vector2(1280, 720);
        public Vector2 HitSpriteSheetSize = new Vector2(7680, 13680);
        public int HitSpriteCount = 112;

        public List<BillboardQuad> Sprites = new List<BillboardQuad>();

        public List<BillboardQuad> WaterSplashSprites = new List<BillboardQuad>();
        public int CurrentWaterSplash = 0;

        public List<BillboardQuad> ExplosionSprites = new List<BillboardQuad>();
        public int CurrentExplosion = 0;

        public List<BillboardQuad> HitSprites = new List<BillboardQuad>();
        public int CurrentHit = 0;

        public EffectSystem(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;
        } 

        public void Load()
        {
            for(var i = 0; i < 50; i++)
            {
                InitializeExplosion();
                InitializeWaterSplash();
                InitializeHit();
            }
        }

        public void InitializeExplosion()
        {
            Texture2D spriteSheet = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "SpriteSheets/explosion");
            Effect effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BillboardQuadShader");

            BillboardQuad billboard = new BillboardQuad(Graphics, spriteSheet, effect, ExplosionSize, ExplosionSpritePixelSize, ExplosionSpriteSheetSize, ExplosionSpriteCount);

            ExplosionSprites.Add(billboard);
            Sprites.Add(billboard);
        }
        public void InitializeWaterSplash()
        {
            Texture2D spriteSheet = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "SpriteSheets/water_splash");
            Effect effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BillboardQuadShader");

            BillboardQuad billboard = new BillboardQuad(Graphics, spriteSheet, effect, WaterSplashSize, WaterSplashSpritePixelSize, WaterSplashSpriteSheetSize, WaterSplashSpriteCount);

            WaterSplashSprites.Add(billboard);
            Sprites.Add(billboard);
        }
        public void InitializeHit()
        {
            Texture2D spriteSheet = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "SpriteSheets/hit");
            Effect effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BillboardQuadShader");

            BillboardQuad billboard = new BillboardQuad(Graphics, spriteSheet, effect, HitSize, HitSpritePixelSize, HitSpriteSheetSize, HitSpriteCount);

            HitSprites.Add(billboard);
            Sprites.Add(billboard);
        }

        public void CreateExplosion(Vector3 position)
        {
            BillboardQuad currentExplosion = ExplosionSprites[CurrentExplosion];

            currentExplosion.Position = position;
            currentExplosion.Play();

            CurrentExplosion++;
            if (CurrentExplosion >= ExplosionSprites.Count)
                CurrentExplosion = 0;
        }
        public void CreateWaterSplash(Vector3 position)
        {
            BillboardQuad currentWaterSplash = WaterSplashSprites[CurrentWaterSplash];

            currentWaterSplash.Position = position;
            currentWaterSplash.Play();

            CurrentWaterSplash++;
            if (CurrentWaterSplash >= WaterSplashSprites.Count)
                CurrentWaterSplash = 0;
        }
        public void CreateHit(Vector3 position)
        {
            BillboardQuad currenHit = HitSprites[CurrentHit];

            currenHit.Position = position;
            currenHit.Play();

            CurrentHit++;
            if (CurrentHit >= HitSprites.Count)
                CurrentHit = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (Inputs.isJustPressed(Keys.K))
            {
                var random = new Random();
                var position = new Vector3(random.Next(-2000, 2000), 250f, random.Next(-2000, 2000));
                CreateExplosion(position);
            }

            if (Inputs.isJustPressed(Keys.I))
            {
                var random = new Random();
                var position = new Vector3(random.Next(-2000, 2000), 370f, random.Next(-2000, 2000));
                CreateWaterSplash(position);
            }

            for (int i = 0; i < Sprites.Count; i++)
            {
                Sprites[i].Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, Matrix view, Matrix proj, Matrix cameraWorld)
        {
            Vector3 cameraPosition = cameraWorld.Translation;

            Sprites.Sort((y, x) => Vector3.Distance(x.Position, cameraPosition).CompareTo(Vector3.Distance(y.Position, cameraPosition)));

            for (int i = 0; i < Sprites.Count; i++)
            {
                Sprites[i].PixelSize = Sprites[i].PixelSize;
                Sprites[i].SpriteSheetSize = Sprites[i].SpriteSheetSize;
                Sprites[i].Position = Sprites[i].Position;

                Sprites[i].Effect.Parameters["SpriteSheet"]?.SetValue(Sprites[i].SpriteSheet);

                Sprites[i].Draw(gameTime, view, proj, Sprites[i].Effect);
            }

        }
    }
}
