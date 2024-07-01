using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace VZ.Shared.Data
{
    public class VzDataContext : DbContext
    {
#if DEBUG
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder 
            => { builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Debug); });
#endif

        public DbSet<BusinessProduct> BusinessProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(Config.DB.PostgresConnection);
#if DEBUG
            optionsBuilder
                .UseLoggerFactory(MyLoggerFactory)
                .EnableDetailedErrors();
#endif
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BusinessProduct>().ToTable("business_products");
            builder.Entity<BusinessProduct>().Property(x => x.BusinessId).HasColumnName("business_id");
            builder.Entity<BusinessProduct>().Property(x => x.CreatedOn).HasColumnName("created_on");
            builder.Entity<BusinessProduct>().Property(x => x.ExternalProductIds).HasColumnName("external_product_ids");
            builder.Entity<BusinessProduct>().Property(x => x.Height).HasColumnName("height");
            builder.Entity<BusinessProduct>().Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
            builder.Entity<BusinessProduct>().Property(x => x.Image).HasColumnName("image").HasColumnType("jsonb");
            builder.Entity<BusinessProduct>().Property(x => x.ModelDirection).HasColumnName("model_direction");
            builder.Entity<BusinessProduct>().Property(x => x.Models).HasColumnName("models").HasColumnType("jsonb");
            builder.Entity<BusinessProduct>().Property(x => x.Name).HasColumnName("name");
            builder.Entity<BusinessProduct>().Property(x => x.Rating).HasColumnName("rating");
            builder.Entity<BusinessProduct>().Property(x => x.Sku).HasColumnName("sku");
            builder.Entity<BusinessProduct>().Property(x => x.Unit).HasColumnName("unit");
            builder.Entity<BusinessProduct>().Property(x => x.Upc).HasColumnName("upc");
            builder.Entity<BusinessProduct>().Property(x => x.UpdatedOn).HasColumnName("updated_on");
            builder.Entity<BusinessProduct>().Property(x => x.Uuid).HasColumnName("uuid");
            builder.Entity<BusinessProduct>().Property(x => x.Width).HasColumnName("width");
            builder.Entity<BusinessProduct>().Property(x => x.NftMetaCid).HasColumnName("nft_meta_cid");
            builder.Entity<BusinessProduct>().Property(x => x.NftMinter).HasColumnName("nft_minter");
            builder.Entity<BusinessProduct>().Property(x => x.NftSignature).HasColumnName("nft_signature");
            builder.Entity<BusinessProduct>().Property(x => x.NftTokenId).HasColumnName("nft_token_id");
            builder.Entity<BusinessProduct>().Property(x => x.NftTransaction).HasColumnName("nft_transaction");
            builder.Entity<BusinessProduct>().Property(x => x.NftTransactionStatus).HasColumnName("nft_transaction_status");
        }
    }
}
