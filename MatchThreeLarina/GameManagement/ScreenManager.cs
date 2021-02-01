using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace MatchThreeLarina.GameStateManagement
{
    public class ScreenManager : DrawableGameComponent
    {
        private readonly Input input = new Input();

        private readonly List<GameScreen> screens = new List<GameScreen>();
        private readonly List<GameScreen> tempScreensList = new List<GameScreen>();

        private bool isInitialized;

        public SpriteBatch SpriteBatch;

        public ScreenManager(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (var screen in screens) screen.Activate(false);
        }

        protected override void UnloadContent()
        {
            foreach (var screen in screens) screen.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            input.Update();

            tempScreensList.Clear();

            foreach (var screen in screens)
                tempScreensList.Add(screen);

            var otherScreenHasFocus = !Game.IsActive;
            var coveredByOtherScreen = false;

            while (tempScreensList.Count > 0)
            {
                var screen = tempScreensList[tempScreensList.Count - 1];

                tempScreensList.RemoveAt(tempScreensList.Count - 1);

                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState != ScreenState.TransitionOn &&
                    screen.ScreenState != ScreenState.Active) continue;

                if (!otherScreenHasFocus)
                    screen.HandleInput(gameTime, input);

                otherScreenHasFocus = true;

                if (!screen.IsPopup)
                    coveredByOtherScreen = true;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var screen in screens.Where(screen => screen.ScreenState != ScreenState.Hidden))
                screen.Draw(gameTime);
        }

        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.IsExiting = false;

            if (isInitialized) screen.Activate(false);

            screens.Add(screen);
        }

        public void RemoveScreen(GameScreen screen)
        {
            if (isInitialized) screen.Unload();

            screens.Remove(screen);
            tempScreensList.Remove(screen);
        }

        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }
    }
}
