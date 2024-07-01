using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Data
{
    public enum ReviewType
    {
        /// <summary>
        /// Review of a business
        /// </summary>
        Business = 0,

        /// <summary>
        /// Review of a specific business's product
        /// </summary>
        BusinessProduct = 1,

        ///// <summary>
        ///// Review of any product, not directly tied to a business
        ///// </summary>
        //Product = 2
    }
}
