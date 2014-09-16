#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
#endregion

namespace PewPewLazers
{

    public class MenuScene : GameScene
    {
        protected AudioLibrary audio;
        // Fonts
        protected SpriteFont regularFont, selectedFont;
        // Colors
        protected Color regularColor = Color.White, selectedColor = Color.Red;
        // Menu Position
        protected Vector2 menuPosition = new Vector2();
        // Items
        protected int selectedIndex = 0;
        private List<string> menuItems;
        // Used for handle input
        protected KeyboardState oldKeyboardState;
        // Size of menu in pixels
        protected int width, height;

        protected Texture2D title;
        protected Texture2D backgroundTexture;
        protected Rectangle backgroundRect;
        // Spritebatch
        protected SpriteBatch spriteBatch = null;

        //title
        protected Rectangle titleRect = new Rectangle(0, 0, 368, 108);
        protected Vector2 titlePos;

        public MenuScene(Game game)
            : base(game)
        {
            Load();
            backgroundRect = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
            menuItems = new List<string>();
            string[] items = { "Start", "Quit" };
            SetMenuItems(items);

            // Get the audio library
            audio = (AudioLibrary)
                Game.Services.GetService(typeof(AudioLibrary));

            // Get the current spritebatch
            spriteBatch = (SpriteBatch)Game.Services.GetService(
                                            typeof(SpriteBatch));
            // Used for input handling
            oldKeyboardState = Keyboard.GetState();
        }

        protected void Load()
        {
            // Create the Menu Scene
            regularFont = Game.Content.Load<SpriteFont>("Fonts\\menuSmall");
            selectedFont = Game.Content.Load<SpriteFont>("Fonts\\menuLarge");
            backgroundTexture = Game1.skyTex;
            title = Game.Content.Load<Texture2D>("Images\\title");
            base.LoadContent();
        }

        public override void Show()
        {
            ///////////////////////////////////////////////////////////////////////MediaPlayer.Play(audio.StartMusic);
            // Put the menu and title centered in screen
            menuPosition = new Vector2((Game.Window.ClientBounds.Width - width) / 2, 330);
            titlePos = new Vector2(((Game.Window.ClientBounds.Width - width) / 2) - (titleRect.Width / 2), 50);
            base.Show();
        }

        public override void InitializeContent()
        {
            base.InitializeContent();
        }

        public override void Hide()
        {
            MediaPlayer.Stop();
            base.Hide();
        }

        public int SelectedMenuIndex
        {
            get { return selectedIndex; }
        }

        public void SetMenuItems(string[] items)
        {
            menuItems.Clear();
            menuItems.AddRange(items);
            CalculateBounds();
        }

        protected void CalculateBounds()
        {
            width = 0;
            height = 0;
            foreach (string item in menuItems)
            {
                Vector2 size = selectedFont.MeasureString(item);
                if (size.X > width)
                {
                    width = (int)size.X;
                }
                height += selectedFont.LineSpacing;
            }
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            bool down, up;
            // Handle the keyboard
            down = (oldKeyboardState.IsKeyDown(Keys.Down) &&
                (keyboardState.IsKeyUp(Keys.Down)));
            up = (oldKeyboardState.IsKeyDown(Keys.Up) &&
                (keyboardState.IsKeyUp(Keys.Up)));

            if (down)
            {
                selectedIndex++;
                if (selectedIndex == menuItems.Count)
                {
                    selectedIndex = 0;
                }
            }
            if (up)
            {
                selectedIndex--;
                if (selectedIndex == -1)
                {
                    selectedIndex = menuItems.Count - 1;
                }
            }

            oldKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Begin
            //spriteBatch.Begin();
            spriteBatch.Draw(backgroundTexture, backgroundRect, Color.White);

            float y = menuPosition.Y;
            for (int i = 0; i < menuItems.Count; i++)
            {
                SpriteFont font;
                Color theColor;
                if (i == selectedIndex)
                {
                    font = selectedFont;
                    theColor = selectedColor;
                }
                else
                {
                    font = regularFont;
                    theColor = regularColor;
                }

                // Draw the text item
                spriteBatch.DrawString(font, menuItems[i], new Vector2(menuPosition.X, y), theColor);
                y += font.LineSpacing;
            }

            base.Draw(gameTime);

            spriteBatch.Draw(title, titlePos, titleRect, Color.White);
            // End.
            //spriteBatch.End();
        }

    }
}