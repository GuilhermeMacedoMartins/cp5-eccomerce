# Ecommerce API — CP5 Advanced Business Development with .NET

API RESTful de e-commerce em **ASP.NET Core 8** com **Clean Architecture**, aplicando Operation Result, erros customizados (retorno e lançamento no repositório), consumo de serviço externo com resiliência, PageResult, HATEOAS e autenticação/autorização com JWT.

---

## Integrantes do grupo

| Nome completo | RM |
|---|---|
| *(preencher)* | *(preencher)* |
| *(preencher)* | *(preencher)* |

## 🎬 Vídeo de demonstração

**Link:** *(preencher antes da entrega — máximo 5 minutos)*

---

## Descrição do projeto

O sistema gerencia o catálogo de produtos e os pedidos de compra de um e-commerce, com controle de acesso por perfil.

| Entidade | Papel |
|---|---|
| **Produto** | Catálogo: nome, descrição, preço, estoque, categoria, ativo/inativo |
| **Pedido** | Compra de um cliente, com status, valor total e endereço de entrega |
| **ItemPedido** | Liga pedido e produto, com quantidade e preço congelado na compra |
| **Usuário** | Autenticação e autorização, com perfil Cliente ou Administrador |

### Regras de negócio

1. **Baixa de estoque com validação** — pedido sem saldo é rejeitado com `409`.
2. **Preço congelado na compra** — alterar o preço depois não muda o histórico do pedido.
3. **Produto inativo não pode ser vendido.**
4. **Itens duplicados são rejeitados** no mesmo pedido.
5. **Pedido Entregue ou Cancelado não muda mais de status.**
6. **Cancelamento repõe o estoque** dos produtos.
7. **Cliente só enxerga os próprios pedidos**; Administrador enxerga todos.

---

## Arquitetura e componentes

Quatro camadas, com as dependências sempre apontando para dentro:

```
API (Presentation) ──► Application ──► Domain
       │                                 ▲
       └──────► Infrastructure ──────────┘
```

```
CP5-.NET/
├─ src/
│  ├─ Ecommerce.Domain/              # Núcleo, sem dependência externa
│  │  ├─ Common/                     # Result, PageResult, Error, Link, DomainErrors
│  │  ├─ Entities/                   # Produto, Pedido, ItemPedido, Usuario
│  │  ├─ Enums/                      # StatusPedido, PerfilUsuario
│  │  ├─ Exceptions/                 # Exceções customizadas de domínio
│  │  └─ Interfaces/                 # Contratos de repositório e serviço externo
│  │
│  ├─ Ecommerce.Application/         # Casos de uso e DTOs
│  │  ├─ DTOs/                       # Common, Produto, Pedido, Auth
│  │  ├─ Interfaces/                 # ITokenService, IPasswordHasher
│  │  ├─ Mappings/                   # Entity ↔ DTO
│  │  └─ UseCases/                   # Produto, Pedido, Auth
│  │
│  ├─ Ecommerce.Infrastructure/      # Persistência, serviço externo, segurança
│  │  ├─ Data/                       # ApplicationContext, configurations, seeder
│  │  ├─ ExternalServices/           # ViaCepEnderecoService (HttpClient + Polly)
│  │  ├─ Repositories/               # Produto (retorno), Pedido (lançamento)
│  │  └─ Security/                   # TokenService (JWT), PasswordHasher (PBKDF2)
│  │
│  └─ Ecommerce.API/                 # Controllers e configuração do host
│     ├─ Common/                     # ApiResponse, ApiControllerBase
│     ├─ Controllers/                # Auth, Produtos, Pedidos, Enderecos
│     ├─ Hateoas/                    # LinkService
│     └─ Middlewares/                # ExceptionHandlingMiddleware
│
└─ tests/
   ├─ Ecommerce.Tests.Unit/          # Domínio, UseCases (Moq), segurança
   └─ Ecommerce.Tests.Functional/    # API completa no banco da aplicação
```

### Tecnologias

| Componente | Tecnologia |
|---|---|
| Framework | ASP.NET Core 8 (Controllers) |
| ORM | Entity Framework Core 8 + SQL Server |
| Autenticação | JWT Bearer, senhas com PBKDF2 (SHA256, 100k iterações) |
| Serviço externo | HttpClient tipado + Polly (retry exponencial e circuit breaker) |
| Logging | Serilog (console e arquivo diário) |
| Documentação | Swagger / OpenAPI com Annotations e botão Authorize |
| Testes | xUnit, Moq, WebApplicationFactory |

---

## Requisitos do CP5 e onde cada um está

| Requisito | Onde está |
|---|---|
| **Clean Architecture** | 4 projetos em `src/`, Domain sem dependências |
| **Otimização** | `AsNoTracking`, `AsSplitQuery`, 8 índices, índice com `IncludeProperties`, compressão, OutputCache, teto de `pageSize` |
| **Operation Result** | `Domain/Common/Result.cs`, usado em todos os UseCases |
| **Erros customizados — retorno** | `ProdutoRepository` devolve `Result.Failure(Error)` |
| **Erros customizados — lançamento** | `PedidoRepository` lança `EntidadeNaoEncontradaException` e `PersistenciaException` |
| **Serviço externo** | `ViaCepEnderecoService` com retry e circuit breaker |
| **PageResult** | `Domain/Common/PageResult.cs` com metadados completos |
| **HATEOAS** | `API/Hateoas/LinkService.cs`, links no recurso e na coleção |
| **Autenticação/Autorização** | JWT + policy `SomenteAdministrador` |

