namespace RP6.Format.ResourceDataPack;

public class EResType
{
    public enum Type : uint
    {
        Invalid = 0,
        Mesh = 16,
        MeshFixups = 17, //new
        Skin = 18,
        Model = 24,
        Texture = 32,
        TextureBitmapData = 33,
        TextureMipBitmapData = 34,
        Material = 48,
        Shader = 49,
        Animation = 64,
        AnimationStream = 65,
        AnimationScr = 66,
        AnimationScrFixups = 67,
        ANM2Header = 68,
        ANM2Payload = 69,
        ANM2Fallback = 70,
        AnimGraphBank = 71,
        AnimGraphBankFixups = 72,
        AnimCustomResource = 73,
        AnimCustomResourceFixups = 74,
        GpuFx = 81,
        EnvprobeBin = 85,
        VoxelizerBin = 86, //new
        Area = 90,
        PrefabText = 96,
        Prefab = 97,
        PrefabFixUps = 98,
        Sound = 101,
        Music = 102,
        Speech = 103,
        SFX_stream = 104,
        SFX_local = 105,
        VertexData = 240,
        IndexData = 241,
        GeometryData = 242,
        ClothData = 243, //new
        TinyObjects = 248,
        BuilderInformation = 255
    }

    private readonly static Entry[] Table = new[]
    {
        new Entry("EResType::Invalid", "_INVALID_", "INVALID", "Invalid", Type.Invalid, memCategory: 0, version: 1),
        new Entry("EResType::Mesh", "_MESH_", "MESH", "Mesh", Type.Mesh, memCategory: 84, version: 60),
        new Entry("EResType::MeshFixups", "_MESH_FIXUPS_", "MESH_FIX", "MeshFixups", Type.MeshFixups, memCategory: 84, version: 60),
        new Entry("EResType::Skin", "_SKIN_", "SKIN", "Skin", Type.Skin, memCategory: 85, version: 13),
        new Entry("EResType::Model", "_MODEL_", "MODEL", "Model", Type.Model, memCategory: 83, version: 3),
        new Entry("EResType::Texture", "_TEXTURE_", "TEXTURE", "Texture", Type.Texture, memCategory: 109, version: 11),
        new Entry("EResType::TextureBitmapData", "_TEXTURE_BITMAP_DATA_", "BITMAP", "TextureBitmapData",
            Type.TextureBitmapData, memCategory: 116, version: 11),
        new Entry("EResType::TextureMipBitmapData", "_TEXTURE_MIP_BITMAP_DATA_", "STRMBMP", "TextureMipBitmapData",
            Type.TextureMipBitmapData, memCategory: 116, version: 11),
        new Entry("EResType::Material", "_MATERIAL_", "MATERIAL", "Material", Type.Material, memCategory: 82, version: 13),
        new Entry("EResType::Shader", "_SHADER_", "SHADER", "Shader", Type.Shader, memCategory: 114, version: 13),
        new Entry("EResType::Animation", "_ANIMATION_", "ANIM", "Animation", Type.Animation, memCategory: 5, version: 4),
        new Entry("EResType::AnimationStream", "_ANIMATION_STREAM_", "ANIMSTRM", "AnimationStream",
            Type.AnimationStream, memCategory: 5, version: 4),
        new Entry("EResType::AnimationScr", "_ANIMATION_SCR_", "ANIMSCR", "AnimationScr", Type.AnimationScr, memCategory: 8, version: 4),
        new Entry("EResType::AnimationScrFixups", "_ANIMATION_SCRFIXUPS_", "ANIMSFIX", "AnimationScrFixups",
            Type.AnimationScrFixups, memCategory: 8, version: 4),
        new Entry("EResType::ANM2Header", "_ANM2_METADATA_", "ANM_META", "ANM2Header", Type.ANM2Header, memCategory: 5, version: 2),
        new Entry("EResType::ANM2Payload", "_ANM2_PAYLOAD_", "ANM_DATA", "ANM2Payload", Type.ANM2Payload, memCategory: 5, version: 2),
        new Entry("EResType::ANM2Fallback", "_ANM2_FALLBACK_", "ANM_FLBK", "ANM2Fallback", Type.ANM2Fallback, memCategory: 5, version: 2),
        new Entry("EResType::AnimGraphBank", "_ANIM_GRAPH_BANK_", "ANMGRAPH", "AnimGraphBank", Type.AnimGraphBank, memCategory: 11,
            version: 140),
        new Entry("EResType::AnimGraphBankFixups", "_ANIM_GRAPH_BANK_FIXUPS_", "AGRPHFIX", "AnimGraphBankFixups",
            Type.AnimGraphBankFixups, memCategory: 11, version: 140),
        new Entry("EResType::AnimCustomResource", "_ANIM_CUSTOM_RESOURCE_", "ACSTMRES", "AnimCustomResource",
            Type.AnimCustomResource, memCategory: 14, version: 4),
        new Entry("EResType::AnimCustomResourceFixups", "_ANIM_CUSTOM_RESOURCE_FIXUPS_", "ACRESFIX",
            "AnimCustomResourceFixups", Type.AnimCustomResourceFixups, memCategory: 14, version: 4),
        new Entry("EResType::GpuFx", "_GPUFX_", "GPUFX", "GpuFx", Type.GpuFx, memCategory: 178, version: 2),
        new Entry("EResType::EnvprobeBin", "_ENV_BIN_", "ENV_BIN", "EnvprobeBin", Type.EnvprobeBin, memCategory: 77, version: 2),
        new Entry("EResType::VoxelizerBin", "_VXL_BIN_", "VXL_BIN", "VoxelizerBin", Type.VoxelizerBin, memCategory: 77, version: 2),
        new Entry("EResType::Area", "_AREA_", "AREA", "Area", Type.Area, memCategory: 122, version: 2),
        new Entry("EResType::PrefabText", "_PREFAB_TEXT_", "PRFBTXT", "PrefabText", Type.PrefabText, memCategory: 148, version: 2),
        new Entry("EResType::Prefab", "_PREFAB_", "PREFAB", "Prefab", Type.Prefab, memCategory: 148, version: 8),
        new Entry("EResType::PrefabFixUps", "_PREFAB_DATA_FIXUPS_", "PRFBFXUP", "PrefabFixUps", Type.PrefabFixUps, memCategory: 148,
            version: 8),
        new Entry("EResType::Sound", "_SOUND_", "SOUND", "Sound", Type.Sound, memCategory: 18, version: 2),
        new Entry("EResType::Music", "_SOUND_MUSIC_", "MUSIC", "Music", Type.Music, memCategory: 18, version: 2),
        new Entry("EResType::Speech", "_SOUND_SPEECH_", "SPEECH", "Speech", Type.Speech, memCategory: 18, version: 2),
        new Entry("EResType::SFX_stream", "_SOUND_STREAM_", "SNDSTRM", "SFX_stream", Type.SFX_stream, memCategory: 18, version: 2),
        new Entry("EResType::SFX_local", "_SOUND_LOCAL_", "SNDLOCAL", "SFX_local", Type.SFX_local, memCategory: 18, version: 2),
        new Entry("EResType::VertexData", "_VERTEX_DATA_", "VERTEXES", "VertexData", Type.VertexData, memCategory: 115, version: 5),
        new Entry("EResType::IndexData", "_INDEX_DATA_", "INDEXES", "IndexData", Type.IndexData, memCategory: 115, version: 4),
        new Entry("EResType::GeometryData", "_GEOMETRY_DATA_", "GEOMETRY", "GeometryData", Type.GeometryData, memCategory: 115, version: 4),
        new Entry("EResType::ClothData", "_CLOTH_DATA_", "CLOTH", "ClothData", Type.ClothData, memCategory: 115, version: 2),
        new Entry("EResType::TinyObjects", "_TINY_OBJECTS_", "TINYOBJS", "TinyObjects", Type.TinyObjects, memCategory: 75, version: 8),
        new Entry("EResType::BuilderInformation", "_BUILDER_INFORMATION_", "BUILDER", "BuilderInformation",
            Type.BuilderInformation, memCategory: 107, version: 2)
    };

