using Microsoft.Maui.Graphics;
using MoneyMate.ViewModels;

namespace MoneyMate.Graphics
{
    public class LineChartDrawable : IDrawable
    {
        public List<ChartPoint> Points { get; set; } = new();
        public List<ChartPoint> BudgetPoints { get; set; } = new();

        private readonly Color _expenseColor = Color.FromArgb("#5B8DB8");
        private readonly Color _budgetColor = Color.FromArgb("#AAAAAA");
        private readonly Color _gridColor = Color.FromArgb("#EEEEEE");
        private readonly Color _labelColor = Color.FromArgb("#AAAAAA");

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Points == null || Points.Count == 0) return;

            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float paddingLeft = 44f;
            float paddingRight = 10f;
            float paddingTop = 16f;
            float paddingBottom = 28f;

            float chartWidth = width - paddingLeft - paddingRight;
            float chartHeight = height - paddingTop - paddingBottom;
            int count = Points.Count;

            // Max sur les deux séries
            double maxExp = Points.Max(p => p.Value);
            double maxBud = BudgetPoints != null && BudgetPoints.Count > 0
                ? BudgetPoints.Max(p => p.Value) : 0;
            double maxVal = Math.Max(maxExp, maxBud);
            if (maxVal <= 0) maxVal = 1;

            // Grille
            canvas.StrokeColor = _gridColor;
            canvas.StrokeSize = 1;
            int gridLines = 6;
            for (int i = 0; i <= gridLines; i++)
            {
                float y = paddingTop + chartHeight - (chartHeight * i / gridLines);
                canvas.DrawLine(paddingLeft, y, paddingLeft + chartWidth, y);

                double val = maxVal * i / gridLines;
                string label = val >= 1000 ? $"{val / 1000:0.#}k" : $"{val:0}";
                canvas.FontColor = _labelColor;
                canvas.FontSize = 9;
                canvas.DrawString(label, 0, y - 6, paddingLeft - 4, 14,
                    HorizontalAlignment.Right, VerticalAlignment.Center);
            }

            float GetX(int index) =>
                paddingLeft + (count == 1 ? chartWidth / 2 : index * chartWidth / (count - 1));
            float GetY(double value) =>
                paddingTop + chartHeight - (float)(value / maxVal * chartHeight);

            // ── Courbe Budget (grise) en premier ──
            if (BudgetPoints != null && BudgetPoints.Count == count)
            {
                canvas.StrokeColor = _budgetColor;
                canvas.StrokeSize = 2;
                canvas.StrokeDashPattern = new float[] { 4, 3 };
                var path = new PathF();
                path.MoveTo(GetX(0), GetY(BudgetPoints[0].Value));
                for (int i = 1; i < count; i++)
                {
                    float x0 = GetX(i - 1), y0 = GetY(BudgetPoints[i - 1].Value);
                    float x1 = GetX(i), y1 = GetY(BudgetPoints[i].Value);
                    float cpx = (x0 + x1) / 2;
                    path.CurveTo(cpx, y0, cpx, y1, x1, y1);
                }
                canvas.DrawPath(path);
                canvas.StrokeDashPattern = null;
            }

            // ── Courbe Dépenses (bleue) ──
            canvas.StrokeColor = _expenseColor;
            canvas.StrokeSize = 2;
            canvas.StrokeDashPattern = null;
            {
                var path = new PathF();
                path.MoveTo(GetX(0), GetY(Points[0].Value));
                for (int i = 1; i < count; i++)
                {
                    float x0 = GetX(i - 1), y0 = GetY(Points[i - 1].Value);
                    float x1 = GetX(i), y1 = GetY(Points[i].Value);
                    float cpx = (x0 + x1) / 2;
                    path.CurveTo(cpx, y0, cpx, y1, x1, y1);
                }
                canvas.DrawPath(path);
            }

            // Points Dépenses
            canvas.FillColor = _expenseColor;
            canvas.StrokeDashPattern = null;
            for (int i = 0; i < count; i++)
                canvas.FillCircle(GetX(i), GetY(Points[i].Value), 3);

            // Labels X
            canvas.FontColor = _labelColor;
            canvas.FontSize = 9;
            for (int i = 0; i < count; i++)
                canvas.DrawString(Points[i].Label, GetX(i) - 14,
                    paddingTop + chartHeight + 6, 28, 16,
                    HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }
}