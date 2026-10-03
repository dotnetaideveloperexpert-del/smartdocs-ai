Q1: "What is Clean Architecture?"
Answer:

"Clean Architecture is a software design pattern that separates code into concentric layers, each with its own responsibility. The core idea is that inner layers should not depend on outer layers — dependencies always point inward.

In my SmartDocs project, I have four layers:

Domain — pure business entities like User, Document, ChatSession. Zero dependencies on any framework.

Application — business logic and interfaces. Depends only on Domain.

Infrastructure — implementations using EF Core, Azure OpenAI, Blob Storage. Depends on Application and Domain.

Api — the entry point with controllers and DI setup. Depends on Application and Infrastructure.

This makes the codebase testable, framework-agnostic, and easy to change. If I switch from SQL Server to PostgreSQL, only Infrastructure changes."

❓ Q2: "Why did you choose 4 layers and not just 1 project?"
Answer:

"A single project creates tight coupling — business logic mixed with database code, HTTP concerns, and framework dependencies. Testing becomes difficult because everything is connected.

With 4 layers:

I can unit test business logic without a database

I can swap implementations — LocalFileStorage in dev, AzureBlobStorage in prod — without touching Application code

New developers understand the codebase faster because each layer has a clear purpose

Framework upgrades don't break business logic

The trade-off is more files and initial setup time, but for a project that will grow to production size, it's worth it."

❓ Q3: "What is Dependency Inversion?"
Answer:

"Dependency Inversion means high-level modules should not depend on low-level modules — both should depend on abstractions.

In my project, AuthService (high-level, in Application) needs to save users. Instead of depending on UserRepository (low-level, in Infrastructure), it depends on IUserRepository (an interface). Infrastructure provides the actual implementation.

The key benefit: Application doesn't know about EF Core or SQL Server. It just knows 'I need a way to save users.' This makes it testable and swappable.

In Program.cs, I wire them up:

text
builder.Services.AddScoped<IUserRepository, UserRepository>();
If I wanted to use a different database tomorrow, I'd just change this line."

❓ Q4: "How do you enforce the dependency direction?"
Answer:

"I use project references. In .NET, a project can only reference types from projects it depends on.

My reference structure:

Domain → NO references (pure entities)

Application → references only Domain

Infrastructure → references Application + Domain

Api → references Application + Infrastructure

If someone accidentally tries to reference Infrastructure from Application, the compiler throws an error. So the architecture is enforced by the compiler, not just by convention."

❓ Q5: "Where do interfaces live and why?"
Answer:

"Interfaces live in the Application layer, implementations in Infrastructure.

Why? Because Application defines what it needs — like 'I need a way to store files' — but doesn't care how it's done.

For example:

IFileStorage is in Application — with methods like SaveAsync, GetAsync

LocalFileStorage (dev) and AzureBlobStorage (prod) are in Infrastructure

The Application code only calls IFileStorage.SaveAsync(). It doesn't know whether it's saving to disk, Azure Blob, or AWS S3. This is Dependency Inversion in practice."

🔐 SECTION 2: JWT (Day 3)
❓ Q6: "What is JWT and how does it work?"
Answer:

"JWT stands for JSON Web Token. It's a stateless authentication mechanism.

When a user logs in, the server generates a signed token containing claims like user ID, email, and role. The token has three parts — Header, Payload, and Signature. The first two are Base64 encoded, so anyone can read them. The Signature is what proves the token is legitimate — it's created using HMAC-SHA256 with a secret key that only the server knows.

The client stores this token and sends it in the Authorization header as Bearer <token>. The server validates the signature on every request — no database lookup or session storage needed.

This is why JWT is horizontally scalable — any server in a cluster can validate the token independently."

❓ Q7: "What are the 3 parts of a JWT?"
Answer:

"JWT has three parts separated by dots:

Header — Base64 encoded JSON with the algorithm (HS256) and token type (JWT).

Payload — Base64 encoded JSON with claims. Standard claims include iss (issuer), exp (expiration), iat (issued at). Custom claims in my project: nameid, email, unique_name, role.

Signature — HMAC-SHA256(Header + Payload + SecretKey). This is what makes the token tamper-proof.

Important: the first two parts are readable by anyone. Only the signature is secure. That's why we never put passwords or sensitive data in a JWT."

❓ Q8: "What are claims?"
Answer:

"Claims are statements about the user stored inside the JWT payload. They come in two types:

Registered claims — standardized by JWT spec: iss (issuer), sub (subject), aud (audience), exp (expiration), nbf (not before), iat (issued at), jti (JWT ID).

