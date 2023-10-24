namespace L4RH.Model.Solids;

[Flags]
public enum SolidFlags : ushort
{
    CompressedVerts      = 1 << 0,
    ShadowMap            = 1 << 3,
    VertexAnimation      = 1 << 4,
    RandomizeStartFrame  = 1 << 5,
    IsLit                = 1 << 6,
    IsWindy              = 1 << 7,
    DuplicateName        = 1 << 8,
    DuplicateNameError   = 1 << 9,
    Duplicated           = 1 << 10,
    WantSpotlightContext = 1 << 11,
    MorphInitialized     = 1 << 12,
    SkinInfoCreated      = 1 << 13,
    PixelDamageCleared   = 1 << 14
}
