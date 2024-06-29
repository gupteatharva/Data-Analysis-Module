using Python.Runtime;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Apache.Arrow;
using Apache.Arrow.Ipc;
using MessagePack;
using Microsoft.AspNetCore.Hosting.Server;
//using System.Text.Json;

namespace WebApplication1.Backend
{
  public class RowsColumns
  {
    public dynamic RowColumnGenerator(DataSource ds, int pageSize, int pageNumber, List<ColData> headers)
    {
      string pythondll = @"c:\users\kaushal\appdata\local\programs\python\python37\python37.dll";
      Environment.SetEnvironmentVariable("pythonnet_pydll", pythondll);
      PythonEngine.Initialize();
      //dynamic data_array_str = "";
      dynamic csharpDictionary = "";
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            if (ds.sourcetype.ToString() == "CSV")
            {
              dynamic pd = Py.Import("pandas");
              dynamic pa = Py.Import("pyarrow");
              dynamic fa = Py.Import("feather");
              dynamic msgpack = Py.Import("msgpack"); 
              dynamic base64 = Py.Import("base64"); 
              dynamic re = Py.Import("re");
              dynamic io = Py.Import("io");
              pyscope.Set("io", io);
              pyscope.Set("pd", pd);
              pyscope.Set("fa", fa);
              pyscope.Set("base64", base64);
              pyscope.Set("msgpack", msgpack);
              pyscope.Set("pa", pa);
              pyscope.Set("df", ds.dataFrame);
              pyscope.Set("fp", ds.sourcecsv?.filePath);
              pyscope.Set("length", ds.sourcecsv?.fileLength);
              pyscope.Exec("df = pd.read_csv(fp,encoding='iso-8859-1')");
              pyscope.Exec("df = df.fillna('null')");
              pyscope.Exec("print(df)");
              ds.dataFrame = pyscope.Get("df");
              //pyscope.Exec("table = pa.Table.from_pandas(df)");
              //dynamic table = pyscope.Get("table");
              //pyscope.Exec("buffer = io.BytesIO()");
              //pyscope.Exec("pa.feather.write_feather(table, buffer)");
              //pyscope.Exec("serialized_bytes = buffer.getvalue()");
              //dynamic serialized_bytes = pyscope.Get("serialized_bytes");
              //byte[] bytes = serialized_bytes;
              //pyscope.Exec("fa.write_dataframe(df, 'mydata.feather', compression=None)");
              //byte[] featherBinaryData = File.ReadAllBytes("mydata.feather");


              pyscope.Exec("total_rows = len(df)");
              PyObject? totalRowsObj = pyscope.Get("total_rows");
              int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
              int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

              int skipRows = (pageNumber - 1) * pageSize;
              int takeRows = pageSize;
              pyscope.Exec($"page_data = df.iloc[{skipRows}:{skipRows + takeRows}]");

              PyObject? pageData = pyscope.Get("page_data");
              PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

              string jsonStr = data.ToString()!;
              dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
              string jsonDataStr = JsonConvert.SerializeObject(jsonData);
              csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
              ds.rowData = csharpDictionary;

              ds.sourcecsv!.fileLength = totalRows;
              pyscope.Exec("dt = df.dtypes.to_list()");
              //dynamic dt = ds.dataFrame.dtypes.to_list();
              dynamic dt = pyscope.Get("dt");
              string[] dtypes = dt.ToString().Split(',');
              string[] dtypeNames = System.Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

              pyscope.Exec("columns = df.columns.tolist()");
              //dynamic columns = ds.dataFrame.columns.tolist();
              dynamic columns = pyscope.Get("columns");
              pyscope.Exec("shape = df.shape[1]");
              PyObject? allColumns = pyscope.Get("shape");
              int cols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));