Custom claims — defined by us. In my SmartDocs project: nameid for the user's Guid, email, unique_name for display, and role for authorization.

The role claim is used for authorization — admin endpoints check [Authorize(Roles=\"Admin\")]. When the request comes in, the JwtBearer middleware extracts claims and populates HttpContext.User. My controllers then use User.FindFirstValue(ClaimTypes.NameIdentifier) to get the user ID."

❓ Q9: "How does signature verification work?"
Answer:

"When a request comes in, the JWT Bearer middleware does the following:

Splits the token into Header, Payload, and Signature.

Re-hashes Header + Payload + SecretKey using HMAC-SHA256.

Compares the new hash with the received signature.

If they match, the token is authentic. If not, it returns 401.

An attacker can't forge a token because they don't have the secret key. If they modify the payload — like changing role from User to Admin — the signature won't match. So JWT is tamper-proof.

I configure this in Program.cs via TokenValidationParameters — ValidateIssuer, ValidateAudience, ValidateLifetime, ValidateIssuerSigningKey are all set to true."

❓ Q10: "What if someone steals my token?"
Answer:

"If a token is stolen, the attacker can impersonate the user until the token expires. To mitigate:

Short expiry — my current implementation is 24 hours, but production best practice is 15 minutes.

Refresh tokens — for longer sessions, the refresh token (stored in HTTP-only cookie) gets a new access token silently.

HTTPS always — prevents token interception in transit.

Token blacklist — for immediate revocation, add the token ID to a Redis blacklist and check it on every request.

Secret rotation — if the secret leaks, rotate it. This invalidates all tokens.

The important thing is to never store tokens in localStorage for high-security apps — prefer HTTP-only cookies which JavaScript cannot read."

❓ Q11: "Where do you store the JWT secret in production?"
Answer:

"In development, the secret is in appsettings.Development.json which is gitignored — it never reaches source control.

In production, I use Azure Key Vault with Managed Identity. The App Service has a Managed Identity — a service principal Azure manages automatically. This identity has read access to specific secrets in Key Vault. The App Service references the secret using @Microsoft.KeyVault(SecretUri=...) syntax in its configuration.

Azure handles the authentication — no credentials anywhere. If the secret rotates, Key Vault updates it and the App Service picks up the new value. Even our database connection string is stored in Key Vault."

🔒 SECTION 3: BCrypt & Passwords (Day 3)
❓ Q12: "Why BCrypt over SHA256 for passwords?"
Answer:

"SHA256 is a fast hashing algorithm — it can compute billions of hashes per second on modern GPUs. That's fine for file integrity, but dangerous for passwords. A hacker could brute-force billions of password combinations per second.

BCrypt is intentionally slow. It has a configurable cost factor — I use 11, which means 2^11 = 2048 rounds. That takes about 100 milliseconds per hash. For a normal login, that's fine. But for an attacker brute-forcing, it becomes computationally infeasible — centuries instead of seconds.

BCrypt also automatically adds a random salt to each password, so identical passwords produce different hashes."

❓ Q13: "What is salt and why is it important?"
Answer:

"Salt is random data added to the password before hashing. Without salt, two users with the same password would have identical hashes. An attacker could use a rainbow table — pre-computed hashes of common passwords — to crack them instantly.

With salt, identical passwords produce completely different hashes. If 100 users all have password 'hello123', the database shows 100 different hashes.

BCrypt stores the salt inside the hash string itself. The format is $2a$11$<22-chars-salt><31-chars-hash>. So we don't need a separate column. When verifying, BCrypt extracts the salt from the stored hash, hashes the input with the same salt, and compares."

❓ Q14: "Why hash instead of encrypt passwords?"
Answer:

"Encryption is reversible — if the encryption key leaks, all passwords leak. It's like locking a diary — if the key is stolen, everything is readable.

Hashing is one-way — you cannot reverse a hash into a password. Even if our entire database leaks, attackers cannot directly get passwords. They'd need to brute-force each hash individually, which with BCrypt takes centuries.

If we encrypted passwords and the key ever leaked — through a code leak, log file, or misconfigured server — every user's password would be compromised instantly. With hashing, there's no key to leak. This is why hashing is the industry standard for passwords."

❓ Q15: "If two users have the same password, what's stored?"
Answer:

"Different hashes because BCrypt generates a random salt for each password. Even though the plaintext is the same, the salt is different, so the resulting hash is completely different.

For example:

User A: password hello123, salt X8kP9..., hash $2a$11$X8kP9...abc

User B: password hello123, salt K9mN2..., hash $2a$11$K9mN2...xyz

During verification, BCrypt extracts the salt from each user's stored hash and uses it to re-hash the input. Both verify successfully because the salt matches their own hash.

This defeats rainbow tables — the attacker can't pre-compute hashes since each user's salt is different."

🗄️ SECTION 4: EF Core & Database (Day 2)
❓ Q16: "What is EF Core and why use it?"
Answer:

"EF Core is Microsoft's Object-Relational Mapper. It maps C# classes to database tables and lets me write LINQ queries instead of raw SQL.

Benefits:

Type safety — errors caught at compile time

Migrations — schema changes tracked in version control

Database-agnostic — switch from SQL Server to PostgreSQL with one line

Change tracking — automatic UPDATE/DELETE generation

I use it in the Infrastructure layer with a DbContext. Controllers and services don't touch EF Core directly — they use repositories or the DbContext through Dependency Injection."

❓ Q17: "What are migrations?"
Answer:

"Migrations are a version control system for the database schema. Instead of manually writing SQL to create tables, I define entities in C# and EF Core generates migration files.

Each migration is a snapshot of the schema at a point in time. When I add a new entity or property, I create a new migration. The old one stays.

Key commands:

Add-Migration InitialCreate — create a migration

Update-Database — apply migrations to the database

Remove-Migration — undo the last migration

In production, migrations can be applied automatically at startup or via a CI/CD pipeline step. This keeps dev, staging, and production schemas in sync."

❓ Q18: "Why do you need Design package in API project?"
Answer:

"EF Core design tools need to bootstrap the DI container from the startup project — which is the API project. The Design package provides the tooling for Add-Migration and Update-Database commands.

Even though the DbContext lives in Infrastructure, the design tools must be in API because API is the startup project and owns the DI configuration and appsettings.json.

So: SqlServer package in Infrastructure for runtime database access. Design package in both API and Infrastructure for design-time tooling."

❓ Q19: "What is DbContext and why is it scoped?"
Answer:

"DbContext is the EF Core class that represents a session with the database. It tracks entity changes, generates SQL, and manages transactions.

It's registered as Scoped — one instance per HTTP request. Why? Because DbContext is not thread-safe. If two threads shared the same instance, we'd have race conditions.

Scoped lifetime means each request gets a fresh DbContext, and it's disposed at the end. This gives us clean isolation between requests and prevents memory leaks."

🎯 SECTION 5: Register & Login Flow
❓ Q20: "Walk me through the register flow."
Answer:

"When a user submits the register form:

Request hits AuthController.Register — the body is deserialized into a RegisterRequest DTO.

Controller calls AuthService.RegisterAsync — the actual business logic.

Email check — IUserRepository.EmailExistsAsync queries the database. If the email exists, we return null and the controller responds with 409 Conflict.

User creation — a new User object with a Guid ID, normalized email (lowercase), and BCrypt-hashed password.

Database save — _db.Users.Add(user) and SaveChangesAsync, which generates INSERT INTO Users.

Token generation — JwtService.GenerateToken(user) creates a signed JWT with 4 claims (nameid, email, unique_name, role) and 24-hour expiry.

Response — 200 OK with userId, name, email, role, and token."

❓ Q21: "Walk me through the login flow."
Answer:

"Login is simpler:

Request hits AuthController.Login with LoginRequest (email, password).

AuthService.LoginAsync — first finds the user by email. If not found, returns null → 401.

Password verification — BCrypt.Verify(inputPassword, storedHash). BCrypt extracts the salt from the stored hash, hashes the input with the same salt, and compares. If no match → 401.

Token generation — a new JWT is generated with the same claims.

Response — 200 OK with the same structure as register.

A key security point: whether the user doesn't exist or the password is wrong, the response is the same 401 Unauthorized. This prevents user enumeration attacks."

❓ Q22: "How do you secure user-specific data?"
Answer:

"Every query is filtered by UserId from the JWT token. The UserId comes from the nameid claim — the client can't manipulate it.

In the DocumentsController:

text
private Guid UserId => Guid.Parse(
    User.FindFirstValue(ClaimTypes.NameIdentifier)!);

var docs = await _db.Documents
    .Where(d => d.UserId == UserId)
    .ToListAsync();
This is called row-level security. Even if someone knows another user's document ID and tries to delete it, the WHERE UserId = @currentUser clause means the query returns nothing. We return 404 Not Found — the user can't even tell if the document exists.

It's defense-in-depth — even if an ID leaks, cross-user access is impossible."

⚛️ SECTION 6: React Frontend (Day 5)
❓ Q23: "Why Vite over Create React App?"
Answer:

"Vite is significantly faster than Create React App. It uses native ES modules during development, so there's no bundling step. The dev server starts in under a second — CRA takes 10-30 seconds.

Hot Module Replacement is also instant. In CRA, changing a component requires a full rebuild. In Vite, only the changed module reloads.

CRA is also no longer maintained by the React team. Vite is now the recommended way to start React projects."

❓ Q24: "Why do you need a proxy in Vite config?"
Answer:

"During development, the React app runs on localhost:5173 and the API runs on localhost:7001. These are different origins.

Browsers block cross-origin requests by default — this is the CORS policy. Without a proxy, every API call from React would fail with a CORS error.

I use a Vite proxy — /api requests are forwarded to the API server. The browser thinks it's calling localhost:5173/api, but Vite internally proxies to localhost:7001/api. Since the request is same-origin from the browser's perspective, no CORS issue.

In production, both frontend and backend are on the same domain, so no proxy needed. The Vite config only affects local development."

❓ Q25: "How do you handle authentication in the frontend?"
Answer:

"I use Axios interceptors. On every request, a request interceptor reads the JWT from localStorage and adds it to the Authorization header. This way I don't have to manually attach the token in every API call.

On the response side, a response interceptor catches 401 errors — it clears the token and redirects to the login page. This handles token expiry transparently.

text
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
For production, I'd move the token to an HTTP-only cookie to prevent XSS attacks, but localStorage is fine for learning."

🎤 SECTION 7: General .NET / C#
❓ Q26: "What is Dependency Injection?"
Answer:

"DI is a design pattern where a class doesn't create its own dependencies — they're injected from outside.

In my AuthController, instead of new AuthService(...), I ask for AuthService in the constructor. The DI container provides it. This makes the class testable — I can inject a mock in unit tests.

I register services in Program.cs:

text
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthService>();
.NET has three lifetimes:

Transient — new instance every time

Scoped — one per HTTP request (most services)

Singleton — one for the whole application"

❓ Q27: "What's the difference between authentication and authorization?"
Answer:

"Authentication is 'who are you?' — verifying identity via login. In my project, JWT handles authentication.

Authorization is 'what can you do?' — checking permissions. In my project, [Authorize] attribute handles authorization. For role-based access, I use [Authorize(Roles=\"Admin\")] on admin endpoints.

Authentication always comes first. You can't authorize someone you haven't authenticated.

On the Documents controller, I have [Authorize] at the class level — every endpoint requires a valid token. The admin controller has [Authorize(Roles = \"Admin\")] — only admins can access it."

❓ Q28: "How do you handle async/await?"
Answer:

"I use async/await throughout the codebase because most operations are I/O-bound — database queries, file uploads, HTTP calls to Azure OpenAI.

Async frees up the thread while waiting for I/O. Instead of blocking a thread for 100ms while a database query runs, the thread goes back to the pool and handles other requests. This is critical for scalability.

I pass CancellationToken through the stack. If a user closes the browser mid-request, the token signals cancellation and we stop processing — saving resources.

A common mistake is .Result or .Wait() — this blocks the thread and can cause deadlocks. I always use await."

🎯 SECTION 8: Project-Specific
❓ Q29: "Walk me through your SmartDocs AI project."
Answer:

"SmartDocs AI is an AI-powered document Q&A platform with three main components:

1. Document Management — users upload PDF/DOCX/TXT files. The backend extracts text, chunks it, generates embeddings, and indexes in Azure AI Search.

2. RAG Chat — users ask questions in natural language. The system embeds the question, retrieves top-5 relevant chunks, and sends them as context to GPT-4o-mini. Answers come back with source citations.

3. MCP Server — exposes 5 tools (SearchDocuments, GetDocumentDetails, GetDocumentSummary, GetUserDocuments, AskDocumentQuestion) via Model Context Protocol, so external AI clients like Claude can invoke them.

Technically:

.NET 10 backend with Clean Architecture (4 layers)

EF Core + SQL Server for metadata

JWT + BCrypt for auth

Azure OpenAI (GPT-4o-mini + text-embedding-3-small)

Azure AI Search for vector retrieval

Azure Blob Storage for file uploads

React + TypeScript frontend

Docker + GitHub Actions + Azure App Service for deployment"

❓ Q30: "Why did you build this project?"
Answer:

"I wanted to build something that demonstrates modern AI engineering skills — not just CRUD apps. RAG, MCP, and vector search are the cutting-edge technologies in AI development right now.

The project shows that I can:

Architect a production-grade .NET solution

Integrate AI services (LLM, embeddings, vector search)

Implement MCP protocol for AI tool integration

Handle authentication, security, and deployment

It's also a working product that solves a real problem — searching documents with natural language is a common enterprise need."