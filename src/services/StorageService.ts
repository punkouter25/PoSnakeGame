import { TableClient, TableServiceClient } from "@azure/data-tables";
import { StorageConfigFactory } from "../config/StorageConfig";

export class StorageService {
    private tableClient: TableClient;

    constructor(tableName: string) {
        const config = StorageConfigFactory.getConfig();
        this.tableClient = TableClient.fromConnectionString(
            config.connectionString,
            tableName,
            {
                allowInsecureConnection: process.env.NODE_ENV !== 'production'
            }
        );
    }

    // Add your table operations methods here
}
