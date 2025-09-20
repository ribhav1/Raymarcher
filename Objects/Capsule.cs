using System.Numerics;

namespace RayMarch.Objects
{
    class Capsule(Vector3 _Position, Vector3 _Rotation, Vector4 _Color, float _Reflectivity, float _Radius, float _Height) : IObject
    {
        public Vector3 Position { get; set; } = _Position;

        public Vector3 Rotation { get; set; } = _Rotation;

        public Vector3 LinearVelocity { get; set; } = Vector3.Zero;

        public Vector3 AngularVelocity { get; set; } = Vector3.Zero;

        public Vector4 Color { get; set; } = _Color;

        public float Reflectivity { get; set; } = _Reflectivity;

        public int Type { get; } = 4;

        public float Radius = _Radius;

        public float Height = _Height;
    }
}