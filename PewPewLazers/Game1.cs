using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        //GameWorld gameWorld;
        SpriteBatch spriteBatch;
        public static Effect pe;
        public static Effect texNorm;
        public static Texture2D skyTex;
        public static SpriteFont font;
        public static Random rand;
        // Audio Stuff
        private AudioLibrary audio;

        //gamescenes
        protected MenuScene menuScene;
        protected GameScene activeScene;
        protected Level1Scene level1Scene;
        private Texture2D pimage;

        public static Texture2D asterTex;

        //User inputs
        protected KeyboardState oldKeyboardState;

        public Game1()
        {
            rand = new Random();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 1024;
            graphics.IsFullScreen = true;
           
        }

        protected override void Initialize()
        {
            //gameWorld = new GameWorld(this);
            graphics.PreferredBackBufferHeight = 1050;
            graphics.PreferredBackBufferWidth = 1680;
            graphics.ToggleFullScreen();
            graphics.ToggleFullScreen();
            base.Initialize();
            //gameWorld.Initialize();
        }

        public static float nextFloat()
        {
            return (float)rand.NextDouble();
        }

        protected override void LoadContent()
        {
            texNorm = Content.Load<Effect>("Shaders\\TextureNormal");
            font = Content.Load<SpriteFont>("Fonts\\SpriteFont1");
            //gameWorld.LoadContent();
            asterTex = Content.Load<Texture2D>("Models\\fbx\\asteroid");
            skyTex = new Texture2D(GraphicsDevice, 2048, 2048);

            proceduralSpace(skyTex, 2048, 2048, 1000, 1);
            // Create a new SpriteBatch
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            pe = Content.Load<Effect>("Shaders\\particle");
            pimage = Content.Load<Texture2D>("Images\\Particle");
            pe.Parameters["pic"].SetValue(pimage);

            // Load Audio Elements
            audio = new AudioLibrary();
            audio.LoadContent(Content);
            Services.AddService(typeof(AudioLibrary), audio);

            menuScene = new MenuScene(this);
            Components.Add(menuScene);
            menuScene.Show();

            level1Scene = new Level1Scene(this);
            Components.Add(level1Scene);

            activeScene = menuScene;

            base.LoadContent();
        }
        protected override void UnloadContent()
        {

        }

        protected void ShowScene(GameScene scene)
        {
            scene.InitializeContent();
            activeScene.Hide();
            activeScene = scene;
            scene.Show();
        }

        private bool CheckEnter()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            bool result = (oldKeyboardState.IsKeyDown(Keys.Enter) && (keyboardState.IsKeyUp(Keys.Enter)));
            oldKeyboardState = keyboardState;
            return result;
        }

        private void HandleScenesInput()
        {
            // handle start scene input
            if (activeScene == menuScene)
            {
                HandleMenuSceneInput();
            }
            //handle level 1 scene input
            else if (activeScene == level1Scene)
            {
                HandleLevel1SceneInput();
            }
        }

        private void HandleMenuSceneInput()
        {
            if (CheckEnter())
            {
                switch (menuScene.SelectedMenuIndex)
                {
                    case 0:
                        ShowScene(level1Scene);
                        break;
                    case 1:
                        Exit();
                        break;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
        }

        private void HandleLevel1SceneInput()
        {
            // Get the Keyboard
            KeyboardState keyboardState = Keyboard.GetState();

            bool backKey = (oldKeyboardState.IsKeyDown(Keys.Escape) &&
                (keyboardState.IsKeyUp(Keys.Escape)));

            bool enterKey = (oldKeyboardState.IsKeyDown(Keys.Enter) &&
                (keyboardState.IsKeyUp(Keys.Enter)));

            oldKeyboardState = keyboardState;

            //handle enter and exit
            if (enterKey)
            {
                if (level1Scene.GameOver)
                {
                    ShowScene(menuScene);
                }
            }
            if (backKey)
            {
                ShowScene(menuScene);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Handle Inputs
            HandleScenesInput();

            //gameWorld.Update(gameTime);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Begin..
            spriteBatch.Begin();
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            //gameWorld.Draw(gameTime);
            base.Draw(gameTime);
            // End.
            spriteBatch.End();
        }



        private void cloudynesspass(Color[] data, int width, int height)
        {

            Color tint = new Color(6, 17, 49);
            //Color tint = new Color(255, 255, 255);
            data[0] = tint;
            for (int i = 1; i < width; i++)
            {
                data[i] = new Color(data[i - 1].ToVector3() * (0.99f + (float)(Game1.rand.NextDouble() / 50)));
            }
            for (int i = width; i < data.Length; i++)
            {
                data[i] = new Color((data[i - 1].ToVector3() + data[i - width].ToVector3()) / 2 * (0.5f + (float)(Game1.rand.NextDouble())));
            }
        }

        private void cloudynesspass2(Color[] data, int width, int height)
        {
            Color tint = new Color(6, 17, 49);
            data[data.Length - 1] = tint;
            for (int i = data.Length - 2; i > (data.Length - 1) - width; i--)
            {
                data[i] = new Color(Vector3.Max(data[i].ToVector3(), data[i + 1].ToVector3() * (0.99f + (float)(Game1.rand.NextDouble() / 50))));
            }
            for (int i = data.Length - width - 1; i >= 0; i--)
            {
                data[i] = new Color(Vector3.Max(data[i].ToVector3(), data[i + 1].ToVector3() + data[i + width].ToVector3()) / 2 * (0.5f + (float)(Game1.rand.NextDouble())));
            }
        }

        private void dataSum(Color[] a, Color[] b)
        {
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                a[i].B = Math.Max(a[i].B, b[i].B);
                a[i].G = Math.Min(Math.Max(a[i].G, b[i].G), (byte)(a[i].B / 2));
                a[i].R = 0;// Math.Max(a[i].R, b[i].R);
            }
        }

        private void addStar(Color[] data, int width, int height, int x, int y, float brightness, Color tint)
        {
            int falloff = (int)Math.Max(1, 6 - brightness);

            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            for (int j = Math.Max(x - falloff, 0); j < Math.Min(x + falloff, width); j++)
                for (int k = Math.Max(y - falloff, 0); k < Math.Min(y + falloff, height); k++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(j, k));
                    if (distance <= falloff)
                    {
                        distance /= falloff;
                        distance = 1 - distance;
                        data[k * width + j] = new Color(data[k * width + j].ToVector4() + (tint.ToVector4() * distance) * 3);
                    }
                }
        }

        private void proceduralSpace(Texture2D bmp, int width, int height, int starsc, int layer)
        {
            Color[] data = new Color[width * height];
            Color[] data2 = new Color[width * height];
            bmp.GetData<Color>(data);
            if (layer == 1)
            {
                cloudynesspass(data, width, height);
                cloudynesspass2(data, width, height);
                //cloudynesspass3(data, width, height);
                //cloudynesspass4(data, width, height);
                dataSum(data, data2);
                //addStar(data, width, height, 400, 500, 1);
                for (int i = 0; i < data.Length; i++)
                {

                    if (Game1.rand.Next(1, 50000) < data[i].B)
                    {
                        addStar(data, width, height, i % width, i / width, Game1.rand.Next(3, 5), new Color(100, 100, 255));
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Length; i++)
                {

                    if (Game1.rand.Next(1, 1000) == 1)
                    {
                        Color rand = new Color((float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble());
                        addStar(data, width, height, i % width, i / width, 6, rand);
                    }
                }
            }

            bmp.SetData<Color>(data);
        }

    }
}
