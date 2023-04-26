using System.Collections.Generic;

namespace NinjaOne.DataExtractUtility
{
    public class Entity
    {
        public string Name { get; set; }
        public string SourceAPIUrl { get; set; }
        public string SharePointListName { get; set; }
        public string ResponseNode { get; set; }
        public List<EntityField> MappingColumns { get; set; }
    }

    public class EntityField
    {
        public string Source { get; set; }
        public string Target { get; set; }
    }
}
