using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace MoneyMate.Behaviors
{
    public class EmailValidationBehavior : Behavior<Entry>
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEmailChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEmailChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnEmailChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (Entry)sender;
            string email = e.NewTextValue ?? "";

            bool isValid = EmailRegex.IsMatch(email);
        }
    }
}
