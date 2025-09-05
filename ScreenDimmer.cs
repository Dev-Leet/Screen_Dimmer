/*
 * Screen Dimmer - A C# WinForms Application
 * * This application allows users to dim their screens beyond the system's minimum
 * brightness by overlaying a semi-transparent black form over each monitor.
 * * Features:
 * - Multi-monitor support.
 * - Click-through overlay.
 * - Brightness control via a slider.
 * - Global hotkeys (Ctrl + Arrow Up/Down) to adjust brightness.
 * - Global hotkey (Ctrl + Shift + X) to exit the application.
 * - Option to start automatically with Windows.
 * - Lightweight and contained within a single executable.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

// NOTE: Namespace updated to match your project name.
namespace Screen_Dimmer
{
    // The Program class and Main method have been removed from this file.
    // The project's own Program.cs file will be used as the entry point.

    /// <summary>
    /// The main control window for the application.
    /// This form contains the trackbar for brightness, the "Start with Windows" checkbox,
    /// and handles global hotkeys. It is not a dimmer overlay itself.
    /// </summary>
    public class ControlForm : Form
    {
        // A list to hold a reference to each dimmer form, one for each monitor.
        private readonly List<DimmerForm> dimmerForms = new List<DimmerForm>();
        private TrackBar brightnessTrackBar;
        private CheckBox startWithWindowsCheckBox;
        private Label infoLabel;
        private Label percentageLabel;
        private Button exitButton; // Added exit button

        // Constants for registering global hotkeys.
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004; // Added SHIFT modifier
        private const int WM_HOTKEY = 0x0312;

        // P/Invoke declarations for registering and unregistering global hotkeys.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Registry key for "Start with Windows" functionality.
        private const string RunRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ScreenDimmer";

        public ControlForm()
        {
            InitializeComponent();
            InitializeDimmerForms();
            RegisterHotkeys();
            LoadSettings();
        }

        // Sets up the UI components of the control form.
        private void InitializeComponent()
        {
            this.brightnessTrackBar = new TrackBar();
            this.startWithWindowsCheckBox = new CheckBox();
            this.infoLabel = new Label();
            this.percentageLabel = new Label();
            this.exitButton = new Button(); // Initialize exit button

            // Form properties
            this.Text = "Screen Dimmer";
            this.ClientSize = new Size(300, 150); // Increased height for the new button
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Application; // Simple default icon

            // Brightness TrackBar
            this.brightnessTrackBar.Location = new Point(12, 12);
            this.brightnessTrackBar.Size = new Size(276, 45);
            this.brightnessTrackBar.Minimum = 0;   // 0% dimming
            this.brightnessTrackBar.Maximum = 90;  // Capped at 90% to prevent a fully black screen
            this.brightnessTrackBar.TickFrequency = 10;
            this.brightnessTrackBar.ValueChanged += BrightnessTrackBar_ValueChanged;

            // Percentage Label
            this.percentageLabel.Location = new Point(135, 60);
            this.percentageLabel.Size = new Size(50, 20);
            this.percentageLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Info Label
            this.infoLabel.Text = "Ctrl+↑/↓ to adjust. Ctrl+Shift+X to exit.";
            this.infoLabel.Location = new Point(12, 60);
            this.infoLabel.AutoSize = true;

            // Start with Windows CheckBox
            this.startWithWindowsCheckBox.Text = "Start with Windows";
            this.startWithWindowsCheckBox.Location = new Point(15, 90);
            this.startWithWindowsCheckBox.AutoSize = true;
            this.startWithWindowsCheckBox.CheckedChanged += StartWithWindowsCheckBox_CheckedChanged;

            // Exit Button
            this.exitButton.Text = "Exit Application";
            this.exitButton.Location = new Point(180, 86);
            this.exitButton.Size = new Size(108, 28);
            this.exitButton.Click += ExitButton_Click;

            // Add controls to form
            this.Controls.Add(this.brightnessTrackBar);
            this.Controls.Add(this.percentageLabel);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.startWithWindowsCheckBox);
            this.Controls.Add(this.exitButton); // Add new button to controls

            // Initial update
            UpdatePercentageLabel();
        }

        // Creates and displays a DimmerForm for each connected monitor.
        private void InitializeDimmerForms()
        {
            // Loop through all screens connected to the system.
            foreach (Screen screen in Screen.AllScreens)
            {
                DimmerForm dimmer = new DimmerForm();
                dimmer.Bounds = screen.Bounds; // Set the form's size and position to match the screen.
                dimmer.Show();
                dimmerForms.Add(dimmer);
            }
        }

        // Registers the global hotkeys for increasing/decreasing brightness and exiting.
        private void RegisterHotkeys()
        {
            // ID 1 for decrease, ID 2 for increase, ID 3 for exit.
            RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Down);
            RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Up);
            RegisterHotKey(this.Handle, 3, MOD_CONTROL | MOD_SHIFT, (int)Keys.X); // Added Exit Hotkey
        }

        // Checks registry to see if the app is configured to start with Windows.
        private void LoadSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(AppName);
                        // If the key exists and points to our executable, check the box.
                        startWithWindowsCheckBox.Checked = value != null && value.ToString() == Application.ExecutablePath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler for the trackbar value changing.
        private void BrightnessTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // The opacity is a value between 0.0 and 1.0.
            double opacity = brightnessTrackBar.Value / 100.0;
            // Update the opacity on all dimmer forms.
            foreach (var form in dimmerForms)
            {
                form.SetOpacity(opacity);
            }
            UpdatePercentageLabel();
        }

        private void UpdatePercentageLabel()
        {
            percentageLabel.Text = $"{brightnessTrackBar.Value}%";
        }

        // Event handler for the Exit button click.
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // Event handler for the "Start with Windows" checkbox.
        private void StartWithWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (startWithWindowsCheckBox.Checked)
                    {
                        // Add the application path to the registry key.
                        key.SetValue(AppName, Application.ExecutablePath);
                    }
                    else
                    {
                        // Remove the value from the registry key.
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Overrides the window procedure to listen for messages, specifically WM_HOTKEY.
        protected override void WndProc(ref Message m)
        {
            // Check if the message is a hotkey press.
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32(); // The ID of the hotkey that was pressed.
                switch (id)
                {
                    case 1: // Decrease brightness (Ctrl + Down)
                        if (brightnessTrackBar.Value > brightnessTrackBar.Minimum)
                            brightnessTrackBar.Value--;
                        break;
                    case 2: // Increase brightness (Ctrl + Up)
                        if (brightnessTrackBar.Value < brightnessTrackBar.Maximum)
                            brightnessTrackBar.Value++;
                        break;
                    case 3: // Exit application (Ctrl + Shift + X)
                        Application.Exit();
                        break;
                }
            }
            base.WndProc(ref m);
        }

        // Ensures hotkeys are unregistered when the form closes to avoid system-wide conflicts.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);
            UnregisterHotKey(this.Handle, 3); // Unregister exit hotkey
            base.OnFormClosing(e);
        }
    }


    /// <summary>
    /// The overlay form that covers a single screen to create the dimming effect.
    /// It is black, semi-transparent, borderless, and click-through.
    /// THIS CLASS CONTAINS THE FIX.
    /// </summary>
    public class DimmerForm : Form
    {
        // Constants for setting window styles to enable click-through.
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80; // Bonus: Hides the overlay from Alt+Tab

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public DimmerForm()
        {
            this.BackColor = Color.Black;
            this.Opacity = 0; // Start fully transparent.
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false; // Hide from the taskbar.
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true; // Stay on top of all other windows.
        }

        /// <summary>
        /// This is the key to the fix. By overriding the CreateParams property,
        /// we modify the window's creation parameters *before* it is created.
        /// This is a much more reliable way to set the extended window styles.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Set the extended style to include WS_EX_LAYERED and WS_EX_TRANSPARENT
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
                // Add WS_EX_TOOLWINDOW to prevent the form from showing in the Alt+Tab dialog
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        // A public method to allow the ControlForm to set the opacity of this dimmer.
        public void SetOpacity(double opacity)
        {
            // Ensure opacity is within the valid range [0.0, 1.0].
            if (opacity >= 0 && opacity <= 1)
            {
                this.Opacity = opacity;
            }
        }

        // NOTE: The previous WndProc and OnHandleCreated methods are no longer needed
        // because the CreateParams override is a superior way to handle this.
    }
}

