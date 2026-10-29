using CourseRegistration.Api.Configuration;
using CourseRegistration.Api.Interfaces;
using CourseRegistration.Api.Repositories;
using CourseRegistration.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<CourseOptions>(builder.Configuration.GetSection(CourseOptions.SectionName));
builder.Services.AddSingleton<ICourseRegistrationRepository, InMemoryCourseRegistrationRepository>();
builder.Services.AddSingleton<ICourseRegistrationService, CourseRegistrationService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();
app.MapControllers();
app.Run();

public partial class Program { }
