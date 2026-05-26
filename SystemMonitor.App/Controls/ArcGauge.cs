using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SystemMonitor.App.Controls;

/// <summary>
/// Кастомный дуговой гейдж. Рисует 270° трек с прогрессом.
/// Угол начала: 135° (нижний-левый), разворот по часовой.
/// </summary>
public sealed class ArcGauge : Control
{
    // ── Styled Properties ──────────────────────────────────────────────────

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ArcGauge, double>(nameof(Value), 0d,
            coerce: (_, v) => Math.Clamp(v, 0, 100));

    public static readonly StyledProperty<IBrush> ArcBrushProperty =
        AvaloniaProperty.Register<ArcGauge, IBrush>(nameof(ArcBrush),
            new SolidColorBrush(Color.FromRgb(99, 102, 241)));

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<ArcGauge, string>(nameof(Label), "–");

    public static readonly StyledProperty<string> UnitProperty =
        AvaloniaProperty.Register<ArcGauge, string>(nameof(Unit), "%");

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<ArcGauge, string>(nameof(Subtitle), string.Empty);

    // ── CLR wrappers ───────────────────────────────────────────────────────

    public double Value   { get => GetValue(ValueProperty);   set => SetValue(ValueProperty, value); }
    public IBrush ArcBrush{ get => GetValue(ArcBrushProperty);set => SetValue(ArcBrushProperty, value); }
    public string Label   { get => GetValue(LabelProperty);   set => SetValue(LabelProperty, value); }
    public string Unit    { get => GetValue(UnitProperty);    set => SetValue(UnitProperty, value); }
    public string Subtitle{ get => GetValue(SubtitleProperty);set => SetValue(SubtitleProperty, value); }

    // ── Static ctor: invalidate render on property changes ─────────────────

    static ArcGauge()
    {
        AffectsRender<ArcGauge>(ValueProperty, ArcBrushProperty, LabelProperty, UnitProperty, SubtitleProperty);
    }

    // ── Constants ──────────────────────────────────────────────────────────

    private const double StartAngleDeg = 135;
    private const double SweepDeg = 270;
    private const double StrokeWidth = 11;

    // ── Render ─────────────────────────────────────────────────────────────

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        var center = new Point(w / 2, h / 2);
        var radius = Math.Min(w, h) / 2 - StrokeWidth / 2 - 2;

        // ── Background track ──────────────────────────────────────────────
        var trackPen = new Pen(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), StrokeWidth,
            lineCap: PenLineCap.Round);
        DrawArc(context, trackPen, center, radius, StartAngleDeg, SweepDeg);

        // ── Progress arc ──────────────────────────────────────────────────
        if (Value > 0)
        {
            var progressSweep = SweepDeg * (Value / 100.0);
            var progressPen = new Pen(ArcBrush, StrokeWidth, lineCap: PenLineCap.Round);
            DrawArc(context, progressPen, center, radius, StartAngleDeg, progressSweep);
        }

        // ── Inner glow circle ─────────────────────────────────────────────
        var glowRadius = radius - StrokeWidth / 2 - 6;
        if (glowRadius > 0 && ArcBrush is SolidColorBrush solidBrush)
        {
            var glowColor = Color.FromArgb(20, solidBrush.Color.R, solidBrush.Color.G, solidBrush.Color.B);
            context.DrawEllipse(new SolidColorBrush(glowColor), null,
                center, glowRadius, glowRadius);
        }

        // ── Value text (center) ───────────────────────────────────────────
        var valueStr = $"{Value:F0}{Unit}";
        var valueFt = new FormattedText(
            valueStr,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Inter, SF Pro Display, -apple-system, sans-serif", weight: FontWeight.Bold),
            emSize: radius * 0.38,
            foreground: new SolidColorBrush(Color.FromRgb(226, 232, 240)));

        context.DrawText(valueFt,
            new Point(center.X - valueFt.Width / 2, center.Y - valueFt.Height / 2 - 6));

        // ── Label below value ─────────────────────────────────────────────
        if (!string.IsNullOrEmpty(Label))
        {
            var labelFt = new FormattedText(
                Label,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, SF Pro Text, -apple-system, sans-serif"),
                emSize: radius * 0.16,
                foreground: new SolidColorBrush(Color.FromRgb(100, 116, 139)));

            context.DrawText(labelFt,
                new Point(center.X - labelFt.Width / 2, center.Y + valueFt.Height / 2 - 4));
        }

        // ── Subtitle below gauge ──────────────────────────────────────────
        if (!string.IsNullOrEmpty(Subtitle))
        {
            var subFt = new FormattedText(
                Subtitle,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, SF Pro Text, -apple-system, sans-serif"),
                emSize: 11,
                foreground: new SolidColorBrush(Color.FromRgb(100, 116, 139)));

            context.DrawText(subFt,
                new Point(center.X - subFt.Width / 2, h - subFt.Height - 2));
        }
    }

    // ── Arc helper ─────────────────────────────────────────────────────────

    private static void DrawArc(
        DrawingContext ctx,
        Pen pen,
        Point center,
        double radius,
        double startDeg,
        double sweepDeg)
    {
        // Clamp to avoid degenerate full-circle arcs
        sweepDeg = Math.Min(sweepDeg, 359.99);

        var startRad = ToRad(startDeg - 90);
        var endRad   = ToRad(startDeg + sweepDeg - 90);

        var startPt = PolarToPoint(center, radius, startRad);
        var endPt   = PolarToPoint(center, radius, endRad);

        var geo = new StreamGeometry();
        using (var gctx = geo.Open())
        {
            gctx.BeginFigure(startPt, false);
            gctx.ArcTo(endPt,
                new Size(radius, radius),
                rotationAngle: 0,
                isLargeArc: sweepDeg > 180,
                sweepDirection: SweepDirection.Clockwise);
        }

        ctx.DrawGeometry(null, pen, geo);
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;

    private static Point PolarToPoint(Point center, double r, double rad)
        => new(center.X + r * Math.Cos(rad), center.Y + r * Math.Sin(rad));
}
