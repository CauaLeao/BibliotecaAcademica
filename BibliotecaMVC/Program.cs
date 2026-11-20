using BibliotecaMVC.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDistributedMemoryCache(); // guarda a sessão atual do usuário na RAM do servidor
builder.Services.AddSession(options => // adidiona o mecanismo de sessão por usuário
{
   options.IdleTimeout = TimeSpan.FromMinutes(30); // desconecta usuários inativos por mais de 30 minutos
   options.Cookie.HttpOnly = true; // impede códigos javascript maliciosos de lerem o cookie de sessão do usuário
   options.Cookie.IsEssential = true; // torna o cookie obrigatório pro sistema rodar
});
builder.Services.AddControllersWithViews();

var app = builder.Build();
var connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
var caminhoScript = Path.Combine(app.Environment.ContentRootPath, "Database", "script.sql");
DbInitializer.Inicializar(connectionString, caminhoScript);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession(); // middleware que lê o cookie da sessão e grava as alterações no servidor

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
