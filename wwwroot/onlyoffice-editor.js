// OnlyOffice Document Editor Integration

let docEditor = null;

window.onlyOfficeEditor = {
    init: function (config) {
        console.log('Initializing OnlyOffice Editor with config:', config);

        if (docEditor) {
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
                        console.log('Document state changed:', event);
                    },
                    onDocumentReady: function () {
                        console.log('Document is ready');
                    },
                    onError: function (event) {
                        console.error('OnlyOffice Editor Error:', event);
                    },
                    onWarning: function (event) {
                        console.warn('OnlyOffice Editor Warning:', event);
                    },
                    onInfo: function (event) {
                        console.info('OnlyOffice Editor Info:', event);
                    }
                }
            });

            console.log('OnlyOffice Editor initialized successfully');
            return true;
        } catch (error) {
            console.error('Failed to initialize OnlyOffice Editor:', error);
            return false;
        }
    },

    destroy: function () {
        if (docEditor) {
            console.log('Destroying OnlyOffice Editor');
            docEditor.destroyEditor();
            docEditor = null;
        }
    },

    requestSave: function () {
        if (docEditor) {
            docEditor.processSaveResult(true);
        }
    }
};
