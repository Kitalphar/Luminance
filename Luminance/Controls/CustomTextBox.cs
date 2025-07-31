using System.Windows;
using System.Windows.Controls;

namespace Luminance.Controls
{
    public class CustomTextBox : TextBox   
    {
        static CustomTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomTextBox), new FrameworkPropertyMetadata(typeof(CustomTextBox)));
        }
    }
}
