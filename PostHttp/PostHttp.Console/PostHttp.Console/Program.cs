using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


//To compare HTTP posts use Fiddler 
//https://www.telerik.com/download/fiddler

class Program
{
  static async Task Main(string[] args)
  {
    var boundary = "--------------------------464896959184052182908497"; // Same as Postman's boundary

    using (var client = new HttpClient())
    {
      // Manually build the multipart/form-data body
      var bodyBuilder = new StringBuilder();

      // Add repository_id field
      bodyBuilder.AppendLine($"--{boundary}");
      bodyBuilder.AppendLine(@"Content-Disposition: form-data; name=""repository_id""");
      bodyBuilder.AppendLine();
      bodyBuilder.AppendLine("4");

      // Add image file
      bodyBuilder.AppendLine($"--{boundary}");
      bodyBuilder.AppendLine(@"Content-Disposition: form-data; name=""image_filename""; filename=""koronaTesztZsolt.jpg""");
      bodyBuilder.AppendLine("Content-Type: image/jpeg");
      bodyBuilder.AppendLine();

      // Convert text part to bytes
      var bodyStart = Encoding.UTF8.GetBytes(bodyBuilder.ToString());

      // Add image file content
      var filePath = @"d:/BBox/tmp/emt/koronaTesztZsolt.jpg";
      var fileBytes = await File.ReadAllBytesAsync(filePath);

      // Add closing boundary
      var bodyEnd = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

      // Combine all parts
      var content = new ByteArrayContent(CombineArrays(bodyStart, fileBytes, bodyEnd));
      content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue($"multipart/form-data") { Parameters = { new System.Net.Http.Headers.NameValueHeaderValue("boundary", boundary) } };

      try
      {
        // Send the request
        var response = await client.PostAsync("http://127.0.0.1:8005/iseeker/4_0/addimage", content);

        // Check response
        if (response.IsSuccessStatusCode)
        {
          var responseData = await response.Content.ReadAsStringAsync();
          Console.WriteLine("Response: " + responseData);
        }
        else
        {
          Console.WriteLine($"Error: {response.StatusCode}");
          var errorData = await response.Content.ReadAsStringAsync();
          Console.WriteLine("Response Content: " + errorData);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Exception: " + ex.Message);
      }
    }
  }

  // Helper method to combine byte arrays
  private static byte[] CombineArrays(params byte[][] arrays)
  {
    var length = 0;
    foreach (var array in arrays)
    {
      length += array.Length;
    }

    var result = new byte[length];
    var offset = 0;

    foreach (var array in arrays)
    {
      Buffer.BlockCopy(array, 0, result, offset, array.Length);
      offset += array.Length;
    }

    return result;
  }
}
