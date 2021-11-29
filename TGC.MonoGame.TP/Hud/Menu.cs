using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace TGC.MonoGame.TP.Hud
{
    public class Menu
    {
        private ContentManager Content;
        private GraphicsDevice Graphics;
        private TGCGame Game;

        // Main Menu
        private Button PlayButton;
        private Button MusicButton;
        private Button ExitButton;

        private Color ButtonColor = new Color(0f, 1f, 0f);
        private Vector2 MinButtonSize = new Vector2(200, 0);
        private Vector2 ButtonPadding = new Vector2(20, 10);

        private Song[] Songs;

        public Menu(GraphicsDevice graphics, ContentManager content, TGCGame game)
        {
            Graphics = graphics;
            Content = content;
            Game = game;

            Songs = new Song[]
            {
                Content.Load<Song>(TGCGame.ContentFolderMusic + "erika"),
                Content.Load<Song>(TGCGame.ContentFolderMusic + "over_there"),
                Content.Load<Song>(TGCGame.ContentFolderMusic + "sacred_war")
            };

            PlayButton = new Button(Content, Graphics, "Jugar", new Vector2(50, 500), false, ButtonPadding, MinButtonSize, ButtonColor);
            MusicButton = new Button(Content, Graphics, "Cambiar Musica", new Vector2(50, 420), false, ButtonPadding, MinButtonSize, ButtonColor);
            ExitButton = new Button(Content, Graphics, "Salir", new Vector2(50, 340), false, ButtonPadding, MinButtonSize, ButtonColor);

            Random random = new Random();

            MediaPlayer.IsShuffled = true;
            MediaPlayer.Volume = 0.1f;
            MediaPlayer.Play(Songs[random.Next(Songs.Length)]);
            MediaPlayer.MoveNext();
        }
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (Game.GameStatus)
            {
                case TGCGame.GameState.Dead:
                case TGCGame.GameState.Menu:
                    Game.IsMouseVisible = true;
                    MediaPlayer.Volume += deltaTime / 15;
                    MediaPlayer.Volume = Math.Clamp(MediaPlayer.Volume, 0, 0.5f);
                    break;
                case TGCGame.GameState.Playing:
                    MediaPlayer.Volume -= deltaTime / 15;
                    MediaPlayer.Volume = Math.Clamp(MediaPlayer.Volume, 0, 0.5f);
                    Game.IsMouseVisible = false;
                    break;
            }

            UpdateMainMenu(gameTime);
        }
        private void UpdateMainMenu(GameTime gameTime)
        {
            if (Game.GameStatus != TGCGame.GameState.Menu)
                return;

            if (PlayButton.Click())
            {
                Game.GameStatus = TGCGame.GameState.Playing;
            }
            if (MusicButton.Click())
            {
                Random random = new Random();
                MediaPlayer.Play(Songs[random.Next(Songs.Length)]);
            }
            if (ExitButton.Click())
            {
                Game.Exit();
            }
        }
        public void Draw(GameTime gameTime)
        {
            switch(Game.GameStatus)
            {
                case TGCGame.GameState.Menu:
                    DrawMainMenu(gameTime);
                    break;
            }
        }
        private void DrawMainMenu(GameTime gameTime)
        {
            PlayButton.Draw(gameTime);
            MusicButton.Draw(gameTime);
            ExitButton.Draw(gameTime);
        }
    }
}
