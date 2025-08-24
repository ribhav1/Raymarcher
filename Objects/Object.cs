using OpenTK.Mathematics;

namespace RayMarch.Objects
{
    public interface IObject
    {
        Vector3 Position { get; set; }
        
        Vector4 Color { get; }
        
        Vector3 Rotation { get; set; }

        float Reflectivity { get; }
        
        int Type { get; } // using ints because GLSL doesn't support strings
    }
}
