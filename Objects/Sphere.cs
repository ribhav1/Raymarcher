using OpenTK.Mathematics;

namespace RayMarch.Objects
{
    //record Sphere(Vector3 Position, Vector4 Color, float Reflectivity);
    class Sphere(Vector3 _Position, Vector4 _Color, float _Radius, float _Reflectivity) : IObject
    {
        public Vector3 Position { get; set; } = _Position;

        public Vector4 Color { get; } = _Color;

        public float Radius = _Radius;

        public Vector3 Rotation { get; set; } = Vector3.Zero;

        public float Reflectivity { get; } = _Reflectivity;

        public int Type { get; } = 1;
    }
}