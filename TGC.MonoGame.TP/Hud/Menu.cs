using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int CurrentSong;

        private SpriteBatch SpriteBatch;
        private SpriteFont FontBig;
        private SpriteFont FontRegular;

        // Dead Menu
        private Button RestartButton;
        private Button MainMenuButton;

        public Menu(GraphicsDevice graphics, ContentManager content, TGCGame game)
        {
            Graphics = graphics;
            Content = content;
            Game = game;

            SpriteBatch = new SpriteBatch(Graphics);
            FontRegular = Content.Load<SpriteFont>("Fonts/Outfit");
            FontBig = Content.Load<SpriteFont>("Fonts/OutfitBig");

            Songs = new Song[]
            {
                Content.Load<Song>(TGCGame.ContentFolderMusic + "erika"),
                Content.Load<Song>(TGCGame.ContentFolderMusic + "over_there"),
                Content.Load<Song>(TGCGame.ContentFolderMusic + "sacred_war")
            };

            PlayButton = new Button(Content, Graphics, "JUGAR", new Vector2(50, 500), false, ButtonPadding, MinButtonSize, ButtonColor);
            MusicButton = new Button(Content, Graphics, "CAMBIAR MÚSICA", new Vector2(50, 500 - PlayButton.Size.Y + 2), false, ButtonPadding, MinButtonSize, ButtonColor);
            ExitButton = new Button(Content, Graphics, "SALIR", new Vector2(50, 500 - PlayButton.Size.Y * 2 + 4), false, ButtonPadding, MinButtonSize, ButtonColor);

            RestartButton = new Button(Content, Graphics, "REINICIAR", new Vector2(0, Graphics.Viewport.Height / 2), true, ButtonPadding, MinButtonSize, ButtonColor);
            MainMenuButton = new Button(Content, Graphics, "IR A MENU", new Vector2(0, Graphics.Viewport.Height / 2 - PlayButton.Size.Y + 2), true, ButtonPadding, MinButtonSize, ButtonColor);

            Random random = new Random();
            int playSong = random.Next(Songs.Length);

            MediaPlayer.IsShuffled = true;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.1f;
            MediaPlayer.Play(Songs[playSong]);
            CurrentSong = playSong;
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
                var songsPossible = new List<int>();

                for(int i = 0; i < Songs.Length; i++)
                {
                    if(i != CurrentSong)
                    {
                        songsPossible.Add(i);
                    }
                }

                Random random = new Random();
                int nextSong = random.Next(songsPossible.Count);

                MediaPlayer.Play(Songs[songsPossible[nextSong]]);
                CurrentSong = songsPossible[nextSong];
            }
            if (ExitButton.Click())
            {
                Game.Exit();
            }
        }
        public bool Restart()
        {
            return RestartButton.Click() && Game.GameStatus == TGCGame.GameState.Dead;
        }
        public bool GoToMainMenu()
        {
            return MainMenuButton.Click() && Game.GameStatus == TGCGame.GameState.Dead;
        }
        public void Draw(GameTime gameTime)
        {
            switch(Game.GameStatus)
            {
                case TGCGame.GameState.Menu:
                    DrawMainMenu(gameTime);
                    break;
                case TGCGame.GameState.Dead:
                    DrawDeadMenu(gameTime);
                    break;
            }
        }
        private void DrawMainMenu(GameTime gameTime)
        {
            SpriteBatch.Begin();
            SpriteBatch.DrawString(FontRegular, "Técnicas de Gráficos por Computadora | Grupo 1", new Vector2(50, 50), ButtonColor);
            SpriteBatch.DrawString(FontBig, "Midway", new Vector2(50, 70), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "Controles:", new Vector2(50, 150), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "WASD para moverse", new Vector2(50, 170), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "Mouse para apuntar y disparar", new Vector2(50, 190), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "Debug:", new Vector2(50, 250), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "G para god camera", new Vector2(50, 270), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "P, T para cambiar clima y activar efectos de trueno", new Vector2(50, 290), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "K, J para probar explosiones", new Vector2(50, 310), ButtonColor);
            SpriteBatch.DrawString(FontRegular, "C para mostrar/ocultar gizmos", new Vector2(50, 330), ButtonColor);
            SpriteBatch.End();
            PlayButton.Draw(gameTime);
            MusicButton.Draw(gameTime);
            ExitButton.Draw(gameTime);
        }
        private void DrawDeadMenu(GameTime gameTime)
        {
            string message = "¡Has muerto!";
            float width = FontBig.MeasureString(message).X;

            SpriteBatch.Begin();
            SpriteBatch.DrawString(FontBig, message, new Vector2((Graphics.Viewport.Width - width) / 2, 200), Color.Red);
            SpriteBatch.End();
            RestartButton.Draw(gameTime);
            MainMenuButton.Draw(gameTime);
        }
    }
}
