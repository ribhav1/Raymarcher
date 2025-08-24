using OpenTK.Mathematics;

namespace RayMarch.Objects
{
    class Box(Vector3 _Position, Vector4 _Color, Vector3 _Size, Vector3 _Rotation, float _Reflectivity) : IObject
    {
        public Vector3 Position { get; set; } = _Position;

        public Vector4 Color { get; } = _Color;

        public Vector3 Size = _Size;

        public Vector3 Rotation { get; set; } = _Rotation;

        public float Reflectivity { get; } = _Reflectivity;

        public int Type { get; } = 2;
    }
}
