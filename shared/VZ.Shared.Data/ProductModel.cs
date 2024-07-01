using System;
using System.Collections.Generic;
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
}
