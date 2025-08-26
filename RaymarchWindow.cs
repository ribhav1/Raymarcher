using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using RayMarch.Objects;
using System.Diagnostics;
using ImGuiNET;
using System.Numerics;

namespace RayMarch
{
    class RaymarchWindow : GameWindow
    {
        private Shader _raymarchShader;
        private RenderQuad _quad;
        public Scene _currentScene;
        private Stopwatch _timer;
        private Camera _camera;
        private ImGuiController _imgui;

        private bool _showSceneControls = true;
        private bool _inScene = false;

        public RaymarchWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { /*CursorState = CursorState.Grabbed;*/ }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0f, 0f, 0f, 1f);

            _imgui = new ImGuiController(Size.X, Size.Y);
            ImGui.GetIO().FontGlobalScale = 2f;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

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
                new Box(new Vector3(0f, -5f, 0f), new Vector4(0.1f, 0.1f, 1f, 1f), new Vector3(10f, 0.1f, 10f), new Vector3(), 0.2f),
                //new Light(new Vector3(0, 10, 0), new Vector3(1, 1, 1), 1f),

            });
            _currentScene.SetUpdate(DemoSceneUpdate);
            _timer = Stopwatch.StartNew();
            _camera = new Camera();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);

            _imgui.WindowResized(e.Width, e.Height);
        }

        // game logic
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            UpdateFrequency = 0.0;

            // switch between in viewport controls and ui
            if (MouseState.IsButtonReleased(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button1))
            {
                _inScene = true;
                this.CursorState = CursorState.Grabbed;
            }
            if (KeyboardState.IsKeyReleased(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                _inScene = false;
                this.CursorState = CursorState.Normal;
            }

            // update object animations and camera
            double elapsedTime = _timer.Elapsed.TotalSeconds;
            _currentScene.Update((float)args.Time, elapsedTime);
            _camera.UpdateFromInput(KeyboardState, MouseState, (float)args.Time, _inScene);
        }

        // render logic
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // shader rendering logic
            _raymarchShader.Use();

            _currentScene.UploadToShader(_raymarchShader);
            _camera.UploadToShader(_raymarchShader);
            _raymarchShader.SetVector3("iResolution", Size.X, Size.Y, 1f);
            _raymarchShader.SetFloat("elapsedTime", (float)_timer.Elapsed.TotalSeconds);

            _quad.Render();

            
            // ui rendering logic
            _imgui.Update(this, (float)args.Time);

            ImGui.Begin("Scene Controls", ref _showSceneControls);
            if (ImGui.CollapsingHeader("Performance", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text($"FPS: {1f / args.Time:0}");
                ImGui.Text($"Frame Time: {args.Time * 1000:0.00} ms");
            }
            if (ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.SliderFloat("FOV", ref _camera.Fov, 20f, 120f);
                ImGui.SliderFloat("Speed", ref _camera.Speed, 0.1f, 10f);
                ImGui.SliderFloat("Sensitivity", ref _camera.Sensitivity, 0.01f, 1f);

                Vector3 camPos = _camera.Position;
                if (ImGui.DragFloat3("Position", ref camPos))
                    _camera.Position = camPos;
            }
            if (ImGui.CollapsingHeader("Scene Objects", ImGuiTreeNodeFlags.DefaultOpen))
            {
                for (int i = 0; i < _currentScene.Objects.Count; i++)
                {
                    var obj = _currentScene.Objects[i];
                    if (ImGui.TreeNode($"Object {i}: {obj.GetType().Name}"))
                    {
                        Vector3 pos = obj.Position;
                        if (ImGui.DragFloat3("Position", ref pos, 0.1f))
                            obj.Position = pos;

                        Vector3 rot = obj.Rotation;
                        if (ImGui.DragFloat3("Rotation", ref rot, 1f))
                            obj.Rotation = rot;

                        Vector4 color = obj.Color;
                        if (ImGui.ColorEdit4("Color", ref color))
                            obj.Color = color;

                        float reflectivity = obj.Reflectivity;
                        if (ImGui.SliderFloat("Reflectivity", ref reflectivity, 0f, 1f))
                            obj.Reflectivity = reflectivity;

                        ImGui.TreePop();
                    }
                }
            }
            ImGui.End();
            _imgui.Render();

            SwapBuffers();
        }

        void DemoSceneUpdate(double time)
        {
            _currentScene.Objects[1].Position = new Vector3(3 * (float)Math.Cos(time + 0.75), 3 * (float)Math.Sin(time + 0.75), _currentScene.Objects[1].Position.Z);
            _currentScene.Objects[2].Position = new Vector3(-3 * (float)Math.Cos(time - 0.75), -3 * (float)Math.Sin(time - 0.75), _currentScene.Objects[2].Position.Z);
            _currentScene.Objects[3].Position = new Vector3(3 * (float)Math.Cos(time - 0.75), 3 * (float)Math.Sin(time - 0.75), _currentScene.Objects[3].Position.Z);
            _currentScene.Objects[4].Position = new Vector3(-3 * (float)Math.Cos(time + 0.75), -3 * (float)Math.Sin(time + 0.75), _currentScene.Objects[4].Position.Z);
            _currentScene.Objects[5].AngularVelocity = new Vector3(0, 1, 0);
        }

    }
}