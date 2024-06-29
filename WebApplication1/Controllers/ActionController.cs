using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Dynamic;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using WebApplication1.Backend;
using WebApplication1.Data;
using WebApplication1.Models;
using static WebApplication1.Backend.DataSource;

namespace WebApplication1.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ActionController : ControllerBase
  {
    [HttpGet("concat/{fileId1}/{fileId2}/{pageSize}/{pageNumber}")]
    public async Task<IActionResult> ActionConcat(int fileId1, int fileId2, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource ds1;
        DataSource ds2;
        ActionInfo actionInfo = new ActionInfo();
        List<string>? headers = new List<string>();
        using (var context = new FileContext(new DbContextOptions<FileContext>()))
        {
          var file1 = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId1);
          if (file1 == null)
          {
            return NotFound();
          }
          ds1 = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file1?.FileName)!;
          var file2 = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId2);
          if (file2 == null)
          {
            return NotFound();
          }
          ds2 = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file2?.FileName)!;
        }

        Report report = new Report();
        dynamic dfToReturn = report.Concat(ds1, ds2, pageSize, pageNumber, actionInfo);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;
        Report.returnedDsList.Add(actionInfo.returnedDataSource);
        actionInfo.ActionCreated();
        Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Concat, dataSrc1 = ds1.sourceName, dataSrc2 = ds2.sourceName, returnedDataSource = actionInfo.returnedDataSource, actionSourceName = Report.ActionType.Concat.ToString() + "-" + ds1.sourceName + "_" + ds2.sourceName });
        // need to store this concatted data to db?

        return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, actionName = Report.ActionType.Concat.ToString(), actionSourceName = Report.ActionType.Concat.ToString() + "-" + ds1.sourceName + "_" + ds2.sourceName });
        //sourcetype
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("pivot/{fileId}")]
    public async Task<IActionResult> ActionPivot(int fileId, [FromQuery] string index, [FromQuery] string columns, [FromQuery] string values)
    {
      try
      {
        DataSource dataSource = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        List<string>? headers = new List<string>();
        using (var context = new FileContext(new DbContextOptions<FileContext>()))
        {
          var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
          if (file == null)
          {
            return NotFound();
          }
          dataSource = Report.dslist.FirstOrDefault(f => f.sourcecsv!.fileName == file!.FileName)!;
        }
        Report report = new Report();
        actionInfo.returnedDataSource.dataFrame = report.Pivot(dataSource, index, columns, values, actionInfo);
        foreach (var col in actionInfo.returnedDataSource.cols)
        {
          headers.Add(col.colName!.ToString()!);
        }
        //Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Pivot, dataSrc1 = ds1.sourceName, dataSrc2 = ds2.sourceName, returnedDataSource = actionInfo.returnedDataSource });

        return Ok(new { header = headers, data = actionInfo.returnedDataSource.dataFrame });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("groupby/{fileId}/{pageSize}/{pageNumber}/{sourceType}/{tableName}")]
    public async Task<IActionResult> ActionGroupBy(int fileId, [FromQuery] string actionSourceName, [FromQuery] string column, [FromQuery] string operation, string sourceType, string tableName, int pageSize = 25, int pageNumber = 1)
    {
      try
        {
        DataSource dataSource = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        List<string>? headers = new List<string>();
        Console.WriteLine($"fileId: {fileId}, sourceType: {sourceType}, tableName: {tableName}");
        Console.WriteLine(sourceType == DataSource.SourceType.SQL.ToString());
        Console.WriteLine(tableName != "null");
        Console.WriteLine(fileId == 0);
        if (fileId == 0 && sourceType != "SQL" && tableName == "null")
        {
          //foreach(var item in  Report.dslist)
          //{
          //  if(item.actionDataSourceName == actionSourceName)
          //  {
          //    dataSource = item;
          //    break;
          //  }
          //}
          dataSource = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName)!;
          Console.WriteLine(dataSource);
        }
        
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          //foreach (var item in Report.dslist)
          //{
          //  if (item?.sourcesql?.tableName == tableName)
          //  {
          //    dataSource = item;
          //    break;
          //  }
          //}
          dataSource = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName)!;
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
              return NotFound();
            }
            //foreach (var item in Report.dslist)
            //{
            //  if (item?.sourcecsv?.fileName == file?.FileName)
            //  {
            //    dataSource = item!;
            //    break;
            //  }
            //}
            dataSource = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file?.FileName)!;
          }
        }
        Report report = new Report();
        dynamic dfToReturn = report.GroupBy(dataSource, column, operation, pageSize, pageNumber, actionInfo);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;
        dynamic extraVariables = new ExpandoObject();
        extraVariables.column = column;
        extraVariables.operation = operation;
        actionInfo.ActionCreated();
        actionInfo.actionCreated += 1;
        //dynamic x = Report.actions.Find(x => x.actionName == Report.ActionType.GroupBy)!;
        //if(x != null)
        //{
        //  actionInfo.alreadyExists= true;
        //  actionInfo.actionPerformed = x.actionPerformed + 1;
        //  Report.actions.Add(new ActionInfo { actionName = Report.ActionType.GroupBy, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, alreadyExists = true, actionPerformed = actionInfo.actionPerformed });
        //}
        //else
        //{
        //  actionInfo.actionPerformed = 1;
        //Report.actions.Add(new ActionInfo { actionName = Report.ActionType.GroupBy, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionPerformed = actionInfo.actionPerformed });
        //}
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.GroupBy, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.GroupBy.ToString() + "-" + actionSourceName });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.GroupBy, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.GroupBy.ToString() + "-" + dataSource.sourceName });
        }
        else
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.GroupBy, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.GroupBy.ToString() + "-" + dataSource.sourceName });
        }

        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.actionDataSourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.GroupBy.ToString(), actionAlreadyExists = actionInfo.alreadyExists, actionPerformed = actionInfo.actionPerformed, actionSourceName = Report.ActionType.GroupBy.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString() });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcesql!.fileName, actionName = Report.ActionType.GroupBy.ToString(), actionSourceName = Report.ActionType.GroupBy.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString(), tableName = dataSource.sourcesql.tableName });
        }
        else
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.GroupBy.ToString(), actionAlreadyExists = actionInfo.alreadyExists, actionPerformed = actionInfo.actionPerformed, actionSourceName = Report.ActionType.GroupBy.ToString() + "-" + dataSource.sourceName, sourceType = dataSource.sourcetype.ToString() });
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }

    }

    [HttpGet("aggregate/{fileId}/{pageSize}/{pageNumber}/{sourceType}/{tableName}")]
    public async Task<IActionResult> ActionAggregate(int fileId, [FromQuery] string actionSourceName, [FromQuery] string operations, string sourceType, string tableName, [FromQuery] string? columns = null, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource dataSource = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        List<string>? headers = new List<string>();
        Console.WriteLine(fileId == 0);
        Console.WriteLine(tableName == "null");
        Console.WriteLine("null");
        Console.WriteLine(sourceType != DataSource.SourceType.SQL.ToString());
        Console.WriteLine(DataSource.SourceType.SQL.ToString());
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          dataSource = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName)!;
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != null && fileId == 0)
        {
          dataSource = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName)!;
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
              return NotFound();
            }
            dataSource = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file?.FileName)!;
          }
        }
        Report report = new Report();
        dynamic dfToReturn = report.Aggregate(dataSource, operations, columns!, pageSize, pageNumber, actionInfo);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;
        dynamic extraVariables = new ExpandoObject();
        extraVariables.column = columns;
        extraVariables.operation = operations;
        actionInfo.ActionCreated();
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Aggregate, dataSrc1 = dataSource.actionDataSourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Aggregate.ToString() + "-" + actionSourceName });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Aggregate, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Aggregate.ToString() + "-" + dataSource.sourceName });
        }
        else
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Aggregate, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Aggregate.ToString() + "-" + dataSource.sourceName });
        }

        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.actionDataSourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.Aggregate.ToString(), actionSourceName = Report.ActionType.Aggregate.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString() });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcesql!.fileName, actionName = Report.ActionType.Aggregate.ToString(),actionSourceName = Report.ActionType.Aggregate.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString(), tableName = dataSource.sourcesql.tableName });
        }
        else
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.Aggregate.ToString(), actionSourceName = Report.ActionType.Aggregate.ToString() + "-" + dataSource.sourceName, sourceType = dataSource.sourcetype.ToString() });
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("fillNa/{fileId}/{pageSize}/{pageNumber}/{sourceType}/{tableName}")]
    public async Task<IActionResult> ActionFillNa(int fileId, [FromQuery] string actionSourceName, string sourceType, string tableName, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource dataSource = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          dataSource = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName)!;
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          dataSource = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName)!;
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
              return NotFound();
            }
            dataSource = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file?.FileName)!;
          }
        }
        Report report = new Report();
        dynamic dfToReturn = report.FillNa(dataSource, actionInfo, pageSize, pageNumber);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;
        actionInfo.ActionCreated();
        actionInfo.actionCreated += 1;
        if (fileId == 0 && tableName == "null" && sourceType != DataSource.SourceType.SQL.ToString())
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.FillNullValues, dataSrc1 = dataSource.actionDataSourceName, returnedDataSource = actionInfo.returnedDataSource, actionSourceName = Report.ActionType.FillNullValues.ToString() + "-" + actionSourceName });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.FillNullValues, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, actionSourceName = Report.ActionType.FillNullValues.ToString() + "-" + dataSource.sourceName });
        }
        else
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.FillNullValues, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, actionSourceName = Report.ActionType.FillNullValues.ToString() + "-" + dataSource.sourceName });
        }
        if (fileId == 0 && tableName == "null" && sourceType != DataSource.SourceType.SQL.ToString())
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.actionDataSourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.FillNullValues.ToString(), actionSourceName = Report.ActionType.FillNullValues.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString() });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcesql!.fileName, actionName = Report.ActionType.FillNullValues.ToString(), actionSourceName = Report.ActionType.FillNullValues.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString(), tableName = dataSource.sourcesql.tableName });
        }
        else
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.FillNullValues.ToString(), actionSourceName = Report.ActionType.FillNullValues.ToString() + "-" + dataSource.sourceName, sourceType = dataSource.sourcetype.ToString() });
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost("cols/{fileId}/{pageSize}/{pageNumber}/{sourceType}/{tableName}")]
    public async Task<IActionResult> ActionDeleteColumns(int fileId, [FromQuery] string actionSourceName, [FromBody] string[] columns, string sourceType, string tableName, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource dataSource = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        List<string>? headers = new List<string>();
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          dataSource = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName)!;
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          dataSource = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName)!;
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
              return NotFound();
            }
            dataSource = Report.dslist.LastOrDefault(f => f?.sourcecsv?.fileName == file?.FileName)!;
          }
        }
        Report report = new Report();
        dynamic dfToReturn = report.DeleteColumns(dataSource, actionInfo, columns, pageSize, pageNumber);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;

        dynamic extraVariables = new ExpandoObject();
        extraVariables.columns = columns;
        actionInfo.ActionCreated();
        string serializedColumnsToDrop = JsonConvert.SerializeObject(columns);
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.DeleteColumns, dataSrc1 = dataSource.actionDataSourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.DeleteColumns.ToString() + "-" + actionSourceName });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.DeleteColumns, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.DeleteColumns.ToString() + "-" + dataSource.sourceName });
        }
        else
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.DeleteColumns, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.DeleteColumns.ToString() + "-" + dataSource.sourceName });
        }
        //check when multiple columns passed.
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.DeleteColumns.ToString(), actionSourceName = Report.ActionType.DeleteColumns.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString() });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcesql!.fileName, actionName = Report.ActionType.DeleteColumns.ToString(), actionSourceName = Report.ActionType.DeleteColumns.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString(), tableName = dataSource.sourcesql.tableName });
        }
        else
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.actionDataSourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.DeleteColumns.ToString(), actionSourceName = Report.ActionType.DeleteColumns.ToString() + "-" + dataSource.sourceName, sourceType = dataSource.sourcetype.ToString() });
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost("getcols/{fileId}/{actionSourceName}/{pageSize}/{pageNumber}/{sourceType}/{tableName}")]
    public async Task<IActionResult> ActionGetColumns(int fileId, string actionSourceName, [FromBody] string[] cols, string sourceType, string tableName, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource dataSource = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          //dataSource = Report.dslist.FirstOrDefault(ds => ds.sourceName == sourceName)!;
          dataSource = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName)!;

          //not taking actionSOucreName here. check that if not working.
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          if(Report.dslist.Find(x=> x?.sourcesql?.tableName == tableName) != null)
          {
          dataSource = Report.dslist.FirstOrDefault(x => x?.actionDataSourceName == actionSourceName)!;
          }
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
              return NotFound();
            }
            dataSource = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file?.FileName)!;
          }
        }
        Report report = new Report();
        report.GetCols(dataSource, cols, pageSize, pageNumber, actionInfo);
        dynamic extraVariables = new ExpandoObject();
        extraVariables.column = cols;
        actionInfo.ActionCreated();

        Report.actions.Add(new ActionInfo { actionName = Report.ActionType.GetColumns, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource });
