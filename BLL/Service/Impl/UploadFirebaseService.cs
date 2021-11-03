using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Service.Impl
{
    public class UploadFirebaseService : IUploadFirebaseService
    {
        private IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string bucket;

        public UploadFirebaseService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            bucket = _configuration.GetValue<string>("Firebase:Bucket");
        }
        public async Task<string> UploadFilesToFirebase(List<IFormFile> files, string type, string parent, string fileName)
        {
            string urlConcat = String.Empty;
            foreach(var file in files)
            {
                try
                {
                    string url = await UploadFileToFirebase(file, type, parent, 
                        fileName + (files.IndexOf(file) + 1));

                    if(file == files[files.Count -1])
                    {
                        urlConcat += url;
                    }
                    else
                    {
                        urlConcat += url + "|";
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message + "\nCannot upload image to Firebase Storage");
                    return null;
                }
            }
            return urlConcat;
        }

        public async Task<string> UploadFileToFirebase(IFormFile file, string type, string parent, string fileName)
        {
            if (file != null)
            {
                if (file.Length > 0)
                {
                    string typeOfFile = file.FileName.Substring(file.FileName.IndexOf("."));

                    try
                    {
                        FirebaseStorageTask task = new FirebaseStorage(bucket)
                            .Child(type)
                            .Child(parent)
                            .Child(fileName + typeOfFile)
                            .PutAsync(file.OpenReadStream());
                        return await task;
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message + "\nCannot upload image to Firebase Storage");
                        return null;
                    }

                }
            }
            return String.Empty;
        }
    }
}
