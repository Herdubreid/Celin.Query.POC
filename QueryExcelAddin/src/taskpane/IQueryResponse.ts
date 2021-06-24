export default interface IQueryResponse {
  Title: string;
  Columns: any;
  Environment: string;
  Submitted: Date;
  Summary: {
    records: number;
    moreRecords: boolean;
  };
  Data: [][];
  Count: number;
  Demo: boolean;
  Error: string;
}
