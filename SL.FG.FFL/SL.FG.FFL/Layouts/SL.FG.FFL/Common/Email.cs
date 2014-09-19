using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SL.FG.FFL.Layouts.SL.FG.FFL.Common
{

    public class Message
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class Email
    {
        /// <summary>
        /// this method is used to get values againt provided key
        /// </summary>
        /// <param name="configList"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool SendEmail(List<Message> lstMessage)
        {
            try
            {
                string smtp = null;

                if (lstMessage != null && lstMessage.Count > 0)
                {
                    using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb oSPWeb = oSPsite.OpenWeb())
                        {
                            SPList spList = oSPWeb.Lists["CommonDictionary"];

                            if (spList != null)
                            {
                                SPQuery query = new SPQuery();
                                string key = "SMTP";

                                smtp = Utility.GetValueByKey(key);

                                if (!String.IsNullOrEmpty(smtp))
                                {
                                    SmtpClient smtpClient = new SmtpClient(smtp);

                                    foreach (var message in lstMessage)
                                    {
                                        if (!String.IsNullOrEmpty(message.To) && !String.IsNullOrEmpty(message.From))
                                        {
                                            using (MailMessage msg = new MailMessage())
                                            {
                                                MailAddress mailFrom = new MailAddress(message.From);
                                                msg.To.Add(message.To);
                                                msg.From = mailFrom;
                                                msg.Subject = message.Subject;
                                                msg.Body = message.Body;
                                                msg.IsBodyHtml = true;
                                                smtpClient.Send(msg);
                                            }

                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Send Email!!!", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return false;
        }

        public static bool SendEmail(Message message)
        {
            try
            {
                string smtp = null;

                if (!String.IsNullOrEmpty(message.To) && !String.IsNullOrEmpty(message.From))
                {
                    using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb oSPWeb = oSPsite.OpenWeb())
                        {
                            SPList spList = oSPWeb.Lists["CommonDictionary"];

                            if (spList != null)
                            {
                                SPQuery query = new SPQuery();
                                string key = "SMTP";
                                smtp = Utility.GetValueByKey(key);

                                if (!String.IsNullOrEmpty(smtp))
                                {
                                    SmtpClient smtpClient = new SmtpClient(smtp);

                                    using (MailMessage msg = new MailMessage())
                                    {
                                        MailAddress mailFrom = new MailAddress(message.From);
                                        msg.To.Add(message.To);
                                        msg.From = mailFrom;
                                        msg.Subject = message.Subject;
                                        msg.Body = message.Body;
                                        msg.IsBodyHtml = true;
                                        smtpClient.Send(msg);
                                    }

                                    return true;
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("Send Email!!!", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return false;
        }
    }
}
