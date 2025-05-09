using BadApi.Account;
using BadApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace BadApi.BrokenAccessControl;

public static class Endpoints
{
    public static void SetupAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("purchase", policy => policy.RequireClaim("role", "purchase", "admin"));
            options.AddPolicy("admin", policy => policy.RequireClaim("role", "admin"));
        });
    }

    public static void Register(WebApplication app)
    {
        var group = app.MapGroup("invoice")
            .RequireAuthorization();
        
        group.MapGet("/v1", (Delegate)List);
        group.MapGet("/v2", (Delegate)List)
            .RequireAuthorization("purchase");
        group.MapGet("/v3", (Delegate)FilteredList)
            .RequireAuthorization("purchase");
        
        group.MapGet("v1/{id}", (Delegate)Invoice)
            .RequireAuthorization("purchase");
        group.MapGet("v2/{id}", (Delegate)InvoiceFiltered)
            .RequireAuthorization("purchase");
    }
    
    public static async Task<IResult> List([FromServices]Database db)
    {
        var model = db.Invoices.List();
        return Results.Ok(model);
    }
    
    public static async Task<IResult> FilteredList(HttpContext context, [FromServices]Database db)
    {
        var user = context.GetUser();
        var model = db.Invoices.List(user.Id);
        return Results.Ok(model);
    }
    
    public static async Task<IResult> Invoice(string id, [FromServices]Database db)
    {
        var model = db.Invoices.FindByNumber(id);
        return model != null ? Results.Ok(model) : Results.NotFound();
    }
    
    public static async Task<IResult> InvoiceFiltered(string id, HttpContext context, [FromServices]Database db)
    {
        var user = context.GetUser();
        var model = db.Invoices.FindByNumber(id, user.Id);
        return model != null ? Results.Ok(model) : Results.NotFound();
    }
}