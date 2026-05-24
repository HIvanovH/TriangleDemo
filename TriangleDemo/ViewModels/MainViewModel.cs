using System.ComponentModel;
using TriangleDemo.Models;

namespace TriangleDemo.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly PropertyChangedEventHandler _onInputChanged;

        public MainViewModel()
        {
            Input = new InputPanelViewModel();
            Render = new RenderViewModel();

            _onInputChanged = (_, e) =>
            {
                if (e.PropertyName == nameof(InputPanelViewModel.ValidTriangle))
                    Render.CurrentTriangle = Input.ValidTriangle;
            };

            Input.PropertyChanged += _onInputChanged;
        }

        public void Dispose() => Input.PropertyChanged -= _onInputChanged;

        public InputPanelViewModel Input { get; }
        public RenderViewModel Render { get; }
    }
}
