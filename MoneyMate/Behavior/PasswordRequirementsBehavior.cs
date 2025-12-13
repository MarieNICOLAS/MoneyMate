using System.Text.RegularExpressions;

namespace MoneyMate.Behaviors
{
    public class PasswordRequirementsBehavior : Behavior<Entry>
    {
        public Label? RequirementLabel { get; set; }

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

        private void OnPasswordChanged(object? sender, TextChangedEventArgs e)
        {
            if (RequirementLabel == null) return;

            string pwd = e.NewTextValue ?? "";

            bool hasLength = pwd.Length >= 8;
            bool hasUpper = Regex.IsMatch(pwd, @"[A-Z]");
            bool hasDigit = Regex.IsMatch(pwd, @"\d");
            bool hasSpecial = Regex.IsMatch(pwd, @"[\W_]");

            RequirementLabel.Text =
                $"{(hasLength ? "✔" : "✖")} Minimum 8 caractères\n" +
                $"{(hasUpper ? "✔" : "✖")} 1 majuscule\n" +
                $"{(hasDigit ? "✔" : "✖")} 1 chiffre\n" +
                $"{(hasSpecial ? "✔" : "✖")} 1 caractère spécial";

            RequirementLabel.TextColor = hasLength && hasUpper && hasDigit && hasSpecial
                ? Colors.Green
                : Colors.Red;
        }
    }
}
