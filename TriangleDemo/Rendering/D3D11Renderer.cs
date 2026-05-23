using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Windows.Media;
using Silk.NET.Direct3D.Compilers;
using System.Text;
using TriangleDemo.Models;

namespace TriangleDemo.Rendering
{
    public class D3D11Renderer : IDisposable
    {
        private D3D11 _d3d11 = null!;
        private DXGI _dxgi = null!;
        private int _width, _height;

        private ComPtr<ID3D11Device> _device;
        private ComPtr<ID3D11DeviceContext> _context;
        private ComPtr<IDXGIFactory2> _factory;
        private ComPtr<IDXGISwapChain1> _swapchain;

        private ComPtr<ID3D11Buffer> _vertexBuffer;
        private ComPtr<ID3D11Buffer> _colorBuffer;
        private ComPtr<ID3D11VertexShader> _vs;
        private ComPtr<ID3D11PixelShader> _ps;
        private ComPtr<ID3D11PixelShader> _psGrid;
        private ComPtr<ID3D11InputLayout> _inputLayout;
        private ComPtr<ID3D11RasterizerState> _rastState;
        private ComPtr<ID3D11Buffer> _gridBuffer;

        private bool _initialized;
        private bool _needsResize;

        private TriangleData? _currentTriangle;
        private TriangleData? _lastValidTriangle;

        private static readonly float[] ClearColor = { 0.067f, 0.094f, 0.153f, 1.0f };
        private static readonly float[] GridColor = { 0.1f, 0.3f, 0.2f, 1.0f };

        private static readonly float[] GridLines =
        {
            -1f, -0.75f, 0f,   1f, -0.75f, 0f,
            -1f, -0.5f,  0f,   1f, -0.5f,  0f,
            -1f, -0.25f, 0f,   1f, -0.25f, 0f,
            -1f,  0f,    0f,   1f,  0f,    0f,
            -1f,  0.25f, 0f,   1f,  0.25f, 0f,
            -1f,  0.5f,  0f,   1f,  0.5f,  0f,
            -1f,  0.75f, 0f,   1f,  0.75f, 0f,

            -0.75f, -1f, 0f,  -0.75f, 1f, 0f,
            -0.5f,  -1f, 0f,  -0.5f,  1f, 0f,
            -0.25f, -1f, 0f,  -0.25f, 1f, 0f,
             0f,    -1f, 0f,   0f,    1f, 0f,
             0.25f, -1f, 0f,   0.25f, 1f, 0f,
             0.5f,  -1f, 0f,   0.5f,  1f, 0f,
             0.75f, -1f, 0f,   0.75f, 1f, 0f,
        };

        private const string ShaderSource = @"
            cbuffer ColorBuf : register(b0) { float4 color; };

            struct VSOut { float4 pos : SV_POSITION; };

            VSOut vs_main(float3 pos : POS) {
                VSOut o;
                o.pos = float4(pos, 1.0);
                return o;
            }

            float4 ps_main(VSOut i) : SV_TARGET {
                return color;
            }

            float4 ps_grid(VSOut i) : SV_TARGET {
                if (fmod(i.pos.x + i.pos.y, 8.0) < 6.0) discard;
                return color;
            }
            ";

        private D3DCompiler _compiler = null!;

        public unsafe void Initialize(nint hwnd, int width, int height)
        {
            _d3d11 = D3D11.GetApi(null);
            _dxgi = DXGI.GetApi(null);

            _width = width;
            _height = height;

            SilkMarshal.ThrowHResult(
                _d3d11.CreateDevice(
                    default(ComPtr<IDXGIAdapter>),
                    D3DDriverType.Hardware,
                    Software: default,
                    0u,
                    null, 0,
                    D3D11.SdkVersion,
                    ref _device,
                    null,
                    ref _context));

            _factory = _dxgi.CreateDXGIFactory<IDXGIFactory2>();

            var desc = new SwapChainDesc1
            {
                BufferCount = 2,
                Format = Format.FormatB8G8R8A8Unorm,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SwapEffect = SwapEffect.FlipDiscard,
                SampleDesc = new SampleDesc(1, 0)
            };
            var rastDesc = new RasterizerDesc
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                DepthClipEnable = true
            };

