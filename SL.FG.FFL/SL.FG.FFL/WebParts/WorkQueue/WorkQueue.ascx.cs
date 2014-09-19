using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;

using Microsoft.SharePoint;
using System.Data;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Administration;
using SL.FG.FFL.Layouts.SL.FG.FFL.Common;
using System.Text;
using System.Globalization;

namespace SL.FG.FFL.WebParts.WorkQueue
{
    [ToolboxItemAttribute(false)]
    public partial class WorkQueue : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public WorkQueue()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb oSPWeb = oSPsite.OpenWeb())
                        {
                            SPUser currentUser = oSPWeb.CurrentUser;

                            if (currentUser != null)
                            {
                                FillMSAScheduleWorkQueue(oSPWeb, currentUser);
                                FillMSAWorkQueue(oSPWeb, currentUser);
                                FillMSARecommendationWorkQueue(oSPWeb, currentUser);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(WorkQueue->PageLoad)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = " { " + ex.Message + " } " + " Please Contact the administrator.";
            }
        }
        private void FillMSAScheduleWorkQueue(SPWeb oSPWeb, SPUser currentUser)
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("AreaAudited", typeof(string));
                dt.Columns.Add("StartTime", typeof(string));
                dt.Columns.Add("EndTime", typeof(string));
                dt.Columns.Add("LinkFileName", typeof(string));

                SPSecurity.RunWithElevatedPrivileges(delegate()
                {

                    using (SPSite oSPSite = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb pSPWeb = oSPSite.OpenWeb())
                        {
                            SPList spList = pSPWeb.Lists["MSA Schedule"];
                            SPQuery spQuery = new SPQuery();

                            StringBuilder sb = new StringBuilder();
                            sb.Append("<Where>")
                                .Append("<And>")
                                 .Append("<Eq>")
                                 .Append("<FieldRef Name='FFLScheduleName' />")
                                 .Append("<Value Type='User'>" + currentUser.Name + "</Value>")
                                 .Append("</Eq>")
                                 .Append("</Where>");

                            string strQuery = "<Where><And><And><Eq><FieldRef Name='FFLScheduleName' LookupId='TRUE' /><Value Type='Integer' >" + currentUser.ID + "</Value></Eq><Geq><FieldRef Name='EventDate' /><Value Type='DateTime' IncludeTimeValue='FALSE'><Today /></Value></Geq></And><Eq><FieldRef Name='MSAStatus' /><Value Type='Choice'>Not Started</Value></Eq></And></Where>";
                            //"<Where><Eq><FieldRef Name='FFLScheduleName' LookupId='TRUE' /><Value Type='Integer' >" + currentUser.ID + "</Value></Eq></Where>";
                            spQuery.Query = strQuery; //sb.ToString(); // <UserID />



                            SPListItemCollection spListItems = spList.GetItems(spQuery);
                            if (spListItems != null && spListItems.Count > 0)
                            {
                                DataRow dr;
                                string recommendationLink = Utility.GetRedirectUrl("MSAFormLink");

                                SPFieldUrlValue link;

                                foreach (SPListItem item in spListItems)
                                {
                                    dr = dt.NewRow();
                                    string name = Convert.ToString(item["FFLScheduleName"]);

                                    if (Convert.ToString(item["FFLArea"]) != null)
                                    {
                                        string[] areas = Convert.ToString(item["FFLArea"]).Split('#');
                                        if (areas.Length > 1)
                                        {
                                            dr["AreaAudited"] = areas[1];
                                        }
                                    }

                                    //dr["AreaAudited"] = item["FFLArea"] != null ? Convert.ToString(item["FFLArea"]) : "";
                                    if (!String.IsNullOrEmpty(Convert.ToString(item["EventDate"])))
                                    {
                                        DateTime StartTime;
                                        bool bValid = DateTime.TryParse(Convert.ToString(item["EventDate"]), new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out StartTime);

                                        if (bValid)
                                        {
                                            dr["StartTime"] = StartTime.ToShortDateString();
                                        }
                                        else
                                        {
                                            dr["StartTime"] = Convert.ToDateTime(StartTime).ToShortDateString();
                                        }
                                    }
                                    else
                                    {
                                        dr["StartTime"] = "";
                                    }

                                    if (!String.IsNullOrEmpty(Convert.ToString(item["EndDate"])))
                                    {
                                        DateTime EndTime;
                                        bool bValid = DateTime.TryParse(Convert.ToString(item["EndDate"]), new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out EndTime);

                                        if (bValid)
                                        {
                                            dr["EndTime"] = EndTime.ToShortDateString();
                                        }
                                        else
                                        {
                                            dr["EndTime"] = Convert.ToDateTime(EndTime).ToShortDateString(); ;
                                        }
                                    }
                                    else
                                    {
                                        dr["EndTime"] = "";
                                    }

                                    link = new SPFieldUrlValue(Convert.ToString(item["MSAFormLink"]));
                                    dr["LinkFileName"] = link.Url;  //.Format("{0}?SID=" + item["ID"], recommendationLink);


                                    dt.Rows.Add(dr);
                                }

                                BoundField bf = new BoundField();

                                //RecommendationNo Column
                                bf = new BoundField();
                                bf.DataField = "AreaAudited";
                                bf.HeaderText = "Area To Be Audited";
                                grdMSAScheduled.Columns.Add(bf);

                                bf = new BoundField();
                                bf.DataField = "StartTime";
                                bf.HeaderText = "Start Date";
                                grdMSAScheduled.Columns.Add(bf);

                                bf = new BoundField();
                                bf.DataField = "EndTime";
                                bf.HeaderText = "End Date";
                                grdMSAScheduled.Columns.Add(bf);

                                //bf = new BoundField();
                                //bf.DataField = "MSADate";
                                //bf.HeaderText = "MSA Date";
                                //grdMSATask.Columns.Add(bf);

                                //bf = new BoundField();
                                //bf.DataField = "AuditedBy";
                                //bf.HeaderText = "Audited By";
                                //grdMSATask.Columns.Add(bf);



                                HyperLinkField hyperlinkField = new HyperLinkField();
                                hyperlinkField.HeaderText = "View MSA";
                                hyperlinkField.DataNavigateUrlFields = new[] { "LinkFileName" };
                                hyperlinkField.Text = "View";
                                grdMSAScheduled.Columns.Add(hyperlinkField);


                                grdMSAScheduled.DataSource = dt;
                                grdMSAScheduled.DataBind();






                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL->FillMSASchedule)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
            }
        }
        private void FillMSAWorkQueue(SPWeb oSPWeb_, SPUser currentUser)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                    {
                        using (SPWeb oSPWeb_EP = oSPsite.OpenWeb())
                        {
                            string getName = string.Empty;


                            DataTable dt = new DataTable();

                            dt.Columns.Add("AreaAudited", typeof(string));
                            dt.Columns.Add("StartTime", typeof(string));
                            dt.Columns.Add("MSADate", typeof(string));
                            dt.Columns.Add("EndTime", typeof(string));
                            dt.Columns.Add("AuditedBy", typeof(string));
                            dt.Columns.Add("LinkFileName", typeof(string));

                            string listName = "MSA";
                            // Fetch the List
                            SPList splistMSARecommendation = oSPWeb_EP.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb_EP.Url, listName));

                            SPQuery query = new SPQuery();
                            SPListItemCollection spListItems;
                            // Include only the fields you will use.


                            string vf = "<FieldRef Name='ID' /><FieldRef Name='Author' /><FieldRef Name='MSADate' /><FieldRef Name='AuditedBy' /><FieldRef Name='AreaAudited' /><FieldRef Name='AccompaniedBy' /><FieldRef Name='StartTime' /><FieldRef Name='EndTime' />";

                            query.ViewFields = vf;
                            query.ViewFieldsOnly = true;

                            StringBuilder sb = new StringBuilder();
                            sb.Append("<Where>")
                                .Append("<And>")
                                .Append("<Eq>")
                                 .Append("<FieldRef Name='IsSavedAsDraft' />")
                                 .Append("<Value Type='Boolean'>1</Value>")
                                 .Append("</Eq>")
                                 .Append("<Eq>")
                                 .Append("<FieldRef Name='Author' />")
                                 .Append("<Value Type='User'>" + currentUser.Name + "</Value>")
                                 .Append("</Eq>")
                                 .Append("</And>")
                                 .Append("</Where>");


                            query.Query = sb.ToString();
                            spListItems = splistMSARecommendation.GetItems(query);

                            DataRow dr;


                            if (spListItems != null && spListItems.Count > 0)
                            {
                                foreach (SPListItem item in spListItems)
                                {

                                    dr = dt.NewRow();

                                    SPUser author = null;
                                    
                                    if (item["Author"] != null)
                                    {
                                        string authorStr = Convert.ToString(item["Author"]);

                                        var temp = authorStr.Split('#');

                                        if (temp.Length > 1)
                                        {
                                            temp = temp[0].Split(';');

                                            if (temp.Length > 1)
                                            {
                                                author = Utility.GetUser(oSPWeb_EP, null, null, Int32.Parse(temp[0]));
                                            }
                                        }


                                    }

                                    dr["AreaAudited"] = item["AreaAudited"] != null ? Convert.ToString(item["AreaAudited"]) : "";

                                    dr["StartTime"] = item["StartTime"] != null ? Convert.ToString(item["StartTime"]) : "";

                                    dr["EndTime"] = item["EndTime"] != null ? Convert.ToString(item["EndTime"]) : "";

                                    string auditedBy = item["AuditedBy"] != null ? Convert.ToString(item["AuditedBy"]) : "";

                                    if (!String.IsNullOrEmpty(auditedBy))
                                    {
                                        SPUser auditedByUser = Utility.GetUser(oSPWeb_EP, auditedBy);

                                        if (auditedByUser != null)
                                        {
                                            dr["AuditedBy"] = auditedByUser.Name;
                                        }
                                    }


                                    DateTime date;
                                    bool bValid = DateTime.TryParse(Convert.ToString(item["MSADate"]), new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                                    if (bValid)
                                    {
                                        dr["MSADate"] = date.ToShortDateString();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            dr["MSADate"] = Convert.ToDateTime(Convert.ToString(item["MSADate"])).ToShortDateString();
                                        }
                                        catch (Exception ex)
                                        {
                                            SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("WQ-MSAD:" + Convert.ToString(item["MSADate"]), TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                                        }
                                    }


                                    string recommendationLink = Utility.GetRedirectUrl("MSAFormLink");


                                    dr["LinkFileName"] = string.Format("{0}?MSAID=" + item["ID"], recommendationLink);



                                    if (author != null && Utility.CompareUsername(author.LoginName, currentUser.LoginName))
                                    {
                                        dt.Rows.Add(dr);
                                    }

                                }
                            }

                            BoundField bf = new BoundField();


                            //RecommendationNo Column
                            bf = new BoundField();
                            bf.DataField = "AreaAudited";
                            bf.HeaderText = "Area Audited";
                            grdMSATask.Columns.Add(bf);

                            bf = new BoundField();
                            bf.DataField = "StartTime";
                            bf.HeaderText = "Start Time";
                            grdMSATask.Columns.Add(bf);

                            bf = new BoundField();
                            bf.DataField = "EndTime";
                            bf.HeaderText = "End Time";
                            grdMSATask.Columns.Add(bf);

                            bf = new BoundField();
                            bf.DataField = "MSADate";
                            bf.HeaderText = "MSA Date";
                            grdMSATask.Columns.Add(bf);

                            bf = new BoundField();
                            bf.DataField = "AuditedBy";
                            bf.HeaderText = "Audited By";
                            grdMSATask.Columns.Add(bf);



                            HyperLinkField hyperlinkField = new HyperLinkField();
                            hyperlinkField.HeaderText = "View MSA";
                            hyperlinkField.DataNavigateUrlFields = new[] { "LinkFileName" };
                            hyperlinkField.Text = "View";
                            grdMSATask.Columns.Add(hyperlinkField);


                            grdMSATask.DataSource = dt;
                            grdMSATask.DataBind();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(WorkQueue->FillMSAWorkQueue)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
            }
        }
        private void FillMSARecommendationWorkQueue(SPWeb oSPWeb, SPUser currentUser)
        {            
            try
            {
                if (oSPWeb != null)
                {
                    string getName = string.Empty;

                    DataTable dt = new DataTable();

                    dt.Columns.Add("ItemID", typeof(int));
                    dt.Columns.Add("RecommendationNo", typeof(string));
                    dt.Columns.Add("ResponsiblePerson", typeof(string));
                    dt.Columns.Add("TargetDate", typeof(string));
                    dt.Columns.Add("TaskName", typeof(string));
                    dt.Columns.Add("LinkFileName", typeof(string));
                    dt.Columns.Add("LinkDisplayText", typeof(string));
                    
                    string listName = "MSARecommendation";
                    // Fetch the List
                    SPList splistMSARecommendation = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                    SPQuery query = new SPQuery();
                    SPListItemCollection spListItems;
                    // Include only the fields you will use.
                    StringBuilder vf = new StringBuilder();
                    vf.Append("<FieldRef Name='ID' />")
                        .Append("<FieldRef Name='MSARecommendationDescription'/>")
                        .Append("<FieldRef Name='TargetDate'/>")
                        .Append("<FieldRef Name='ResponsiblePerson'/>")
                        .Append("<FieldRef Name='ApprovedBy'/>")
                        .Append("<FieldRef Name='ResponsibleDepartment'/>")
                        .Append("<FieldRef Name='AssigneeEmail'/>")
                        .Append("<FieldRef Name='RecommendationNo'/>");
                    

                    query.ViewFields = vf.ToString();
                    query.ViewFieldsOnly = true;
                    
                    query.Query = "<Where><And><And><Neq><FieldRef Name='Status' /><Value Type='Text'>Completed</Value></Neq><Eq><FieldRef Name='AssigneeEmail' /><Value Type='Text'>" + currentUser.Email + "</Value></Eq></And><Eq><FieldRef Name='IsSavedAsDraft' /><Value Type='Boolean'>0</Value></Eq></And></Where>";
                    //query.Query = "<Where><And><Eq><FieldRef Name='AssigneeEmail' /><Value Type='Text'>" + currentUser.Email + "</Value></Eq><Eq><FieldRef Name='IsSavedAsDraft' /><Value Type='Boolean'>0</Value></Eq></And></Where>";
                    spListItems = splistMSARecommendation.GetItems(query);
                    
                    
                    DataRow dr;

                    if (spListItems != null && spListItems.Count > 0)
                    {
                        
                        foreach (SPListItem item in spListItems)
                        {
                            dr = dt.NewRow();

                            dr["ItemID"] = item["ID"];
                            dr["RecommendationNo"] = item["RecommendationNo"] != null ? Convert.ToString(item["RecommendationNo"]) : "";
                            
                            string rpUsername = item["ResponsiblePerson"] != null ? Convert.ToString(item["ResponsiblePerson"]) : "";
                            
                            SPUser responsiblePerson = null;

                            if (!String.IsNullOrEmpty(rpUsername))
                            {
                                 responsiblePerson = Utility.GetUser(oSPWeb, rpUsername);                                 
                                if (responsiblePerson != null)
                                {
                                    dr["ResponsiblePerson"] = responsiblePerson.Name;                            
                                }
                            }


                            DateTime date;
                            bool bValid = DateTime.TryParse(Convert.ToString(item["TargetDate"]), new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);
                            
                            if (bValid)
                            {
                                dr["TargetDate"] = date.ToShortDateString();                                
                            }
                            else
                            {
                                try
                                {
                                    dr["TargetDate"] = Convert.ToDateTime(Convert.ToString(item["TargetDate"])).ToShortDateString();
                                }
                                catch (Exception ex)
                                {
                                    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("WQ-TRGD:" + Convert.ToString(item["TargetDate"]), TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                                }
                            }

                            dr["TaskName"] = "MSA Recommendation";

                            string recommendationLink = Utility.GetRedirectUrl("MSARecommendationFormLink");

                            dr["LinkFileName"] = string.Format("{0}?MSARID=" + item["ID"], recommendationLink);                            
                            if (String.IsNullOrEmpty(Convert.ToString(item["ApprovedBy"])))
                            {
                                if (getHOD(oSPWeb, Convert.ToString(item["ResponsibleDepartment"]), responsiblePerson.Email))
                                {
                                    dr["LinkDisplayText"] = "View (for approval)";                                    
                                }
                                else
                                {
                                    dr["LinkDisplayText"] = "View (for submission)";                                    
                                }                                
                            }
                            else
                            {
                                if (Convert.ToString(item["ApprovedBy"]).Equals(Convert.ToString(item["AssigneeEmail"]),StringComparison.OrdinalIgnoreCase))
                                {
                                    dr["LinkDisplayText"] = "View (for approval)";                                    
                                }
                                else
                                {
                                    dr["LinkDisplayText"] = "View (for submission)";                                    
                                }
                            }                           

                            dt.Rows.Add(dr);                            
                        }
                    }

                    BoundField bf = new BoundField();
                    bf.DataField = "ItemID";
                    bf.HeaderText = "ID #";
                    grdMSARecommendationTask.Columns.Add(bf);

                    //RecommendationNo Column
                    bf = new BoundField();
                    bf.DataField = "RecommendationNo";
                    bf.HeaderText = "Recommendation No";
                    grdMSARecommendationTask.Columns.Add(bf);

                    bf = new BoundField();
                    bf.DataField = "ResponsiblePerson";
                    bf.HeaderText = "Responsible Person";
                    grdMSARecommendationTask.Columns.Add(bf);

                    bf = new BoundField();
                    bf.DataField = "TargetDate";
                    bf.HeaderText = "Target Date";
                    grdMSARecommendationTask.Columns.Add(bf);

                    HyperLinkField hyperlinkField = new HyperLinkField();
                    hyperlinkField.HeaderText = "View Recommendations";
                    hyperlinkField.DataNavigateUrlFields = new[] { "LinkFileName" };
                    //hyperlinkField.Text = "View";
                    hyperlinkField.DataTextField = "LinkDisplayText";
                    grdMSARecommendationTask.Columns.Add(hyperlinkField);

                    grdMSARecommendationTask.DataSource = dt;
                    grdMSARecommendationTask.DataBind();         
                }

            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(WorkQueue->FillMSARecommendationWorkQueue)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
            }
        }

        private bool getHOD(SPWeb oSPWeb, string departmentID, string responsiblepersonEmailAddress)
        {            
            try
            {
                if (oSPWeb != null)
                {
                    SPList spList = oSPWeb.Lists["Department"];
                    SPListItem listItem = spList.GetItemById(Convert.ToInt32(departmentID));
                    if (listItem != null)
                    {                        
                        if (Convert.ToString(listItem["HODEmail"]).Equals(responsiblepersonEmailAddress, StringComparison.OrdinalIgnoreCase))
                        {            
                            return true;
                        }
                    }
                }
            }catch(Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(WorkQueue-GetHOD)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
            }
            
            return false;
        }
    }
}
