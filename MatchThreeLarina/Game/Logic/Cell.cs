using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteBatchMod;

namespace MatchThreeLarina.GameLogic
{
    internal class Cell
    {
        private static readonly float speedOpacity = 2f;
        private readonly Point size;
        private readonly Texture2D texture;
        private Vector2 location;
        private Vector2 moveDestination;
        private float opacity;
        private int speed;

        public Cell(int row, int column)
        {
            Shape = ShapeType.Empty;
            texture = Resources.Cell;
            Row = row;
            Column = column;

            size = Grid.CellSize;
            location = new Vector2(Column * size.X + 10, Row * size.Y + 10);

            Animation = AnimationType.Hiding;
            State = CellState.Normal;
            Bonus = Bonus.None;
            IsSelected = false;
        }


        public AnimationType Animation { get; set; }
        public bool IsSelected { get; set; }
        public int Row { get; }
        public int Column { get; }
        public ShapeType Shape { get; set; }
        public Bonus Bonus { get; set; }
        public CellState State { get; set; }
        public float Scale { get; set; }

        internal void AnimmationUpdate(GameTime time)
        {
            if (Animation == AnimationType.Idle)
                return;

            if (Animation == AnimationType.Hiding)
                FadeIn(time);
            else if (Animation == AnimationType.Revealing)
                FadeOut(time);
            else if (Animation == AnimationType.Falling)
                Fall(time);
            else if (Animation == AnimationType.Swapping)
                Swap(time);
        }


        private void FadeIn(GameTime time)
        {
            opacity += (float)(speedOpacity * time.ElapsedGameTime.TotalSeconds);
            if (opacity >= 1f)
            {
                opacity = 1f;
                Animation = AnimationType.Idle;
            }
        }

        private void FadeOut(GameTime time)
        {
            opacity -= (float)(speedOpacity * time.ElapsedGameTime.TotalSeconds);
            if (opacity <= 0f)
            {
                Shape = ShapeType.Empty;
                opacity = 1f;
                Animation = AnimationType.Idle;
            }
        }

        private void Fall(GameTime time)
        {
            location.Y += (float)(speed * time.ElapsedGameTime.TotalSeconds);
            if (location.Y >= moveDestination.Y)
            {
                location.Y = moveDestination.Y;
                Animation = AnimationType.Idle;
            }
        }

        private void Swap(GameTime time)
        {
            if (location != moveDestination)
            {
                var difference = moveDestination - location;
                var direction = Vector2.Normalize(difference);
                var movement = direction * (float)(speed * time.ElapsedGameTime.TotalSeconds);
                if (movement.Length() < difference.Length())
                {
                    location += movement;
                }
                else
                {
                    location = moveDestination;
                    Animation = AnimationType.Idle;
                    opacity = 1f;
                }
            }
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.WrappedDraw(() =>
            {
                var rectangle = new Rectangle((int)location.X, (int)location.Y, size.X, size.Y);
                if (texture != null)
                {
                    if (State == CellState.Normal)
                        spriteBatch.Draw(texture, rectangle, Color.White);
                    else if (State == CellState.Hover)
                        spriteBatch.Draw(texture, rectangle, Color.Chocolate);
                    else if (State == CellState.Pressed) spriteBatch.Draw(texture, rectangle, Color.White);

                    if (IsSelected) spriteBatch.Draw(texture, rectangle, Color.Red);
                }

                var opacityModification = opacity;
                var opacitySize = 0.5f;

                if (Scale == 0.55f)
                {
                    opacityModification = 0.5f;
                    opacitySize = Scale;
                }

                spriteBatch.Draw(Resources.GetTexture(Shape, Bonus),
                    location,
                    null,
                    new Color(Color.White, opacityModification),
                    0,
                    Vector2.Zero,
                    opacitySize,
                    SpriteEffects.None,
                    0f);

                Scale = 0.5f;
            });
        }

        internal void Destroy()
        {
            if (Shape == ShapeType.Empty || Animation == AnimationType.Revealing) return;
            State = CellState.Normal;
            Animation = AnimationType.Revealing;
            Grid.Instance.SpawnDestroyer(Row, Column, Bonus);
        }

        internal void Spawn(ShapeType shape)
        {
            State = CellState.Normal;
            Bonus = Bonus.None;
            Shape = shape;
            opacity = 0f;
            Animation = AnimationType.Hiding;
        }

        internal void FallInto(Cell cell)
        {
            cell.State = CellState.Normal;
            cell.Shape = Shape;
            Shape = ShapeType.Empty;
            cell.Bonus = Bonus;
            Bonus = Bonus.None;

            cell.moveDestination = cell.location;
            cell.location = location;
            cell.speed = cell.Row * 35 + 150;

            cell.Animation = AnimationType.Falling;
        }

        public bool IsCloseTo(Cell cell)
        {
            return Column == cell.Column && (Row == cell.Row - 1 || Row == cell.Row + 1) ||
                   Row == cell.Row && (Column == cell.Column - 1 || Column == cell.Column + 1);
        }

        internal void SwapWith(Cell cell, bool unswap)
        {
            var swapSpeed = unswap ? 250 : 180;

            State = CellState.Normal;
            cell.State = CellState.Normal;

            var tempLocation = location;
            location = cell.location;
            cell.location = tempLocation;

            cell.moveDestination = location;
            moveDestination = cell.location;

            var tempShape = Shape;
            Shape = cell.Shape;
            cell.Shape = tempShape;

            var tempBonus = Bonus;
            Bonus = cell.Bonus;
            cell.Bonus = tempBonus;

            speed = swapSpeed;
            cell.speed = swapSpeed;
            opacity = 0.5f;
            cell.opacity = 0.5f;
            Animation = AnimationType.Swapping;
            cell.Animation = AnimationType.Swapping;

            Resources.ClickSound.Play();
        }
    }
}
