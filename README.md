## Password Manager

Ett konsolbaserat program i C# som lagrar och krypterar lösenord lokalt. Programmet använder en klient- och serverfil för att skydda känslig information med hjälp av AES-kryptering.

### Funktioner
- Skapa krypterade klient- och serverfiler
- Lägga till, visa och ta bort lösenord
- Automatisk lösenordsgenerering
- Läsbar meny och hjälpfunktion
- All data sparas i JSON-format och krypteras


#### Tillgängliga kommandon

| Kommando     | Syntax                                                       | Beskrivning                                                  |
|--------------|--------------------------------------------------------------|--------------------------------------------------------------|
| `init`       | `init <clientPath> <serverPath>`                             | Skapar nya krypterade klient- och serverfiler.               |
| `create`     | `create <clientPath> <serverPath>`                           | Återskapar klientfil från servern med masterlösenord.        |
| `set`        | `set <clientPath> <serverPath> <property>`                   | Lägger till ett lösenord för en viss property.               |
| `set -g`     | `set <clientPath> <serverPath> <property> -g`                | Lägger till ett slumpgenererat lösenord.                     |
| `get`        | `get <clientPath> <serverPath>`                              | Visar alla sparade property-namn.                            |
| `get <prop>` | `get <clientPath> <serverPath> <property>`                   | Hämtar lösenord för en specifik property.                    |
| `delete`     | `delete <clientPath> <serverPath> <property>`                | Tar bort ett sparat lösenord.                                |
| `secret`     | `secret <clientPath>`                                        | Visar den hemliga nyckeln från klientfilen.                  |
| `help`       | `help`                                                       | Visar denna hjälptext.                                       |

#### Exempel

```bash
dotnet run init client.json server.json
dotnet run set client.json server.json Gmail
dotnet run get client.json server.json
dotnet run delete client.json server.json Gmail
```

###  Struktur

```plaintext
Passwordmanager/
│
├── Program.cs               # Innehåller logiken för alla kommandon
├── Kryptering.cs           # Krypteringsmetoder (AES)
├── Passwordfiler.csproj    # Projektfil
└── bin/ & obj/             # Kompilerade filer (ingår ej i Git)
```