using System.Diagnostics;
using System.Numerics;

namespace L4RH.Model.Sceneries;

[DebuggerDisplay("Instance of {Info.Name}")]
public class SceneryInstance
{
    public Vector3 BoundBoxMin { get; set; }
    public Vector3 BoundBoxMax { get; set; }

    public ushort SceneryInfo { get; set; }
    public int PreCullerInfo { get; set; }

    public InstanceFlags Flags { get; set; }

    public Vector3 Position
    {
        get => InstanceMatrix.Translation;
        set
        {
            var matrix = InstanceMatrix;
            matrix.Translation = value;
            InstanceMatrix = matrix;
        }
    }

    public Matrix4x4 InstanceMatrix { get; set; } = Matrix4x4.Identity;


    public SceneryInfo Info => Scenery.ObjectInfos[SceneryInfo];

    private readonly Scenery Scenery;

    public SceneryInstance(Scenery scenery)
        => Scenery = scenery;
}
