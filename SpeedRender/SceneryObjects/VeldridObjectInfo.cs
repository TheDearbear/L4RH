using L4RH;
using L4RH.Model.Sceneries;
using L4RH.Model.Solids;
using Speed.Engine.Render;
using Speed.Engine.Texture;
using Veldrid;

namespace Speed.Engine.SceneryObjects;

public sealed class VeldridObjectInfo : SceneryObjectInfo
{
    public DeviceBuffer? VertexBuffer { get; private set; }
    public DeviceBuffer? IndexBuffer { get; private set; }
    public DeviceBuffer InstancesBuffer { get; private set; }
    public DeviceBuffer SolidBuffer { get; private set; }
    public ResourceSet MatrixSet { get; private set; }

    private int _instancesCount;
    public override int InstancesCount
    {
        get => _instancesCount;
        set
        {
            if (_instancesCount == value) return;

            _instancesCount = value;
            InstancesBuffer.Dispose();
            InstancesBuffer = _device.ResourceFactory.CreateBuffer(new((uint)(64 * _instancesCount), BufferUsage.VertexBuffer));
        }
    }

    private readonly VeldridRenderContext? _context;
    private readonly GraphicsDevice _device;

    internal VeldridObjectInfo(GraphicsDevice device, ResourceLayout layout, VeldridRenderContext ctx, SolidObject solid, int instances, SceneryInfo? info = null)
        : this(device, layout, solid, instances, info) => _context = ctx;

    public VeldridObjectInfo(GraphicsDevice device, ResourceLayout layout, SolidObject solid, int instances, SceneryInfo? info = null)
        : base(solid, info)
    {
        _device = device;
        _instancesCount = instances;

        InstancesBuffer = _device.ResourceFactory.CreateBuffer(new((uint)(64 * instances), BufferUsage.VertexBuffer));
        SolidBuffer = _device.ResourceFactory.CreateBuffer(new(64, BufferUsage.UniformBuffer));

        var matrix = Solid.Matrix.ToArray();
        matrix[12] = matrix[13] = matrix[14] = 0;
        _device.UpdateBuffer(SolidBuffer, 0, matrix);

        MatrixSet = _device.ResourceFactory.CreateResourceSet(new(layout, SolidBuffer));
    }

    public override void LoadModel(IRenderContext context)
    {
        if (ModelLoaded) return;

        if (Solid.VerticesNumber < 1) return;

        float[] vertices = Solid.Vertices.SelectMany(v => v.ToArray()).ToArray();
        uint[] indices = Solid.Indices.Select<ushort, uint>(x => x).ToArray();

        VertexBuffer = _device.ResourceFactory.CreateBuffer(new((uint)(vertices.Length * sizeof(float)), BufferUsage.VertexBuffer));
        IndexBuffer = _device.ResourceFactory.CreateBuffer(new((uint)(indices.Length * sizeof(uint)), BufferUsage.IndexBuffer));

        _device.UpdateBuffer(VertexBuffer, 0, vertices);
        _device.UpdateBuffer(IndexBuffer, 0, indices);

        if (Solid.ShadingGroups.Any() && _context is not null)
        {
            for (int i = 0; i < Solid.ShadingGroups.Count; i++)
            {
                var entry = Solid.ShadingGroups[i];
                uint textureHash = Solid.TextureHashes[entry.Texture];

                if (!_context.TextureStorage.TryGetValue(textureHash, out IObjectTexture? texture))
                {
                    // System.Diagnostics.Debug.WriteLine($"Error (Texture {i} on solid '{Solid.Name}'): No texture! (Hash: 0x{textureHash:X8})");
                    continue;
                }

                texture.LoadTexture();
            }
        }

        ModelLoaded = true;
    }

    public override void UnloadModel(IRenderContext context)
    {
        ModelLoaded = false;

        VertexBuffer?.Dispose();
        VertexBuffer = null;

        IndexBuffer?.Dispose();
        IndexBuffer = null;
    }
}
