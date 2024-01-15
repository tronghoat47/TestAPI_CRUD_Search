using Microsoft.OpenApi.Models;
using Practice_1.Configurations;
using Practice_1.Models;

var builder = WebApplication.CreateBuilder(args);

//Add service to the container
builder.Services.AddControllers();
builder.Services.AddDbContext<SE1631_DBContext>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "1.0.0",
        Title = "Todo REST API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
});



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = true; // H? tr? t??ng thích ng??c làm vi?c v?i JSON phiên b?n 2.0
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}


app.MapControllers();

app.Run();
