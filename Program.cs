using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace RayMarch
{
    class Program
    {
        static void Main()
        {
            var native = new NativeWindowSettings
            {
                Title = "RayMarch",
                Size = new Vector2i(1920, 1080)
            };

            using var window = new RaymarchWindow(GameWindowSettings.Default, native);
            
            window.Run();
        }
    }
}