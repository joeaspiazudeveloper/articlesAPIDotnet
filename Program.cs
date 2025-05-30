using ArticlesNewsApi.Data;
using ArticlesNewsApi.Models; // ¡Importante! Usar el namespace correcto de tu modelo
using ArticlesNewsApi.Repositories;
using ArticlesNewsApi.Services;
using Microsoft.EntityFrameworkCore; // ¡Importante! Usar el namespace correcto de tu repositorio

var builder = WebApplication.CreateBuilder(args);

// --- Configuración de Inyección de Dependencias (DI) ---
// 1. Register a DbContext for Sqlite
builder.Services.AddDbContext<NewsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Register the ArticleRepository (one instance per HTTP request)
builder.Services.AddScoped<IArticleRepository, EfCoreArticleRepository>();

// 3. Register the ArticleService (one instance per HTTP request)
builder.Services.AddScoped<ArticleService>();

// Opcional: Agregar Swagger/OpenAPI para la documentación del API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Configuración del Middleware HTTP (orden es importante) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // migrate database
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
        db.Database.Migrate();
    }
}

app.UseHttpsRedirection();

// Define endpoints for the API

// GET /articles - Obtener todos los artículos
app.MapGet("/articles", async (ArticleService service) =>
{
    var articles = await service.GetArticlesAsync(); // <-- Ahora llama al método correcto GetArticlesAsync
    return Results.Ok(articles);
})
.WithName("GetAllArticles")
.WithOpenApi();

// GET /articles/{id} - Obtener un artículo por ID
app.MapGet("/articles/{id:int}", async (int id, ArticleService service) =>
{
    var article = await service.GetArticleByIdAsync(id);
    return article != null ? Results.Ok(article) : Results.NotFound();
})
.WithName("GetArticleById")
.WithOpenApi();

// POST /articles - Crear un nuevo artículo
app.MapPost("/articles", async (Article article, ArticleService service) =>
{
    if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Description))
    {
        return Results.BadRequest("El título y la descripción no pueden estar vacíos.");
    }

    article.Id = 0; // Asegurarse de que el ID es 0 y será asignado por el repositorio
    article.PublishedDate = DateTime.UtcNow;

    await service.AddArticleAsync(article);
    return Results.Created($"/articles/{article.Id}", article);
})
.WithName("CreateArticle")
.WithOpenApi();

// PUT /articles/{id} - Actualizar un artículo existente
app.MapPut("/articles/{id:int}", async (int id, Article article, ArticleService service) =>
{
    if (id != article.Id)
    {
        return Results.BadRequest("El ID en la URL no coincide con el ID del cuerpo.");
    }

    var existingArticle = await service.GetArticleByIdAsync(id);
    if (existingArticle == null)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Description))
    {
        return Results.BadRequest("El título y la descripción no pueden estar vacíos para la actualización.");
    }

    await service.UpdateArticleAsync(article);
    return Results.NoContent();
})
.WithName("UpdateArticle")
.WithOpenApi();

// DELETE /articles/{id} - Eliminar un artículo
app.MapDelete("/articles/{id:int}", async (int id, ArticleService service) =>
{
    var existingArticle = await service.GetArticleByIdAsync(id);
    if (existingArticle == null)
    {
        return Results.NotFound();
    }

    await service.DeleteArticleAsync(id);
    return Results.NoContent();
})
.WithName("DeleteArticle")
.WithOpenApi();

app.Run();