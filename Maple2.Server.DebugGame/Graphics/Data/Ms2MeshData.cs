using Maple2.Server.DebugGame.Graphics.Data.VertexBuffer;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Maple2.Server.DebugGame.Graphics.Data {
    // Vertex attributes need to be at byte alignments that are multiples of their size
    namespace VertexBuffer {
        // Bindings[0] - Position is separate to improve early depth test pixel culling
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PositionBinding {
            public Vector3 Position; // POSITION, POSITION_BP

            public PositionBinding() { }
            public PositionBinding(Vector3 position) {
                Position = position;
            }
        }

        // Bindings[1] - Contains general use vertex attributes
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct AttributeBinding {
            public Vector3 Normal; // NORMAL, NORMAL_BP
            public uint Color; // COLOR
            public Vector2 TexCoord; // TEXCOORD

            public AttributeBinding() { }
            public AttributeBinding(Vector3 normal, uint color, Vector2 texcoord) {
                Normal = normal;
                Color = color;
                TexCoord = texcoord;
            }
        }

        // Bindings[2] - Contains orientation data for TBN matrix for normal mapping & similar techniques
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct OrientationBinding {
            public Vector3 Tangent; // TANGENT, TANGENT_BP
            public Vector3 Binormal; // BINORMAL, BINORMAL_BP

            public OrientationBinding() { }
            public OrientationBinding(Vector3 tangent, Vector3 binormal) {
                Tangent = tangent;
                Binormal = binormal;
            }
        }

        // Bindings[3] - Contains morph shape targets
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MorphBinding {
            // MORPH_POSITION & MORPH_POSITION_BP are left out because they're largely just duplicates of POSITION
            public Vector3 MorphPos1; // MORPH_POSITION1, MORPH_POSITION_BP1

            public MorphBinding() { }
            public MorphBinding(Vector3 morphPos1) {
                MorphPos1 = morphPos1;
            }
        }

        // Bindings[4] - Contains skeletal mesh blending parameters
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct BlendBinding {
            public Vector3 BlendWeight; // BLENDWEIGHT
            public uint BlendIndices; // BLENDINDICES

            public BlendBinding() { }
            public BlendBinding(Vector3 blendWeight, uint blendIndices) {
                BlendWeight = blendWeight;
                BlendIndices = blendIndices;
            }
        }
    }

    public class Ms2MeshData {
        public int VertexCount {
            get => positionBinding.Count;
            set {
                Resize(positionBinding, value);
                Resize(attributeBinding, value);
                Resize(orientationBinding, value);
                Resize(morphBinding, value);
                Resize(blendBinding, value);
            }
        }

        public Span<PositionBinding> PositionBinding { get => CollectionsMarshal.AsSpan(positionBinding); }
        public Span<AttributeBinding> AttributeBinding { get => CollectionsMarshal.AsSpan(attributeBinding); }
        public Span<OrientationBinding> OrientationBinding { get => CollectionsMarshal.AsSpan(orientationBinding); }
        public Span<MorphBinding> MorphBinding { get => CollectionsMarshal.AsSpan(morphBinding); }
        public Span<BlendBinding> BlendBinding { get => CollectionsMarshal.AsSpan(blendBinding); }
        public Span<uint> IndexBuffer { get => CollectionsMarshal.AsSpan(indexBuffer); }

        public int PrimitiveCount {
            get => (int) indexBuffer.Count / PrimitiveVertexCount;
            set => Resize(indexBuffer, value);
        }

        public int PrimitiveVertexCount { get => isTriangleMesh ? 3 : 2; }

        public bool IsTriangleMesh {
            get => isTriangleMesh;
            set {
                if (isTriangleMesh != value) {
                    int newPrimitiveVertexCount = value ? 3 : 2;

                    Resize(indexBuffer, indexBuffer.Count / PrimitiveVertexCount * newPrimitiveVertexCount);
                }
                isTriangleMesh = value;
            }
        }

        private readonly List<PositionBinding> positionBinding;
        private readonly List<AttributeBinding> attributeBinding;
        private readonly List<OrientationBinding> orientationBinding;
        private readonly List<MorphBinding> morphBinding;
        private readonly List<BlendBinding> blendBinding;
        private readonly List<uint> indexBuffer;
        bool isTriangleMesh;

        public Ms2MeshData() {
            isTriangleMesh = true;

            positionBinding = new List<PositionBinding>();
            attributeBinding = new List<AttributeBinding>();
            orientationBinding = new List<OrientationBinding>();
            morphBinding = new List<MorphBinding>();
            blendBinding = new List<BlendBinding>();
            indexBuffer = new List<uint>();
        }

        public void SetPositionBinding(PositionBinding[] buffer) {
            VertexCount = buffer.Length;

            positionBinding.Clear();
            positionBinding.AddRange(buffer);
        }

        public void SetAttributeBinding(AttributeBinding[] buffer) {
            VertexCount = buffer.Length;

            attributeBinding.Clear();
            attributeBinding.AddRange(buffer);
        }

        public void SetOrientationBinding(OrientationBinding[] buffer) {
            VertexCount = buffer.Length;

            orientationBinding.Clear();
            orientationBinding.AddRange(buffer);
        }

        public void SetMorphBinding(MorphBinding[] buffer) {
            VertexCount = buffer.Length;

            morphBinding.Clear();
            morphBinding.AddRange(buffer);
        }

        public void SetBlendBinding(BlendBinding[] buffer) {
            VertexCount = buffer.Length;

            blendBinding.Clear();
            blendBinding.AddRange(buffer);
        }

        public void SetIndexBuffer(uint[] buffer) {
            indexBuffer.Clear();
            indexBuffer.AddRange(buffer);
        }

        private void Resize<BufferType>(List<BufferType> buffer, int size) {
            buffer.EnsureCapacity(size);

            if (buffer.Count > size) {
                buffer.RemoveRange(size, buffer.Count - size);

                return;
            }

            if (buffer.Count < size) {
                buffer.AddRange(new BufferType[size - buffer.Count]);
            }
        }
    }

}
