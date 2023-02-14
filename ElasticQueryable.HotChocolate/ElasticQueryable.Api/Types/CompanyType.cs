namespace ElasticQueryable.Api.Types
{
    public class CompanyType : ObjectType<Company>
    {
        protected override void Configure(IObjectTypeDescriptor<Company> descriptor)
        {
            descriptor
                .Field(f => f.Id)
                .Type<IntType>();

            descriptor
                .Field(f => f.company_name)
                .Type<StringType>();
        }
    }
}
