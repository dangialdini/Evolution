using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Evolution.Extensions;
using Evolution.Enumerations;
using Renci.SshNet;

namespace Evolution.FTPService {
    public class FTPService {

        private string      _hostName = "",
                            _loginName = "",
                            _password = "";
        private int         _portNo;
        private FTPProtocol _protocol;

        public FTPService(string hostName, string loginName, string password, FTPProtocol protocol) {
            _hostName = hostName;
            _loginName = loginName;
            _password = password;
            _protocol = protocol;

            switch(_protocol) {
            case FTPProtocol.FTP:   // 21
            case FTPProtocol.FTPS:  // 21 or 990
                _portNo = 21;
                break;
            case FTPProtocol.SFTP:  // 22
                _portNo = 22;
                break;
            }
        }

        public bool DownloadFile(string source, string target, 
                                 List<string> downloadedFiles,
                                 ref string errorMsg) {
            // Source examples:
            //      /path/filename.ext          Single file
            //      /path                       Multiple files (wildcards not allowed)
            // Target examples:
            //      c:\temp\filename.ext        Single file
            //      c:\temp                     Multiple files

            bool    bRc = false,
                    bMultiple = false;
            int     bytesRead = 0;
            byte[]  buffer = new byte[1024];
            string  tempTarget;

            try {
                if(downloadedFiles != null) downloadedFiles.Clear();

                List<string> fileList = new List<string>();
                if (source.IsFileSpec()) {
                    // Source file is a file spec, so get a list of matching files
                    bRc = GetFTPFileList(source, ref fileList, ref errorMsg);
                    bMultiple = true;
                } else {
                    fileList.Add(source);
                }

                if (!bRc) {
                    if (_protocol == FTPProtocol.SFTP) {
                        using (SftpClient client = new SftpClient(_hostName, _portNo, _loginName, _password)) {
                            client.Connect();
                            foreach (var fileName in fileList) {
                                tempTarget = target;
                                if (bMultiple) tempTarget = target + "\\" + fileName.FileName();
                                using (FileStream fs = new FileStream(tempTarget, FileMode.Create)) {
                                    client.BufferSize = 4 * 1024;
                                    client.DownloadFile(fileName, fs);
                                }
                                if (downloadedFiles != null) downloadedFiles.Add(tempTarget);
                            }
                        }

                    } else {
                        foreach (var fileName in fileList) {
                            tempTarget = target;
                            if (bMultiple) tempTarget = target + "\\" + fileName.FileName();

                            Uri sourceUri = makeUri(fileName);
                            if (sourceUri.Scheme != Uri.UriSchemeFtp) {
                                errorMsg = "Error: Invalid source URI format!";
                                bRc = true;

                            } else {
                                // ftp://login:password@ftp.domain.com.au:21/folder/filename.ext
                                FtpWebRequest request = CreateFtpWebRequest(sourceUri, (_protocol == FTPProtocol.FTPS));
                                request.Method = WebRequestMethods.Ftp.DownloadFile;

                                using (var resp = request.GetResponse()) {
                                    using (Stream reader = resp.GetResponseStream()) {
                                        using (BinaryWriter writer = new BinaryWriter(File.Open(tempTarget, FileMode.Create))) {

                                            do {
                                                bytesRead = reader.Read(buffer, 0, buffer.Length);
                                                if (bytesRead != 0) writer.Write(buffer, 0, bytesRead);
                                            } while (bytesRead != 0);
                                        }
                                    }
                                }
                                if (downloadedFiles != null) downloadedFiles.Add(tempTarget);
                            }
                        }
                    }
                }

            } catch (Exception e1) {
                errorMsg = "Error: " + e1.Message;
                if (e1.InnerException != null && !string.IsNullOrEmpty(e1.InnerException.Message)) errorMsg += " : " + e1.InnerException;
                bRc = true;
            }
            return bRc;
        }

