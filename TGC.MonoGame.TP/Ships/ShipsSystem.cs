using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipsSystem
    {
        private ContentManager Content;
        private GraphicsDevice Graphics;
        private MapEnvironment Environment;
        public EffectSystem EffectSystem;

        public int ShipsCount = 1;
        public int ShipsSeparation = 15000;
        public Ship[] Ships;
        public Ship ShipPlayer;

        public ShipsSystem(ContentManager content, GraphicsDevice graphics)
        {
            Content = content;
            Graphics = graphics;

            Ships = new Ship[ShipsCount];

            Ships[0] = new ShipPlayer(Content, Graphics);
            ShipPlayer = Ships[0];

            ShipPlayer.Position.X = 1;
            ShipPlayer.Position.Z = 1;

            /*
            for (int i = 1; i < ShipsCount; i++)
            {
                Random random = new Random();

                if(random.Next(2) == 0)
                    Ships[i] = new ShipA(Content, Color.Gray);
                else
                    Ships[i] = new ShipB(Content, Color.Black);

                CpuPlayer cpu = new CpuPlayer
                {
                    Ship = Ships[i]
                };

                Ships[i].Position.X = random.Next(-ShipsSeparation, ShipsSeparation);
                Ships[i].Position.Z = random.Next(-ShipsSeparation, ShipsSeparation);

                Ships[i].Controller = cpu;
            }
            */
        }
        public void Load()
        {
            ShipPlayer.Load();
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Load();
            }
        }
        public void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            Environment = environment;
            EffectSystem = effectSystem;
            ShipPlayer.Update(gameTime, Environment, EffectSystem);
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Update(gameTime, Environment, EffectSystem);
            }
        }
        public void Draw(Matrix view, Matrix proj)
        {
            ShipPlayer.Draw(view, proj);
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Draw(view, proj);
            }
        }
    }
}
