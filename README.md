# 🔦 Raymarcher

A high-performance **real-time [ray marching](https://en.wikipedia.org/wiki/Ray_marching) engine** built with **C#** and [**OpenTK 4**](https://opentk.net/), leveraging GLSL shaders for dynamic lighting, reflections, and scene composition.  
This project provides an interactive playground for experimenting with ray-marched primitives, camera movement, and **live scene editing via [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET)**.

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-blue" />
  <img src="https://img.shields.io/badge/OpenTK-4.9.4-green" />
  <img src="https://img.shields.io/badge/ImGui.NET-1.91.6-red" />
  <img src="https://img.shields.io/badge/License-MIT-yellow" />
</p>

---

## Overview

Raymarcher is a C# application that demonstrates real-time rendering with signed distance function (SDF) ray marching. It renders spheres, boxes, and simple light sources in a fragment shader, with support for transformations such as translation and rotation. A first-person camera is provided for navigation, and an ImGui-based interface allows direct adjustment of object and camera parameters while the program is running. The code is organized into a small framework with separate components for scene management, shader handling, object definitions, and user interface integration.

---

## Key Features


- **Camera System** - First-person camera with smooth movement and mouse look
- **Real-time Editor Interface** - ImGui-powered scene controls and parameter 
- **Multiple Primitive Support** - Spheres, boxes, and lights with full transformation support
- **Advanced Lighting Model** - Diffuse and specular Blinn-Phong shading
- **Rotation Support** - Full 3D rotations for box primitives using inverse rotation matrices

---

## Rendering Pipeline

The engine implements a sophisticated multi-bounce raymarching pipeline:

1. **Ray Generation** - Screen-space rays generated from camera parameters
2. **Distance Field Evaluation** - SDF functions calculate minimum distance to scene geometry
3. **Surface Detection** - Iterative marching until surface intersection found
4. **Normal Calculation** - Surface normals computed via gradient estimation
5. **Lighting Calculation** - Direct and indirect lighting with shadow testing
6. **Reflection Bounces** - Multi-bounce reflections for realistic material interaction

---

## Architecture

### Core Components

**Raymarching Engine**
- `RaymarchWindow.cs` - Main application window and render loop
- `Shader.cs` - OpenGL shader management and uniform handling
- `RenderQuad.cs` - Full-screen quad rendering for raymarching
- `Shaders/Raymarch.frag` - GPU raymarching implementation

**Scene Management**
- `Scene.cs` - Object management and shader data upload
- `Camera.cs` - First-person camera with input handling
- `Objects/` - Geometric primitive implementations (Sphere, Box, Light)

**User Interface**
- `ImGuiController.cs` - ImGui integration for OpenTK
- Real-time parameter adjustment and scene inspection

### File Structure

```
RayMarch/
├── Objects/
│   ├── Box.cs                    # Box primitive
│   ├── Light.cs                  # Light implementation
│   ├── Object.cs                 # Base object interface
│   └── Sphere.cs                 # Sphere primitive
├── Shaders/
│   ├── Raymarch.frag             # Core raymarching fragment shader
│   └── Raymarch.vert             # Vertex shader for full-screen quad
├── Camera.cs                     # First-person camera system
├── ImGuiController.cs            # ImGui integration
├── Program.cs                    # Application entry point
├── RaymarchWindow.cs             # Main window and render loop
├── RenderQuad.cs                 # Full-screen rendering quad
├── Scene.cs                      # Scene management and object handling
└── Shader.cs                     # OpenGL shader wrapper
```

---

## Installation & Setup

### Build Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/ribhav1/RayMarch.git
   cd RayMarch
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build and run:**
   ```bash
   dotnet run
   ```

The application will launch with a default scene containing animated spheres and a reflective floor plane.

---

## Usage Guide

### Camera Controls

- **W/A/S/D** - Move forward/left/backward/right
- **Space/Shift** - Move up/down
- **Mouse** - Look around (when in scene mode)
- **Left Click** - Enter scene control mode
- **Escape** - Exit to UI mode

### Scene Editor

The ImGui interface provides real-time control over:

- **Performance Monitoring** - FPS and frame time display
- **Camera Settings** - FOV, movement speed, and sensitivity adjustment
- **Object Properties** - Position, rotation, color, and reflectivity for each object

---

## Contributing

Contributions are welcome! Areas for improvement include:

- **New primitive types** (torus, cylinders, complex CSG operations)
- **Post-processing effects** (bloom, tone mapping, anti-aliasing)
- **Scene serialization** (save/load functionality)

### Development Guidelines

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-primitive`)
3. Implement changes with appropriate testing
4. Update documentation as needed
5. Submit a pull request with detailed description

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for complete details.

---

## Author

Created by [Ribhav Malhotra](https://github.com/ribhav1)

For questions, feedback, or collaboration ideas, feel free to reach out!
