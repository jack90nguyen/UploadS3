using Amazon.S3;
using Amazon.S3.Model;

namespace Uploader.Helpers;

public static class Helpers
{
  private static bool isMacOS = Environment.CurrentDirectory.Contains("/");

  public static readonly Dictionary<string, string> Config = GetConfig();


  private static Dictionary<string, string> GetConfig()
  {
    string path = GetFullPath("/config.txt");
    var config = new Dictionary<string, string>();
    if (File.Exists(path))
    {
      var data = File.ReadAllLines(path);
      foreach (var item in data)
      {
        var temp = item.Split("=");
        config.Add(temp[0].Trim(), temp[1].Trim());
      }
    }
    return config;
  }

  public static void ConsoleLog(ConsoleColor color, string message)
  {
    Console.ForegroundColor = color;
    Console.WriteLine(message);
    Console.ForegroundColor = ConsoleColor.White;
  }

  /// <summary>
  /// Lấy folder chính xác theo hệ điều hành
  /// </summary>
  public static string GetPath(string path)
  {
    if (isMacOS)
      return path.Replace("\\", "/");
    else
      return path.Replace("/", "\\");
  }

  /// <summary>
  /// Lấy dường dẫn chính xác để lưu file
  /// </summary>
  public static string GetFullPath(string fileName)
  {
    string fullPath = Environment.CurrentDirectory + fileName;
    return GetPath(fullPath);
  }

  public static List<string> GetFolder()
  {
    string path = GetFullPath("/folder.txt");
    var results = new List<string>();
    if (File.Exists(path))
    {
      var data = File.ReadAllLines(path);
      foreach (var item in data)
      {
        results.Add(item);
      }

      if(results.Count == 1)
      {
        string filePath = GetFullPath("/out.txt");
        // Write some text to the file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
          var subFolders = Directory.GetDirectories(results[0]);
          foreach (var subPath in subFolders)
            writer.WriteLine(subPath);
        }
      }
    }
    return results;
  }

  /// <summary>
  /// Lấy tất cả file trong folder chính và subfolder
  /// </summary>
  public static List<string> GetFilesInFoler(string path)
  {
    var results = new List<string>();

    // Lấy file trong Folder chính
    var fileList = Directory.GetFiles(path);
    if (fileList != null && fileList.Length > 0)
    {
      results.AddRange(fileList);
    }

    // Lấy file trong Folder con
    var subFolders = Directory.GetDirectories(path);
    if (subFolders != null && subFolders.Length > 0)
    {
      foreach (var subPath in subFolders)
        results.AddRange(GetFilesInFoler(subPath));
    }

    return results;
  }

  #region AWS S3

  private static IAmazonS3 GetClient()
  {
    string ACCESS_KEY = Config["ACCESS_KEY"];
    string SECRET_KEY = Config["SECRET_KEY"];
    var credentials = new Amazon.Runtime.BasicAWSCredentials(ACCESS_KEY, SECRET_KEY);
    return new AmazonS3Client(credentials, Amazon.RegionEndpoint.APSoutheast1);
  }

  public static async Task<string> UploadFile(string filePath, int index, int total)
  {
    string fileName = filePath.Replace(Config["Folder"], "").Replace("\\", "/");
    ConsoleLog(ConsoleColor.Green, $"Uploading {index} / {total}");
    Console.WriteLine($" -> {fileName}");

    try
    {
      var client = GetClient();
      var request = new PutObjectRequest
      {
        BucketName = Config["Bucket"],
        FilePath = filePath,
        Key = fileName,
      };

      var response = await client.PutObjectAsync(request);
      if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
      {
        return fileName;
      }
      else
      {
        ConsoleLog(ConsoleColor.Red, $"Upload fail {fileName}");
      }
    }
    catch (Exception ex)
    {
      ConsoleLog(ConsoleColor.Red, $"Upload Error: {fileName}\n{ex.Message}");
    }
    return null;
  }

  private static string GetFileUrl(string fileName)
  {
    string CDN_URL = Config["CDN_URL"];
    return $"{CDN_URL}/{fileName.Replace(" ", "+")}";
  }
  
  #endregion
}