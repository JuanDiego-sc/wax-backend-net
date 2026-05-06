---
name: dotnet-professional
description: >
  Rules and standards for professional .NET backend development with OpenCode.
  Apply this skill at the start of every session, before any planning or code generation.
  Triggers on: new feature planning, architecture decisions, command execution requests,
  code generation, refactoring, or any task involving Clean Architecture, CQRS, EF Core,
  FluentValidation, third-party SDK integration, authentication providers, or payment processing.
  This skill must be consulted before writing a single line of code or running any terminal command.
---

# Professional .NET Development Standards

## Pre-Session Checklist

Before planning or writing any code, execute this checklist without exception:

1. Read all available SKILL.md files relevant to the current task.
2. Identify connected MCPs and confirm which tools are available in the current session.
3. Understand the existing architecture before proposing any change.
4. Ask clarifying questions if the task is ambiguous. Do not assume intent.

---

## Communication Rules

- Always respond in Spanish, without exception. This applies to plans, code comments, explanations, error messages, and any other output.
- No emojis in any response, plan, comment, or documentation.
- No bold text inside paragraphs. Bold is only allowed in titles and subtitles.
- Responses must be concise. Do not add unrequested explanations, summaries, or closing remarks.
- Do not be condescending. Do not validate decisions uncritically. If a decision has a problem, state it directly.
- Do not use filler phrases such as "great question", "certainly", "of course", or "happy to help".

---

## Planning Standards

When the user requests a plan, feature breakdown, or implementation guide, apply the following standards without exception.

The goal is that a developer could execute every step manually without ambiguity. Write as a mentor explaining to a capable but uninformed developer: precise, direct, and complete.

- Break the implementation into numbered steps. Each step describes exactly one action.
- For each step, specify: what file is affected, what layer it belongs to, what exactly must be written or modified, and why.
- If a step involves creating a new type, show its complete signature.
- If a step has a dependency on a previous step, state it explicitly.
- If a decision has more than one valid approach, present the options with their tradeoffs before recommending one.
- Do not skip steps under the assumption that they are obvious.
- Do not group multiple actions into a single step.
- Code snippets within a plan must be complete enough to be written directly, not illustrative placeholders.

---

## Architecture Rules

The project follows Clean Architecture with strict layer boundaries. These rules are non-negotiable.

### Layer Dependency Direction

```
Domain → (no dependencies)
Application → Domain
Infrastructure → Application + Domain
Presentation → Application
```

Violations to catch before writing code:

- Infrastructure types (IFormFile, HttpContext, Stripe SDK types, EF-specific attributes) must never appear in Application or Domain.
- Domain entities must not reference DTOs.
- Application interfaces must use only types defined in Application or Domain.
- If an external type needs to cross a boundary, define an abstraction in Application and map in Infrastructure.

### CQRS

- Every use case is a Command or Query. No exceptions.
- Commands mutate state and return `Result<T>` or `Unit`.
- Queries return data and must not mutate state.
- A Command or Query with no parameters is a design smell. Resolve identity (userId, basketId) in the Presentation layer from JWT claims and pass it explicitly.
- Handlers must not contain HTTP context awareness. If a handler references `IHttpContextAccessor`, that is a violation.

### Domain Services and Extensions

- Query extension methods on `IQueryable<T>` live in the Domain layer.
- These methods must not reference DTOs or Application types.

---

## Code Quality Rules

### General

- Follow SOLID principles. If a class has more than one reason to change, split it.
- No magic strings. Use constants or strongly-typed identifiers.
- No commented-out code in commits.
- No `var` for non-obvious types. Prefer explicit types when the right side does not make the type immediately clear.

### EF Core

- Never call `context.Entity.Update()` on a tracked entity. EF change tracking handles mutations automatically after `FindAsync` or `FirstOrDefaultAsync`.
- Always pass `CancellationToken` to async EF calls.
- Avoid loading entire collections when only a count or existence check is needed.

### DTOs and Mapping

- AutoMapper is not used. Mapping is explicit.
- DTO → Entity: implement a `ToEntity()` method on the DTO.
- Entity mutation from DTO: implement an `ApplyTo(Entity entity)` method on the DTO. This method must not return the entity; it mutates and returns void.
- `ApplyTo` must not touch fields managed outside of it (e.g., image URLs set by an image service before `ApplyTo` is called).
- `IFormFile` must not appear in Application DTOs. Resolve file handling in the controller and pass a neutral type such as `Stream` or a dedicated upload DTO.

### Validation

- `DataAnnotations` are not used for business validation. FluentValidation is the only validation mechanism.
- Each Command and Query that accepts input has a corresponding `AbstractValidator<T>` registered via the pipeline behavior.
- Validators live in the Application layer alongside their Command or Query.

### Error Handling

