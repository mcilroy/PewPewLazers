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
    public class Camera
    {
        Matrix projection, view;
        Vector3 camPos, camTarg, camUp;
        public Camera(int windowWidth, int windowHeight)
        {
            view = Matrix.CreateLookAt(
                camPos = new Vector3(0, 0, 0),
                camTarg = new Vector3(0, 0, 1),
                camUp = new Vector3(0, 1, 0));


            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.Pi / 4.0f, //FoV Radians
                (float)windowWidth / (float)windowHeight,  //Aspect ratio
                0.1f, //Near Clip
                10000f); // Far Clip

        }

        public Vector3 Position
        {
            get
            {
                return camPos;
            }
            set
            {
                camPos = value;
            }
        }
        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.CreateLookAt(
                    camPos,
                    camTarg,
                    camUp);
            }
            set
            {
                view = value;
            }
        }
        public Matrix ProjectionMatrix
        {
            get
            {
                return projection;
            }
            set
            {
                projection = value;
            }
        }

        public void setUp(Vector3 up)
        {
            camUp = up;
        }

        public void setAt(Vector3 at)
        {
            camTarg = at;
        }

        public void setPos(Vector3 pos)
        {
            camPos = pos;
        }

        public Matrix getProjection()
        {
            return projection;
        }
    }
}