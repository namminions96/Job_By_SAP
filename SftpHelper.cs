using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Serilog;

namespace BluePosVoucher
{
    public class SftpHelper
    {
        private readonly ILogger _logger;
        private readonly int Port;
        private readonly string Host;
        private readonly string Username;
        private readonly string Password;

        public SftpHelper(string _host, int _SftpPort, string _username, string _password, ILogger logger)
        {
            Port = _SftpPort;
            Host = _host;
            Username = _username;
            Password = _password;
            _logger = logger;
        }
        public void DownloadAuthen(string source, string destination)
        {
            var count = 0;
            try
            {
                PrepareDownloadFolder(destination);
                KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(Username);
                keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

                ConnectionInfo conInfo = new ConnectionInfo(Host, Port, Username, keybAuth);

                using (SftpClient client = new SftpClient(conInfo))
                {
                    client.Connect();
                    client.ChangeDirectory(source);
                    // List the files and folders of the directory
                    var files = client.ListDirectory(source);
                    files = files.OrderBy(e => e.Name);
                    // Iterate over them
                    foreach (SftpFile file in files)
                    {
                        // If is a file, download it
                        if (!file.IsDirectory && !file.IsSymbolicLink)
                        {
                            DownloadFile(client, file, destination);
                            count++;
                            file.Delete();
                        }
                    }
                    client.Disconnect();
                }
                _logger.Information("Download from sftp: " + count.ToString() + " file");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DownloadAuthen" + source);
            }
        }
        public void DownloadNoAuthen(string source, string destination, bool isMove)
        {
            var count = 0;
            try
            {
                PrepareDownloadFolder(destination);
                //KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(Username);
                //keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

                using (SftpClient client = new SftpClient(Host, Port, Username, Password))
                {
                    client.Connect();
                    client.ChangeDirectory(source);
                    // List the files and folders of the directory
                    var files = client.ListDirectory(source);
                    files = files.OrderBy(e => e.Name);
                    // Iterate over them
                    foreach (SftpFile file in files)
                    {
                        // If is a file, download it
                        if (!file.IsDirectory && !file.IsSymbolicLink)
                        {
                            DownloadFile(client, file, destination);
                            count++;
                            if (isMove)
                            {
                                file.Delete();
                            }

                        }
                    }
                    client.Disconnect();
                }
                _logger.Information("Download from sftp: " + count.ToString() + " file");
            }
            catch (Exception ex)
            {
                _logger.Error("Lỗi",ex);
            }
        }
        public void UploadSftpLinux(string source, string destination, string archive, string extention, short setPermissions = 744, bool isMove = true)
        {
            var count = 0;
            try
            {
                //KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(username);
                //keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
                //ConnectionInfo conInfo = new ConnectionInfo(host, SftpPort, username, keybAuth);

                using (SftpClient client = new SftpClient(Host, Port, Username, Password))
                {
                    client.Connect();
                    client.ChangeDirectory(destination);

                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(source);
                    IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles(extention, System.IO.SearchOption.AllDirectories);

                    foreach (System.IO.FileInfo file in fileList)
                    {
                        using (var fileStream = new FileStream(source + file.Name, FileMode.Open))
                        {
                            //client.BufferSize = 4 * 1024; // bypass Payload error large files
                            client.UploadFile(fileStream, Path.GetFileName(file.Name));
                            if (setPermissions > 0)
                            {
                                //client.BufferSize = 4 * 1024; // bypass Payload error large files
                                SftpFileAttributes myFileAttributes = client.GetAttributes(Path.GetFileName(file.Name));

                                // Then you can see any attributes here..
                                Console.WriteLine(myFileAttributes.OwnerCanRead);

                                // Set to read only for others rwxr--r--
                                myFileAttributes.SetPermissions(setPermissions);

                                client.SetAttributes(Path.GetFileName(file.Name), myFileAttributes);
                            }
                            count++;
                        }
                        if (isMove)
                        {
                            //FileHelper.MoveFileToDestination(source + file.Name, archive);
                        }
                        //FileHelper.WriteLogs("Uploaded file: " + file.Name);
                    }
                    client.Disconnect();
                }
                //FileHelper.WriteLogs("Uploaded " + count.ToString() + " file");
            }
            catch (Exception ex)
            {
                //FileHelper.WriteLogs(ex.ToString());
            }
        }
        //public void UploadSftpWindow(string source, string destination, string archive, string extention, short setPermissions = 744, bool isMoveFile = true, int pageSize = 100)
        //{
        //    var count = 0;
        //    try
        //    {
        //        //KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(username);
        //        //keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
        //        //ConnectionInfo conInfo = new ConnectionInfo(host, SftpPort, username, keybAuth);
        //        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(source);
        //        IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles(extention, System.IO.SearchOption.AllDirectories);
        //        if (fileList.Count() > 0)
        //        {
        //            FileHelper.WriteLogs("===> upload to: " + destination);
        //            pageSize = pageSize <= 0 ? 100 : pageSize;
        //            decimal totalRows = fileList.Count();
        //            if(totalRows > 10000)
        //            {
        //                totalRows = 5000;
        //            }
        //            int pageNumber = (int)Math.Ceiling(totalRows / pageSize);
        //            pageNumber = pageNumber < 1 ? 1 : pageNumber;
        //            FileHelper.WriteLogs("===> Total file: " + totalRows.ToString() + " - PageSize: " + pageSize.ToString() + " - PageNumber: " + pageNumber.ToString());
        //            var queryable = fileList.AsQueryable();
        //            FileHelper.WriteLogs("===> AsQueryable: " + queryable.Count().ToString());

