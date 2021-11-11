using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Artillery
{
    public class Shell
    {
        private ContentManager Content;
        private GraphicsDevice Graphics;
        private EffectSystem EffectSystem;
        private MapEnvironment Environment;
        private ShipsSystem ShipsSystem;
        private Gizmos Gizmos;

        private Model Model;
        private Effect Effect;

        private Vector3 Position;
        private Vector3 Velocity;
        private Vector3 Gravity = new Vector3(0, -500, 0);

        private Matrix World;

        private BoundingSphere BoundingSphere;

        public float Damage = 65;

        public bool Active = false;

        public Shell(GraphicsDevice graphics, ContentManager content, EffectSystem effectSystem, MapEnvironment environment, ShipsSystem shipsSystem, Gizmos gizmos)
        {
            Content = content;
            Graphics = graphics;
            EffectSystem = effectSystem;
            Environment = environment;
            ShipsSystem = shipsSystem;
            Gizmos = gizmos;

            Model = Content.Load<Model>(TGCGame.ContentFolder3D + "Weapons/Shell");
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "ShellShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }

            BoundingSphere = new BoundingSphere();
        }
        public void Fire(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
            Active = true;
        }
        public void MRUV(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Velocity += Gravity * deltaTime;
            Position += Velocity * deltaTime;
        }
        public void CollisionCheck(GameTime gameTime)
        {
            // Check if collided with ship
            Ship[] ships = ShipsSystem.Ships;
            for(var i = 0; i < ships.Length; i++)
            {
                if(ships[i].BoundingBox.Intersects(BoundingSphere))
                {
                    ships[i].Damage(Damage);
                    Active = false;
                }
            }

            Vector3 flatPosition = new Vector3(Position.X, 0, Position.Z);

            (Vector3, Vector3) ocean = Environment.Ocean.WaveNormalPosition(flatPosition, gameTime);

            Vector3 oceanPosition = ocean.Item2;

            if(Position.Y <= oceanPosition.Y)
            {
                Active = false;
                EffectSystem.CreateWaterSplash(flatPosition + new Vector3(0, 350 + oceanPosition.Y, 0));
            }
        }
        public void Update(GameTime gameTime)
        {
            if (!Active)
                return;

            BoundingSphere.Center = Position;
            BoundingSphere.Radius = 10;

            MRUV(gameTime);
            CollisionCheck(gameTime);

            World = Matrix.CreateWorld(Position, Vector3.Normalize(Velocity), Vector3.Up);
        }
        public void Draw(GameTime gameTime, Matrix view, Matrix proj)
        {
            if (!Active)
                return;

            foreach (var mesh in Model.Meshes)
            {
                var world = mesh.ParentBone.Transform * World;

                Effect.Parameters["World"]?.SetValue(world);
                Effect.Parameters["View"]?.SetValue(view);
                Effect.Parameters["Projection"]?.SetValue(proj);

                mesh.Draw();
            }

            Gizmos.DrawSphere(BoundingSphere.Center, Vector3.One * BoundingSphere.Radius, Color.Red);
        }
    }
}
