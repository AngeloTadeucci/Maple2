using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using System.Numerics;
using Silk.NET.DXGI;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using Maple2.Server.DebugGame.Graphics.Assets;
using System.Runtime.InteropServices;

namespace Maple2.Server.DebugGame.Graphics.Resources;

internal class ShaderCompilationJob {
    public string ShaderPath = string.Empty;
    public byte[] IncludeSource = [];
    public GCHandle IncludeSourceHandle;
    public List<string> IncludedFiles = [];
}

public abstract class Shader {
    // HRESULT values, mandatory for some DX11 features
    public const int S_OK = 0;
    public const int E_FAIL = unchecked((int) 0x80004005);

    protected ComPtr<ID3D10Blob> Bytecode;
    protected ComPtr<ID3D10Blob> CompileErrors;
    public bool Loaded { get; private set; }
    private static unsafe ConcurrentDictionary<IntPtr, ShaderCompilationJob> _compilingShaders = new();
    public string FilePath { get; private set; } = string.Empty;
    public IReadOnlyList<string> IncludedFiles => includedFiles;
    private List<string> includedFiles = [];

    public Shader() { }

    static unsafe int IncludeOpen(ID3DInclude* include, D3DIncludeType includeType, byte* pFileName, void* pParentData, void** ppData, uint* pBytes) {
        string filePath = string.Empty;
        string shaderPath = string.Empty;
        string fileName = new((sbyte*) pFileName);
        ShaderCompilationJob? job = null;

        string message = includeType switch {
            D3DIncludeType.D3DIncludeLocal => $"Shader included path \"{fileName}\"",
            D3DIncludeType.D3DIncludeSystem => $"Shader included path <{fileName}>",
            _ => $"Shader included path '{fileName}'",
        };

        Log.Information(message);

        try {
            if (!_compilingShaders.TryGetValue((IntPtr) include, out ShaderCompilationJob? value)) {
                Log.Error("[{ShaderPath}]: Failed to find shader path for compiling shader", shaderPath);

                return E_FAIL;
            }

            job = value;
            shaderPath = Path.GetRelativePath(ShaderPipelines.AssetRootPath, job.ShaderPath);

            if (job.IncludeSource.Length > 0) {
                Log.Error("[{ShaderPath}]: Old shader compilation job lingering, failed to clean up previous job", shaderPath);

                return E_FAIL;
            }

            switch (includeType) {
                case D3DIncludeType.D3DIncludeLocal:
                    filePath = job.ShaderPath;

                    DirectoryInfo? shaderParent = Directory.GetParent(filePath);

                    if (shaderParent is null) {
                        Log.Error("[{ShaderPath}]: Failed to find shader parent directory path for compiling shader", shaderPath);

                        return E_FAIL;
                    }

                    filePath = Path.Combine(shaderParent.FullName, fileName);

                    break;
                case D3DIncludeType.D3DIncludeSystem:
                    string shaderRoot = Path.Combine(ShaderPipelines.AssetRootPath, "Shaders");
                    filePath = Path.Combine(shaderRoot, fileName);

                    break;
                default:
                    throw new InvalidDataException($"Unexpected include type: {includeType}");
            }

            if (!File.Exists(filePath)) {
                Log.Error("[{ShaderPath}]: Failed to find included file path '{S}'", shaderPath, filePath);

                return E_FAIL;
            }

            FileStream file = File.Open(filePath, FileMode.Open, FileAccess.Read);

            if (file.Length > 0) {
                job.IncludeSource = new byte[file.Length];

                file.Read(job.IncludeSource);

                job.IncludeSourceHandle = GCHandle.Alloc(job.IncludeSource, GCHandleType.Pinned);

                *ppData = job.IncludeSourceHandle.AddrOfPinnedObject().ToPointer();
                *pBytes = (uint) file.Length;
            } else {
                *ppData = null;
                *pBytes = 0;
            }

            job.IncludedFiles.Add(filePath);

            file.Close();

            return S_OK;
        } catch (Exception exception) {
            if (job is not null && job.IncludeSource.Length > 0) {
                job.IncludeSourceHandle.Free();
                job.IncludeSource = [];
            }

            Log.Error("[{ShaderPath}]: {ExceptionMessage}", shaderPath, exception.Message);

            return E_FAIL;
        }
    }

    static unsafe int IncludeClose(ID3DInclude* include, void* pData) {
        if (!_compilingShaders.TryGetValue((IntPtr) include, out ShaderCompilationJob? job)) {
            Log.Error($"Failed to find shader path for compiling shader");

            return E_FAIL;
        }

        if (job.IncludeSource.Length > 0) {
            job.IncludeSourceHandle.Free();
            job.IncludeSource = [];
        }

        return S_OK;
    }

    protected unsafe void LoadBytecode(DebugGraphicsContext context, string path, string entry, string target) {
        FilePath = path;

        string source = File.ReadAllText(path);
        ID3DInclude* defaultInclude = (ID3DInclude*) 1; // enable simple include behavior

        ID3D10Blob* bytecode = null;
        ID3D10Blob* compileErrors = null;

        #region pInclude
        // Ugly hack: C# doesn't support struct inheritance, so need to implement a vtable by hand to pass to pInclude
        var lpVtable = stackalloc void*[2];

        delegate*<ID3DInclude*, D3DIncludeType, byte*, void*, void**, uint*, int> open = &IncludeOpen;
        delegate*<ID3DInclude*, void*, int> close = &IncludeClose;

        lpVtable[0] = open;
        lpVtable[1] = close;

        var include = new ID3DInclude(lpVtable);
        #endregion

        ID3DInclude* pInclude = &include;
        ShaderCompilationJob job = new() {
            ShaderPath = path,
        };

        if (!_compilingShaders.TryAdd((IntPtr) pInclude, job)) {
            Log.Error("[{Path}]: Failed to add shader path to ID3DInclude compilation job dictionary", path);
        }

        HResult result = context.Compiler!.Compile(
            pSrcData: Marshal(source),
            SrcDataSize: (nuint) source.Length,
            pSourceName: Marshal(path),
            pDefines: null,
            pInclude: ref include,
            pEntrypoint: Marshal(entry),
            pTarget: Marshal(target),
            Flags1: 0,
            Flags2: 0,
            ppCode: ref bytecode,
            ppErrorMsgs: ref compileErrors);


        if (job.IncludeSource.Length > 0) {
            job.IncludeSourceHandle.Free();
            job.IncludeSource = [];
        }

        if (!_compilingShaders.TryRemove((IntPtr) pInclude, out _)) {
            Log.Error("[{Path}]: Failed to remove shader path from ID3DInclude compilation job dictionary", path);
        }

        includedFiles = job.IncludedFiles;

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

            ID3D11PixelShader* shader = null;

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
        Context.DxDeviceContext.PSSetShader(Shader, in classInstances, 0);
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
            ID3D11VertexShader* shader = null;
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
            ID3D11InputLayout* layout = null;
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
        Context.DxDeviceContext.VSSetShader(Shader, in classInstances, 0);
        Context.DxDeviceContext.IASetInputLayout(InputLayout);
    }
}
