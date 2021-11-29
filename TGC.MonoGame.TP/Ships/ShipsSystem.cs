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
        public int ShipsSeparation = 2500;
        public Ship[] Ships;
        public Ship ShipPlayer;

        public Vector3[] SpawnPoints = new Vector3[]
        {
            new Vector3(-12000, 0, 12000),
            new Vector3(12000, 0, -12000),
            new Vector3(0, 0, -8000),
            new Vector3(-12000, 0, -12000),
        };

        public ShipsSystem(GraphicsDevice graphics, ContentManager content, Gizmos gizmos)
        {
            Content = content;
            Graphics = graphics;
            Gizmos = gizmos;

            Ships = new Ship[ShipsCount];

            Ships[0] = new ShipPlayer(Content, Graphics, Gizmos);
            ShipPlayer = Ships[0];

            for (int i = 1; i < ShipsCount; i++)
            {
                Ships[i] = new ShipEnemy(Content, Graphics, (ShipPlayer)ShipPlayer, Gizmos);
            }

            ResetShips();
        }
        public void Load()
        {
            ShipPlayer.Load();
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Load();
            }
        }
        public void ResetShips()
        {
            ShipPlayer.Position.X = 4000;
            ShipPlayer.Position.Z = 4000;
            ShipPlayer.Angle = MathF.PI * 0.25f;
            ShipPlayer.Health = ShipPlayer.MaxHealth;
            ShipPlayer.Destroyed = false;
            ShipPlayer.Active = true;

            for (int i = 1; i < ShipsCount; i++)
            {
                Random random = new Random();

                Ships[i].Position = SpawnPoints[random.Next(0, SpawnPoints.Length)];

                Ships[i].Position.X += random.Next(-ShipsSeparation, ShipsSeparation);
                Ships[i].Position.Z += random.Next(-ShipsSeparation, ShipsSeparation);

                Ships[i].Health = Ships[i].MaxHealth;
                Ships[i].Destroyed = false;
                Ships[i].Active = true;
            }
        }
        public void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem, WeaponSystem weaponSystem, Camera activeCamera, bool crosshair)
        {
            Environment = environment;
            EffectSystem = effectSystem;
            ShipPlayer.Update(gameTime, Environment, EffectSystem, weaponSystem, activeCamera, crosshair);
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Update(gameTime, Environment, EffectSystem, weaponSystem, activeCamera, crosshair);
            }
        }
        public void Draw(Camera activeCamera, RenderState renderState, MapEnvironment environment)
        {
            ShipPlayer.Draw(activeCamera, renderState, environment);
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Draw(activeCamera, renderState, environment);
            }
        }
    }
}