### As duas abordagens de erro customizado

```csharp
// RETORNO — ProdutoRepository
var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);
return produto is null
    ? Result.Failure<ProdutoEntity>(DomainErrors.Produto.NaoEncontrado(id))
    : Result.Success(produto);

// LANÇAMENTO — PedidoRepository
if (pedido is null)
    throw new EntidadeNaoEncontradaException(DomainErrors.Pedido.NaoEncontrado(id));
```

O `PedidoUseCase` captura a exceção e a converte em `Result`, de modo que a API sempre recebe um Operation Result, independentemente da abordagem usada.

### Mapeamento de erro para HTTP

| ErrorType | Status |
|---|---|
| Validation | 400 |
| NotFound | 404 |
| Conflict | 409 |
| Unauthorized | 403 |
| External | 503 |
| Unexpected | 500 |

---

## Como rodar o projeto

### Pré-requisitos

- .NET 8 SDK
- SQL Server (LocalDB já serve)
- `dotnet-ef` versão 8:
  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.10
  ```
  Se já houver uma versão mais nova instalada, use `dotnet tool update --global dotnet-ef --version 8.0.10`.

### Execução

```bash
dotnet restore
dotnet build
dotnet run --project src/Ecommerce.API
```

Swagger em **http://localhost:5250/swagger** (ou `https://localhost:7250/swagger` com o perfil https).

O banco e os dados de demonstração são criados automaticamente na primeira execução. Para trabalhar com migrations versionadas:

```bash
dotnet ef migrations add InitialCreate --project src/Ecommerce.Infrastructure --startup-project src/Ecommerce.API
dotnet ef database update --project src/Ecommerce.Infrastructure --startup-project src/Ecommerce.API
```

### Usuários de demonstração

| E-mail | Senha | Perfil |
|---|---|---|
| `admin@ecommerce.com` | `Admin@123` | Administrador |
| `cliente@ecommerce.com` | `Cliente@123` | Cliente |

### Executar os testes

```bash
dotnet test
```

Os testes funcionais usam **o mesmo provedor da aplicação (SQL Server)**, numa base separada chamada `EcommerceCP5_Tests`, criada e destruída automaticamente — nenhum banco alternativo como SQLite ou InMemory é usado, conforme exige o enunciado.

| Projeto | Cobertura |
|---|---|
| `Tests.Unit` | Result, PageResult, regras de domínio, UseCases com Moq, hash de senha |
| `Tests.Functional` | Ciclo HTTP completo, autorização, PageResult, HATEOAS, serviço externo |

---

## Configurações necessárias

Tudo fica em `src/Ecommerce.API/appsettings.json`.

### Banco de dados

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EcommerceCP5;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### JWT

```json
"Jwt": {
  "Issuer": "EcommerceAPI",
  "Audience": "EcommerceClientes",
  "SecretKey": "chave-super-secreta-do-cp5-fiap-com-no-minimo-32-caracteres-2026",
  "ExpiracaoEmMinutos": 120
}
```

> A chave precisa ter no mínimo 32 caracteres. Em produção ela sairia do appsettings e iria para variável de ambiente ou cofre de segredos.

### Serviço externo (ViaCEP)

```json
"ServicosExternos": {
  "ViaCep": {
    "BaseUrl": "https://viacep.com.br/",
    "TimeoutSegundos": 10
  }
}
```

A API pública do ViaCEP **não exige chave nem cadastro**. É necessário apenas acesso à internet. A resiliência está configurada em `Infrastructure/DependencyInjection.cs`: 3 tentativas com backoff exponencial (1s, 2s, 4s) e circuit breaker que abre por 30 segundos após 5 falhas seguidas.

---

## Endpoints

### Autenticação — `/api/auth`

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | Público | Devolve o token JWT |
| POST | `/api/auth/registrar` | Público | Cria conta com perfil Cliente |

### Produtos — `/api/produtos`

| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/produtos?pageNumber=1&pageSize=10&categoria=Perifericos` | Público |
| GET | `/api/produtos/{id}` | Público |
| POST | `/api/produtos` | **Administrador** |
| PUT | `/api/produtos/{id}` | **Administrador** |
| DELETE | `/api/produtos/{id}` | **Administrador** |

### Pedidos — `/api/pedidos`

| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/pedidos?pageNumber=1&pageSize=10` | Autenticado |
| GET | `/api/pedidos/{id}` | Autenticado (dono ou admin) |
| POST | `/api/pedidos` | Autenticado |
| PUT | `/api/pedidos/{id}/status` | Autenticado (dono ou admin) |
| DELETE | `/api/pedidos/{id}` | **Administrador** |

### Serviço externo — `/api/enderecos`

