using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyGameBot;

public partial class Form1 : Form
{
    // ---- Win32 hotkeys ----
    [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint mods, uint vk);
    [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    const uint MOD_ALT = 0x0001;
    const int WM_HOTKEY = 0x0312;

    const int HK_RUNNER = 1;
    const int HK_AIM    = 2;
    const int HK_FIRE   = 3;
    const int HK_RUSH   = 4;

    // ---- Systems ----
    private readonly System.Windows.Forms.Timer _loop = new() { Interval = 16 };
    private GameAdapter   _adapter = null!;
    private AimTelemetry  _aim     = null!;
    private ScenarioRunner _runner = null!;
    private float _lastTick;

    public Form1()
    {
        InitializeComponent();
        DoubleBuffered = true;
        KeyPreview = true;

        _adapter = new GameAdapter();
        _aim     = new AimTelemetry(_adapter, _adapter);
        _runner  = new ScenarioRunner(_adapter, _adapter, _aim);

        _lastTick = Environment.TickCount / 1000f;
        _loop.Tick += Loop_Tick;
        _loop.Start();
    }

    private void Loop_Tick(object? sender, EventArgs e)
    {
        float now = Environment.TickCount / 1000f;
        float dt  = now - _lastTick;
        _lastTick = now;

        _adapter.Update(dt);
        _aim.Tick(dt);
        _runner.Tick(dt);

        Invalidate();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        RegisterHotKey(Handle, HK_RUNNER, MOD_ALT, (uint)Keys.D0);
        RegisterHotKey(Handle, HK_AIM,    MOD_ALT, (uint)Keys.D1);
        RegisterHotKey(Handle, HK_FIRE,   MOD_ALT, (uint)Keys.D2);
        RegisterHotKey(Handle, HK_RUSH,   MOD_ALT, (uint)Keys.D9);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        UnregisterHotKey(Handle, HK_RUNNER);
        UnregisterHotKey(Handle, HK_AIM);
        UnregisterHotKey(Handle, HK_FIRE);
        UnregisterHotKey(Handle, HK_RUSH);
        base.OnHandleDestroyed(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            switch (m.WParam.ToInt32())
            {
                case HK_RUNNER: _runner.Toggle(); break;
                case HK_AIM:    _aim.Toggle(); break;
                case HK_FIRE:   _aim.ToggleFire(); break;
                case HK_RUSH:   _runner.RushEnabled = !_runner.RushEnabled; break;
            }
        }
        base.WndProc(ref m);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float cx = ClientSize.Width  * 0.5f;
        float cy = ClientSize.Height * 0.5f;

        if (_aim.Enabled)
        {
            float pulse01 = (float)(Math.Sin(_aim.PulsePhase) * 0.5 + 0.5);
            float radius  = _aim.LockRadiusPx + pulse01 * _aim.PulseAmplitudePx;
            bool  locked  = _aim.LockedTarget != null;
            Color ring    = locked ? Color.Red : Color.FromArgb(120, 255, 255, 255);

            if (locked)
            {
                for (int i = 4; i > 0; i--)
                {
                    float r = radius + i * _aim.GlowWidthPx;
                    int a = (int)(_aim.GlowAlphaMax * 60 * (1f - i / 5f) * (0.7f + 0.3f * pulse01));
                    using var glowPen = new Pen(Color.FromArgb(a, 255, 60, 60), 2f);
                    g.DrawEllipse(glowPen, cx - r, cy - r, r * 2, r * 2);
                }
            }

            using var pen = new Pen(ring, 1.5f);
            g.DrawEllipse(pen, cx - radius, cy - radius, radius * 2, radius * 2);

            if (locked)
            {
                var (bx, by) = _adapter.ProjectToScreen(_aim.LockedTarget!.WorldPosition);
                using var linePen = new Pen(Color.Red, 2f);
                g.DrawLine(linePen, bx, by, cx, cy);
            }
        }

        using var hudFont  = new Font("Consolas", 10f);
        using var hudBrush = new SolidBrush(Color.White);
        g.DrawString($"Runner: {(_runner.Enabled ? "ON" : "OFF")} [Alt+0]", hudFont, hudBrush, 10, 10);
        g.DrawString($"Aim:    {(_aim.Enabled ? "ON" : "OFF")} [Alt+1]",     hudFont, hudBrush, 10, 28);
        g.DrawString($"Fire:   {(_aim.FireEnabled ? "ON" : "OFF")} [Alt+2]", hudFont, hudBrush, 10, 46);
        g.DrawString($"Rush:   {(_runner.RushEnabled ? "ON" : "OFF")} [Alt+9]", hudFont, hudBrush, 10, 64);
    }
}
