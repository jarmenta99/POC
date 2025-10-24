using AlaskaAir.CmsCrew.CrewEmployeeDB.Models;
using AlaskaAir.CmsCrew.CrewEmployeeDB.Models.Models;

namespace CrewDemographics.Api.GraphQL.Models;

public class PersonQuery
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<person> GetPersons(
        [Service] CrewEmployeeLookupContext dbContext)
        => dbContext.person;

}
