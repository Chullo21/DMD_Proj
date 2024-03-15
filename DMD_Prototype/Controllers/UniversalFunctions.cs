using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using DMDLibrary;

namespace DMD_Prototype.Controllers
{   
    public interface ISharedFunct
    {
        public Task<string> GetAssyDrawingName();

        public Task<string> GetBOMName();

        public Task<string> GetSchemaDiagramName();

        public Task<string> GetUserDocsPath();

        public Task<string> GetMainDocsPath();

        public Task<string> GetMainDocName();

        public Task<IActionResult> ShowPdf(string path);

        public Task<string> GetPath(string path);

        public Task<IEnumerable<SerialNumberModel>> GetSerialNumbers();

        public Task<IEnumerable<MTIModel>> GetMTIs();

        public Task<IEnumerable<AccountModel>> GetAccounts();

        public Task<IEnumerable<StartWorkModel>> GetStartWork();

        public Task<IEnumerable<PauseWorkModel>> GetPauseWorks();

        public Task<IEnumerable<ProblemLogModel>> GetProblemLogs();

        public Task<IEnumerable<ModuleModel>> GetModules();

        public Task<IEnumerable<RequestSessionModel>> GetRS();

        public Task<IEnumerable<AnnouncementModel>> GetAnnouncements();

        public Task RecordOriginatorAction(string action, string originator, DateTime date);

        public Task<IEnumerable<UserActionModel>> GetUA();

        public Task SendEmailNotification(List<string> receivers, string subject, string body);

        public Task SendEmailNotification(string receiver, string subject, string body);

        public Task<IEnumerable<string>> GetMultipleusers(string userRole);

        public Task BackupHandler(string logType, whichFileEnum whichFile, string sessionId, string setName);

    }

    public class UniversalFunctions : Controller, ISharedFunct
    {
        public UniversalFunctions(AppDbContext context)
        {
            _Db = context;
        }

        private readonly AppDbContext _Db;

        private readonly string userDir = "V:\\DMD_Documents_Directory\\User_Sessions";
        private readonly string mainDir = "V:\\DMD_Documents_Directory\\Documents";
        private readonly string tempDir = "V:\\DMD_Documents_Directory\\DMD_Temporary_Files";
        private readonly string travelerForBackupDir = "V:\\DMD_Documents_Directory\\ForBackup\\Travelers";
        private readonly string configForBackupDir = "V:\\DMD_Documents_Directory\\ForBackup\\Configuration Log";
        private readonly string testForBackupDir = "V:\\DMD_Documents_Directory\\ForBackup\\Test Equipment Log";
        private readonly string configDir = "V:\\DMD_Documents_Directory\\Document Templates\\Configuration Logsheet\\Configuration Logsheet.xlsx";
        private readonly string testDir = "V:\\DMD_Documents_Directory\\Document Templates\\Test Equipment Logsheet\\Test Equipment Logsheet.xlsx";
        private readonly string mpiDir = "V:\\DMD_Documents_Directory\\Document Templates\\MPI Traveler\\TravelerConfig.txt";
        private readonly string mpiFileDir = "V:\\DMD_Documents_Directory\\Document Templates\\MPI Traveler\\MPITemplate.xlsx";
        private readonly string mtiDir = "V:\\DMD_Documents_Directory\\Document Templates\\MTI Traveler\\TravelerConfig.txt";
        private readonly string mtiFileDir = "V:\\DMD_Documents_Directory\\Document Templates\\MPI Traveler\\MTITemplate.xlsx";
        private readonly string dispossableDir = "V:\\DMD_Documents_Directory\\DMD_Temporary_Files";

        //private readonly string userDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\User_Sessions";
        //private readonly string mainDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Documents";
        //private readonly string tempDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\DMD_Temporary_Files";
        //private readonly string travelerForBackupDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\ForBackup\\Travelers";
        //private readonly string configForBackupDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\ForBackup\\Configuration Log";
        //private readonly string testForBackupDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\ForBackup\\Test Equipment Log";
        //private readonly string configDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Document Templates\\Configuration Logsheet\\Configuration Logsheet.xlsx";
        //private readonly string testDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Document Templates\\Test Equipment Logsheet\\Test Equipment Logsheet.xlsx";
        //private readonly string mpiDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Document Templates\\MPI Traveler\\TravelerConfig.txt";
        //private readonly string mpiFileDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Document Templates\\MPI Traveler\\MPITemplate.xlsx";
        //private readonly string mtiDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Document Templates\\MTI Traveler\\TravelerConfig.txt";
        //private readonly string mtiFileDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Document Templates\\MPI Traveler\\MTITemplate.xlsx";
        //private readonly string dispossableDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\DMD_Temporary_Files";

