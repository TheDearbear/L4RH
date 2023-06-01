using Speed.Engine.Camera;
using Speed.Engine.Texture;
using System.Diagnostics;
using System.Numerics;
using L4RH;
using L4RH.Model;
using Veldrid;
using Veldrid.SPIRV;
using System.Text;
using Speed.Engine.Camera.Frustum;
using Speed.Engine.SceneryObjects;
using Speed.Engine.Cache;
using L4RH.Model.Solids;
using L4RH.Model.Sceneries;

namespace Speed.Engine.Render;

public sealed class VeldridRenderContext : IRenderContext
{
    public event EventHandler<(double Delta, InputSnapshot? Input)>? NewLogicFrame;
    public event EventHandler<double>? NewRenderFrame;

    public IDictionary<uint, IObjectTexture> TextureStorage { get; } = new Dictionary<uint, IObjectTexture>();
    public TextureFSCacheController CacheController { get; } = new();
    public Region Region { get; private set; }
    public ICamera Camera { get; private set; }

    private RgbaFloat _bgColor = new(0, 0, 0, 1);
    public Vector4 BackgroundColor
    {
        get => _bgColor.ToVector4();
        set => _bgColor = new(value);
    }

    public int TotalObjects { get; private set; }
    public int TotalRendered { get; private set; }
    public bool IsLoading { get; private set; }
    public List<Scenery> RenderedSceneries { get; private set; } = new();

    private readonly GraphicsDevice _device;
    private readonly CommandList _cmd;
    private readonly Pipeline _pipeline;
    private readonly DeviceBuffer _projection;
    private readonly DeviceBuffer _view;
    private readonly ResourceLayout _modelLayout;
    private readonly ResourceLayout _fragmentLayout;
    private readonly ResourceSet _cameraSet;

    public VeldridRenderContext(GraphicsDevice device, ICamera camera, Region? region = null)
    {
        _device = device;

        #region Devices depended init

        var factory = _device.ResourceFactory;

        _cmd = factory.CreateCommandList();

        _projection = factory.CreateBuffer(new(64, BufferUsage.UniformBuffer));
        _view = factory.CreateBuffer(new(64, BufferUsage.UniformBuffer));

        var shaderSet = new ShaderSetDescription(
            new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                    new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                ),
                new VertexLayoutDescription(
                    new VertexElementDescription("InstanceColumn1", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                    new VertexElementDescription("InstanceColumn2", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                    new VertexElementDescription("InstanceColumn3", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                    new VertexElementDescription("InstanceColumn4", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
                ) { InstanceStepRate = 1 }
            },
            factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(File.ReadAllText("Assets/DefaultShader.vert")), "main", true),
                new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(File.ReadAllText("Assets/DefaultShader.frag")), "main", true)
            )
        );

