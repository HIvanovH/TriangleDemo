using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriangleDemo.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Input = new InputPanelViewModel();
            Render = new RenderViewModel();
        }

        public InputPanelViewModel Input { get; }
        public RenderViewModel Render { get; }
    }
}
