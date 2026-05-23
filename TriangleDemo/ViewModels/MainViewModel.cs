using TriangleDemo.Models;

namespace TriangleDemo.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly Action<TriangleData?> _onTriangleChanged;

        public MainViewModel()
        {
            Input = new InputPanelViewModel();
            Render = new RenderViewModel();

            _onTriangleChanged = triangle => Render.CurrentTriangle = triangle;
            Input.TriangleChanged += _onTriangleChanged;
        }

        public void Dispose() => Input.TriangleChanged -= _onTriangleChanged;

        public InputPanelViewModel Input { get; }
        public RenderViewModel Render { get; }
    }
}
