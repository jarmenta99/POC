using AlaskaAir.CmsCrew.CrewEmployeeDB.Models;
using AlaskaAir.CmsCrew.CrewEmployeeDB.Models.Models;

namespace CrewDemographics.Api.GraphQL.Models;

public class CrewQualificationsQuery
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<crew_qualifications> GetCrewQualifications(
        [Service] CrewEmployeeLookupContext dbContext)
        => dbContext.crew_qualifications;
}