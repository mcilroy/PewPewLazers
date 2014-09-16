using System.Linq;
using System;
using System.Collections.Generic;

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
    public class Player : DrawableGameComponent
    {
        //Player Variables
        Vector3 position;
        Vector3 velocity;
        private Health health;
        private Score score;
        Matrix world = Matrix.Identity;

        //Fire Wall
        float distanceTravelled;
        float wallSpeed;

        //Tunnel Variables
        Vector3 tunnelDireciton;
        Vector3 tunnelRadius;
        public int bulletStyle;

        //Player Model/Draw Stuff
        Camera cam;
        Model shipModel;
        Matrix[] shipMatrix;
        static Player thePlayer;
        private Texture2D targetTexture;
        private Effect textureEffect;          // shader object 
        private EffectParameter textureEffectWVP;       // cumulative matrix w*v*p
        private EffectParameter textureEffectImage;     // texture parameter
        private VertexPositionColorTexture[] targetVerticesA;
        private VertexPositionColorTexture[] targetVerticesB;
        private VertexDeclaration positionColorTexture;
        protected SpriteBatch spriteBatch = null;
        protected readonly SpriteFont font;
        protected readonly Color fontColor;

        public static Player get()
        {
            return thePlayer;
        }

        public Player(Game game, Vector3 pos)
            : base(game)
        {
            bulletStyle = 1;
            thePlayer = this;
            //Player Variables
            position = pos;
            velocity = new Vector3(0, 0, 0);

            health = new Health(game, Color.Yellow);
            health.Position = new Vector2(Game.Window.ClientBounds.Width - 180, 10);
            health.Value = 100;

            score = new Score(game, Color.Blue);
            score.Position = new Vector2(10, 10);

            //Tunnel Variables
            tunnelDireciton = Vector3.Zero;
            tunnelRadius = Vector3.Zero;

            //Fire Wall
            distanceTravelled = 0.0f;
            wallSpeed = 1.0f;

            //Drawing Stuff
            font = Game.Content.Load<SpriteFont>("Fonts\\menuSmall");
            fontColor = Color.Red;
            // Get the current spritebatch
            spriteBatch = (SpriteBatch)
                            Game.Services.GetService(typeof(SpriteBatch));
        }

        public void Load()
        {
            // load ship model
            targetTexture = Game.Content.Load<Texture2D>("Images\\target");
            shipModel = Game.Content.Load<Model>("Models\\alien1");
            shipMatrix = new Matrix[shipModel.Bones.Count];
            shipModel.CopyAbsoluteBoneTransformsTo(shipMatrix);
            base.LoadContent();
        }

        public override void Initialize()
        {
            cam = Game.Services.GetService(typeof(Camera)) as Camera; 
            textureEffect = Game.Content.Load<Effect>("Shaders\\Texture");
            textureEffectWVP = textureEffect.Parameters["wvpMatrix"];
            textureEffectImage = textureEffect.Parameters["textureImage"];

            targetVerticesA = new VertexPositionColorTexture[4];
            targetVerticesB = new VertexPositionColorTexture[4];

            base.Initialize();
            positionColorTexture = new VertexDeclaration(GraphicsDevice, VertexPositionColorTexture.VertexElements);
        }

        public void updateFireWall(GameTime gameTime)
        {
            if (wallSpeed < 3.8f)
            {
                wallSpeed += 0.0005f;
            }
            
            distanceTravelled += wallSpeed;
            Game1.texNorm.Parameters["wallPos"].SetValue(distanceTravelled);

            for (int pi = 0; pi < 40; pi++)
            {
                Vector3 vel = new Vector3(Game1.nextFloat() - 0.5f, Game1.nextFloat() - 0.5f, 0);
                vel = vel * 50;
                vel.Z = (Game1.nextFloat()) * 700;
                ParticleSystem.get().addParticle(new ParticleSystem.prVertex(
                    new Vector3((Game1.nextFloat() - 0.5f) * 20, (Game1.nextFloat() - 0.5f) * 20, distanceTravelled),
                    vel,
                    Color.Orange.ToVector4(),
                    100,
                    (float)gameTime.TotalGameTime.TotalSeconds,
                    0.1f));
            }

            if (distanceTravelled >= position.Z)
            {
                this.damage(100);
            }
        }

        public int distanceFromWall()
        {
            return (int)(position.Z - distanceTravelled);
        }

        public override void Update(GameTime gameTime)
        {
            UpdatePlayer();
            UpdateCamera();
            updateFireWall(gameTime);
            base.Update(gameTime);
        }

        private Vector3 getInputMove()
        {
            world = Matrix.Identity;
            world *= Matrix.CreateScale(0.7f);
            Vector3 move = Vector3.Zero;
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.W) && !state.IsKeyDown(Keys.S))
            {
                move.Y += 1;
                world *= Matrix.CreateRotationX(-0.2f);
            }
            else if (state.IsKeyDown(Keys.S) && !state.IsKeyDown(Keys.W))
            {
                move.Y -= 1;
                world *= Matrix.CreateRotationX(0.2f);
            }

            if (state.IsKeyDown(Keys.A) && !state.IsKeyDown(Keys.D))
            {
                move.X += 1;
                world *= Matrix.CreateRotationZ(-0.2f);
            }
            else if (state.IsKeyDown(Keys.D) && !state.IsKeyDown(Keys.A))
            {
                move.X -= 1;
                world *= Matrix.CreateRotationZ(0.2f);
            }

            if (move != Vector3.Zero)
                move.Normalize(); //stop diagonal faster
            return move;
        }
        private int getInputSpeed()
        {
            //-1 brakes, 0 normal, 1 afterburner
            int speed = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Space)) //brakes
                speed--;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift)) //afterburner
                speed++;

            return speed;
        }

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
        public Health Health
        {
            get
            {
                return health;
            }
        }
        public Score Score
        {
            get
            {
                return score;
            }
        }
        #endregion

        private void UpdatePlayer()
        {
            float speed = 0.0f;
            float friction = 0.95f;
            float acceleration = 0.1f;

            //wasd control, non accelerable
            position += getInputMove();

            //Speed Forward
            switch (getInputSpeed())
            {
                case -1: //brakes
                    speed = 0.4f;
                    break;
                case 0: //regular
                    speed = 0.8f;
                    break;
                case 1: //afterburner
                    speed = 4.0f;
                    break;
            }

            if (velocity.Z <= speed)
            {
                velocity.Z += acceleration;
            }
            else
            {
                velocity.Z *= friction;
            }

            if (velocity.Z < 0.05f)
                velocity.Z = 0;
            position += velocity;
        }

        private void UpdateCamera()
        {
            cam.setPos(position - new Vector3(0, 0, 5) + Vector3.Up);
            cam.setAt(position + Vector3.Up);
            cam.setUp(new Vector3(0, 1, 0));
        }

        void DrawModel(Model model)
        {
            // declare matrices

            world *= Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // pass wvp to shader
                    effect.World = shipMatrix[mesh.ParentBone.Index] * Matrix.CreateRotationX(MathHelper.PiOver2)*Matrix.CreateRotationZ(MathHelper.Pi) * world;
                    effect.View = cam.ViewMatrix;
                    effect.Projection = cam.ProjectionMatrix;

                    // set lighting
                    effect.EnableDefaultLighting();
                    effect.CommitChanges();
                }
                // draw object
                mesh.Draw();
            }
        }

        private void DrawReDERP()
        {
            //DERP
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


            float targetX = ((float)(0.5f - mousex / ((float)GraphicsDevice.Viewport.Width)) * planeWidth)*0.5f + position.X + velocity.X;
            float targetY = ((float)(0.5f - mousey / ((float)GraphicsDevice.Viewport.Height)) * planeHeight)*0.5f + position.Y + velocity.Y;
            float targetZ = (farDistance + 10)*0.5f + position.Z + velocity.Z*0.5f;

            targetVerticesA[0] = new VertexPositionColorTexture(new Vector3(targetX - 1, targetY - 1, targetZ), Color.White, new Vector2(0, 0));
            targetVerticesA[1] = new VertexPositionColorTexture(new Vector3(targetX - 1, targetY + 1, targetZ), Color.White, new Vector2(0, 1));
            targetVerticesA[2] = new VertexPositionColorTexture(new Vector3(targetX + 1, targetY - 1, targetZ), Color.White, new Vector2(1, 0));
            targetVerticesA[3] = new VertexPositionColorTexture(new Vector3(targetX + 1, targetY + 1, targetZ), Color.White, new Vector2(1, 1));

            targetX = ((float)(0.5f - mousex / ((float)GraphicsDevice.Viewport.Width)) * planeWidth)*0.35f + position.X + velocity.X;
            targetY = ((float)(0.5f - mousey / ((float)GraphicsDevice.Viewport.Height)) * planeHeight)*0.35f + position.Y + velocity.Y;
            targetZ = (farDistance + 10)*0.35f + position.Z + velocity.Z*0.35f;

            targetVerticesB[0] = new VertexPositionColorTexture(new Vector3(targetX - 1, targetY - 1, targetZ), Color.White, new Vector2(0, 0));
            targetVerticesB[1] = new VertexPositionColorTexture(new Vector3(targetX - 1, targetY + 1, targetZ), Color.White, new Vector2(0, 1));
            targetVerticesB[2] = new VertexPositionColorTexture(new Vector3(targetX + 1, targetY - 1, targetZ), Color.White, new Vector2(1, 0));
            targetVerticesB[3] = new VertexPositionColorTexture(new Vector3(targetX + 1, targetY + 1, targetZ), Color.White, new Vector2(1, 1));

            textureEffectWVP.SetValue(Matrix.Identity * cam.ViewMatrix
            * cam.ProjectionMatrix);
            textureEffectImage.SetValue(targetTexture);
            TextureShader();
        }

        private void TextureShader()
        {
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            textureEffect.Begin(); // begin using Texture.fx
            textureEffect.Techniques[0].Passes[0].Begin();
            GraphicsDevice.VertexDeclaration = positionColorTexture;

            GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleStrip, targetVerticesA, 0, 2);
            GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleStrip, targetVerticesB, 0, 2);

            textureEffect.Techniques[0].Passes[0].End();
            textureEffect.End(); // stop using Textured.fx
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
        }

        public override void Draw(GameTime gameTime)
        {
            DrawModel(shipModel);
            DrawReDERP();

            string TextToDraw = string.Format("Distance: {0}", this.distanceFromWall());
            spriteBatch.DrawString(font, TextToDraw,
                                    new Vector2(10, 60),
                                    fontColor);

            base.Draw(gameTime);
        }

        public void damage(int amount)
        {
            health.Value -= amount;
        }

        public void addScore(int amount)
        {
            score.Value += amount;
        }
    }
}
