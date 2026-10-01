window.CKEditorInterop = (() => {

    const editors = {};

    class MyUploadAdapter {
        constructor(loader, uploadUrl) {
            this.loader = loader;
            this.uploadUrl = uploadUrl;
        }

        upload() {
            return this.loader.file
                .then(file => {
                    const maxSize = 7 * 1024 * 1024;
                    if (file.size > maxSize) {
                        return Promise.reject('حجم تصویر نباید بیشتر از 7 مگابایت باشد.');
                    }
                    return new Promise((resolve, reject) => {
                        this._initRequest();
                        this._initListeners(resolve, reject, file);
                        this._sendRequest(file);
                    });
                });
        }

        abort() {
            if (this.xhr) {
                this.xhr.abort();
            }
        }

        _initRequest() {
            const xhr = this.xhr = new XMLHttpRequest();
            xhr.open('POST', this.uploadUrl, true);
            xhr.responseType = 'json';
        }

        _initListeners(resolve, reject, file) {
            const xhr = this.xhr;
            const loader = this.loader;
            const genericErrorText = `Couldn't upload file: ${file.name}.`;

            xhr.addEventListener('error', () => reject(genericErrorText));
            xhr.addEventListener('abort', () => reject());
            xhr.addEventListener('load', () => {
                const response = xhr.response;
                console.log("UPLOAD RESPONSE:", response);
                if (!response || response.error) {
                    return reject(response?.error?.message || genericErrorText);
                }
                resolve({
                    default: response.url
                });
            });

            if (xhr.upload) {
                xhr.upload.addEventListener('progress', evt => {
                    if (evt.lengthComputable) {
                        loader.uploadTotal = evt.total;
                        loader.uploaded = evt.loaded;
                    }
                });
            }
        }

        _sendRequest(file) {
            const data = new FormData();
            data.append('upload', file);
            this.xhr.send(data);
        }
    }

    return {
        init: async function (id, uploadImageUrl, dotNetReference, initialValue) {

            const textarea = document.getElementById(id);
            if (!textarea) {
                console.error(`Textarea with id ${id} not found`);
                return;
            }

            try {
                const editor = await ClassicEditor.create(textarea, {
                    toolbar: {
                        items: [
                            'heading',
                            'bold',
                            'italic',
                            'underline',
                            'link',
                            'bulletedList',
                            'numberedList',
                            'alignment',
                            '|',
                            'fontSize',
                            'fontFamily',
                            'fontColor',
                            '|',
                            'imageUpload',
                            'imageInsert',
                            'insertTable',
                            'removeFormat',
                            'undo',
                            'redo'
                        ]
                    },
                    language: 'fa',
                    image: {
                        toolbar: [
                            'imageTextAlternative',
                            'imageStyle:full',
                            'imageStyle:side'
                        ]
                    },
                    table: {
                        contentToolbar: [
                            'tableColumn',
                            'tableRow',
                            'mergeTableCells'
                        ]
                    },
                    extraPlugins: [
                        function (editor) {
                            editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
                                return new MyUploadAdapter(loader, uploadImageUrl);
                            };
                        }
                    ]
                });

                editors[id] = editor;

                // تنظیم مقدار اولیه - این قسمت مهم است
                if (initialValue && initialValue !== '') {
                    editor.setData(initialValue);
                }

                //监听数据变化
                editor.model.document.on('change:data', () => {
                    let data = editor.getData();
                    const el = document.createElement('div');
                    el.innerHTML = data;
                    if (el.innerText.trim() === '')
                        data = null;

                    if (dotNetReference) {
                        dotNetReference.invokeMethodAsync('EditorDataChanged', data);
                    }
                });

            } catch (error) {
                console.error('CKEditor initialization error:', error);
            }
        },

        setData: function (id, data) {
            const editor = editors[id];
            if (editor && data !== undefined) {
                editor.setData(data || '');
            }
        },

        getData: function (id) {
            const editor = editors[id];
            if (editor) {
                return editor.getData();
            }
            return '';
        },

        destroy: async function (id) {
            if (editors[id]) {
                try {
                    await editors[id].destroy();
                    delete editors[id];
                } catch (error) {
                    console.log('Error destroying editor:', error);
                }
            }
        }
    };
})();