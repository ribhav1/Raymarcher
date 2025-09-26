using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RayMarch
{
    // made possible thanks to github.com/NogginBops/ImGui.NET_OpenTK_Sample
    public sealed class ImGuiController : IDisposable
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private int _shader;
        private int _fontTex;

        private int _locProjMtx;
        private int _locTexture;

        private int _vboSize;
        private int _eboSize;

        private IntPtr _context;

        public ImGuiController(int width, int height)
        {
            _context = ImGui.CreateContext();
            ImGui.SetCurrentContext(_context);
            var io = ImGui.GetIO();

            io.Fonts.AddFontDefault();
            ImGui.StyleColorsDark();

            io.DisplaySize = new System.Numerics.Vector2(width, height);

            CreateDeviceObjects();
            CreateFontTexture();
        }

        public void WindowResized(int width, int height)
        {
            ImGui.GetIO().DisplaySize = new System.Numerics.Vector2(width, height);
        }

        public void Update(GameWindow window, float deltaSeconds)
        {
            var io = ImGui.GetIO();
            io.DeltaTime = deltaSeconds <= 0 ? 1f / 60f : deltaSeconds;

            var mouse = window.MouseState;
            io.MouseDown[0] = mouse.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = mouse.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = mouse.IsButtonDown(MouseButton.Middle);
            io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);

            var keyboard  = window.KeyboardState;
            io.AddKeyEvent(ImGuiKey.Backspace, keyboard.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backspace));
            io.AddKeyEvent(ImGuiKey.Delete, keyboard.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Delete));
            io.AddKeyEvent(ImGuiKey.LeftArrow, keyboard.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left));
            io.AddKeyEvent(ImGuiKey.RightArrow, keyboard.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right));
            io.AddKeyEvent(ImGuiKey.UpArrow, keyboard.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up));
            io.AddKeyEvent(ImGuiKey.DownArrow, keyboard.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down));

            ImGui.NewFrame();
        }

        public void Render()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData());
        }

        private void CreateDeviceObjects()
        {
            string vertexSrc = 
            @"#version 330 core
            uniform mat4 projection_matrix;

            layout(location = 0) in vec2 in_position;
            layout(location = 1) in vec2 in_texCoord;
            layout(location = 2) in vec4 in_color;

            out vec4 color;
            out vec2 texCoord;

            void main()
            {
                gl_Position = projection_matrix * vec4(in_position, 0, 1);
                color = in_color;
                texCoord = in_texCoord;
            }";
            string fragmentSrc = 
            @"#version 330 core
            uniform sampler2D in_fontTexture;

            in vec4 color;
            in vec2 texCoord;

            out vec4 outputColor;

            void main()
            {
                outputColor = color * texture(in_fontTexture, texCoord);
            }";

            int vs = Compile(ShaderType.VertexShader, vertexSrc);
            int fs = Compile(ShaderType.FragmentShader, fragmentSrc);

            _shader = GL.CreateProgram();
            GL.AttachShader(_shader, vs);
            GL.AttachShader(_shader, fs);
            GL.LinkProgram(_shader);
            GL.GetProgram(_shader, GetProgramParameterName.LinkStatus, out int linked);
            if (linked == 0) throw new Exception(GL.GetProgramInfoLog(_shader));
            GL.DetachShader(_shader, vs);
            GL.DetachShader(_shader, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            _locProjMtx = GL.GetUniformLocation(_shader, "projection_matrix");
            _locTexture = GL.GetUniformLocation(_shader, "in_texture");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            int stride = Marshal.SizeOf<ImDrawVert>();
            var posOffset = Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos));
            var uvOffset = Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv));
            var colOffset = Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col));

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, posOffset);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, uvOffset);

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, colOffset);

            GL.BufferData(BufferTarget.ArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, 0, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            _vboSize = 0;
            _eboSize = 0;

            GL.BindVertexArray(0);
        }

        private void CreateFontTexture()
        {
            var io = ImGui.GetIO();

            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out _);

            _fontTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                          PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTex);
            io.Fonts.ClearTexData();

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            int fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
            int fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
            if (fbWidth <= 0 || fbHeight <= 0) return;

            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.Viewport(0, 0, fbWidth, fbHeight);

            var io = ImGui.GetIO();
            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            var proj = Matrix4.CreateOrthographicOffCenter(
                0, drawData.DisplaySize.X,
                drawData.DisplaySize.Y, 0,
                -1f, 1f);

            GL.UseProgram(_shader);
            GL.Uniform1(_locTexture, 0);
            GL.UniformMatrix4(_locProjMtx, false, ref proj);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmd = drawData.CmdLists[n];

                int vtxSize = cmd.VtxBuffer.Size * Marshal.SizeOf<ImDrawVert>();
                if (vtxSize > _vboSize)
                {
                    _vboSize = Math.Max(vtxSize, (int)(_vboSize * 1.5f) + 1000);
                    GL.BufferData(BufferTarget.ArrayBuffer, _vboSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vtxSize, cmd.VtxBuffer.Data);

                int idxSize = cmd.IdxBuffer.Size * sizeof(ushort);
                if (idxSize > _eboSize)
                {
                    _eboSize = Math.Max(idxSize, (int)(_eboSize * 1.5f) + 2000);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, _eboSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                }
                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, idxSize, cmd.IdxBuffer.Data);

                int indexOffset = 0;
                for (int i = 0; i < cmd.CmdBuffer.Size; i++)
                {
                    ImDrawCmdPtr pcmd = cmd.CmdBuffer[i];

                    GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);

                    var clip = pcmd.ClipRect;
                    int x = (int)clip.X;
                    int y = (int)(fbHeight - clip.W);
                    int w = (int)(clip.Z - clip.X);
                    int h = (int)(clip.W - clip.Y);
                    if (w <= 0 || h <= 0) continue;
                    GL.Scissor(x, y, w, h);

                    GL.DrawElements(PrimitiveType.Triangles,
                                    (int)pcmd.ElemCount,
                                    DrawElementsType.UnsignedShort,
                                    (IntPtr)(indexOffset * sizeof(ushort)));

                    indexOffset += (int)pcmd.ElemCount;
                }
            }

            GL.Disable(EnableCap.ScissorTest);
        }

        private static int Compile(ShaderType type, string src)
        {
            int s = GL.CreateShader(type);
            GL.ShaderSource(s, src);
            GL.CompileShader(s);
            GL.GetShader(s, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(s));
            return s;
        }

        public void Dispose()
        {
            if (_fontTex != 0) GL.DeleteTexture(_fontTex);
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_ebo != 0) GL.DeleteBuffer(_ebo);
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_shader != 0) GL.DeleteProgram(_shader);

            if (_context != IntPtr.Zero)
            {
                ImGui.DestroyContext(_context);
                _context = IntPtr.Zero;
            }
        }
    }
}