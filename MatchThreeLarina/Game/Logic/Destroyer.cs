using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatchMod;

namespace MatchThreeLarina.GameLogic
{
    internal class Destroyer
    {
        private readonly Texture2D texture = Resources.Destroyer;
        private Vector2 location;
        private double timer;

        public Destroyer(Vector2 location, Direction direction)
        {
            this.location = location;
            Direction = direction;
            ToRemove = false;
            Position = new Point(-1, -1);
        }

        public Direction Direction { get; }
        public Point Position { get; private set; }
        public bool ToRemove { get; private set; }

        internal bool Update(GameTime gameTime)
        {
            var isNewBlockReached = SetPosition() &&
                                    Position.Y < 8 && Position.Y >= 0 && Position.X >= 0 && Position.X < 8;
            var speed = (float)(300f * gameTime.ElapsedGameTime.TotalSeconds);
            if (Direction == Direction.Up)
                MoveUp(speed);
            else if (Direction == Direction.Down)
                MoveDown(speed);
            else if (Direction == Direction.Left)
                MoveLeft(speed);
            else if (Direction == Direction.Right)
                MoveRight(speed);
            else if (Direction == Direction.Bomb)
                Detonate(gameTime.ElapsedGameTime.TotalMilliseconds);
            else if (Direction == Direction.BombExplosion) Detonate(gameTime.ElapsedGameTime.TotalMilliseconds);

            return isNewBlockReached;
        }

        private bool SetPosition()
        {
            var r = false;
            var row = (int)((location.Y - Grid.Location.Y) / Grid.CellSize.Y);
            var column = (int)((location.X - Grid.Location.X) / Grid.CellSize.X);

            if (Position.X != column || Position.Y != row) r = true;
            Position = new Point(row, column);
            return r;
        }

        private void MoveUp(float dist)
        {
            location.Y -= dist;
            float end = Grid.Location.Y - Grid.CellSize.Y;
            if (location.Y <= end)
            {
                location.Y = end;
                ToRemove = true;
            }
        }

        private void MoveDown(float dist)
        {
            location.Y += dist;
            float end = Grid.Location.Y + Grid.CellSize.Y * 8;
            if (location.Y >= end)
            {
                location.Y = end;
                ToRemove = true;
            }
        }

        private void MoveLeft(float dist)
        {
            location.X -= dist;
            float end = Grid.Location.X - Grid.CellSize.X;
            if (location.X <= end)
            {
                location.X = end;
                ToRemove = true;
            }
        }

        private void MoveRight(float dist)
        {
            location.X += dist;
            float end = Grid.Location.X + Grid.CellSize.X * 8;
            if (location.X >= end)
            {
                location.X = end;
                ToRemove = true;
            }
        }

        private void Detonate(double elapsedMilliseconds)
        {
            timer += elapsedMilliseconds;
            if (timer >= 250f) ToRemove = true;
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.WrappedDraw(() =>
            {
                spriteBatch.Draw(texture, location, null, Color.White,
                    0, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            });
        }
    }
}