// datasrc1 and datasrc 2, returnedDs.dataframe is null 

        return Ok(new { header = cols, dataOne = actionInfo.returnedDataSource.getColsRowDataOne, datatwo = actionInfo.returnedDataSource.getColsRowDataTwo });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("avg/{fileId}/{sourceType}/{tableName}")]
    public async Task<IActionResult> ActionAverage(int fileId, [FromQuery] string actionSourceName, string sourceType, string tableName)
    {
      try
      {
        DataSource dataSource = new DataSource();
        if (fileId == 0 && sourceType != DataSource.SourceType.SQL.ToString() && tableName == "null")
        {
          dataSource = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName)!;
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          dataSource = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName)!;
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId);
            if (file == null)
            {
              return NotFound();
            }
            dataSource = Report.dslist.LastOrDefault(f => f?.sourcecsv?.fileName == file?.FileName)!;
          }
        }
        ActionInfo actionInfo = new ActionInfo();

        Report report = new Report();
        dynamic dfToReturn = report.AverageOfOneCol(dataSource, actionInfo);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;
        dynamic extraVariables = new ExpandoObject();
        //extraVariables.column = column;
        actionInfo.ActionCreated();
        if (fileId == 0 && tableName == "null" && sourceType != DataSource.SourceType.SQL.ToString())
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Average, dataSrc1 = dataSource.actionDataSourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Average.ToString() + "-" + actionSourceName });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Average, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Average.ToString() + "-" + dataSource.sourceName });
        }
        else
        {
          Report.actions.Add(new ActionInfo { actionName = Report.ActionType.Average, dataSrc1 = dataSource.sourceName, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Average.ToString() + "-" + dataSource.sourceName });
        }
        if (fileId == 0 && tableName == "null" && sourceType != DataSource.SourceType.SQL.ToString())
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.actionDataSourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.Average.ToString(), actionSourceName = Report.ActionType.Average.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString() });
        }
        else if (sourceType == DataSource.SourceType.SQL.ToString() && tableName != "null" && fileId == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcesql!.fileName, actionName = Report.ActionType.Average.ToString(), actionSourceName = Report.ActionType.Average.ToString() + "-" + actionSourceName, sourceType = dataSource.sourcetype.ToString(), tableName = dataSource.sourcesql.tableName });
        }
        else
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSource.sourceName, fileName = dataSource!.sourcecsv!.fileName, actionName = Report.ActionType.Average.ToString(), actionSourceName = Report.ActionType.Average.ToString() + "-" + dataSource.sourceName, sourceType = dataSource.sourcetype.ToString() });
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet("join/{fileId1}/{fileId2}/{pageSize}/{pageNumber}/{sourceType1}/{tableName1}/{sourceType2}/{tableName2}")]
    public async Task<IActionResult> ActionJoin(int fileId1, int fileId2, [FromQuery] string left, [FromQuery] string right, [FromQuery] string left_on, [FromQuery] string right_on, [FromQuery] string how, [FromQuery] string actionSourceName1, [FromQuery] string actionSourceName2, string sourceType1, string tableName1, string sourceType2, string tableName2, int pageSize = 25, int pageNumber = 1)
    {
      try
      {
        DataSource dataSourceOne = new DataSource();
        DataSource dataSourceTwo = new DataSource();
        ActionInfo actionInfo = new ActionInfo();
        if (fileId1 == 0 && fileId2 == 0 && sourceType1 != DataSource.SourceType.SQL.ToString() && sourceType2 != DataSource.SourceType.SQL.ToString() && tableName1 == "null" && tableName2 == "null")
        {
          dataSourceOne = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName1)!;
          dataSourceTwo = Report.dslist.FirstOrDefault(action => action?.actionDataSourceName == actionSourceName2)!;
        }
        else if (sourceType1 == DataSource.SourceType.SQL.ToString() && sourceType2 == DataSource.SourceType.SQL.ToString() && tableName1 != "null" && fileId1 == 0 && tableName2 != "null" && fileId2 == 0)
        {
          dataSourceOne = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName1)!;
          dataSourceTwo = Report.dslist.FirstOrDefault(x => x?.sourcesql?.tableName == tableName2)!;
        }
        else if (sourceType1 != DataSource.SourceType.SQL.ToString() && fileId1 != 0 && sourceType2 == DataSource.SourceType.SQL.ToString() && fileId2 == 0)
        {
          // source1 is not sql and it is org uploaded csv, source2 is org sql
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file1 = await context.FileDetails.FindAsync(fileId1);
            if (file1 == null)
            {
              return NotFound();
            }
            dataSourceOne = Report.dslist.Find(f => f?.sourcecsv?.fileName == file1?.FileName)!;
            //foreach(var item in Report.dslist)
            //{
            //  if(item?.sourcecsv?.fileName == file1.FileName)
            //  {
            //    dataSourceOne = item!;
            //    break;
            //  }
            //}
          }
          //foreach (var item in Report.dslist)
          //{
          //  if (item?.sourcesql!.tableName == tableName2)
          //  {
          //    dataSourceTwo = item;
          //    break;
          //  }
          //}
          dataSourceTwo = Report.dslist.Find(x => x?.sourcesql?.tableName == tableName2)!;
        }
        else if (sourceType2 != DataSource.SourceType.SQL.ToString() && fileId2 != 0 && sourceType1 == DataSource.SourceType.SQL.ToString() && fileId1 == 0 && tableName2 == "null" && tableName1 != "null")
        {
          // source2 is not sql and it is org uploaded csv, source1 is org sql
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file2 = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId2);
            if (file2 == null)
            {
              return NotFound();
            }
            dataSourceTwo = Report.dslist.Find(f => f?.sourcecsv?.fileName == file2?.FileName)!;
          }
          dataSourceOne = Report.dslist.Find(x => x?.sourcesql?.tableName == tableName1)!;
        }
        else
        {
          using (var context = new FileContext(new DbContextOptions<FileContext>()))
          {
            var file1 = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId1);
            if (file1 == null)
            {
              return NotFound();
            }
            dataSourceOne = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file1?.FileName)!;
            var file2 = await context.FileDetails.FirstOrDefaultAsync(f => f.Id == fileId2);
            if (file2 == null)
            {
              return NotFound();
            }
            dataSourceTwo = Report.dslist.FirstOrDefault(f => f?.sourcecsv?.fileName == file2?.FileName)!;
          }
        }
        Report report = new Report();
        dynamic dfToReturn = report.Join(dataSourceOne, dataSourceTwo, actionInfo, left, right, left_on, right_on, how, pageSize, pageNumber);
        actionInfo.returnedDataSource.deserializedCols = dfToReturn;
        dynamic extraVariables = new ExpandoObject();
        extraVariables.left = left;
        extraVariables.right = right;
        extraVariables.left_on = left_on;
        extraVariables.right_on = right_on;
        extraVariables.how = how;
        actionInfo.ActionCreated();
        //string actName = "";
        //switch(how)
        //{
        //  case "left":
        //    actName = Report.ActionType.LeftJoin.ToString();
        //    break;
        //  case "right":
        //    actName = Report.ActionType.RightJoin.ToString();
        //    break;
        //  case "inner":
        //    actName = Report.ActionType.InnerJoin.ToString();
        //    break;
        //  case "outer":
        //    actName = Report.ActionType.OuterJoin.ToString();
        //    break;
        //}

        Report.actions.Add(new ActionInfo { dataSrc1 = dataSourceOne.sourceName, dataSrc2 = dataSourceTwo.sourceName, actionName = Report.ActionType.Join, returnedDataSource = actionInfo.returnedDataSource, extraVars = extraVariables, actionSourceName = Report.ActionType.Join.ToString() + "-" + dataSourceOne.sourceName + "_" + dataSourceTwo.sourceName });

        if (fileId1 == 0 && fileId2 == 0 && sourceType1 != DataSource.SourceType.SQL.ToString() && sourceType2 != DataSource.SourceType.SQL.ToString() && tableName1 == "null" && tableName2 == "null")
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSourceOne.sourceName + dataSourceTwo.sourceName, fileName = left == dataSourceOne!.sourceName ? dataSourceOne!.sourcecsv!.fileName : dataSourceTwo!.sourcecsv!.fileName, actionName = Report.ActionType.Join.ToString(), actionSourceName = Report.ActionType.Join.ToString() + "-" + dataSourceOne.sourceName + "_" + dataSourceTwo.sourceName });
        }
        else if (sourceType1 == DataSource.SourceType.SQL.ToString() && sourceType2 == DataSource.SourceType.SQL.ToString() && tableName1 != "null" && fileId1 == 0 && tableName2 != "null" && fileId2 == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSourceOne.sourceName + dataSourceTwo.sourceName, fileName = dataSourceOne!.sourcesql!.fileName, actionName = Report.ActionType.Join.ToString(), actionSourceName = Report.ActionType.Join.ToString() + "-" + dataSourceOne.sourceName + "_" + dataSourceTwo.sourceName });
        }
        else if (sourceType1 != DataSource.SourceType.SQL.ToString() && fileId1 != 0 && sourceType2 == DataSource.SourceType.SQL.ToString() && fileId2 == 0)
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSourceOne.sourceName + dataSourceTwo.sourceName, fileName = dataSourceTwo!.sourcesql!.fileName, actionName = Report.ActionType.Join.ToString(), actionSourceName = Report.ActionType.Join.ToString() + "-" + dataSourceOne.sourceName + "_" + dataSourceTwo.sourceName });
        }
        else if (sourceType2 != DataSource.SourceType.SQL.ToString() && fileId2 != 0 && sourceType1 == DataSource.SourceType.SQL.ToString() && fileId1 == 0 && tableName2 == "null" && tableName1 != "null")
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSourceOne.sourceName + dataSourceTwo.sourceName, fileName = dataSourceOne!.sourcesql!.fileName, actionName = Report.ActionType.Join.ToString(), actionSourceName = Report.ActionType.Join.ToString() + "-" + dataSourceOne.sourceName + "_" + dataSourceTwo.sourceName});
        }
        else
        {
          return Ok(new { header = dfToReturn, data = actionInfo.returnedDataSource.rowData, fileLength = actionInfo.returnedDataSource.sourceFileLength, sourcename = dataSourceOne.sourceName + dataSourceTwo.sourceName, fileName = dataSourceOne!.sourcecsv!.fileName, actionName = Report.ActionType.Join.ToString(), actionSourceName = Report.ActionType.Join.ToString() + "-" + dataSourceOne.sourceName + "_" + dataSourceTwo.sourceName });
        }

        // sourceType
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
