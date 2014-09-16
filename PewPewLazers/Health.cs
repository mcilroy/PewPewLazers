using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PewPewLazers
{
    public class Health : DrawableGameComponent
    {
        // Spritebatch
        protected SpriteBatch spriteBatch = null;

        protected TimeSpan elapsedTime;

        // Score Position
        protected Vector2 position = new Vector2();

        // Values
        protected int value;

        protected readonly SpriteFont font;
        protected readonly Color fontColor;

        public Health(Game game, Color fontColor)
            : base(game)
        {
            font = Game.Content.Load<SpriteFont>("Fonts\\menuSmall");
            this.fontColor = fontColor;
            // Get the current spritebatch
            spriteBatch = (SpriteBatch)
                            Game.Services.GetService(typeof(SpriteBatch));
        }

        public int Value
        {
            get { return value; }
            set {
                this.value = value;
                if (this.value < 0)
                    this.value = 0;
                if (this.value > 100)
                    this.value = 100;
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void Draw(GameTime gameTime)
        {
            string TextToDraw = string.Format("Health: {0}", value);

            // Draw the text item
            spriteBatch.DrawString(font, TextToDraw,
                                    new Vector2(position.X, position.Y),
                                    fontColor);
            base.Draw(gameTime);
        }
    }
}