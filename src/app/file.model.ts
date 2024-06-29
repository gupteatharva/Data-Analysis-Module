import { MatTableDataSource } from "@angular/material/table";

export class FileDetails {
    constructor(public fileId: number,
        public finalData: Object[],
        public dataSource: MatTableDataSource<any>,
        public headers: string[],
        public totalRows: number,
        public sourceName: string,
        public fileName: string,
        public actionSourceName: string,
        public actionName: string,
        public dataType: string[],
        public sourceType: string,
        public tableName: string
        ) { }
}

