using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Luminance.Controls
{
    public partial class RecoveryKeyTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RecoveryKeyTextBox), new PropertyMetadata(""));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public RecoveryKeyTextBox()
        {
            InitializeComponent();
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                Clipboard.SetText(Text);

                // Change to checkmark
                CopyButton.Content = "✔"; // Segoe UI Symbol checkmark
                CopyButton.Foreground = Brushes.Green;

                // Wait 2 seconds then restore
                await Task.Delay(2000);
                CopyButton.Content = "📋"; // Clipboard symbol
            }
        }
    }
}
