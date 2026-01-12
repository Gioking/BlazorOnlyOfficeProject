// OnlyOffice Document Editor Integration
let docEditor = null;

window.onlyOfficeEditor = {
    init: function (config) {
        console.log('=== CONFIG RICEVUTA DAL SERVER ===');
        console.log(JSON.stringify(config, null, 2));

        if (docEditor) {
            console.log('Distruzione editor esistente');
            docEditor.destroyEditor();
            docEditor = null;
        }

        try {
            // Costruisci la config nel formato corretto per OnlyOffice
            const editorConfig = {
                documentType: config.documentType || "word",
                document: {
                    fileType: config.document?.fileType || "docx",
                    key: config.document?.key || "",
                    title: config.document?.title || "",
                    url: config.document?.url || ""
                },
                editorConfig: {
                    mode: config.editorConfig?.mode || "edit",
                    lang: "it",
                    callbackUrl: config.editorConfig?.callbackUrl || "",
                    user: {
                        id: config.editorConfig?.user?.id || "user1",
                        name: config.editorConfig?.user?.name || "User"
                    },
                    customization: {
                        autosave: true,
                        forcesave: true
                    }
                },
                token: config.token,
                width: "100%",
                height: "800px",
                events: {
                    onDocumentStateChange: function (event) {
                        console.log('Stato documento cambiato:', event);
                    },
                    onDocumentReady: function () {
                        console.log('Documento pronto');
                    },
                    onError: function (event) {
                        console.error('Errore OnlyOffice:', event);
                    }
                }
            };

            console.log('=== CONFIG FINALE PER ONLYOFFICE ===');
            console.log(JSON.stringify(editorConfig, null, 2));

            docEditor = new DocsAPI.DocEditor("onlyoffice-editor", editorConfig);

            console.log('OnlyOffice Editor inizializzato');
            return true;
        } catch (error) {
            console.error('Errore inizializzazione:', error);
            return false;
        }
    },

    destroy: function () {
        if (docEditor) {
            console.log('Distruzione editor');
            docEditor.destroyEditor();
            docEditor = null;
        }
    }
};

console.log('Script onlyoffice-editor.js caricato');