namespace TriangleDemo.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Input = new InputPanelViewModel();
            Render = new RenderViewModel();

            Input.TriangleChanged += triangle => Render.CurrentTriangle = triangle;
        }

        public InputPanelViewModel Input { get; }
        public RenderViewModel Render { get; }
    }
}
