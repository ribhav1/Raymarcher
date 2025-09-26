using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace RayMarch
{
    public class Shader
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

        public void SetMatrix3Array(string name, Matrix3[] data, int count)
        {
            int loc = GL.GetUniformLocation(Handle, name + "[0]");
            if (loc != -1)
                GL.UniformMatrix3(loc, count, false, ref data[0].Row0.X);
        }

        // i put my blind trust into these matrices
        public static Matrix3 CalculateInverseRotationMatrix(Vector3 euler)
        {
            var cx = MathF.Cos(euler.X); var sx = MathF.Sin(euler.X);
            var cy = MathF.Cos(euler.Y); var sy = MathF.Sin(euler.Y);
            var cz = MathF.Cos(euler.Z); var sz = MathF.Sin(euler.Z);

            var Rx = new Matrix3(
                1, 0, 0,
                0, cx, sx,
                0, -sx, cx
            );

            var Ry = new Matrix3(
                 cy, 0, -sy,
                 0, 1, 0,
                 sy, 0, cy
            );

            var Rz = new Matrix3(
                 cz, sz, 0,
                -sz, cz, 0,
                  0, 0, 1
            );

            var R = Rz * Ry * Rx;
            // inverse of rotation is transpose
            return Matrix3.Transpose(R);
        }
    }
}