        public bool UploadFile(string sourceFile, string targetFolder, 
                               ref string errorMsg) {
            // Source examples:
            //      c:\temp\filename.ext        Single file
            //      c:\temp\filename.*          Multiple files
            // Target examples:
            //      /path/filename.ext          Single file
            //      /path                       Multiple files

            bool bRc = false,
                 bMultiple = false;
            string targetFile;

            try {
                List<string> fileList = new List<string>();
                if (sourceFile.IndexOf("*") != -1 || sourceFile.IndexOf("?") != -1) {
                    // Source file is a file spec, so get a list of matching files
                    bRc = GetLocalFileList(sourceFile, ref fileList, ref errorMsg);
                    bMultiple = true;
                } else {
                    fileList.Add(sourceFile);
                }

                if (!bRc) {
                    if (_protocol == FTPProtocol.SFTP) {
                        using (SftpClient client = new SftpClient(_hostName, _portNo, _loginName, _password)) {
                            client.Connect();
                            foreach (var file in fileList) {
                                if (bMultiple) {
                                    targetFile = targetFolder + "/" + file.FileName();
                                } else {
                                    targetFile = targetFolder.FolderName() + "/" + file.FileName();
                                }
                                using (FileStream fs = new FileStream(file, FileMode.Open)) {
                                    client.BufferSize = 4 * 1024;
                                    client.UploadFile(fs, targetFile);
                                }
                            }
                        }

                    } else {
                        foreach (var file in fileList) {
                            if (bMultiple) {
                                targetFile = targetFolder + "/" + file.FileName();
                            } else {
                                targetFile = targetFolder.FolderName();
                                if(targetFile.Right(1) != "/") targetFile += "/";
                                targetFile += file.FileName();
                            }
                            Uri targetUri = makeUri(targetFile);
                            if (targetUri.Scheme != Uri.UriSchemeFtp) {
                                errorMsg = "Error: Invalid target URI format!";
                                bRc = true;

                            } else {
                                FtpWebRequest ftpRequest = CreateFtpWebRequest(targetUri, (_protocol == FTPProtocol.FTPS));
                                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                                FileInfo ff = new FileInfo(file);
                                byte[] fileContents = new byte[ff.Length];

                                using (FileStream fr = ff.OpenRead()) {
                                    fr.Read(fileContents, 0, Convert.ToInt32(ff.Length));
                                }

                                using (Stream writer = ftpRequest.GetRequestStream()) {
                                    writer.Write(fileContents, 0, fileContents.Length);
                                }

                                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                                errorMsg += ftpResponse.StatusDescription;
                            }
                        }
                    }
                }

            } catch (Exception e1) {
                errorMsg = "Error: " + e1.Message;
                if (e1.InnerException != null && !string.IsNullOrEmpty(e1.InnerException.Message)) errorMsg += " : " + e1.InnerException;
                bRc = true;
            }
            return bRc;
        }

        public bool DeleteFile(string targetFile, ref string errorMsg) {
            bool bRc = false;

            try {
                if (_protocol == FTPProtocol.SFTP) {
                    using (SftpClient client = new SftpClient(_hostName, _portNo, _loginName, _password)) {
                        client.Connect();
                        client.DeleteFile(targetFile);
                    }

                } else {
                    Uri targetUri = makeUri(targetFile);
                    if (targetUri.Scheme != Uri.UriSchemeFtp) {
                        errorMsg = "Error: Invalid target URI format!";
                        bRc = true;

                    } else {
                        FtpWebRequest ftpRequest = CreateFtpWebRequest(targetUri, (_protocol == FTPProtocol.FTPS));
                        ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                        FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                        errorMsg = ftpResponse.StatusDescription;
                    }
                }

            } catch (Exception e1) {
                errorMsg = "Error: " + e1.Message;
                if (e1.InnerException != null && !string.IsNullOrEmpty(e1.InnerException.Message)) errorMsg += " : " + e1.InnerException;
                bRc = true;
            }
            return bRc;
        }

