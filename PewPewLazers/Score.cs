using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace PewPewLazers
{
    public class Score : DrawableGameComponent
    {
        // Spritebatch
        protected SpriteBatch spriteBatch = null;

        private const int ADDVALUE = 100;
        protected TimeSpan elapsedTime;

        // Score Position
        protected Vector2 position = new Vector2();

        // Values
        protected int value;

        protected readonly SpriteFont font;
        protected readonly Color fontColor;

        public Score(Game game, Color fontColor)
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
            set { this.value = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;
            
            if (elapsedTime > TimeSpan.FromMilliseconds(ADDVALUE))
            {
                elapsedTime -= TimeSpan.FromMilliseconds(ADDVALUE);

                value++;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            string TextToDraw = string.Format("Score: {0}", value);

            // Draw the text item
            spriteBatch.DrawString(font, TextToDraw,
                                    new Vector2(position.X, position.Y),
                                    fontColor);
            base.Draw(gameTime);
        }
    }
}