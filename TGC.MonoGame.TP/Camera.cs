using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace TGC.MonoGame.TP
{
    class Camera
    {
        public Matrix Projection;
        public Matrix View;
        private Vector3 followPositon;
        private Vector3 Position;

        private Matrix Rotation;
        private Matrix Follow;
        private float NearPlaneDistance;
        private float FarPlaneDistance;
        private float AspectRatio;
        private float FOV;
        private Vector2 MousePosition;
        public Camera(float GradosApertura,GraphicsDevice GraphicsDevice, float NearPlaneDistance, float FarPlaneDistance)
        {
            this.AspectRatio = GraphicsDevice.Viewport.AspectRatio ;
            this.FOV = GradosApertura * MathHelper.Pi / 180f; 
            this.NearPlaneDistance = NearPlaneDistance;
            this.FarPlaneDistance = FarPlaneDistance;

            Position = Vector3.Zero;
            followPositon = Vector3.One;
            Follow = Matrix.CreateTranslation(followPositon);  
            Rotation = Matrix.CreateRotationX(3) * Matrix.CreateRotationY(32) * Matrix.CreateRotationZ(42);

            View = Matrix.CreateLookAt(Position, Follow.Translation, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FOV,AspectRatio, NearPlaneDistance, FarPlaneDistance);

            MousePosition = Mouse.GetState().Position.ToVector2();
        }

        
        public void Update(GameTime GameTime)
        {
            var keyState = Keyboard.GetState();
            var MouseState = Mouse.GetState();
            float avance = 0;
            float costado = 0;
            float camSpeed = 80;
            float time = (float)GameTime.ElapsedGameTime.TotalSeconds;
           
            if (keyState.IsKeyDown(Keys.W)){
                avance = 1;
                /*Position.Z += camSpeed * time;
                PositionFollow.Z += camSpeed * time;*/
            }

            if (keyState.IsKeyDown(Keys.S)){
                avance = -1;
                /*Position.Z -= camSpeed * time;
                PositionFollow.Z -= camSpeed * time;*/
            }

            if (keyState.IsKeyDown(Keys.A)){
                costado = -1;
                /*Position.X += camSpeed * time;
                PositionFollow.X += camSpeed * time;*/
            }

            if (keyState.IsKeyDown(Keys.D)){
                costado = 1;
                /*Position.X -= camSpeed * time;
                PositionFollow.X -= camSpeed * time;*/
            }

            if (Inputs.isJustPressed(Keys.E))
            {
                FarPlaneDistance += 1000;
                Projection = Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlaneDistance, FarPlaneDistance);
            }
            if (Inputs.isJustPressed(Keys.Q))
            {
                FarPlaneDistance =  Math.Max(FarPlaneDistance - 1000,100);
                Projection = Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlaneDistance, FarPlaneDistance);
            }

            var versor = (followPositon - Position);
            versor.Normalize();
            float turbo = 1;
            if (keyState.IsKeyDown(Keys.LeftShift))
            {
                turbo = 3;
            }

            if (MouseState.Position.X > MousePosition.X)
            {
                //Rotation *= Matrix.CreateRotationZ(-time * camSpeed );

               followPositon.X -= camSpeed * time * 0.1f;
            }
            if (MouseState.Position.X < MousePosition.X)
            {
                //Rotation *= Matrix.CreateRotationZ(time * camSpeed);

                followPositon.X += camSpeed * time * 0.1f;
            }
            if (MouseState.Position.Y > MousePosition.Y)
            {
                //Rotation *= Matrix.CreateRotationX(time * camSpeed);
                followPositon.Y -= camSpeed * time * 0.1f;
            }
            if (MouseState.Position.Y < MousePosition.Y)
            {
                
               // Rotation *= Matrix.CreateRotationX(time * camSpeed);
                followPositon.Y += camSpeed * time * 0.1f;
            }
            MousePosition = MouseState.Position.ToVector2();

            

            var move = versor * avance * camSpeed * time * turbo + Vector3.Cross(versor,Vector3.Up) * costado * camSpeed * time * turbo;

            Position += move;

            followPositon += move;

            Follow = Rotation * Matrix.CreateTranslation(followPositon) ;

            View = Matrix.CreateLookAt(Position, followPositon, Vector3.Up);
        }

    }
}
