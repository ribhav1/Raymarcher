using System.Numerics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace RayMarch
{
    class Camera
    {
        public Vector3 Position = new Vector3(0, 0, 20);
        public float Yaw = (float)Math.PI; // set to Pi off of startup due to OpenGl's weird Z axis orientation
        public float Pitch = 0f;
        public float Speed = 20f;
        public float Sensitivity = 0.0005f;

        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }
        public float Fov = 60f;

        public Camera()
        {
            UpdateVectors();
        }

        private void UpdateVectors()
        {
            Forward = new Vector3(
                MathF.Cos(Pitch) * MathF.Sin(Yaw),
                MathF.Sin(Pitch),
                MathF.Cos(Pitch) * MathF.Cos(Yaw)
            );

            Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
        }

        public void UpdateFromInput(KeyboardState keys, MouseState mouse, float deltaTime, bool inScene)
        {
            if (!inScene) return;

            // mouse look
            Yaw -= mouse.Delta.X * Sensitivity;
            Pitch -= mouse.Delta.Y * Sensitivity;
            Pitch = OpenTK.Mathematics.MathHelper.Clamp(Pitch, -OpenTK.Mathematics.MathHelper.PiOver2 + 0.01f, OpenTK.Mathematics.MathHelper.PiOver2 - 0.01f);

            // keyboard movement
            Vector3 move = Vector3.Zero;
            if (keys.IsKeyDown(Keys.W)) move += Forward;
            if (keys.IsKeyDown(Keys.S)) move -= Forward;
            if (keys.IsKeyDown(Keys.D)) move += Right;
            if (keys.IsKeyDown(Keys.A)) move -= Right;
            if (keys.IsKeyDown(Keys.Space)) move += Vector3.UnitY;
            if (keys.IsKeyDown(Keys.LeftShift)) move -= Vector3.UnitY;

            if (move.LengthSquared() > 0)
                move = Vector3.Normalize(move) * Speed * deltaTime;

            Position += move;

            UpdateVectors();
        }

        public void UploadToShader(Shader shader)
        {
            shader.SetVector3("camPos", Position.X, Position.Y, Position.Z);
            shader.SetVector3("camForward", Forward.X, Forward.Y, Forward.Z);
            shader.SetVector3("camRight", Right.X, Right.Y, Right.Z);
            shader.SetVector3("camUp", Up.X, Up.Y, Up.Z);
            shader.SetFloat("fov", Fov);
        }
    }
}