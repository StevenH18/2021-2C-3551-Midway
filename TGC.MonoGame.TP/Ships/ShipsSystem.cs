using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Controller;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipsSystem
    {
        private ContentManager Content;
        private MapEnvironment Environment;
        public EffectSystem EffectSystem;

        public int ShipsCount = 2;
        public int ShipsSeparation = 15000;
        public Ship[] Ships;

        public ShipsSystem(ContentManager content)
        {
            Content = content;

            Ships = new Ship[ShipsCount];

            Ships[0] = new ShipA(Content, Color.Gray);
            Player player = new Player();
            Ships[0].Controller = player;

            Ships[0].Position.X = 1;
            Ships[0].Position.Z = 1;

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
        }
        public void Load()
        {
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Load();
            }
        }
        public void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            Environment = environment;
            EffectSystem = effectSystem;
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Update(gameTime, Environment, EffectSystem);
            }
        }
        public void Draw(Matrix view, Matrix proj)
        {
            for (int i = 0; i < ShipsCount; i++)
            {
                Ships[i].Draw(view, proj);
            }
        }
    }
}