            SilkMarshal.ThrowHResult(
                _factory.CreateSwapChainForHwnd(
                    _device,
                    hwnd,
                    in desc,
                    null,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref _swapchain));

            _device.CreateRasterizerState(in rastDesc, ref _rastState);
            _context.RSSetState(_rastState);

            InitShaders();
            InitVertexBuffer();
            InitColorBuffer();
            InitGridBuffer();

            _initialized = true;
            _needsResize = true;
            CompositionTarget.Rendering += OnRender;
        }

        private unsafe void InitShaders()
        {
            _compiler = D3DCompiler.GetApi();
            var shaderBytes = Encoding.ASCII.GetBytes(ShaderSource);

            ComPtr<ID3D10Blob> vsCode = default, vsErrors = default;
            var hr = (HResult)_compiler.Compile(
                in shaderBytes[0], (nuint)shaderBytes.Length,
                nameof(ShaderSource), null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "vs_main", "vs_5_0", 0, 0,
                ref vsCode, ref vsErrors);

            if (hr.IsFailure)
            {
                if (vsErrors.Handle != null)
                    throw new Exception(SilkMarshal.PtrToString((nint)vsErrors.GetBufferPointer()));
                hr.Throw();
            }

            ComPtr<ID3D10Blob> psCode = default, psErrors = default;
            hr = (HResult)_compiler.Compile(
                in shaderBytes[0], (nuint)shaderBytes.Length,
                nameof(ShaderSource), null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "ps_main", "ps_5_0", 0, 0,
                ref psCode, ref psErrors);

            if (hr.IsFailure)
            {
                if (psErrors.Handle != null)
                    throw new Exception(SilkMarshal.PtrToString((nint)psErrors.GetBufferPointer()));
                hr.Throw();
            }

            ComPtr<ID3D10Blob> psGridCode = default, psGridErrors = default;
            hr = (HResult)_compiler.Compile(
                in shaderBytes[0], (nuint)shaderBytes.Length,
                nameof(ShaderSource), null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "ps_grid", "ps_5_0", 0, 0,
                ref psGridCode, ref psGridErrors);

            if (hr.IsFailure)
            {
                if (psGridErrors.Handle != null)
                    throw new Exception(SilkMarshal.PtrToString((nint)psGridErrors.GetBufferPointer()));
                hr.Throw();
            }

            SilkMarshal.ThrowHResult(
                _device.CreateVertexShader(vsCode.GetBufferPointer(),
                    vsCode.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _vs));

            SilkMarshal.ThrowHResult(
                _device.CreatePixelShader(psGridCode.GetBufferPointer(),
                    psGridCode.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _psGrid));

            SilkMarshal.ThrowHResult(
                _device.CreatePixelShader(psCode.GetBufferPointer(),
                    psCode.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(), ref _ps));


            fixed (byte* name = SilkMarshal.StringToMemory("POS"))
            {
                var element = new InputElementDesc
                {
                    SemanticName = name,
                    Format = Format.FormatR32G32B32Float,
                    InputSlotClass = InputClassification.PerVertexData
                };
                _device.CreateInputLayout(in element, 1,
                    vsCode.GetBufferPointer(), vsCode.GetBufferSize(),
                    ref _inputLayout);
            }

            vsCode.Dispose(); vsErrors.Dispose();
            psCode.Dispose(); psErrors.Dispose();
            psGridCode.Dispose(); psGridErrors.Dispose();
        }

        private unsafe void InitVertexBuffer()
        {
            float[] vertices =
            {
                0.0f,  0.5f, 0.0f,   
               -0.5f, -0.5f, 0.0f,   
                0.5f, -0.5f, 0.0f, 
            };

            var bufDesc = new BufferDesc
            {
                ByteWidth = (uint)(vertices.Length * sizeof(float)),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.VertexBuffer
            };

            fixed (float* data = vertices)
            {
                var sub = new SubresourceData { PSysMem = data };
                SilkMarshal.ThrowHResult(
                    _device.CreateBuffer(in bufDesc, in sub, ref _vertexBuffer));
            }
        }

