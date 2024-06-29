using System.Dynamic;

namespace WebApplication1.Backend
{
  public class ActionInfo
  {
    public static int counter = 0;
    public int actionCreated = 0;
    public ActionInfo()
    {
      //counter++;
      actionCreated = 1;
    }

    public void ActionCreated()
    {
      counter++;
    }
    // public string? actionName;
    public Report.ActionType actionName = new Report.ActionType();
    public string? actionVarName = "actionVar" + counter;
    public int? whichAction = counter;
    // public Datasource? dataSrc1;
    // public Datasource? dataSrc2;
    public string? dataSrc1;
    public string? dataSrc2;

    public string? objectName = "actionObj" + counter;

    public DataSource returnedDataSource = new DataSource();

    public static List<ColData> returnedColumns = new List<ColData>();

    public dynamic? extraVars = new ExpandoObject();

    public bool alreadyExists = false;

    public string? actionSourceName;

    public int? actionPerformed;

  }
}
