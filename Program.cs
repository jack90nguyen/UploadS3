using Uploader.Helpers;

Console.Title = "Upload to S3 - by Jack Nguyen";

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("::::: Upload to S3 - by Jack Nguyen :::::\n");

var folders = Helpers.GetFolder();

Console.WriteLine($"Scaning {folders.Count} folders...");

Console.Write($"You want to upload {folders.Count} folders (y/n): ");
bool isRun = Console.ReadLine().EndsWith("y");

if(isRun)
{
  Helpers.ConsoleLog(ConsoleColor.Green, "Start uploading...");
  var results = new Dictionary<string, string>();
  foreach (var folder in folders)
  {
    Console.Clear();
    Helpers.ConsoleLog(ConsoleColor.Yellow, $"Uploading {folder}...");
    var files = Helpers.GetFilesInFoler(folder);
    int total = files.Count;
    int done = 0;
    var taskList = new List<Task<string>>();
    for (int i = 0; i < total; i++)
    {
      taskList.Add(Helpers.UploadFile(files[i], i+1, total));
      if(taskList.Count == 10)
      {
        var links = await Task.WhenAll(taskList);
        done += links.Count(x => !string.IsNullOrEmpty(x));
        taskList.Clear();
      }
    }
    if(taskList.Count > 0)
    {
      var links = await Task.WhenAll(taskList);
      done += links.Count(x => !string.IsNullOrEmpty(x));
      taskList.Clear();
    }
    results.Add(folder, $"{done}/{total}");
  }
  foreach (var item in results)
  {
    Helpers.ConsoleLog(ConsoleColor.Green, $"{item.Key}: {item.Value}");
  }
}

Console.ReadKey();