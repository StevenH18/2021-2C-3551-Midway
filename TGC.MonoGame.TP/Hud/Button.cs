using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Hud
{
    public class Button
    {
        private ContentManager Content;
        private GraphicsDevice Graphics;

        private ScreenQuad ScreenQuad;
        private Effect ButtonEffect;
        private SpriteBatch SpriteBatch;
        private SpriteFont Font;

        public String Text;
        public Vector2 Size;
        public Vector2 Position;
        public Vector2 Padding;
        public Vector2 MinSize;
        public Vector2 TextSize;
        public Color Color;

        public float HoverProgress;
        public float AnimationDuration = 0.1f;

        public Button(ContentManager content, GraphicsDevice graphics, String text, Vector2 position, bool centered, Vector2 padding, Vector2 minSize, Color color)
        {
            Graphics = graphics;
            Content = content;
            Text = text;
            Padding = padding;
            Position = position;
            MinSize = minSize;
            Color = color;

            SpriteBatch = new SpriteBatch(Graphics);
            Font = Content.Load<SpriteFont>("Fonts/Basic");

            CalculateButtonSize();

            if(centered)
            {
                Center();
            }

            ScreenQuad = new ScreenQuad(Graphics, new Vector3(Position.X, Position.Y, 0), new Vector3(Size.X, Size.Y, 0));
            ButtonEffect = Content.Load<Effect>(TGCGame.ContentFolderEffects + "ButtonShader");
        }
        public void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            ButtonEffect.Parameters["AspectRatio"]?.SetValue(Size.X / Size.Y);
            ButtonEffect.Parameters["Color"]?.SetValue(Color.ToVector4());
            ButtonEffect.Parameters["Hover"]?.SetValue(Hover());
            ButtonEffect.Parameters["HoverProgress"]?.SetValue(HoverProgress);
            ButtonEffect.Parameters["Time"]?.SetValue(time);

            HoverProgressUpdate(gameTime);

            SpriteBatch.Begin();
            Vector2 textPosition = new Vector2(Position.X, Graphics.Viewport.Height - Position.Y - Size.Y);
            textPosition += (Size - TextSize) / 2;
            SpriteBatch.DrawString(Font, Text, textPosition, Color);
            ScreenQuad.Draw(ButtonEffect);
            SpriteBatch.End();
        }
        public bool Hover()
        {
            Vector2 mousePosition = Mouse.GetState().Position.ToVector2();
            mousePosition.Y = Graphics.Viewport.Height - mousePosition.Y;

            return mousePosition.X >= Position.X && mousePosition.X <= Position.X + Size.X && mousePosition.Y >= Position.Y && mousePosition.Y <= Position.Y + Size.Y;
        }
        public bool Pressed()
        {
            return Hover() && Inputs.mouseLeftJustPressed();
        }
        public bool Click()
        {
            return Hover() && Mouse.GetState().LeftButton == ButtonState.Pressed;
        }
        private void CalculateButtonSize()
        {
            TextSize = Font.MeasureString(Text);
            Size = TextSize + Padding * 2;
            Size = Vector2.Max(Size, MinSize);
        }
        private void Center()
        {
            Position.X = (Graphics.Viewport.Width - Size.X) / 2;
        }
        private void HoverProgressUpdate(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(Hover())
            {
                HoverProgress += deltaTime / AnimationDuration;
            }
            else
            {
                HoverProgress -= deltaTime / AnimationDuration;
            }
            HoverProgress = Math.Clamp(HoverProgress, 0, 1);
        }
    }
}
