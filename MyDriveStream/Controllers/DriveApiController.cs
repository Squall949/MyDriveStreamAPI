using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using MyDriveStream.Models;

namespace MyDriveStream.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriveApiController : ControllerBase
    {
        private static string[] Scopes = { DriveService.Scope.DriveReadonly };
        private static string ApplicationName = "MyDriveStream";

        [HttpGet]
        [Route(nameof(GetFiles))]
        public async Task<IList<DriveFile>> GetFiles()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name, thumbnailLink, owners)";
            listRequest.Q = "mimeType='video/mp4'";

            // List files.
            var files = await listRequest.ExecuteAsync();

            List<DriveFile> fileList = files.Files.Where(f => f.Owners[0].Me == true).Select(
                    e => new DriveFile()
                    {
                        FileID = e.Id,
                        Name = e.Name,
                        Thumbnail = e.ThumbnailLink
                    }).ToList();

            if (files != null && fileList.Count > 0)
                return fileList;
            else return new List<DriveFile>();
        }
    }
}
