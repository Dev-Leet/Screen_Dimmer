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
using System.Drawing.Drawing2D; // --- FIX #1: ADDED THIS LINE ---
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Screen_Dimmer.Properties; // --- FIX #2: ADDED THIS LINE -

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

        private Label presetsLabel;
        private Button presetReadingButton;
        private Button presetMovieButton;
        private Button presetNightButton;

        // --- NEW: Declare the components as class fields ---
        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu; // The menu for the tray icon
        private ToolTip toolTip;

        private Label colorTempLabel;
        private TrackBar colorTempTrackBar;
        private Panel colorGradientPanel;
        private Label coolLabel;
        private Label warmLabel;

        private static readonly Color CoolColor = Color.Black;
        private static readonly Color WarmColor = Color.FromArgb(255, 140, 70);

        // (P/Invoke declarations for hotkeys are unchanged)
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const string RunRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "ScreenDimmer";

        public ControlForm()
        {
            InitializeComponent();
            ReconfigureDimmerForms();
            RegisterHotkeys();
            LoadSettings();
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        private void InitializeComponent()
        {
            // --- Initialize ALL Components ---
            this.brightnessTrackBar = new TrackBar();
            this.startWithWindowsCheckBox = new CheckBox();
            this.infoLabel = new Label();
            this.percentageLabel = new Label();
            this.presetsLabel = new Label();
            this.presetReadingButton = new Button();
            this.presetMovieButton = new Button();
            this.presetNightButton = new Button();
            this.colorTempLabel = new Label();
            this.colorTempTrackBar = new TrackBar();
            this.colorGradientPanel = new Panel();
            this.coolLabel = new Label();
            this.warmLabel = new Label();

            // --- NEW: Manually initialize the NotifyIcon, ContextMenu, and ToolTip ---
            this.notifyIcon = new NotifyIcon();
            this.trayMenu = new ContextMenuStrip();
            this.toolTip = new ToolTip();

            // (Form Properties are unchanged)
            this.Text = "Screen Dimmer";
            this.ClientSize = new Size(320, 225);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Icon = SystemIcons.Application;

            // --- Controls Setup ---
            this.brightnessTrackBar.Location = new Point(15, 20);
            this.brightnessTrackBar.Size = new Size(290, 45);
            this.brightnessTrackBar.Minimum = 0;
            this.brightnessTrackBar.Maximum = 90;
            this.brightnessTrackBar.TickFrequency = 10;
            this.brightnessTrackBar.ValueChanged += (s, e) => UpdateAllOverlays();
            // --- NEW: Assign a tooltip ---
            this.toolTip.SetToolTip(this.brightnessTrackBar, "Adjust screen dimness level");

            this.percentageLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.percentageLabel.Location = new Point(140, 55);
            this.percentageLabel.Size = new Size(50, 20);
            this.percentageLabel.TextAlign = ContentAlignment.MiddleCenter;

            // (Preset button setup is unchanged)
            this.presetsLabel.Text = "Presets:";
            this.presetsLabel.Location = new Point(15, 90);
            this.presetsLabel.AutoSize = true;
            var presetButtons = new[] { presetReadingButton, presetMovieButton, presetNightButton };
            string[] presetNames = { "Reading", "Movie", "Night" };
            int[] presetValues = { 30, 65, 85 };
            for (int i = 0; i < presetButtons.Length; i++)
            {
                presetButtons[i].Text = presetNames[i];
                presetButtons[i].Tag = presetValues[i];
                presetButtons[i].Location = new Point(80 + (i * 75), 85);
                presetButtons[i].Size = new Size(70, 28);
                presetButtons[i].FlatStyle = FlatStyle.Flat;
                presetButtons[i].FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
                presetButtons[i].BackColor = Color.FromArgb(63, 63, 70);
                presetButtons[i].Click += PresetButton_Click;
                this.Controls.Add(presetButtons[i]);
            }

            this.startWithWindowsCheckBox.Text = "Start with Windows";
            this.startWithWindowsCheckBox.Location = new Point(15, 125);
            this.startWithWindowsCheckBox.AutoSize = true;
            this.startWithWindowsCheckBox.CheckedChanged += StartWithWindowsCheckBox_CheckedChanged;
            // --- NEW: Assign a tooltip ---
            this.toolTip.SetToolTip(this.startWithWindowsCheckBox, "Automatically run Screen Dimmer when you log in.");

            // (Color Temperature controls setup is unchanged)
            this.colorTempLabel.Text = "Color Temperature:";
            this.colorTempLabel.Location = new Point(15, 155);
            this.colorTempLabel.AutoSize = true;
            this.colorTempTrackBar.Location = new Point(15, 170);
            this.colorTempTrackBar.Size = new Size(290, 45);
            this.colorTempTrackBar.Minimum = 0;
            this.colorTempTrackBar.Maximum = 100;
            this.colorTempTrackBar.TickStyle = TickStyle.None;
            this.colorTempTrackBar.ValueChanged += (s, e) => UpdateAllOverlays();
            this.toolTip.SetToolTip(this.colorTempTrackBar, "Adjust the warmth of the screen tint");
            this.colorGradientPanel.Location = new Point(20, 177);
            this.colorGradientPanel.Size = new Size(280, 8);
            this.colorGradientPanel.Paint += ColorGradientPanel_Paint;
            this.coolLabel.Text = "Neutral";
            this.coolLabel.ForeColor = Color.Gray;
            this.coolLabel.Location = new Point(15, 195);
            this.coolLabel.AutoSize = true;
            this.warmLabel.Text = "Warm";
            this.warmLabel.ForeColor = Color.Gray;
            this.warmLabel.Location = new Point(265, 195);
            this.warmLabel.AutoSize = true;

            this.infoLabel.Text = "Ctrl+↑/↓ to adjust | Ctrl+Shift+X to exit";
            this.infoLabel.ForeColor = Color.Gray;
            this.infoLabel.Location = new Point(15, 200);
            this.infoLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // --- NEW: Configure the System Tray Icon and Menu in code ---
            // 1. Configure the menu items
            this.trayMenu.Items.Add("Show Controls", null, (s, a) => this.Show());
            this.trayMenu.Items.Add("Exit", null, (s, a) => Application.Exit());
            // 2. Configure the tray icon itself
            this.notifyIcon.Icon = this.Icon; // Use the form's icon
            this.notifyIcon.Text = "Screen Dimmer";
            this.notifyIcon.Visible = true;
            // 3. Add event handlers
            this.notifyIcon.DoubleClick += (s, e) => this.Show();
            // 4. Assign the menu to the icon
            this.notifyIcon.ContextMenuStrip = this.trayMenu;

            // Add all controls to the form
            this.Controls.Add(this.percentageLabel);
            this.Controls.Add(this.presetsLabel);
            this.Controls.Add(this.startWithWindowsCheckBox);
            this.Controls.Add(this.colorTempLabel);
            this.Controls.Add(this.coolLabel);
            this.Controls.Add(this.warmLabel);
            this.Controls.Add(this.colorGradientPanel);
            this.Controls.Add(this.colorTempTrackBar);
            this.Controls.Add(this.brightnessTrackBar);
            this.Controls.Add(this.infoLabel);

            this.Resize += (s, e) => { if (WindowState == FormWindowState.Minimized) this.Hide(); };
        }

        // (The rest of the ControlForm class - ReconfigureDimmerForms, LoadSettings, event handlers, etc. - is exactly the same as the previous version)
        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                ReconfigureDimmerForms();
            });
        }

        private void ReconfigureDimmerForms()
        {
            foreach (var form in dimmerForms)
            {
                form.Close();
            }
            dimmerForms.Clear();

            foreach (Screen screen in Screen.AllScreens)
            {
                var dimmer = new DimmerForm
                {
                    Bounds = screen.Bounds
                };
                dimmer.Show();
                dimmerForms.Add(dimmer);
            }
            UpdateAllOverlays();
        }

        // ... all other methods from the previous step remain here ...
        private void RegisterHotkeys()
        {
            RegisterHotKey(this.Handle, 1, MOD_CONTROL, (int)Keys.Down);
            RegisterHotKey(this.Handle, 2, MOD_CONTROL, (int)Keys.Up);
            RegisterHotKey(this.Handle, 3, MOD_CONTROL | MOD_SHIFT, (int)Keys.X);
        }
        private void LoadSettings()
        {
            brightnessTrackBar.Value = Settings.Default.LastBrightness;
            colorTempTrackBar.Value = Settings.Default.ColorTemperature;
            UpdateAllOverlays();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    startWithWindowsCheckBox.Checked = key?.GetValue(AppName) != null;
                }
            }
            catch { /* ignore */ }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;

            Settings.Default.LastBrightness = brightnessTrackBar.Value;
            Settings.Default.ColorTemperature = colorTempTrackBar.Value;
            Settings.Default.Save();

            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);
            UnregisterHotKey(this.Handle, 3);
            base.OnFormClosing(e);
        }

        private void UpdateAllOverlays()
        {
            double opacity = brightnessTrackBar.Value / 100.0;
            Color tintColor = InterpolateColor(CoolColor, WarmColor, colorTempTrackBar.Value / 100.0);

            foreach (var form in dimmerForms)
            {
                form.UpdateOverlay(tintColor, opacity);
            }
            percentageLabel.Text = $"{brightnessTrackBar.Value}%";
        }

        private Color InterpolateColor(Color color1, Color color2, double fraction)
        {
            fraction = Math.Max(0, Math.Min(1, fraction));
            int r = (int)Math.Round(color1.R + (color2.R - color1.R) * fraction);
            int g = (int)Math.Round(color1.G + (color2.G - color1.G) * fraction);
            int b = (int)Math.Round(color1.B + (color2.B - color1.B) * fraction);
            return Color.FromArgb(r, g, b);
        }

        private void PresetButton_Click(object sender, EventArgs e)
        {
            if (sender is Button clickedButton)
            {
                brightnessTrackBar.Value = (int)clickedButton.Tag;
            }
        }

        private void ColorGradientPanel_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(e.ClipRectangle, CoolColor, WarmColor, LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, e.ClipRectangle);
            }
        }

        private void StartWithWindowsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (startWithWindowsCheckBox.Checked)
                        key.SetValue(AppName, Application.ExecutablePath);
                    else
                        key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                switch (id)
                {
                    case 1: if (brightnessTrackBar.Value > 0) brightnessTrackBar.Value--; break;
                    case 2: if (brightnessTrackBar.Value < brightnessTrackBar.Maximum) brightnessTrackBar.Value++; break;
                    case 3: Application.Exit(); break;
                }
            }
            base.WndProc(ref m);
        }
    }


    /// <summary>
    /// The overlay form that covers a single screen to create the dimming effect.
    /// It is black, semi-transparent, borderless, and click-through.
    /// THIS CLASS CONTAINS THE FIX.
    /// </summary>
    public class DimmerForm : Form
    {
        // Constants used to set the extended window style for click-through functionality.
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80; // Hides the form from the Alt+Tab list.

        // P/Invoke is used here for completeness, but the CreateParams override is the primary method.
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public DimmerForm()
        {
            // Set the basic properties for the overlay form.
            this.BackColor = Color.Black;
            this.Opacity = 0; // Start fully transparent.
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false; // Hide from the taskbar.
            this.StartPosition = FormStartPosition.Manual; // We will set its position manually.
            this.TopMost = true; // Ensure it stays on top of other windows.
        }

        /// <summary>
        /// Overriding the CreateParams property is the most reliable way to apply
        /// special window styles *before* the window is created. This ensures
        /// the click-through behavior is set correctly from the very beginning.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Apply the necessary extended styles:
                // WS_EX_LAYERED:       Required for transparency (opacity).
                // WS_EX_TRANSPARENT:   Makes the window click-through.
                // WS_EX_TOOLWINDOW:    Hides the window from the Alt+Tab switcher.
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        /// <summary>
        /// A single, efficient public method allowing the ControlForm to update
        /// both the color and opacity of this overlay simultaneously.
        /// </summary>
        /// <param name="newColor">The new tint color for the overlay.</param>
        /// <param name="newOpacity">The new opacity level (0.0 to 1.0).</param>
        public void UpdateOverlay(Color newColor, double newOpacity)
        {
            // Basic validation to ensure values are within the correct range.
            if (newOpacity >= 0 && newOpacity <= 1)
            {
                this.BackColor = newColor;
                this.Opacity = newOpacity;
            }
        }
    }
}

