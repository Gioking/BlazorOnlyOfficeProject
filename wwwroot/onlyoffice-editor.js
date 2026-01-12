// OnlyOffice Document Editor Integration
// Questo file gestisce l'integrazione con OnlyOffice Document Server
let docEditor = null;

window.onlyOfficeEditor = {
    init: function (config) {
        console.log('Inizializzazione OnlyOffice Editor con configurazione:', config);

        if (docEditor) {
            console.log('Distruzione editor esistente');
            docEditor.destroyEditor();
            docEditor = null;
        }

        try {
            const editorConfig = {
                documentType: config.documentType || "word",
                document: {
                    fileType: config.document?.fileType || config.fileType || "docx",
                    key: config.document?.key || config.key,
                    title: config.document?.title || config.title,
                    url: config.document?.url || config.documentUrl,
                    permissions: {
                        comment: true,
                        download: true,
                        edit: true,
                        fillForms: true,
                        modifyFilter: true,
                        modifyContentControl: true,
                        review: true,
                        print: true
                    }
                },
                editorConfig: {
                    mode: config.editorConfig?.mode || config.mode || "edit",
                    lang: "it",
                    callbackUrl: config.editorConfig?.callbackUrl || config.callbackUrl,
                    user: {
                        id: config.editorConfig?.user?.id || config.userId || "user1",
                        name: config.editorConfig?.user?.name || config.userName || "User"
                    },
                    customization: {
                        autosave: true,
                        forcesave: true,
                        commentAuthorOnly: false,
                        comments: true,
                        compactHeader: false,
                        compactToolbar: false,
                        hideRightMenu: false,
                        hideRulers: false,
                        toolbarNoTabs: false,
                        zoom: 100
                    }
                },
                width: "100%",
                height: "600px",
                events: {
                    onDocumentStateChange: function (event) {
                        console.log('Stato documento cambiato:', event);
                    },
                    onDocumentReady: function () {
                        console.log('Documento pronto per la modifica');
                    },
                    onError: function (event) {
                        console.error('Errore OnlyOffice Editor:', event);
                    },
                    onWarning: function (event) {
                        console.warn('Warning OnlyOffice Editor:', event);
                    },
                    onInfo: function (event) {
                        console.info('Info OnlyOffice Editor:', event);
                    }
                }
            };

            // *** PARTE CRITICA: Aggiungi il token JWT se presente ***
            if (config.token) {
                editorConfig.token = config.token;
                console.log('Token JWT aggiunto alla configurazione');
            } else {
                console.warn('Nessun token JWT fornito - OnlyOffice potrebbe rifiutare la connessione');
            }

            console.log('Configurazione finale editor:', editorConfig);

            docEditor = new DocsAPI.DocEditor("onlyoffice-editor", editorConfig);

            console.log('OnlyOffice Editor inizializzato con successo');
            return true;
        } catch (error) {
            console.error('Errore durante l\'inizializzazione di OnlyOffice Editor:', error);
            return false;
        }
    },

    destroy: function () {
        if (docEditor) {
            console.log('Distruzione OnlyOffice Editor');
            docEditor.destroyEditor();
            docEditor = null;
        }
    },

    requestSave: function () {
        if (docEditor) {
            console.log('Richiesta salvataggio documento');
            docEditor.processSaveResult(true);
        }
    }
};

console.log('Script onlyoffice-editor.js caricato correttamente');