﻿using Speed.Engine.Camera;
using Speed.Engine.Camera.Frustum;
using Speed.Engine.Cache;
using Speed.Engine.Texture;
using Speed.Engine.SceneryObjects;
using Speed.Math;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using L4RH;
using L4RH.Model;
using L4RH.Model.Solids;
using L4RH.Model.Sceneries;
using Veldrid;
using Veldrid.SPIRV;

namespace Speed.Engine.Render;

public sealed class VeldridRenderContext : IRenderContext
{
    public event IRenderContext.LogicEventHandler? NewLogicFrame;
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
    private List<TrackSection> _renderedSections = new();
    public IReadOnlyList<TrackSection> RenderedSections { get => _renderedSections; }

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
        _cameraSet.Dispose();
        _pipeline.Dispose();
        _projection.Dispose();
        _view.Dispose();
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
        NewLogicFrame?.Invoke(this, delta, input);
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

        #region Find Currently Visible Sections

        _renderedSections = Region.Sections.Where(section => section.Visible?.IsInPolygon(Camera.Position) == true || (section.Id / 100) == 26).ToList();

        var neighbors = new List<TrackSection>();
        foreach (var section in RenderedSections)
        {
            if (section.Visible is null)
                continue;

            IEnumerable<TrackSection>? neighborsForSection = section.Visible?.CanSeeIds
                .Select(id => Region.Sections.FirstOrDefault(section => section.Id == id))
                .Where(section => section is not null && !RenderedSections.Any(rendered => rendered.Id == section.Id))
                .Cast<TrackSection>();

            if (neighborsForSection is not null)
                foreach (var neighbor in neighborsForSection)
                    if (!neighbors.Contains(neighbor))
                        neighbors.Add(neighbor);
        }
        _renderedSections.AddRange(neighbors);

        #endregion

        IFrustumCamera? frustum = Camera as IFrustumCamera;
        TotalRendered = 0;

        foreach (var section in RenderedSections)
        {
            if (section.Scenery is null)
                continue;

            foreach (var objInfo in section.Scenery.ObjectInfos)
            {
                if (objInfo is not VeldridObjectInfo info) continue;
                if (!info.ModelLoaded) continue;

                // I should fix frustum ¯\_(ツ)_/¯
                IEnumerable<SceneryInstance> instances = section.Scenery.ObjectInstances.Where(instance => instance.Info == objInfo); //&& frustum?.IsOnFrustum(instance) != false);

                // TODO: Rebuild instance buffer only when something in instances changes, not every frame
                //
                // Matrix4x4[] instanceMatrices = instances.Select(instance => instance.InstanceMatrix).ToArray();
                // for (int i = 0; i < instanceMatrices.Length; i++)
                //     _cmd.UpdateBuffer(info.InstancesBuffer, (uint)i * 64, instanceMatrices[i].ToArray());

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
        }

        _cmd.End();

        _device.SubmitCommands(_cmd);

        _renderedSections.Sort((section1, section2) => section1.Name.CompareTo(section2.Name));
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
                if (hash == Hashes.Bin(texture.Name))
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

    public RegionUpdateStatus UpdateRegion(Region region)
    {
        UnloadRegion();
        return LoadRegion(region);
    }

    private void UnloadRegion()
    {
        if (Region is null) return;

        foreach (var section in Region.Sections)
            if (section.Scenery is not null)
                foreach (var objInfo in section.Scenery.ObjectInfos)
                    if (objInfo is VeldridObjectInfo info)
                        info.UnloadModel(this);

        foreach (var texture in TextureStorage)
            texture.Value.UnloadTexture();

        TextureStorage.Clear();
    }

    private RegionUpdateStatus LoadRegion(Region newRegion)
    {
        IsLoading = true;

        var status = new RegionUpdateStatus();

        Task.Run(() =>
        {
            status.SectionsTotal = newRegion.Sections.Where(section => section.Scenery is not null).Count();

            int sectionCount = 0;
            foreach (var section in newRegion.Sections)
            {
                if (section.Scenery is null) continue;

                status.SectionsLoaded = sectionCount++;
                status.InfoTotal = section.Scenery.ObjectInfos.Count;

                for (int j = 0; j < section.Scenery.ObjectInfos.Count; j++)
                {
                    status.InfoLoaded = j;

                    IEnumerable<SceneryInstance> instances = section.Scenery.ObjectInstances.Where(instance => instance.Info == section.Scenery.ObjectInfos[j]);

                    if (section.Scenery.ObjectInfos[j] is not VeldridObjectInfo)
                    {
                        var info = section.Scenery.ObjectInfos[j];

                        SolidObject? solid = null;
                        foreach (var solidSection in newRegion.Sections)
                        {
                            solid = solidSection.Solids?.FirstOrDefault(solid => solid.Key == info.SolidMeshKey1);

                            if (solid is not null) break;
                        }

                        if (solid is null) continue;

                        section.Scenery.ObjectInfos[j] = CreateMesh(solid, instances.Count(), section.Scenery.ObjectInfos[j], newRegion);
                    }

                    var objInfo = (VeldridObjectInfo)section.Scenery.ObjectInfos[j];
                    objInfo.LoadModel(this);

                    // TODO: Remove this code because of needing in implementation of buffer rebuilding after instance/camera changes
                    for (int k = 0; k < instances.Count(); k++)
                        _device.UpdateBuffer(objInfo.InstancesBuffer, (uint)k * 64, instances.ElementAt(k).InstanceMatrix.ToArray());
                }
            }
            IsLoading = false;
            Region = newRegion;
        });

        TotalObjects = newRegion.Sections.Select(s => s.Scenery?.ObjectInstances.Count ?? 0).Sum();

        return status;
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
