# Blazor OnlyOffice Project

Applicazione Blazor .NET 8.0 con integrazione OnlyOffice Document Editor per la gestione di documenti Word.

## 📋 Caratteristiche

- ✅ **Template Blazor Standard** - Tutte le pagine di default del template preservate
- ✅ **Gestione Documenti** - CRUD completo per documenti Word (.docx)
- ✅ **API RESTful** - Backend API per operazioni sui documenti
- ✅ **Interfaccia Intuitiva** - UI moderna con Bootstrap
- ✅ **Archiviazione Locale** - Documenti salvati localmente in `wwwroot/documents`

## 🚀 Funzionalità

### Document Editor Page (`/document-editor`)

L'applicazione include una pagina dedicata per la gestione dei documenti con le seguenti funzionalità:

1. **➕ Crea Nuovo Documento**
   - Genera automaticamente un file .docx vuoto
   - Naming automatico con timestamp

2. **📤 Carica Documento**
   - Upload di documenti Word esistenti (.docx)
   - Limite dimensione: 10MB

3. **📖 Apri Documento**
   - Visualizza e modifica documenti esistenti
   - Placeholder per integrazione OnlyOffice

4. **💾 Scarica Documento**
   - Download diretto dei documenti
   - Formato .docx nativo

5. **🗑️ Elimina Documento**
   - Rimozione documenti con conferma
   - Pulizia sicura del filesystem

## 🛠️ API Endpoints

Il backend fornisce i seguenti endpoint RESTful:

| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/api/documents` | Lista tutti i documenti |
| GET | `/api/documents/{filename}` | Download documento |
| POST | `/api/documents/create` | Crea nuovo documento |
| POST | `/api/documents/upload` | Carica documento esistente |
| POST | `/api/documents/save` | Salva documento |
| DELETE | `/api/documents/{filename}` | Elimina documento |

## 📁 Struttura Progetto

```
BlazorOnlyOfficeProject/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── Counter.razor (template default)
│   │   ├── Home.razor (template default)
│   │   ├── Weather.razor (template default)
│   │   └── DocumentEditor.razor (✨ nuova pagina)
│   └── App.razor
├── Controllers/
│   └── DocumentsController.cs
├── wwwroot/
│   ├── documents/ (documenti generati/caricati)
│   └── ...
├── Program.cs
└── BlazorOnlyOfficeProject.csproj
```

## 🔧 Configurazione e Avvio

### Prerequisiti

- .NET 8.0 SDK o superiore
- Browser moderno (Chrome, Firefox, Edge)

### Installazione

```bash
# Clone del repository
git clone <repository-url>
cd BlazorOnlyOfficeProject

# Restore dei package (se necessario)
dotnet restore

# Build del progetto
dotnet build

# Esecuzione
dotnet run
```

### Accesso all'Applicazione

Dopo l'avvio, l'applicazione sarà disponibile su:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

Naviga su `/document-editor` per accedere alla pagina di gestione documenti.

## 📝 Note sull'Integrazione OnlyOffice

L'applicazione include un placeholder per l'integrazione completa di OnlyOffice Document Editor.

### Per Integrazione Completa

Per utilizzare l'editor OnlyOffice completo, è necessario:

1. **Installare OnlyOffice Document Server**
   - Self-hosted: https://github.com/ONLYOFFICE/DocumentServer
   - Docker: `docker run -i -t -d -p 80:80 onlyoffice/documentserver`
   - Cloud: OnlyOffice Cloud Service

2. **Configurare l'Endpoint**
   - Aggiungere l'URL del Document Server in `appsettings.json`
   - Aggiornare `DocumentEditor.razor` con la configurazione API

3. **Implementare Callback**
   - Callback URL per il salvataggio automatico
   - Gestione degli eventi di editing

### Workflow Attuale

Attualmente l'applicazione supporta:
- ✅ Creazione documenti vuoti (.docx)
- ✅ Upload di documenti esistenti
- ✅ Download per editing offline
- ✅ Gestione completa del ciclo di vita dei documenti

Per editare i documenti:
1. Scarica il documento tramite il pulsante "💾 Scarica"
2. Modifica con Word, LibreOffice o altro editor
3. Ricarica il documento tramite "📤 Carica Documento"

## 🎨 Tecnologie Utilizzate

- **ASP.NET Core 8.0** - Framework backend
- **Blazor Server** - UI interattiva
- **Bootstrap 5** - Styling e componenti UI
- **C# 12** - Linguaggio di programmazione
- **Open XML** - Formato documenti Word

## 📦 Dipendenze

Il progetto utilizza solo le dipendenze standard di .NET 8.0 SDK:
- Microsoft.NET.Sdk.Web
- AspNetCore.Runtime
- Blazor Components

## 🔒 Sicurezza

- Validazione input sui nomi file
- Limite dimensione upload (10MB)
- Path traversal protection
- Antiforgery token abilitato

## 📄 Licenza

Questo progetto è fornito come esempio educativo.

## 👥 Contributi

Per contribuire al progetto:
1. Fork del repository
2. Crea un branch feature
3. Commit delle modifiche
4. Push e Pull Request

## 📞 Supporto

Per domande o problemi, apri una issue nel repository.

---

**Sviluppato con ❤️ usando Blazor e .NET 8.0**
