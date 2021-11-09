using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        public float Angle = 0;

        public float Velocity = 0;
        public float MaxVelocity = 5;
        public float Acceleration = 0.2f;

        public float AngularVelocity = 0f;
        public float MaxAngularVelocity = 0.005f;
        public float AngularAcceleration = 0.002f;

        public Ship(ContentManager content, GraphicsDevice graphics)
        {
            Content = content;
            Graphics = graphics;

            if (Ship.TexturesA.Albedos == null)
                Ship.TexturesA.Albedos = new List<Texture2D>();

            if (Ship.TexturesB.Albedos == null)
                Ship.TexturesB.Albedos = new List<Texture2D>();
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