              for (int i = 0; i < cols; i++)
              {
                if (dtypeNames[i] == "O")
                {
                  ds.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = "string" });
                }
                else
                {
                  ds.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = dtypeNames[i] });
                }
              }
              string json = JsonConvert.SerializeObject(ds.cols);
              Console.WriteLine(json);
              dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
              csharpDictionary = deserializedCols;

            }
            else if (ds.sourcetype.ToString() == "API")
            {
              dynamic pd = Py.Import("pandas");
              dynamic req = Py.Import("requests");
              dynamic json = Py.Import("json");
              pyscope.Set("pd", pd);
              pyscope.Set("req", req);
              pyscope.Set("json", json);
              pyscope.Set("df", ds.dataFrame);
              dynamic data = "";
              pyscope.Set("data", data);
              pyscope.Exec($"data = req.get('{ds.sourceapi!.APIurl}').json()");
              //pyscope.Exec($"data = data.json()");
              //pyscope.Exec($"data2 = json.dumps(data, indent=2)");
              //dynamic data2 = pyscope.Get("data2");
              //Console.WriteLine(data2);
              pyscope.Exec("df = pd.json_normalize(data, max_level=10)");
              //pyscope.Exec("df = pd.DataFrame.from_dict(data, orient='index')");
              ds.dataFrame = pyscope.Get("df");
              Console.WriteLine(ds.dataFrame);

              pyscope.Exec("total_rows = len(df)");
              PyObject? totalRowsObj = pyscope.Get("total_rows");
              int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
              int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

              // Get the desired page of data
              int skipRows = (pageNumber - 1) * pageSize;
              int takeRows = pageSize;
              pyscope.Exec($"page_data = df.iloc[{skipRows}:{skipRows + takeRows}]");

              PyObject? pageData = pyscope.Get("page_data");
              PyObject? apiData = pageData.InvokeMethod("to_dict", new PyString("records"));

              //string jsonStr = apiData.InvokeMethod("to_json", new PyString("records")).ToString()!;
              string jsonStr = apiData.ToString()!;
              dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
              string jsonDataStr = JsonConvert.SerializeObject(jsonData);
              csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
              ds.rowData = csharpDictionary;
              Console.WriteLine(ds.rowData[0]);
              //pyscope.Exec("length = len(df.index)");
              //PyObject? pyFileLength = pyscope.Get("length");
              //int fileLength = Convert.ToInt32(pyFileLength.AsManagedObject(typeof(int)));
              ds.sourceapi!.dataLength = totalRows;
              pyscope.Exec("dt = df.dtypes.to_list()");
              pyscope.Exec("columns = df.columns.tolist()");
              dynamic dt = pyscope.Get("dt");

              dynamic columns = pyscope.Get("columns");
              int cols = ds.dataFrame.shape[1];

              for (int i = 0; i < cols; i++)
              {
                if (dt[i] == "object")
                {
                  ds.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = "string" });
                }
                else
                {
                  ds.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = dt[i] });
                }
              }
            }
            else if (ds.sourcetype.ToString() == "SQL")
            {
              dynamic pd = Py.Import("pandas");
              dynamic pyodbc = Py.Import("pyodbc");
              pyscope.Set("pd", pd);
              pyscope.Set("pyodbc", pyodbc);
              string? server = ds.sourcesql!.server;
              string? database = ds.sourcesql!.database;
              string? username = ds.sourcesql!.username;
              string? password = ds.sourcesql!.password;
              string conn_string = "'DRIVER={ODBC Driver 17 for SQL Server};'" + $"'SERVER={server};'" + $"'DATABASE={database};'" + $"'UID={username};'" + $"'PWD={password};'" + "'Trusted_Connection=yes;'";
              pyscope.Exec($"conn = pyodbc.connect({conn_string})");
              //dynamic conn = pyscope.Get("conn");
              pyscope.Exec($"sql_query = pd.read_sql_query('select * from {ds.sourcesql.tableName}', conn)");
              pyscope.Exec($"df= pd.DataFrame(sql_query)");
              pyscope.Exec("print(df)");
              pyscope.Exec("df = df.fillna('null')");
              ds.dataFrame = pyscope.Get("df");


              pyscope.Exec("total_rows = len(df)");
              PyObject? totalRowsObj = pyscope.Get("total_rows");
              int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
              int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

              int skipRows = (pageNumber - 1) * pageSize;
              int takeRows = pageSize;
              pyscope.Exec($"page_data = df.iloc[{skipRows}:{skipRows + takeRows}]");

              PyObject? pageData = pyscope.Get("page_data");
              PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

              string jsonStr = data.ToString()!;
              dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
              string jsonDataStr = JsonConvert.SerializeObject(jsonData);
              csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
              ds.rowData = csharpDictionary;
              ds.sourcesql!.fileLength = totalRows;
              pyscope.Exec("dt = df.dtypes.to_list()");
              //dynamic dt = ds.dataFrame.dtypes.to_list();
              dynamic dt = pyscope.Get("dt");
              string[] dtypes = dt.ToString().Split(',');
              string[] dtypeNames = System.Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

              pyscope.Exec("columns = df.columns.tolist()");
              //dynamic columns = ds.dataFrame.columns.tolist();
              dynamic columns = pyscope.Get("columns");
              pyscope.Exec("shape = df.shape[1]");
              PyObject? allColumns = pyscope.Get("shape");
              int cols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));

              for (int i = 0; i < cols; i++)
              {
                if (dtypeNames[i] == "O")
                {
                  ds.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = "string" });
                }
                else
                {
                  ds.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = dtypeNames[i] });
                }
              }
              string json = JsonConvert.SerializeObject(ds.cols);
              Console.WriteLine(json);
              dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
              csharpDictionary = deserializedCols;
            }
          }
        }
        return csharpDictionary;
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic SQLConnection(string server, string database, string username, string password)
    {
      string pythondll = @"c:\users\kaushal\appdata\local\programs\python\python37\python37.dll";
      Environment.SetEnvironmentVariable("pythonnet_pydll", pythondll);
      PythonEngine.Initialize();
      dynamic toReturn = "";
      try
      {
        using(var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            dynamic pyodbc = Py.Import("pyodbc");
            pyscope.Set("pd", pd);
            pyscope.Set("pyodbc", pyodbc);

            //string conn_string = $"'DRIVER={{ODBC Driver 17 for SQL Server}};''SERVER={server};''DATABASE={database};''UID={username};''PWD={password};''Trusted_Connection=yes'";
            string conn_string = "'DRIVER={ODBC Driver 17 for SQL Server};'" + $"'SERVER={server};'" + $"'DATABASE={database};'" + $"'UID={username};'" + $"'PWD={password};'" + "'Trusted_Connection=yes;'";
            pyscope.Exec($"conn = pyodbc.connect({conn_string})");
            dynamic conn = pyscope.Get("conn");
            pyscope.Exec("cursor = conn.cursor()");
            pyscope.Exec("cursor.execute('select * from sys.tables')");
            pyscope.Exec("results = cursor.fetchall()");
            dynamic results = pyscope.Get("results");
            string[] table_names = new string[0];
            //pyscope.Exec("db_names=[result[0] for result in results]");
            foreach(var item in results)
            {
              System.Array.Resize(ref table_names, table_names.Length + 1);
              table_names[table_names.Length - 1] = item[0].ToString();
              //Console.WriteLine(db_names);
            }

            pyscope.Exec("cursor.close()");
            pyscope.Exec("conn.close()");
            toReturn = table_names;
          }
        }
        return toReturn;
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }
  }
}
