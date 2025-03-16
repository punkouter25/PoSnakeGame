interface IStorageConfig {
    connectionString: string;
    tableEndpoint: string;
}

export class StorageConfigFactory {
    public static getConfig(): IStorageConfig {
        if (process.env.NODE_ENV === 'production') {
            return {
                connectionString: process.env.AZURE_STORAGE_CONNECTION_STRING || '',
                tableEndpoint: process.env.AZURE_STORAGE_TABLE_ENDPOINT || ''
            };
        } else {
            return {
                connectionString: 'UseDevelopmentStorage=true',
                tableEndpoint: 'http://127.0.0.1:10002/devstoreaccount1'
            };
        }
    }
}