        private readonly string travelerBackupDir = "A:\\DMD Portal Backups\\Travelers";
        private readonly string configBackupDir = "A:\\DMD Portal Backups\\Configuration Logs";
        private readonly string testBackupDir = "A:\\DMD Portal Backups\\Test Equipment Logs";
        private readonly string plBackupDir = "A:\\DMD Portal Backups\\Problem Logs";

        private readonly string wsFolderName = "1_WORKMANSHIP_STANDARD_FOLDER";
        private readonly string wsName = "WS.pdf";
        private readonly string userTravName = "Traveler.xlsx";
        private readonly string userLogName = "Logsheet.xlsx";

        public readonly string travName = "TravelerFileDoNotEdit.xlsx";
        public readonly string maindocName = "MainDoc.pdf";

        public readonly string assydrawingName = "AssyDrawing.pdf";
        public readonly string bomName = "BOM.pdf";
        public readonly string schemaName = "SchematicDiag.pdf";

        public readonly string oplName = "OPL.pdf";
        public readonly string prcoName = "PRCO.pdf";
        public readonly string derogationName = "Derogation.pdf";
        public readonly string memoName = "EngineeringMemo.pdf";

        public async Task<string> GetMainDocName()
        {
            return this.maindocName;
        }

        public async Task<string> GetUserDocsPath()
        {
            return this.userDir;
        }

        public async Task<string> GetMainDocsPath()
        {
            return this.mainDir;
        }

        public async Task<string> GetAssyDrawingName()
        {
            return this.assydrawingName;
        }

        public async Task<string> GetBOMName()
        {
            return this.bomName;
        }

        public async Task<string> GetSchemaDiagramName()
        {
            return this.schemaName;
        }

