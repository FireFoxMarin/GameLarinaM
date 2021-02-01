using MatchThreeLarina.GameLogic;
using MatchThreeLarina.GameStateManagement;
using MatchThreeLarina.Gui.Screens;
using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatchMod;
using System;
using System.Collections.Generic;

namespace MatchThreeLarina.Gui
{
    internal abstract class MenuScreen : GameScreen
    {
        private readonly List<Button> menuButtons = new List<Button>();

        protected IList<Button> MenuButtons => menuButtons;

        public override void HandleInput(GameTime gameTime, Input input)
        {
            foreach (var button in menuButtons) button.HandleInput();
        }

        public override void Draw(GameTime time)
        {
            var spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.WrappedDraw(() =>
            {
                foreach (var t in menuButtons) t.Draw(time);
            });
        }
    }

    internal class MainMenuScreen : MenuScreen
    {
        public MainMenuScreen()
        {
            var viewport = MatchGame.Instance.SpriteBatch.GraphicsDevice.Viewport;
            var width = (viewport.Width - Resources.PlayButton.Width) / 2;
            var height = (viewport.Height - Resources.PlayButton.Height) / 2;

            var playButton = new Button(Resources.PlayButton, new Point(width, height));
            playButton.Clicked += PlayGameMenuClicked;

            MenuButtons.Add(playButton);
        }

        private void PlayGameMenuClicked(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, new GameplayScreen());
        }
    }

    internal class EndGameScreen : MenuScreen
    {
        private readonly Texture2D gameOverScreen;
        private readonly SpriteBatch spriteBatch = MatchGame.Instance.SpriteBatch;
        private Viewport viewport = MatchGame.Instance.GraphicsDevice.Viewport;

        public EndGameScreen()
        {
            var menuButtonTexture = Resources.MenuButton;
            var menuButton = new Button(menuButtonTexture,
                new Point((viewport.Width - menuButtonTexture.Width) / 2, 300));
            menuButton.Clicked += OkButtonClicked;
            MenuButtons.Add(menuButton);
            gameOverScreen = Resources.GameOverScreen;
        }

        private void OkButtonClicked(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, new MainMenuScreen());
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.WrappedDraw(() =>
            {
                spriteBatch.Draw(gameOverScreen,
                    new Vector2((viewport.Width - gameOverScreen.Width) / 2,
                        (viewport.Height - gameOverScreen.Height) / 2),
                    Color.White);
                spriteBatch.DrawString(Resources.Font,
                    GameScore.ScoreString,
                    new Vector2((viewport.Width - Resources.Font.MeasureString(GameScore.ScoreString).X) / 2,
                        250),
                    Color.Black);
            });

            base.Draw(gameTime);
        }
    }
}
