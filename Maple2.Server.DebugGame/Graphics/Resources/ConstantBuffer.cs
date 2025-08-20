using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Maple2.Server.DebugGame.Graphics.Resources;

public class ConstantBuffer {
    public DebugGraphicsContext Context { get; init; }
    public ComPtr<ID3D11Buffer> Buffer { get; private set; }
    public int ResourceSize { get; private set; }
    public int ResourceCount { get; private set; }

    public ConstantBuffer(DebugGraphicsContext context) {
        Context = context;
    }

#pragma warning disable CS8500
    public unsafe void Initialize<T>(in T data, Usage usage = Usage.Default, CpuAccessFlag cpuAccess = CpuAccessFlag.None) where T : notnull {
        ResourceSize = sizeof(T);
        ResourceCount = 1;

        fixed (T* dataPointer = &data) {
            BufferDesc bufferDesc = new BufferDesc() {
                ByteWidth = (uint) ResourceSize,
                Usage = usage,
                BindFlags = (uint) (BindFlag.ConstantBuffer),
                CPUAccessFlags = (uint) cpuAccess,
                MiscFlags = 0,
                StructureByteStride = (uint) ResourceSize,
            };

            SubresourceData initialData = new SubresourceData() {
                PSysMem = dataPointer,
                SysMemPitch = 0,
                SysMemSlicePitch = 0,
            };

            ID3D11Buffer* buffer = null;

            SilkMarshal.ThrowHResult(Context.DxDevice.CreateBuffer(
                pDesc: ref bufferDesc,
                pInitialData: ref initialData,
                ppBuffer: ref buffer));

            Buffer = buffer;
        }
    }

    public unsafe void Initialize<T>(T[] data, Usage usage = Usage.Default, CpuAccessFlag cpuAccess = CpuAccessFlag.None) where T : notnull {
        ResourceSize = sizeof(T);
        ResourceCount = data.Length;

        fixed (T* dataPointer = &data[0]) {
            BufferDesc bufferDesc = new BufferDesc() {
                ByteWidth = (uint) (ResourceSize * ResourceCount),
                Usage = usage,
                BindFlags = (uint) (BindFlag.ConstantBuffer),
                CPUAccessFlags = (uint) cpuAccess,
                MiscFlags = 0,
                StructureByteStride = (uint) ResourceSize,
            };

            SubresourceData initialData = new SubresourceData() {
                PSysMem = dataPointer,
                SysMemPitch = 0,
                SysMemSlicePitch = 0,
            };

            ID3D11Buffer* buffer = null;

            SilkMarshal.ThrowHResult(Context.DxDevice.CreateBuffer(
                pDesc: ref bufferDesc,
                pInitialData: ref initialData,
                ppBuffer: ref buffer));

            Buffer = buffer;
        }
    }

    public unsafe void Update<T>(in T data) {
        int resourceSize = sizeof(T);
        int resourceCount = 1;

        if (Buffer.Handle is null) {
            throw new InvalidOperationException($"Attempt to upload {resourceCount} objects of size {resourceSize} bytes to uninitialized constant buffer");
        }

        if (resourceSize != ResourceSize || resourceCount != ResourceCount) {
            throw new ArgumentException($"Constant buffer initialized with {ResourceCount} objects of size {ResourceSize} bytes, attempt to update with {resourceCount} objects of size {resourceSize} bytes");
        }

        ID3D11Resource* bufferPointer = (ID3D11Resource*) (ID3D11Buffer*) Buffer;

        fixed (T* dataPointer = &data) {
            Box* box = null;
            Context.DxDeviceContext.UpdateSubresource(
                pDstResource: bufferPointer,
                DstSubresource: 0u,
                pDstBox: box,
                pSrcData: dataPointer,
                SrcRowPitch: 0u,
                SrcDepthPitch: 0u);
        }
    }

    public unsafe void Update<T>(T[] data) {
        int resourceSize = sizeof(T);
        int resourceCount = data.Length;

        if (Buffer.Handle is null) {
            throw new InvalidOperationException($"Attempt to upload {resourceCount} objects of size {resourceSize} bytes to uninitialized constant buffer");
        }

        if (resourceSize != ResourceSize || resourceCount != ResourceCount) {
            throw new ArgumentException($"Constant buffer initialized with {ResourceCount} objects of size {ResourceSize} bytes, attempt to update with {resourceCount} objects of size {resourceSize} bytes");
        }

        ID3D11Resource* bufferPointer = (ID3D11Resource*) (ID3D11Buffer*) Buffer;

        fixed (T* dataPointer = &data[0]) {
            Box* box = null;
            Context.DxDeviceContext.UpdateSubresource(
                pDstResource: bufferPointer,
                DstSubresource: 0u,
                pDstBox: box,
                pSrcData: dataPointer,
                SrcRowPitch: 0u,
                SrcDepthPitch: 0u);
        }
    }
#pragma warning restore CS8500

    public unsafe void CleanUp() {
        if (Buffer.Handle is null) {
            return;
        }

        Buffer.Dispose();
    }
}
