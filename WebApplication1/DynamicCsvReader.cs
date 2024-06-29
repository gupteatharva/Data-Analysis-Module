using CsvHelper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;

namespace WebApplication1
{
  public class DynamicCsvReader
  {
    public List<dynamic> ReadCsv(string filePath)
    {
      List<dynamic> data = new List<dynamic>();
      using (var reader = new StreamReader(filePath))
      using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
      {
        csv.Read();
        csv.ReadHeader();
        while (csv.Read())
        {
          var record = csv.GetRecord<dynamic>();
          data.Add(record);
        }
        return data;
      }
    }
  }
}
