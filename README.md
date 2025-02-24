# Unity Program - HoloLens 2 Deployment (UWP & WebGL Support)

This project is a Unity 2022-based application designed for deployment on AR devices such as the **HoloLens 2**. It support for **Universal Windows Platform (UWP)** builds.

---

## **Prerequisites**

Before starting, ensure the following tools and software are installed:

1. **Unity Editor**:
   - Version: `Unity 2022.X.X` (LTS preferred for stability).
   - Required Modules:
     - **UWP Build Support**
     - **WebGL Build Support**
2. **Microsoft Visual Studio**:
   - Version: 2019 or later (recommended: Visual Studio 2022).
   - Required Workloads:
     - **Universal Windows Platform development**
     - **Game development with Unity**
3. **HoloLens 2**:
   - AR display
4. **Windows SDK**:
   - Version: 10.0.19041.0 or later.

---

## **Setup & Configuration**

### 1. **Unity Editor Setup**
- Open the Unity Hub and install **Unity 2022.X.X** with the following modules enabled:
  - **UWP Build Support**
  - **WebGL Build Support**
- Create a new Unity project or open an existing one.

### 2. **Project Settings**
- Go to **Edit > Project Settings**, and configure the following:
  - **Player Settings**:
    - Set the target platform to:
      - **UWP** for HoloLens 2 deployment.
  - **XR Plug-in Management**:
    - Enable **Windows Mixed Reality** under the UWP tab.

### 3. **UWP Build Configuration**
- Navigate to **File > Build Settings > UWP**:
  - Target Device: **HoloLens**
  - Architecture: **ARM64** (recommended for HoloLens 2).
  - Build Type: **D3D Project**.

---

## **Deployment to HoloLens 2**

### 1. **Build the Project**
- In Unity, select **File > Build Settings**, and choose **UWP** as the target platform.
- Set the target device to **HoloLens**, and click **Build**.
- Choose a folder to save the UWP build files.

### 2. **Open in Visual Studio**
- Open the generated UWP solution (`.sln`) in Visual Studio.
- Set the build configuration to **Release** and the architecture to **ARM64**.
- Connect your HoloLens 2 device to your computer (via USB or Wi-Fi).

### 3. **Deploy to HoloLens 2**
- In Visual Studio, select **Device** as the deployment target.
- Click **Deploy** to install the app on the HoloLens 2.
- 
