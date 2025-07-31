using System.Windows;
using System.Windows.Controls;

namespace Luminance.Controls
{
    public partial class PasswordInputTextBox : UserControl
    {
        public PasswordInputTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(PasswordInputTextBox),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordInputTextBox)d;
            control.UpdatePlaceholderVisibility();
        }

        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.Register(
                nameof(PlaceHolder),
                typeof(string),
                typeof(PasswordInputTextBox),
                new PropertyMetadata(string.Empty));

        public string PlaceHolder
        {
            get => (string)GetValue(PlaceHolderProperty);
            set => SetValue(PlaceHolderProperty, value);
        }

        private void UpdatePlaceholderVisibility()
        {
            passwordInputPlaceholder.Visibility = string.IsNullOrEmpty(Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
