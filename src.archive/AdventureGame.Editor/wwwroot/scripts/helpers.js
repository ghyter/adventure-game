window.blazorDownloadFile = (fileName, contentType, base64Data) => {
    const a = document.createElement('a');
    a.download = fileName;
    a.href = `data:${contentType};base64,${base64Data}`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};


window.indexedDbInterop = (function () {
    function openDb(dbName, version, storeName) {
        return new Promise(function (resolve, reject) {
            const req = indexedDB.open(dbName, version);
            req.onupgradeneeded = function (e) {
                const db = e.target.result;
                if (!db.objectStoreNames.contains(storeName)) {
                    db.createObjectStore(storeName, { keyPath: "id" });
                }
            };
            req.onsuccess = function (e) {
                resolve(e.target.result);
            };
            req.onerror = function (e) {
                reject(e.target.error);
            };
        });
    }

    function txRequest(db, storeName, mode, callback) {
        return new Promise(function (resolve, reject) {
            const tx = db.transaction([storeName], mode);
            const store = tx.objectStore(storeName);
            const req = callback(store);
            req.onsuccess = function (e) { resolve(e.target.result); };
            req.onerror = function (e) { reject(e.target.error); };
        });
    }

    return {
        ensureDb: function (dbName, version, storeName) {
            return openDb(dbName, version, storeName).then(db => {
                db.close();
                return true;
            });
        },

        put: function (dbName, version, storeName, id, json) {
            return openDb(dbName, version, storeName).then(db =>
                txRequest(db, storeName, "readwrite", store => store.put({ id: id, json: json }))
                    .finally(() => db.close())
                    .then(() => json)
            );
        },

        get: function (dbName, version, storeName, id) {
            return openDb(dbName, version, storeName).then(db =>
                txRequest(db, storeName, "readonly", store => store.get(id))
                    .finally(() => db.close())
            ).then(record => record ? record.json : null);
        },

        getAll: function (dbName, version, storeName) {
            return openDb(dbName, version, storeName).then(db =>
                txRequest(db, storeName, "readonly", store => store.getAll())
                    .finally(() => db.close())
            ).then(records => (records || []).map(r => r.json));
        },

        remove: function (dbName, version, storeName, id) {
            return openDb(dbName, version, storeName).then(db =>
                txRequest(db, storeName, "readwrite", store => store.delete(id))
                    .finally(() => db.close())
            );
        }
    };
})();