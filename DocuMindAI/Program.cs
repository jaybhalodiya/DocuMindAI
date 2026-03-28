using DocuMindAI.Services;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi;
using OpenAI;
using Qdrant.Client;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

var githubToken = builder.Configuration["GitHubModels:Token"]
    ?? throw new InvalidOperationException("Missing configuration: GitHubModels:Token");

var credential = new ApiKeyCredential(githubToken);
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.inference.ai.azure.com")
};

var ghModelsClient = new OpenAIClient(credential, openAIOptions);

builder.Services.AddSingleton<IChatClient>(sp =>
    ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient());

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
    ghModelsClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());

builder.Services.AddSingleton(new QdrantClient("localhost", 6334));

builder.Services.AddScoped<PdfIngestionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DocuMindAI API", Version = "v1" });
});

builder.Services.AddCors(options => {
    options.AddPolicy("AngularPolicy", p =>
        p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
