using System.Linq;
using System;
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
    public class Asteroid : DrawableGameComponent
    {
        Camera cam;
        int index;
        Vector3 position;
        float orientation;
        float rotation;
        Vector3 velocity;
        bool alive;
        Model asterModel;
        Matrix[] asterMatrix;
        //int modelNumber;
        protected Random random;
        public Asteroid(Game game, float rot, float orient, Vector3 velocity,Model model,Matrix[] asterMatrix)
            : base(game)
        {
            random = new Random(GetHashCode());
            PutinStartPosition(Player.get().Position);
            
            this.velocity = velocity;

            //this.modelNumber = modelNumber;
            this.asterModel = model;
            this.asterMatrix = asterMatrix;
            rotation = rot;
            orientation = orient;
            alive = true;
        }

        public void PutinStartPosition(Vector3 playerPos)
        {
            position.Z = playerPos.Z + random.Next(160,400);
            position.X = playerPos.X+random.Next(-15,15);
            position.Y = playerPos.Y + random.Next(-15, 15);
            velocity.Z = -1.0f * (float)random.NextDouble();
            velocity.X = (float)random.NextDouble() * 5.0f;
            velocity.Y = (float)random.NextDouble() * 5.0f;
        }

        //public void Load()
        //{
        //    // load ship model
        //    if (modelNumber == 1)
        //    {
        //        asterModel = Game.Content.Load<Model>("Models\\asteroidWorking");
        //    }
        //    else if (modelNumber == 2)
        //    {
        //        asterModel = Game.Content.Load<Model>("Models\\asteroidWorking2");
        //    }
        //    asterMatrix = new Matrix[asterModel.Bones.Count];
        //    asterModel.CopyAbsoluteBoneTransformsTo(asterMatrix);
        //    base.LoadContent();
        //}

        #region fields
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
            }
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public bool Alive
        {
            get { return alive; }
            set { alive = value; }
        }
        #endregion

        public override void Initialize()
        {
            cam = Game.Services.GetService(typeof(Camera)) as Camera;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Check if the meteor still visible
            Vector3 pos = Player.get().Position;
            if ((position.Z <= pos.Z - 100.0f) ||
                (position.X >= pos.X + 400.0f) ||
                (position.X <= pos.X - 400.0f) ||
                (position.Y >= pos.Y + 400.0f) ||
                (position.Y <= pos.Y - 400.0f))
            {
                PutinStartPosition(pos);
            }

            // Move meteor
            position += (velocity * (gameTime.ElapsedGameTime.Milliseconds / 1000f));
            //position.Y += velocity.Y;
            // position.X += velocity.X;
            //position.Z += velocity.Z;

            if (rotation >= 2 * Math.PI)
            {
                rotation = 0.0f;
            }
            else
            {
                rotation += 0.1f + random.Next(1);
            }
            base.Update(gameTime);
        }

        void DrawModel(Model model)
        {
            //GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
            // declare matrices
            Matrix world = Matrix.Identity;
            world *= Matrix.CreateScale(0.3f);
            world *= Matrix.CreateRotationZ(rotation/2);
            world *= Matrix.CreateRotationY(rotation/1);
            world *= Matrix.CreateTranslation(position);

            Game1.texNorm.Parameters["textureImage"].SetValue(Game1.asterTex);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["world"].SetValue(asterMatrix[mesh.ParentBone.Index] * world);
                }
                // draw object                
                mesh.Draw();
            }
            //GraphicsDevice.RenderState.CullMode = CullMode.None;
        }

        

        public override void Draw(GameTime gameTime)
        {
            if (alive)
            {
                DrawModel(asterModel);
                base.Draw(gameTime);
            }
        }

    }
}
