using ElasticSearchQueryable;
using Nest;

namespace ElasticQueryable.Api.Types
{
    public class Company
    {
        public int Id { get; set; }

        [Text(Name = "company_name")]
        [KeywordField("company_name.raw")]
        [Prefix("company_name.raw")]
        public string? company_name { get; set; }
        public Location? location { get; set; }
    }
}
