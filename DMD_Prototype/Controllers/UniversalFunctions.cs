﻿using DMD_Prototype.Data;
using DMD_Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using DMDLibrary;

namespace DMD_Prototype.Controllers
{   
    public interface ISharedFunct
    {
        public IActionResult ShowPdf(string path);

        public string GetPath(string path);

        public IEnumerable<SerialNumberModel> GetSerialNumbers();

        public List<MTIModel> GetMTIs();

        public List<AccountModel> GetAccounts();

        public List<StartWorkModel> GetStartWork();

        public List<PauseWorkModel> GetPauseWorks();

        public List<ProblemLogModel> GetProblemLogs();

        public List<ModuleModel> GetModules();

        public List<RequestSessionModel> GetRS();

        public IEnumerable<AnnouncementModel> GetAnnouncements();

        public void RecordOriginatorAction(string action, string originator, DateTime date);

        public IEnumerable<UserActionModel> GetUA();

        public void SendEmailNotification(List<string> receivers, string subject, string body);

        public void SendEmailNotification(string receiver, string subject, string body);

        public List<string> GetMultipleusers(string userRole);

        public void BackupHandler(string logType, whichFileEnum whichFile, string sessionId, string setName);


    }

    public class UniversalFunctions : Controller, ISharedFunct
    {
        public UniversalFunctions(AppDbContext context)
        {
            _Db = context;
        }

        private readonly AppDbContext _Db;

        //private readonly string userDir = "V:\\DMD_Documents_Directory\\User_Sessions";
        //private readonly string mainDir = "V:\\DMD_Documents_Directory\\Documents";
        //private readonly string tempDir = "V:\\DMD_Documents_Directory\\DMD_Temporary_Files";
        //private readonly string travelerForBackupDir = "V:\\DMD_Documents_Directory\\ForBackup\\Travelers";
        //private readonly string configForBackupDir = "V:\\DMD_Documents_Directory\\ForBackup\\Configuration Log";
        //private readonly string testForBackupDir = "V:\\DMD_Documents_Directory\\ForBackup\\Test Equipment Log";

        private readonly string userDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\User_Sessions";
        private readonly string mainDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\Documents";
        private readonly string tempDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\DMD_Temporary_Files";
        private readonly string travelerForBackupDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\ForBackup\\Travelers";
        private readonly string configForBackupDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\ForBackup\\Configuration Log";
        private readonly string testForBackupDir = "D:\\DMDPortalFiles\\DMD_Documents_Directory\\ForBackup\\Test Equipment Log";

        private readonly string travelerBackupDir = "A:\\DMD Portal Backups\\Travelers";
        private readonly string configBackupDir = "A:\\DMD Portal Backups\\Configuration Logs";
        private readonly string testBackupDir = "A:\\DMD Portal Backups\\Test Equipment Logs";
        private readonly string plBackupDir = "A:\\DMD Portal Backups\\Problem Logs";

        private readonly string wsFolderName = "1_WORKMANSHIP_STANDARD_FOLDER";
        private readonly string wsName = "WS.pdf";
        private readonly string userTravName = "Traveler.xlsx";
        private readonly string userLogName = "Logsheet.xlsx";

        private readonly string travName = "TravelerFileDoNotEdit.xlsx";
        private readonly string maindocName = "MainDoc.pdf";
        private readonly string assydrawingName = "AssyDrawing.pdf";
        private readonly string bomName = "BOM.pdf";
        private readonly string schemaName = "SchematicDiag.pdf";
        private readonly string oplName = "OPL.pdf";
        private readonly string prcoName = "PRCO.pdf";
        private readonly string derogationName = "Derogation.pdf";
        private readonly string memoName = "EngineeringMemo.pdf";

        public void BackupHandler(string logType, whichFileEnum whichFile, string sessionId, string setName)
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

        public IEnumerable<AnnouncementModel> GetAnnouncements()
        {
            return _Db.AnnouncementDb;
        }

        public IEnumerable<SerialNumberModel> GetSerialNumbers()
        {
            return _Db.SerialNumberDb;
        }

        public IEnumerable<UserActionModel> GetUA()
        {
            return _Db.UADb;
        }

        public void RecordOriginatorAction(string action, string originator, DateTime date)
        {
            _Db.UADb.Add(new UserActionModel().CreateAction(action, originator, date)); 
        }

        public List<RequestSessionModel> GetRS()
        {
            return _Db.RSDb.ToList();
        }

        public List<ModuleModel> GetModules()
        {
            return _Db.ModuleDb.ToList();
        }

        public List<ProblemLogModel> GetProblemLogs()
        {
            return _Db.PLDb.ToList();
        }

        public List<MTIModel> GetMTIs()
        {
            return _Db.MTIDb.ToList();
        }

        public List<AccountModel> GetAccounts()
        {
            return _Db.AccountDb.ToList();
        }

        public List<StartWorkModel> GetStartWork()
        {
            return _Db.StartWorkDb.ToList();
        }

        public List<PauseWorkModel> GetPauseWorks()
        {
            return _Db.PauseWorkDb.ToList();
        }

        public string GetPath(string whichPath)
        {
            switch (whichPath)
            {
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

        public IActionResult ShowPdf(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);

            return File(ms.ToArray(), "application/pdf");
        }

        public void SendEmailNotification(List<string> receivers, string subject, string body)
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

        public void SendEmailNotification(string receiver, string subject, string body)
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

        public List<string> GetMultipleusers(string userRole)
        {
            List<string> listOfPlEmails = new List<string>();
            IEnumerable<AccountModel> plAccounts = GetAccounts().Where(j => j.Role == userRole);

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
