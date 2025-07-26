using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Maple2.Server.DebugGame.Graphics.Resources;
using Maple2.Server.DebugGame.Graphics.Enum;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class SceneState {
    public DebugGraphicsContext Context { get; init; }
    private readonly List<ComPtr<ID3D11SamplerState>> samplerStates = [];
    private readonly List<ComPtr<ID3D11ShaderResourceView>> resourceViews = [];
    private readonly List<ComPtr<ID3D11Buffer>> vsConstantBuffers = [];
    private readonly List<ComPtr<ID3D11Buffer>> psConstantBuffers = [];
    private bool samplerStatesChanged;
    private bool resourceViewsChanged;
    private bool vsConstantBuffersChanged;
    private bool psConstantBuffersChanged;

    public SceneState(DebugGraphicsContext context) {
        Context = context;
    }

    private static void Grow<T>(List<T?> list, int size) {
        for (int i = list.Count; i < size; ++i) {
            list.Add(default);
        }
    }

    private static void AddSlot<T>(List<T?> list, int slot, T value) {
        Grow(list, slot + 1);

        list[slot] = value;
    }

    public void BindTexture(Texture? texture, int resourceSlot = 0, int samplerSlot = -1) {
        if (samplerSlot == -1) {
            samplerSlot = resourceSlot;
        }

        AddSlot(samplerStates, samplerSlot, texture?.SamplerState ?? null);
        AddSlot(resourceViews, resourceSlot, texture?.ResourceView ?? null);

        samplerStatesChanged = true;
        resourceViewsChanged = true;
    }

    public void BindConstantBuffer(ConstantBuffer buffer, int slot, ShaderStageFlags stages) {
        if ((stages & ShaderStageFlags.Vertex) != ShaderStageFlags.None) {
            AddSlot(vsConstantBuffers, slot, buffer.Buffer);

            vsConstantBuffersChanged = true;
        }

        if ((stages & ShaderStageFlags.Pixel) != ShaderStageFlags.None) {
            AddSlot(psConstantBuffers, slot, buffer.Buffer);

            psConstantBuffersChanged = true;
        }
    }

    private static unsafe Type*[] GetPointerArray<Type>(List<ComPtr<Type>> list) where Type : unmanaged, IComVtbl<Type> {
        Type*[] array = new Type*[list.Count];

        for (int i = 0; i < list.Count; i++) {
            array[i] = list[i].GetPinnableReference();
        }

        return array;
    }

    public unsafe void UpdateBindings() {
        if (samplerStatesChanged) {
            samplerStatesChanged = false;

            ID3D11SamplerState*[] samplerStateArray = GetPointerArray(samplerStates);

            if (samplerStates.Count > 0) {
                Context.DxDeviceContext.PSSetSamplers(0, (uint) samplerStates.Count, ref samplerStateArray[0]);
            } else {
                Context.DxDeviceContext.PSSetSamplers(0, 0, null);
            }
        }

        if (resourceViewsChanged) {
            resourceViewsChanged = false;

            ID3D11ShaderResourceView*[] resourceViewArray = GetPointerArray(resourceViews);

            if (resourceViews.Count > 0) {
                Context.DxDeviceContext.PSSetShaderResources(0, (uint) resourceViews.Count, ref resourceViewArray[0]);
            } else {
                Context.DxDeviceContext.PSSetShaderResources(0, 0, null);
            }
        }

        if (vsConstantBuffersChanged) {
            vsConstantBuffersChanged = false;

            ID3D11Buffer*[] vsConstBufferArray = GetPointerArray(vsConstantBuffers);

            if (vsConstantBuffers.Count > 0) {
                Context.DxDeviceContext.VSSetConstantBuffers(0, (uint) vsConstantBuffers.Count, ref vsConstBufferArray[0]);
            } else {
                Context.DxDeviceContext.VSSetConstantBuffers(0, 0, null);
            }
        }

        if (psConstantBuffersChanged) {
            psConstantBuffersChanged = false;

            ID3D11Buffer*[] psConstBufferArray = GetPointerArray(psConstantBuffers);

            if (psConstantBuffers.Count > 0) {
                Context.DxDeviceContext.PSSetConstantBuffers(0, (uint) psConstantBuffers.Count, ref psConstBufferArray[0]);
            } else {
                Context.DxDeviceContext.PSSetConstantBuffers(0, 0, null);
            }
        }
    }
}
