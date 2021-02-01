using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatchThreeLarina.GameLogic
{
    internal class Grid
    {
        public static Grid Instance;

        private readonly Random random = new Random();
        private readonly Array shapes = Enum.GetValues(typeof(ShapeType));

        public Cell[,] cells = new Cell[8, 8];
        public Cell CurrentCell;

        public List<Destroyer> DestroyerList = new List<Destroyer>();

        public Action OnSwapBegins;
        public Cell SelectedCell;

        public Grid()
        {
            CellSize = new Point(Resources.Cell.Width / 2, Resources.Cell.Height / 2);
            IsAnimating = false;
            Instance = this;
        }


        public static Point Location => new Point(10, 10);

        public static Point CellSize { get; private set; }
        public bool IsAnimating { get; private set; }

        internal void LoadContent(ContentManager content)
        {
            for (var x = 0; x < cells.GetLength(0); x++)
                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    var cell = new Cell(x, y);
                    cells[x, y] = cell;
                }
        }

        internal void Update(GameTime gameTime)
        {
            IsAnimating = false;

            foreach (var cell in cells)
            {
                if (cell.Animation != AnimationType.Idle) IsAnimating = true;
                cell.AnimmationUpdate(gameTime);
            }

            UpdateDestroyers(gameTime);
        }

        private void UpdateDestroyers(GameTime gameTime)
        {
            var positionsToDestroy = new List<Point>();

            foreach (var destroyer in DestroyerList)
            {
                IsAnimating = true;
                if (destroyer.Update(gameTime))
                    positionsToDestroy.Add(destroyer.Position);
            }


            ExplodeBombs();
            DestroyerList.RemoveAll(d => d.ToRemove);
            var destroyedItems = positionsToDestroy.Count(c => cells[c.X, c.Y].Animation == AnimationType.Idle);
            GameScore.Add(10 * destroyedItems);

            positionsToDestroy.ForEach(point =>
                cells[point.X, point.Y].Destroy());
        }

        private void ExplodeBombs()
        {
            var bombs = DestroyerList.FindAll(d => d.ToRemove &&
                                                   d.Direction == Direction.Bomb);
            foreach (var item in bombs)
                for (var x = item.Position.X - 1; x <= item.Position.X + 1; x++)
                    for (var y = item.Position.Y - 1; y <= item.Position.Y + 1; y++)
                    {
                        if (x == item.Position.X && y == item.Position.Y) continue;

                        DestroyerList.Add(new Destroyer(
                            new Vector2(y * CellSize.X + Location.X, x * CellSize.Y + Location.Y),
                            Direction.BombExplosion));
                    }
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            foreach (var cell in cells) cell.Draw(spriteBatch);
            foreach (var destroyer in DestroyerList) destroyer.Draw(spriteBatch);
        }


        internal bool FillAnewGrid(bool newBoard)
        {
            var gridChanged = false;

            for (var x = 0; x < cells.GetLength(0); x++)
                for (var y = 0; y < cells.GetLength(1); y++)
                    if (newBoard || cells[x, y].Shape == ShapeType.Empty)
                    {
                        gridChanged = true;
                        var shape = (ShapeType)shapes.GetValue(random.Next(shapes.Length - 1) + 1);
                        cells[x, y].Spawn(shape);
                        cells[x, y].Bonus = Bonus.None;
                    }

            return gridChanged;
        }

        internal int MatchAndGetPoints()
        {
            var score = 0;
            var toDestroy = new List<Cell>(64);
            var newBonuses = new List<Cell>();

            for (var x = 0; x < cells.GetLength(0); x++)
            {
                var match = new List<Cell>(8) { cells[x, 0] };
                for (var y = 1; y < cells.GetLength(1); y++)
                {
                    var stop = false;
                    if (cells[x, y].Shape == match[0].Shape)
                        match.Add(cells[x, y]);
                    else
                        stop = true;
                    if (!stop && y != cells.GetLength(1) - 1) continue;

                    findOfMatchAndBonus(match, Bonus.LineVertical);

                    match.Clear();
                    match.Add(cells[x, y]);
                }
            }

            for (var y = 0; y < cells.GetLength(1); y++)
            {
                var match = new List<Cell>(8) { cells[0, y] };
                for (var x = 1; x < cells.GetLength(0); x++)
                {
                    var stop = false;
                    if (cells[x, y].Shape == match[0].Shape)
                        match.Add(cells[x, y]);
                    else
                        stop = true;
                    if (!stop && x != cells.GetLength(0) - 1) continue;

                    findOfMatchAndBonus(match, Bonus.LineHorizontal);

                    match.Clear();
                    match.Add(cells[x, y]);
                }
            }

            newBonuses.ForEach(c => toDestroy.Remove(c));
            toDestroy.ForEach(cell => cell.Destroy());
            if (SelectedCell != null && score > 0) SelectedCell = null;
            return score;

            void searchCrossingLinesForBomb(List<Cell> match)
            {
                var intersect = toDestroy.Intersect(match);
                var intersectList = intersect.ToList();

                foreach (var bonusCell in intersectList)
                {
                    match.Remove(bonusCell);
                    toDestroy.Remove(bonusCell);
                    bonusCell.Bonus = Bonus.Bomb;
                }
            }

            void findOfMatchAndBonus(List<Cell> match, Bonus bonusState)
            {
                if (match.Count >= 3)
                {
                    if (SelectedCell != null && match.Count > 3)
                    {
                        newBonuses.Add(SpawnBonus(match, bonusState));
                    }
                    else
                    {
                        if (match.Count == 4)
                            newBonuses.Add(SpawnBonus(match, bonusState));
                        else if (match.Count >= 5)
                            newBonuses.Add(SpawnBonus(match, Bonus.Bomb));
                    }

                    searchCrossingLinesForBomb(match);

                    toDestroy.AddRange(match);
                    score += match.Count * 10;

                    Resources.MatchSound.Play();
                }
            }
        }

        private Cell SpawnBonus(List<Cell> matchedCells, Bonus lineType)
        {
            var targetCell = matchedCells.Find(cell => cell == SelectedCell || cell == CurrentCell);
            if (targetCell == null) targetCell = matchedCells[2];

            if (targetCell.Bonus != Bonus.None)
                SpawnDestroyer(targetCell.Row, targetCell.Column, targetCell.Bonus);


            switch (matchedCells.Count)
            {
                case 4:
                    targetCell.Bonus = lineType;
                    break;
                case 5:
                    targetCell.Bonus = Bonus.Bomb;
                    break;
            }

            return targetCell;
        }


        internal void SpawnDestroyer(int row, int column, Bonus bonus)
        {
            if (bonus == Bonus.LineVertical)
            {
                Destroy(column * CellSize.X + Location.X, (row - 0.5f) * CellSize.Y + Location.Y, Direction.Up);
                Destroy(column * CellSize.X + Location.X, (row + 0.5f) * CellSize.Y + Location.Y, Direction.Down);
                Resources.LineSound.Play();
            }
            else if (bonus == Bonus.LineHorizontal)
            {
                Destroy((column - 0.5f) * CellSize.X + Location.X, row * CellSize.Y + Location.Y, Direction.Left);
                Destroy((column + 0.5f) * CellSize.X + Location.X, row * CellSize.Y + Location.Y, Direction.Right);
                Resources.LineSound.Play();
            }
            else if (bonus == Bonus.Bomb)
            {
                Destroy(column * CellSize.X + Location.X, row * CellSize.Y + Location.Y, Direction.Bomb);
                Resources.BombSound.Play();
            }

            void Destroy(float deltaX, float deltaY, Direction direction)
            {
                DestroyerList.Add(new Destroyer(new Vector2(deltaX, deltaY), direction));
            }
        }


        public List<List<Cell>> FindMoves()
        {
            var avaibleMoves = new List<List<Cell>>();
            var indexMoveArray = 0;

            int[,] swipeDirections = { { 1, 0 }, { 0, 1 } };

            for (var direction = 0; direction < 2; direction++)
                for (var x = 0; x < cells.GetLength(0) - swipeDirections[direction, 0]; x++)
                    for (var y = 0; y < cells.GetLength(1) - swipeDirections[direction, 1]; y++)
                    {
                        swap(new Point(x, y), new Point(x + swipeDirections[direction, 0], y + swipeDirections[direction, 1]));
                        var clusters = findClusters();
                        swap(new Point(x, y), new Point(x + swipeDirections[direction, 0], y + swipeDirections[direction, 1]));

                        if (clusters.Count > 0)
                        {
                            avaibleMoves.Add(new List<Cell>(clusters));
                            indexMoveArray++;
                        }
                    }

            return avaibleMoves;
        }

        public void swap(Point pointCellFirst, Point pointCellSecond)
        {
            var tmp = cells[pointCellFirst.X, pointCellFirst.Y];
            cells[pointCellFirst.X, pointCellFirst.Y] = cells[pointCellSecond.X, pointCellSecond.Y];
            cells[pointCellSecond.X, pointCellSecond.Y] = tmp;
        }

        private List<Cell> findClusters()
        {
            var clusters = new List<Cell>();

            // horizontal
            for (var x = 0; x < cells.GetLength(0); x++)
            {
                var matchLen = 1;

                for (var y = 0; y < cells.GetLength(1); y++)
                {
                    var check = false;

                    if (y == cells.GetLength(1) - 1)
                    {
                        check = true;
                    }
                    else
                    {
                        if (cells[x, y].Shape == cells[x, y + 1].Shape && cells[x, y] != null)
                            matchLen++;
                        else
                            check = true;
                    }

                    if (check)
                    {
                        if (matchLen >= 3)
                            for (var k = 1; k <= matchLen; k++)
                                clusters.Add(cells[x, y + 1 - k]);


                        matchLen = 1;
                    }
                }
            }

            // vertical
            for (var y = 0; y < cells.GetLength(1); y++)
            {
                var matchLen = 1;

                for (var x = 0; x < cells.GetLength(0); x++)
                {
                    var check = false;

                    if (x == cells.GetLength(0) - 1)
                    {
                        check = true;
                    }
                    else
                    {
                        if (cells[x, y].Shape == cells[x + 1, y].Shape && cells[x, y] != null)
                            matchLen++;
                        else
                            check = true;
                    }

                    if (check)
                    {
                        if (matchLen >= 3)
                            for (var k = 1; k <= matchLen; k++)
                                clusters.Add(cells[x + 1 - k, y]);

                        matchLen = 1;
                    }
                }
            }

            return clusters;
        }


        internal void DropCells()
        {
            for (var y = 0; y < cells.GetLength(1); y++)
                for (var x = cells.GetLength(0) - 1; x > 0; x--)
                    if (cells[x, y].Shape == ShapeType.Empty)
                    {
                        var shift = x - 1;
                        while (shift >= 0 && cells[shift, y].Shape == ShapeType.Empty) shift--;
                        if (shift < 0) break;
                        cells[shift, y].FallInto(cells[x, y]);
                    }
        }

        internal void UserInput()
        {
            var mouseState = Mouse.GetState();

            var fieldRect = new Rectangle(Location.X, Location.Y, CellSize.X * 8, CellSize.Y * 8);

            if (fieldRect.Contains(mouseState.Position))
            {
                var x = (mouseState.Position.Y - fieldRect.Y) / CellSize.Y;
                var y = (mouseState.Position.X - fieldRect.X) / CellSize.X;

                if (CurrentCell != null && cells[x, y] != CurrentCell) CurrentCell.State = CellState.Normal;
                CurrentCell = cells[x, y];

                if (mouseState.LeftButton == ButtonState.Released && CurrentCell.State == CellState.Pressed)
                {
                    CurrentCell.State = CellState.Hover;
                    if (CurrentCell.IsSelected)
                        ClearSelection();
                    else if (SelectedCell != null && CurrentCell.IsCloseTo(SelectedCell))
                        OnSwapBegins?.Invoke();
                    else
                        SelectCurrentCell();
                }
                else if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    CurrentCell.State = CellState.Pressed;
                }
                else
                {
                    CurrentCell.State = CellState.Hover;
                }
            }
            else
            {
                if (CurrentCell != null) CurrentCell.State = CellState.Normal;
            }
        }

        private void SelectCurrentCell()
        {
            CurrentCell.IsSelected = !CurrentCell.IsSelected;
            if (SelectedCell != null) SelectedCell.IsSelected = !SelectedCell.IsSelected;
            SelectedCell = CurrentCell;
        }

        private void ClearSelection()
        {
            CurrentCell.IsSelected = !CurrentCell.IsSelected;
            SelectedCell = null;
        }

        internal void Swap()
        {
            SelectedCell.SwapWith(CurrentCell, false);
            SelectedCell.IsSelected = !SelectedCell.IsSelected;
        }

        internal void SwapBack()
        {
            CurrentCell.SwapWith(SelectedCell, true);
            SelectedCell = null;
        }
    }

    internal static class GridExtention
    {
        public static int Width(this Cell[,] array)
        {
            return array.GetLength(0);
        }

        public static int Height(this Cell[,] array)
        {
            return array.GetLength(1);
        }

        public static bool IsValid(this Cell[,] array, Vector2 coords)
        {
            return coords.X < 0 || coords.X <= array.GetLength(0) ||
                   coords.Y < 0 || coords.Y <= array.GetLength(1);
        }

        public static Cell Get(this Cell[,] array, Vector2 coords)
        {
            return array.IsValid(coords) ? array[(int)coords.X, (int)coords.Y] : null;
        }
    }
}
