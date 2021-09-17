using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Controller;

namespace TGC.MonoGame.TP.Ships
{
    public abstract class Ship
    {
        protected ContentManager Content;
        public Matrix World { get; set; }
        protected Model Model { get; set; }
        protected Effect Effect { get; set; }
        protected Matrix Scale;
        public Matrix Rotation;
        public Vector3 Position;


        private float rotation = 0;
        private Ocean ocean;
        Vector3 originalPos;
        private Color color;

        //
        private float aceleration = 0.02f;
        private float speed = 0;
        private const float turningSpeed = 0.07f;
        private const float Maxspeed = 5;

        public Ship(ContentManager content, Ocean ocean,Color color)
        {
            this.ocean = ocean;
            this.Content = content;
            this.color = color;

            originalPos = new Vector3();


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
            originalPos = Position;
        }
        public void Update(GameTime gameTime,Controll control)
        {
            var time = (float) gameTime.ElapsedGameTime.TotalSeconds;
            speed = Math.Max(Math.Min(speed + aceleration * control.avanzar, Maxspeed),0);
            
            if (control.avanzar == 0)
            {
                var retroceso = -1 * Math.Sign(speed);
                speed += retroceso * aceleration ;
                if (speed / retroceso > 0)
                    speed = 0;
            }


            rotation += time * control.virar * turningSpeed * speed;
           
            originalPos += Vector3.Transform(Vector3.Forward, Rotation) * speed;

            flotar(gameTime);

            World = Scale * Rotation * Matrix.CreateTranslation(Position);
            
        }
       
        private void flotar(GameTime gameTime)
        {

            (Vector3, Vector3) result = ocean.WaveNormalPosition(originalPos, gameTime);
            Vector3 normal = result.Item1;
            Vector3 position = result.Item2;

            // MAGIA MAGIA MAGIA NEGRA !!!!!!!!!!!!!!!!!!!
            Rotation = Matrix.CreateFromYawPitchRoll(0f, normal.Z, -normal.X) * Matrix.CreateFromAxisAngle(normal, rotation);

            originalPos.Y = position.Y;
            Position = position;
        }

        public void Draw(Matrix view, Matrix proj)
        {

            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(proj);
            Effect.Parameters["DiffuseColor"].SetValue(color.ToVector3());

            foreach (var mesh in Model.Meshes)
            {

                var w = mesh.ParentBone.Transform * World;

                Effect.Parameters["World"].SetValue(w);

                mesh.Draw();
            }

        }

    }
}
