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

namespace PewPewLazers.GameObject
{
    public class SkyBox : DrawableGameComponent
    {
        private const float BOUNDARY = 1000.0f;
        private const float EDGE = BOUNDARY * 2.0f;
        private VertexPositionColorTexture[] skyVertices = new VertexPositionColorTexture[4];
        private Texture2D frontTexture, backTexture, leftTexture, rightTexture, skyTexture, bottomTexture;
        private Camera cam;
        // load and access Texture.fx shader
        private Effect textureEffect;          // shader object 
        private EffectParameter textureEffectWVP;       // cumulative matrix w*v*p
        private EffectParameter textureEffectImage;     // texture parameter

        private VertexDeclaration positionColorTexture;

        public SkyBox(Game game)
            :base(game)
        {
        }
        public void Load()
        {

            backTexture = Game1.skyTex;
            frontTexture = Game.Content.Load<Texture2D>("Images\\space512");
            leftTexture = Game.Content.Load<Texture2D>("Images\\space512");
            rightTexture = Game.Content.Load<Texture2D>("Images\\space512");
            skyTexture = Game.Content.Load<Texture2D>("Images\\space512");
            bottomTexture = Game.Content.Load<Texture2D>("Images\\space512");
            base.LoadContent();
        }

        public override void Initialize()
        {


            cam = Game.Services.GetService(typeof(Camera)) as Camera;
            // load Texture.fx and set global params
            textureEffect = Game.Content.Load<Effect>("Shaders\\Texture");
            textureEffectWVP = textureEffect.Parameters["wvpMatrix"];
            textureEffectImage = textureEffect.Parameters["textureImage"];
            InitializeSkybox();
            base.Initialize();
            positionColorTexture = new VertexDeclaration(GraphicsDevice,
                                          VertexPositionColorTexture.VertexElements);

            
            
        }

        private void InitializeSkybox()
        {
            Vector3 pos = Vector3.Zero;
            Vector2 uv = Vector2.Zero;
            Color color = Color.White;
            const float MAX = 0.997f; // offset to remove white seam at image edge
            const float MIN = 0.003f; // offset to remove white seam at image edge
            // set position, image, and color data for each vertex in rectangle
            pos.X = +EDGE; pos.Y = -EDGE; uv.X = MIN; uv.Y = MAX; //Bottom R
            skyVertices[0] = new VertexPositionColorTexture(pos, color, uv);
            pos.X = +EDGE; pos.Y = +EDGE; uv.X = MIN; uv.Y = MIN; //Top R
            skyVertices[1] = new VertexPositionColorTexture(pos, color, uv);
            pos.X = -EDGE; pos.Y = -EDGE; uv.X = MAX; uv.Y = MAX; //Bottom L
            skyVertices[2] = new VertexPositionColorTexture(pos, color, uv);
            pos.X = -EDGE; pos.Y = +EDGE; uv.X = MAX; uv.Y = MIN; //Top L
            skyVertices[3] = new VertexPositionColorTexture(pos, color, uv);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        private void TextureShader(PrimitiveType primitiveType,
                                   VertexPositionColorTexture[] vertexData,
                                   int numPrimitives)
        {
            textureEffect.Begin(); // begin using Texture.fx
            textureEffect.Techniques[0].Passes[0].Begin();

            // set drawing format and vertex data then draw surface
            GraphicsDevice.VertexDeclaration = positionColorTexture;
            GraphicsDevice.DrawUserPrimitives
                                    <VertexPositionColorTexture>(
                                    primitiveType, vertexData, 0, numPrimitives);

            textureEffect.Techniques[0].Passes[0].End();
            textureEffect.End(); // stop using Textured.fx
        }

        private void DrawSkybox()
        {
            const float DROP = -1.2f;
            // 1: declare matrices and set defaults
            Matrix world;
            Matrix rotationY = Matrix.CreateRotationY(0.0f);
            Matrix rotationX = Matrix.CreateRotationX(0.0f);
            Matrix translation = Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);
            Matrix camTranslation // move skybox with camera
            = Matrix.CreateTranslation(cam.Position.X, cam.Position.Y, cam.Position.Z);
            // 2: set transformations and also texture for each wall
            for (int i = 0; i < 6; i++)
            {
                switch (i)
                {
                    case 0: // BACK
                        translation = Matrix.CreateTranslation(0.0f, DROP, EDGE);
                        textureEffectImage.SetValue(backTexture); break;
                    case 1: // RIGHT
                        translation = Matrix.CreateTranslation(-EDGE, DROP, 0.0f);
                        rotationY = Matrix.CreateRotationY(-(float)Math.PI / 2.0f);
                        textureEffectImage.SetValue(rightTexture); break;
                    case 2: // FRONT
                        translation = Matrix.CreateTranslation(0.0f, DROP, -EDGE);
                        rotationY = Matrix.CreateRotationY((float)Math.PI);
                        textureEffectImage.SetValue(frontTexture); break;
                    case 3: // LEFT
                        translation = Matrix.CreateTranslation(EDGE, DROP, 0.0f);
                        rotationY = Matrix.CreateRotationY((float)Math.PI / 2.0f);
                        textureEffectImage.SetValue(leftTexture); break;
                    case 4: // SKY
                        translation = Matrix.CreateTranslation(0.0f, EDGE + DROP, 0.0f);
                        rotationX = Matrix.CreateRotationX(-(float)Math.PI / 2.0f);
                        rotationY =
                        Matrix.CreateRotationY(3.0f * MathHelper.Pi / 2.0f);
                        textureEffectImage.SetValue(skyTexture); break;
                    case 5: // Bottom
                        translation = Matrix.CreateTranslation(0.0f, - EDGE - DROP, 0.0f);
                        rotationX =  Matrix.CreateRotationX((float)Math.PI / 2.0f);
                        rotationY =
                        Matrix.CreateRotationY(3.0f * MathHelper.Pi / 2.0f);
                        textureEffectImage.SetValue(bottomTexture); break;
                }
                // 3: build cumulative world matrix using I.S.R.O.T. sequence
                world = rotationX * rotationY * translation * camTranslation;
                // 4: set shader variables
                textureEffectWVP.SetValue(world * cam.ViewMatrix
                * cam.ProjectionMatrix);
                // 5: draw object - primitive type, vertices, # primitives
                TextureShader(PrimitiveType.TriangleStrip, skyVertices, 2);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            DrawSkybox();
            base.Draw(gameTime);
        }
    }
}
