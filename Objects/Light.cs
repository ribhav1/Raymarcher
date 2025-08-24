using OpenTK.Mathematics;

namespace RayMarch.Objects
{
    //record Light(Vector3 Position, Vector4 Color);
    class Light(Vector3 _Position, Vector3 _Color, float _Radius) : IObject
    {
        public Vector3 Position { get; set; } = _Position;

        public Vector4 Color { get; } = new Vector4(_Color, 0.0f);

        public Vector3 Rotation { get; set; } = Vector3.Zero;

        public float Reflectivity { get; } = 0.0f;

        public int Type { get; } = 0;

        public float Radius = _Radius;
    }
}