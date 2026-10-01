# SmartDocs AI

AI-powered document Q&A platform with RAG pipeline, MCP Server, and .NET 10.

## 🎯 What It Does

- Upload PDF/DOCX/TXT documents
- Ask questions in natural language
- Get AI answers with **source citations**
- Chat history per session
- Admin dashboard for user/document management
- **MCP Server** exposing 5 tools for external AI clients

## 🛠 Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10, ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | SQL Server |
| AI (LLM) | Azure OpenAI (GPT-4o-mini) |
| AI (Embeddings) | Azure OpenAI (text-embedding-3-small) |
| Vector Search | Azure AI Search |
| File Storage | Azure Blob Storage |
| Frontend | React + TypeScript + Vite |
| Auth | JWT |
| Protocol | Model Context Protocol (MCP) |
| DevOps | Docker, GitHub Actions, Azure App Service |

## 🏗 Architecture

Clean Architecture with 4 layers:
