using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Service
{
    public interface IUploadFirebaseService
    {
        Task<string> UploadFileToFirebase(IFormFile file, string type, string parent, string fileName);

        Task<string> UploadFilesToFirebase(List<IFormFile> file, string type, string parent, string fileName);

    }
}
