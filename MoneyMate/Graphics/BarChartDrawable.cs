using Microsoft.Maui.Graphics;
using MoneyMate.ViewModels;

namespace MoneyMate.Graphics
{
    public class BarChartDrawable : IDrawable
    {
        public List<BarMonth> Months { get; set; } = new();
        public double MaxValue { get; set; } = 1;

        private readonly Color _budgetColor = Color.FromArgb("#7B9EC5");
        private readonly Color _spentColor = Color.FromArgb("#B0C4D8");
        private readonly Color _overBudgetColor = Color.FromArgb("#E57373");
        private readonly Color _gridColor = Color.FromArgb("#EEEEEE");
        private readonly Color _labelColor = Color.FromArgb("#AAAAAA");

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Months == null || Months.Count == 0) return;

            float width = dirtyRect.Width;
            float height = dirtyRect.Height;
            float paddingLeft = 44f;
            float paddingRight = 8f;
            float paddingTop = 16f;
            float paddingBottom = 28f;

            float chartWidth = width - paddingLeft - paddingRight;
            float chartHeight = height - paddingTop - paddingBottom;

            double maxVal = MaxValue > 0 ? MaxValue : 1;

            // ── Grille horizontale ───────────────────────────────
            canvas.StrokeColor = _gridColor;
            canvas.StrokeSize = 1;

            int gridLines = 4;
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

            // ── Barres ───────────────────────────────────────────
            int count = Months.Count;
            float slotWidth = chartWidth / count;
            float barWidth = Math.Max(4, slotWidth * 0.35f);
            float gap = 2f;

            for (int i = 0; i < count; i++)
            {
                var m = Months[i];
                float slotCenterX = paddingLeft + i * slotWidth + slotWidth / 2;

                // Barre Budget
                float budgetH = (float)(m.BudgetAmount / maxVal * chartHeight);
                float budgetX = slotCenterX - barWidth - gap / 2;
                float budgetY = paddingTop + chartHeight - budgetH;

                canvas.FillColor = _budgetColor;
                DrawRoundedBar(canvas, budgetX, budgetY, barWidth, budgetH, 3);

                // Barre Dépenses
                float spentH = (float)(m.SpentAmount / maxVal * chartHeight);
                float spentX = slotCenterX + gap / 2;
                float spentY = paddingTop + chartHeight - spentH;

                canvas.FillColor = m.IsOverBudget ? _overBudgetColor : _spentColor;
                DrawRoundedBar(canvas, spentX, spentY, barWidth, spentH, 3);

                // Label mois
                canvas.FontColor = _labelColor;
                canvas.FontSize = 9;
                float labelY = paddingTop + chartHeight + 6;
                canvas.DrawString(m.Month, slotCenterX - 14, labelY, 28, 16,
                    HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }

        private void DrawRoundedBar(ICanvas canvas, float x, float y, float w, float h, float radius)
        {
            if (h <= 0) return;
            if (h < radius * 2) radius = h / 2;

            var path = new PathF();
            // Coins arrondis en haut seulement
            path.MoveTo(x + radius, y);
            path.LineTo(x + w - radius, y);
            path.CurveTo(x + w, y, x + w, y, x + w, y + radius);
            path.LineTo(x + w, y + h);
            path.LineTo(x, y + h);
            path.LineTo(x, y + radius);
            path.CurveTo(x, y, x, y, x + radius, y);
            path.Close();

            canvas.FillPath(path);
        }
    }
}
