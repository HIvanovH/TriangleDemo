using TriangleDemo.Models;

namespace TriangleDemo.ViewModels
{
    public class RenderViewModel : ViewModelBase
    {
        private TriangleData? _currentTriangle;
        private TriangleData? _lastValidTriangle;

        public bool IsShowingLastFrame
            => _lastValidTriangle != null && _currentTriangle == null;

        public TriangleData? CurrentTriangle
        {
            get => _currentTriangle;
            set
            {
                if (value != null)
                    _lastValidTriangle = value;

                _currentTriangle = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsShowingLastFrame));
            }
        }
    }
}
