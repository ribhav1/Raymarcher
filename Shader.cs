using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace RayMarch
{
    class Shader
    {
        public int Handle { get; private set; }

        public Shader(string vertexSrc, string fragmentSrc)
        {
            int vs = Compile(ShaderType.VertexShader, vertexSrc);
            int fs = Compile(ShaderType.FragmentShader, fragmentSrc);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vs);
            GL.AttachShader(Handle, fs);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
                throw new Exception(GL.GetProgramInfoLog(Handle));

            GL.DetachShader(Handle, vs);
            GL.DetachShader(Handle, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
        }

        private int Compile(ShaderType type, string src)
        {
            int id = GL.CreateShader(type);
            GL.ShaderSource(id, src);
            GL.CompileShader(id);
            GL.GetShader(id, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
                throw new Exception(GL.GetShaderInfoLog(id));
            return id;
        }

        public void Use() => GL.UseProgram(Handle);

        // functions to pass data to uniforms of different types
        public void SetFloat(string name, float value) => GL.Uniform1(GL.GetUniformLocation(Handle, name), value);

        public void SetVector3(string name, float x, float y, float z) => GL.Uniform3(GL.GetUniformLocation(Handle, name), x, y, z);

        public void SetInt(string name, int value) => GL.Uniform1(GL.GetUniformLocation(Handle, name), value);
    }
}