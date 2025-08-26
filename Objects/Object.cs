//using OpenTK.Mathematics;
using System.Numerics;

namespace RayMarch.Objects
{
    public interface IObject
    {
        Vector3 Position { get; set; }

        Vector3 Rotation { get; set; }

        Vector3 LinearVelocity { get; set; }

        Vector3 AngularVelocity { get; set; }

        Vector4 Color { get; set; }

        float Reflectivity { get; set; }
        
        int Type { get; } // using ints because GLSL doesn't support strings
    }
}
