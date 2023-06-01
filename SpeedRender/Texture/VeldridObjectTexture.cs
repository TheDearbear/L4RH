using L4RH;
using System.Diagnostics.CodeAnalysis;
using Veldrid;

namespace Speed.Engine.Texture;

public sealed class VeldridObjectTexture : IObjectTexture
{
    public string Name { get; init; } = string.Empty;
    public uint Width { get; init; }
    public uint Height { get; init; }
    public uint Mipmaps { get; init; }
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public TextureFormat Format { get; init; }
    public TextureType Type { get; init; }

    public bool TextureLoaded => _texture is not null;

    public ResourceSet? TextureSet { get; private set; }

    private Veldrid.Texture? _texture;
    private DeviceBuffer? _isPalette;
    private Sampler? _sampler;

    private readonly GraphicsDevice _device;
    private readonly ResourceLayout _fragment;

    public VeldridObjectTexture(GraphicsDevice device, ResourceLayout fragmentLayout)
        => (_device, _fragment) = (device, fragmentLayout);

    public void LoadTexture()
    {
        if (TextureLoaded) return;

        var textureTarget = TypeToVeldrid(Type);
        var pixelFormat = FormatToVeldrid(Format);
        var factory = _device.ResourceFactory;

        _texture = factory.CreateTexture(new(Width, Height, 1, 1, 1, pixelFormat, TextureUsage.Sampled, textureTarget));
        _sampler = factory.CreateSampler(SamplerDescription.Linear);

        _device.UpdateTexture(_texture, Data, 0, 0, 0, Width, Height, 1, 0, 0);

        TextureSet = factory.CreateResourceSet(new(_fragment, _texture, _sampler));
    }

    public void UnloadTexture()
    {
        if (!TextureLoaded) return;

        TextureSet?.Dispose();
        TextureSet = null;

        _texture?.Dispose();
        _texture = null;

        _sampler?.Dispose();
        _sampler = null;

        _isPalette?.Dispose();
        _isPalette = null;
    }

    private static Veldrid.TextureType TypeToVeldrid(TextureType type)
        => type switch
        {
            TextureType.OneDimensional => Veldrid.TextureType.Texture1D,
            _ => Veldrid.TextureType.Texture2D
        };

    private static PixelFormat FormatToVeldrid(TextureFormat format)
        => format switch
        {
            TextureFormat.RGBA8 => PixelFormat.R8_G8_B8_A8_UNorm,
            TextureFormat.DXT1 => PixelFormat.BC1_Rgba_UNorm,
            TextureFormat.DXT3 => PixelFormat.BC2_UNorm,
            TextureFormat.DXT5 => PixelFormat.BC3_UNorm,
            _ => throw new NotImplementedException()
        };

    public bool Equals(IObjectTexture? x, IObjectTexture? y)
        => x?.GetHashCode() == y?.GetHashCode();

    public int GetHashCode([DisallowNull] IObjectTexture obj)
        => (int)Name.SpeedHash();
}
