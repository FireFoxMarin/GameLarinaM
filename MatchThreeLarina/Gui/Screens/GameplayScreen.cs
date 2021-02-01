using MatchThreeLarina.GameLogic;
using MatchThreeLarina.GameStateManagement;
using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatchMod;
using System.Collections.Generic;
using static MatchThreeLarina.GameLogic.Timer;

namespace MatchThreeLarina.Gui.Screens
{
    internal class GameplayScreen : GameScreen
    {
        private readonly Grid grid = new Grid();
        private readonly SpriteBatch spriteBatch;
        private List<List<Cell>> availableMoves;
        private ContentManager content;

        private SpriteFont gameFont;

        private GameState gameState;


        public GameplayScreen()
        {
            spriteBatch = MatchGame.Instance.SpriteBatch;

            gameState = GameState.GridFill;

            grid.LoadContent(content);
            grid.OnSwapBegins += () => { gameState = GameState.Swap; };
        }

        public override void Activate(bool instancePreserved)
        {
            if (instancePreserved) return;
            if (content == null)
                content = MatchGame.Instance.Content;

            AddListener(() => { ScreenManager.AddScreen(new EndGameScreen()); });
            ScreenManager.Game.ResetElapsedTime();
            Reset();

            GameScore.Reset();

            gameFont = Resources.Font;

            ScreenManager.Game.ResetElapsedTime();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            if (!IsActive)
                return;

            Tick(gameTime);
            GameUpdate();
            grid.Update(gameTime);
        }


        private void GameUpdate()
        {
            if (grid.IsAnimating)
                return;

            var score = 0;

            if (gameState == GameState.GridFill)
            {
                gameState = grid.FillAnewGrid(false) ? GameState.MatchAfterFill : GameState.Input;
            }
            else if (gameState == GameState.MatchAfterFill)
            {
                StartFallingIfMatchedOtherwiseSetState(GameState.Input);
            }
            else if (gameState == GameState.CellFalling)
            {
                grid.DropCells();
                gameState = GameState.GridFill;
            }
            else if (gameState == GameState.Input)
            {
                availableMoves = grid.FindMoves();
                if (availableMoves.Count == 0)
                    gameState = grid.FillAnewGrid(true) ? GameState.MatchAfterFill : GameState.Input;

                var demonstrationMove = availableMoves.Find(item => item.Count >= 4);
                if (demonstrationMove == null) demonstrationMove = availableMoves[0];
                foreach (var cellMove in demonstrationMove) cellMove.Scale = 0.55f;

                grid.UserInput();
            }
            else if (gameState == GameState.Swap)
            {
                grid.Swap();
                gameState = GameState.MatchAfterSwap;
            }
            else if (gameState == GameState.MatchAfterSwap)
            {
                StartFallingIfMatchedOtherwiseSetState(GameState.SwapBack);
            }
            else if (gameState == GameState.SwapBack)
            {
                grid.SwapBack();
                gameState = GameState.Input;
            }

            void StartFallingIfMatchedOtherwiseSetState(GameState Status)
            {
                score = grid.MatchAndGetPoints();
                if (score > 0)
                {
                    GameScore.Add(score);
                    gameState = GameState.CellFalling;
                }
                else
                {
                    gameState = Status;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Aqua, 0, 0);


            spriteBatch.WrappedDraw(() =>
            {
                spriteBatch.Draw(MatchGame.Background, new Vector2(-200, 0), null, Color.White,
                    0.0F, Vector2.Zero, 0.75f, SpriteEffects.None, 1.0F);

                spriteBatch.Draw(Resources.FieldBackground, new Vector2(10, 10), null, Color.White,
                    0.0F, Vector2.Zero, 0.5f, SpriteEffects.None, 1.0F);

                if (availableMoves != null)
                    spriteBatch.DrawString(gameFont, "Available moves: " + availableMoves.Count, new Vector2(0, 550),
                        Color.Black);

                spriteBatch.DrawString(gameFont, GameScore.ScoreString, new Vector2(310, 550), Color.Black);
                spriteBatch.DrawString(gameFont, TimeRemaining, new Vector2(210, 550),
                    timeToWait <= 5 ? Color.Red : Color.Black);
            });

            grid.Draw(spriteBatch);
        }
    }
}
