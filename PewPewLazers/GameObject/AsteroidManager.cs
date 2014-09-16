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
    public class AsteroidManager : DrawableGameComponent
    {
        List<Asteroid> asteroids;
        Camera cam;
        // Constant for initial asteroid count
        private const int STARTASTEROIDCOUNT = 25;
        // Time for a new asteroid
        private const int ADDASTEROIDTIME = 200;
        protected TimeSpan elapsedTime;
        Player player;
        protected Random random;
        Model asterModel;
        Matrix[] asterMatrix;
        Model asterModel2;
        Matrix[] asterMatrix2;

        public AsteroidManager(Game game)
            : base(game)
        {
            random = new Random(GetHashCode());
            this.player = Player.get();
            asteroids = new List<Asteroid>();
        }

        public void Load()
        {
            // load ship model

            asterModel = Game.Content.Load<Model>("Models\\asteroidWorking");
            foreach (ModelMesh mesh in asterModel.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = Game1.texNorm;

            asterModel2 = Game.Content.Load<Model>("Models\\asteroidWorking2");
            foreach (ModelMesh mesh in asterModel2.Meshes)
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    meshPart.Effect = Game1.texNorm;

            asterMatrix = new Matrix[asterModel.Bones.Count];
            asterModel.CopyAbsoluteBoneTransformsTo(asterMatrix);

            asterMatrix2 = new Matrix[asterModel2.Bones.Count];
            asterModel2.CopyAbsoluteBoneTransformsTo(asterMatrix2);
            base.LoadContent();
        }

        public override void Initialize()
        {
            cam = Game.Services.GetService(typeof(Camera)) as Camera;
            asteroids.Clear();

            Start();

            for (int i = 0; i < asteroids.Count; i++)
            {
                asteroids[i].Initialize();
            }
            base.Initialize();
        }

        public void Start()
        {
            // Initialize a counter
            elapsedTime = TimeSpan.Zero;

            // Add the asteroids
            for (int i = 0; i < STARTASTEROIDCOUNT; i++)
            {
                AddNewAsteroid(random.Next(1, 3), Player.get().Position);
            }
        }

        public List<Asteroid> AllAsteroids
        {
            get
            {
                return asteroids;
            }
        }

        private void CheckForNewAsteroid(GameTime gameTime)
        {
            // Add asteroid each time
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromMilliseconds(ADDASTEROIDTIME))
            {
                elapsedTime -= TimeSpan.FromMilliseconds(ADDASTEROIDTIME);
                AddNewAsteroid(random.Next(1, 3), Player.get().Position);
            }
        }

        private Asteroid AddNewAsteroid(int modelNumber, Vector3 position)
        {
            //TO DO: add some random speed and position so they will come at the player from different angles and speed
            //They also need random scaling and rotation speeds
            float orientation = 0f;
            float rotation = (float)random.NextDouble();
            Vector3 pos = position;
            Vector3 directionAsteroid = new Vector3(0.0f, 0.0f, 0.0f);
            Asteroid newAsteroid;
            if (modelNumber == 1)
            {
                newAsteroid = new Asteroid(Game, rotation, orientation, directionAsteroid, asterModel,asterMatrix);
            }
            else
            {
                newAsteroid = new Asteroid(Game, rotation, orientation, directionAsteroid, asterModel2,asterMatrix2);
            }
                //newAsteroid.Load();
            newAsteroid.Initialize();
            asteroids.Add(newAsteroid);
            // Set the Asteroid identifier
            newAsteroid.Index = asteroids.Count - 1;

            return newAsteroid;
        }

        private void CheckForAsteroidDeaths(GameTime gameTime)
        {
            // Update Asteroid
            for (int i = 0; i < asteroids.Count; i++)
            {
                if (!asteroids[i].Alive)
                {
                    for (int pi = 0; pi < 500; pi++)
                    {
                        Vector3 vel = new Vector3(Game1.nextFloat() - 0.5f, Game1.nextFloat() - 0.5f, Game1.nextFloat() - 0.5f);
                        vel = Vector3.Normalize(vel) * 50 + vel * 100;
                        ParticleSystem.get().addParticle(new ParticleSystem.prVertex(
                            asteroids[i].Position + new Vector3(Game1.nextFloat() - 0.5f, Game1.nextFloat() - 0.5f, Game1.nextFloat() - 0.5f) * 10,
                            vel,
                            Color.Orange.ToVector4(),
                            10,
                            (float)gameTime.TotalGameTime.TotalSeconds,
                            1f));
                    }
                    asteroids.Remove(asteroids[i]);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            CheckForAsteroidDeaths(gameTime);
            CheckForNewAsteroid(gameTime);

            // Update asteroids
            for (int i = 0; i < asteroids.Count; i++)
            {
                asteroids[i].Update(gameTime);
            }

            base.Update(gameTime);
        }

        public bool CheckForCollisions(Rectangle rect)
        {
            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw the asteroids
            for (int i = 0; i < asteroids.Count; i++)
            {
                asteroids[i].Draw(gameTime);
            }

            base.Draw(gameTime);
        }

        public void checkCollisions(GameTime gameTime)
        {
            for (int i = 0; i < asteroids.Count; i++)
            {
                if (asteroids[i].Position != null)
                {
                    if (Vector3.Distance(player.Position, asteroids[i].Position) <= 5.0f)
                    {
                        asteroids[i].Alive = false;

                        player.damage(25);
                    }
                }
            }
        }
    }
}
