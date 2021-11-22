using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Artillery;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipsSystem
    {
        private ContentManager Content;
        private GraphicsDevice Graphics;
        private MapEnvironment Environment;
        private Gizmos Gizmos;

        public EffectSystem EffectSystem;

        public int ShipsCount = 20;
        public int ShipsSeparation = 20000;
        public Ship[] Ships;
        public Ship ShipPlayer;

        public ShipsSystem(GraphicsDevice graphics, ContentManager content, Gizmos gizmos)
        {
            Content = content;
            Graphics = graphics;
            Gizmos = gizmos;

            Ships = new Ship[ShipsCount];

            Ships[0] = new ShipPlayer(Content, Graphics, Gizmos);
            ShipPlayer = Ships[0];

            ShipPlayer.Position.X = 1;
            ShipPlayer.Position.Z = 1;

            for (int i = 1; i < ShipsCount; i++)
            {
                Random random = new Random();

                Ships[i] = new ShipEnemy(Content, Graphics, (ShipPlayer)ShipPlayer, Gizmos);

                Ships[i].Position.X = random.Next(-ShipsSeparation, ShipsSeparation);
                Ships[i].Position.Z = random.Next(-ShipsSeparation, ShipsSeparation);
            }
        }
        public void Load()
        {
            ShipPlayer.Load();
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Load();
            }
        }
        public void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem, WeaponSystem weaponSystem, Camera activeCamera)
        {
            Environment = environment;
            EffectSystem = effectSystem;
            ShipPlayer.Update(gameTime, Environment, EffectSystem, weaponSystem, activeCamera);
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Update(gameTime, Environment, EffectSystem, weaponSystem, activeCamera);
            }
        }
        public void Draw(Matrix view, Matrix proj, RenderState renderState)
        {
            ShipPlayer.Draw(view, proj, renderState);
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Draw(view, proj, renderState);
            }
        }
    }
}
