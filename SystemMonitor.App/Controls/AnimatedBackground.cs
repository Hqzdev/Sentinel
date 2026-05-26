using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace SystemMonitor.App.Controls;

/// <summary>
/// Анимированный фон с плавающими частицами и линиями-связями.
/// Работает через DispatcherTimer + InvalidateVisual() ~30 fps.
/// </summary>
public sealed class AnimatedBackground : Control
{
    // ── Particle ───────────────────────────────────────────────────────────

    private sealed class Particle
    {
        public double X, Y;
        public double Vx, Vy;
        public double Radius;
        public double Opacity;
        public double OpacityDir = 1;
        public Color Color;
    }

    // ── Fields ─────────────────────────────────────────────────────────────

    private readonly List<Particle> _particles = new();
    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new();
    private bool _initialized;

    // Accent colours for particles (matches dashboard palette)
    private static readonly Color[] Palette =
    [
        Color.FromArgb(180, 59,  130, 246),   // blue
        Color.FromArgb(160, 168, 85,  247),   // purple
        Color.FromArgb(140, 16,  185, 129),   // green
        Color.FromArgb(120, 249, 115, 22),    // orange
    ];

    // ── Constructor ────────────────────────────────────────────────────────

    public AnimatedBackground()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) }; // ~30 fps
        _timer.Tick += (_, _) => Tick();
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleProperty)
        {
            if (IsVisible && VisualRoot is not null) _timer.Start();
            else _timer.Stop();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    // ── Tick ───────────────────────────────────────────────────────────────

    private void Tick()
    {
        if (!_initialized || _particles.Count == 0) return;

        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        foreach (var p in _particles)
        {
            p.X += p.Vx;
            p.Y += p.Vy;

            // Bounce off walls
            if (p.X < 0 || p.X > w) { p.Vx = -p.Vx; p.X = Math.Clamp(p.X, 0, w); }
            if (p.Y < 0 || p.Y > h) { p.Vy = -p.Vy; p.Y = Math.Clamp(p.Y, 0, h); }

            // Pulse opacity
            p.Opacity += p.OpacityDir * 0.008;
            if (p.Opacity > 1) { p.Opacity = 1; p.OpacityDir = -1; }
            if (p.Opacity < 0.15) { p.Opacity = 0.15; p.OpacityDir = 1; }
        }

        InvalidateVisual();
    }

    // ── Render ─────────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        // Init particles once we know the size
        if (!_initialized)
        {
            InitParticles(w, h);
            _initialized = true;
        }

        // ── Hex grid background ───────────────────────────────────────────
        DrawHexGrid(ctx, w, h);

        // ── Connection lines between nearby particles ─────────────────────
        const double maxDist = 110;
        for (int i = 0; i < _particles.Count; i++)
        {
            for (int j = i + 1; j < _particles.Count; j++)
            {
                var a = _particles[i];
                var b = _particles[j];
                var dx = a.X - b.X;
                var dy = a.Y - b.Y;
                var dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist < maxDist)
                {
                    var alpha = (byte)(40 * (1 - dist / maxDist) * Math.Min(a.Opacity, b.Opacity));
                    if (alpha < 4) continue;
                    var lineBrush = new SolidColorBrush(Color.FromArgb(alpha, 148, 163, 184));
                    ctx.DrawLine(new Pen(lineBrush, 0.6),
                        new Point(a.X, a.Y), new Point(b.X, b.Y));
                }
            }
        }

        // ── Particles ─────────────────────────────────────────────────────
        foreach (var p in _particles)
        {
            var alpha = (byte)(p.Opacity * p.Color.A);
            var c = Color.FromArgb(alpha, p.Color.R, p.Color.G, p.Color.B);
            ctx.DrawEllipse(new SolidColorBrush(c), null,
                new Point(p.X, p.Y), p.Radius, p.Radius);

            // Inner bright core
            var coreAlpha = (byte)(p.Opacity * 200);
            var core = Color.FromArgb(coreAlpha, 255, 255, 255);
            ctx.DrawEllipse(new SolidColorBrush(core), null,
                new Point(p.X, p.Y), p.Radius * 0.35, p.Radius * 0.35);
        }
    }

    // ── Hex grid ───────────────────────────────────────────────────────────

    private static void DrawHexGrid(DrawingContext ctx, double w, double h)
    {
        const double size = 38;
        const double col = size * 1.732;  // sqrt(3)
        const double row = size * 1.5;
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(10, 148, 163, 184)), 0.5);

        for (double y = -size; y < h + size; y += row)
        {
            for (double cx = -col; cx < w + col; cx += col)
            {
                var offset = (Math.Round(y / row) % 2 != 0) ? col / 2 : 0;
                DrawHexagon(ctx, pen, cx + offset, y, size);
            }
        }
    }

    private static void DrawHexagon(DrawingContext ctx, Pen pen, double cx, double cy, double r)
    {
        var geo = new StreamGeometry();
        using var gctx = geo.Open();
        for (int i = 0; i < 6; i++)
        {
            var ang = Math.PI / 180 * (60 * i - 30);
            var x = cx + r * Math.Cos(ang);
            var y = cy + r * Math.Sin(ang);
            if (i == 0) gctx.BeginFigure(new Point(x, y), false);
            else gctx.LineTo(new Point(x, y));
        }
        gctx.EndFigure(true);
        ctx.DrawGeometry(null, pen, geo);
    }

    // ── Init ───────────────────────────────────────────────────────────────

    private void InitParticles(double w, double h)
    {
        _particles.Clear();
        int count = (int)(w * h / 14000);
        count = Math.Clamp(count, 18, 55);

        for (int i = 0; i < count; i++)
        {
            var color = Palette[_rng.Next(Palette.Length)];
            _particles.Add(new Particle
            {
                X = _rng.NextDouble() * w,
                Y = _rng.NextDouble() * h,
                Vx = (_rng.NextDouble() - 0.5) * 0.55,
                Vy = (_rng.NextDouble() - 0.5) * 0.55,
                Radius = _rng.NextDouble() * 2.5 + 1.5,
                Opacity = _rng.NextDouble() * 0.6 + 0.2,
                OpacityDir = _rng.Next(2) == 0 ? 1 : -1,
                Color = color,
            });
        }
    }
}
