using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Repositories
{
    public class CategoryRepository : BaseIntRepository<Category>
    {
        public CategoryRepository()
        {
            base.Init("Category", "NameEn,Name,Icon,Color,DisplayOrder,IsActive");
        }

        public void DeleteAll()
        {
            Execute($"DELETE FROM {TableName};");
        }
    }

    public class Category : BaseIntModel
    {
        
        public string NameEn { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<Dimension> Dimensions { get; set; }
    }
   
}
