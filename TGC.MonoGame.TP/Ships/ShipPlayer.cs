using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Artillery;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public class ShipPlayer : ShipA
    {
        private float FireRate = 1f;
        private float FireTime = 0f;

        public ShipPlayer(ContentManager content, GraphicsDevice graphics, Gizmos gizmos) : base(content, graphics, gizmos)
        {
            Health = 300;
        }

        public override void Load()
        {
            base.Load();

            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            BoundingBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            BoundingBox.Extents = new Vector3(20,50,200);
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
                AngularVelocity -= AngularAcceleration * time;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                AngularVelocity += AngularAcceleration * time;
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (Math.Abs(Velocity) > 0.003f)
                {
                    Velocity -= Math.Sign(Velocity) * Acceleration * time;
                }
                else
                {
                    Velocity = 0;
                }
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.D) && !Keyboard.GetState().IsKeyDown(Keys.A)) 
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

            Velocity = Math.Clamp(Velocity, -MaxVelocity, MaxVelocity);
            AngularVelocity = Math.Clamp(AngularVelocity, -MaxAngularVelocity, MaxAngularVelocity) * MathF.Pow(Math.Abs(Velocity / MaxVelocity), 0.2f);
        }

        public void CollisionDetection(MapEnvironment environment)
        {
            for(int i = 0; i < environment.IslandSystem.IslandColliders.Count; i++)
            {
                var islandCollider = environment.IslandSystem.IslandColliders[i];

                if(BoundingBox.Intersects(islandCollider))
                {
                    Health = 0;
                }
            }
        }

        public override void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem, WeaponSystem weaponSystem, Camera activeCamera)
        {
            HealthController(gameTime, effectSystem);
            CollisionDetection(environment);

            (Vector3, Vector3) result = environment.Ocean.WaveNormalPosition(Position, gameTime);

            Vector3 normal = result.Item1;
            Vector3 position = result.Item2;

            OceanPosition = position;

            if (!Destroyed)
            {
                // Fire
                if (Inputs.mouseLeftJustPressed() && Mouse.GetState().RightButton == ButtonState.Pressed)
                {
                    float time = (float)gameTime.TotalGameTime.TotalSeconds;

                    if (time - FireTime > FireRate)
                    {
                        FireTime = time;

                        weaponSystem.Fire(activeCamera.World.Translation, activeCamera.World.Forward * 5000, this);
                    }
                }

                PlayerControl(gameTime);
                Position += Rotation.Forward * Velocity;
                Angle += AngularVelocity;

                Position.Y = 0;

                Rotation = Matrix.CreateFromYawPitchRoll(0f, normal.Z, -normal.X) * Matrix.CreateFromAxisAngle(normal, Angle);
            }

            DestroyAnimation(gameTime);

            World = Scale * Rotation * Matrix.CreateTranslation(OceanPosition);

            BoundingBox.Center = OceanPosition;

            BoundingBoxMatrix = Matrix.CreateScale(BoundingBox.Extents * 2f) *
                 Rotation *
                 Matrix.CreateTranslation(OceanPosition);

        }

        public override void Draw(Matrix view, Matrix proj, Matrix cameraWorld, RenderState renderState, MapEnvironment environment)
        {
            if (!Active)
                return;
            Gizmos.DrawCube(BoundingBoxMatrix, Color.Green);

            base.Draw(view, proj, cameraWorld, renderState, environment);
        }
    }
}
