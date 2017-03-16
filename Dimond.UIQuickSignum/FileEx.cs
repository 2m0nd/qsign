using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dimond.QuickSignum;
using Ionic.Zip;

namespace UIQuickSignum
{
    public class FileEx
    {
        public static IEnumerable<FileEx> CreateFileExs(string sourcePath, FileType type)
        {
            if (type == FileType.File)
                return new FileEx[] { new FileEx(sourcePath) };
            else
            {
                using (var zip = ZipFile.Read(sourcePath))
                {
                    var zipEntries = zip.ToList().Where(entry => !entry.IsDirectory).Select(ze => new FileEx(sourcePath, FileType.Zip, ze.FileName));
                    return zipEntries;
                }
            }
        }

        FileEx(string sourcePath, FileType type = FileType.File, string zipEntryPath = null)
        {
            SourcePath = sourcePath;
            Type = type;
            ZipEntryPath = zipEntryPath ?? string.Empty;
        }

        public string ZipEntryPath { get; private set; }
        public string SourcePath { get; private set; }
        public FileType Type { get; private set; }

        public enum FileType
        {
            File = 0,
            Zip = 1,
        }

        public override string ToString()
        {
            if (Type == FileType.Zip)
            {
                return string.Format("../{0} :: {1}", Path.GetFileName(SourcePath), ZipEntryPath);
            }
            else
            {
                return SourcePath;
            }
        }

        public void SaveSignature(byte[] signature)
        {
            if (Type == FileType.Zip)
            {
                using (var zip = ZipFile.Read(SourcePath))
                {
                    var zipEntry = zip[ZipEntryPath];
                    var entryNameForSignature = string.Format("{0}{1}", zipEntry.FileName, QuickSignum.SignExtension);
                    
                    if (zip.ContainsEntry(entryNameForSignature))
                        zip.RemoveEntry(entryNameForSignature);

                    zip.AddEntry(entryNameForSignature, signature);
                    zip.Save();
                }
            }
            else
            {
                using (var writeStream = new FileStream(string.Format("{0}{1}", SourcePath, QuickSignum.SignExtension), FileMode.Create, FileAccess.Write))
                {
                    writeStream.Write(signature, 0, signature.Count());
                    writeStream.Close();
                }
            }
        }

        public byte[] GetFileAsByteArray()
        {
            if (Type == FileType.Zip)
            {
                using (var zip = ZipFile.Read(SourcePath))
                {
                    var zipEntry = zip[ZipEntryPath];
                    var memStream = new MemoryStream();
                    zipEntry.Extract(memStream);
                    memStream.Position = 0;
                    return memStream.ToArray();
                }
            }
            else
            {
                return File.ReadAllBytes(SourcePath);
            }
        }

        public byte[] GetSignatureAsByteArray()
        {
            if (Type == FileType.Zip)
            {
                using (var zip = ZipFile.Read(SourcePath))
                {
                    var signatureEntryPath = ZipEntryPath + QuickSignum.SignExtension;
                    if (!zip.Entries.Any(entry => entry.FileName.Equals(signatureEntryPath)))
                        return null;
                    else
                    {
                        var zipEntry = zip[signatureEntryPath];
                        var memStream = new MemoryStream();
                        zipEntry.Extract(memStream);
                        memStream.Position = 0;
                        return memStream.ToArray();
                    }
                }
            }
            else
            {
                var signatureFilePath = SourcePath + QuickSignum.SignExtension;
                if (!File.Exists(signatureFilePath))
                    return null;
                else
                    return File.ReadAllBytes(signatureFilePath);
            }
        }
    }
}
