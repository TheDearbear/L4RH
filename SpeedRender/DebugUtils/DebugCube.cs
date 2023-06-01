using L4RH;
using L4RH.Model.Solids;
using Speed.Engine.Render;
using Speed.Engine.SceneryObjects;
using Speed.Engine.Texture;
using System.Numerics;
using Veldrid;

namespace Speed.Engine.DebugUtils;

public class DebugCube
{
    static VeldridObjectInfo _object = null!;
    static VeldridRenderContext _context = null!;
    static GraphicsDevice _device = null!;
    static VeldridObjectTexture _texture = null!;

    public DebugCube(GraphicsDevice device, VeldridRenderContext ctx)
    {
        _context = ctx;
        _device = device;

        SolidObject solid = new()
        {
            VerticesNumber = 8,
            MinPoint = new Vector3(-1, -1, -1),
            MaxPoint = new Vector3(1, 1, 1),
            Name = "__SPEEDRENDER_DEBUG_CUBE",
            Matrix = Matrix4x4.Identity,
            Indices = new ushort[]
            {
                5, 3, 1,
                3, 8, 4,
                7, 6, 8,
                2, 8, 6,
                1, 4, 2,
                5, 2, 6,
                5, 7, 3,
                3, 7, 8,
                7, 5, 6,
                2, 4, 8,
                1, 3, 4,
                5, 1, 2
            },
            Vertices = new()
            {
                new( 1,  1, -1),
                new( 1, -1, -1),
                new( 1,  1,  1),
                new( 1, -1,  1),
                new(-1,  1, -1),
                new(-1, -1, -1),
                new(-1,  1,  1),
                new(-1, -1,  1)
            }
        };

        foreach (var vertice in solid.Vertices)
            vertice.SetColor(0xFFFF0000u);

        _object = _context.CreateObject(solid, 6, new() { Name = "__SpeedRender_Debug_Cube",  });
        _object.LoadModel(_context);

        _texture = _context.CreateTexture("__SPEEDRENDER_DEBUG_TEXTURE", TextureFormat.RGBA8, 1, 1, 0, new byte[] { 255, 0, 0, 255 });
        _texture.LoadTexture();
    }

    public static void Draw(Vector3 position, CommandList list)
    {
        _object.Solid.Matrix = Matrix4x4.CreateTranslation(position);
        _device.UpdateBuffer(_object.SolidBuffer, 0, _object.Solid.Matrix.ToArray());

        //var list = _context.CreateCommandList();

        list.SetVertexBuffer(0, _object.VertexBuffer);
        list.SetIndexBuffer(_object.IndexBuffer, IndexFormat.UInt32);
        list.SetGraphicsResourceSet(1, _object.MatrixSet);
        list.SetGraphicsResourceSet(2, _texture.TextureSet);
        list.DrawIndexed((uint)_object.Solid.Indices.Length);

        //list.End();
        //_device.SubmitCommands(list);
        //_device.DisposeWhenIdle(list);
    }
}
