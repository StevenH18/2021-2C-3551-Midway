using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TGC.MonoGame.TP.Ships;

namespace TGC.MonoGame.TP.Cameras
{
    public class MenuCamera : Camera
    {
        private GameWindow Window;
        private GraphicsDevice Graphics;

        public Vector3 Position;
        public Vector3 Forward;

        public MenuCamera(GraphicsDevice gfxDevice, GameWindow window)
        {
            Window = window;
            Graphics = gfxDevice;

            RecreateProjection();
        }
        private void RecreateProjection()
        {
            float aspectRatio = Graphics.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 8f, aspectRatio, 0.1f, 100000f);
        }
        public override void Update(GameTime gameTime, Ship ship, TGCGame game)
        {
            World = Matrix.CreateWorld(Position, Forward, Vector3.Up);
            View = Matrix.CreateLookAt(Position, Position + Forward, Vector3.Up);
        }
    }
}
