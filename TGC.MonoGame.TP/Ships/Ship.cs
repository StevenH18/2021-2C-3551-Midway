using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Collisions;
using TGC.MonoGame.Samples.Viewer.Gizmos;
using TGC.MonoGame.TP.Effects;
using TGC.MonoGame.TP.Environment;

namespace TGC.MonoGame.TP.Ships
{
    public abstract class Ship
    {
        protected ContentManager Content;
        protected GraphicsDevice Graphics;
        protected Model Model;
        protected Effect Effect;
        protected Gizmos Gizmos;

        public OrientedBoundingBox BoundingBox;
        public Matrix BoundingBoxMatrix;

        protected struct TexturesShipA
        {
            public List<Texture2D> Albedos;
        }
        protected struct TexturesShipB
        {
            public List<Texture2D> Albedos;
        }
        protected static TexturesShipA TexturesA;
        protected static TexturesShipB TexturesB;

        public Matrix Scale;
        public Matrix Rotation;
        public Matrix World;

        public Vector3 Position;
        public Vector3 OceanPosition;
        public float Angle = 0;

        public float Velocity = 0;
        public float MaxVelocity = 5;
        public float Acceleration = 0.5f;

        public float AngularVelocity = 0f;
        public float MaxAngularVelocity = 0.005f;
        public float AngularAcceleration = 0.02f;

        public float Health;
        public bool Destroyed;
        public float DestroyedTime;
        public bool Active = true;

        public Ship(ContentManager content, GraphicsDevice graphics, Gizmos gizmos)
        {
            Content = content;
            Graphics = graphics;
            Gizmos = gizmos;

            if (Ship.TexturesA.Albedos == null)
                Ship.TexturesA.Albedos = new List<Texture2D>();

            if (Ship.TexturesB.Albedos == null)
                Ship.TexturesB.Albedos = new List<Texture2D>();
        }

        public void Damage(float hitPoints)
        {
            Health -= hitPoints;
        }

        protected void HealthController(GameTime gameTime, EffectSystem effectSystem)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            if(Health <= 0 && !Destroyed)
            {
                Destroyed = true;
                DestroyedTime = time;
                effectSystem.CreateExplosion(World.Translation + new Vector3(0, 250, 0));
            }
        }

        protected void DestroyAnimation(GameTime gameTime)
        {
            if (!Destroyed)
                return;

            float time = (float)gameTime.TotalGameTime.TotalSeconds;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (time - DestroyedTime < 20)
            {
                Rotation = Rotation * Matrix.CreateFromAxisAngle(Rotation.Right, 0.05f * deltaTime * (1 - (time - DestroyedTime) / 20));
                OceanPosition.Y -= (time - DestroyedTime) * 20;
            }
            else
            {
                Active = false;
            }
        }

        public virtual void Load()
        {
            Effect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "BasicShader");

            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }
        }
        public virtual void Update(GameTime gameTime, MapEnvironment environment, EffectSystem effectSystem)
        {
            World = Scale * Rotation * Matrix.CreateTranslation(Position);
        }

        public virtual void Draw(Matrix view, Matrix proj)
        {

        }
    }
}
