using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ReportDesigner.Blazor.Common.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<DesignerOptionService>();
builder.Services.AddScoped<DesignerCSSService>();
builder.Services.AddScoped<ControlCreationService>();
builder.Services.AddScoped<ControlModificationServcie>();
builder.Services.AddScoped<SelectedControlService>();
builder.Services.AddScoped<DragAndDropService>();
builder.Services.AddScoped<MultiLanguageService>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