    public static Type FromInt(int param)
    {
        var id = (uint)param;
        foreach (var e in Table)
        {
            if (e.Id != (Type)id)
                continue;

            if (string.Equals(e.Name, "_INVALID_", StringComparison.Ordinal))
                return Type.Invalid;

            return e.Id;
        }

        return Type.Invalid;
    }

    public static Type GetByName(string name)
    {
        return string.IsNullOrEmpty(name)
            ? Type.Invalid
            : (from e in Table where string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase) select e.Id)
            .FirstOrDefault();
    }

    public static string GetName(Type t)
    {
        foreach (var e in Table)
            if (e.Id == t)
                return e.Name;

        return "_INVALID_";
    }

    public static string GetPrettyName(Type t)
    {
        foreach (var e in Table)
            if (e.Id == t)
                return e.PrettyName;

        return "_INVALID_";
    }

    private readonly struct Entry(
        string name,
        string longName,
        string shortName,
        string prettyName,
        Type id,
        ushort memCategory,
        ushort version)
    {
        public string Name { get; } = name;
        public string LongName { get; } = longName;
        public string ShortName { get; } = shortName;
        public string PrettyName { get; } = prettyName;
        public Type Id { get; } = id;
        public ushort MemCategory { get; } = memCategory;
        public ushort Version { get; } = version;
    }
}