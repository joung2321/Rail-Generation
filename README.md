# 🛤️Rail Generation Test
A simple project testing procedural generation of railway

This project demonstrates :
- extracting edges on XY plane from mesh
- mesh extrusion using extracted edges and unit speed curve

This project defines 4 types of unit speed curves :
- line
- vertical curve (arc)
- horizontal curve (helix)
- Clothoid (a.k.a. Euler spiral)
## 🛠️How to Use
(1) Clone this repository.
```bash
git clone https://github.com/joung2321/Rail-Generation.git
```
(2) Open **project.godot**  
(3) Run a main scene, **LabRailProfile.tscn**
## 📖Script Description
### 📐script/math
|Name|Description|
|-|-|
|FresnelIntegral.cs|calculates Fresnel integral S(x) and C(x) using Maclaurin series. Call FresnelIntegral.Approximate() first.|
|UnitSpeedCurve.cs|defines 4 types of unit speed curve C(s). These curves return a **Pose** (position and rotation) for each arc length s.|
### 🗿script/graphics
|Name|Description|
|-|-|
|RailProfile.cs|extracts edges with constraints Z = k (or X = k, Y = k).|
|RailGenerator.cs|extrudes a RailProfile along a unit speed curve.|
### 🧰script/utility
|Name|Description|
|-|-|
|FpsCamera.cs|A simple first person camera.|
## 📄License
This project is released under the MIT License.