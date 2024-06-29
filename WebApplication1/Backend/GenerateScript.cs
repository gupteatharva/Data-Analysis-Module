using Newtonsoft.Json;
using System;
using System.Runtime;
using System.Security.Cryptography;

namespace WebApplication1.Backend
{
  public class GenerateScript
  {
    public static string final_def_String = "";
    public static string final_call_String = "";
    public static string Imports()
    {
      return @"import pandas as pd";
    }
    public static string ETL_CSV()
    {
      return @"
pyDsList = []

class ETL_CSV:
    def extract(self, filename,cols):
        self.filename = filename
        self.cols = cols
        df = pd.read_csv(self.filename)
        df = df[self.cols]
        return df";
    }
    public static string ETL_API()
    {
      return @"
            class ETL_API:
                res_list=[]
                def extract(self,api_url, api_key,m_l,m_u):
                    self.api_url = api_url
                    self.api_key = api_key
                    self.m_l = m_l
                    self.m_u = m_u

                    for movie_id in range(self.m_l, self.m_u):
                        url = '{}{}?api_key={}'.format(self.api_url,movie_id, self.api_key)
                        r = requests.get(url)
                        self.__class__.res_list.append(r.json())
                    return self.__class__.res_list";
    }

    public static string createObjectsforCSV(DataSource? ds)
    {
      string createObject = $"{ds?.objname}=ETL_{ds?.sourcetype}()\n{ds?.objname + ds?.sourceName}={ds?.objname}.extract('{ds?.sourcecsv?.filePath}',[";
      foreach (var col in ds!.cols)
      {
        createObject += col == ds.cols.Last() ? $"'{col?.colName}'" : $"'{col?.colName}',";
      }
      createObject += "])";
      createObject += $"\npyDsList.append({{'df':{ds?.objname + ds?.sourceName}, 'name': '{ds?.objname + ds?.sourceName}', 'sourceName': '{ds?.sourceName}', 'actionName': '', 'actionDataSourceName': '{ds?.actionDataSourceName}', 'sourceType': '{ds?.sourcetype.ToString()}', 'columns': {{";
      foreach (var col in ds!.cols)
      {
        createObject += col == ds.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
      }
      //createObject += "], 'dataTypes': [";
      //foreach (var col in ds!.cols)
      //{
      //  createObject += col == ds.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
      //}
        createObject += $"}}}})";
      return createObject;
    }
    //public static string createObjectsforAPI(DataSource? ds)
    //{
    //  return $@"{ds?.objname}=ETL_{ds?.sourcetype}()\n pd.DataFrame.from_dict({ds?.objname}.extract('{ds?.sourceapi?.API_URL}','{ds?.sourceapi?.par?.apiKey}',{ds?.sourceapi?.par?.loLim},{ds?.sourceapi?.par?.upLim}))";
    //}

    public static string action_Class()
    {
      return @"

class Actions: ";
    }

