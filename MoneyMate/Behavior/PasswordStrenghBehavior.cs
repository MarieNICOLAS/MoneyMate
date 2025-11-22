using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace MoneyMate.Behaviors
{
    public class PasswordStrengthIndicatorBehavior : Behavior<Entry>
    {
        public ProgressBar StrengthBar { get; set; }

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnPasswordChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnPasswordChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnPasswordChanged(object sender, TextChangedEventArgs e)
        {
            if (StrengthBar == null) return;

            string pwd = e.NewTextValue ?? "";

            int score = 0;
            if (pwd.Length >= 8) score++;
            if (Regex.IsMatch(pwd, @"[A-Z]")) score++;
            if (Regex.IsMatch(pwd, @"\d")) score++;
            if (Regex.IsMatch(pwd, @"[\W_]")) score++;

            double level = score / 4.0;
            StrengthBar.Progress = level;

            StrengthBar.ProgressColor = score switch
            {
                <= 1 => Colors.Red,
                2 => Colors.Orange,
                >= 3 => Colors.Green,
            };
        }
    }
}
