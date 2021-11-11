using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Artillery
{
    public class WeaponSystem
    {
        private ContentManager Content;
        private GraphicsDevice Graphics;
        private EffectSystem EffectSystem;
        private MapEnvironment Environment;
        private ShipsSystem ShipsSystem;
        private Gizmos Gizmos;

        private int MaxShells = 20;
        private Shell[] Shells;
        private int CurrentShell = 0;

        public WeaponSystem(GraphicsDevice graphics, ContentManager content, EffectSystem effectSystem, MapEnvironment environment, ShipsSystem shipsSystem, Gizmos gizmos)
        {
            Content = content;
            Graphics = graphics;
            EffectSystem = effectSystem;
            Environment = environment;
            ShipsSystem = shipsSystem;
            Gizmos = gizmos;

            Shells = new Shell[MaxShells];

            for(int i = 0; i < MaxShells; i++)
            {
                Shells[i] = new Shell(Graphics, Content, EffectSystem, Environment, ShipsSystem, Gizmos);
            }
        }
        public void Fire(Vector3 position, Vector3 velocity)
        {
            Shells[CurrentShell].Fire(position, velocity);

            CurrentShell++;
            if (CurrentShell >= Shells.Length)
                CurrentShell = 0;
        }
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Shells.Length; i++)
            {
                Shells[i].Update(gameTime);
            }
        }
        public void Draw(GameTime gameTime, Matrix view, Matrix proj, Matrix cameraWorld)
        {
            for(int i = 0; i < Shells.Length; i++)
            {
                Shells[i].Draw(gameTime, view, proj);
            }
        }
    }
}
