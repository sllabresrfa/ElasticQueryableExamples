using ElasticSearchQueryable.Request.Criteria;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using static LocationFilteringConvention;

public static class LocationExtension
{
    public static bool Within(this Location r, LocationInput input)
    {
        return true;
    }
}

public class Location
{
    public float lat { get; set; }
    public float lon { get; set; }
}

public class GeoDistanceInputType : InputObjectType<LocationInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<LocationInput> descriptor)
    {
    }
}

public class LocationInput : IGeoDistance
{
    public string Distance { get; set; }
    public float Longitude { get; set; }
    public float Latitude { get; set; }
}

public class LocationFilterInput : FilterInputType<Location>
{
    protected override void Configure(IFilterInputTypeDescriptor<Location> descriptor)
    {
        descriptor.Field(f => f.lon).Ignore();
        descriptor.Field(f => f.lat).Ignore();
        descriptor.Operation(CustomOperations.Within).Name("within").Type<GeoDistanceInputType>();
    }
}

public sealed class LocationFilteringConvention : FilterConvention
{
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();
        descriptor.Operation(CustomOperations.Within).Name("within");
        descriptor.BindRuntimeType<Location, LocationFilterInput>();
        descriptor.AddProviderExtension(new QueryableFilterProviderExtension(x =>
            x.AddFieldHandler<LocationOperationHandler>())
        );
    }
    public static class CustomOperations
    {
        public const int Within = 1025;
    }

    public class QueryableFilterProviderExtension
    : FilterProviderExtensions<QueryableFilterContext>
    {
        public QueryableFilterProviderExtension()
        {
        }

        public QueryableFilterProviderExtension(
            Action<IFilterProviderDescriptor<QueryableFilterContext>> configure)
            : base(configure)
        {
        }
    }

    public class LocationOperationHandler : QueryableOperationHandlerBase
    {
        public LocationOperationHandler(InputParser inputParser) : base(inputParser)
        {
        }

        public override bool CanHandle(ITypeCompletionContext context, IFilterInputTypeDefinition typeDefinition,
            IFilterFieldDefinition fieldDefinition)
        {
            return fieldDefinition is FilterOperationFieldDefinition { Id: CustomOperations.Within };
        }

        public override Expression HandleOperation(QueryableFilterContext context, IFilterOperationField field, HotChocolate.Language.IValueNode value, object parsedValue)
        {
            var property = context.GetInstance();            
            var method = typeof(LocationExtension).GetMethod("Within");

            if (parsedValue is LocationInput loc && method != null)
            {
                return Expression.Call(null, method, property, Expression.Constant(loc));
            }

            throw new InvalidOperationException();
        }
    }
}