- Do not use try/catch in handlers for flow control. Use a `Result<T>` pattern or throw domain exceptions that are caught by a pipeline behavior or middleware.
- Infrastructure is responsible for catching and translating SDK-specific exceptions (e.g., `StripeException`) before they reach Application.
- Global exception handling is done through middleware or `IPipelineBehavior`, not in individual controllers.

---

## Command Execution Rules

Terminal commands must only be run when strictly necessary. Before executing any command:

1. Verify the command is not destructive or irreversible without confirmation.
2. Do not run `dotnet ef database update` or migration commands without explicit user instruction.
3. Do not install packages without confirming they belong to the correct layer.
4. Do not run multiple commands in sequence when the result of the first is uncertain.
5. Prefer reading files and analyzing code before running build or test commands.

---

## External SDK and Third-Party Integration

Any third-party SDK (payment processors, storage providers, messaging services, etc.) is confined to the Infrastructure layer. This applies universally regardless of the provider.

- No SDK-specific type may appear in Application or Presentation.
- All parsing, casting, and SDK-specific logic happens entirely in Infrastructure.
- Infrastructure maps SDK objects to Application-defined records before returning them to the caller.
- If a provider is replaced, only Infrastructure changes. Application and Domain remain untouched.
- Event-driven integrations (webhooks, callbacks): the controller validates the incoming request and dispatches a Command. No business logic lives in the controller.
- When an event carries multiple possible types, routing per type is acceptable in a single handler. Actual business logic for each type belongs in dedicated handlers or domain services.
- Before choosing or extending an integration, check whether a better-suited provider or library exists. Do not default to the current implementation if the requirement has changed.

---

## Authentication and Identity

Authentication mechanism (ASP.NET Identity, Keycloak, Auth0, custom JWT, etc.) is an infrastructure concern. The Application layer must not be coupled to any specific provider.

- Operations that are purely infrastructural with no domain state involved (session termination, cookie invalidation) do not require a Command. Keep them in the controller.
- Operations that affect domain state (token revocation persisted in the database, audit logging, user status changes) must be wrapped in a Command with an explicit `UserId` parameter resolved from claims in the controller.
- `IHttpContextAccessor` must not be injected into handlers to resolve user identity.
- If the authentication provider needs to change, the Application layer must require zero modifications.

---

## Pre-Planning Protocol

When the user requests a new feature or change, follow this sequence before producing any plan:

1. Identify which layers are affected.
2. Check if an abstraction or interface already exists for the required behavior.
3. Check available MCPs for tools that can assist (database introspection, file access, etc.).
4. Identify boundary violations the implementation could introduce and call them out before writing code.
5. Propose the minimal change that satisfies the requirement without breaking existing architecture.

## Convencion de Mapeo de DTOs

El proyecto sigue un patron hibrido optimizado para CQRS:

### Write Path (Commands)
- **DTO -> Entity**: Usar `ToEntity()` en el DTO de creacion
- **DTO -> Entity (update)**: Usar `ApplyTo(entity)` en el DTO de actualizacion
- **Entity -> DTO (response)**: Usar extension `ToDto()` en `Application/{Module}/Extensions/`

### Read Path (Queries)
- **ReadModel -> DTO**: ReadRepository usa `Expression<Func<ReadModel, DTO>>` y retorna `IQueryable<DTO>`
- **Query operations**: Extensions `Sort()`, `Filter()`, `Search()` operan sobre `IQueryable<DTO>`

### Ubicacion de archivos
| Tipo | Ubicacion |
|------|-----------|
| DTOs de Write | `Application/{Module}/DTOs/Create*Dto.cs`, `Update*Dto.cs` |
| DTOs de Read | `Application/{Module}/DTOs/*Dto.cs` |
| Extensions de mapeo | `Application/{Module}/Extensions/*Extensions.cs` |
| ReadRepository con mapeo | `Infrastructure/Repositories/ReadRepositories/*ReadRepository.cs` |

### Reglas
1. No duplicar logica de mapeo entre ToDto() y Select() inline
2. ReadRepositories retornan IQueryable<DTO>, no IQueryable<Entity>
3. Sort/Filter/Search siempre sobre IQueryable<DTO> para consistencia
4. ToDto() solo se usa en respuestas de Commands (no en Queries)

Do not propose creating new abstractions, services, or patterns unless the existing structure genuinely cannot support the requirement.

---

## Estado Actual del Proyecto

### Arquitectura Implementada

El proyecto implementa Clean Architecture con CQRS y segregacion de bases de datos:

| Componente | Estado | Descripcion |
|------------|--------|-------------|
| Clean Architecture | Implementado | Domain, Application, Infrastructure, Presentation |
| CQRS | Implementado | Commands/Queries separados con MediatR |
| Segregacion BD | Implementado | WriteDbContext + ReadDbContext separados |
| Event-Driven | Implementado | MassTransit con Outbox pattern |
| Result Pattern | Implementado | Result<T> en Application/Core |

### Modulos Implementados