| Método | Rota | Acesso |
|---|---|---|
| GET | `/api/enderecos/{cep}` | Autenticado |

---

## Exemplos de requisição

### 1. Login

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "cliente@ecommerce.com", "senha": "Cliente@123" }
```

```json
{
  "sucesso": true,
  "dados": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiraEm": "2026-10-26T23:00:00Z",
    "usuarioId": 2,
    "nome": "Cliente Demo",
    "perfil": "Cliente"
  },
  "erro": null
}
```

No Swagger, clique em **Authorize** e cole apenas o `accessToken`.

### 2. Listagem com PageResult e HATEOAS

```http
GET /api/produtos?pageNumber=1&pageSize=2
```

```json
{
  "sucesso": true,
  "dados": {
    "data": [
      {
        "id": 1,
        "nome": "Mouse Gamer RGB",
        "preco": 189.90,
        "quantidadeEstoque": 50,
        "links": [
          { "href": "http://localhost:5250/api/produtos/1", "rel": "self", "method": "GET" },
          { "href": "http://localhost:5250/api/produtos", "rel": "collection", "method": "GET" }
        ]
      }
    ],
    "pageNumber": 1,
    "pageSize": 2,
    "totalRecords": 5,
    "totalPages": 3,
    "itemsOnPage": 2,
    "hasPrevious": false,
    "hasNext": true,
    "isFirstPage": true,
    "isLastPage": false,
    "links": [
      { "href": ".../api/produtos?pageNumber=1&pageSize=2", "rel": "self", "method": "GET" },
      { "href": ".../api/produtos?pageNumber=1&pageSize=2", "rel": "first", "method": "GET" },
      { "href": ".../api/produtos?pageNumber=3&pageSize=2", "rel": "last", "method": "GET" },
      { "href": ".../api/produtos?pageNumber=2&pageSize=2", "rel": "next", "method": "GET" }
    ]
  }
}
```

Com token de Administrador, cada produto também traz os links `update` e `delete`.

### 3. Criar pedido (consulta o serviço externo)

```http
POST /api/pedidos
Authorization: Bearer {token}
Content-Type: application/json

{
  "cep": "01001000",
  "itens": [
    { "produtoId": 1, "quantidade": 2 },
    { "produtoId": 2, "quantidade": 1 }
  ]
}
```

```json
{
  "sucesso": true,
  "dados": {
    "id": 1,
    "clienteNome": "Cliente Demo",
    "status": "Pendente",
    "valorTotal": 729.70,
    "endereco": {
      "cep": "01001-000",
      "logradouro": "Praça da Sé",
      "bairro": "Sé",
      "cidade": "São Paulo",
      "uf": "SP"
    },
    "itens": [
      { "produtoId": 1, "produtoNome": "Mouse Gamer RGB", "quantidade": 2, "precoUnitario": 189.90, "subtotal": 379.80 }
    ],
    "links": [
      { "href": ".../api/pedidos/1", "rel": "self", "method": "GET" },
      { "href": ".../api/pedidos/1/status", "rel": "update-status", "method": "PUT" }
    ]
  }
}
```

O endereço foi preenchido pela chamada ao ViaCEP.

### 4. Respostas de erro

**409 — estoque insuficiente**
```json
{
  "sucesso": false,
  "dados": null,
  "erro": {
    "codigo": "ESTOQUE_INSUFICIENTE",
    "mensagem": "Estoque insuficiente para 'Mouse Gamer RGB'. Disponivel: 1, solicitado: 5.",
    "tipo": "Conflict",
    "traceId": "00-a1b2c3...",
    "timestamp": "2026-10-26T21:40:00Z"
  }
}
```

**404 — erro lançado pelo repositório de pedidos**
```json
{
  "sucesso": false,
  "erro": { "codigo": "PEDIDO_NAO_ENCONTRADO", "tipo": "NotFound" }
}
```

**503 — serviço externo indisponível**
```json
{
  "sucesso": false,
  "erro": { "codigo": "CEP_SERVICO_INDISPONIVEL", "tipo": "External" }
}
```

**403 — Cliente tentando cadastrar produto**
```
HTTP/1.1 403 Forbidden
```

---

## Roteiro do vídeo (5 minutos)

| Tempo | O que mostrar |
|---|---|
| 0:00–0:30 | Estrutura em 4 camadas no Solution Explorer |
| 0:30–1:10 | `dotnet test` com a suíte completa aprovada |
| 1:10–1:50 | Operation Result e erro customizado: `GET /api/produtos/999999` → 404 com código; `GET /api/pedidos/999999` → 404 vindo da exceção lançada |
| 1:50–2:40 | Serviço externo: `GET /api/enderecos/01001000` e criação de pedido com endereço preenchido |
| 2:40–3:20 | PageResult: `GET /api/produtos?pageNumber=1&pageSize=2` com todos os metadados |
| 3:20–3:50 | HATEOAS: links `self`, `next`, `last` na coleção e nos itens |
| 3:50–4:40 | Autorização: POST sem token → 401; com token de Cliente → 403; com token de Admin → 201 |
| 4:40–5:00 | Fechamento com os critérios atendidos |
