## DocuMindAI.API

DocuMind AI is a professional-grade Retrieval-Augmented Generation (RAG) platform designed to turn static corporate documents (PDFs) into an intelligent, interactive, and verifiable knowledge base.

---

**Technical Architecture & Key Components**

**1. The Ingestion Lifecycle (PDF to Vector)**
This is where your static PDFs are transformed into AI-ready data.

Extraction: The .NET API uses UglyToad.PdfPig to parse text page-by-page.

Vectorization: Text chunks are sent to GitHub Models (GPT-4o) to generate high-dimensional Embeddings (mathematical lists that represent meaning).

Storage: These vectors are indexed and stored in the Qdrant Vector Database.

Deterministic Upserting: A custom ID strategy ensures that if a document is updated and re-uploaded, the old version is overwritten—preventing "data ghosting."

**2. The Retrieval Lifecycle (Question to Answer)**
This is the "AI Intelligence" that generates the verified response.

Semantic Search: When a user asks a question, the API searches Qdrant not for keywords, but for the meaning of the question.

Context Assembly: The top matching document paragraphs are retrieved.

Generation: This specific context, combined with the original question, is fed to the LLM to generate the final, grounded answer.

---

**Primary Use Cases**

**Product Support Chatbot**
Answer complex, technical questions (e.g., wiring diagrams, repair steps) directly from product manuals.

**HR/Company Policy Bot**
Provide instant answers to internal policy questions (e.g., "What is the hybrid work policy?")

---

**Technology Stack & Library Details**

Frontend
Angular 19 (Standalone)
Signals: (For sub-millisecond reactivity)
Marked.js: (AI-generated Markdown parser)

Backend
ASP.NET Core 10
Microsoft.Extensions.AI: (Unified AI Model interface)
Qdrant.Client: (gRPC Vector DB Client)

Vector DB
Qdrant (via Docker)

AI Models
GitHub Models (GPT-4o-mini, Text-Embedding-3)

---

![Alt text](DocuMindAI/assets/image_1.png)

![Alt text](DocuMindAI/assets/image_2.png)



