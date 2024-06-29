using Newtonsoft.Json;
using Python.Runtime;
using System;
using WebApplication1.Models;

namespace WebApplication1.Backend
{
  public class ReadScript
  {
    public void pyInitialise()
    {
      string pythonDll = @"C:\Users\Kaushal\AppData\Local\Programs\Python\Python37\python37.dll";
      Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
      PythonEngine.Initialize();

    }
    public void getVariablesFromScript(string fileName, string filePath)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            string code = System.IO.File.ReadAllText(filePath);
            pyscope.Exec(code);
            dynamic variable1 = pyscope.Get("pyDsList");
            foreach (var item in variable1)
            {
              DataSource ds = new DataSource();
              ds.dataFrame = item["df"];
              ds.sourceName = item["sourceName"].ToString();
              dynamic columns = item["columns"];
              ds.actionDataSourceName = item["actionDataSourceName"];
              dynamic keysObject = columns.GetAttr("keys").Invoke();
              dynamic valuesObject = columns.GetAttr("values").Invoke();
              
              List<object> keys = new List<object>();
              foreach (dynamic key in keysObject)
              {
                keys.Add(key);
              }
              List<object> values = new List<object>();
              foreach (dynamic value in valuesObject)
              {
                values.Add(value);
              }

              for (int idx = 0; idx < keys.Count; idx++)
              {
                string colName = keys[idx].ToString()!;
                string colType = values[idx].ToString()!;
                ds.cols.Add(new ColData { colName = colName, colType = colType });
              }
              string json = JsonConvert.SerializeObject(ds.cols);
              Console.WriteLine(json);
              dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
              ds.deserializedCols = deserializedCols;

              pyscope.Set("df", ds.dataFrame);
              if (item["actionName"] == "GroupBy")
              {
                pyscope.Exec("df = df.reset_index()");
                ds.dataFrame = pyscope.Get("df");
              }
              pyscope.Exec("df = df.fillna('null')");
              pyscope.Exec("total_rows = len(df)");
              PyObject? totalRowsObj = pyscope.Get("total_rows");
              int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));

              pyscope.Exec($"page_data = df.iloc[0:25]");
              PyObject? pageData = pyscope.Get("page_data");
              PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));
              string jsonStr = data.ToString()!;
              dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
              string jsonDataStr = JsonConvert.SerializeObject(jsonData);
              var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
              ds.sourceFileLength = totalRows;
              if (item["sourceType"] == "CSV")
              {
                ds.sourcetype = DataSource.SourceType.CSV;
                ds.sourcecsv = new SourceCSV();
                ds.sourcecsv!.fileLength = totalRows;
                ds.sourcecsv.fileName = ds.sourceName+".csv";
              }
              ds.rowData = csharpDictionary;
              Report.dslist.Add(ds);
            }
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }
  }
}
