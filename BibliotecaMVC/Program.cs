using BibliotecaMVC.Database;
using BibliotecaMVC.Repositories.Dapper;
using BibliotecaMVC.Repositories.Interfaces;

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

// Repositórios (Injeção de Dependência)
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<IExemplarRepository, ExemplarRepository>();
builder.Services.AddScoped<IEmprestimoRepository, EmprestimoRepository>();
// Serviços de Domínio e Aplicação
builder.Services.AddScoped<BibliotecaMVC.Services.AuthService>();
builder.Services.AddScoped<BibliotecaMVC.Services.UsuarioService>();
builder.Services.AddScoped<BibliotecaMVC.Services.CategoriaService>();
builder.Services.AddScoped<BibliotecaMVC.Services.LivroService>();
builder.Services.AddScoped<BibliotecaMVC.Services.ExemplarService>();

var app = builder.Build();
var connectionString = app.Configuration.GetConnectionString("DefaultConnection")!; // lê o banco de dados bibliotech.db
var caminhoScript = Path.Combine(app.Environment.ContentRootPath, "Database", "script.sql"); // monta o caminho absoluto do script de criação do banco
DbInitializer.Inicializar(connectionString, caminhoScript); // abre a conexão com o banco

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
