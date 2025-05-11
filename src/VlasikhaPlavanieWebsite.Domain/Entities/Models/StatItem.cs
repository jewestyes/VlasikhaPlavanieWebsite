using System.Collections.Generic;

namespace VlasikhaPlavanieWebsite.Models
{
    public class StatItem
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public List<string> Files { get; set; } = new List<string>();
    }
}