        //            for (var i = 0; i < pageNumber; i++)
        //            {
        //                var dataPage = queryable.Paging(i, pageSize);
        //                FileHelper.WriteLogs("===> Thread " + i.ToString() + " AsQueryable: " + dataPage.Count().ToString());
        //                Thread t = new Thread(() =>
        //                {
        //                    using (SftpClient client = new SftpClient(Host, Port, Username, Password))
        //                    {
        //                        client.Connect();
        //                        client.ChangeDirectory(destination);
        //                        foreach (System.IO.FileInfo file in dataPage)
        //                        {
        //                            using (var fileStream = new FileStream(source + file.Name, FileMode.Open))
        //                            {
        //                                client.UploadFile(fileStream, Path.GetFileName(file.Name), null);
        //                                if (setPermissions > 0)
        //                                {
        //                                    //client.BufferSize = 4 * 1024; // bypass Payload error large files
        //                                    SftpFileAttributes myFileAttributes = client.GetAttributes(Path.GetFileName(file.Name));
        //                                    // Then you can see any attributes here..
        //                                    Console.WriteLine(myFileAttributes.OwnerCanRead);
        //                                    // Set to read only for others rwxr--r--
        //                                    myFileAttributes.SetPermissions(setPermissions);
        //                                    client.SetAttributes(Path.GetFileName(file.Name), myFileAttributes);
        //                                }
        //                                count++;
        //                            }
        //                            if (isMoveFile)
        //                            {
        //                                FileHelper.MoveFileToDestination(source + file.Name, archive);
        //                            }
        //                            Thread.Sleep(100);
        //                        }
        //                        client.Disconnect();
        //                    }
        //                });
        //                t.Start();
        //            }
        //        }
        //        FileHelper.WriteLogs("===> Uploaded " + fileList.Count().ToString() + " file");
        //    }
        //    catch (Exception ex)
        //    {
        //        FileHelper.WriteLogs("===> UploadSftpWindow Exception: " + ex.ToString());
        //    }
        //}
        //public void UploadSftpLinux2(string source, string destination, string archive, string fileType, short setPermissions = 744, bool isMoveFile = true)
        //{
        //    var count = 0;
        //    try
        //    {
        //        KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(Username);
        //        keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
        //        ConnectionInfo conInfo = new ConnectionInfo(Host, Port, Username, keybAuth);
        //        using (SftpClient client = new SftpClient(conInfo))
        //        {
        //            client.Connect();
        //            client.ChangeDirectory(destination);

        //            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(source);
        //            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles(fileType, System.IO.SearchOption.AllDirectories);

        //            foreach (System.IO.FileInfo file in fileList)
        //            {
        //                using (var fileStream = new FileStream(source + file.Name, FileMode.Open))
        //                {
        //                    client.UploadFile(fileStream, Path.GetFileName(file.Name));
        //                    if (setPermissions > 0)
        //                    {
        //                        client.BufferSize = 4 * 1024; // bypass Payload error large files
        //                        SftpFileAttributes myFileAttributes = client.GetAttributes(Path.GetFileName(file.Name));

