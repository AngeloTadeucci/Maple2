using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Maple2.Server.DebugGame.Graphics.Resources;

public class Texture {
    public DebugGraphicsContext Context { get; init; }
    public static string TextureRootPath = "";
    public ComPtr<ID3D11Texture2D> Texture2D;
    public ComPtr<ID3D11ShaderResourceView> ResourceView;
    public ComPtr<ID3D11SamplerState> SamplerState;

    public Texture(DebugGraphicsContext context) {
        Context = context;
    }

    public unsafe void Load(string path) {
        CleanUp();

        path = Path.GetFullPath(Path.Combine(TextureRootPath, path));

        DecoderOptions decoderOptions = new DecoderOptions();
        decoderOptions.Configuration.PreferContiguousImageBuffers = true;

        Image<Bgra32> image = Image.Load<Bgra32>(decoderOptions, path);

        Texture2DDesc textureDesc = new Texture2DDesc {
            Width = (uint) image.Width,
            Height = (uint) image.Height,
            Format = Silk.NET.DXGI.Format.FormatB8G8R8A8Unorm,
            MipLevels = 1,
            BindFlags = (uint) BindFlag.ShaderResource,
            Usage = Usage.Default,
            CPUAccessFlags = 0,
            MiscFlags = (uint) ResourceMiscFlag.None,
            SampleDesc = new SampleDesc(1, 0),
            ArraySize = 1,
        };

        if (image.DangerousTryGetSinglePixelMemory(out var imageData)) {
            using (var pixelData = imageData.Pin()) {
                SubresourceData subresourceData = new SubresourceData {
                    PSysMem = pixelData.Pointer,
                    SysMemPitch = (uint) image.Width * sizeof(int),
                    SysMemSlicePitch = (uint) (image.Width * sizeof(int) * image.Height),
                };

                ID3D11Texture2D* texture = default;
                SilkMarshal.ThrowHResult(Context.DxDevice.CreateTexture2D(
                    pDesc: in textureDesc,
                    pInitialData: in subresourceData,
                    ppTexture2D: ref texture));
                Texture2D = texture;
            }
        } else {
            // handle split texture memory. likely covers .dds textures
        }

        ShaderResourceViewDesc resourceViewDesc = new ShaderResourceViewDesc {
            Format = textureDesc.Format,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Anonymous = new ShaderResourceViewDescUnion {
                Texture2D = {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                },
            },
        };

        ID3D11ShaderResourceView* resourceView = default;
        SilkMarshal.ThrowHResult(Context.DxDevice.CreateShaderResourceView(
            pResource: Texture2D,
            pDesc: ref resourceViewDesc,
            ppSRView: ref resourceView));
        ResourceView = resourceView;

        SamplerDesc samplerDesc = new SamplerDesc {
            Filter = Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            MipLODBias = 0,
            MaxAnisotropy = 1,
            MinLOD = float.MinValue,
            MaxLOD = float.MaxValue,
        };

        samplerDesc.BorderColor[0] = 0.0f;
        samplerDesc.BorderColor[1] = 0.0f;
        samplerDesc.BorderColor[2] = 0.0f;
        samplerDesc.BorderColor[3] = 1.0f;

        ID3D11SamplerState* samplerState = default;
        SilkMarshal.ThrowHResult(Context.DxDevice.CreateSamplerState(
            pSamplerDesc: in samplerDesc,
            ppSamplerState: ref samplerState));
        SamplerState = samplerState;
    }

    public unsafe void CleanUp() {
        if (Texture2D.Handle is null) {
            return;
        }

        Texture2D.Dispose();
        ResourceView.Dispose();
        SamplerState.Dispose();
    }

    public unsafe void Bind() {
        // bad impl, needs to be changed to be extensible for multiple textures
        // all textures need to be prefetched, organized, and fed in through SetSamplers and SetShaderResources together
        Context.DxDeviceContext.PSSetSamplers(0, 1, ref SamplerState);
        Context.DxDeviceContext.PSSetShaderResources(0, 1, ref ResourceView);
    }
}

