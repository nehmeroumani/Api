using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Repositories
{
    public class DimensionRepository : BaseIntRepository<Dimension>
    {
        public DimensionRepository()
        {
            base.Init("Dimension", "NameEn,Name,Icon,Color,DisplayOrder,CategoryId,IsActive");
        }

        public void DeleteAll()
        {
            Execute($"DELETE FROM {TableName};");
        }
    }

    public class Dimension : BaseIntModel
    {
        public string NameEn { get; set; }

        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int DisplayOrder { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }

    }

}
