using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Security.Cryptography;
using System.Text;
using UglyToad.PdfPig;

namespace DocuMindAI.Services
{
    public class PdfIngestionService(IEmbeddingGenerator<string, Embedding<float>> generator, QdrantClient qdrant)
    {
        private const string CollectionName = "pdf_collection";

        public async Task IngestFolderAsync(string folderPath)
        {
            var collections = await qdrant.ListCollectionsAsync();
            bool exists = collections.Contains(CollectionName);
            if (!exists)
            {
                await qdrant.CreateCollectionAsync(CollectionName,
                new VectorParams { Size = 1536, Distance = Distance.Cosine });
            }
           
            var files = Directory.GetFiles(folderPath, "*.pdf");

            foreach (var file in files)
            {
                using var document = PdfDocument.Open(file);
                var fileName = Path.GetFileName(file);
                var points = new List<PointStruct>();

                foreach (var page in document.GetPages())
                {
                    if (string.IsNullOrWhiteSpace(page.Text)) continue;

                    var embeddings = await generator.GenerateAsync([page.Text]);

                    // --- CHANGE: GENERATE DETERMINISTIC ID ---
                    // Instead of Guid.NewGuid(), we create an ID based on the file and page
                    Guid deterministicId = CreateDeterministicGuid(fileName, page.Number);

                    points.Add(new PointStruct
                    {
                        Id = deterministicId, // <--- CHANGE: THIS OVERWRITES OLD DATA
                        Vectors = embeddings[0].Vector.ToArray(),
                        Payload = {
                            ["text"] = page.Text,
                            ["source"] = fileName,
                            ["page"] = page.Number,
                            ["ingested_at"] = DateTime.UtcNow.ToString("o")
                        }
                    });

                    if (points.Count >= 10)
                    {
                        await qdrant.UpsertAsync(CollectionName, points);
                        points.Clear();
                    }
                }
                if (points.Any()) await qdrant.UpsertAsync(CollectionName, points);
            }
        }

        // This creates a consistent UUID from a string so Qdrant knows it's the same record
        private Guid CreateDeterministicGuid(string fileName, int pageNumber)
        {
            string input = $"{fileName}_{pageNumber}";
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                return new Guid(hash);
            }
        }
    }
}
