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

namespace PewPewLazers.GameObject
{
    public class BulletManager:DrawableGameComponent
    {
        public int justShot;
        private AudioLibrary audio;
        List<Bullet> bullets;
        Camera cam;
        public BulletManager(Game game)
            : base(game)
        {
            justShot = 0;
            bullets = new List<Bullet>();
            // Get the audio library
            audio = (AudioLibrary)
                Game.Services.GetService(typeof(AudioLibrary));
        }

        public override void Initialize()
        {
            cam = Game.Services.GetService(typeof(Camera)) as Camera;
            bullets.Clear();
            base.Initialize();
        }

        public List<Bullet> AllBullets
        {
            get
            {
                return bullets;
            }
        }

        private Bullet AddNewBullet()
        {
            audio.FireBullet.Play();
            //MouseState ms = Mouse.GetState();
            //float xrot = ms.X / Game.GraphicsDevice.Viewport.Width;
            //float yrot = ms.Y / Game.GraphicsDevice.Viewport.Width;

            ////angles are 45 degree extremes from forward.


            ////Game.GraphicsDevice.Viewport.Height;
            
            //Vector3 playerPos = Player.get().getPosition();
            //float side1O = ms.X - playerPos.X ;
            //float side2O = 20.0f - playerPos.Z;
            //float orientation = (float)Math.Atan2(side2O,side1O);

            //float side1R = ms.Y - playerPos.Y;
            //float side2R = 20.0f - playerPos.Z;
            //float rotation = (float)Math.Atan2(side1R, side2R);

            MouseState mouseState = Mouse.GetState();


            float angle = MathHelper.Pi / 4.0f;
            float farDistance = 100f;
            float planeWidth = farDistance * (float)Math.Tan(angle);
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            
            float planeHeight = planeWidth / aspect;
 
            float mousex = mouseState.X;
            float mousey = mouseState.Y;
            mousex = Math.Max(0, mousex);
            mousey = Math.Max(0, mousey);
            mousex = Math.Min(GraphicsDevice.Viewport.Width, mousex);
            mousey = Math.Min(GraphicsDevice.Viewport.Height, mousey);


            float firingPointX = (float)(0.5f - mousex / ((float)GraphicsDevice.Viewport.Width)) * planeWidth;
            float firingPointY = (float)(0.5f - mousey / ((float)GraphicsDevice.Viewport.Height)) * planeHeight;
            float firingPointZ = farDistance + 10;

            Vector3 firingPoint = new Vector3(firingPointX, firingPointY, firingPointZ);
            //firingPoint *= 

            Vector3 directionBullet = firingPoint;// -Player.get().getPosition();
            directionBullet.Normalize();
            directionBullet *= 3;
            directionBullet += Player.get().Velocity;// +Vector3.Backward * 0.5f;

            Bullet newBullet = null;
            if (Player.get().bulletStyle == 0)
            {
                newBullet = new Bullet(Game, Player.get().Position, directionBullet);
                newBullet.Load();
                newBullet.Initialize();
                bullets.Add(newBullet);
            }
            else if(Player.get().bulletStyle == 1)
            {
                newBullet = new Bullet(Game, Player.get().Position - Vector3.Left / 2, directionBullet);
                newBullet.Load();
                newBullet.Initialize();
                bullets.Add(newBullet);
                newBullet = new Bullet(Game, Player.get().Position + Vector3.Left / 2, directionBullet);
                newBullet.Load();
                newBullet.Initialize();
                bullets.Add(newBullet);
            }
            
            // Set the bullet identifier
            newBullet.Index = bullets.Count - 1;

            return newBullet;
        }
        bool released = true;
        private void CheckForNewBullet(GameTime gameTime)
        {
            if(justShot > 0)
                justShot--;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && released)
            {
                AddNewBullet();
                justShot = 5;
                released = false;
            }
            if (Mouse.GetState().LeftButton == ButtonState.Released)
            {
                released = true;
            }
        }

        private void CheckForBulletDeaths(GameTime gameTime)
        {
            // Update bullets
            for (int i = 0; i < bullets.Count; i++)
            {   
                if(!bullets[i].Alive){
                    bullets.Remove(bullets[i]);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            CheckForBulletDeaths(gameTime);
            CheckForNewBullet(gameTime);

            // Update bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Update(gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw the bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        public void CheckCollisions(List<Asteroid> asteroids)
        {
            for (int i = 0; i < asteroids.Count; i++)
            {
                for (int j = 0; j < bullets.Count; j++)
                {
                    if (asteroids[i].Position != null && bullets[j].Position != null)
                    {
                        if (Vector3.Distance(asteroids[i].Position, bullets[j].Position) <= 5.0f)
                        {
                            asteroids[i].Alive = false;
                            bullets[j].Alive = false;
                            Player.get().addScore(3);
                        }
                    }
                }
            }
        }
    }
}
