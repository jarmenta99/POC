using AlaskaAir.CmsCrew.CrewEmployeeDB.Models;
using CrewDemographics.Api.GraphQL.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your DbContext as usual
builder.Services.AddDbContext<CrewEmployeeLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureSqlCrewEmployeeLookup")));

// Register the DbContext for GraphQL
builder.Services.AddGraphQLServer()
    //.AddQueryType<CrewQualificationsQuery>()
    .AddQueryType<PersonQuery>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    /*.AddPagingOptions(options =>
    {
        options.DefaultPageSize = 10;
        options.MaxPageSize = 100;
    })*/
    .AddAuthorization(); // Optional: Only if you have HotChocolate.AspNetCore.Authorization

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGraphQL();

await app.RunAsync();