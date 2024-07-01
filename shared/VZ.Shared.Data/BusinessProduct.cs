using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using VZ.Shared.Data;
using VZ.Shared.Models;

namespace VZ.Shared.Data
{
    public class BusinessProduct
    {
        /// <summary>
        /// Initialize with the businessId, which is also the Cosmos DB collection partition key
        /// </summary>
        /// <param name="businessId"></param>
        //public BusinessProduct(string businessId) : base(businessId)
        //{
        //    this.businessId = businessId;
        //}

        public BusinessProduct()
        {
            Uuid = Guid.NewGuid();
            ExternalProductIds = new List<string>();
        }

        public long Id { get; set; }
        public long BusinessId { get; set; }

        public Guid Uuid { get; set; }

        ///// <summary>
        ///// Business's product ID
        ///// </summary>
        //public string externalProductId { get; set; }
        /// <summary>
        /// Business's product ID
        /// </summary>
        public List<string> ExternalProductIds { get; set; }

        public string Name { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }
        public decimal Rating { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public SizeUnit? Unit { get; set; }
        public ProductImage Image { get; set; }
        public List<ProductModel> Models { get; set; }
        public ModelDirection ModelDirection { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string NftMetaCid { get; set; }
        public string NftMinter { get; set; }
        public string NftSignature { get; set; }
        public string NftTokenId { get; set; }
        public string NftTransaction { get; set; }
        public string NftTransactionStatus { get; set; }
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
