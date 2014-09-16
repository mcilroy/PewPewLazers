using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PewPewLazers.GameObject;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
namespace PewPewLazers
{
    public class Level1Scene : GameScene
    {
        private AudioLibrary audio;
        protected SpriteBatch spriteBatch = null;
        protected bool gameOver;
        protected Vector2 gameoverPosition;
        protected SpriteFont gameOverFont;

        protected Vector2 healthPosition;
        protected SpriteFont healthFont;

        public Camera theCamera;
        public Player player;
        public Tunnel tunnel;
        private SkyBox skybox;
        private BulletManager bulletManager;
        private AsteroidManager asteroidManager;

        public ParticleSystem pgen;
        private Game game1;


        public Level1Scene(Game game)
            : base(game)
        {
            
            // Get the current sprite batch
            spriteBatch = (SpriteBatch)
                Game.Services.GetService(typeof(SpriteBatch));

            theCamera = new Camera(1280, 1024);
            game.Services.AddService(typeof(Camera), theCamera);

            // Get the audio library
            audio = (AudioLibrary)
                Game.Services.GetService(typeof(AudioLibrary));

            

            player = new GameObject.Player(game, new Vector3(0, 0, 300));
            Components.Add(player.Health);
            Components.Add(player.Score);

            tunnel = new GameObject.Tunnel(game, 200, 200);
            skybox = new SkyBox(game);
            bulletManager = new BulletManager(game);
            asteroidManager = new AsteroidManager(game);





            pgen = new ParticleSystem(game, Game1.pe);
            game1 = game;

            Load();
        }

        protected void Load()
        {
            asteroidManager.Load();
            player.Load();
            tunnel.Load();
            skybox.Load();
            gameOverFont = Game.Content.Load<SpriteFont>("Fonts\\menuHuge");
            base.LoadContent();
        }

        public override void InitializeContent()
        {
            player.Initialize();
            skybox.Initialize();
            tunnel.Initialize();
            bulletManager.Initialize();
            asteroidManager.Initialize();
            base.InitializeContent();
        }

        public override void Show()
        {
            //////////////////////////////////////////////////////////MediaPlayer.Play(audio.Level1Music);
            gameOver = false;
            gameoverPosition.X = Game.Window.ClientBounds.Width / 2;
            gameoverPosition.Y = Game.Window.ClientBounds.Height / 2;
            base.Show();
        }

        public override void Hide()
        {
            MediaPlayer.Stop();
            base.Hide();
        }

        public bool GameOver
        {
            get { return gameOver; }
        }

        public override void Update(GameTime gameTime)
        {
            if (!gameOver)
            {
                gameOver = (player.Health.Value <= 0);
                if (gameOver)
                {
                    MediaPlayer.Stop();
                }
                player.Update(gameTime);


                tunnel.Update(gameTime);
                bulletManager.Update(gameTime);
                asteroidManager.Update(gameTime);
                pgen.Update(gameTime);
                if (tunnel.getColliding(player.Position, 1.0f))
                    player.damage(100);

                asteroidManager.checkCollisions(gameTime);
                bulletManager.CheckCollisions(asteroidManager.AllAsteroids);

                // Update all other game components
                base.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {

            Vector4 shipColor = new Vector4(0.5f + ((float)bulletManager.justShot/15f), 0.5f, 0.5f, 1);
            Game1.texNorm.Parameters["shipColor"].SetValue(shipColor);

            Game1.texNorm.Parameters["shipLight"].SetValue(35 + bulletManager.justShot * 2);

            Game1.texNorm.Parameters["shipPos"].SetValue(Player.get().Position);
            Game1.texNorm.Parameters["vpMatrix"].SetValue(theCamera.ViewMatrix * theCamera.ProjectionMatrix);
            Game1.texNorm.Parameters["wallLight"].SetValue(200);
            Game1.texNorm.Parameters["wallColor"].SetValue(Color.Orange.ToVector4());

            asteroidManager.Draw(gameTime);
            skybox.Draw(gameTime);
            tunnel.Draw(gameTime);
            player.Draw(gameTime);
            bulletManager.Draw(gameTime);
            pgen.Draw(gameTime);
            // Draw all game components
            base.Draw(gameTime);

            if (gameOver)
            {
                // TO DO: Draw the "gameover" text
                String TextToDraw = "Game Over!";
                spriteBatch.DrawString(gameOverFont, TextToDraw,
                                        new Vector2((Game.Window.ClientBounds.Width - 250) / 2, (Game.Window.ClientBounds.Height - 250)/2),
                                        Color.Yellow);
            }
        }
    }
}
