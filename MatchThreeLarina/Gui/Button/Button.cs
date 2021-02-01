using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteBatchMod;
using System;

namespace MatchThreeLarina.Gui
{
    internal abstract class InstanceComponent : DrawableGameComponent
    {
        public bool IsClicked;

        protected InstanceComponent() : base(MatchGame.Instance)
        {
        }

        protected Rectangle Rectangle { get; set; }

        protected bool IsHighlighted { get; private set; }

        protected new MatchGame Game => (MatchGame)base.Game;

        public virtual void HandleInput()
        {
            var mouseState = Mouse.GetState();
            IsHighlighted = Rectangle.Contains(new Point(mouseState.X, mouseState.Y));
            IsClicked = IsHighlighted && mouseState.LeftButton == ButtonState.Pressed;
            if (IsClicked)
                OnClicked();
        }

        public event EventHandler Clicked;

        protected void OnClicked()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public override void Update(GameTime time)
        {
            HandleInput();
        }
    }

    internal class Button : InstanceComponent
    {
        protected readonly Texture2D texture;

        public Button(Texture2D texture, Point position)
        {
            this.texture = texture;
            Rectangle = new Rectangle(position, new Point(this.texture.Width, this.texture.Height));
        }

        public override void Draw(GameTime time)
        {
            var color = Color.White;
            if (IsHighlighted)
                color = Color.LightBlue;
            if (IsClicked)
                color = Color.DodgerBlue;

            Game.SpriteBatch.WrappedDraw(() => { Game.SpriteBatch.Draw(texture, Rectangle, color); });
        }
    }
}
