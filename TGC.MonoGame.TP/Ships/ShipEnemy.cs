using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Shooting
    }
    class ShipEnemy : ShipB
    {
        private Vector3[] WayPoints;
        private Vector3 WayPointVariation;
        private int CurrentWayPoint;
        private float WayPointRadius = 1000;

        private EnemyState EnemyState;
        private ShipPlayer ShipPlayer;
        private WeaponSystem WeaponSystem;

        private float ShootingRange = 4000;
        private float ChaseRange = 6000;

        private float VelocityLerp = 0.01f;
        private float AngleLerp = 0.005f;

        private float PreviousAngle = 0f;

        private float FireRate = 3f;
        private float FireTime = 0f;

        public ShipEnemy(ContentManager content, GraphicsDevice graphics, ShipPlayer shipPlayer, Gizmos gizmos) : base(content, graphics, gizmos)
        {
            ShipPlayer = shipPlayer;

            WayPoints = new Vector3[]
            {
                new Vector3(14000, 0, 10000),
                new Vector3(16000, 0, 0),
                new Vector3(14000, 0, -12000),
                new Vector3(6000, 0, -10000),
                new Vector3(0, 0, -10000),
                new Vector3(-8000, 0, -12000),
                new Vector3(-14000, 0, -2000),
                new Vector3(-10000, 0, 9000),
                new Vector3(0, 0, 8000),
                new Vector3(6000, 0, 10000),
            };
            CurrentWayPoint = 0;
            Health = 100;
        }
        public override void Load()
        {
            base.Load();

            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
            BoundingBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            BoundingBox.Extents = new Vector3(50, 50, 200);
        }

        private float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        private void StateManagement(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            var distanceToPlayer = Vector3.Distance(ShipPlayer.Position, Position);

            if (distanceToPlayer < ShootingRange && ShipPlayer.Health > 0)
            {
                EnemyState = EnemyState.Shooting;
            }
            else if (distanceToPlayer > ShootingRange && distanceToPlayer < ChaseRange && ShipPlayer.Health > 0)
            {
                EnemyState = EnemyState.Chasing;
            }
            else
            {
                EnemyState = EnemyState.Patrolling;
            }
        }

        private float RotateToTarget(Vector3 target)
        {
            var angleToFollow = MathF.Atan2(target.X, target.Z);

            if (angleToFollow - PreviousAngle > MathF.PI)
            {
                angleToFollow -= MathF.PI * 2;
            }
            else if (angleToFollow - PreviousAngle < -MathF.PI)
            {
                angleToFollow += MathF.PI * 2;
            }

            PreviousAngle = angleToFollow;

            return angleToFollow;
        }

        private void Wandering(GameTime gameTime)
        {
            var target = Position - (WayPoints[CurrentWayPoint] + WayPointVariation);
            var distanceToWayPoint = Vector3.Distance(Position, WayPoints[CurrentWayPoint] + WayPointVariation);
            var angleToFollow = RotateToTarget(target);

            Velocity = Lerp(Velocity, 3f, VelocityLerp);
            Angle = Lerp(Angle, angleToFollow, AngleLerp * Velocity / 10f);

            if (distanceToWayPoint < WayPointRadius)
            {
                Random random = new Random();
                float variation = (float)random.NextDouble() * 2000 - 1000;
                WayPointVariation = new Vector3(variation, 0f, variation);

                CurrentWayPoint++;
                if(CurrentWayPoint >= WayPoints.Length)
                {
                    CurrentWayPoint = 0;
                }
            }
        }
        private void Chasing(GameTime gameTime)
        {
            var target = Position - ShipPlayer.Position;
            var angleToFollow = RotateToTarget(target);

            Velocity = Lerp(Velocity, 7f, VelocityLerp);
            Angle = Lerp(Angle, angleToFollow, AngleLerp * Velocity / 10f);
        }
        private void Shooting(GameTime gameTime)
        {
            var target = Position - ShipPlayer.Position;
            var angleToFollow = RotateToTarget(target);

            Velocity = Lerp(Velocity, 5f, VelocityLerp);
            Angle = Lerp(Angle, angleToFollow, AngleLerp * Velocity / 10f);

            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            if(time - FireTime > FireRate)
            {
                var random = new Random();

                FireTime = time;

                var innacuracy = Vector3.One * ((float)random.NextDouble() * 200 - 100);
                var offset = World.Forward * 200 + new Vector3(0, 50, 0);
                var fireTarget = (ShipPlayer.World.Translation + new Vector3(0, 200, 0) + innacuracy + ShipPlayer.Rotation.Forward * ShipPlayer.Velocity * 100f) - World.Translation;

                WeaponSystem.Fire(World.Translation + offset, fireTarget, this);
            }

        }

        private void MovementManagement(GameTime gameTime)
        {
            switch (EnemyState)
            {
                case EnemyState.Patrolling:
                    Wandering(gameTime);
                    break;

                case EnemyState.Chasing:
                    Chasing(gameTime);
                    break;

                case EnemyState.Shooting:
                    Shooting(gameTime);
                    break;
                    
            }
        }

        public override void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem, WeaponSystem weaponSystem, Camera activeCamera)
        {
            WeaponSystem = weaponSystem;

            HealthController(gameTime, effectSystem);

            (Vector3, Vector3) result = environment.Ocean.WaveNormalPosition(Position, gameTime);

            Vector3 normal = result.Item1;
            Vector3 position = result.Item2;

            OceanPosition = position;

            if (!Destroyed)
            {
                StateManagement(gameTime, environment, effectSystem);
                MovementManagement(gameTime);

                Position += Rotation.Forward * Velocity;
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
        public override void Draw(Matrix view, Matrix proj)
        {
            if (!Active)
                return;
            Gizmos.DrawCube(BoundingBoxMatrix, Color.Blue);

            base.Draw(view, proj);
        }
    }
}
