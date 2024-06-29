using Azure;
using CsvHelper;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Python.Runtime;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Backend
{
  public class Report
  {
    public static List<DataSource> dslist = new List<DataSource>();
    public static List<DataSource> returnedDsList = new List<DataSource>();
    public static List<SourceSQL> sqlList = new List<SourceSQL>();
    public static List<ActionInfo> actions = new List<ActionInfo>();
    public static List<DataSource> pyGeneratedDsList = new List<DataSource>();
    public enum ActionType
    {
      Concat = 1,
      Pivot,
      GroupBy,
      Aggregate,
      FillNullValues,
      DeleteColumns,
      GetColumns,
      Average,
      Join
    }
    public void pyInitialise()
    {
      string pythonDll = @"C:\Users\Kaushal\AppData\Local\Programs\Python\Python37\python37.dll";
      Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
      PythonEngine.Initialize();

    }

    public dynamic Concat(DataSource ds1, DataSource ds2, int pageSize, int pageNumber, ActionInfo actionInfo)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            pyscope.Set("df", actionInfo.returnedDataSource.dataFrame);
            pyscope.Set("df1", ds1.dataFrame);
            pyscope.Set("df2", ds2.dataFrame);
            int skipRow = (pageNumber - 1) * pageSize;
            pyscope.Exec("df = pd.concat([df1,df2], axis=1)");
            pyscope.Exec("df = df.fillna('-')");
            pyscope.Exec("print(df)");
            actionInfo.returnedDataSource.dataFrame = pyscope.Get("df");
            pyscope.Exec("dt = df.dtypes.to_list()");
            pyscope.Exec("columns = df.columns.tolist()");
            dynamic dt = pyscope.Get("dt");
            dynamic columns = pyscope.Get("columns");

            pyscope.Exec("shape= df.shape[1]");
            PyObject? allColumns = pyscope.Get("shape");
            int cols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));
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
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;
            actionInfo.returnedDataSource.rowData = csharpDictionary;
            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < cols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = dtypeNames[i] });
              }
            }
            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            csharpDictionary = deserializedCols;
            return csharpDictionary;
            //  where to store after concatenation?
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic Pivot(DataSource ds, string index, string columns, string values, ActionInfo actionInfo)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            dynamic df_p = pd.DataFrame();
            pyscope.Set("df", ds.dataFrame);
            pyscope.Set("df_p", df_p);
            //pyscope.Exec($"df_p=df_p.reset_index().drop_duplicates(subset=['{index}', '{columns}', '{values}'])");
            pyscope.Exec($"df_p = df.pivot(index='{index}',columns='{columns}',values='{values}')");
            pyscope.Exec("df_p = df_p.fillna('-')");
            pyscope.Exec("df_p = df_p.reset_index()");
            df_p = pyscope.Get("df_p");

            dynamic dt = df_p.dtypes.to_list();
            dynamic cols = df_p.columns.tolist();
            int allCols = df_p.shape[1];

            for (int i = 0; i < allCols; i++)
            {
              if (dt[i] == "object")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dt[i] });
              }
            }

            df_p = df_p.to_dict("records").ToString();
            //string jsonStr = df_p.ToString()!;
            string jsonDataStr = JsonConvert.SerializeObject(df_p);
            dynamic jsonData = JsonConvert.DeserializeObject(jsonDataStr)!;
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonData)!;
            return csharpDictionary;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic GroupBy(DataSource ds, string column, string operation, int pageSize, int pageNumber, ActionInfo actionInfo)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            dynamic df_gb = "";
            pyscope.Set("df", ds.dataFrame);

            pyscope.Exec("df_gb = pd.DataFrame()");
            pyscope.Set("df_gb", df_gb);
            switch (operation)
            {
              case "min":
                pyscope.Exec($"df_gb = df.groupby('{column}').min()");
                break;
              case "max":
                pyscope.Exec($"df_gb = df.groupby('{column}').max()");
                break;
              case "sum":
                pyscope.Exec($"df_gb = df.groupby('{column}').sum()");
                pyscope.Exec("print(df_gb)");
                break;
              case "mean":
                pyscope.Exec($"df_gb = df.groupby('{column}').mean()");
                break;
            }
            pyscope.Exec("df_gb = df_gb.reset_index()");
            df_gb = pyscope.Get("df_gb");

            pyscope.Exec("print(df_gb)");
            var newColumn = df_gb[column];
            df_gb[column] = newColumn;
            pyscope.Exec("print(df_gb)");
            pyscope.Set("df_gb", df_gb);
            actionInfo.returnedDataSource.dataFrame = df_gb;
            pyscope.Exec("dt = df_gb.dtypes.to_list()");
            dynamic dt = pyscope.Get("dt");
            pyscope.Exec("cols = df_gb.columns.tolist()");
            dynamic cols = pyscope.Get("cols");
            pyscope.Exec("allCols = df_gb.shape[1]");
            PyObject? allColumns = pyscope.Get("allCols");
            int allCols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));

            pyscope.Exec("total_rows = len(df_gb)");

            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            int skipRows = (pageNumber - 1) * pageSize;
            int takeRows = pageSize;
            pyscope.Exec($"page_data = df_gb.iloc[{skipRows}:{skipRows + takeRows}]");

            PyObject? pageData = pyscope.Get("page_data");
            PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

            string jsonDataStr = JsonConvert.SerializeObject(data.ToString());
            dynamic jsonData = JsonConvert.DeserializeObject(jsonDataStr)!;
            dynamic jsonDataF = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonData)!;
            actionInfo.returnedDataSource.rowData = jsonDataF;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;

            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < allCols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dtypeNames[i] });
              }
            }

            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            jsonDataF = deserializedCols;

            return jsonDataF;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic Aggregate(DataSource ds, string operations, string columns, int pageSize, int pageNumber, ActionInfo actionInfo)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            dynamic df_agg = "";
            pyscope.Set("pd", pd);
            pyscope.Exec("df_agg = pd.DataFrame()");
            pyscope.Set("df", ds.dataFrame);
            pyscope.Set("df_agg", df_agg);
            if (columns != null)
            {
              pyscope.Exec($"df_agg = df.agg({{'{columns}':'{operations}'}})");
              pyscope.Exec("df_agg=df_agg.fillna('-')");
              pyscope.Exec("df_agg=df_agg.to_frame()");
              pyscope.Exec("df_agg=df_agg.transpose()");
            }
            else
            {
              pyscope.Exec($"df_agg = df.agg(['{operations}'])");
              pyscope.Exec("df_agg=df_agg.fillna('-')");
            }
            df_agg = pyscope.Get("df_agg");
            actionInfo.returnedDataSource.dataFrame = df_agg;
            pyscope.Exec("dt = df_agg.dtypes.to_list()");
            dynamic dt = pyscope.Get("dt");
            pyscope.Exec("cols = df_agg.columns.tolist()");
            dynamic cols = pyscope.Get("cols");
            pyscope.Exec("allCols = df_agg.shape[1]");
            PyObject? allColumns = pyscope.Get("allCols");
            int allCols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));
            pyscope.Exec("total_rows = len(df_agg)");

            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            int skipRows = (pageNumber - 1) * pageSize;
            int takeRows = pageSize;
            pyscope.Exec($"page_data = df_agg.iloc[{skipRows}:{skipRows + takeRows}]");

            PyObject? pageData = pyscope.Get("page_data");
            PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

            dynamic jsonData = JsonConvert.DeserializeObject(data.ToString()!)!;
            string jsonDataStr = JsonConvert.SerializeObject(jsonData);
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
            actionInfo.returnedDataSource.rowData = csharpDictionary;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;

            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < allCols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dtypeNames[i] });
              }
            }
            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            csharpDictionary = deserializedCols;
            return csharpDictionary;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic FillNa(DataSource ds, ActionInfo actionInfo, int pageSize, int pageNumber)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            dynamic df_fna = "";
            pyscope.Set("df", ds.dataFrame);
            pyscope.Set("df_fna", df_fna);
            pyscope.Exec($"df_fna = df.replace(to_replace='null', method='pad')");
            actionInfo.returnedDataSource.dataFrame = pyscope.Get("df_fna");
            pyscope.Exec("dt = df_fna.dtypes.to_list()");
            pyscope.Exec("columns = df_fna.columns.tolist()");
            dynamic dt = pyscope.Get("dt");
            dynamic columns = pyscope.Get("columns");

            pyscope.Exec("total_rows = len(df)");
            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            int skipRows = (pageNumber - 1) * pageSize;
            int takeRows = pageSize;
            pyscope.Exec($"page_data = df_fna.iloc[{skipRows}:{skipRows + takeRows}]");

            PyObject? pageData = pyscope.Get("page_data");
            PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

            string jsonStr = data.ToString()!;
            dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
            string jsonDataStr = JsonConvert.SerializeObject(jsonData);
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;
            actionInfo.returnedDataSource.rowData = csharpDictionary;
            pyscope.Exec("allCols = df_fna.shape[1]");
            PyObject? allColumns = pyscope.Get("allCols");
            int cols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));

            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < cols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(columns[i]), colType = dtypeNames[i] });
              }
            }

            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            csharpDictionary = deserializedCols;
            return csharpDictionary;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic DeleteColumns(DataSource ds, ActionInfo actionInfo, string[] columns, int pageSize, int pageNumber)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            pyscope.Set("df", ds.dataFrame);
            dynamic df_delCols = "";
            string serializedColumnsToDrop = JsonConvert.SerializeObject(columns);
            Console.WriteLine(serializedColumnsToDrop);
            pyscope.Exec($"df_delCols = df.drop(columns={serializedColumnsToDrop})");
            actionInfo.returnedDataSource.dataFrame = pyscope.Get("df_delCols");
            ds.dataFrame = pyscope.Get("df_delCols");
            pyscope.Exec("dt = df_delCols.dtypes.to_list()");
            pyscope.Exec("columns = df_delCols.columns.tolist()");
            dynamic dt = pyscope.Get("dt");
            dynamic cols = pyscope.Get("columns");
            pyscope.Exec("shape= df_delCols.shape[1]");
            PyObject? allColumns = pyscope.Get("shape");
            int allCols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));
            pyscope.Exec("total_rows = len(df_delCols)");
            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            int skipRows = (pageNumber - 1) * pageSize;
            int takeRows = pageSize;
            pyscope.Exec($"page_data = df_delCols.iloc[{skipRows}:{skipRows + takeRows}]");

            PyObject? pageData = pyscope.Get("page_data");
            PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

            string jsonStr = data.ToString()!;
            dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
            string jsonDataStr = JsonConvert.SerializeObject(jsonData);
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;
            actionInfo.returnedDataSource.rowData = csharpDictionary;

            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < allCols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dtypeNames[i] });
              }
            }

            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            csharpDictionary = deserializedCols;

            return csharpDictionary;

          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }


    }

    public void GetCols(DataSource ds, string[] columns, int pageSize, int pageNumber, ActionInfo actionInfo)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            string colsData1 = columns[0];
            string colsData2 = columns[1];

            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            pyscope.Set("df", ds.dataFrame);
            dynamic df_getCols1 = "";
            dynamic df_getCols2 = "";
            //pyscope.Exec($"unique_categories = df['{colsData1}'].unique()");
            //dynamic unique_categories = pyscope.Get("unique_categories");
            //pyscope.Exec($"filtered_data = df.loc[df['{colsData1}'] == '{unique_categories[0]}', '{colsData2}'");
            //dynamic filtered_data = pyscope.Get("filtered_data");

            pyscope.Exec($"df_getCols1 = df['{colsData1}']");
            pyscope.Exec($"df_getCols1 = df_getCols1.replace(to_replace='null', method='pad')");

            pyscope.Exec($"df_getCols2 = df['{colsData2}']");
            pyscope.Exec($"df_getCols2 = df_getCols2.replace(to_replace='null', method='pad')");
            pyscope.Exec("total_rows = len(df_getCols1)");
            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

            int skipRows = (pageNumber - 1) * pageSize;
            int takeRows = pageSize;
            pyscope.Exec($"page_data1 = df_getCols1.iloc[{skipRows}:{skipRows + takeRows}]");
            pyscope.Exec($"page_data2 = df_getCols2.iloc[{skipRows}:{skipRows + takeRows}]");

            PyObject? pageData1 = pyscope.Get("page_data1");
            PyObject? pageData2 = pyscope.Get("page_data2");
            PyObject? data1 = pageData1.InvokeMethod("to_list");
            PyObject? data2 = pageData2.InvokeMethod("to_list");

            string jsonStr = data1.ToString()!;
            dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
            string jsonDataStr = JsonConvert.SerializeObject(jsonData);
            var csharpDictionary = JsonConvert.DeserializeObject<List<object>>(jsonDataStr)!;
            actionInfo.returnedDataSource.getColsRowDataOne = csharpDictionary;

            string jsonStr2 = data2.ToString()!;
            dynamic jsonData2 = JsonConvert.DeserializeObject(jsonStr2)!;
            string jsonDataStr2 = JsonConvert.SerializeObject(jsonData2);
            var csharpDictionary2 = JsonConvert.DeserializeObject<List<object>>(jsonDataStr2)!;
            actionInfo.returnedDataSource.getColsRowDataTwo = csharpDictionary2;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public dynamic AverageOfOneCol(DataSource ds, ActionInfo actionInfo)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            pyscope.Set("df", ds.dataFrame);
            dynamic df_avg = "";
            pyscope.Set("df_avg", df_avg);
            //pyscope.Exec($"df_avg = df[['{column}']].mean()");
            pyscope.Exec($"df_avg = df.mean()");
            //pyscope.Exec($"print(df[['{column}']].mean())");
            pyscope.Exec("df_avg=df_avg.to_frame()");
            pyscope.Exec("df_avg=df_avg.transpose()");
            df_avg = pyscope.Get("df_avg");
            actionInfo.returnedDataSource.dataFrame = df_avg;

            pyscope.Exec("total_rows = len(df_avg)");
            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            pyscope.Exec("dt = df_avg.dtypes.to_list()");
            pyscope.Exec("columns = df_avg.columns.tolist()");
            dynamic dt = pyscope.Get("dt");
            dynamic cols = pyscope.Get("columns");
            pyscope.Exec("shape= df_avg.shape[1]");
            PyObject? allColumns = pyscope.Get("shape");
            int allCols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));
            PyObject? data = df_avg.InvokeMethod("to_dict", new PyString("records"));
            dynamic jsonData = JsonConvert.DeserializeObject(data.ToString()!)!;
            string jsonDataStr = JsonConvert.SerializeObject(jsonData);
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
            actionInfo.returnedDataSource.rowData = csharpDictionary;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;

            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < allCols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dtypeNames[i] });
              }
            }

            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            csharpDictionary = deserializedCols;

            return csharpDictionary;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    //public dynamic InnerJoin(DataSource ds1, DataSource ds2, ActionInfo actionInfo, string left, string right, string left_on, string right_on, int pageSize, int pageNumber)
    //{
    //  pyInitialise();
    //  try
    //  {
    //    using (var pyscope = Py.CreateScope())
    //    {
    //      using(Py.GIL())
    //      {
    //        dynamic pd = Py.Import("pandas");
    //        pyscope.Set("pd", pd);
    //        left = left == ds1.sourceName ? ds1.sourceName : ds2.sourceName!;
    //        right = left == ds2.sourceName ? ds1!.sourceName : ds2!.sourceName;
    //        pyscope.Set("dfLeft", left == ds1.sourceName? ds1.dataFrame : ds2.dataFrame);
    //        pyscope.Set("dfRight", right == ds1.sourceName ? ds1.dataFrame : ds2.dataFrame);
    //        dynamic df_ij = "";
    //        pyscope.Exec($"df_ij=pd.merge(left=dfLeft, right=dfRight, left_on='{left_on}', right_on='{right_on}')");

    //        pyscope.Exec("total_rows = len(df_ij)");
    //        PyObject? totalRowsObj = pyscope.Get("total_rows");
    //        int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
    //        pyscope.Exec("dt = df_ij.dtypes.to_list()");
    //        pyscope.Exec("columns = df_ij.columns.tolist()");
    //        dynamic dt = pyscope.Get("dt");
    //        dynamic cols = pyscope.Get("columns");
    //        pyscope.Exec("shape= df_ij.shape[1]");
    //        PyObject? allColumns = pyscope.Get("shape");
    //        int allCols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));

    //        int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);
    //        int skipRows = (pageNumber - 1) * pageSize;
    //        int takeRows = pageSize;
    //        pyscope.Exec($"page_data = df_ij.iloc[{skipRows}:{skipRows + takeRows}]");

    //        PyObject? pageData = pyscope.Get("page_data");
    //        PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

    //        string jsonStr = data.ToString()!;
    //        dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
    //        string jsonDataStr = JsonConvert.SerializeObject(jsonData);
    //        var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
    //        actionInfo.returnedDataSource.sourceFileLength = totalRows;
    //        actionInfo.returnedDataSource.rowData = csharpDictionary;

    //        string[] dtypes = dt.ToString().Split(',');
    //        string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

    //        for (int i = 0; i < allCols; i++)
    //        {
    //          if (dtypeNames[i] == "O")
    //          {
    //            actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
    //          }
    //          else
    //          {
    //            actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dtypeNames[i] });
    //          }
    //        }

    //        string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
    //        Console.WriteLine(json);
    //        dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
    //        csharpDictionary = deserializedCols;

    //        return csharpDictionary;
    //      }
    //    }
    //  }
    //  finally
    //  {
    //    PythonEngine.Shutdown();
    //  }
    //}

    public dynamic Join(DataSource ds1, DataSource ds2, ActionInfo actionInfo, string left, string right, string left_on, string right_on, string how, int pageSize, int pageNumber)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            pyscope.Set("pd", pd);
            left = left == ds1.sourceName ? ds1.sourceName : ds2.sourceName!;
            right = left == ds2.sourceName ? ds1!.sourceName! : ds2!.sourceName!;
            pyscope.Set("dfLeft", left == ds1.sourceName ? ds1.dataFrame : ds2.dataFrame);
            pyscope.Set("dfRight", right == ds1.sourceName ? ds1.dataFrame : ds2.dataFrame);
            dynamic df_ij = "";
            pyscope.Exec($"df_ij=pd.merge(left=dfLeft, right=dfRight, left_on='{left_on}', right_on='{right_on}', how='{how}')");
            pyscope.Exec($"df_ij = df_ij.fillna('null')");
            df_ij = pyscope.Get("df_ij");
            actionInfo.returnedDataSource.dataFrame = df_ij;
            pyscope.Exec("total_rows = len(df_ij)");
            PyObject? totalRowsObj = pyscope.Get("total_rows");
            int totalRows = Convert.ToInt32(totalRowsObj.AsManagedObject(typeof(int)));
            pyscope.Exec("dt = df_ij.dtypes.to_list()");
            pyscope.Exec("columns = df_ij.columns.tolist()");
            dynamic dt = pyscope.Get("dt");
            dynamic cols = pyscope.Get("columns");
            pyscope.Exec("shape= df_ij.shape[1]");
            PyObject? allColumns = pyscope.Get("shape");
            int allCols = Convert.ToInt32(allColumns.AsManagedObject(typeof(int)));

            int totalPages = (int)Math.Ceiling((double)totalRows / pageSize);
            int skipRows = (pageNumber - 1) * pageSize;
            int takeRows = pageSize;
            pyscope.Exec($"page_data = df_ij.iloc[{skipRows}:{skipRows + takeRows}]");

            PyObject? pageData = pyscope.Get("page_data");
            PyObject? data = pageData.InvokeMethod("to_dict", new PyString("records"));

            string jsonStr = data.ToString()!;
            dynamic jsonData = JsonConvert.DeserializeObject(jsonStr)!;
            string jsonDataStr = JsonConvert.SerializeObject(jsonData);
            var csharpDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonDataStr)!;
            actionInfo.returnedDataSource.sourceFileLength = totalRows;
            actionInfo.returnedDataSource.rowData = csharpDictionary;

            string[] dtypes = dt.ToString().Split(',');
            string[] dtypeNames = Array.ConvertAll(dtypes, dtype => System.Text.RegularExpressions.Regex.Match(dtype, @"\'(\w+)\'").Groups[1].Value);

            for (int i = 0; i < allCols; i++)
            {
              if (dtypeNames[i] == "O")
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = "string" });
              }
              else
              {
                actionInfo.returnedDataSource.cols.Add(new ColData { colName = Convert.ToString(cols[i]), colType = dtypeNames[i] });
              }
            }

            string json = JsonConvert.SerializeObject(actionInfo.returnedDataSource.cols);
            Console.WriteLine(json);
            dynamic deserializedCols = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)!;
            csharpDictionary = deserializedCols;

            return csharpDictionary;
          }
        }
      }
      finally
      {
        PythonEngine.Shutdown();
      }
    }

    public byte[] toCSV(dynamic df, string name)
    {
      pyInitialise();
      try
      {
        using (var pyscope = Py.CreateScope())
        {
          using (Py.GIL())
          {
            dynamic pd = Py.Import("pandas");
            dynamic io = Py.Import("io");
            dynamic csv = Py.Import("csv");
            pyscope.Set("pd", pd);
            pyscope.Set("io", io);
            pyscope.Set("csv", csv);
            pyscope.Set("df", df);
            pyscope.Exec($"data = pd.DataFrame(df)");
            dynamic data = pyscope.Get("data");
            //string path = @$"D:\Languages\Angular\angularAPICall\csharp\savedDataSources\{name}.csv";
            name = name.Replace("/", "");
            pyscope.Exec(@$"data.to_csv('D:\\Languages\\Angular\\angularAPICall\\csharp\\savedDataSources\\{name}.csv')");
            string csvFilePath = @$"D:\\Languages\\Angular\\angularAPICall\\csharp\\savedDataSources\\{name}.csv";
            byte[] csvFileBytes = File.ReadAllBytes(csvFilePath);

            return csvFileBytes;
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
