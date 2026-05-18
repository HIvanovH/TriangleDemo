using System.Windows;
using System.Windows.Controls;

namespace TriangleDemo.Views.Controls
{
    /// <summary>
    /// Interaction logic for SideInputControl.xaml
    /// </summary>
    public partial class SideInputControl : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(SideInputControl));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(SideInputControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ErrorProperty =
            DependencyProperty.Register(nameof(Error), typeof(string), typeof(SideInputControl));

        public string Label 
        { 
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value); 
        }

        public string Value 
        { 
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value); 
        }

        public string? Error 
        { 
            get => (string?)GetValue(ErrorProperty); 
            set => SetValue(ErrorProperty, value); 
        }

        public SideInputControl() => InitializeComponent();
    }
}
