using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Cameras
{
    public class DefeatedCamera : Camera
    {
        private GameWindow Window;
        private GraphicsDevice Graphics;

        private float Angle = 0f;

        public DefeatedCamera(GraphicsDevice gfxDevice, GameWindow window)
        {
            Window = window;
            Graphics = gfxDevice;

            RecreateProjection();
        }
        private void RecreateProjection()
        {
            float aspectRatio = Graphics.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 3f, aspectRatio, 0.1f, 100000f);
        }
        public override void Update(GameTime gameTime, Ship ship)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Angle += deltaTime * 0.2f;

            var position = ship.Position + new Vector3(MathF.Cos(Angle) * 2000, 700, MathF.Sin(Angle) * 2000);

            World = Matrix.CreateWorld(position, position + ship.World.Translation, Vector3.Up);
            View = Matrix.CreateLookAt(position, ship.World.Translation, Vector3.Up);
        }
    }
}
