# AccountServiceProvider

En gRPC-baserad .NET-tjänst som hanterar konton, lösenord och e-postlogik för användare.

## Funktioner

- Skapa konto
- Validera inloggning
- Bekräfta konton och e-post
- Hämta eller uppdatera kontodata
- Generera återställningstokens

## Sekvensdiagram – Skapa konto

```mermaid
sequenceDiagram
    participant Client (AuthServiceProvider)
    participant AccountService (gRPC)
    participant IdentityUserManager
    participant Database

    Client->>AccountService: CreateAccount(request)
    AccountService->>IdentityUserManager: CreateAsync(user, password)
    IdentityUserManager->>Database: INSERT user
    Database-->>IdentityUserManager: result
    IdentityUserManager-->>AccountService: IdentityResult
    AccountService-->>Client: CreateAccountReply
```

## Projektstruktur

```
Presentation/
├── Services/
├── Data/
├── Migrations/
├── Protos/
└── Program.cs
```

## Tester

Testprojektet använder:
- `xUnit`
- `Moq`
- `Grpc.Core.Testing`

### Testfall
- ✔️ Skapa konto
- ❌ Validera felaktigt lösenord
- ❌ Användare finns inte

##  Kom igång

```bash
dotnet restore
dotnet ef database update
dotnet run
```

## 📄 Protobuf

Se `Protos/account.proto` för detaljerad tjänstdefinition.
