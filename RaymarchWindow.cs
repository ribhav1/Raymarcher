using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using RayMarch.Objects;
using System.Diagnostics;

namespace RayMarch
{
    class RaymarchWindow : GameWindow
    {
        private Shader _raymarchShader;
        private RenderQuad _quad;
        private Scene _currentScene;
        private Stopwatch _timer;
        private Camera _camera;

        public RaymarchWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { CursorState = CursorState.Grabbed; }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 1f);

            _raymarchShader = new Shader(File.ReadAllText("Shaders/Raymarch.vert"), File.ReadAllText("Shaders/Raymarch.frag"));
            _quad = new RenderQuad();
            // setting it up like this to allow for scene switching in the future
            _currentScene = new Scene(new List<IObject>
            {
                new Sphere(new Vector3(0,0,0), new Vector4(1,0.1f,0.1f,1), 1f, 0.9f),
                new Sphere(Vector3.Zero, new Vector4(0.1f, 1f, 0.1f, 1), 1f, 0.2f),
                new Sphere(Vector3.Zero, new Vector4(0.1f, 0.1f, 1, 1), 1f, 0.2f),
                new Sphere(Vector3.Zero, new Vector4(1, 1, 0.1f, 1), 1f, 0.2f),
                new Sphere(Vector3.Zero, new Vector4(0.1f, 1, 1, 1), 1f, 0.2f),
                //new Light(new Vector3(0, 10, 0), new Vector3(1, 1, 1), 1f),
                new Box(new Vector3(0f, -5f, 0f), new Vector4(0.1f, 0.1f, 1f, 1f), new Vector3(10f, 0.1f, 10f), new Vector3(), 0.2f)
            });
            _timer = Stopwatch.StartNew();
            _camera = new Camera();

        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            _raymarchShader.Use();
            double elapsedTime = _timer.Elapsed.TotalSeconds;

            _currentScene.Update(elapsedTime);

            _camera.UpdateFromInput(KeyboardState, MouseState, (float)args.Time);
            _camera.UploadToShader(_raymarchShader);

            _raymarchShader.SetVector3("iResolution", Size.X, Size.Y, 1f);
            _raymarchShader.SetFloat("elapsedTime", (float)elapsedTime);
            _currentScene.UploadToShader(_raymarchShader);

            _quad.Render();

            SwapBuffers();
        }
    }
}