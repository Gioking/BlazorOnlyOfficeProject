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

## 📝 Integrazione OnlyOffice Document Editor

L'applicazione è completamente integrata con OnlyOffice Document Server per l'editing online di documenti Word.

### ✅ Funzionalità Implementate

- ✅ **Editor OnlyOffice Completo** - Editing Word in browser
- ✅ **Auto-Save** - Salvataggio automatico tramite callback
- ✅ **Document Versioning** - Chiavi uniche per versioning
- ✅ **Gestione Errori** - Messaggi di troubleshooting dettagliati
- ✅ **Caricamento Dinamico** - Script API OnlyOffice caricati runtime
- ✅ **Localizzazione Italiana** - UI e editor in italiano

### 🚀 Setup OnlyOffice Document Server

#### Opzione 1: Docker (Consigliata)

```bash
# Avvia OnlyOffice Document Server
docker run -i -t -d -p 8080:80 --name onlyoffice-server onlyoffice/documentserver

# Verifica che sia in esecuzione
docker ps | grep onlyoffice

# Il server sarà disponibile su http://localhost:8080
```

#### Opzione 2: Installazione Manuale

- Linux: https://github.com/ONLYOFFICE/DocumentServer
- Windows: https://www.onlyoffice.com/download-docs.aspx
- Cloud: OnlyOffice Cloud Service

### ⚙️ Configurazione

La configurazione è già presente in `appsettings.json`:

```json
{
  "OnlyOffice": {
    "DocumentServerUrl": "http://localhost:8080",
    "ApiUrl": "http://localhost:8080/web-apps/apps/api/documents/api.js"
  }
}
```

**Nota:** Se il tuo Document Server è su una porta diversa, modifica queste impostazioni.

### 🎯 Come Usare l'Editor

1. **Avvia OnlyOffice Document Server** (vedi sopra)
2. **Avvia l'applicazione Blazor**:
   ```bash
   dotnet run
   ```
3. **Naviga su** `http://localhost:5000/document-editor`
4. **Crea o carica un documento**
5. **Clicca sul documento** nella lista per aprirlo nell'editor
6. **Modifica direttamente nel browser** - Le modifiche vengono salvate automaticamente!

### 🔧 Troubleshooting

**Errore: "Non è possibile connettersi al Document Server"**

Verifica:
1. Il container Docker è attivo: `docker ps | grep onlyoffice`
2. Il server risponde: apri `http://localhost:8080` nel browser
3. La porta 8080 non sia bloccata dal firewall
4. La configurazione in `appsettings.json` sia corretta

**Editor non si carica:**

1. Apri la console del browser (F12)
2. Verifica che non ci siano errori di CORS
3. Controlla che lo script API venga caricato correttamente
4. Riavvia il container OnlyOffice se necessario

### 💡 Funzionalità Avanzate

L'editor OnlyOffice supporta:
- **Formattazione Completa** - Stili, font, colori, tabelle
- **Immagini e Media** - Inserimento immagini, grafici, forme
- **Collaborazione** - Editing multi-utente (configurazione aggiuntiva richiesta)
- **Revisioni e Commenti** - Tracciamento modifiche
- **Esportazione** - PDF, DOCX e altri formati

### 📋 Endpoint API OnlyOffice

| Endpoint | Descrizione |
|----------|-------------|
| `GET /api/documents/config/{filename}` | Ottieni configurazione editor |
| `POST /api/documents/callback` | Callback per salvataggio automatico |

## 🎨 Tecnologie Utilizzate

- **ASP.NET Core 8.0** - Framework backend
- **Blazor Server** - UI interattiva
- **OnlyOffice Document Server** - Editor documenti online
- **Bootstrap 5** - Styling e componenti UI
- **C# 12** - Linguaggio di programmazione
- **JavaScript** - Integrazione OnlyOffice API
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
