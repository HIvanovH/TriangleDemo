using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using TriangleDemo.Models;
using TriangleDemo.Rendering;

namespace TriangleDemo.Interop
{
    public class D3DHwndHost : HwndHost
    {
        private readonly D3D11Renderer _renderer = new();

        public static readonly DependencyProperty TriangleProperty =
            DependencyProperty.Register(
                nameof(Triangle),
                typeof(TriangleData),
                typeof(D3DHwndHost),
                new PropertyMetadata(null, OnTriangleChanged));

        private static void OnTriangleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((D3DHwndHost)d)._renderer.UpdateTriangle((TriangleData?)e.NewValue);

        public TriangleData? Triangle
        {
            get => (TriangleData?)GetValue(TriangleProperty);
            set => SetValue(TriangleProperty, value);
        }

        public D3DHwndHost()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            Window.GetWindow(this).Closing += OnWindowClosing;
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var src = PresentationSource.FromVisual(this);
            double dpiX = src?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
            double dpiY = src?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;
            _renderer.Resize((int)(e.NewSize.Width * dpiX), (int)(e.NewSize.Height * dpiY));
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Window.GetWindow(this).Closing -= OnWindowClosing;
            SizeChanged -= OnSizeChanged;
            Dispose();
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var hwnd = CreateWindowEx(
                0, "STATIC", "",
                WS_CHILD | WS_VISIBLE,
                0, 0, 0, 0,
                hwndParent.Handle,
                nint.Zero, nint.Zero, nint.Zero);

            _renderer.Initialize(hwnd, (int)ActualWidth, (int)ActualHeight);
            return new HandleRef(this, hwnd);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            _renderer.Dispose();
            DestroyWindow(hwnd.Handle);
        }

        [DllImport("user32.dll")]
        private static extern nint CreateWindowEx(
            int dwExStyle, string lpClassName, string lpWindowName,
            int dwStyle, int x, int y, int nWidth, int nHeight,
            nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(nint hwnd);

        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;
    }
}
