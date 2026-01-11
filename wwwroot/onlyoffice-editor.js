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
            docEditor = new DocsAPI.DocEditor("onlyoffice-editor", {
                documentType: config.documentType || "word",
                document: {
                    fileType: config.fileType || "docx",
                    key: config.key,
                    title: config.title,
                    url: config.documentUrl,
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
                    mode: config.mode || "edit",
                    lang: "it",
                    callbackUrl: config.callbackUrl,
                    user: {
                        id: config.userId || "user1",
                        name: config.userName || "User"
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
            });

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