        private unsafe void InitGridBuffer()
        {
            var bufDesc = new BufferDesc
            {
                ByteWidth = (uint)(GridLines.Length * sizeof(float)),
                Usage = Usage.Immutable,
                BindFlags = (uint)BindFlag.VertexBuffer
            };

            fixed (float* data = GridLines)
            {
                var sub = new SubresourceData { PSysMem = data };
                SilkMarshal.ThrowHResult(
                    _device.CreateBuffer(in bufDesc, in sub, ref _gridBuffer));
            }
        }

        private unsafe void InitColorBuffer()
        {
            var bufDesc = new BufferDesc
            {
                ByteWidth = 16,
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.ConstantBuffer
            };

            SilkMarshal.ThrowHResult(
                _device.CreateBuffer(in bufDesc, null, ref _colorBuffer));
        }

        public void UpdateTriangle(TriangleData? triangle)
        {
            if (triangle != null) 
                _lastValidTriangle = triangle;

            _currentTriangle = triangle;
        }

        private unsafe void ApplyTriangle(TriangleData triangle)
        {
            var vertices = TriangleGeometry.ComputeNdcVertices(triangle.A, triangle.B, triangle.C);
            fixed (float* data = vertices)
                _context.UpdateSubresource(_vertexBuffer, 0, null, data, 0, 0);

            var hex = triangle.ColorHex.TrimStart('#');
            float r = Convert.ToInt32(hex[..2], 16) / 255f;
            float g = Convert.ToInt32(hex[2..4], 16) / 255f;
            float b = Convert.ToInt32(hex[4..6], 16) / 255f;
            float[] color = [r, g, b, 1.0f];

            fixed (float* data = color)
                _context.UpdateSubresource(_colorBuffer, 0, null, data, 0, 0);
        }

        public void Resize(int width, int height)
        {
            if (!_initialized || width <= 0 || height <= 0) return;
            _width = width;
            _height = height;
            _needsResize = true;
        }

        private unsafe void OnRender(object? sender, EventArgs e)
        {
            if (!_initialized) 
                return;

            if (_needsResize)
            {
                _swapchain.ResizeBuffers(0, (uint)_width, (uint)_height, Format.FormatUnknown, 0);
                _needsResize = false;
            }

            var toDraw = _currentTriangle ?? _lastValidTriangle;

            using var framebuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0);

            Texture2DDesc texDesc = default;
            framebuffer.GetDesc(ref texDesc);
            if (texDesc.Width == 0 || texDesc.Height == 0) 
                return;

            var viewport = new Viewport(0, 0, texDesc.Width, texDesc.Height, 0, 1);

            ComPtr<ID3D11RenderTargetView> rtv = default;
            SilkMarshal.ThrowHResult(
                _device.CreateRenderTargetView(framebuffer, null, ref rtv));

            _context.ClearRenderTargetView(rtv, ref ClearColor[0]);

            _context.RSSetViewports(1, in viewport);
            _context.OMSetRenderTargets(1, ref rtv, ref Unsafe.NullRef<ID3D11DepthStencilView>());
            _context.IASetInputLayout(_inputLayout);

            _context.VSSetShader(_vs, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            _context.PSSetShader(_ps, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);

            uint stride = 3 * sizeof(float), offset = 0;

            if (toDraw != null)
            {
                _context.IASetVertexBuffers(0, 1, ref _vertexBuffer, in stride, in offset);
                _context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
                ApplyTriangle(toDraw);
                _context.Draw(3, 0);
            }

            _context.IASetVertexBuffers(0, 1, ref _gridBuffer, in stride, in offset);
            _context.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyLinelist);
            _context.PSSetShader(_psGrid, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);

            fixed (float* c = GridColor)
                _context.UpdateSubresource(_colorBuffer, 0, null, c, 0, 0);
            _context.PSSetConstantBuffers(0, 1, ref _colorBuffer);
            _context.Draw(28, 0);


            _swapchain.Present(1, 0);

            rtv.Dispose();
        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= OnRender;
            _initialized = false;

            _rastState.Dispose();
            _vertexBuffer.Dispose();
            _colorBuffer.Dispose();
            _inputLayout.Dispose();
            _gridBuffer.Dispose();
            _vs.Dispose();
            _ps.Dispose();
            _psGrid.Dispose();
            _compiler.Dispose();
            _swapchain.Dispose();
            _context.Dispose();
            _device.Dispose();
            _factory.Dispose();
            _dxgi.Dispose();
            _d3d11.Dispose();
        }

    }
}