    public static string action_Concat_def()
    {
      return @"
    def Concat(self, df1, df2):
        self.df1 = df1
        self.df2 = df2
        df3 = pd.concat([self.df1, self.df2], axis=1)
        return df3";
    }
    public static string createObjectsforConcat(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      DataSource? ds2 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc2!));
      string createObject = $"{info.objectName}=Actions() \n";
      createObject += $"{info.actionVarName}={info.objectName}.{info.actionName}({ds1!.objname + ds1?.sourceName},{ds2!.objname + ds2?.sourceName})";
      return createObject;
    }

    public static string action_GroupBy_def()
    {
      return @"
    def GroupBy(self, df, column, operation):
        self.df = df
        self.column = column
        self.operation = operation
        if self.operation == 'min':
            df1 = df.groupby(self.column).min()
        elif self.operation == 'max':
            df1 = df.groupby(self.column).max() 
        elif self.operation == 'sum':
            df1 = df.groupby(self.column).sum() 
        elif self.operation == 'mean':
            df1 = df.groupby(self.column).mean() 
        return df1";
    }

    public static string createObjectsforGroupBy(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      string createObject = info.objectName + "=Actions()" + "\n";
      if (ds1!.savedDataSource == true && info.whichAction != 1)
      {
        var action = Report.actions.FirstOrDefault(act => act.actionSourceName == ds1.sourceName);
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({action!.actionVarName}, '{info!.extraVars!.column}', '{info!.extraVars!.operation}')";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{action.actionSourceName}', 'sourceName': '{ds1.sourceName}', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': '{info?.actionSourceName}','actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      else
      {
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({ds1!.objname + info.dataSrc1}, '{info!.extraVars!.column}', '{info!.extraVars!.operation}')";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info.dataSrc1}','sourceName': '{ds1.sourceName}', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': '{info?.actionSourceName}', 'actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      // string createObject = ActionInfo.counter == 0 ? $"{info.objectName}=Actions() \n" : "";

      return createObject;
    }
    public static string action_Aggregate_def()
    {
      return @"
    def Aggregate(self, df, operation, column = ''):
        self.df = df
        self.column = column
        self.operation = operation
        if self.column != '':
            df1 = df.agg(x=(self.column, self.operation))
        else:
            df1 = df.agg(['self.operation'])
        return df1         
            ";
    }

    public static string createObjectsforAggregate(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      string createObject = info.objectName + "=Actions()" + "\n";
      if (ds1!.savedDataSource == true && info.whichAction != 1)
      {
        var action = Report.actions.FirstOrDefault(act => act.actionSourceName == ds1.sourceName);
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({action!.actionVarName},'{info!.extraVars!.operation}', '{info!.extraVars!.column}')";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{action.actionSourceName}', 'sourceName': '{ds1.sourceName}', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': '{info?.actionSourceName}', 'actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      else
      {
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({ds1!.objname + info.dataSrc1}, '{info!.extraVars!.operation}', '{info!.extraVars!.column}')";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info!.dataSrc1}', 'sourceName': '{ds1.sourceName}', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': '{info?.actionSourceName}','actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      // string createObject = ActionInfo.counter == 0 ? $"{info.objectName}=Actions() \n" : "";

      return createObject;
    }

    public static string action_FillNa_def()
    {
      return @"
    def FillNullValues(self, df):
        self.df = df
        df1 = df.replace(to_replace='null', method='pad')
        return df1         
            ";
    }

    public static string createObjectsforFillNa(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      string createObject = info.objectName + "=Actions()" + "\n";
      if (ds1!.savedDataSource == true && info.whichAction != 1)
      {
        var action = Report.actions.FirstOrDefault(act => act.actionSourceName == ds1.sourceName);
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({action!.actionVarName})";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{action!.actionSourceName}', 'sourceName': '{ds1.sourceName}', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': '{info?.actionSourceName}','actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      else
      {
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({ds1!.objname + info.dataSrc1})";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info.dataSrc1}', 'sourceName': '{ds1.sourceName} ', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': ' {info?.actionSourceName}','actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      // string createObject = ActionInfo.counter == 0 ? $"{info.objectName}=Actions() \n" : "";

      return createObject;
    }

    public static string action_DeleteColumns_def()
    {
      return @"
    def DeleteColumns(self, df, cols):
        self.df = df
        self.cols = cols
        df1 = df.drop(columns = self.cols)
        return df1         
            ";
    }

    public static string createObjectsforDeleteColumns(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      string createObject = info.objectName + "=Actions()" + "\n";
      if (ds1!.savedDataSource == true && info.whichAction != 1)
      {
        var action = Report.actions.FirstOrDefault(act => act.actionSourceName == ds1.sourceName);
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({action!.actionVarName}, [";
        foreach (var col in info.extraVars!.columns)
        {
          createObject += col == info.extraVars.columns[info.extraVars.columns.Length - 1] ? $"'{col}'" : $"'{col}',";
        }
        createObject += "])";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info!.actionSourceName}','sourceName': '{ds1.sourceName} ', 'sourceType': '{ds1?.sourcetype.ToString()}','actionDataSourceName': ' {info?.actionSourceName}', 'actionName':'{info?.actionName}', 'columns': {{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      else
      {
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({ds1!.objname + info.dataSrc1}, [";
        foreach (var col in info.extraVars!.columns)
        {
          createObject += col == info.extraVars.columns[info.extraVars.columns.Length - 1] ? $"'{col}'" : $"'{col}',";
        }
        createObject += "])";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info!.dataSrc1}', 'sourceName': '{ds1.sourceName}','sourceType': '{ds1?.sourcetype.ToString()}', 'actionDataSourceName': '{info?.actionSourceName}','actionName':'{info?.actionName}', 'columns':{{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        } 
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"{{}})";
      }
      // string createObject = ActionInfo.counter == 0 ? $"{info.objectName}=Actions() \n" : "";

      return createObject;
    }

    public static string action_Average_def()
    {
      return @"
    def Average(self, df):
        self.df = df
        df1 = df.mean()
        return df1         
            ";
    }

    public static string createObjectsforAverage(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      string createObject = info.objectName + "=Actions()" + "\n";
      if (ds1!.savedDataSource == true && info.whichAction != 1)
      {
        var action = Report.actions.FirstOrDefault(act => act.actionSourceName == ds1.sourceName);
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({action!.actionVarName})";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{action.actionSourceName}', 'sourceName': '{ds1.sourceName} ','sourceType': '{ds1?.sourcetype.ToString()}', 'actionDataSourceName': ' {info?.actionSourceName}','actionName':'{info?.actionName}', 'columns':{{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"}}}})";
      }
      // string createObject = ActionInfo.counter == 0 ? $"{info.objectName}=Actions() \n" : "";
      else
      {
        createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({ds1!.objname + info.dataSrc1})";
        createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info!.dataSrc1}', 'sourceName': '{ds1.sourceName} ','sourceType': '{ds1?.sourcetype.ToString()}', 'actionDataSourceName': ' {info?.actionSourceName}','actionName':'{info?.actionName}', 'columns':{{";
        foreach (var col in info!.returnedDataSource.cols)
        {
          createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
        }
        //createObject += "], 'dataTypes': [";
        //foreach (var col in info!.returnedDataSource.cols)
        //{
        //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
        //}
        createObject += $"{{}})";
      }
      return createObject;
    }

    public static string action_Join_def()
    {
      return @"
    def Join(self, df1, df2, left, right, left_on, right_on, how):
        self.df1 = df1
        self.df2 = df2
        self.left = left
        self.right = right
        self.left_on = left_on
        self.right_on = right_on
        self.how = how
        df = pd.merge(left=self.left, right=self.right, left_on=self.left_on, right_on=self.right_on, how=self.how)
        df = df.fillna('null')
        return df         
            ";
    }

    public static string createObjectsforJoin(ActionInfo info)
    {
      DataSource? ds1 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc1!));
      DataSource? ds2 = Report.dslist.Find(x => x.sourceName!.Contains(info.dataSrc2!));
      string createObject = info!.objectName + "=Actions()" + "\n";
      // string createObject = ActionInfo.counter == 0 ? $"{info.objectName}=Actions() \n" : "";
      createObject += $"{info.actionVarName} = {info.objectName}.{info.actionName}({ds1!.objname + info.dataSrc1}, {ds2!.objname + info.dataSrc2}, {(info!.extraVars!.left == ds1.sourceName ? ds1!.objname + info!.extraVars!.left : ds2!.objname + info!.extraVars!.left)}, {(info.extraVars.right == ds1.sourceName ? ds1!.objname + info!.extraVars!.right : ds2!.objname + info!.extraVars!.right)}, '{info!.extraVars!.left_on}', '{info!.extraVars!.right_on}', '{info.extraVars.how}')";
      createObject += $"\npyDsList.append({{'df':{info.actionVarName}, 'name': '{info.actionName}-{info!.dataSrc1}', 'sourceName': '{ds1.sourceName} ','sourceType': '{ds1?.sourcetype.ToString()}', 'actionDataSourceName': ' {info?.actionSourceName}','actionName':'{info?.actionName}', 'columns': {{";
      foreach (var col in info!.returnedDataSource.cols)
      {
        createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colName}':'{col?.colType}'" : $"'{col?.colName}':'{col?.colType}',";
      }
      //createObject += "], 'dataTypes': [";
      //foreach (var col in info!.returnedDataSource.cols)
      //{
      //  createObject += col == info!.returnedDataSource.cols.Last() ? $"'{col?.colType}'" : $"'{col?.colType}',";
      //}
      createObject += $"}}}})";
      return createObject;
    }

    public static void dynamicCallsdef()
    {
      foreach (Report.ActionType act in Enum.GetValues(typeof(Report.ActionType)))
      {
        switch (act)
        {
          case Report.ActionType.Concat:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_Concat_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_Concat_def()}";
              }
            }
            break;

          //case Report.ActionType.Pivot:
          //  if (Report.actions.Exists(x => x.actionName == act))
          //  {
          //    GenerateScript.final_def_String += $"\n{GenerateScript.action_Pivot_def()}";
          //  }
          //  break;

          case Report.ActionType.GroupBy:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_GroupBy_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_GroupBy_def()}";
              }
            }
            break;
          case Report.ActionType.Aggregate:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_Aggregate_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_Aggregate_def()}";
              }
            }
            break;
          case Report.ActionType.FillNullValues:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_FillNa_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_FillNa_def()}";
              }
            }
            break;
          case Report.ActionType.DeleteColumns:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_DeleteColumns_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_DeleteColumns_def()}";
              }
            }
            break;
          case Report.ActionType.Average:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_Average_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_Average_def()}";
              }
            }
            break;
          case Report.ActionType.Join:
            if (Report.actions.Exists(x => x.actionName == act))
            {
              if (GenerateScript.final_def_String.Contains(GenerateScript.action_Join_def()))
              {
                continue;
              }
              else
              {
                GenerateScript.final_def_String += $"\n{GenerateScript.action_Join_def()}";
              }
            }
            break;
        }
      }
    }
    public static void dynamicCallsobj()
    {
      foreach (var actitem in Report.actions)
      {
        switch (actitem.actionName)
        {
          case Report.ActionType.Concat:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforConcat(actitem)))
            { continue; }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforConcat(actitem)}";
            }
            break;
          //case Report.ActionType.Pivot:
          //  GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforPivot(actitem)}";
          //  break;
          case Report.ActionType.GroupBy:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforGroupBy(actitem)))
            {
              continue;
            }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforGroupBy(actitem)}";
            }
            break;
          case Report.ActionType.Aggregate:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforAggregate(actitem)))
            { continue; }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforAggregate(actitem)}";
            }
            break;
          case Report.ActionType.FillNullValues:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforFillNa(actitem)))
            {
              continue;
            }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforFillNa(actitem)}";
            }
            break;
          case Report.ActionType.DeleteColumns:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforDeleteColumns(actitem)))
            {
              continue;
            }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforDeleteColumns(actitem)}";
            }
            break;
          case Report.ActionType.Average:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforAverage(actitem)))
            {
              continue;
            }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforAverage(actitem)}";
            }
            break;
          case Report.ActionType.Join:
            if (GenerateScript.final_call_String.Contains(GenerateScript.createObjectsforJoin(actitem)))
            {
              continue;
            }
            else
            {
              GenerateScript.final_call_String += $"\n{GenerateScript.createObjectsforJoin(actitem)}";
            }
            break;
            // default:
        }
      }

    }

  }
}
