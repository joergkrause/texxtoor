using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImportFromWord.PlatformService;

namespace ImportFromWord.ServiceClient {
  internal class Client
  {

    private static PlatformService.UploadServiceClient client = new UploadServiceClient();


    internal static UploadServiceClient Upload
    {
      get { return client; }
    }



    public static string SessionId { get; set; }
  }
}
