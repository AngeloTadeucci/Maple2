using Maple2.Server.DebugGame.Graphics.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Maple2.Server.DebugGame.Extensions;

namespace Maple2.Server.DebugGame.Graphics.Assets;

public class ShaderPipelines {
    public static string AssetRootPath = string.Empty;

    public DebugGraphicsContext Context { get; init; }
    public IReadOnlyDictionary<string, ShaderPipeline> Pipelines { get => pipelines; }

    private string currentDocument = string.Empty;
    private Dictionary<string, ShaderPipeline> pipelines = new();
    public bool CanRender => !CompilingShaders && !shaderErrors;
    public bool CompilingShaders {
        get {
            compilingShadersMutex.WaitOne();
            bool value = compilingShaders;
            compilingShadersMutex.ReleaseMutex();

            return value;
        }
        private set {
            compilingShadersMutex.WaitOne();
            compilingShaders = value;
            compilingShadersMutex.ReleaseMutex();
        }
    }
    public int RenderingFrames {
        get {
            renderingFramesMutex.WaitOne();
            int value = renderingFrames;
            renderingFramesMutex.ReleaseMutex();

            return value;
        }
    }
    private bool compilingShaders;
    private int renderingFrames;
    private bool shaderErrors;
    private Mutex compilingShadersMutex = new();
    private Mutex renderingFramesMutex = new();
    private HashSet<string> filesToWatch = new();
    private FileSystemWatcher? shaderWatcher;
    private FileSystemWatcher? pipelineWatcher;

    public ShaderPipelines(DebugGraphicsContext context) {
        Context = context;
    }

    public void StartedFrame() {
        renderingFramesMutex.WaitOne();
        ++renderingFrames;
        renderingFramesMutex.ReleaseMutex();
    }

    public void EndedFrame() {
        renderingFramesMutex.WaitOne();
        --renderingFrames;
        renderingFramesMutex.ReleaseMutex();
    }

    private bool NodeTypeError(XmlElement element, string expected = "", string label = "") {
        if (expected == "") {
            shaderErrors = true;

            DebugGraphicsContext.Logger.Error("[{0}]: Unrecognized {label}node type '{1}'", currentDocument, element.Name);

            return true;
        }

        if (element.Name != expected) {
            shaderErrors = true;

            DebugGraphicsContext.Logger.Error("[{0}]: Unrecognized {label}node type '{1}', expected '{2}'", currentDocument, element.Name, expected);

            return true;
        }

        return false;
    }

    private bool GetAttribute(XmlElement element, string attributeName, [NotNullWhen(returnValue: true)] out string? value) {
        if (!element.TryGetAttribute(attributeName, out value)) {
            shaderErrors = true;

            DebugGraphicsContext.Logger.Error("[{0}]: Expected attribute '{1}' on node type '{2}' not found", currentDocument, attributeName, element.Name);

            return false;
        }

        return true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e) {
        // TODO: optimize to reduce recompilations later?

        CompilingShaders = true;
    }

    private void InitializeFileWatchers(string pipelineRoot, string shaderRoot) {
        if (shaderWatcher is not null && pipelineWatcher is not null) {
            return;
        }

        NotifyFilters filters = default;
        filters |= NotifyFilters.CreationTime;
        filters |= NotifyFilters.DirectoryName;
        filters |= NotifyFilters.FileName;
        filters |= NotifyFilters.LastWrite;
        filters |= NotifyFilters.Security;
        filters |= NotifyFilters.Size;

        shaderWatcher = new FileSystemWatcher(shaderRoot);

        shaderWatcher.NotifyFilter = filters;
        shaderWatcher.Changed += OnChanged;
        shaderWatcher.Created += OnChanged;
        shaderWatcher.Deleted += OnChanged;
        shaderWatcher.Renamed += OnChanged;
        shaderWatcher.IncludeSubdirectories = true;
        shaderWatcher.EnableRaisingEvents = true;

        pipelineWatcher = new FileSystemWatcher(pipelineRoot);

        pipelineWatcher.NotifyFilter = filters;
        pipelineWatcher.Changed += OnChanged;
        pipelineWatcher.Created += OnChanged;
        pipelineWatcher.Deleted += OnChanged;
        pipelineWatcher.Renamed += OnChanged;
        pipelineWatcher.IncludeSubdirectories = true;
        pipelineWatcher.EnableRaisingEvents = true;
    }

    public void ExternalError() {
        shaderErrors = true;
    }

