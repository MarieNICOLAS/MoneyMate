using Microsoft.Maui.Controls;

namespace MoneyMate.Behaviors
{
    public class PasswordVisibilityBehavior : Behavior<Entry>
    {
        public ImageButton ToggleButton { get; set; }
        private bool isVisible = false;

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);

            if (ToggleButton == null)
                return;

            // Utilise le Dispatcher de l'Entry pour exécuter sur le thread principal
            bool v = entry.Dispatcher.Dispatch(() =>
            {
                ToggleButton.Clicked += (s, e) =>
                {
                    isVisible = !isVisible;

                    entry.IsPassword = !isVisible;
                    ToggleButton.Source = isVisible
                        ? "close_eye_icon.png"
                        : "open_eye_icon.png";
                };
            });
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            if (ToggleButton != null)
                ToggleButton.Clicked -= null;

            base.OnDetachingFrom(entry);
        }
    }
}
