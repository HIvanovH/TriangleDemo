using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using TriangleDemo.Commands;
using TriangleDemo.Models;

namespace TriangleDemo.ViewModels
{
    public class InputPanelViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        #region Declarations

        private string? _sideA;
        private string? _sideB;
        private string? _sideC;

        private string _selectedColorHex = "#3B82F6";
        private ICommand? _selectColorCommand;

        private readonly Dictionary<string, List<string>> _errors = new();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        #endregion //Declarations

        #region Properties

        public string SideA
        {
            get => _sideA ?? string.Empty;
            set
            {
                if (_sideA == value) 
                    return;

                _sideA = value;

                OnPropertyChanged();
                Validate();
            }
        }

        public string SideB
        {
            get => _sideB ?? string.Empty;
            set
            {
                if (_sideB == value) 
                    return;
                _sideB = value;

                OnPropertyChanged();
                Validate();
            }
        }

        public string SideC
        {
            get => _sideC ?? string.Empty;
            set
            {
                if (_sideC == value) 
                    return;

                _sideC = value;

                OnPropertyChanged();
                Validate();
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
                OnPropertyChanged();
            }
        }

        public string? SideAError 
            => GetError(nameof(SideA));
        public string? SideBError 
            => GetError(nameof(SideB));
        public string? SideCError 
            => GetError(nameof(SideC));

        public bool HasErrors 
            => _errors.Count > 0;
        public bool IsValid 
            => !HasErrors;

        public TriangleData? ValidTriangle
        {
            get
            {
                if (!ValidateNumericInputs(out double a, out double b, out double c))
                    return null;

                return TriangleFactory.TryCreate(a, b, c, SelectedColorHex);
            }
        }

        #endregion //Properties

        #region Commands

        public ICommand SelectColorCommand =>
            _selectColorCommand ??= new RelayCommand(p => SelectedColorHex = (string)p!);

        #endregion //Properties

        #region INotifyDataErrorInfo

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.Values.SelectMany(e => e);

            return _errors.GetValueOrDefault(propertyName) ?? (IEnumerable)Array.Empty<string>();
        }

        private string? GetError(string propertyName) =>
            _errors.GetValueOrDefault(propertyName)?.FirstOrDefault();

        private void SetErrors(string propertyName, string? error)
        {
            if (error == null)
                _errors.Remove(propertyName);
            else
                _errors[propertyName] = [error];

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            NotifyErrorPropertiesChanged(propertyName);
        }

        private void ClearAllErrors()
        {
            foreach (var key in _errors.Keys.ToList())
            {
                _errors.Remove(key);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
            }

            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(SideAError));
            OnPropertyChanged(nameof(SideBError));
            OnPropertyChanged(nameof(SideCError));
        }

        private void NotifyErrorPropertiesChanged(string propertyName)
        {
            OnPropertyChanged(nameof(HasErrors));
            OnPropertyChanged(nameof(IsValid));

            if (propertyName == nameof(SideA))
                OnPropertyChanged(nameof(SideAError));
            else if (propertyName == nameof(SideB)) 
                OnPropertyChanged(nameof(SideBError));
            else if (propertyName == nameof(SideC)) 
                OnPropertyChanged(nameof(SideCError));
        }

        #endregion //Commands

        #region Private Methods

        private void Validate()
        {
            ClearAllErrors();
            ValidateNumericInputs(out double a, out double b, out double c);
            ValidateTriangleInequality(a, b, c);

            OnPropertyChanged(nameof(ValidTriangle));
        }

        private bool TryParseSideSize(string value, out double result) =>
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        private bool ValidateNumericInputs(out double a, out double b, out double c)
        {
            a = b = c = 0;

            bool isAOk = ValidateSide(SideA, nameof(SideA), "a", out a);
            bool isBOk = ValidateSide(SideB, nameof(SideB), "b", out b);
            bool isCOk = ValidateSide(SideC, nameof(SideC), "c", out c);

            return isAOk && isBOk && isCOk;
        }

        private bool ValidateSide(string raw, string propertyName, string label, out double result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            if (!TryParseSideSize(raw, out result))
            {
                SetErrors(propertyName, $"{label} must be a number.");
                return false;
            }
            if (result <= 0)
            {
                SetErrors(propertyName, $"{label} must be greater than zero.");
                return false;
            }

            return true;
        }

        private bool ValidateTriangleInequality(double a, double b, double c)
        {
            if (a >= b + c) 
                SetErrors(nameof(SideA), "a must be less than b + c.");

            if (b >= a + c) 
                SetErrors(nameof(SideB), "b must be less than a + c.");

            if (c >= a + b) 
                SetErrors(nameof(SideC), "c must be less than a + b.");

            return !HasErrors;
        }

        #endregion //Private Methods
    }
}