using AlaskaAir.CmsCrew.CrewEmployeeDB.Models;
using AlaskaAir.CmsCrew.CrewEmployeeDB.Models.Models;
using CrewDemographics.Api.GraphQL.Models; // Add this
using Microsoft.AspNetCore.Mvc;

namespace CrewDemographics.Api.GraphQL.Controllers;

[ApiController]
[Route("[controller]")]
public class CrewQualificationsController : ControllerBase
{
    private readonly CrewEmployeeLookupContext _dbContext;
    private readonly CrewQualificationsQuery _query;

    public CrewQualificationsController(CrewEmployeeLookupContext dbContext)
    {
        _dbContext = dbContext;
        _query = new CrewQualificationsQuery();
    }

    [HttpGet]
    public ActionResult<IEnumerable<crew_qualifications>> Get()
    {
        // Directly call the query method, passing the dbContext
        var qualifications = _query.GetCrewQualifications(_dbContext).ToList();
        return Ok(qualifications);
    }
}