using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VZ.Shared.Data
{
    public class ProductModel
    {
        /// <summary>
        /// Azure storage container relative path
        /// </summary>
        public string fullpath { get; set; }

        /// <summary>
        /// Azure storage container relative path
        /// </summary>
        public string thumbpath { get; set; }
        public ModelType modelType { get; set; }
    }

    public class ProductImage
    {
        /// <summary>
        /// Azure storage container relative path
        /// </summary>
        public string fullpath { get; set; }

        /// <summary>
        /// Azure storage container relative path
        /// </summary>
        public string thumbpath { get; set; }
    }

    public enum SizeUnit
    {
        [EnumMember(Value = "in")]
        Inches,

        [EnumMember(Value = "cm")]
        Centimeters
    }

    public enum ModelDirection
    {
        unknown = 0,

        [EnumMember(Value = "horizontal")]
        horizontal = 1,

        [EnumMember(Value = "vertical")]
        vertical = 2
    }
}
