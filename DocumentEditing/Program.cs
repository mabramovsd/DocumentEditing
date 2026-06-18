using DocumentEditing.Libs;
using DocumentEditing.Repositories;
using DocumentEditing.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "DocumentEditing",

        ValidateAudience = true,
        ValidAudience = "DocumentEditing",

        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_1234567890987654321")),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.Configure<DirectorySettings>(builder.Configuration.GetSection("Directories"));
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IAuditService, AuditService>();
//Keyed because two implementations of one interface
builder.Services.AddKeyedScoped<IDocumentFileSystemService, DocumentFileSystemService>(DependencyKeys.DocumentsService);
builder.Services.AddKeyedScoped<IDocumentFileSystemService, AuditFileSystemService>(DependencyKeys.AuditService);
builder.Services.AddSingleton<IDocumentSessionRepository, InMemoryDocumentSessionRepository>();
builder.Services.AddSingleton<IDocumentSessionService, DocumentSessionService>();
builder.Services.AddSingleton<DocumentLockService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<DocumentHub>("/chat");   // ChatHub будет обрабатывать запросы по пути /chat
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
//сделать возможность одновременного редактирования какого-нибудь документа1. Ну и сделать там лок, критическая секция и т.д.
