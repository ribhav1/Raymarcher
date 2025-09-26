using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RayMarch.Objects;
using System.Diagnostics;
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

        private Scene _demoScene;

        private bool _showSceneControls = true;
        private bool _showPerformance = true;
        private bool _showCamera = true;
        private bool _showObjects = true;
        private bool _inScene = false;
        
        private int currentItem = 0;
        private string[] objectTypes = new string[] { "Sphere", "Box", "Light", "Torus", "Capsule" };

        private Vector3 posText = Vector3.Zero;
        private Vector3 rotText = Vector3.Zero;
        private Vector4 colText = Vector4.Zero;
        private float reflText = 0;
        private float radText = 0;
        private Vector3 sizeText = Vector3.Zero;
        private float radToroidalText = 0;
        private float radPoloidalText = 0;
        private float heightText = 0;

        public RaymarchWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { /*CursorState = CursorState.Grabbed;*/ }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0f, 0f, 0f, 1f);

            _imgui = new ImGuiController(Size.X, Size.Y);
            ImGui.GetIO().FontGlobalScale = 2f;
            this.TextInput += (TextInputEventArgs e) =>
            {
                ImGui.GetIO().AddInputCharacter((uint)e.Unicode);
            };

            _raymarchShader = new Shader(File.ReadAllText("Shaders/Raymarch.vert"), File.ReadAllText("Shaders/Raymarch.frag"));
            _quad = new RenderQuad();
            // setting it up like this to allow for scene switching in the future
            _demoScene = new Scene(new List<IObject>
            {
                new Sphere(Vector3.Zero, Vector3.Zero, new Vector4(1,0.1f,0.1f,1), 0.9f, 1f),
                new Sphere(Vector3.Zero, Vector3.Zero, new Vector4(0.1f, 1f, 0.1f, 1), 0.2f, 1f),
                new Sphere(Vector3.Zero, Vector3.Zero, new Vector4(0.1f, 0.1f, 1, 1), 0.2f, 1f),
                new Sphere(Vector3.Zero, Vector3.Zero, new Vector4(1, 1, 0.1f, 1), 0.2f, 1f),
                new Sphere(Vector3.Zero, Vector3.Zero, new Vector4(0.1f, 1, 1, 1), 0.2f, 1f),
                new Box(new Vector3(0f, -5f, 0f), new Vector3(0,0,0), new Vector4(0.1f, 0.1f, 1f, 1f), 0.2f, new Vector3(10f, 0.1f, 10f)),
                new Torus(new Vector3(-3f, 0f, 0f), Vector3.Zero, new Vector4(1f, 0.1f, 1, 1), 0.2f, 1.5f, 0.25f),
                new Capsule(new Vector3(5f, 0f, 0f), Vector3.Zero, new Vector4(1f, 1f, 0.1f, 1), 0.2f, 0.5f, 2f)

            });
            _demoScene.SetUpdate(DemoSceneUpdate);
            _currentScene = _demoScene;
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
                if (!ImGui.GetIO().WantCaptureMouse)
                {
                    _inScene = true;
                    this.CursorState = CursorState.Grabbed;
                }
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
            if (!_inScene)
            {
                _imgui.Update(this, (float)args.Time);
            }
            else
            {
                ImGui.NewFrame();
            }

            // main menu ui
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New")) { _currentScene = new Scene(); }
                    if (ImGui.BeginMenu("Open")) 
                    { 
                        if (ImGui.MenuItem("From Files")) { _currentScene = SaveLoader.LoadSceneFromFies() ?? _currentScene; }
                        if (ImGui.MenuItem("Demo")) { _currentScene = _demoScene; }
                        ImGui.EndMenu();
                    }
                    if (ImGui.MenuItem("Save")) { SaveLoader.SaveCurrentScene(_currentScene); }
                    if (ImGui.MenuItem("Exit")) { Close(); }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.BeginMenu("Scene Controls"))
                    {
                        ImGui.Checkbox("Show", ref _showSceneControls);
                        ImGui.Separator();

                        ImGui.Checkbox("Performance", ref _showPerformance);
                        ImGui.Checkbox("Camera", ref _showCamera);
                        ImGui.Checkbox("Objects", ref _showObjects);

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }            

                ImGui.EndMainMenuBar();
            }

            // scene controls widget
            if (_showSceneControls)
            {
                ImGui.Begin("Scene Controls");
                if (_showPerformance && ImGui.CollapsingHeader("Performance", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Text($"FPS: {1f / args.Time:0}");
                    ImGui.Text($"Frame Time: {args.Time * 1000:0.00} ms");
                }

                if (_showCamera && ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.SliderFloat("FOV", ref _camera.Fov, 20f, 120f);
                    ImGui.SliderFloat("Speed", ref _camera.Speed, 1f, 40f);
                    float sliderScale = _camera.Sensitivity * 1000f;
                    if (ImGui.SliderFloat("Sensitivity", ref sliderScale, 0f, 1f))
                    {
                        _camera.Sensitivity = sliderScale / 1000f;
                    }

                    Vector3 camPos = _camera.Position;
                    if (ImGui.DragFloat3("Position", ref camPos))
                        _camera.Position = camPos;
                }

                if (_showObjects &&  ImGui.CollapsingHeader("Scene Objects", ImGuiTreeNodeFlags.DefaultOpen))
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
                            if (ImGui.DragFloat3("Rotation", ref rot, 0.1f))
                                obj.Rotation = rot;

                            Vector4 color = obj.Color;
                            if (ImGui.ColorEdit4("Color", ref color))
                                obj.Color = color;

                            float reflectivity = obj.Reflectivity;
                            if (ImGui.SliderFloat("Reflectivity", ref reflectivity, 0f, 1f))
                                obj.Reflectivity = reflectivity;

                            if (ImGui.Button("Remove Object"))
                            {
                                _currentScene.RemoveObject(_currentScene.Objects[i]);
                            }

                            ImGui.TreePop();
                        }
                    }
                    if (ImGui.CollapsingHeader("Add Object"))
                    {
                        ImGui.Combo("Type", ref currentItem, objectTypes, objectTypes.Length);
                        ImGui.InputFloat3("Create Position", ref posText);
                        ImGui.InputFloat3("Create Rotation", ref rotText);
                        ImGui.InputFloat("Create Reflectivity", ref reflText, 0.1f);
                        ImGui.ColorEdit4("Create Color", ref colText);

                        switch (objectTypes[currentItem])
                        {
                            case "Sphere":
                            case "Light":
                                ImGui.InputFloat("Radius", ref radText, 1f);
                                break;
                            case "Box":
                                ImGui.InputFloat3("Size", ref sizeText);
                                break;
                            case "Torus":
                                ImGui.InputFloat("Toroidal Radius", ref radToroidalText, 1f);
                                ImGui.InputFloat("Poloidal Radius", ref radPoloidalText, 1f);
                                break;
                            case "Capsule":
                                ImGui.InputFloat("Radius", ref radText, 1f);
                                ImGui.InputFloat("Height", ref heightText, 1f);
                                break;
                        }

                        if (ImGui.Button("Add"))
                        {
                            reflText = Math.Clamp(reflText, 0f, 1f);
                            IObject addObject = null;

                            switch (objectTypes[currentItem])
                            {
                                case "Sphere":
                                case "Light":
                                    addObject = new Sphere(posText, rotText, colText, reflText, radText);
                                    break;
                                case "Box":
                                    addObject = new Box(posText, rotText, colText, reflText, sizeText);
                                    break;
                                case "Torus":
                                    addObject = new Torus(posText, rotText, colText, reflText, radToroidalText, radPoloidalText);
                                    break;
                                case "Capsule":
                                    addObject = new Capsule(posText, rotText, colText, reflText, radText, heightText);
                                    break;
                            }

                            if (addObject != null) _currentScene.AddObject(addObject);
                        }
                    }

                }
                ImGui.End();
            }

            _imgui.Render();

            SwapBuffers();
        }

        void DemoSceneUpdate(double time)
        {
            if (_currentScene.Objects.Count < 8) return;
            _currentScene.Objects[1].Position = new Vector3(3 * (float)Math.Cos(time + 0.75), 3 * (float)Math.Sin(time + 0.75), _currentScene.Objects[1].Position.Z);
            _currentScene.Objects[2].Position = new Vector3(-3 * (float)Math.Cos(time - 0.75), -3 * (float)Math.Sin(time - 0.75), _currentScene.Objects[2].Position.Z);
            _currentScene.Objects[3].Position = new Vector3(3 * (float)Math.Cos(time - 0.75), 3 * (float)Math.Sin(time - 0.75), _currentScene.Objects[3].Position.Z);
            _currentScene.Objects[4].Position = new Vector3(-3 * (float)Math.Cos(time + 0.75), -3 * (float)Math.Sin(time + 0.75), _currentScene.Objects[4].Position.Z);
            _currentScene.Objects[5].AngularVelocity = new Vector3(0, 1, 0);
            _currentScene.Objects[6].Rotation = new Vector3((float)Math.Sin(time * 3) *0.25f, 0, (float)Math.Cos(time * 3) * 0.25f);
            _currentScene.Objects[7].AngularVelocity = new Vector3(1, 0, 0);
        }

    }
}