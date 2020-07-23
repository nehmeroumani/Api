using System;

namespace Core.Repositories
{
    public class LevelOfConfidenceRepository : BaseIntRepository<LevelOfConfidence>
    {
        public LevelOfConfidenceRepository()
        {
            base.Init("LevelOfConfidence", "Value,IsActive,NameEn,Name,Icon,Color,DisplayOrder");
        }
    }

    public class LevelOfConfidence : BaseIntModel
    {
        public string NameEn { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int DisplayOrder { get; set; }
        public int Value { get; set; }
        public bool IsActive { get; set; }
        
    }
    
}
