namespace HackerFlow.Services;

public class OpenFileService : IOpenFileService
{
    public void OpenFile(string path)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }
}

public interface IOpenFileService
{
    void OpenFile(string path);
}