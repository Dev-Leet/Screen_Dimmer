# **Screen Dimmer for Windows**

**A lightweight, feature-rich utility to reduce screen brightness, adjust color temperature, and filter blue light beyond your system's default limits. Perfect for late-night work, media consumption, or reducing eye strain.**

<img width="320" height="280" alt="Screenshot 2025-09-10 184946" src="https://github.com/user-attachments/assets/80b24f88-74a7-4cef-a13d-81c116906b02" />


## **✨ Features**

Screen Dimmer is more than just a simple brightness tool. It's a robust application designed for a seamless user experience.

* **Advanced Brightness Control:** Dim your screen's brightness level from 0% to 90%, allowing you to go far darker than the default Windows settings.  
* **Multi-Monitor Support:** Applies settings consistently across all connected displays.  
* **Dynamic Monitor Handling:** Automatically detects when monitors are connected or disconnected and adjusts the overlays on the fly without needing a restart.  
* **Color Temperature Slider:** Intuitively adjust the screen tint from a neutral black to a warm, sepia-like orange to reduce blue light and ease eye strain at night.  
* **Blue Light Filter:** A dedicated slider to apply a blue tint, giving you granular control over the color profile of your screen.  
* **One-Click Presets:** Instantly apply predefined brightness levels for common scenarios like "Reading," "Movie," and "Night."  
* **System Tray Integration:** Runs quietly in your system tray to avoid cluttering your taskbar. Double-click to show controls, right-click for a quick menu.  
* **Global Hotkeys:** Control the application from anywhere in Windows without needing to open the control panel.  
  * Ctrl \+ ↑: Increase brightness  
  * Ctrl \+ ↓: Decrease brightness  
  * Ctrl \+ Shift \+ X: Exit the application  
* **Start with Windows:** A simple checkbox lets you set the application to launch automatically when you log in.  
* **Persistent Settings:** Remembers your last used brightness, color temperature, and blue light filter settings for the next time you start the app.  
* **Lightweight & Portable:** The entire application is a single, standalone .exe that requires no installation.

## **🚀 How to Use**

### **For Users**

1. Go to the [**Releases**](https://github.com/Dev-Leet/Screen_Dimmer/releases) page of this repository.  
2. Download the latest ScreenDimmer.exe file.  
3. Run the executable. The application will start in your system tray.

### **For Developers (Building from Source)**

This project was built using Visual Studio and the .NET Framework.

1. **Clone the Repository:**  
   git clone https://github.com/Dev-Leet/Screen\_Dimmer  
2. **Open in Visual Studio:** Open the Screen\_Dimmer.sln file in Visual Studio (2019 or later recommended).  
3. **Select .NET Framework:** The project is configured to use **.NET Framework 4.8**. If you don't have it, Visual Studio should prompt you to install it.  
4. **Build the Solution:**  
   * Go to the Build menu.  
   * Select Rebuild Solution.  
5. **Run:** The final executable will be located in the bin/Debug or bin/Release folder inside the project directory.

## **🛠️ Technology Stack**

* **Language:** C\#  
* **Framework:** .NET Framework 4.8  
* **UI:** Windows Forms (WinForms)  
* **System Interaction:** P/Invoke for global hotkeys and system event handling.

## **🤝 Contributing**

Contributions are welcome\! If you have ideas for new features, find a bug, or want to improve the code, feel free to:

1. Open an [**Issue**](https://github.com/Dev-Leet/Screen_Dimmer/issues) to discuss the change.  
2. Fork the repository and submit a **Pull Request**.

## **📄 License**

This project is licensed under the MIT License. See the [LICENSE.md](https://github.com/Dev-Leet/Screen_Dimmer/blob/master/LICENSE) file for details.
