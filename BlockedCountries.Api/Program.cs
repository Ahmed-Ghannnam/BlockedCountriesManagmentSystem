using BlockedCountries.BL.Managers;
using BlockedCountries.BL.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Default
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
    });
});

#endregion

#region Service

builder.Services.AddSingleton<BlockedCountryService>();
builder.Services.AddSingleton<GeoLocationService>();
builder.Services.AddSingleton<LogService>();
builder.Services.AddHttpClient<GeoLocationService>();

builder.Services.AddHostedService<TemporalBlockCleanupService>();
#endregion

#region Rate Limiting

var fixedPolicy = "fixed";

builder.Services.AddRateLimiter(Options =>
{
    Options.AddFixedWindowLimiter(policyName: fixedPolicy, options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });

    Options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync($"Too many requests. Please try again later .", cancellationToken);

    };
});

#endregion

//builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRateLimiter();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers().RequireRateLimiting(fixedPolicy); // will automatically run the policy against all controllers

app.Run();
