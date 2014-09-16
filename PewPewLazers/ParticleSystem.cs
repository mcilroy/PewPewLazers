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
    public class ParticleSystem : DrawableGameComponent
    {
        Camera cam;
        Effect particleEffect;
        VertexDeclaration myVertexDeclaration;
        List<prVertex> particleList;

        private static ParticleSystem me;

        public struct prVertex
        {
            public Vector3 pos;
            public Vector3 vel;
            public Vector4 color;
            public float size;
            public float lifeTime;
            public float startTime;

            public static readonly VertexElement[] vertexElements = new VertexElement[] {
                new VertexElement(0,0,VertexElementFormat.Vector3,
                    VertexElementMethod.Default, VertexElementUsage.Position,0),
                new VertexElement(0,sizeof(float)*3,VertexElementFormat.Vector3,
                    VertexElementMethod.Default, VertexElementUsage.Position,1),
                new VertexElement(0,sizeof(float)*6,VertexElementFormat.Vector4,
                    VertexElementMethod.Default, VertexElementUsage.Color,0),
                new VertexElement(0,sizeof(float)*10,VertexElementFormat.Single,
                    VertexElementMethod.Default, VertexElementUsage.PointSize,0),
                new VertexElement(0,sizeof(float)*11,VertexElementFormat.Single,
                    VertexElementMethod.Default, VertexElementUsage.PointSize,1),
                new VertexElement(0,sizeof(float)*12,VertexElementFormat.Single,
                    VertexElementMethod.Default, VertexElementUsage.PointSize,2),
            };

            public prVertex(Vector3 ppos, Vector3 pvel, Vector4 pcol, float psize, float pstartTime, float lifeTime)
            {
                this.pos = ppos;
                this.vel = pvel;
                this.color = pcol;
                this.size = psize;
                this.startTime = pstartTime;
                this.lifeTime = lifeTime;
            }
        }


        public static ParticleSystem get()
        {
            return me;
        }

        public ParticleSystem(Game game, Effect pe)
            : base(game)
        {
            me = this;
            particleList = new List<prVertex>();
            cam = Game.Services.GetService(typeof(Camera)) as Camera;
            particleEffect = pe;
        }

        public void addParticle(prVertex p)
        {
            particleList.Add(p);
        }

        public override void Draw(GameTime gameTime)
        {
            if (particleArray == null || particleArray.Length == 0)
                return;
            GraphicsDevice g = particleEffect.GraphicsDevice;
            // set vertex declaration to custom vertex format
            myVertexDeclaration = new VertexDeclaration(g, prVertex.vertexElements);

            g.VertexDeclaration = myVertexDeclaration;

            g.RenderState.PointSpriteEnable = true;
            g.RenderState.DepthBufferWriteEnable = true;
            g.RenderState.AlphaBlendEnable = true;
            g.RenderState.SourceBlend = Blend.SourceAlpha;
            g.RenderState.DestinationBlend = Blend.One;
            g.RenderState.AlphaBlendOperation = BlendFunction.Add;

            // now the actual draw

            particleEffect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            particleEffect.Parameters["World"].SetValue(Matrix.Identity);
            particleEffect.Parameters["View"].SetValue(cam.ViewMatrix);
            particleEffect.Parameters["Projection"].SetValue(cam.ProjectionMatrix);
            particleEffect.Begin();
            particleEffect.Techniques[0].Passes[0].Begin();

            g.DrawUserPrimitives<prVertex>(PrimitiveType.PointList,
                particleArray, 0, particleArray.Length);

            particleEffect.Techniques[0].Passes[0].End();
            particleEffect.End();

            g.RenderState.DepthBufferWriteEnable = true;
        }
        float _time;
        prVertex[] particleArray;
        private bool removeIf(prVertex vert)
        {
            return _time - vert.startTime > vert.lifeTime;
        }

        public override void Update(GameTime gt)
        {
            _time = (float)gt.TotalGameTime.TotalSeconds;
            particleList.RemoveAll(removeIf);
            particleArray = particleList.ToArray();
        }
    }
}
