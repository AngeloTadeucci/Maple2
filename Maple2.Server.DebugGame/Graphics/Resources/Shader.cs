using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System.Numerics;
using Silk.NET.DXGI;
using System.Runtime.CompilerServices;

namespace Maple2.Server.DebugGame.Graphics.Resources;

public abstract class Shader {
    public static string ShaderRootPath = "";

    protected ComPtr<ID3D10Blob> Bytecode;
    protected ComPtr<ID3D10Blob> CompileErrors;
    public bool Loaded { get; private set; }

    public Shader() {
    }

    protected unsafe void LoadBytecode(DebugGraphicsContext context, string path, string entry, string target) {
        path = Path.GetFullPath(Path.Combine(ShaderRootPath, path));

        string source = File.ReadAllText(path);
        ID3DInclude* defaultInclude = (ID3DInclude*) 1; // enable simple include behavior

        ID3D10Blob* bytecode = default;
        ID3D10Blob* compileErrors = default;

        HResult result = context.Compiler!.Compile(
            pSrcData: Marshal(source),
            SrcDataSize: (nuint) source.Length,
            pSourceName: Marshal(path),
            pDefines: null,
            pInclude: defaultInclude,
            pEntrypoint: Marshal(entry),
            pTarget: Marshal(target),
            Flags1: 0,
            Flags2: 0,
            ppCode: ref bytecode,
            ppErrorMsgs: ref compileErrors);

        Bytecode = bytecode;
        CompileErrors = compileErrors;

        if (result.IsFailure) {
            if (CompileErrors.Handle is not null) {
                Log.Error(SilkMarshal.PtrToString((nint) CompileErrors.GetBufferPointer())!);
            }

            result.Throw();
        }
    }

    protected void CleanUpBytecode() {
        Loaded = false;

        Bytecode.Dispose();
        CompileErrors.Dispose();
    }

    protected unsafe byte* Marshal(string name) {
        fixed (byte* marshalledName = SilkMarshal.StringToMemory(name)) {
            return marshalledName;
        }
    }
}

public class PixelShader : Shader {
    public DebugGraphicsContext Context { get; init; }
    public ComPtr<ID3D11PixelShader> Shader;

    public PixelShader(DebugGraphicsContext context) {
        Context = context;
    }

    public void Load(string path, string entry) {
        unsafe {
            LoadBytecode(Context, path, entry, "ps_5_0");

            ID3D11PixelShader* shader = default;

            SilkMarshal.ThrowHResult(Context.DxDevice.CreatePixelShader(
                pShaderBytecode: Bytecode.GetBufferPointer(),
                BytecodeLength: Bytecode.GetBufferSize(),
                pClassLinkage: ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ppPixelShader: ref shader));

            Shader = shader;
        }
    }

    public void CleanUp() {
        if (!Loaded) {
            return;
        }

        CleanUpBytecode();

        Shader.Dispose();
    }

    public unsafe void Bind() {
        ID3D11ClassInstance* classInstances = null;
        Context.DxDeviceContext.PSSetShader(Shader, ref classInstances, 0);
    }
}

public class VertexShader : Shader {
    public ComPtr<ID3D11VertexShader> Shader;
    public ComPtr<ID3D11InputLayout> InputLayout { get; private set; }
    public DebugGraphicsContext Context { get; init; }

    public VertexShader(DebugGraphicsContext context) {
        Context = context;
    }

    public void Load(string path, string entry) {
        if (Loaded) {
            CleanUp();
        }

        LoadBytecode(Context, path, entry, "vs_5_0");

        unsafe {
            ID3D11VertexShader* shader = default;
            SilkMarshal.ThrowHResult(Context.DxDevice.CreateVertexShader(
                pShaderBytecode: Bytecode.GetBufferPointer(),
                BytecodeLength: Bytecode.GetBufferSize(),
                pClassLinkage: ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ppVertexShader: ref shader));

            Shader = shader;
        }

        CreateInputLayout();
    }

    private unsafe void CreateInputLayout() {
        InputElementDesc[] vertexAttributes = new InputElementDesc[9];

        vertexAttributes[0] = new InputElementDesc {
            SemanticName = Marshal("POSITION"),
            Format = Format.FormatR32G32B32Float,
            AlignedByteOffset = 0,
            InputSlot = 0,
        };
        vertexAttributes[1] = new InputElementDesc {
            SemanticName = Marshal("NORMAL"),
            Format = Format.FormatR32G32B32Float,
            AlignedByteOffset = 0,
            InputSlot = 1,
        };
        vertexAttributes[2] = new InputElementDesc {
            SemanticName = Marshal("COLOR"),
            Format = Format.FormatR32Uint,
            AlignedByteOffset = (uint) sizeof(Vector3),
            InputSlot = 1,
        };
        vertexAttributes[3] = new InputElementDesc {
            SemanticName = Marshal("TEXCOORD"),
            Format = Format.FormatR32G32Float,
            AlignedByteOffset = (uint) (sizeof(Vector3) + sizeof(uint)),
            InputSlot = 1,
        };
        vertexAttributes[4] = new InputElementDesc {
            SemanticName = Marshal("TANGENT"),
            Format = Format.FormatR32G32B32Float,
            AlignedByteOffset = 0,
            InputSlot = 2,
        };
        vertexAttributes[5] = new InputElementDesc {
            SemanticName = Marshal("BINORMAL"),
            Format = Format.FormatR32G32B32Float,
            AlignedByteOffset = (uint) sizeof(Vector3),
            InputSlot = 2,
        };
        vertexAttributes[6] = new InputElementDesc {
            SemanticName = Marshal("MORPH_POSITION"),
            Format = Format.FormatR32G32B32Float,
            AlignedByteOffset = 0,
            InputSlot = 3,
        };
        vertexAttributes[7] = new InputElementDesc {
            SemanticName = Marshal("BLENDWEIGHT"),
            Format = Format.FormatR32G32B32Float,
            AlignedByteOffset = 0,
            InputSlot = 4,
        };
        vertexAttributes[8] = new InputElementDesc {
            SemanticName = Marshal("BLENDINDICES"),
            Format = Format.FormatR32Uint,
            AlignedByteOffset = (uint) sizeof(Vector3),
            InputSlot = 4,
        };

        for (int i = 0; i < vertexAttributes.Length; i++) {
            vertexAttributes[i].SemanticIndex = 0;
            vertexAttributes[i].InputSlotClass = InputClassification.PerVertexData;
            vertexAttributes[i].InstanceDataStepRate = 0;
        }

        fixed (InputElementDesc* inputElements = vertexAttributes) {
            ID3D11InputLayout* layout = default;
            SilkMarshal.ThrowHResult(Context.DxDevice.CreateInputLayout(
                pInputElementDescs: inputElements,
                NumElements: 9,
                pShaderBytecodeWithInputSignature: Bytecode.GetBufferPointer(),
                BytecodeLength: Bytecode.GetBufferSize(),
                ppInputLayout: ref layout));
            InputLayout = layout;
        }
    }

    public void CleanUp() {
        if (!Loaded) {
            return;
        }

        CleanUpBytecode();

        Shader.Dispose();
        InputLayout.Dispose();

        Shader = default;
        InputLayout = default;
    }

    public unsafe void Bind() {
        ID3D11ClassInstance* classInstances = null;
        Context.DxDeviceContext.VSSetShader(Shader, ref classInstances, 0);
        Context.DxDeviceContext.IASetInputLayout(InputLayout);
    }
}
