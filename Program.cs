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
    for (int i = 0; i < total; i++)
    {
      Helpers.ConsoleLog(ConsoleColor.Green, $"Uploading {i+1}/{total}...");
      string link = await Helpers.UploadFile(files[i]);
      if(!string.IsNullOrEmpty(link))
        done++;
    }
    results.Add(folder, $"{done}/{total}");
  }
  foreach (var item in results)
  {
    Helpers.ConsoleLog(ConsoleColor.Green, $"{item.Key}: {item.Value}");
  }
}

Console.ReadKey();