    public void LoadPipelines() {
        CompilingShaders = true;
        shaderErrors = false;

        string pipelineRoot = Path.Combine(AssetRootPath, "RenderPipeline");
        string shaderRoot = Path.Combine(AssetRootPath, "Shaders");
        DirectoryInfo directory = new DirectoryInfo(pipelineRoot);

        InitializeFileWatchers(pipelineRoot, shaderRoot);

        foreach (FileInfo file in directory.EnumerateFiles()) {
            string relpath = Path.GetRelativePath(pipelineRoot, file.FullName);
            currentDocument = relpath;

            if (file.Extension.ToLower() != ".xml") {
                shaderErrors = true;

                DebugGraphicsContext.Logger.Error("[{0}]: Attempt to load non xml file in RenderPipeline/", relpath);

                continue;
            }

            XmlDocument document = new XmlDocument();
            FileStream fileStream = file.Open(FileMode.Open, FileAccess.Read);

            document.Load(fileStream);

            XmlElement? root = document.DocumentElement;

            if (root is null) {
                shaderErrors = true;
                fileStream.Close();

                DebugGraphicsContext.Logger.Error("[{0}]: Missing root 'pipeline' element", relpath);

                continue;
            }

            if (NodeTypeError(root, "pipeline", "root ")) {
                fileStream.Close();

                continue;
            }

            if (!GetAttribute(root, "name", out string? pipelineName)) {
                fileStream.Close();

                continue;
            }

            if (pipelineName == "") {
                shaderErrors = true;
                fileStream.Close();

                DebugGraphicsContext.Logger.Error("[{0}]: Pipeline name is empty", relpath);

                continue;
            }

            DebugGraphicsContext.Logger.Information("[{0}]: Initializing pipeline '{1}'", relpath, pipelineName);

            ShaderPipeline pipeline = new() {
                Name = pipelineName
            };

            foreach (XmlElement pass in root.ChildNodes) {
                if (NodeTypeError(pass, "pass")) {
                    continue;
                }

                DebugGraphicsContext.Logger.Information("[{0}]: Adding render pass", relpath);

                RenderPass renderPass = new();

                foreach (XmlElement child in pass.ChildNodes) {
                    if (child.Name == "output") {
                        if (!GetAttribute(child, "name", out string? value)) {
                            continue;
                        }

                        renderPass.TargetName = value;

                        DebugGraphicsContext.Logger.Information("[{0}]: Registered render pass target '{1}'", relpath, value);

                        continue;
                    }

                    if (child.Name == "program") {
                        foreach (XmlElement shader in child.ChildNodes) {
                            if (NodeTypeError(shader, "shader")) {
                                continue;
                            }

                            if (!GetAttribute(shader, "path", out string? path)) {
                                continue;
                            }

                            if (!GetAttribute(shader, "stage", out string? stage)) {
                                continue;
                            }

                            if (!GetAttribute(shader, "entry", out string? entry)) {
                                continue;
                            }

                            string shaderPath = path;
                            path = Path.Combine(shaderRoot, path);

                            try {
                                switch (stage!) {
                                    case "vertex":
                                        if (renderPass.VertexShader is not null) {
                                            shaderErrors = true;

                                            DebugGraphicsContext.Logger.Error("[{0}]: Found unexpected second vertex shader", currentDocument);

                                            break;
                                        }

                                        renderPass.VertexShader = new VertexShader(Context);
                                        renderPass.VertexShader.Load(path, entry);

                                        DebugGraphicsContext.Logger.Information("[{0}]: Loaded vertex shader '{1}' with entry '{2}'", relpath, shaderPath, entry);

                                        break;
                                    case "pixel":
                                        if (renderPass.PixelShader is not null) {
                                            shaderErrors = true;

                                            DebugGraphicsContext.Logger.Error("[{0}]: Found unexpected second pixel shader", currentDocument);

                                            break;
                                        }

                                        renderPass.PixelShader = new PixelShader(Context);
                                        renderPass.PixelShader.Load(path, entry);

                                        DebugGraphicsContext.Logger.Information("[{0}]: Loaded pixel shader '{1}' with entry '{2}'", relpath, shaderPath, entry);

                                        break;
                                    default:
                                        shaderErrors = true;

                                        DebugGraphicsContext.Logger.Error("[{0}]: Unexpected shader stage: '{1}'", currentDocument, stage);

                                        break;
                                }
                            } catch (Exception e) {
                                shaderErrors = true;

                                DebugGraphicsContext.Logger.Error($"[{{0}}, {{1}}]: {e.Message}", currentDocument, stage);
                            }
                        }

                        continue;
                    }

                    NodeTypeError(child);
                }

                bool error = false;

                if (renderPass.TargetName != "swapchain") {
                    DebugGraphicsContext.Logger.Error("[{0}]: Unexpected render pass target: '{1}'", currentDocument, renderPass.TargetName);

                    error = true;
                }

                if (renderPass.VertexShader is null) {
                    DebugGraphicsContext.Logger.Error("[{0}]: Render pass missing vertex shader", currentDocument);

                    error = true;
                }

                if (renderPass.PixelShader is null) {
                    DebugGraphicsContext.Logger.Error("[{0}]: Render pass missing pixel shader", currentDocument);

                    error = true;
                }

                if (error) {
                    shaderErrors = true;

                    DebugGraphicsContext.Logger.Error("[{0}]: Failed to initialize render pass in pipeline '{1}'", currentDocument, pipelineName);

                    continue;
                }

                pipeline.RenderPass = renderPass;

                DebugGraphicsContext.Logger.Information("[{0}]: Finished render pass", relpath);
            }

            if (pipeline.RenderPass is null) {
                shaderErrors = true;
                fileStream.Close();

                DebugGraphicsContext.Logger.Error("[{0}]: Failed to initialize pipeline '{1}'", currentDocument, pipelineName);

                continue;
            }

            pipelines[pipelineName] = pipeline;

            DebugGraphicsContext.Logger.Information("[{0}]: Registered pipeline '{1}'", relpath, pipelineName);

            fileStream.Close();
        }

        if (shaderErrors) {
            DebugGraphicsContext.Logger.Error("Shader pipeline errors detected when initializing shaders");
        }

        CompilingShaders = false;
    }

    public void CleanUp() {
        foreach ((_, ShaderPipeline pipeline) in pipelines) {
            pipeline.CleanUp();
        }

        pipelines.Clear();
    }
}
