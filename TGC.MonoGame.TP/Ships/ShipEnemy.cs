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
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        Shooting
    }
    class ShipEnemy : ShipB
    {
        private Vector3[] WayPoints;
        private int CurrentWayPoint;
        private int PreviousWayPoint;
        private EnemyState EnemyState;
        private ShipPlayer ShipPlayer;

        private float ShootingRange = 1000;
        private float ChaseRange = 5000;

        public ShipEnemy(ContentManager content, GraphicsDevice graphics, ShipPlayer shipPlayer) : base(content, graphics)
        {
            ShipPlayer = shipPlayer;

            WayPoints = new Vector3[]
            {
                new Vector3(15000, 0, 5000),
                new Vector3(15000, 0, -5000),
                new Vector3(20000, 0, 0)
            };
            CurrentWayPoint = 0;
        }

        private float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        private void StateManagement(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            var distanceToPlayer = Vector3.Distance(ShipPlayer.Position, Position);

            if (distanceToPlayer < ShootingRange)
            {
                EnemyState = EnemyState.Shooting;
            }
            else if (distanceToPlayer > ShootingRange && distanceToPlayer < ChaseRange)
            {
                EnemyState = EnemyState.Chasing;
            }
            else
            {
                EnemyState = EnemyState.Patrolling;
            }
        }

        private void Wandering()
        {
            var target = Position - WayPoints[CurrentWayPoint];
            var distanceToWayPoint = Vector3.Distance(Position, WayPoints[CurrentWayPoint]);

            Velocity = Lerp(Velocity, MaxVelocity, 0.001f);
            Angle = Lerp(Angle, MathF.Atan2(target.X, target.Z), 0.005f * Velocity / MaxVelocity);

            if (distanceToWayPoint < 500f)
            {
                CurrentWayPoint++;
                if(CurrentWayPoint >= WayPoints.Length)
                {
                    CurrentWayPoint = 0;
                }
            }
        }
        private void Chasing()
        {
            var target = Position - ShipPlayer.Position;

            Velocity = Lerp(Velocity, MaxVelocity, 0.001f);
            Angle = Lerp(Angle, MathF.Atan2(target.X, target.Z), 0.005f * Velocity / MaxVelocity);
        }
        private void Shooting()
        {
            var target = Position - ShipPlayer.Position;

            Velocity = Lerp(Velocity, 0f, 0.001f);
            Angle = Lerp(Angle, MathF.Atan2(target.X, target.Z), 0.005f * Velocity / MaxVelocity);
        }

        private void MovementManagement()
        {
            switch (EnemyState)
            {
                case EnemyState.Patrolling:
                    Wandering();
                    break;

                case EnemyState.Chasing:
                    Chasing();
                    break;

                case EnemyState.Shooting:
                    Shooting();
                    break;
            }
        }

        public override void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            StateManagement(gameTime, environment, effectSystem);
            MovementManagement();

            Position += Rotation.Forward * Velocity;
            Position.Y = 0;

            (Vector3, Vector3) result = environment.Ocean.WaveNormalPosition(Position, gameTime);

            Vector3 normal = result.Item1;
            Vector3 position = result.Item2;

            Rotation = Matrix.CreateFromYawPitchRoll(0f, normal.Z, -normal.X) * Matrix.CreateFromAxisAngle(normal, Angle);
            World = Scale * Rotation * Matrix.CreateTranslation(position);
        }
    }
}
