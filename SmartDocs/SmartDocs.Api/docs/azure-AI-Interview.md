Q1. AI, ML aur Deep Learning mein kya farak hai?
Answer:

"AI ek umbrella term hai — koi bhi technique jo machine ko insaan jaisi intelligence de. ML, AI ka subset hai jahan machine data se khud seekhti hai, rules hum nahi likhte. Deep Learning, ML ka subset hai jisme multi-layer neural networks use hote hain. GPT-4o jaise LLMs Deep Learning hi hain — Transformers pe based."

Yaad rakho: AI ⊃ ML ⊃ Deep Learning ⊃ LLMs
(Bahar wala bada, andar wala specific)

Q2. Generative AI kya hai?
Answer:

"Generative AI wo models hain jo naya content banate hain, sirf classify nahi karte. Discriminative AI ek fixed label chunta hai — jaise spam filter. Generative AI naya text, code, image banata hai — jaise ChatGPT. LLMs next token predict karke content generate karte hain. Real project mein main dono use karta hu — ek classifier ticket route karne ke liye, aur generative model reply likhne ke liye."

Analogy (interviewer ko yaad reh jaati hai):

Discriminative AI = Judge (faisla deta hai)

Generative AI = Artist (naya banata hai)

Q3. Discriminative aur Generative AI mein farak?
Answer:

"Discriminative AI bounded output deta hai — spam/not spam, churn/no churn. Generative AI unbounded naya content banata hai — text, image, code. Discriminative purana hai, decisions ke liye. Generative naya wave hai, creation ke liye."

🔵 SECTION 2: Azure OpenAI (Practical)
Q4. Azure OpenAI kyun, public OpenAI kyun nahi?
Answer:

"Azure mein data isolation hai — mera data training ke liye kabhi use nahi hota. 99.9% SLA milti hai. SOC2, HIPAA, GDPR jaise compliance standards meet hote hain. Billing aur governance Azure subscription ke andar rehti hai. Enterprise projects mein Azure OpenAI default choice hota hai."

Follow-up aaye to ye points:

Data isolation

Training ke liye data use nahi hota

99.9% SLA

Compliance ready

Regional deployment

Single Azure billing

Q5. Azure OpenAI ka access kaise secure karte ho?
Answer:

"Main API key use hi nahi karta. Meri app Managed Identity se authenticate karti hai, Azure RBAC ke through — 'Cognitive Services OpenAI User' role use karke. Locally Azure CLI login via DefaultAzureCredential, aur production mein specific managed identity. Jo bhi secrets bache, wo Key Vault mein hain. Code ya config mein kuch sensitive nahi hota."

Ye answer senior-level hai. Kyun:

API key avoid karta hai (security best practice)

RBAC role mention (Azure IAM knowledge)

Dev vs Prod alag (production thinking)

Key Vault mention (secret management)

Q6. Managed Identity kya hai aur kyun use karte ho?
Answer:

"Managed Identity Azure ka feature hai jo kisi resource — jaise Container App ya App Service — ko Microsoft Entra ID mein apni identity deta hai. Ye resource doosri Azure services se authenticate kar sakta hai bina koi secret store kiye. Main ise use karta hu kyunki API keys config ya logs se leak ho sakti hain, aur rotate karna painful hota hai. Managed Identity mein access Azure RBAC roles se control hota hai."

Follow-up: "System-assigned vs User-assigned?"

"System-assigned identity resource ke lifecycle se bandhi hoti hai — resource delete karo, identity bhi gayi. User-assigned alag resource hoti hai jo multiple resources share kar sakte hain. Single app ke liye system-assigned simple hai. Shared access ke liye user-assigned better hai."

Q7. DefaultAzureCredential kya hai?
Answer:

"DefaultAzureCredential Azure.Identity se ek chained credential hai. Ye multiple authentication methods try karta hai ek order mein — environment variables, managed identity, Azure CLI, Visual Studio. Jo pehla kaam kare, wahi use karta hai. Isse same code locally bhi chalta hai (az login ke saath) aur Azure pe bhi (Managed Identity ke saath) — koi code change nahi."

Q8. Kaunsa model deploy kiya aur kyun?
Answer:

"Maine gpt-4.1-mini chat ke liye aur text-embedding-3-small embeddings ke liye deploy kiya. Initially gpt-4o-mini use karna chahta tha, lekin Microsoft ne usko naye deployments ke liye deprecate kar diya. To main gpt-4.1-mini pe switch kiya, jo recommended replacement hai. Lesson ye tha ki model-agnostic abstraction banana chahiye, taaki model switch karna sirf config change ho — code rewrite nahi."

Ye answer bonus points deta hai — kyunki tumne real production problem (deprecation) handle kiya hai. Interviewer samjhega ki tumne sirf tutorial follow nahi kiya.

🟡 SECTION 3: Deployment & RBAC
Q9. RBAC kya hai aur tumne kaise use kiya?
Answer:

"RBAC matlab Role-Based Access Control. Azure mein built-in roles hote hain jo specific permissions dete hain. Maine apni identity ko 'Cognitive Services OpenAI User' role assign kiya, scoped sirf mere OpenAI resource pe. Isse sirf utni hi permission milti hai jitni OpenAI endpoints call karne ke liye chahiye — zyada nahi. Ye least privilege principle follow karta hai."

Q10. Resource Group aur Resource mein farak?
Answer:

"Resource ek individual Azure service hoti hai — jaise Azure OpenAI account, storage account, VM. Resource group ek logical container hai jo related resources ko hold karta hai. Main resource groups environment ya application ke hisaab se banata hu — jaise rg-meridian-ai-dev development environment ke liye."

🟣 SECTION 4: Practical / Hands-on
Q11. Azure OpenAI setup kaise kiya? Step by step batao.
Answer:

"Pehle maine East US region mein Azure portal se Azure OpenAI resource banaya. Phir Azure AI Foundry kholke 2 models deploy kiye — gpt-4.1-mini as 'chat-model' aur text-embedding-3-small as 'embedding-model'. Azure CLI install kiya, device code flow se login kiya kyunki MFA required tha, aur apni identity ko 'Cognitive Services OpenAI User' role assign kiya OpenAI resource pe. Ab mera code DefaultAzureCredential se authenticate karta hai — kahin bhi API key nahi."

Q12. Device code flow kyun use kiya login ke liye?
Answer:

"Kyunki Azure ab CLI logins ke liye MFA compulsory kar chuka hai, aur simple az login MFA error de raha tha. Device code flow mein main browser mein interactively authenticate karta hu, MFA complete karta hu, aur CLI session token pick kar leta hai. Jab MFA enforced ho, ye recommended approach hai."