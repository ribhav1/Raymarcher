using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
using System.Numerics;
using RayMarch.Objects;
using System;
using System.Collections.Generic;

namespace RayMarch
{
    class Scene
    {
        private const int MAX_OBJS = 32;
        private readonly List<IObject> _objects;
        public IReadOnlyList<IObject> Objects => _objects;

        private Vector3 SunDirection;
        private Vector3 SunColor;

        private Action<double> UpdateAction;

        public Scene()
        {
            _objects = new List<IObject>();
        }

        public Scene(List<IObject> Objects)
        {
            _objects = Objects;
        }

        public void AddObject(IObject addObject)
        {
            if (_objects.Count < MAX_OBJS)
            {
                _objects.Add(addObject);
            }
            else
            {
                throw new InvalidOperationException("Too many objects in scene!");
            }
        }

        public void RemoveObject(IObject removeObject)
        {
            if (_objects.Contains(removeObject))
            {
                _objects.Remove(removeObject);
            }
            else
            {
                throw new InvalidOperationException("The object you are trying to remove does not exist");
            }
        }

        public void SetUpdate(Action<double> UpdateActionFunction)
        {
            UpdateAction = UpdateActionFunction;
        }

        public void Update(float deltaTime, double time)
        {
            UpdateAction?.Invoke(time);

            foreach (IObject obj in _objects)
            {
                obj.Position += obj.LinearVelocity * deltaTime;
                obj.Rotation += obj.AngularVelocity * deltaTime;
            }
        }

        public void SetSunDirection(Vector3 direction)
        {
            SunDirection = direction;
        }

        public void SetSunColor(Vector3 color)
        {
            SunColor = color;
        }

        public void UploadToShader(Shader shader)
        {
            shader.SetInt("objCount", _objects.Count);

            Vector3[] posArray = new Vector3[MAX_OBJS];
            Vector4[] colArray = new Vector4[MAX_OBJS];
            float[] reflectivityArray = new float[MAX_OBJS];
            int[] typeArray = new int[MAX_OBJS];
            float[] sphereRadiiArray = new float[MAX_OBJS];
            Vector3[] boxSizeArray = new Vector3[MAX_OBJS];
            Vector2[] torusRadiiArray = new Vector2[MAX_OBJS];
            OpenTK.Mathematics.Matrix3[] rotInvArray = new OpenTK.Mathematics.Matrix3[MAX_OBJS];

            for (int i = 0; i < _objects.Count; i++)
            {
                posArray[i] = _objects[i].Position;
                colArray[i] = _objects[i].Color;
                reflectivityArray[i] = _objects[i].Reflectivity;
                typeArray[i] = _objects[i].Type;
                rotInvArray[i] = Shader.CalculateInverseRotationMatrix((OpenTK.Mathematics.Vector3)_objects[i].Rotation);

                if (_objects[i] is Sphere sphere) { sphereRadiiArray[i] = sphere.Radius; }
                else if (_objects[i] is Light light) { sphereRadiiArray[i] = light.Radius; }
                else if (_objects[i] is Box box) { boxSizeArray[i] = box.Size; }
                else if (_objects[i] is Torus torus) { torusRadiiArray[i] = new Vector2(torus.RadiusToroidal, torus.RadiusPoloidal); }
            }

            shader.SetMatrix3Array("objInvRotMat", rotInvArray, _objects.Count);

            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "objPos"), _objects.Count, ref posArray[0].X);
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "objColor"), _objects.Count, ref colArray[0].X);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "objReflect"), _objects.Count, ref reflectivityArray[0]);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "objType"), _objects.Count, ref typeArray[0]);

            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "sphereRadii"), _objects.Count, ref sphereRadiiArray[0]);
            GL.Uniform3(GL.GetUniformLocation(shader.Handle, "boxSizes"), _objects.Count, ref boxSizeArray[0].X);
            GL.Uniform2(GL.GetUniformLocation(shader.Handle, "torusRadii"), _objects.Count, ref torusRadiiArray[0].X);

            SunDirection = new Vector3(0.3f, 0.7f, 0.0f);
            shader.SetVector3("sunDir", SunDirection.X, SunDirection.Y, SunDirection.Z);

            SunColor = new Vector3(1.0f, 0.95f, 0.8f);
            shader.SetVector3("sunColor", SunColor.X, SunColor.Y, SunColor.Z);
        }
    }
}