using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TGC.MonoGame.TP.Effects
{
    public class EffectSystem
    {
        private GraphicsDevice Graphics;
        private ContentManager Content;
        private Camera ActiveCamera;

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

        public Vector2 FireSize = new Vector2(640, 360);
        public Vector2 FireSpritePixelSize = new Vector2(1280, 720);
        public Vector2 FireSpriteSheetSize = new Vector2(3840, 8640);
        public int FireSpriteCount = 36;

        public List<BillboardQuad> Sprites = new List<BillboardQuad>();

        public List<BillboardQuad> WaterSplashSprites = new List<BillboardQuad>();
        public int CurrentWaterSplash = 0;

        public List<BillboardQuad> ExplosionSprites = new List<BillboardQuad>();
        public int CurrentExplosion = 0;

        public List<BillboardQuad> HitSprites = new List<BillboardQuad>();
        public int CurrentHit = 0;

        public List<BillboardQuad> FireSprites = new List<BillboardQuad>();
        public int CurrentFire = 0;

        public List<SoundEffect> ExplosionSounds;
        public List<SoundEffect> WaterSplashSounds;
        public List<SoundEffect> FireSounds;
        public List<SoundEffect> HitSounds;

        public EffectSystem(GraphicsDevice graphics, ContentManager content)
        {
            Graphics = graphics;
            Content = content;
        } 

        public void Load()
        {
            ExplosionSounds = new List<SoundEffect>();
            WaterSplashSounds = new List<SoundEffect>();
            FireSounds = new List<SoundEffect>();
            HitSounds = new List<SoundEffect>();

            ExplosionSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/explosion1"));
            ExplosionSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/explosion2"));
            ExplosionSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/explosion3"));

            WaterSplashSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/water_splash1"));

            FireSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/fire1"));
            FireSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/fire2"));
            FireSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/fire3"));

            HitSounds.Add(Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "/Effects/hit1"));

            for (var i = 0; i < 50; i++)
            {
                InitializeExplosion();
                InitializeWaterSplash();
                InitializeHit();
                InitializeFire();
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
        public void InitializeFire()
        {
            Texture2D spriteSheet = Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "SpriteSheets/fire");
            Effect effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BillboardQuadShader");

            BillboardQuad billboard = new BillboardQuad(Graphics, spriteSheet, effect, FireSize, FireSpritePixelSize, FireSpriteSheetSize, FireSpriteCount);

            FireSprites.Add(billboard);
            Sprites.Add(billboard);
        }

        private void Play3DSound(SoundEffect soundEffect, Vector3 position)
        {
            var target = position - ActiveCamera.World.Translation;
            var camera = ActiveCamera.World.Forward;

            var x1 = target.X;
            var y1 = target.Z;
            var x2 = camera.X;
            var y2 = camera.Z;

            var angleDiff = MathF.Atan2(x1 * y2 - x2 * y1, x1 * x2 + y1 * y2) % (2 * MathF.PI);
            angleDiff = angleDiff * 180f / (float)Math.PI;

            float volume = MathF.Pow(0.9f, Vector3.Distance(ActiveCamera.World.Translation, position) / 500f);
            float volumeLeft = (float)Math.Clamp(MathF.Abs(angleDiff - 90) / 90, 0, 1) * volume;
            float volumeRight = (float)Math.Clamp(MathF.Abs(angleDiff + 90) / 90, 0, 1) * volume;

            soundEffect.Play(volumeLeft, 0, 1);
            soundEffect.Play(volumeRight, 0, -1);
        }

        public void CreateExplosion(Vector3 position)
        {
            BillboardQuad currentExplosion = ExplosionSprites[CurrentExplosion];

            currentExplosion.Position = position;
            currentExplosion.Play();

            var random = new Random();
            var randomExplosionSound = random.Next(0, ExplosionSounds.Count);

            Play3DSound(ExplosionSounds[randomExplosionSound], position);

            CurrentExplosion++;
            if (CurrentExplosion >= ExplosionSprites.Count)
                CurrentExplosion = 0;
        }
        public void CreateWaterSplash(Vector3 position)
        {
            BillboardQuad currentWaterSplash = WaterSplashSprites[CurrentWaterSplash];

            currentWaterSplash.Position = position;
            currentWaterSplash.Play();

            var random = new Random();
            var randomWaterSplashSound = random.Next(0, WaterSplashSounds.Count);

            Play3DSound(WaterSplashSounds[randomWaterSplashSound], position);

            CurrentWaterSplash++;
            if (CurrentWaterSplash >= WaterSplashSprites.Count)
                CurrentWaterSplash = 0;
        }
        public void CreateHit(Vector3 position)
        {
            BillboardQuad currenHit = HitSprites[CurrentHit];

            currenHit.Position = position;
            currenHit.Play();

            var random = new Random();
            var randomHitSound = random.Next(0, HitSounds.Count);

            Play3DSound(HitSounds[randomHitSound], position);

            CurrentHit++;
            if (CurrentHit >= HitSprites.Count)
                CurrentHit = 0;
        }
        public void CreateFire(Vector3 position)
        {
            BillboardQuad currentFire = FireSprites[CurrentFire];

            currentFire.Position = position;
            currentFire.Play();

            var random = new Random();
            var randomFireSound = random.Next(0, FireSounds.Count);

            Play3DSound(FireSounds[randomFireSound], position);

            CurrentFire++;
            if (CurrentFire >= FireSprites.Count)
                CurrentFire = 0;
        }

        public void Update(GameTime gameTime, Camera activeCamera)
        {
            ActiveCamera = activeCamera;

            if (Inputs.isJustPressed(Keys.K))
            {
                var random = new Random();
                var position = new Vector3(0, 250f, 0);
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