        //                        Then you can see any attributes here..Console.WriteLine(myFileAttributes.OwnerCanRead);

        //                        Set to read only for others rwxr--r--

        //                       myFileAttributes.SetPermissions(setPermissions);

        //                        client.SetAttributes(Path.GetFileName(file.Name), myFileAttributes);
        //                    }
        //                    count++;
        //                }

        //                if (isMoveFile)
        //                {
        //                    FileHelper.MoveFileToDestination(source + file.Name, archive);
        //                }

        //                FileHelper.WriteLogs("Uploaded file: " + file.Name);
        //            }
        //            client.Disconnect();
        //        }
        //        FileHelper.WriteLogs("Uploaded " + count.ToString() + " file");
        //    }
        //    catch (Exception ex)
        //    {
        //        FileHelper.WriteLogs(ex.ToString());
        //    }
        //}
        private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        {
            foreach (AuthenticationPrompt prompt in e.Prompts)
            {
                if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    prompt.Response = Password;
                }
            }
        }
        public void PrepareDownloadFolder(string _folder)
        {
            try
            {
                if (!Directory.Exists(_folder))
                {
                    Directory.CreateDirectory(_folder);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Lỗi",ex);
            }
        }
        private void DownloadFile(SftpClient client, SftpFile file, string directory)
        {
            using (Stream fileStream = File.OpenWrite(Path.Combine(directory, file.Name)))
            {
                client.DownloadFile(file.FullName, fileStream);
            }
        }
        //public int UploadSftpWindow2(string source, string destination, string archive, string extention)
        //{
        //    var count = 0;
        //    try
        //    {
        //        KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(Username);
        //        keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
        //        ConnectionInfo conInfo = new ConnectionInfo(Host, Port, Username, keybAuth);
        //        using (SftpClient client = new SftpClient(conInfo))
        //        {
        //            client.Connect();
        //            client.ChangeDirectory(destination);

        //            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(source);
        //            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles(extention, System.IO.SearchOption.AllDirectories);

        //            FileHelper.WriteLogs("Loading: " + source);
        //            FileHelper.WriteLogs("Total: " + fileList.Count().ToString() + " file");

        //            foreach (System.IO.FileInfo file in fileList)
        //            {
        //                using (var fileStream = new FileStream(source + file.Name, FileMode.Open))
        //                {
        //                    //client.BufferSize = 4 * 1024; // bypass Payload error large files
        //                    client.UploadFile(fileStream, Path.GetFileName(file.Name));
        //                    count++;
        //                }
        //                FileHelper.MoveFileToDestination(source + file.Name, archive);
        //            }
        //            client.Disconnect();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FileHelper.WriteLogs("UploadSftpWindow Exception: " + ex.ToString());
        //    }
        //    FileHelper.WriteLogs("Upload to: " + destination);
        //    FileHelper.WriteLogs("Uploaded " + count.ToString() + " file");
        //    return count;
        //}
        //public int UploadSftpLinux3(string source, string destination, string archive, string fileType)
        //{
        //    var count = 0;
        //    try
        //    {
        //        KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(Username);
        //        keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
        //        ConnectionInfo conInfo = new ConnectionInfo(Host, Port, Username, keybAuth);
        //        using (SftpClient client = new SftpClient(conInfo))
        //        {
        //            client.Connect();
        //            client.ChangeDirectory(destination);

        //            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(source);
        //            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles(fileType, System.IO.SearchOption.AllDirectories);

        //            foreach (System.IO.FileInfo file in fileList)
        //            {
        //                using (var fileStream = new FileStream(source + file.Name, FileMode.Open))
        //                {
        //                    client.UploadFile(fileStream, Path.GetFileName(file.Name));
        //                    count++;
        //                }
        //                FileHelper.MoveFileToDestination(source + file.Name, archive);
        //            }
        //            client.Disconnect();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        FileHelper.WriteLogs("UploadSftpLinux Exception: " + ex.ToString());
        //    }

        //    FileHelper.WriteLogs("Upload to: " + destination);
        //    FileHelper.WriteLogs("Uploaded " + count.ToString() + " file");
        //    return count;
        //}
    }
}
