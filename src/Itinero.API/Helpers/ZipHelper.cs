using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Itinero.API.Helpers
{
    public static class ZipHelper
    {
        public static async Task<Stream> Unzip(Stream zipStream)
        {
            var memoryStream = new MemoryStream();
            ZipArchive archive = new ZipArchive(zipStream);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                await entry.Open().CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            throw new Exception("No zip entries in zip file");
        }
    }
}
