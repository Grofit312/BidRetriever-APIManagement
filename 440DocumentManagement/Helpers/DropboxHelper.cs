using System;
using System.Threading.Tasks;
using Dropbox.Api;

namespace _440DocumentManagement.Helpers
{
    public class DropboxHelper
    {
        private DropboxClient dbx = null;

        public DropboxHelper(string accessToken)
        {
            dbx = new DropboxClient(accessToken);
        }

        public async Task<string> GetSharedUrl(string path)
        {
            try
            {
                var link = await dbx.Sharing.ListSharedLinksAsync(path, null, true);
                Dropbox.Api.Sharing.SharedLinkMetadata existingLink = null;

                for (var index = 0; index < link.Links.Count; index++)
                {
                    if (link.Links[index].PathLower == path.ToLower())
                    {
                        existingLink = link.Links[index];
                        break;
                    }
                }

                if (existingLink == null)
                {
                    var result = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(path);
                    return result.Url;
                }
                else
                {
                    string url = link.Links[0].Url;
                    return url;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteFile(string path)
        {
            try
            {
                await dbx.Files.DeleteV2Async(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> MoveFile(string originPath, string targetPath)
        {
            try
            {
                await dbx.Files.MoveV2Async(originPath, targetPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CopyFile(string originPath, string targetPath)
        {
            try
            {
                await dbx.Files.CopyV2Async(originPath, targetPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> isExisting(string path)
        {
            try
            {
                var meta = await dbx.Files.GetMetadataAsync(path);
                return meta != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
