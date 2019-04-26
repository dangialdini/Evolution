using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using AutoMapper;

namespace Evolution.ProductService {
    public partial class ProductService : CommonService.CommonService {

        #region Construction

        protected IMapper Mapper = null;

        public ProductService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<Brand, BrandModel>();
                cfg.CreateMap<BrandModel, Brand>();
                cfg.CreateMap<BrandModel, BrandModel>();
                cfg.CreateMap<BrandCategory, BrandCategoryModel>();
                cfg.CreateMap<BrandCategoryModel, BrandCategory>();
                cfg.CreateMap<BrandCategoryModel, BrandCategoryModel>();
                cfg.CreateMap<Product, Product>();
                cfg.CreateMap<Product, ProductModel>();
                cfg.CreateMap<ProductModel, Product>();
                cfg.CreateMap<ProductModel, ProductModel>();
                cfg.CreateMap<ProductLocation, ProductLocationModel>();
                cfg.CreateMap<ProductLocationModel, ProductLocation>();
                cfg.CreateMap<ProductMedia, ProductMedia>();
                cfg.CreateMap<ProductMedia, ProductMediaModel>();
                cfg.CreateMap<ProductMediaModel, ProductMedia>();
                cfg.CreateMap<ProductPrice, ProductPriceModel>();
                cfg.CreateMap<ProductPriceModel, ProductPrice>();
                cfg.CreateMap<ProductAdditionalCategoryModel, ProductAdditionalCategory>();
                cfg.CreateMap<ProductAdditionalCategory, ProductAdditionalCategoryModel>();
                cfg.CreateMap<ProductCompliance, ProductComplianceModel>();
                cfg.CreateMap<ProductComplianceModel, ProductCompliance>();
                cfg.CreateMap<ProductComplianceAttachment, ProductComplianceAttachmentModel>();
                cfg.CreateMap<ProductComplianceAttachmentModel, ProductComplianceAttachment>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion
    }
}
