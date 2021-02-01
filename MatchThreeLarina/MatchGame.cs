using MatchThreeLarina.GameStateManagement;
using MatchThreeLarina.Gui;
using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatchMod;
using System;

namespace MatchThreeLarina
{
    public class MatchGame : Game
    {
        public static MatchGame Instance;

        public static Texture2D Background;

        private int backgroundDeltaX;
        private Vector2 backgroundPosition;
        private float backgroundScale = 1.0F;
        public GraphicsDeviceManager Graphics;

        private ScreenManager screenManager;
        public SpriteBatch SpriteBatch;


        public MatchGame()
        {
            Graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = 600,
                PreferredBackBufferWidth = 500
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize()
        {
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Resources.Init(Content);

            screenManager.AddScreen(new MainMenuScreen());

            Background = Content.Load<Texture2D>("Sprites/background");
            InitBackgroundRects();
        }

        protected override void Draw(GameTime time)
        {
            GraphicsDevice.Clear(Color.White);

            SpriteBatch.WrappedDraw(() =>
            {
                SpriteBatch.Draw(Background, backgroundPosition, null, Color.White, 0.0F,
                    Vector2.Zero, backgroundScale, SpriteEffects.None, 1.0F);
            });

            base.Draw(time);
        }

        private void InitBackgroundRects()
        {
            backgroundScale = (float)Graphics.PreferredBackBufferHeight / Background.Height;
            backgroundDeltaX = (Graphics.PreferredBackBufferWidth - (int)(Background.Width * backgroundScale)) / 2;
            backgroundPosition = new Vector2(backgroundDeltaX, 0);
        }
    }
}

namespace SpriteBatchMod
{
    public static class MySpriteBatch
    {
        public static void WrappedDraw(this SpriteBatch batch, Action drawAction)
        {
            batch.Begin();
            drawAction?.Invoke();
            batch.End();
        }
    }
}
