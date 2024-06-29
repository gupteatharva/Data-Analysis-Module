namespace WebApplication1.Backend
{
  public class DataSource
  {
    public static string? retString;
    public static int objc = 0;

    public DataSource()
    {
      //objc++;
    }

    public void DataSourceCreated()
    {
      objc++;
    }

    public enum SourceType
    {
      CSV = 1,
      API,
      SQL
    }
    public SourceType sourcetype = new SourceType();
    public string? objname;

    public string? varName;
    public string? sourceName;
    //public sourceAPI? sourceapi;
    public SourceCSV? sourcecsv;
    public SourceAPI? sourceapi;
    public SourceSQL? sourcesql;
    //public List<Dictionary<string, object>>? rowData;
    public dynamic? rowData;
    public int? sourceFileLength;
    public dynamic? dataFrame;
    public dynamic? getColsRowDataOne;
    public dynamic? getColsRowDataTwo;
    public List<ColData> cols = new List<ColData>();
    public dynamic? deserializedCols;
    public string? actionDataSourceName;
    public bool? savedDataSource = false;
  }
}
