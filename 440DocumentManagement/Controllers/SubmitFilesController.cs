
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class SubmitFilesController : Controller
	{
		//readonly string _924TableName = "924ProjectFileRetrieval";
		//readonly string _dropboxAccessToken = "joixejMHI5AAAAAAAAAABiDg4hq4TA3Elm2nnEpD5gmz8tAZzRIYC8RAtgCIrgom";
		//readonly string _boxClientId = "7tghv5zhu1i2m2wbjfdu3axj1xlwtnbd";
		//readonly string _boxClientSecret = "Zv69iME8RmRh4JERBRr7BhRdlfRKKCQ7";
		//readonly Uri _boxRedirectUri = new Uri("http://localhost");

		//IAmazonDynamoDB dynamoDBClient;

		//private int successCount = 0;
		//private int failedCount = 0;

		//public SubmitFilesController(IAmazonDynamoDB dynamoDBClient)
		//{
		//    this.dynamoDBClient = dynamoDBClient;
		//}


		//[HttpPost]
		//[Route("SubmitDropbox")]
		//public async Task<IActionResult> PostAsync(SubmitDropBoxFilesRequest request)
		//{
		//    try
		//    {
		//        // check validation
		//        var missingParameter = request.CheckRequiredParameters(new string[] { "submission_id", "submitter_email", "dropbox_url" });

		//        if (missingParameter != null)
		//        {
		//            return BadRequest(new { status = missingParameter + " is required" });
		//        }

		//        // get submission datetime
		//        var submissionDateTime = new DateTime();

		//        if (DateTime.TryParse(request.submission_datetime, out submissionDateTime))
		//        {
		//            // do nothing
		//        }
		//        else
		//        {
		//            submissionDateTime = DateTime.UtcNow;
		//        }

		//        // access dropbox and get list of files

		//        using (var dbx = new DropboxClient(_dropboxAccessToken))
		//        {
		//            var listFolderResult = await dbx.Files.ListFolderAsync(request.dropbox_url, true);

		//            for (var idx = 0; idx < listFolderResult.Entries.Count; idx ++) 
		//            {
		//                var entry = listFolderResult.Entries[idx];

		//                if (entry.IsFile)
		//                {
		//                    var fileName = entry.Name;
		//                    var filePath = entry.PathDisplay.Substring(0, entry.PathDisplay.LastIndexOf("/"));

		//                    var recordWriteResult = await __writeWIPRecordForDropbox(request, fileName, filePath, submissionDateTime);

		//                    if (recordWriteResult)
		//                    {
		//                        successCount++;
		//                    }
		//                    else
		//                    {
		//                        failedCount++;
		//                    }
		//                }
		//            }
		//        }

		//        return Ok(new { status = string.Format("submitted {0} files, {1} completed, {2} failed", successCount + failedCount, successCount, failedCount)});
		//    }
		//    catch (Exception exception)
		//    {
		//        return BadRequest(new { status = exception.Message });
		//    }
		//}


		//[HttpPost]
		//[Route("SubmitBox")]
		//public async Task<IActionResult> PostAsync(SubmitBoxFilesRequest request)
		//{
		//    try
		//    {
		//        // check validation
		//        var missingParameter = request.CheckRequiredParameters(new string[] { "submission_id", "submitter_email", "box_url" });

		//        if (missingParameter != null)
		//        {
		//            return BadRequest(new { status = missingParameter + " is required" });
		//        }

		//        if (request.box_url.StartsWith("/"))
		//        {
		//            request.box_url = request.box_url.Remove(0, 1);
		//        }

		//        // get submission datetime
		//        var submissionDateTime = new DateTime();

		//        if (DateTime.TryParse(request.submission_datetime, out submissionDateTime))
		//        {
		//            // do nothing
		//        }
		//        else
		//        {
		//            submissionDateTime = DateTime.UtcNow;
		//        }

		//        // initialize box.net

		//        var config = new BoxConfig(_boxClientId, _boxClientSecret, _boxRedirectUri);
		//        var session = new OAuthSession(request.access_token, "NOT_NEEDED", 3600, "bearer");
		//        var client = new BoxClient(config, session);

		//        // get source folder id
		//        var sourceFolderId = await __getSourceFolderId(client, request.box_url);

		//        if (sourceFolderId == null)
		//        {
		//            return BadRequest(new { status = "specified box_url doesn't exist" });
		//        }

		//        /// get all files under the target folder and write records for them
		//        await __processAllFilesUnderFolder(client, request.box_url, sourceFolderId, request, submissionDateTime);

		//        return Ok(new { status = string.Format("submitted {0} files, {1} completed, {2} failed", successCount + failedCount, successCount, failedCount) });
		//    }
		//    catch (Exception exception)
		//    {
		//        return BadRequest(new { status = exception.Message });
		//    }
		//}

		//public async Task<string> __getSourceFolderId(BoxClient client, string folderPath)
		//{
		//    var foldersManager = client.FoldersManager;
		//    var folderPathSegments = folderPath.Split("/");
		//    var folderId = "000"; // root
		//    for (var idx = 0; idx < folderPathSegments.Length; idx++)
		//    {
		//        var folderName = folderPathSegments[idx];

		//        var folderContents = await foldersManager.GetFolderItemsAsync(folderId, 1000);
		//        var matchingEntry = folderContents.Entries.Find(entry =>
		//        {
		//            return entry.Type == "folder" && entry.Name == folderName;
		//        });

		//        if (matchingEntry == null)
		//        {
		//            return null;
		//        }

		//        folderId = matchingEntry.Id;
		//    }

		//    return folderId;
		//}

		//public async Task __processAllFilesUnderFolder(BoxClient client, string currentFolderPath, string folderId, SubmitBoxFilesRequest originalRequest, DateTime submissionDateTime)
		//{
		//    var foldersManager = client.FoldersManager;
		//    var folderContents = await foldersManager.GetFolderItemsAsync(folderId, 1000);

		//    for (var idx = 0; idx < folderContents.Entries.Count; idx ++)
		//    {
		//        var entry = folderContents.Entries[idx];

		//        if (entry.Type == "folder")
		//        {
		//            await __processAllFilesUnderFolder(client, currentFolderPath + "/" + entry.Name, entry.Id, originalRequest, submissionDateTime);
		//        }
		//        else
		//        {
		//            var writeRecordResult = await __writeWIPRecordForBox(originalRequest, entry.Name, currentFolderPath, submissionDateTime);

		//            if (writeRecordResult)
		//            {
		//                successCount++;
		//            } 
		//            else
		//            {
		//                failedCount++;
		//            }
		//        }
		//    }
		//}

		//public async Task<bool> __writeWIPRecordForBox(SubmitBoxFilesRequest originalRequest, string fileName, string filePath, DateTime submissionDateTime)
		//{
		//    try
		//    {
		//        var currentTime = DateTimeHelper.GetDateTimeString(DateTime.UtcNow);
		//        var processAttempts = 0;
		//        var request = new PutItemRequest
		//        {
		//            TableName = _924TableName,
		//            Item = new Dictionary<string, AttributeValue>()
		//            {
		//                { "source_file_id", new AttributeValue { S = Guid.NewGuid().ToString() } },
		//                { "create_datetime", new AttributeValue { S = currentTime } },
		//                { "edit_datetime", new AttributeValue { S = currentTime } },
		//                { "original_filename", new AttributeValue { S = fileName } },
		//                { "original_filepath", new AttributeValue { S = filePath } },
		//                { "process_status", new AttributeValue { S = "queued" } },
		//                { "process_attempts", new AttributeValue { N = processAttempts.ToString() } },
		//                { "source_system", new AttributeValue { S = "Box.net" } },
		//                { "source_system_url", new AttributeValue { S = "https://box.net" } },
		//                { "submission_datetime", new AttributeValue { S = DateTimeHelper.GetDateTimeString(submissionDateTime) } },
		//                { "submission_id", new AttributeValue { S = originalRequest.submission_id } },
		//                { "submitter_email", new AttributeValue { S = originalRequest.submitter_email } },
		//            }
		//        };

		//        if (originalRequest.project_id != null)
		//        {
		//            request.Item.Add("project_id", new AttributeValue { S = originalRequest.project_id });
		//        }
		//        if (originalRequest.project_name != null)
		//        {
		//            request.Item.Add("project_name", new AttributeValue { S = originalRequest.project_name });
		//        }

		//        var result = await dynamoDBClient.PutItemAsync(request);

		//        return true;
		//    }
		//    catch (Exception exception)
		//    {
		//        Debug.WriteLine(exception.Message);
		//        return false;
		//    }
		//}


		//public async Task<bool> __writeWIPRecordForDropbox(SubmitDropBoxFilesRequest originalRequest, string fileName, string filePath, DateTime submissionDateTime)
		//{
		//    try
		//    {
		//        var currentTime = DateTimeHelper.GetDateTimeString(DateTime.UtcNow);
		//        var processAttempts = 0;
		//        var request = new PutItemRequest
		//        {
		//            TableName = _924TableName,
		//            Item = new Dictionary<string, AttributeValue>()
		//            {
		//                { "source_file_id", new AttributeValue { S = Guid.NewGuid().ToString() } },
		//                { "create_datetime", new AttributeValue { S = currentTime } },
		//                { "edit_datetime", new AttributeValue { S = currentTime } },
		//                { "original_filename", new AttributeValue { S = fileName } },
		//                { "original_filepath", new AttributeValue { S = filePath } },
		//                { "process_status", new AttributeValue { S = "queued" } },
		//                { "process_attempts", new AttributeValue { N = processAttempts.ToString() } },
		//                { "source_system", new AttributeValue { S = "DropBox" } },
		//                { "source_system_url", new AttributeValue { S = "https://dropbox.com" } },
		//                { "submission_datetime", new AttributeValue { S = DateTimeHelper.GetDateTimeString(submissionDateTime) } },
		//                { "submission_id", new AttributeValue { S = originalRequest.submission_id } },
		//                { "submitter_email", new AttributeValue { S = originalRequest.submitter_email } },
		//            }
		//        };

		//        if (originalRequest.project_id != null)
		//        {
		//            request.Item.Add("project_id", new AttributeValue { S = originalRequest.project_id });
		//        }
		//        if (originalRequest.project_name != null)
		//        {
		//            request.Item.Add("project_name", new AttributeValue { S = originalRequest.project_name });
		//        }

		//        var result = await dynamoDBClient.PutItemAsync(request);

		//        return true;
		//    }
		//    catch (Exception exception)
		//    {
		//        Debug.WriteLine(exception.Message);
		//        return false;
		//    }
		//}

	}
}