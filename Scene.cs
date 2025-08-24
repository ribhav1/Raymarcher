using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using RayMarch.Objects;
using System;

namespace RayMarch
{
    class Scene
    {
        private const int MAX_OBJS = 32;
        //private const int MAX_LIGHTS = 32;
        public List<IObject> _objects;
        //private Light[] _lights;

        private Vector3 SunDirection;
        private Vector3 SunColor;

        public Scene(List<IObject> Objects)
        {
            //_objects = new List<IObject>
            //{
            //    new Sphere(new Vector3(0,0,0), new Vector4(1,0.1f,0.1f,1), 1f, 0.2f),
            //    new Sphere(Vector3.Zero, new Vector4(0.1f, 1f, 0.1f, 1), 1f, 0.2f),
            //    new Sphere(Vector3.Zero, new Vector4(0.1f, 0.1f, 1, 1), 1f, 0.2f),
            //    new Sphere(Vector3.Zero, new Vector4(1, 1, 0.1f, 1), 1f, 0.2f),
            //    new Sphere(Vector3.Zero, new Vector4(0.1f, 1, 1, 1), 1f, 0.2f),
            //    new Light(new Vector3(0, 5, 0), new Vector3(1, 1, 1), 1f),
            //    new Box(new Vector3(0f, -5f, 0f), new Vector4(0.1f, 0.1f, 1f, 1f), new Vector3(10f, 0.1f, 10f), new Vector3(), 0.2f)
            //};
            _objects = Objects;

            //_lights = new Light[] {
            //    new Light(new Vector3(0, 5, 0), new Vector3(1, 1, 1)),
            //};
        }

        public void Update(double time)
        {
            _objects[1].Position = new Vector3(3 * (float)Math.Cos(time + 0.75), 3 * (float)Math.Sin(time + 0.75), 0);
            _objects[2].Position = new Vector3(-3 * (float)Math.Cos(time - 0.75), -3 * (float)Math.Sin(time - 0.75), 0);
            _objects[3].Position = new Vector3(3 * (float)Math.Cos(time - 0.75), 3 * (float)Math.Sin(time - 0.75), 0);
            _objects[4].Position = new Vector3(-3 * (float)Math.Cos(time + 0.75), -3 * (float)Math.Sin(time + 0.75), 0);
            //_objects[6].Rotation = new Vector3(0.0f, (float)time, 0.0f);
            _objects[5].Rotation = new Vector3(0.0f, (float)time, 0.0f);
        }

        public void UploadToShader(Shader shader)
        {
            shader.SetInt("objCount", _objects.Count);
            //shader.SetInt("lightCount", _lights.Length);

            Vector3[] posArray = new Vector3[MAX_OBJS];
            Vector4[] colArray = new Vector4[MAX_OBJS];
            float[] reflectivityArray = new float[MAX_OBJS];
            int[] typeArray = new int[MAX_OBJS];
            float[] sphereRadiiArray = new float[MAX_OBJS];
            Vector3[] boxSizeArray = new Vector3[MAX_OBJS];
            Vector3[] rotationArray = new Vector3[MAX_OBJS];

            for (int i = 0; i < _objects.Count; i++)
            {
                posArray[i] = _objects[i].Position;
                colArray[i] = _objects[i].Color;
                reflectivityArray[i] = _objects[i].Reflectivity;
                typeArray[i] = _objects[i].Type;
                rotationArray[i] = _objects[i].Rotation;

                if (_objects[i] is Sphere sphere) sphereRadiiArray[i] = sphere.Radius;
                if (_objects[i] is Light light) sphereRadiiArray[i] = light.Radius;
                if (_objects[i] is Box box) boxSizeArray[i] = box.Size;
            }

            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "objPos"), _objects.Count, ref posArray[0].X);
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "objColor"), _objects.Count, ref colArray[0].X);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "objReflect"), _objects.Count, ref reflectivityArray[0]);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "objType"), _objects.Count, ref typeArray[0]);
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "objRot"), _objects.Count, ref rotationArray[0].X);

            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "sphereRadii"), _objects.Count, ref sphereRadiiArray[0]);
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "boxSizes"), _objects.Count, ref boxSizeArray[0].X);

            SunDirection = new Vector3(0.3f, 0.7f, 0.0f);
            shader.SetVector3("sunDir", SunDirection.X, SunDirection.Y, SunDirection.Z);

            SunColor = new Vector3(1.0f, 0.95f, 0.8f);
            shader.SetVector3("sunColor", SunColor.X, SunColor.Y, SunColor.Z);

            //Vector3[] lightPosArray = new Vector3[MAX_LIGHTS];
            //Vector3[] lightColArray = new Vector3[MAX_LIGHTS];
            //for (int i = 0; i < _lights.Length; i++)
            //{
            //    lightPosArray[i] = _lights[i].Position;
            //    lightColArray[i] = _lights[i].Color;
            //}
            //GL.Uniform3(GL.GetUniformLocation(shader.Handle, "lightPos"), _lights.Length, ref lightPosArray[0].X);
            //GL.Uniform3(GL.GetUniformLocation(shader.Handle, "lightColor"), _lights.Length, ref lightColArray[0].X);
        }
    }
}