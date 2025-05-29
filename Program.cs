using ArticlesNewsApi.Models; // ¡Importante! Usar el namespace correcto de tu modelo
using ArticlesNewsApi.Repositories; // ¡Importante! Usar el namespace correcto de tu repositorio

var builder = WebApplication.CreateBuilder(args);

// --- Configuración de Inyección de Dependencias (DI) ---
builder.Services.AddSingleton<IArticleRepository, InMemoryArticleRepository>();

// Opcional: Agregar Swagger/OpenAPI para la documentación del API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Configuración del Middleware HTTP (orden es importante) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Definición de los Endpoints del Minimal API (CRUD para Articles) ---

// GET /articles - Obtener todos los artículos
app.MapGet("/articles", async (IArticleRepository repo) =>
{
    var articles = await repo.GetArticlesAsync(); // <-- Ahora llama al método correcto GetArticlesAsync
    return Results.Ok(articles);
})
.WithName("GetAllArticles")
.WithOpenApi();

// GET /articles/{id} - Obtener un artículo por ID
app.MapGet("/articles/{id:int}", async (int id, IArticleRepository repo) =>
{
    var article = await repo.GetArticleByIdAsync(id);
    return article != null ? Results.Ok(article) : Results.NotFound();
})
.WithName("GetArticleById")
.WithOpenApi();

// POST /articles - Crear un nuevo artículo
app.MapPost("/articles", async (Article article, IArticleRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Description))
    {
        return Results.BadRequest("El título y la descripción no pueden estar vacíos.");
    }

    article.Id = 0; // Asegurarse de que el ID es 0 y será asignado por el repositorio
    article.PublishedDate = DateTime.UtcNow;

    await repo.AddArticleAsync(article);
    return Results.Created($"/articles/{article.Id}", article);
})
.WithName("CreateArticle")
.WithOpenApi();

// PUT /articles/{id} - Actualizar un artículo existente
app.MapPut("/articles/{id:int}", async (int id, Article article, IArticleRepository repo) =>
{
    if (id != article.Id)
    {
        return Results.BadRequest("El ID en la URL no coincide con el ID del cuerpo.");
    }

    var existingArticle = await repo.GetArticleByIdAsync(id);
    if (existingArticle == null)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.Description))
    {
        return Results.BadRequest("El título y la descripción no pueden estar vacíos para la actualización.");
    }

    await repo.UpdateArticleAsync(article);
    return Results.NoContent();
})
.WithName("UpdateArticle")
.WithOpenApi();

// DELETE /articles/{id} - Eliminar un artículo
app.MapDelete("/articles/{id:int}", async (int id, IArticleRepository repo) =>
{
    var existingArticle = await repo.GetArticleByIdAsync(id);
    if (existingArticle == null)
    {
        return Results.NotFound();
    }

    await repo.DeleteArticleAsync(id);
    return Results.NoContent();
})
.WithName("DeleteArticle")
.WithOpenApi();

app.Run();