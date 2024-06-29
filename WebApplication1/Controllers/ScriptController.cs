using Microsoft.AspNetCore.Mvc;
using WebApplication1.Backend;
using System.IO;
using System.Net.Http.Headers;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ScriptController : ControllerBase
  {
    [HttpGet("script")]
    public IActionResult GetScript()
    {
      try
      {
        GenerateScript.final_def_String = GenerateScript.Imports();
        if (SourceCSV.csvObj)
        {
          GenerateScript.final_def_String = GenerateScript.final_def_String + "\n" + GenerateScript.ETL_CSV();
        }
        foreach (var item in Report.dslist)
        {
          if (item.sourcetype == DataSource.SourceType.CSV)
          {
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforCSV(item)))
            {
              continue;
            }
            else
            {
              if (item.savedDataSource == true)
              {
                continue;
              }
              GenerateScript.final_call_String = GenerateScript.final_call_String + "\n" + GenerateScript.createObjectsforCSV(item);
            }
          }
        }
        if (Report.actions.Count != 0)
        {
          GenerateScript.final_def_String = GenerateScript.final_def_String + GenerateScript.action_Class();
        }
        GenerateScript.dynamicCallsdef();
        GenerateScript.dynamicCallsobj();


        Console.WriteLine(GenerateScript.final_def_String + GenerateScript.final_call_String);
        string script = GenerateScript.final_def_String + GenerateScript.final_call_String;
        DataSource dataSource = Report.dslist[0];
        string fileName = $"{dataSource.sourceName!}.py";
        string path = Path.Combine($@"D:\Languages\Angular\angularAPICall\csharp\scripts", fileName);
        using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
          using (StreamWriter writer = new StreamWriter(fileStream))
          {
            writer.Write(script);
          }
        }
        byte[] fileBytes = System.IO.File.ReadAllBytes(path);
        return File(fileBytes, "application/octet-stream", fileName);
        //var memory = new MemoryStream();
        //using (var writer = new StreamWriter(memory))
        //{
        //  writer.Write(script);
        //  writer.Flush();
        //  memory.Position = 0;
        //  return File(memory, "application/octet-stream", fileName);
        //}
        //return Ok(new { script = GenerateScript.final_def_String + GenerateScript.final_call_String });
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost("uploadScript")]
    public IActionResult UploadScript([FromForm] FileModel fileModel)
    {
      try
      {
        Report.dslist.Clear();
        var pyFile = Request.Form.Files[0];
        var fileName = "";
        if (pyFile.Length > 0)
        {
          fileName = ContentDispositionHeaderValue.Parse(pyFile.ContentDisposition).FileName!.Trim('"');
        }
        string path = Path.Combine(@"D:\\Languages\\Angular\\angularAPICall\\csharp\\pyFiles\\", fileName);
        using (Stream stream = new FileStream(path, FileMode.Create))
        {
          fileModel?.File?.CopyTo(stream);
        }
        ReadScript rs = new ReadScript();
        rs.getVariablesFromScript(fileName, path);
        dynamic dsToReturn = Report.dslist.Select(ds => new {
          headers = ds.deserializedCols,
          sourceName = ds.sourceName,
          rowData = ds.rowData,
          sourceType = ds.sourcetype.ToString(),
          fileLength = ds.sourceFileLength,
          fileName = ds.sourcetype == DataSource.SourceType.CSV ? ds.sourcecsv!.fileName : ds.sourcesql!.fileName,
          actionSourceName = ds.actionDataSourceName
        }).ToList();
        return Ok( new {dataSources = dsToReturn});
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }
}
