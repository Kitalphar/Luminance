using System.Windows.Controls;
using Luminance.ViewModels;

namespace Luminance.Views
{
    /// <summary>
    /// Interaction logic for SetupView.xaml
    /// </summary>
    public partial class SetupView : UserControl
    {
        public SetupView(SetupViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}