| Modulo | Estado | Observaciones |
|--------|--------|---------------|
| Product | Completo | CQRS, eventos, ReadRepository |
| Order | Completo | CQRS, eventos, integracion Stripe |
| Payment (Stripe) | Completo | Webhook handler, PaymentIntent |
| SupportTicket | Completo | CQRS, ReadRepository |
| Identity | Deuda tecnica | No sigue CQRS, logica en controllers |

### Planes de Implementacion Pendientes

#### 1. MeshyAI Integration (docs/MeshyAI-Integration-Plan.md)

Plan completo para integracion de generacion 3D con MeshyAI:

| Fase | Descripcion | Estado |
|------|-------------|--------|
| Fase 1 | Extension del modelo de dominio (TPH) | Pendiente |
| Fase 2 | Infrastructure MeshyAI | Pendiente |
| Fase 3 | Webhook Handler | Pendiente |
| Fase 4 | Quotation Module | Pendiente |
| Fase 5 | Approval Flow Commands | Pendiente |
| Fase 6 | Payment Integration | Pendiente |
| Fase 7 | API Controllers | Pendiente |
| Fase 8 | Integration Events y Consumers | Pendiente |
| Fase 9-13 | Queries, DTOs, Validators, Repositories, Tests | Pendiente |
| Fase 14 | Sistema de Reglas de Cotizacion Administrables | Pendiente |

Decisiones clave del plan MeshyAI:
- Modelo TPH: Product (abstract) -> CatalogProduct, CustomProduct
- Flujo de aprobacion dual: Operador (contenido) -> Usuario (cotizacion)
- Reglas de cotizacion en BD (no appsettings.json) con soporte para reglas condicionales
- Cache de reglas con invalidacion en escritura

#### 2. Technical Debt Refactor (docs/TechnicalDebtRefactor.md)

Plan de refactorizacion de deuda tecnica identificada.

### Convenciones de Codigo

#### Estructura de Archivos por Modulo

```
Application/{Module}/
  Commands/
    {Action}Command.cs
    {Action}CommandHandler.cs
  Queries/
    {Action}Query.cs
    {Action}QueryHandler.cs
  DTOs/
    Create{Entity}Dto.cs
    Update{Entity}Dto.cs
    {Entity}Dto.cs
  Extensions/
    {Entity}Extensions.cs
  Validators/
    {Action}Validator.cs
```

#### Nomenclatura

| Tipo | Convencion | Ejemplo |
|------|------------|---------|
| Commands | Verbo + Sustantivo + Command | CreateProductCommand |
| Queries | Get + Sustantivo(s) + Query | GetProductsQuery |
| Handlers | Command/Query + Handler | CreateProductCommandHandler |
| DTOs | Create/Update/Entity + Dto | CreateProductDto, ProductDto |
| Validators | Command/Query + Validator | CreateProductValidator |
| Integration Events | Entity + Action + IntegrationEvent | ProductCreatedIntegrationEvent |
| Consumers | Event + Consumer | ProductCreatedConsumer |

#### Result Pattern

Todos los handlers retornan `Result<T>`:

```csharp
// Exito
return Result<ProductDto>.Success(dto);

// Fallo
return Result<ProductDto>.Failure("Mensaje de error");
```

### Integraciones Externas

| Servicio | SDK | Capa | Estado |
|----------|-----|------|--------|
| Stripe | Stripe.net | Infrastructure | Implementado |
| Cloudinary | CloudinaryDotNet | Infrastructure | Implementado |
| MeshyAI | HttpClient custom | Infrastructure | Planificado |

### Testing

| Tipo | Framework | Ubicacion |
|------|-----------|-----------|
| Unit Tests | xUnit + Moq | UnitTests/ |
| Integration Tests | xUnit | IntegrationTests/ |

Ejecutar tests:
```bash
dotnet test
```

### Migraciones

```bash
# Crear migracion WriteDbContext
dotnet ef migrations add {Name} -p Persistence -s API

# Crear migracion ReadDbContext
dotnet ef migrations add {Name} -p Persistence -s API -c ReadDbContext

# Aplicar migraciones (requiere confirmacion explicita)
dotnet ef database update -p Persistence -s API
```

### Variables de Entorno Requeridas

| Variable | Descripcion | Requerido |
|----------|-------------|-----------|
| ConnectionStrings__DefaultConnection | Write DB | Si |
| ConnectionStrings__ReadConnection | Read DB | Si |
| StripeSettings__SecretKey | Stripe API Key | Si |
| StripeSettings__WebhookSecret | Stripe Webhook Secret | Si |
| CloudinarySettings__CloudName | Cloudinary | Si |
| CloudinarySettings__ApiKey | Cloudinary | Si |
| CloudinarySettings__ApiSecret | Cloudinary | Si |
| MeshySettings__ApiKey | MeshyAI API Key | Cuando se implemente |