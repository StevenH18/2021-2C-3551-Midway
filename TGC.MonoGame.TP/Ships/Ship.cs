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

        private float Viraje = 0;
        private Ocean Ocean;
        Vector3 OriginalPos;
        protected Color Color;

        public float Aceleration = 0.02f;
        public float Speed = 0;
        public float TurningSpeed = 0.07f;
        public float MaxSpeed = 5;
        public Ship(ContentManager content, Ocean ocean,Color color)
        {
            this.Ocean = ocean;
            this.Content = content;
            this.Color = color;

            OriginalPos = Vector3.Zero;

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
            OriginalPos = Position;
        }
        public void Update(GameTime gameTime,Control control)
        {
            var time = (float) gameTime.ElapsedGameTime.TotalSeconds;
            Speed = Math.Max(Math.Min(Speed + Aceleration * control.Avanzar, MaxSpeed),-MaxSpeed);
            
            if (control.Avanzar == 0)
            {
                var retroceso = -1 * Math.Sign(Speed);
                Speed += retroceso * Aceleration ;
                if (Speed / retroceso > 0)
                    Speed = 0;
            }


            Viraje += time * control.Virar * TurningSpeed * Speed;
            //creo una linea con la inclinacion de la recta y y hago una resta
            Vector3 inclinacion = Vector3.Transform(Vector3.Forward, Rotation) * 2 - Vector3.Transform(Vector3.Forward, Rotation);

            if (inclinacion.Y > 0)
            {
                //color = Color.Red;
            }
            else if (inclinacion.Y < 0)
            {
               // color = Color.Green;
            }
            else
            {
                //color = Color.Yellow;
            }
            var potenciaInclinacion = 2;
            OriginalPos += Vector3.Transform(Vector3.Forward, Rotation) * Speed + Vector3.Transform(Vector3.Forward, Rotation) * -inclinacion.Y * potenciaInclinacion;

            flotar(gameTime);

            World = Scale * Rotation * Matrix.CreateTranslation(Position);
            
        }
       
        private void flotar(GameTime gameTime)
        {

            (Vector3, Vector3) result = Ocean.WaveNormalPosition(OriginalPos, gameTime);
            Vector3 normal = result.Item1;
            Vector3 position = result.Item2;

            // MAGIA MAGIA MAGIA NEGRA !!!!!!!!!!!!!!!!!!!
            Rotation = Matrix.CreateFromYawPitchRoll(0f, normal.Z, -normal.X) * Matrix.CreateFromAxisAngle(normal, Viraje);

            OriginalPos.Y = 0f;
            Position = position;
        }

        public virtual void Draw(Matrix view, Matrix proj)
        {

        }
    }
}
