using OpenTK.Graphics.OpenGL4;

namespace RayMarch
{
    class RenderQuad
    {
        private int _vao;
        private int _vbo;

        public RenderQuad()
        {
            float[] quad = { -1f, -1f, 1f, -1f, -1f, 1f, 1f, 1f };
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}