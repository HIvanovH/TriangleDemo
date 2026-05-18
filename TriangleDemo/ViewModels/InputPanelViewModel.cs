using System.Globalization;

namespace TriangleDemo.ViewModels
{
    public class InputPanelViewModel : ViewModelBase
    {
        #region Declarations

        private string? _sideA;
        private string? _sideB;
        private string? _sideC;

        private string _selectedColorHex = "#3B82F6";

        private string? _sideAError;
        private string? _sideBError;
        private string? _sideCError;
        private string? _validationSummary;

        private bool _hasValidationErrors;

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
                OnPropertyChanged(nameof(SideA));
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
                OnPropertyChanged(nameof(SideB));
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
                OnPropertyChanged(nameof(SideC));
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

        #endregion //Properties

        #region Private Methods

        private void Validate()
        {
            ResetValidation();

            if (!ValidateNumericInputs(out double a, out double b, out double c))
            {
                HasValidationErrors = HasAnySideError();
                ValidationSummary = HasValidationErrors ? "Fix validation errors above." : null;
                return;
            }

            if (!ValidateTriangleInequality(a, b, c))
            {
                HasValidationErrors = HasAnySideError();
                ValidationSummary = "Fix validation errors above.";
                return;
            }

            HasValidationErrors = false;
            ValidationSummary = null;
        }

        private bool TryParseSideSize(string value, out double result)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private void ResetValidation()
        {
            SideAError = null;
            SideBError = null;
            SideCError = null;
            ValidationSummary = null;
        }

        private bool ValidateNumericInputs(out double a, out double b, out double c)
        {
            a = b = c = 0;

            bool isAOk = ValidateSide(SideA, "a", out a, out string? aError);
            bool isBOk = ValidateSide(SideB, "b", out b, out string? bError);
            bool isCOk = ValidateSide(SideC, "c", out c, out string? cError);

            SideAError = aError;
            SideBError = bError;
            SideCError = cError;

            return isAOk && isBOk && isCOk;
        }

        private bool ValidateTriangleInequality(double a, double b, double c)
        {
            if (a >= b + c)
                SideAError = "a must be less than b + c.";

            if (b >= a + c)
                SideBError = "b must be less than a + c.";

            if (c >= a + b)
                SideCError = "c must be less than a + b.";

            return !HasAnySideError();
        }

        private bool HasAnySideError()
        {
            return SideAError != null 
                || SideBError != null 
                || SideCError != null;
        }

        private bool ValidateSide(string raw, string label, out double result, out string? error)
        {
            result = 0;
            error = null;
            if (string.IsNullOrWhiteSpace(raw))
                return false;
            if (!TryParseSideSize(raw, out result))
            {
                error = $"{label} must be a number.";
                return false;
            }
            if (result <= 0)
            {
                error = $"{label} must be greater than zero.";
                return false;
            }
            return true;
        }

        #endregion //Private Methods
    }
}