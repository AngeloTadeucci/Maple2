using Maple2.Server.DebugGame.Graphics.Data;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Maple2.Server.DebugGame.Graphics.Resources {
    public class Mesh {
        public DebugGraphicsContext Context { get; init; }
        private ComPtr<ID3D11Buffer> vertexBuffer0;
        private ComPtr<ID3D11Buffer> vertexBuffer1;
        private ComPtr<ID3D11Buffer> vertexBuffer2;
        private ComPtr<ID3D11Buffer> vertexBuffer3;
        private ComPtr<ID3D11Buffer> vertexBuffer4;
        private ComPtr<ID3D11Buffer> indexBuffer;
        private unsafe ID3D11Buffer*[] vertexBufferBindings;
        private uint[] vertexBufferStrides;
        private uint[] vertexBufferOffsets;
        private D3DPrimitiveTopology topologyType;
        private uint indexCount;

        public Mesh(DebugGraphicsContext context) {
            Context = context;

            unsafe {
                vertexBufferBindings = new ID3D11Buffer*[5];
            }
            vertexBufferStrides = new uint[5];
            vertexBufferOffsets = new uint[5];
        }

        public void UploadData(Ms2MeshData meshData) {
            CleanUp();

            if (meshData.VertexCount == 0 || meshData.PrimitiveCount == 0) {
                return;
            }

            UploadBuffer<Data.VertexBuffer.PositionBinding>(meshData.PositionBinding, ref vertexBuffer0, BindFlag.VertexBuffer);
            UploadBuffer<Data.VertexBuffer.AttributeBinding>(meshData.AttributeBinding, ref vertexBuffer1, BindFlag.VertexBuffer);
            UploadBuffer<Data.VertexBuffer.OrientationBinding>(meshData.OrientationBinding, ref vertexBuffer2, BindFlag.VertexBuffer);
            UploadBuffer<Data.VertexBuffer.MorphBinding>(meshData.MorphBinding, ref vertexBuffer3, BindFlag.VertexBuffer);
            UploadBuffer<Data.VertexBuffer.BlendBinding>(meshData.BlendBinding, ref vertexBuffer4, BindFlag.VertexBuffer);
            UploadBuffer<uint>(meshData.IndexBuffer, ref indexBuffer, BindFlag.IndexBuffer);

            unsafe {
                vertexBufferBindings[0] = vertexBuffer0;
                vertexBufferBindings[1] = vertexBuffer1;
                vertexBufferBindings[2] = vertexBuffer2;
                vertexBufferBindings[3] = vertexBuffer3;
                vertexBufferBindings[4] = vertexBuffer4;

                vertexBufferStrides[0] = (uint) sizeof(Data.VertexBuffer.PositionBinding);
                vertexBufferStrides[1] = (uint) sizeof(Data.VertexBuffer.AttributeBinding);
                vertexBufferStrides[2] = (uint) sizeof(Data.VertexBuffer.OrientationBinding);
                vertexBufferStrides[3] = (uint) sizeof(Data.VertexBuffer.MorphBinding);
                vertexBufferStrides[4] = (uint) sizeof(Data.VertexBuffer.BlendBinding);

            }

            topologyType = meshData.IsTriangleMesh ? D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist : D3DPrimitiveTopology.D3D10PrimitiveTopologyLinelist;
            indexCount = (uint) meshData.IndexBuffer.Length;
        }

#pragma warning disable CS8500
        private unsafe void UploadBuffer<BufferType>(ReadOnlySpan<BufferType> data, ref ComPtr<ID3D11Buffer> buffer, BindFlag flags) {
            var bufferDescription = new BufferDesc {
                ByteWidth = (uint) (data.Length * sizeof(BufferType)),
                Usage = Usage.Default,
                BindFlags = (uint) flags,
            };

            fixed (BufferType* bindingData = data) {
                var subresourceData = new SubresourceData {
                    PSysMem = bindingData,
                };

                ID3D11Buffer* bufferHandle = null;
                SilkMarshal.ThrowHResult(Context.DxDevice.CreateBuffer(ref bufferDescription, ref subresourceData, ref bufferHandle));
                buffer = bufferHandle;
            }
        }
#pragma warning restore CS8500

        public void CleanUp() {
            if (indexCount == 0) {
                return;
            }

            vertexBuffer0.Dispose();
            vertexBuffer1.Dispose();
            vertexBuffer2.Dispose();
            vertexBuffer3.Dispose();
            vertexBuffer4.Dispose();
            indexBuffer.Dispose();

            indexCount = 0;
        }

        public unsafe void Draw() {
            if (indexCount == 0) {
                return;
            }

            Context.DxDeviceContext.IASetPrimitiveTopology(topologyType);

            fixed (ID3D11Buffer** bindings = vertexBufferBindings) {
                Context.DxDeviceContext.IASetVertexBuffers(
                    StartSlot: 0,
                    NumBuffers: 5,
                    ppVertexBuffers: bindings,
                    pStrides: vertexBufferStrides,
                    pOffsets: vertexBufferOffsets);
            }

            Context.DxDeviceContext.IASetIndexBuffer(indexBuffer, Silk.NET.DXGI.Format.FormatR32Uint, 0);
            Context.DxDeviceContext.DrawIndexed(indexCount, 0, 0);
        }
    }
}