        public bool MoveFile(string sourceFileName, string targetFileName, 
                             ref string errorMsg, bool overwriteExisting = false) {
            bool bRc = false;

            try {
                string tempMsg = "";
                if (overwriteExisting) DeleteFile(targetFileName, ref tempMsg);

                if (_protocol == FTPProtocol.SFTP) {
                    using (SftpClient client = new SftpClient(_hostName, _portNo, _loginName, _password)) {
                        client.Connect();
                        client.RenameFile(sourceFileName, targetFileName);
                    }

                } else {
                    Uri sourceUri = makeUri(sourceFileName);
                    if (sourceUri.Scheme != Uri.UriSchemeFtp) {
                        errorMsg = "Error: Invalid source URI format!";
                        bRc = true;

                    } else {
                        FtpWebRequest ftpRequest = CreateFtpWebRequest(sourceUri, (_protocol == FTPProtocol.FTPS));
                        ftpRequest.Method = WebRequestMethods.Ftp.Rename;
                        ftpRequest.RenameTo = targetFileName;

                        FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                        errorMsg = ftpResponse.StatusDescription;
                    }
                }

            } catch (Exception e1) {
                errorMsg = "Error: " + e1.Message;
                if (e1.InnerException != null && !string.IsNullOrEmpty(e1.InnerException.Message)) errorMsg += " : " + e1.InnerException;
                bRc = true;
            }
            return bRc;
        }

        public bool GetLocalFileList(string sourceSpec, ref List<string> fileList, ref string errorMsg) {
            bool bRc = false;

            try {
                fileList = Directory.GetFiles(sourceSpec.FolderName(), "*." + sourceSpec.FileExtension())
                                    .ToList();

            } catch (Exception e1) {
                errorMsg = "Error: " + e1.Message;
                if (e1.InnerException != null && !string.IsNullOrEmpty(e1.InnerException.Message)) errorMsg += " : " + e1.InnerException;
                bRc = true;
            }
            return bRc;
        }

        public bool GetFTPFileList(string sourceFolder, ref List<string> fileList, ref string errorMsg) {
            // The parameter must be a folder name with NO wildcard/filespec as SftpClient doesn't support it.
            // The returned list will only contain files (folders are filtered out)
            bool bRc = false;

            try {
                if (_protocol == FTPProtocol.SFTP) {
                    using (SftpClient client = new SftpClient(_hostName, _portNo, _loginName, _password)) {
                        client.Connect();

                        // SftpClient doesn't support wildcards in ListDirectory
                        fileList = client.ListDirectory(sourceFolder)
                                         .Where(fl => fl.Length > 0)
                                         .Select(fl => sourceFolder.TrimEnd('/') + "/" + fl.Name).ToList();
                    }

                } else {
                    Uri sourceUri = makeUri(sourceFolder);
                    if (sourceUri.Scheme != Uri.UriSchemeFtp) {
                        errorMsg = "Error: Invalid source URI format!";
                        bRc = true;

                    } else {
                        // ftp://login:password@ftp.domain.com.au:21/folder/filename.ext
                        FtpWebRequest request = CreateFtpWebRequest(sourceUri, (_protocol == FTPProtocol.FTPS));
                        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                        // FtpWebRequest requires a wildcard file name otherwise
                        // it will return folder names in the list
                        using (var resp = request.GetResponse()) {
                            using (StreamReader reader = new StreamReader(resp.GetResponseStream())) {
                                string line;
                                while ((line = reader.ReadLine()) != null) {
                                    if (line[0] != 'd') fileList.Add(sourceFolder.TrimEnd('/') + "/" + line.Substring(49));
                                }
                            }
                        }
                    }
                }

            } catch (Exception e1) {
                errorMsg = "Error: " + e1.Message;
                if (e1.InnerException != null && !string.IsNullOrEmpty(e1.InnerException.Message)) errorMsg += " : " + e1.InnerException;
                bRc = true;
            }
            return bRc;
        }

        private FtpWebRequest CreateFtpWebRequest(Uri sourceUri, bool isSSL) {
            ignoreExpiredCertificate();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(sourceUri);

            // Set proxy to null. Under current configuration if this option is not set then the proxy that is used will
            // get an html response from the web content gateway (firewall monitoring system)
            request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true;
            request.EnableSsl = isSSL;

            request.Credentials = new NetworkCredential(_loginName, _password);

            return request;
        }

        private void ignoreExpiredCertificate() {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
            delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                    System.Net.Security.SslPolicyErrors sslPolicyErrors) {
                                        return true; // **** Always accept
                                    };
        }

        private Uri makeUri(string fileName) {
            string rc = "";
            rc += "ftp://";
            rc += _hostName;
            if (!string.IsNullOrEmpty(fileName) && fileName[0] != '/') rc += "/";
            rc += fileName;
            return new Uri(rc);
        }
    }
}
