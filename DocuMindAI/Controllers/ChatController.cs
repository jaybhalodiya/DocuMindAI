using DocuMindAI.Models;
using DocuMindAI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Qdrant.Client;

namespace DocuMindAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController (
    IChatClient chatClient,
    IEmbeddingGenerator<string, Embedding<float>> generator,
    QdrantClient qdrant,
    PdfIngestionService ingestionService) : ControllerBase
    {
        [HttpPost("ask")]
        public async Task<ActionResult<Models.ChatResponse>> Ask([FromBody] ChatRequest request)
        {
            // 1. Vectorize query
            var queryEmbed = await generator.GenerateAsync([request.Message]);

            // 2. Retrieve context from Qdrant
            var searchResults = await qdrant.SearchAsync("pdf_collection",
                queryEmbed[0].Vector.ToArray(), limit: 3);

            var context = string.Join("\n", searchResults.Select(r => r.Payload["text"].StringValue));
            var sources = searchResults.Select(r => r.Payload["source"].StringValue).Distinct().ToList();

            var prompt = $"Context:\n{context}\n\nUser Question: {request.Message}\nAnswer using context only:";

            var aiResponse = await chatClient.GetResponseAsync(prompt);

            string answer = aiResponse.Messages.FirstOrDefault()?.Text ?? "No answer found.";

            return Ok(new Models.ChatResponse(answer, sources));
        }

        [HttpPost("ingest")]
        public async Task<IActionResult> Ingest()
        {
            string pdfPath = @"D:\LLM\Data";

            await ingestionService.IngestFolderAsync(pdfPath);
            return Ok(new { message = "Documents processed and vectorized." });
        }
    }
}
