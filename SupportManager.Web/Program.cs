using System.Net.Mail;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Console;
using HtmlTags;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SupportManager.Contracts;
using SupportManager.Control;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.ApiKey;
using SupportManager.Web.Infrastructure.Tags;
using SupportManager.Web.Mailers;

var webApplicationOptions = new WebApplicationOptions()
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
    ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
};
var builder = WebApplication.CreateBuilder(webApplicationOptions);

builder.Host.UseWindowsService();

// Add services to the container.
builder.Services
    .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate()
    .AddApiKeyAuthentication();

builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });

builder.Services.AddAutoMapper(typeof(Program))
    .AddMediatR(typeof(Program))
    .AddScoped(_ => new SupportManagerContext(builder.Configuration["Connections:SupportManager"]))
    .AddHtmlTags(new SupportManagerHtmlConventions())
    .AddHangfire(hangfire =>
    {
        hangfire.UseSqlServerStorage(builder.Configuration["Connections:hangfire"]);
        hangfire.UseConsole();
    })
    .AddTransient<IActionContextAccessor, ActionContextAccessor>()
    .AddScoped<IForwarder, Forwarder>()
    .AddScoped<SupportManager.Control.IPublisher, Publisher>()
    .AddScoped<TeamMemberFilter>()
    .AddTransient<IClaimsTransformation, ClaimsTransformation>()
    .AddTransient<UserMailer>()
    .AddTransient<SmtpClient>(_ => throw new NotImplementedException());


builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/User");
    options.Conventions.Add(new TeamIdPageRouteModelConvention());
}).AddMvcOptions(options =>
{
    options.Filters.Add<AdminAreaAccessFilter>();
    options.Filters.Add<TeamMemberFilter>();
    options.Filters.Add<DbContextTransactionFilter>();
    options.Filters.Add<ValidatorActionFilter>();
    options.ModelBinderProviders.Insert(0, new EntityModelBinderProvider());
    options.ModelBinderProviders.Insert(0, new LoggedInUserModelBinderProvider());
}).AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Program>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard(options: new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.MapRazorPages();
app.MapControllers();

app.Run();