        public async Task BackupHandler(string logType, whichFileEnum whichFile, string sessionId, string setName)
        {
            string saveInIdentifier = travelerForBackupDir;
            string srcDir;

            switch (whichFile)
            {
                case whichFileEnum.Traveler:
                    {
                        srcDir = Path.Combine(userDir, sessionId, userTravName);
                        break;
                    }
                case whichFileEnum.Log:
                    {
                        srcDir = Path.Combine(userDir, sessionId, userLogName);
                        saveInIdentifier = logType.ToLower() == "c" ? configForBackupDir : testForBackupDir;
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            if (System.IO.File.Exists(srcDir))
            {
                COMHandler converter = new COMHandler();

                converter.ConvertExceltoPdfAndStoreInSpecifiedPath(srcDir, saveInIdentifier, $"{setName}.pdf");
            }
        }

        public async Task<IEnumerable<AnnouncementModel>> GetAnnouncements()
        {
            return _Db.AnnouncementDb;
        }

        public async Task<IEnumerable<SerialNumberModel>> GetSerialNumbers()
        {
            return _Db.SerialNumberDb;
        }

        public async Task<IEnumerable<UserActionModel>> GetUA()
        {
            return _Db.UADb;
        }

        public async Task RecordOriginatorAction(string action, string originator, DateTime date)
        {
            _Db.UADb.Add(new UserActionModel().CreateAction(action, originator, date)); 
        }

        public async Task<IEnumerable<RequestSessionModel>> GetRS()
        {
            return _Db.RSDb;
        }

        public async Task<IEnumerable<ModuleModel>> GetModules()
        {
            return _Db.ModuleDb;
        }

        public async Task<IEnumerable<ProblemLogModel>> GetProblemLogs()
        {
            return _Db.PLDb;
        }

        public async Task<IEnumerable<MTIModel>> GetMTIs()
        {
            return _Db.MTIDb;
        }

        public async Task<IEnumerable<AccountModel>> GetAccounts()
        {
            return _Db.AccountDb;
        }

        public async Task<IEnumerable<StartWorkModel>> GetStartWork()
        {
            return _Db.StartWorkDb;
        }

        public async Task<IEnumerable<PauseWorkModel>> GetPauseWorks()
        {
            return _Db.PauseWorkDb;
        }

        public async Task<string> GetPath(string whichPath)
        {
            switch (whichPath)
            {
                case "mpiFileDir":
                    {
                        return mpiFileDir;
                    }
                case "mtiFileDir":
                    {
                        return mtiFileDir;
                    }
                case "disDir":
                    {
                        return dispossableDir;
                    }
                case "testDir":
                    {
                        return testDir;
                    }
                case "configDir":
                    {
                        return configDir;
                    }
                case "mtiDir":
                    {
                        return mtiDir;
                    }
                case "mpiDir":
                    {
                        return mpiDir;
                    }
                case "plBackup":
                    {
                        return plBackupDir;
                    }
                case "testBackup":
                    {
                        return testBackupDir;
                    }
                case "configBackup":
                    {
                        return configBackupDir;
                    }
                case "travelerBackup":
                    {
                        return travelerBackupDir;
                    }
                case "wsf":
                    {
                        return wsFolderName;
                    }
                case "ws":
                    {
                        return wsName;
                    }
                case "schema":
                    {
                        return schemaName;
                    }
                case "bom":
                    {
                        return bomName;
                    }
                case "assy":
                    {
                        return assydrawingName;
                    }
                case "opl":
                    {
                        return oplName;
                    }
                case "prco":
                    {
                        return prcoName;
                    }
                case "derog":
                    {
                        return derogationName;
                    }
                case "memo":
                    {
                        return memoName;
                    }
                case "mainDir":
                    {
                        return mainDir;
                    }
                case "mainDoc":
                    {
                        return maindocName;
                    }
                case "userDir":
                    {
                        return userDir;
                    }
                case "tempDir":
                    {
                        return tempDir;
                    }
                case "userTravName":
                    {
                        return userTravName;
                    }
                case "travName":
                    {
                        return travName;
                    }
                case "logName":
                    {
                        return userLogName;
                    }
                default:
                    {
                        return "error";
                    }
            }
        }

        public async Task<IActionResult> ShowPdf(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);

            return File(ms.ToArray(), "application/pdf");
        }

        public async Task SendEmailNotification(List<string> receivers, string subject, string body)
        {
            EmailModel dmdEmail = new EmailModel().SecondEmailAccount();

            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(dmdEmail.Email, dmdEmail.Password);
            client.EnableSsl = true;

            MailMessage mail = new();

            mail.From = new MailAddress(dmdEmail.Email, "DMD Notificator");
            mail.Subject = subject;
            mail.Body = body;

            foreach (string receiver in receivers.Where(j => j != ""))
            {
                mail.To.Add(receiver);
            }

            if (mail.To.Count > 0 && mail.To != null)
            {
                client.Send(mail);
            }
        }

        public async Task SendEmailNotification(string receiver, string subject, string body)
        {
            EmailModel dmdEmail = new EmailModel().FirstEmailAccount();

            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(dmdEmail.Email, dmdEmail.Password);
            client.EnableSsl = true;

            MailMessage mail = new();

            mail.From = new MailAddress(dmdEmail.Email, "DMD Notificator");
            mail.Subject = subject;
            mail.Body = body;

            if (!string.IsNullOrEmpty(receiver))
            {
                mail.To.Add(receiver);                
            }
            
            if (mail.To.Count > 0)
            {
                client.Send(mail);
            }
        }

        public async Task<IEnumerable<string>> GetMultipleusers(string userRole)
        {
            List<string> listOfPlEmails = new List<string>();
            IEnumerable<AccountModel> plAccounts = (await GetAccounts()).Where(j => j.Role == userRole);

            foreach (var pls in plAccounts)
            {
                if (!string.IsNullOrEmpty(pls.Email) && !string.IsNullOrEmpty(pls.Sec) && !string.IsNullOrEmpty(pls.Dom))
                {
                    listOfPlEmails.Add($"{pls.Email}{pls.Sec}{pls.Dom}");
                }
            }

            return listOfPlEmails;
        }
    }

    public enum Tri
    {
        v,
        iv,
        d
    }

    public enum whichFileEnum
    {
        Traveler,
        Log
    }
}
