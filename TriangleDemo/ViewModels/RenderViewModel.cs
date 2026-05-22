using TriangleDemo.Models;

namespace TriangleDemo.ViewModels
{
    public class RenderViewModel : ViewModelBase
    {
        private TriangleData? _currentTriangle;
        public TriangleData? CurrentTriangle
        {
            get => _currentTriangle;
            set 
            { 
                _currentTriangle = value;
                OnPropertyChanged();
            }
        }
    }
}
