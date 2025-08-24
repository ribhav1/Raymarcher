# 🌠 RayMarch – Real-Time Raymarching Renderer

A **real-time raymarching rendering engine** built with **.NET 9** and **OpenTK**.  
It uses signed distance functions (SDFs) and a custom GLSL shader pipeline to render dynamic 3D scenes with reflections, shadows, and lighting — all without traditional rasterization.

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-purple" />
  <img src="https://img.shields.io/badge/OpenTK-OpenGL_wrapper-green" />
  <img src="https://img.shields.io/badge/License-MIT-yellow" />
</p>

---

## Overview

RayMarch is designed as a **playground for real-time raymarching techniques**.  
It demonstrates how to build a custom rendering engine with:
- **Signed Distance Fields (SDFs)** for geometric primitives
- **Dynamic lighting** with directional sunlight and emissive spheres
- **Shadows & reflections** with multi-bounce raymarching
- **Camera movement** with WASD + mouse controls
- **Scene animation** driven by elapsed time

---

## File Structure

```
RayMarch/
├── Objects/                     # Geometric primitives (Sphere, Box, Light)
├── Shaders/
│   ├── Raymarch.vert            # Vertex shader
│   └── Raymarch.frag            # Fragment shader (raymarching logic)
├── Camera.cs                    # Camera movement
├── Program.cs                   # Entry point
├── RaymarchWindow.cs            # OpenTK GameWindow for rendering
├── RenderQuad.cs                # Fullscreen quad renderer
├── Scene.cs                     # Scene object management & shader uploads
├── Shader.cs                    # GLSL shader loader and uniform helpers
├── RayMarch.csproj
└── RayMarch.sln
```

---

## Installation & Usage

### Prerequisites

- **.NET 9 SDK**
- **OpenTK 4** (installed via NuGet)
- GPU with **OpenGL 3.3+ support**

### Run the Renderer

1. Clone the repository:
   ```bash
   git clone https://github.com/ribhav1/RayMarch.git
   cd RayMarch
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the project:
   ```bash
   dotnet run --project RayMarch
   ```

The renderer will launch in a 1920×1080 window.

---

## Controls

| Key / Mouse        | Action                        |
|--------------------|-------------------------------|
| `W / A / S / D`    | Move forward, left, back, right |
| `Space`            | Move upward                  |
| `Left Shift`       | Move downward                |
| `Mouse Movement`   | Look around (yaw & pitch)    |

---

## Shader Pipeline

- **Raymarching**: Each pixel casts a ray into the scene, stepping forward along the distance field until an object is hit or max distance is reached.  
- **Lighting**: Combines contributions from emissive lights and a directional sun source.  
- **Shadows**: Rays cast towards lights determine occlusion.  
- **Reflections**: Secondary rays are cast when surfaces are reflective.  
- **Environment**: If no object is hit, a sky gradient is used as background.  

---

## Example Scene

The default scene includes:
- Rotating **spheres** orbiting around the origin
- A reflective **floor box** for grounding
- Configurable **colors, reflectivity, and positions**

You can extend the `Scene.cs` file to add more primitives or animation logic.

---

## Contributing

Pull requests are welcome! If you’d like to contribute:
1. Fork this repository
2. Create a feature branch (`git checkout -b feature/NewShape`)
3. Commit your changes (`git commit -m 'Add torus SDF'`)
4. Push to the branch (`git push origin feature/NewShape`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License.  
See the [LICENSE](LICENSE) file for details.

---

## Author

Created by [Ribhav Malhotra](https://github.com/ribhav1)
