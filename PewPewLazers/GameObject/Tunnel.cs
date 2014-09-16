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
    public class Tunnel : DrawableGameComponent
    {
        Random rand;

        VertexPositionNormalTexture[] mesh;
        VertexPositionNormalTexture[,] buffer;

        int primitives = 0;
        Camera cam;

        private Texture2D texture;

        private VertexDeclaration positionNormalTexture;

        int width;
        int viewdistance;
        int posZ;
        int counter;
        int cycleA = 0;
        int cycleB = 0;
        Game game;
        public Tunnel(Game game, int width, int viewdistance)
            : base(game)
        {
            this.game = game;
            this.width = width;
            this.viewdistance = viewdistance;
            posZ = viewdistance;
            counter = 0;
            rand = new Random();
            buildTerrain();
            buildBufferTerrain(50, 30);
        }

        public void Load()
        {
            texture = Game.Content.Load<Texture2D>("Images\\rocktexture");
            base.LoadContent();
        }

        public override void Initialize()
        {
            cam = Game.Services.GetService(typeof(Camera)) as Camera;

            

            base.Initialize();
            positionNormalTexture = new VertexDeclaration(GraphicsDevice,

                      VertexPositionNormalTexture.VertexElements);

        }


        private void buildTerrain()
        {
            VertexPositionNormalTexture[,] grid = new VertexPositionNormalTexture[width, viewdistance];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < viewdistance; j++)
                {
                    grid[i, j] = new VertexPositionNormalTexture(
                        new Vector3(i, 0, j),
                        Vector3.Zero,
                        new Vector2(
                            (float)i / (float)(width - 1),
                            (float)j / (float)(viewdistance)));

                }
            for (int j = 0; j < viewdistance; j++)
                randomize(grid, j, 200, 0);
            for (int j = 0; j < viewdistance; j++)
                lathe(grid, j, 0, 100);

            for (int j = 0; j < viewdistance; j++)
                calculateNormals(grid, j, 0);

            mesh = new VertexPositionNormalTexture[(viewdistance) * (2 * (width + 1))];
            primitives = (viewdistance) * (width + 1) * 2 - 2;

            for (int j = 0; j < viewdistance - 1; j++)
                generateMeshStrip(grid, mesh, j, j);

        }

        private void buildBufferTerrain(int bufferSize, int updateDistance)
        {
            buffer = new VertexPositionNormalTexture[width, bufferSize];

            for (int j = 0; j < bufferSize; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    buffer[i, j] = new VertexPositionNormalTexture(
                        new Vector3(i, 0, posZ),
                        Vector3.Zero,
                        new Vector2(
                            (float)i / (float)(width - 1),
                            (float)posZ / (float)(posZ)));

                }
                posZ++;
            }

            for (int j = 0; j < updateDistance; j++)
                randomize(buffer, j, 200, 0);
            for (int j = 0; j < updateDistance; j++)
                lathe(buffer, j, 0, 100);
            for (int j = 0; j < updateDistance; j++)
                calculateNormals(buffer, j, 0);
        }


        public override void Update(GameTime time)
        {

            counter++;
            while (Player.get().Position.Z + viewdistance > posZ-50)
            {
                //put strip into mesh

                generateMeshStrip(buffer, mesh, cycleA, cycleB);
                for (int i = 0; i < width; i++)
                {
                    buffer[i, cycleB] = new VertexPositionNormalTexture(
                        new Vector3(i, 0, posZ),
                        Vector3.Zero,
                        new Vector2(
                            (float)i / (float)(width - 1),
                            (float)(posZ % 100) / (float)(100)));

                }

                randomize(buffer, 30, 20, cycleB);

                calculateNormals(buffer, 2, cycleB);
                lathe(buffer, 5, cycleB, 10.0f);
                cycleA = (cycleA + 1) % viewdistance;
                cycleB = (cycleB + 1) % buffer.GetLength(1);
                posZ++;
            }
        }

        private void lathe(VertexPositionNormalTexture[,] buffer, int strip, int cycleB, float radius)
        {

            int height = buffer.GetLength(1);
            int j = (strip + cycleB) % height;
            for (int i = 0; i < width; i++)
            {
                Vector3 oldPos = buffer[i, j].Position;
                float distance = radius + oldPos.Y;
                buffer[i, j].Position = new Vector3(
                    (float)Math.Cos(-MathHelper.TwoPi * (width - 1 - i) / (width - 1)) * distance,
                    (float)Math.Sin(-MathHelper.TwoPi * (width - 1 - i) / (width - 1)) * distance,
                    oldPos.Z);
            }
        }

        //private Color randomColor(float lightness)
        //{
        //    byte dividend = (byte)(Math.Max(0, lightness));

        //    if (dividend == 0)
        //        dividend = 1;
        //    Byte[] rgb = new Byte[3];

        //    rand.NextBytes(rgb);
        //    //rgb[0] = dividend;
        //    rgb[1] /= dividend;
        //    rgb[2] = 0;
        //    return new Color(rgb[0], rgb[1], rgb[2]);
        //}

        private float randomEquation(float eww)
        {
            float answer = 0;
            int max = rand.Next(0, 3) * 2;

            for (int i = 0; i < max; i++)
            {
                if (rand.Next(0, 1) == 1)
                {
                    if (i % 2 == 0)
                    {
                        answer += (float)(Math.Pow(eww, i) * rand.NextDouble() * 10);
                    }
                    else
                    {

                        answer += (float)(Math.Pow(eww, i) * rand.NextDouble() * -10);
                    }
                }
            }
            return answer * 0.2f + (eww * eww * 0.8f);
        }

        private void randomize(VertexPositionNormalTexture[,] grid, int strip, float falloffdist, int cyclePosition)
        {
            int bumpyfactor = 100; //higher is less amount of humps
            float humpmax = 30; //hump high
            float humpmin = -1; //hump low

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            int j = (cyclePosition + strip) % height;
            for (int i = 0; i < width; i++) //remove ends from randomization
            {
                if (rand.Next(0, bumpyfactor) == 0)
                {
                    grid[i, j].Position.Y = (float)(rand.NextDouble() * (humpmax - humpmin) + humpmin);
                    Vector2 orig = new Vector2(grid[i, j].Position.X, grid[i, j].Position.Z);
                    for (int asi = (int)(i - falloffdist); asi <= i + falloffdist; asi++)
                    {
                        int si = (asi + width) % (width);
                        for (int mj = cyclePosition; mj < height + cyclePosition; mj++)
                        {
                            int yay = 0;
                            if (asi < 0)
                                yay = -width;
                            else if (asi > width - 1)
                                yay = width;
                            int sj = mj % height;
                            if (si == i && sj == j)
                                continue;
                            Vector2 nonorig = new Vector2(grid[si, sj].Position.X + yay, grid[si, sj].Position.Z);

                            float magnitude = Vector2.Distance(orig, nonorig);
                            if (magnitude < falloffdist)
                            {
                                float rawr = magnitude / falloffdist;
                                float ratio = randomEquation(rawr);

                                grid[si, sj].Position.Y =
                                    grid[si, sj].Position.Y * (ratio) +
                                    grid[i, j].Position.Y * (1 - ratio);

                            }
                        }
                    }
                }
            }
        }

        private void calculateNormals(VertexPositionNormalTexture[,] grid, int strip, int cyclePosition)
        {

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            int j = (strip + cyclePosition) % height;
            int jplus = (j + 1) % height;
            int jminus = (j + height - 1) % height;

            Vector3 normal;
            for (int i = 0; i < width; i++)
            {
                Vector3 a, b, c, d;
                a = i != 0 ? grid[i - 1, j].Position : grid[i, j].Position + Vector3.Left;
                b = j != cyclePosition ? grid[i, jminus].Position : grid[i, j].Position + Vector3.Forward;
                c = i != width - 1 ? grid[i + 1, j].Position : grid[i, j].Position + Vector3.Right;
                d = j != cyclePosition - 1 % height ? grid[i, jplus].Position : grid[i, j].Position + Vector3.Backward;

                normal = Vector3.Cross(d - b, c - a);
                normal.Normalize();
                grid[i, j].Normal = normal;
            }
        }

        private void generateMeshStrip(VertexPositionNormalTexture[,] buffer, VertexPositionNormalTexture[] mesh, int cycleA, int cycleB)
        {
            int width = buffer.GetLength(0);
            int height = buffer.GetLength(1);



            int stride = (2 * (width + 1));

            int y = cycleB;
            mesh[cycleA * stride] = buffer[0, y]; //first point doubled

            bool up = false; //toggles between up down
            int x = 0; //easymode iteration
            for (int i = cycleA * stride + 1; i < (cycleA + 1) * (stride) - 3; i++)
            {
                if (!up)
                {
                    mesh[i] = buffer[x, y];
                }
                else
                {
                    mesh[i] = buffer[x, (y + 1) % height];
                    x++;
                }
                up = !up;
            }
            mesh[(cycleA + 1) * (stride) - 3] = buffer[0, (y) % height];
            mesh[(cycleA + 1) * (stride) - 2] = buffer[0, (y + 1) % height]; //last point doubled
            mesh[(cycleA + 1) * (stride) - 1] = buffer[0, (y + 1) % height]; //last point doubled            
            mesh[(cycleA + 1) * (stride) - 3].TextureCoordinate.X = 1.0f;
            mesh[(cycleA + 1) * (stride) - 2].TextureCoordinate.X = 1.0f;
            mesh[(cycleA + 1) * (stride) - 1].TextureCoordinate.X = 1.0f;
        }

        private void TextureShader(PrimitiveType primitiveType,
                           VertexPositionNormalTexture[] vertexData,
                           int numPrimitives)
        {
            //textureEffect.Parameters["playerPos"].SetValue(Player.get().Position);
            //textureEffect.Parameters["counter"].SetValue((float)(counter % 1000) / 1000f);
            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;


            Game1.texNorm.Begin(); // begin using Texture.fx
            Game1.texNorm.Techniques[0].Passes[0].Begin();

            // set drawing format and vertex data then draw surface
            GraphicsDevice.VertexDeclaration = positionNormalTexture;
            GraphicsDevice.DrawUserPrimitives
                                    <VertexPositionNormalTexture>(
                                    primitiveType, vertexData, 0, numPrimitives);

            Game1.texNorm.Techniques[0].Passes[0].End();
            Game1.texNorm.End(); // stop using Textured.fx
        }

        public Boolean getColliding(Vector3 point, float radius)
        {
            float radiussq = radius * radius;
            for (int i = 0; i < mesh.Length; i++)
            {
                if (Math.Abs(mesh[i].Position.Z - point.Z) < radius)
                {
                   // i += (2 * (width + 1)) - 1;
                   // continue;
                }
                if(Vector3.DistanceSquared(mesh[i].Position, point) < radiussq)
                        return true;
            }
            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            Game1.texNorm.Parameters["textureImage"].SetValue(texture);
            Game1.texNorm.Parameters["world"].SetValue(Matrix.Identity);

            TextureShader(PrimitiveType.TriangleStrip, mesh, primitives);
            base.Draw(gameTime);
        }
    }
}