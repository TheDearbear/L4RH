using System;

namespace L4RH.Model.Sceneries;

[Flags]
public enum InstanceFlags : ushort
{
    ExcludeFlagSplitScreen      = 1 << 0,
    ExcludeFlagMainView         = 1 << 1,
    ExcludeFlagRacing           = 1 << 2,
    ExcludeFlagDisableRendering = 1 << 3,
    ExcludeFlagGroupDisable     = 1 << 4,
    EnableRearView              = 1 << 5,
    EnableReflection            = 1 << 6,
    EnvmapShadow                = 1 << 7,
    IdentityMatrix              = 1 << 8,
    FlipOnBackwardsTrack        = 1 << 9,
    ChoppedRoadway              = 1 << 10,
    Reflection                  = 1 << 11,
    EnvironmentMap              = 1 << 12,
    Swayable                    = 1 << 13,
    EnableWind                  = 1 << 14,
    AlwaysFacing                = 1 << 15
}
