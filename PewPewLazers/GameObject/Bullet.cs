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
    public class Bullet : DrawableGameComponent
    {
        Camera cam;
        private const float EDGE = 4.0f;
        int index;
        Vector3 position;
        Vector3 velocity;
        bool alive;


        private Texture2D bulletTexture;
        private VertexPositionColorTexture[] bulletVerticesA;
        private VertexPositionColorTexture[] bulletVerticesB;
        private VertexPositionColorTexture[] bulletVerticesC;
        private VertexPositionColorTexture[] bulletVerticesD;
        
        private Matrix rotation;

        // load and access Texture.fx shader
        private Effect textureEffect;          // shader object 
        private EffectParameter textureEffectWVP;       // cumulative matrix w*v*p
        private EffectParameter textureEffectImage;     // texture parameter

        private VertexDeclaration positionColorTexture;

        float elapsedGameTime;

        public Bullet(Game game, Vector3 pos, Vector3 velocity)
            : base(game)
        {
            position = pos;
            this.velocity = velocity;

            Vector3 normalVel = Vector3.Normalize(velocity - Player.get().Velocity);
            rotation = Matrix.CreateFromYawPitchRoll(normalVel.X, -normalVel.Y, normalVel.Z);

            elapsedGameTime = 0;
            alive = true;
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

        public void Load()
        {
            bulletTexture = Game.Content.Load<Texture2D>("Images\\terrain1");
            base.LoadContent();
        }

        public override void Initialize()
        {
            cam = Game.Services.GetService(typeof(Camera)) as Camera;

            textureEffect = Game.Content.Load<Effect>("Shaders\\Texture");
            textureEffectWVP = textureEffect.Parameters["wvpMatrix"];
            textureEffectImage = textureEffect.Parameters["textureImage"];
            InitializeBullet();
            base.Initialize();
            positionColorTexture = new VertexDeclaration(GraphicsDevice, VertexPositionColorTexture.VertexElements);
        }

        private void InitializeBullet()
        {
            Vector3 pos = Vector3.Zero;
            Vector2 uv = Vector2.Zero;
            Color color = Color.Red;
            color.A = 128;
            // set position, image, and color data for each vertex in rectangle

            bulletVerticesA = new VertexPositionColorTexture[4];
            bulletVerticesB = new VertexPositionColorTexture[4];
            bulletVerticesC = new VertexPositionColorTexture[4];
            bulletVerticesD = new VertexPositionColorTexture[4];

            bulletVerticesA[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), color, new Vector2(0,0));
            bulletVerticesB[0] = bulletVerticesA[0];
            bulletVerticesC[0] = bulletVerticesA[0];
            bulletVerticesD[0] = bulletVerticesA[0];

            bulletVerticesA[3] = new VertexPositionColorTexture(new Vector3(0, 0, 8), color, new Vector2(0, 0));
            bulletVerticesB[3] = bulletVerticesA[3];
            bulletVerticesC[3] = bulletVerticesA[3];
            bulletVerticesD[3] = bulletVerticesA[3];

            bulletVerticesA[1] = new VertexPositionColorTexture(new Vector3(0.3f, 0, 6), color, new Vector2(0, 0));
            bulletVerticesB[1] = new VertexPositionColorTexture(new Vector3(0, 0.3f, 6), color, new Vector2(0, 0));
            bulletVerticesC[1] = new VertexPositionColorTexture(new Vector3(-0.3f, 0, 6), color, new Vector2(0, 0));
            bulletVerticesD[1] = new VertexPositionColorTexture(new Vector3(0, -0.3f, 6), color, new Vector2(0, 0));

            bulletVerticesA[2] = bulletVerticesB[1];
            bulletVerticesB[2] = bulletVerticesC[1];
            bulletVerticesC[2] = bulletVerticesD[1];
            bulletVerticesD[2] = bulletVerticesA[1];

        }

        public override void Update(GameTime gameTime)
        {
            elapsedGameTime += gameTime.ElapsedGameTime.Milliseconds;
            position += velocity;
            if (elapsedGameTime > 700.0f)
            {
                alive = false;
            }
            base.Update(gameTime);
        }

        private void TextureShader()
        {
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            textureEffect.Begin(); // begin using Texture.fx
            textureEffect.Techniques[0].Passes[0].Begin();

            // set drawing format and vertex data then draw surface
            GraphicsDevice.VertexDeclaration = positionColorTexture;
            GraphicsDevice.DrawUserPrimitives
                                    <VertexPositionColorTexture>(
                                    PrimitiveType.TriangleStrip, bulletVerticesA, 0, 2);
            GraphicsDevice.DrawUserPrimitives
                                    <VertexPositionColorTexture>(
                                    PrimitiveType.TriangleStrip, bulletVerticesB, 0, 2);
            GraphicsDevice.DrawUserPrimitives
                                    <VertexPositionColorTexture>(
                                    PrimitiveType.TriangleStrip, bulletVerticesC, 0, 2);
            GraphicsDevice.DrawUserPrimitives
                                    <VertexPositionColorTexture>(
                                    PrimitiveType.TriangleStrip, bulletVerticesD, 0, 2);

            textureEffect.Techniques[0].Passes[0].End();
            textureEffect.End(); // stop using Textured.fx
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
        }

        private void DrawBullet()
        {
            Matrix world = Matrix.Identity;
            world *= rotation;
            world *= Matrix.CreateTranslation(position);
            // 4: set shader variables
            textureEffectWVP.SetValue(world * cam.ViewMatrix
            * cam.ProjectionMatrix);
            textureEffectImage.SetValue(bulletTexture);
            // 5: draw object - primitive type, vertices, # primitives
            TextureShader();
        }

        public override void Draw(GameTime gameTime)
        {
            DrawBullet();
            base.Draw(gameTime);
        }

    }
}
