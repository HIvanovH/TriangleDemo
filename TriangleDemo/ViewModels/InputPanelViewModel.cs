using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriangleDemo.ViewModels
{
    public class InputPanelViewModel : ViewModelBase
    {
        private string _sideA;
        private string _sideB;
        private string _sideC;

        private string _selectedColorHex = "#3B82F6";

        private string? _sideAError;
        private string? _sideBError;
        private string? _sideCError;
        private string? _validationSummary;

        private bool _hasValidationErrors;

        public string SideA
        {
            get => _sideA;
            set
            {
                if (_sideA == value)
                    return;

                _sideA = value;
                OnPropertyChanged(nameof(SideA));
            }
        }

        public string SideB
        {
            get => _sideB;
            set
            {
                if (_sideB == value)
                    return;

                _sideB = value;
                OnPropertyChanged(nameof(SideB));
            }
        }

        public string SideC
        {
            get => _sideC;
            set
            {
                if (_sideC == value)
                    return;

                _sideC = value;
                OnPropertyChanged(nameof(SideC));
            }
        }

        public string[] PresetColors { get; } =
        {
            "#3B82F6",
            "#10B981",
            "#F59E0B",
            "#EF4444",
            "#8B5CF6",
            "#EC4899"
        };

        public string SelectedColorHex
        {
            get => _selectedColorHex;
            set
            {
                if (_selectedColorHex == value)
                    return;

                _selectedColorHex = value;
                OnPropertyChanged(nameof(SelectedColorHex));
            }
        }

        public string? SideAError
        {
            get => _sideAError;
            private set
            {
                if (_sideAError == value)
                    return;

                _sideAError = value;
                OnPropertyChanged(nameof(SideAError));
            }
        }

        public string? SideBError
        {
            get => _sideBError;
            private set
            {
                if (_sideBError == value)
                    return;

                _sideBError = value;
                OnPropertyChanged(nameof(SideBError));
            }
        }

        public string? SideCError
        {
            get => _sideCError;
            private set
            {
                if (_sideCError == value)
                    return;

                _sideCError = value;
                OnPropertyChanged(nameof(SideCError));
            }
        }

        public string? ValidationSummary
        {
            get => _validationSummary;
            private set
            {
                if (_validationSummary == value)
                    return;

                _validationSummary = value;
                OnPropertyChanged(nameof(ValidationSummary));
            }
        }

        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            private set
            {
                if (_hasValidationErrors == value)
                    return;

                _hasValidationErrors = value;
                OnPropertyChanged(nameof(HasValidationErrors));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => !HasValidationErrors;

    }
}