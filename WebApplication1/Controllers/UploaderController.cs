using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Backend;
//using Newtonsoft.Json;
using Apache.Arrow;
using Apache.Arrow.Types;
using Apache.Arrow.Ipc;
using Python.Runtime;

namespace WebApplication1.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class UploaderController : ControllerBase
  {
    private readonly FileContext _context;
    public UploaderController(FileContext context)
    {
      _context = context;
    }
    [HttpPost]
    [Route("UploadFile")]
    public async Task<IActionResult> UploadFile([FromForm] FileModel fileModel)
    {
      int fileId;
      dynamic fileDetail;
      try
      {
        var file = Request.Form.Files[0];
        var fileName = "";
        if (file.Length > 0)
        {
          fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
        }

        string path = Path.Combine(@"D:\\Languages\\Angular\\angularAPICall\\csharp\\MyFiles\\", fileName);


        using (Stream stream = new FileStream(path, FileMode.Create))
        {
          fileModel?.File?.CopyTo(stream);
        }
        fileDetail = new FileDetail { FileName = fileName, FilePath = path, ReceivedDate = DateTime.Now };
        _context.FileDetails.Add(fileDetail);
        await _context.SaveChangesAsync();
        fileId = fileDetail.Id;

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return BadRequest();

      }
      return CreatedAtAction("UploadFile", new { id = fileId }, fileDetail);
    }

    [HttpGet("GetFile/{id}/{pageSize}/{pageNumber}")]

    public async Task<IActionResult> GetFile(int id, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource ds = new DataSource();
        RowsColumns rowsColumns = new RowsColumns();
        ds.sourcetype = DataSource.SourceType.CSV;
        ds.sourcecsv = new SourceCSV();

        List<ColData>? headers = new List<ColData>();
        string FilePath = "";
        using (var context = new FileContext(new DbContextOptions<FileContext>()))
        {
          var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == id);
          if (file == null)
          {
            return NotFound();
          }
          ds.sourcecsv.filePath = file.FilePath!;
          ds.sourcecsv.fileName = file.FileName!;
          ds.sourceName = ds.sourcecsv.fileName.Split(".")[0];
          ds.actionDataSourceName = ds.sourceName;
          FilePath = file.FilePath!;
        };
        ds.DataSourceCreated();
        dynamic dfToReturn = rowsColumns.RowColumnGenerator(ds, pageSize, pageNumber, headers);
        //ds.deserializedCols = dfToReturn;

        //List<Field> fields = ds.cols.Select(col =>
        //{
        //  switch (col.colType!.ToString())
        //  {
        //    case "int64":
        //      return new Field(col.colName!.ToString(), new Int64Type(), true);
        //    case "float64":
        //      return new Field(col.colName!.ToString(), new FloatType(), true);
        //    case "string":
        //      return new Field(col.colName!.ToString(), new StringType(), true);
        //    default:
        //      return new Field(col.colName!.ToString(), new StringType(), true);
        //  }
        //}).ToList();



        //List<IArrowArray> arrays = new List<IArrowArray>();
        //foreach (Dictionary<string, object> row in ds.rowData!)
        //{
        //  List<object> values = row.Values.ToList();
        //  for (int i = 0; i < fields.Count; i++)
        //  {
        //    if (values[i] == null)
        //    {
        //      // Create a null array with the same length as other arrays
        //      var nullArray = new BooleanArray.Builder().Resize(arrays.Last().Length).Build();
        //      arrays.Add(nullArray);
        //    }
        //    else
        //    {
        //      switch (fields[i].DataType.TypeId)
        //      {
        //        case ArrowTypeId.Int64:
        //          arrays.Add(new Int64Array.Builder().Append(Convert.ToInt64(values[i])).Build());
        //          break;
        //        case ArrowTypeId.Float:
        //          arrays.Add(new FloatArray.Builder().Append(Convert.ToSingle(values[i])).Build());
        //          break;
        //        case ArrowTypeId.String:
        //          arrays.Add(new StringArray.Builder().Append(Convert.ToString(values[i])).Build());
        //          break;
        //        default:
        //          arrays.Add(new StringArray.Builder().Append(Convert.ToString(values[i])).Build());
        //          break;
        //      }
        //    }
        //  }
        //}
        //Schema schema = new Schema(fields, metadata: null);
        //var recordBatch = new RecordBatch(schema, arrays, arrays[0].Length);
        //using var stream = new MemoryStream();
        //var arrowStreamWriter = new ArrowStreamWriter(stream, schema);
        //arrowStreamWriter.WriteRecordBatch(recordBatch);
        //arrowStreamWriter.WriteEnd();
        //byte[] arrowData = stream.ToArray();
        ds.objname = "obj" + DataSource.objc;
        ds.varName = "var" + DataSource.objc;
        Report.dslist.Add(ds);


        return Ok(new { header = dfToReturn, data = ds.rowData, totalRows = ds.sourcecsv.fileLength, sourcename = ds.sourceName, fileName = ds.sourcecsv.fileName, actionSourceName = ds.sourceName, sourceType = ds.sourcetype.ToString() });
        //return Ok(dfToReturn);

      }
      catch (Exception ex)
      {
        return NotFound(ex.Message);
      }

    }

    [HttpPost("PostApiData/{pageSize}/{pageNumber}")]
    public IActionResult PostApiData([FromBody] string url, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource ds = new DataSource();
        RowsColumns rowsColumns = new RowsColumns();
        ds.sourcetype = DataSource.SourceType.API;
        ds.sourceapi = new SourceAPI();

        List<ColData>? headers = new List<ColData>();
        ds.sourceapi.APIurl = url;
        // how to set sourcename
        ds.DataSourceCreated();
        dynamic dfToReturn = rowsColumns.RowColumnGenerator(ds, pageSize, pageNumber, headers);
        foreach (var col in ds.cols)
        {
          headers.Add(new ColData { colName = col.colName!.ToString(), colType = col.colType!.ToString() });
        }
        ds.objname = "obj" + DataSource.objc;
        ds.varName = "var" + DataSource.objc;
        Report.dslist.Add(ds);

        return Ok(new { header = headers, data = dfToReturn[0], totalRows = ds.sourceapi.dataLength, sourcename = ds.sourceName });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost("CreateNewDataSource")]
    public IActionResult CreateNewDataSource([FromBody] Dictionary<string, object> reqBody)
    {
      try
      {
        DataSource dataSource = new DataSource();
        RowsColumns rowsColumns = new RowsColumns();
        dataSource.sourcetype = DataSource.SourceType.CSV;
        dataSource.sourcecsv = new SourceCSV();
        List<string>? headers = new List<string>();
        string? actionSourceName = reqBody["actionSourceName"].ToString();
        string? sourceName = reqBody["sourceName"].ToString();
        string? tableName = "";
        if (reqBody["tableName"].ToString() != "null")
        {
          tableName = reqBody["tableName"].ToString();
        }
        else
        {
          tableName = null;
        }
        var requiredActionData = Report.actions.FirstOrDefault(action => action.actionSourceName == actionSourceName);
        dataSource.actionDataSourceName = requiredActionData!.actionSourceName;
        dataSource.dataFrame = requiredActionData!.returnedDataSource.dataFrame;
        dataSource.rowData = requiredActionData!.returnedDataSource.rowData;
        dataSource.cols = requiredActionData.returnedDataSource.cols;
        dataSource.deserializedCols = requiredActionData!.returnedDataSource.deserializedCols;
        dataSource.sourceName = sourceName;
        dataSource.sourcecsv.fileName = reqBody["fileName"].ToString();
        dataSource.sourcecsv.fileLength = reqBody["totalRows"];
        dataSource.savedDataSource = true;
        //foreach (var col in dataSource.cols)
        //{
        //  headers.Add(col.colName!.ToString()!);
        //}
        dataSource.DataSourceCreated();
        dataSource.objname = "obj" + DataSource.objc;
        dataSource.varName = "var" + DataSource.objc;
        Report.dslist.Add(dataSource);

        return Ok(new { header = dataSource.deserializedCols, data = dataSource.rowData, sourcename = dataSource.sourceName, actionSourceName = requiredActionData.actionSourceName, actionName = reqBody["actionName"].ToString(), fileId = reqBody["fileId"], totalRows = dataSource.sourcecsv.fileLength, fileName = dataSource.sourcecsv.fileName, savedDataSource = dataSource.savedDataSource, tableName = tableName });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost("download/csv")]
    public IActionResult DownloadCSV([FromBody] Dictionary<string, object> reqBody)
    {
      try
      {
        //DataSource dataSource = new DataSource();
        //RowsColumns rowsColumns = new RowsColumns();
        //dataSource.sourcetype = DataSource.SourceType.CSV;
        //dataSource.sourcecsv = new SourceCSV();
        string? actionSourceName = reqBody["actionSourceName"].ToString();
        //int id = fileId;
        string? sourceName = reqBody["sourceName"].ToString();
        var requiredActionData = Report.actions.FirstOrDefault(action => action.actionSourceName == actionSourceName);
        //var requiredDataSource = Report.dslist.FirstOrDefault(ds => ds.sourceName == sourceName);
        dynamic dataFrame = requiredActionData!.returnedDataSource.dataFrame!;
        Report report = new Report();
        byte[] csvFileBytes = report.toCSV(dataFrame, actionSourceName);
        return File(csvFileBytes, "text/csv", "data.csv");
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }

    }

    [HttpGet("getSqlConn/{server}/{database}/{username}/{password}/{pageSize}/{pageNumber}")]
    public IActionResult getSqlConnection(string server, string database, string username, string password, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        //DataSource ds = new DataSource();
        RowsColumns rowsColumns = new RowsColumns();
        //ds.sourcetype = DataSource.SourceType.SQL;
        //ds.sourcesql = new SourceSQL();
        //ds.sourcesql.server = server;
        //ds.sourcesql.database = database;
        //ds.sourcesql.username = username;
        //ds.sourcesql.password = password;
        Report.sqlList.Add(new SourceSQL { server = server, database = database, username = username, password = password });
        List<ColData>? headers = new List<ColData>();
        //dynamic dfToReturn = rowsColumns.RowColumnGenerator(ds, pageSize, pageNumber, headers);
        dynamic test = rowsColumns.SQLConnection(server, database, username, password);
        //Report.dslist.Add(new DataSource { sourcetype = DataSource.SourceType.SQL, sourcesql = ds.sourcesql});
        //sourcename will be table name
        return Ok(new { tables = test });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("getSqlData/{database}/{tableName}/{pageSize}/{pageNumber}")]
    public IActionResult getSqlData(string database, string tableName, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource ds = new DataSource();
        ds.sourcetype = DataSource.SourceType.SQL;
        ds.sourcesql = Report.sqlList.FirstOrDefault(x => x.database == database);
        ds.sourcesql!.tableName = tableName;
        ds.sourceName = tableName;
        ds.sourcesql.fileName = database;
        RowsColumns rowsColumns = new RowsColumns();
        List<ColData>? headers = new List<ColData>();
        dynamic dfToReturn = rowsColumns.RowColumnGenerator(ds, pageSize, pageNumber, headers);
        Report.dslist.Add(ds);
        return Ok(new { header = dfToReturn, data = ds.rowData, totalRows = ds.sourcesql.fileLength, sourcename = ds.sourceName, fileName = ds.sourcesql.fileName, actionSourceName = ds.sourceName, sourceType = ds.sourcetype.ToString() });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }


  }
}

