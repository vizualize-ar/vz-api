using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VZ.Shared.Data
{
    public enum ModelType
    {
        unknown = 0,

        [EnumMember(Value = "gltf")]
        gltf = 1,

        [EnumMember(Value = "fbx")]
        fbx = 2,

        [EnumMember(Value = "image")]
        image = 3,

        [EnumMember(Value = "usd")]
        usd = 4,
    }
}
