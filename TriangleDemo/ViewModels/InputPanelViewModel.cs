using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using TriangleDemo.Models;

namespace TriangleDemo.ViewModels
{
    public class InputPanelViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        #region Declarations

        private string? _sideA;
        private string? _sideB;
        private string? _sideC;

        private double? _parsedA;
        private double? _parsedB;
        private double? _parsedC;

        private int _r = 59, _g = 130, _b = 246;

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

        public int R
        {
            get => _r;
            set => SetChannel(ref _r, value);
        }

        public int G
        {
            get => _g;
            set => SetChannel(ref _g, value);
        }

        public int B
        {
            get => _b;
            set => SetChannel(ref _b, value);
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

        public string SelectedColorHex
            => $"#{R:X2}{G:X2}{B:X2}";

        public TriangleData? ValidTriangle
        {
            get
            {
                if (_parsedA is null || _parsedB is null || _parsedC is null) 
                    return null;

                return TriangleFactory.TryCreate(_parsedA.Value, _parsedB.Value, _parsedC.Value, SelectedColorHex);
            }
        }

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

        #endregion

        #region Private Methods

        private void Validate()
        {
            ClearAllErrors();
            _parsedA = _parsedB = _parsedC = null;

            bool numericOk = ValidateNumericInputs(out double a, out double b, out double c);

            if (numericOk && ValidateTriangleInequality(a, b, c))
            {
                _parsedA = a;
                _parsedB = b;
                _parsedC = c;
            }

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

        private void SetChannel(ref int field, int value, [CallerMemberName] string? prop = null)
        {
            if (field == value)
                return;

            field = value;

            OnPropertyChanged(prop);
            OnPropertyChanged(nameof(SelectedColorHex));
            OnPropertyChanged(nameof(ValidTriangle));
        }

        #endregion //Private Methods
    }
}