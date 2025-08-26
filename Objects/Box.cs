//using OpenTK.Mathematics;
using System.Numerics;

namespace RayMarch.Objects
{
    class Box(Vector3 _Position, Vector4 _Color, Vector3 _Size, Vector3 _Rotation, float _Reflectivity) : IObject
    {
        public Vector3 Position { get; set; } = _Position;

        public Vector3 Rotation { get; set; } = _Rotation;

        public Vector3 LinearVelocity { get; set; } = Vector3.Zero;

        public Vector3 AngularVelocity { get; set; } = Vector3.Zero;

        public Vector4 Color { get; set; } = _Color;

        public float Reflectivity { get; set; } = _Reflectivity;

        public int Type { get; } = 2;

        public Vector3 Size = _Size;
    }
}
