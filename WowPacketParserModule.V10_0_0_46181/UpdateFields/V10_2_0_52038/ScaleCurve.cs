using WowPacketParser.Misc;
using WowPacketParser.Store.Objects.UpdateFields;

// This file is automatically generated, DO NOT EDIT

namespace WowPacketParserModule.V10_0_0_46181.UpdateFields.V10_2_0_52038
{
    public class ScaleCurve : IScaleCurve
    {
        public System.Nullable<uint> StartTimeOffset { get; set; }
        public Vector2[] Points { get; } = new Vector2[2];
        public System.Nullable<uint> ParameterCurve { get; set; }
        public System.Nullable<bool> OverrideActive { get; set; }
    }
}