        var vertexLayout = factory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("ViewBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("PerspectiveBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        _fragmentLayout = factory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("TextureSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        _modelLayout = factory.CreateResourceLayout(new(
            new ResourceLayoutElementDescription("SolidBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));



        _pipeline = factory.CreateGraphicsPipeline(new()
        {
            BlendState = BlendStateDescription.SingleAlphaBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = shaderSet,
            ResourceLayouts = new ResourceLayout[] { vertexLayout, _modelLayout, _fragmentLayout },
            Outputs = _device.SwapchainFramebuffer.OutputDescription,
            ResourceBindingModel = ResourceBindingModel.Improved
        });
        _cameraSet = factory.CreateResourceSet(new(vertexLayout, _view, _projection));

        #endregion

        Camera = camera;
        NewRenderFrame += OnRender;
        Region = new();
        UpdateRegion(region ?? new Region());
    }

    public void Dispose()
    {
        //
    }

    public void Resize(uint width, uint height)
    {
        Camera.AspectRatio = (float)((double)width / height);

        _device.ResizeMainWindow(width, height);
    }

    public void DoRender(double delta)
    {
        NewRenderFrame?.Invoke(this, delta);
    }

    public void DoLogic(double delta, InputSnapshot? input)
    {
        NewLogicFrame?.Invoke(this, new(delta, input));
    }

    private void OnRender(object? sender, double e)
    {
        _cmd.Begin();

        _cmd.UpdateBuffer(_projection, 0, Camera.Perspective);
        _cmd.UpdateBuffer(_view, 0, Camera.View);

        _cmd.SetFramebuffer(_device.SwapchainFramebuffer);

        _cmd.ClearColorTarget(0, _bgColor);
        _cmd.ClearDepthStencil(1);

        _cmd.SetPipeline(_pipeline);

        _cmd.SetGraphicsResourceSet(0, _cameraSet);

        RenderedSceneries = Region.Sceneries.Where(scenery =>
        {
            var section = Region.VisibleSections.FirstOrDefault(section => section.Id == scenery.VisibleSectionId);

            return section is not null && ((section.Id / 100) == 26 || section.IsInPolygon(Camera.Position));
        }).ToList();

        var neighbors = new List<Scenery>();
        foreach (var scenery in RenderedSceneries)
        {
            IEnumerable<Scenery>? neighborsForScenery = Region.VisibleSections.FirstOrDefault(section => section.Id == scenery.VisibleSectionId)?.CanSeeIds
                .Select(id => Region.VisibleSections.FirstOrDefault(section => section.Id == id))
                .Where(section => section is not null && !RenderedSceneries.Any(rendered => rendered.VisibleSectionId == section.Id))
                .SelectMany(section => Region.Sceneries.Where(scenery => scenery.VisibleSectionId == section?.Id));

            if (neighborsForScenery is not null)
                foreach (var neighbor in neighborsForScenery)
                    if (!neighbors.Contains(neighbor))
                        neighbors.Add(neighbor);
        }
        RenderedSceneries.AddRange(neighbors);

        IFrustumCamera? frustum = Camera as IFrustumCamera;
        TotalRendered = 0;

        foreach (var scenery in RenderedSceneries)
            foreach (var objInfo in scenery.ObjectInfos)
            {
                if (objInfo is not VeldridObjectInfo info) continue;
                if (!info.ModelLoaded) continue;

                // Currently skipping global section (26xx)
                if (scenery.VisibleSectionId / 100 == 26) continue;

                IEnumerable<SceneryInstance> instances = scenery.ObjectInstances.Where(instance => instance.Info == objInfo); //&& frustum?.IsOnFrustum(instance) != false);

                // I should fix frustum ¯\_(ツ)_/¯
                // if (frustum is not null && !frustum.IsOnFrustum(instance)) continue;

                //Matrix4x4[] instanceMatrices = instances.Select(instance => instance.InstanceMatrix).ToArray();
                // TODO: Rebuild instance buffer only when something in instances changes, not every frame
                //for (int i = 0; i < instanceMatrices.Length; i++)
                //    _cmd.UpdateBuffer(info.InstancesBuffer, (uint)i * 64, instanceMatrices[i].ToArray());

                _cmd.SetVertexBuffer(0, info.VertexBuffer);
                _cmd.SetVertexBuffer(1, info.InstancesBuffer);

                _cmd.SetIndexBuffer(info.IndexBuffer, IndexFormat.UInt32);

                _cmd.SetGraphicsResourceSet(1, info.MatrixSet);

                var shadings = info.Solid.ShadingGroups.Where(shading => TextureStorage.ContainsKey(info.Solid.TextureHashes[shading.Texture]));

                if (shadings.Any())
                {
                    TotalRendered += info.InstancesCount;
                    foreach (var entry in shadings.Where(entry => entry.Length > 0))
                    {
                        var texture = TextureStorage[info.Solid.TextureHashes[entry.Texture]];

                        _cmd.SetGraphicsResourceSet(2, ((VeldridObjectTexture)texture)?.TextureSet);
                        _cmd.DrawIndexed((uint)entry.Length, (uint)info.InstancesCount, (uint)entry.Offset, 0, 0);
                    }
                }
            }

        //if (frustum is not null)
        //    frustum.DebugDrawFrustum(_cmd);

        _cmd.End();

        _device.SubmitCommands(_cmd);
    }

    private static TextureFormat? TypeToFormat(byte type)
    {
        return type switch
        {
            0x26 => TextureFormat.DXT5,
            0x24 => TextureFormat.DXT3,
            0x22 => TextureFormat.DXT1,
            _ => null
        };
    }

    private SceneryObjectInfo CreateMesh(SolidObject solid, int instances = 1, SceneryInfo? info = null, Region? region = null)
    {
        // Creating every texture associated with solid (if they not yet)
        foreach (var shading in solid.ShadingGroups)
        {
            uint hash = solid.TextureHashes[shading.Texture];

            if (!TextureStorage.ContainsKey(hash))
                CreateTexture(hash, region);
        }

        Vector3 minPoint = new(solid.MinPoint.X, solid.MinPoint.Y, solid.MinPoint.Z);
        Vector3 maxPoint = new(solid.MaxPoint.X, solid.MaxPoint.Y, solid.MaxPoint.Z);

        float radius = Vector3.Distance(minPoint, maxPoint) / 2;

        return new VeldridObjectInfo(_device, _modelLayout, this, solid, instances, info)
        {
            Radius = radius,
            MinPoint = new(solid.MinPoint.X, solid.MinPoint.Y, solid.MinPoint.Z),
            MaxPoint = new(solid.MaxPoint.X, solid.MaxPoint.Y, solid.MaxPoint.Z)
        };
    }

    private int CreateTexture(uint hash, Region? region = null)
    {
        foreach (var pack in (region ?? Region).TexturePacks)
            foreach (var texture in pack)
            {
                if (hash == texture.Name.SpeedHash())
                {
                    TextureFormat? format = TypeToFormat(texture.CompressionType);
                    if (format is null && texture.CompressionType != 8) continue;

                    byte[] data = texture.Data;
                    format ??= TextureFormat.RGBA8;

                    if (texture.CompressionType == 8)
                    {
                        var cached = CacheController.LoadFromCache(texture.Name);

                        if (cached is null)
                        {
                            int size = texture.Width * texture.Height;

                            Debug.WriteLine("Converting and caching palette texture '" + texture.Name + "'...");
                            data = IObjectTexture.ConvertPaletteToRaw(data.Take(size).ToArray(), texture.Palette, 1);
                        }
                        else
                        {
                            data = cached;
                            Debug.WriteLine("Loaded cached palette texture '" + texture.Name + "'");
                        }
                    }

                    var objTexture = new VeldridObjectTexture(_device, _fragmentLayout)
                    {
                        Name = texture.Name,
                        Format = format.Value,
                        Width = (uint)texture.Width,
                        Height = (uint)texture.Height,
                        Mipmaps = texture.Mipmaps,
                        Data = data
                    };

                    if (texture.CompressionType == 8)
                        CacheController.SaveToCache(objTexture);

                    TextureStorage.Add(hash, objTexture);
                    return TextureStorage.Count - 1;
                }
            }
        return -1;
    }

    public void UpdateRegion(Region region)
    {
        UnloadRegion();
        LoadRegion(region);
    }

    private void UnloadRegion()
    {
        if (Region is null) return;

        foreach (var scenery in Region.Sceneries)
            foreach (var objInfo in scenery.ObjectInfos)
                if (objInfo is VeldridObjectInfo info)
                    info.UnloadModel(this);

        foreach (var texture in TextureStorage)
            texture.Value.UnloadTexture();

        TextureStorage.Clear();
    }

    private void LoadRegion(Region newRegion)
    {
        IsLoading = true;

        Task.Run(() =>
        {
            for (int i = 0; i < newRegion.Sceneries.Count; i++)
            {
                var scenery = newRegion.Sceneries[i];
                for (int j = 0; j < scenery.ObjectInfos.Count; j++)
                {
                    IEnumerable<SceneryInstance> instances = scenery.ObjectInstances.Where(instance => instance.Info == scenery.ObjectInfos[j]);

                    if (scenery.ObjectInfos[j] is not VeldridObjectInfo)
                    {
                        var info = scenery.ObjectInfos[j];

                        SolidObject? solid = newRegion.Sections.FirstOrDefault(section => section.Solids.Any(obj => obj.Key == info.SolidMeshKey1))?.Solids.FirstOrDefault(solid => solid.Key == info.SolidMeshKey1);
                        if (solid is null) continue;

                        newRegion.Sceneries[i].ObjectInfos[j] = CreateMesh(solid, instances.Count(), scenery.ObjectInfos[j], newRegion);
                    }

                    var objInfo = (VeldridObjectInfo)scenery.ObjectInfos[j];
                    objInfo.LoadModel(this);

                    // TODO: Remove this code because of needing in implementation of buffer rebuilding after instance/camera changes
                    for (int k = 0; k < instances.Count(); k++)
                        _device.UpdateBuffer(objInfo.InstancesBuffer, (uint)k * 64, instances.ElementAt(k).InstanceMatrix.ToArray());
                }
            }
            IsLoading = false;
            Region = newRegion;
        });

        TotalObjects = newRegion.Sceneries.Select(s => s.ObjectInstances.Count).Sum();
    }

    public VeldridObjectInfo CreateObject(SolidObject solid, int instances = 1, SceneryInfo? info = null)
    {
        return new(_device, _modelLayout, this, solid, instances, info);
    }

    public VeldridObjectTexture CreateTexture(string name, TextureFormat format, uint width, uint height, uint mipmaps, byte[] data)
    {
        return new(_device, _fragmentLayout)
        {
            Name = name,
            Format = format,
            Width = width,
            Height = height,
            Mipmaps = mipmaps,
            Data = data
        };
    }

    /// <summary>
    /// When calling this, <see cref="CommandList.Begin"/> will be already invoked!
    /// </summary>
    /// <returns></returns>
    public CommandList CreateCommandList()
    {
        var list = _device.ResourceFactory.CreateCommandList();
        list.Begin();

        _cmd.SetFramebuffer(_device.SwapchainFramebuffer);
        _cmd.SetPipeline(_pipeline);
        _cmd.SetGraphicsResourceSet(0, _cameraSet);

        return list;
        // Then do whatever you want with this command list...
        // Don't forget to use GraphicsDevice.DisposeWhenIdle(list);
    }
}
