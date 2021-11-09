using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipPlayer : ShipA
    {
        public ShipPlayer(ContentManager content, GraphicsDevice graphics) : base(content, graphics)
        {

        }

        private void PlayerControl(GameTime gameTime)
        {
            var time = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Velocity += Acceleration * time;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Velocity -= Acceleration * time;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                AngularVelocity -= AngularAcceleration * (Velocity / MaxVelocity) * time;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                AngularVelocity += AngularAcceleration * (Velocity / MaxVelocity) * time;
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (Math.Abs(Velocity) > 0.003f && !Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    Velocity -= Math.Sign(Velocity) * Acceleration * time;
                }
                else
                {
                    Velocity = 0;
                }
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.D) && !Keyboard.GetState().IsKeyDown(Keys.A) || Math.Abs(Velocity) < 2f) 
            { 
                if (Math.Abs(AngularVelocity) > 0.0003f)
                {
                    AngularVelocity -= Math.Sign(AngularVelocity) * AngularAcceleration * time;
                }
                else
                {
                    AngularVelocity = 0;
                }
            }

            Velocity = Math.Clamp(Velocity, -MaxVelocity * 0.5f, MaxVelocity);
            AngularVelocity = Math.Clamp(AngularVelocity, -MaxAngularVelocity, MaxAngularVelocity);
        }

        public override void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            PlayerControl(gameTime);

            Position += Rotation.Forward * Velocity;
            Angle += AngularVelocity;

            Position.Y = 0;

            (Vector3, Vector3) result = environment.Ocean.WaveNormalPosition(Position, gameTime);

            Vector3 normal = result.Item1;
            Vector3 position = result.Item2;

            Rotation = Matrix.CreateFromYawPitchRoll(0f, normal.Z, -normal.X) * Matrix.CreateFromAxisAngle(normal, Angle);
            World = Scale * Rotation * Matrix.CreateTranslation(position);
        }
    }
}
