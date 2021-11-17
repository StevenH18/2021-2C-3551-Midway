using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Cameras
{
    class AimingCamera : Camera
    {
        private GameWindow Window;
        private GraphicsDevice Graphics;

        public float MouseSensitivity = 0.4f;

        private Vector3 Offset = new Vector3(0, 100, 0);
        public float YawAngles = 0f;
        public float PitchAngles = 0f;

        public float Zoom = 3f;
        public float MaxZoom = 14f;
        public float MinZoom = 3f;

        private MouseState PreviousMouseState;
        private int PreviousScroll = 0;

        public AimingCamera(GraphicsDevice gfxDevice, GameWindow window)
        {
            Window = window;
            Graphics = gfxDevice;

            RecreateProjection();
        }
        private void RecreateProjection()
        {
            float aspectRatio = Graphics.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(Zoom, aspectRatio, 0.1f, 100000f);
        }
        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        private void Controls(GameTime gameTime, Ship ship)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var mouseState = Mouse.GetState();

            Vector2 diff = mouseState.Position.ToVector2() - PreviousMouseState.Position.ToVector2();
            YawAngles = Lerp(YawAngles, YawAngles - diff.X * deltaTime * MouseSensitivity, 0.1f);
            PitchAngles = Lerp(PitchAngles, PitchAngles - diff.Y * deltaTime * MouseSensitivity, 0.1f);

            PitchAngles = (float)Math.Clamp(PitchAngles, -Math.PI / 2, Math.PI / 2);

            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
            mouseState = Mouse.GetState();

            var scrollDiff = PreviousScroll - Mouse.GetState().ScrollWheelValue;

            if (MathF.Abs(scrollDiff) > 0)
            {
                Zoom += MathF.Sign(scrollDiff) / (MathF.PI * 2f);
            }

            Zoom = Math.Clamp(Zoom, MathF.PI / MaxZoom, MathF.PI / MinZoom);
            RecreateProjection();

            PreviousMouseState = mouseState;
            PreviousScroll = Mouse.GetState().ScrollWheelValue;

            World = Matrix.CreateFromYawPitchRoll(YawAngles, PitchAngles, 0f) * ship.Rotation * Matrix.CreateTranslation(ship.World.Translation + Offset);
        }

        public override void Update(GameTime gameTime, Ship ship)
        {
            Controls(gameTime, ship);

            World = Matrix.CreateWorld(World.Translation, World.Forward, Vector3.Up);
            View = Matrix.CreateLookAt(World.Translation, World.Forward + World.Translation, Vector3.Up);
        }
    }
}
