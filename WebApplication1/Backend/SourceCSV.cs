namespace WebApplication1.Backend
{
  public class SourceCSV
  {
    public static bool csvObj = false;
    public SourceCSV()
    {
      csvObj = true;
    }

    public string? filePath;
    public string? fileName;
    public dynamic? fileLength;
  }
}
