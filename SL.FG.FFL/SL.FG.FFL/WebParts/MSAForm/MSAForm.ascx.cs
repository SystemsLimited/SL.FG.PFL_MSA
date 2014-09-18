using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using System;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Web;
using SL.FG.FFL.Layouts.SL.FG.FFL.Model;
using System.Text.RegularExpressions;
using System.Text;
using SL.FG.FFL.Layouts.SL.FG.FFL.Common;
using System.Globalization;
using System.IO;

namespace SL.FG.FFL.WebParts.MSAForm
{
    [ToolboxItemAttribute(false)]

    public class PairId
    {
        public List<int> ContactIds { get; set; }
        public List<int> RecommendationIds { get; set; }
    }

    public partial class MSAForm : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public MSAForm()
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
                    FillDropdowns();//Fill Dropdowns values

                    if (!String.IsNullOrEmpty(Page.Request.QueryString["SID"]))
                    {
                        this.hdnScheduleId.Value = Page.Request.QueryString["SID"];

                        SPUser auditedBy = null;

                        SPUser currentUser = null;

                        using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                        {
                            using (SPWeb oSPWeb = oSPsite.OpenWeb())
                            {
                                currentUser = oSPWeb.CurrentUser;

                                if (!IsMSASubmitted(oSPWeb, Convert.ToInt32(this.hdnScheduleId.Value)))
                                {
                                    auditedBy = UpdateMSAScheduleControls(oSPWeb);
                                }
                                else
                                {
                                    this.message_div.InnerHtml = "Update operation is not allowed on saved records!!! Please Contact the administrator.";
                                }
                            }
                        }

                        if (currentUser != null && auditedBy != null)
                        {
                            if (!Utility.CompareUsers(currentUser, auditedBy))
                            {
                                DisableControls();
                                if (!CheckPermission())
                                {
                                    string accessDeniedUrl = Utility.GetRedirectUrl("Access_Denied");

                                    if (!String.IsNullOrEmpty(accessDeniedUrl))
                                    {
                                        DisableControls();
                                        Page.Response.Redirect(accessDeniedUrl, false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            DisableControls();
                        }
                    }

                    if (CheckPermission())
                    {
                        this.msaQualityScore_div.Visible = true;
                    }

                    if (!String.IsNullOrEmpty(Page.Request.QueryString["Status"]))
                    {
                        if (Page.Request.QueryString["Status"].Equals("1"))
                        {
                            this.message_div.InnerHtml = "Save Operation is Successfull!!!";
                        }
                    }

                    if (!String.IsNullOrEmpty(Page.Request.QueryString["MSAID"]))
                    {
                        this.hdnMSAId.Value = Page.Request.QueryString["MSAID"];
                        int msaID;

                        Int32.TryParse(this.hdnMSAId.Value, out msaID);
                        bool isSuccess = InitializeMSAControls(msaID);
                        if (isSuccess == false)
                        {
                            DisableControls();
                        }
                        UpdateControls(true);
                    }
                    else
                    {
                        if (!CheckPermission_Authenticated_Users())
                        {
                            DisableControls();

                            string accessDeniedUrl = Utility.GetRedirectUrl("Access_Denied");

                            if (!String.IsNullOrEmpty(accessDeniedUrl))
                            {
                                DisableControls();
                                Page.Response.Redirect(accessDeniedUrl, false);
                            }
                        }

                        UpdateControls(false);//Set default values and restrict controls on the basis of situation
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->Page_Load)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
        
        }

        private bool IsMSASubmitted(SPWeb oSPWeb, int scheduleId)
        {
            try
            {
                string listName = "MSA";

                // Fetch the List
                SPList spList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                SPQuery query = new SPQuery();
                SPListItemCollection spListItems;
                // Include only the fields you will use.
                query.ViewFields = "<FieldRef Name='ID'/>";
                query.ViewFieldsOnly = true;
                query.RowLimit = 1;
                StringBuilder sb = new StringBuilder();
                sb.Append("<Where><Eq><FieldRef Name='ScheduleId' /><Value Type='Number'>" + scheduleId + "</Value></Eq></Where>");
                query.Query = sb.ToString();
                spListItems = spList.GetItems(query);

                if (spListItems.Count > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->IsMSASubmitted)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
            return false;
        }
        private SPUser UpdateMSAScheduleControls(SPWeb oSPWeb)
        {
            SPUser auditedBy = null;

            try
            {
                string spListNameMS = "MSASchedule";

                // Fetch the List
                SPList spListMS = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, spListNameMS));

                if (spListMS != null && !String.IsNullOrEmpty(this.hdnScheduleId.Value))
                {
                    int sid = Convert.ToInt32(this.hdnScheduleId.Value);

                    SPListItem spListItemMS = spListMS.GetItemById(sid);

                    if (spListItemMS["Title"] != null)
                    {
                        string designation = Convert.ToString(spListItemMS["Title"]);

                        if (!String.IsNullOrEmpty(designation))
                        {
                            this.designation_tf.Value = designation;
                            this.designation_tf.Disabled = true;
                        }
                    }

                    if (spListItemMS["Area"] != null)
                    {
                        string areaAudited = Convert.ToString(spListItemMS["Area"]);

                        if (!String.IsNullOrEmpty(areaAudited))
                        {
                            if (areaAudited.Contains("#"))
                            {
                                var temp = areaAudited.Split('#');

                                if (temp != null && temp.Length > 1)
                                {
                                    this.areaAudited_ddl.SelectedValue = temp[1];

                                    if (this.areaAudited_ddl.SelectedIndex > 0)
                                    {
                                        this.areaAudited_ddl.Enabled = false;
                                    }
                                }
                            }
                        }
                    }

                    if (spListItemMS["FFLScheduleName"] != null)
                    {
                        string auditedByUsername = Convert.ToString(spListItemMS["FFLScheduleName"]);

                        if (!String.IsNullOrEmpty(auditedByUsername))
                        {
                            var temp = auditedByUsername.Split('#');

                            if (temp.Length > 1)
                            {
                                temp = temp[0].Split(';');

                                if (temp.Length > 1)
                                {
                                    auditedBy = Utility.GetUser(oSPWeb, null, null, Int32.Parse(temp[0]));
                                }
                            }
                        }

                        if (auditedBy != null)
                        {
                            // Clear existing users from control
                            this.auditedBy_PeopleEditor.Entities.Clear();

                            // PickerEntity object is used by People Picker Control
                            PickerEntity UserEntity = new PickerEntity();

                            // CurrentUser is SPUser object
                            UserEntity.DisplayText = auditedBy.Name;
                            UserEntity.Key = auditedBy.LoginName;

                            // Add PickerEntity to People Picker control
                            this.auditedBy_PeopleEditor.Entities.Add(this.auditedBy_PeopleEditor.ValidateEntity(UserEntity));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->UpdateMSAScheduleControls)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
            return auditedBy;
        }
        private void DisableControls()
        {
            this.btnSave.Visible = false;
            this.btnSaveAsDraft.Visible = false;
            this.fileUploadControl.Enabled = false;
        }
        private void UpdateControls(bool IsEditCase)
        {
            try
            {
                using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                {
                    using (SPWeb oSPWeb = oSPsite.OpenWeb())
                    {
                        this.targetDate_dtc.SelectedDate = DateTime.Now.Date;
                        this.msaDate_dtc.SelectedDate = DateTime.Now.Date;
                        this.msaDate_dtc.Enabled = false;

                        //Ristrict users
                        //string groupName = Utility.GetValueByKey("ADGroup");

                        //if (!String.IsNullOrEmpty(groupName))
                        //{
                        //    //this.responsiblePerson_PeopleEditor.PrincipalSource = null;
                        //    var spGroup = oSPWeb.Groups[groupName];
                        //    if (spGroup != null)
                        //    {
                        //        this.responsiblePerson_PeopleEditor.SharePointGroupID = spGroup.ID;
                        //    }
                        //}
                        //End

                        if (!IsEditCase)
                        {
                            SPUser CurrentUser = oSPWeb.CurrentUser;
                            if (CurrentUser != null && this.auditedBy_PeopleEditor.Entities.Count == 0)
                            {
                                // Clear existing users from control
                                this.auditedBy_PeopleEditor.Entities.Clear();

                                // PickerEntity object is used by People Picker Control
                                PickerEntity UserEntity = new PickerEntity();

                                // CurrentUser is SPUser object
                                UserEntity.DisplayText = CurrentUser.Name;
                                UserEntity.Key = CurrentUser.LoginName;

                                // Add PickerEntity to People Picker control
                                this.auditedBy_PeopleEditor.Entities.Add(this.auditedBy_PeopleEditor.ValidateEntity(UserEntity));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->UpdateControls)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
        }
        private void FillDropdowns()
        {
            try
            {
                using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                {
                    using (SPWeb oSPWeb = oSPsite.OpenWeb())
                    {
                        string listName = "Area";

                        // Fetch the List
                        SPList spList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                        SPQuery query = new SPQuery();
                        SPListItemCollection spListItems;
                        // Include only the fields you will use.
                        query.ViewFields = "<FieldRef Name='ID'/><FieldRef Name='Title'/>";
                        query.ViewFieldsOnly = true;
                        //query.RowLimit = 200; // Only select the top 200.
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<OrderBy Override='TRUE;><FieldRef Name='Title'/></OrderBy>");
                        query.Query = sb.ToString();
                        spListItems = spList.GetItems(query);


                        this.areaAudited_ddl.DataSource = spListItems;
                        this.areaAudited_ddl.DataTextField = "Title";
                        this.areaAudited_ddl.DataValueField = "Title"; //As we dont save Area Id, therefore no need to use here
                        this.areaAudited_ddl.DataBind();

                        this.areaAudited_ddl.Items.Insert(0, new ListItem("Please Select", "0"));
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->FillDropdowns)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
        }
        private bool CheckPermission()
        {
            bool isMember = false;
            using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
            {
                using (SPWeb oSPWeb = oSPsite.OpenWeb())
                {
                    string groupName = Utility.GetValueByKey("MasterGroup");
                    var spGroup = oSPWeb.Groups[groupName];
                    if (spGroup != null)
                    {
                        isMember = oSPWeb.IsCurrentUserMemberOfGroup(spGroup.ID);
                    }
                }
            }
            return isMember;
        }
        private bool CheckPermission_Authenticated_Users()
        {
            bool isMember = false;
            using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
            {
                using (SPWeb oSPWeb = oSPsite.OpenWeb())
                {
                    string groupName = Utility.GetValueByKey("Authenticated_Users");
                    var spGroup = oSPWeb.Groups[groupName];
                    if (spGroup != null)
                    {
                        isMember = oSPWeb.IsCurrentUserMemberOfGroup(spGroup.ID);
                    }
                }
            }
            return isMember;
        }

        private double CalculateMSAQualityScore(double auditMinutes, int noOfSafetyContacts, string accompaniedBy, int noOfPositivePoint, int noOfAreaImprovement, double noOfFatalityPoints, double noOfUnsafeConditions, double noOfSeriusInjury, int noOfClosureObservations, int noOfConsentTaken)
        {
            try
            {
                double auditMinutes_marks = 0;
                double safetyContact_marks = 0;
                double observation_marks = 0;
                double closureOfObservation_marks = 0;
                double communicationOfAreas_marks = 0;

                if (auditMinutes >= 30)
                {
                    auditMinutes_marks = 15;
                }

                if (noOfSafetyContacts >= 3)
                {
                    safetyContact_marks = 20;
                }
                else if (noOfSafetyContacts == 2)
                {
                    safetyContact_marks = 15;
                }
                else if (noOfSafetyContacts == 1)
                {
                    safetyContact_marks = 10;
                }

                if (!String.IsNullOrEmpty(accompaniedBy))
                {
                    safetyContact_marks += 5;
                }

                int noOfPositivePoint_temp = 0;

                if (noOfPositivePoint > 0)
                {
                    noOfPositivePoint_temp = 5;
                }


                int noOfAreaImprovement_temp = 0;

                if (noOfAreaImprovement > 0)
                {
                    noOfAreaImprovement_temp = 5;
                }

                double noOfFatalityPoints_temp = 0;

                if (noOfFatalityPoints * 20 >= 40)
                {
                    noOfFatalityPoints_temp = 40;
                }
                else
                {
                    noOfFatalityPoints_temp = noOfFatalityPoints * 20;
                }

                double noOfSeriusInjury_temp = 0;

                if (noOfSeriusInjury * 10 >= 40)
                {
                    noOfSeriusInjury_temp = 40;
                }
                else
                {
                    noOfSeriusInjury_temp = noOfSeriusInjury * 10;
                }


                if (noOfFatalityPoints_temp + noOfSeriusInjury_temp < 40)
                {
                    observation_marks = noOfFatalityPoints_temp + noOfSeriusInjury_temp;
                }
                else
                {
                    observation_marks = 40;
                }


                if (observation_marks + noOfPositivePoint_temp + noOfAreaImprovement_temp < 40)
                {
                    observation_marks += noOfPositivePoint_temp + noOfAreaImprovement_temp;
                }
                else
                {
                    observation_marks = 40;
                }


                if (noOfClosureObservations >= 2)
                {
                    closureOfObservation_marks = 10;
                }
                else if (noOfClosureObservations == 1)
                {
                    closureOfObservation_marks = 5;
                }

                if (noOfConsentTaken >= 3)
                {
                    communicationOfAreas_marks = 10;
                }


                //auditMinutes_marks = auditMinutes_marks * 0.15;
                //safetyContact_marks = safetyContact_marks * 0.25;
                //observation_marks = observation_marks * 0.40;
                //closureOfObservation_marks = closureOfObservation_marks * 0.10;
                //communicationOfAreas_marks = communicationOfAreas_marks * 0.10;

                double msaQualityScore = auditMinutes_marks + safetyContact_marks + observation_marks + closureOfObservation_marks + communicationOfAreas_marks;

                if (msaQualityScore > 100)
                {
                    msaQualityScore = 100;
                }

                return msaQualityScore;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->CalculateMSAQualityScore)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return 0;
        }
        private bool InitializeMSAControls(int msaId)
        {
            try
            {
                using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                {
                    using (SPWeb oSPWeb = oSPsite.OpenWeb())
                    {
                        string listName = "MSA";
                        // Fetch the List
                        SPList splistMSA = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                        if (splistMSA != null)
                        {
                            SPListItem spListItemMSA = splistMSA.GetItemById(msaId);

                            if (spListItemMSA != null)
                            {
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["AuditedBy"])))
                                {
                                    bool isAllowed = true;

                                    string username = Convert.ToString(spListItemMSA["AuditedBy"]);

                                    SPUser auditedBy = Utility.GetUser(oSPWeb, username);

                                    if (auditedBy != null)
                                    {
                                        // Clear existing users from control
                                        this.auditedBy_PeopleEditor.Entities.Clear();

                                        // PickerEntity object is used by People Picker Control
                                        PickerEntity UserEntity = new PickerEntity();

                                        // CurrentUser is SPUser object
                                        UserEntity.DisplayText = auditedBy.Name;
                                        UserEntity.Key = auditedBy.LoginName;

                                        // Add PickerEntity to People Picker control
                                        this.auditedBy_PeopleEditor.Entities.Add(this.auditedBy_PeopleEditor.ValidateEntity(UserEntity));
                                    }

                                    SPUser CurrentUser = oSPWeb.CurrentUser;
                                    if (CurrentUser != null)
                                    {
                                        if (!Utility.CompareUsername(username, CurrentUser.LoginName))
                                        {
                                            isAllowed = false;
                                        }
                                    }

                                    if (isAllowed == false)
                                    {
                                        if (CheckPermission())
                                        {
                                            DisableControls();
                                        }
                                        else
                                        {
                                            string accessDeniedUrl = Utility.GetRedirectUrl("Access_Denied");

                                            if (!String.IsNullOrEmpty(accessDeniedUrl))
                                            {
                                                DisableControls();
                                                Page.Response.Redirect(accessDeniedUrl, false);
                                            }
                                            return false;
                                        }
                                    }
                                }

                                bool isSavedAsDraft = true;

                                if (spListItemMSA["IsSavedAsDraft"] != null)
                                {
                                    isSavedAsDraft = Convert.ToBoolean(spListItemMSA["IsSavedAsDraft"]);

                                    if (isSavedAsDraft == false)
                                    {
                                        this.message_div.InnerHtml = "Update operation is not allowed on saved records!!! Please Contact the administrator.";
                                        DisableControls();
                                    }
                                }

                                if (isSavedAsDraft == false && spListItemMSA["MSADate"] != null && !String.IsNullOrEmpty(Convert.ToString(spListItemMSA["MSADate"])))
                                {
                                    DateTime date;
                                    bool bValid = DateTime.TryParse(Convert.ToString(spListItemMSA["MSADate"]), new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                                    if (bValid)
                                    {
                                        this.msaDate_dtc.SelectedDate = date.Date;
                                    }
                                    else
                                    {
                                        this.msaDate_dtc.SelectedDate = Convert.ToDateTime(spListItemMSA["MSADate"]).Date;
                                    }
                                }

                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["AccompaniedBy"])))
                                {
                                    this.accompaniedBy_tf.Value = Convert.ToString(spListItemMSA["AccompaniedBy"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["Designation"])))
                                {
                                    this.designation_tf.Value = Convert.ToString(spListItemMSA["Designation"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["AreaAudited"])))
                                {
                                    this.areaAudited_ddl.SelectedValue = Convert.ToString(spListItemMSA["AreaAudited"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["StartTime"])))
                                {
                                    DateTime startTime = Convert.ToDateTime(spListItemMSA["StartTime"]);
                                    this.startTime_dtc.SelectedDate = startTime;
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["EndTime"])))
                                {
                                    DateTime endTime = Convert.ToDateTime(spListItemMSA["EndTime"]);
                                    this.endTime_dtc.SelectedDate = endTime;
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["NoOfUnsafeActs"])))
                                {
                                    this.noOfUnsafeActs_tf.InnerHtml = Convert.ToString(spListItemMSA["NoOfUnsafeActs"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["NoOfUnsafeConditions"])))
                                {
                                    this.noOfUnsafeConditions_tf.InnerHtml = Convert.ToString(spListItemMSA["NoOfUnsafeConditions"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["NoOfSeriousInjury"])))
                                {
                                    this.noOfSeriousInjury_tf.InnerHtml = Convert.ToString(spListItemMSA["NoOfSeriousInjury"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["NoOfFatalityInjury"])))
                                {
                                    this.noOfFatalityInjury_tf.InnerHtml = Convert.ToString(spListItemMSA["NoOfFatalityInjury"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["PositivePoints"])))
                                {
                                    this.hdnPositivePointList.Value = Convert.ToString(spListItemMSA["PositivePoints"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["AreaOfImprovement"])))
                                {
                                    this.hdnAreaOfImprovementList.Value = Convert.ToString(spListItemMSA["AreaOfImprovement"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["MSAQualityScore"])))
                                {
                                    this.msaQualityScore_tf.InnerText = Convert.ToString(spListItemMSA["MSAQualityScore"]);
                                }
                                if (!String.IsNullOrEmpty(Convert.ToString(spListItemMSA["ScheduleId"])))
                                {
                                    this.hdnScheduleId.Value = Convert.ToString(spListItemMSA["ScheduleId"]);
                                }

                                foreach (String attachmentname in spListItemMSA.Attachments)
                                {
                                    String attachmentAbsoluteURL =
                                    spListItemMSA.Attachments.UrlPrefix // gets the containing directory URL
                                    + attachmentname;
                                    // To get the SPSile reference to the attachment just use this code
                                    SPFile attachmentFile = oSPWeb.GetFile(attachmentAbsoluteURL);

                                    StringBuilder sb = new StringBuilder();

                                    HtmlTableRow tRow = new HtmlTableRow();

                                    HtmlTableCell removeLink = new HtmlTableCell();
                                    HtmlTableCell fileLink = new HtmlTableCell();

                                    sb.Append(String.Format("<a href='{0}/{1}' target='_blank'>{2}</a>", oSPWeb.Url, attachmentFile.Url, attachmentname));
                                    removeLink.InnerHtml = "<span class='btn-danger removeLink' style='padding:3px; margin-right:3px; border-radius:2px;'><i class='glyphicon glyphicon-remove'></i></span><span class='fileName' style='display:none;'>" + attachmentFile.Name + "</span>";

                                    fileLink.InnerHtml = sb.ToString();

                                    tRow.Cells.Add(removeLink);
                                    tRow.Cells.Add(fileLink);

                                    this.grdAttachments.Rows.Add(tRow);
                                }


                                string p1 = "~|~"; //separate records
                                string p2 = "*|*"; //separate content with in a record

                                //Positive Point List
                                List<string> lstPositivePoint = Utility.GetFormattedDataList(this.hdnPositivePointList.Value, p1, true);

                                //Area Of Improvement List
                                List<string> lstAreaOfImprovement = Utility.GetFormattedDataList(this.hdnAreaOfImprovementList.Value, p1, true);


                                if (lstPositivePoint != null)
                                {
                                    FillPositivePointGrid(lstPositivePoint);
                                }

                                if (lstAreaOfImprovement != null)
                                {
                                    FillAreaOfImprovementGrid(lstAreaOfImprovement);
                                }

                                //Contacts
                                List<MSAContact> lstMSAContact = GetFormattedContactsByMSA(oSPWeb, msaId);

                                //Recommendations
                                List<MSARecommendation> lstMSARecommendation = GetFormattedRecommendationsByMSA(oSPWeb, msaId);

                                StringBuilder ids = new StringBuilder();

                                if (lstMSAContact != null && lstMSARecommendation != null)
                                {
                                    //Add contacts in grid
                                    foreach (var contact in lstMSAContact)
                                    {
                                        HtmlTableRow tRow = new HtmlTableRow();

                                        tRow.Attributes.Add("class", "contactItem");

                                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = Convert.ToString(this.contactDetails_table.Rows.Count) });

                                        HtmlTableCell contactId = new HtmlTableCell();
                                        HtmlTableCell contactDetail = new HtmlTableCell();

                                        string actions = "<span class='btn btn-default editContact'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeContact'><i class='glyphicon glyphicon-remove'></i></span>";

                                        contactId.InnerHtml = "<span class='contactId'>" + Convert.ToString(contact.ContactId) + "</span>";
                                        contactId.Attributes.Add("style", "display:none");

                                        contactDetail.InnerHtml = "<span class='contactDetail'>" + Convert.ToString(contact.ContactDetail) + "</span>";

                                        tRow.Cells.Add(contactId);
                                        tRow.Cells.Add(contactDetail);

                                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = actions });

                                        this.contactDetails_table.Rows.Add(tRow);

                                        ids.Append(Convert.ToString(contact.ContactId));
                                        ids.Append(p2);
                                    }

                                    ids.Append(p1);


                                    //Add recommendations in grid
                                    foreach (var recommendation in lstMSARecommendation)
                                    {
                                        HtmlTableRow tRow = new HtmlTableRow();

                                        tRow.Attributes.Add("class", "recommendationItem");

                                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = Convert.ToString(this.recommendationDetails_table.Rows.Count) });

                                        HtmlTableCell recommendationId = new HtmlTableCell();
                                        HtmlTableCell recommendationNo = new HtmlTableCell();
                                        HtmlTableCell description = new HtmlTableCell();
                                        HtmlTableCell typeOfVoilation = new HtmlTableCell();
                                        HtmlTableCell responsiblePersonUsername = new HtmlTableCell();
                                        HtmlTableCell responsiblePersonEmail = new HtmlTableCell();
                                        HtmlTableCell responsibleSection = new HtmlTableCell();
                                        HtmlTableCell responsibleSectionId = new HtmlTableCell();
                                        HtmlTableCell responsibleDepartment = new HtmlTableCell();
                                        HtmlTableCell injuryClassification = new HtmlTableCell();
                                        HtmlTableCell responsibleDepartmentId = new HtmlTableCell();
                                        HtmlTableCell consentTaken = new HtmlTableCell();
                                        HtmlTableCell targetDate = new HtmlTableCell();
                                        HtmlTableCell observationCategory = new HtmlTableCell();
                                        HtmlTableCell observationSubCategory = new HtmlTableCell();
                                        HtmlTableCell observationSpot = new HtmlTableCell();
                                        HtmlTableCell status = new HtmlTableCell();

                                        string actions = "<span class='btn btn-default editRecommendation' ><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeRecommendation'><i class='glyphicon glyphicon-remove'></i></span>";

                                        recommendationId.InnerHtml = "<span class='recommendationId'>" + Convert.ToString(recommendation.RecommendationId) + "</span>";
                                        recommendationId.Attributes.Add("style", "display:none");

                                        recommendationNo.InnerHtml = "<span class='recommendationNo'>" + Convert.ToString(recommendation.RecommendationNo) + "</span>";
                                        recommendationNo.Attributes.Add("style", "display:none");

                                        description.Attributes.Add("class", "td-description");
                                        description.InnerHtml = "<span class='description'>" + Convert.ToString(recommendation.Description) + "</span>";
                                        typeOfVoilation.InnerHtml = "<span class='typeOfVoilation'>" + Convert.ToString(recommendation.TypeOfVoilation) + "</span>";
                                        responsiblePersonUsername.InnerHtml = "<span class='username'>" + Convert.ToString(recommendation.RPUsername) + "</span>";

                                        responsiblePersonEmail.InnerHtml = "<span class='email'>" + Convert.ToString(recommendation.RPEmail) + "</span>";
                                        responsiblePersonEmail.Attributes.Add("style", "display:none");

                                        responsibleSection.InnerHtml = "<span class='sectionName'>" + Convert.ToString(recommendation.SectionName) + "</span>";

                                        responsibleSectionId.InnerHtml = "<span class='sectionId'>" + Convert.ToString(recommendation.SectionId) + "</span>";
                                        responsibleSectionId.Attributes.Add("style", "display:none");

                                        responsibleDepartment.InnerHtml = "<span class='departmentName'>" + Convert.ToString(recommendation.DepartmentName) + "</span>";
                                        injuryClassification.InnerHtml = "<span class='injuryClass'>" + Convert.ToString(recommendation.InjuryClass) + "</span>";

                                        responsibleDepartmentId.InnerHtml = "<span class='departmentId'>" + Convert.ToString(recommendation.DepartmentId) + "</span>";
                                        responsibleDepartmentId.Attributes.Add("style", "display:none");

                                        consentTaken.InnerHtml = "<span class='consentTaken'>" + ((recommendation.ConsentTaken == true) ? "Yes" : "No") + "</span>";
                                        targetDate.InnerHtml = "<span class='targetDate'>" + Convert.ToString(recommendation.TargetDate) + "</span>";
                                        observationCategory.InnerHtml = "<span class='category'>" + Convert.ToString(recommendation.ObservationCategory) + "</span>";

                                        observationSubCategory.InnerHtml = "<span class='subCategory'>" + Convert.ToString(recommendation.ObservationSubcategory) + "</span>";
                                        //observationSubCategory.Attributes.Add("style", "display:none");

                                        observationSpot.InnerHtml = "<span class='observationSpot'>" + ((recommendation.ObservationSpot == true) ? "Yes" : "No") + "</span>";
                                        status.InnerHtml = "<span class='status'>" + Convert.ToString(recommendation.Status) + "</span>";

                                        tRow.Cells.Add(recommendationId);
                                        tRow.Cells.Add(description);
                                        tRow.Cells.Add(typeOfVoilation);
                                        tRow.Cells.Add(responsiblePersonUsername);
                                        tRow.Cells.Add(responsibleSection);
                                        tRow.Cells.Add(responsibleSectionId);
                                        tRow.Cells.Add(responsibleDepartment);
                                        tRow.Cells.Add(injuryClassification);
                                        tRow.Cells.Add(responsibleDepartmentId);
                                        tRow.Cells.Add(consentTaken);
                                        tRow.Cells.Add(targetDate);
                                        tRow.Cells.Add(observationCategory);
                                        tRow.Cells.Add(observationSubCategory);
                                        tRow.Cells.Add(observationSpot);
                                        tRow.Cells.Add(status);

                                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = actions });

                                        this.recommendationDetails_table.Rows.Add(tRow);

                                        ids.Append(Convert.ToString(recommendation.RecommendationId));
                                        ids.Append(p2);
                                    }

                                    this.hdnIdList.Value = ids.ToString();
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->InitializeMSAControls)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

            return false;
        }
        private bool FillPositivePointGrid(List<string> lstPositivePoint)
        {
            try
            {
                if (lstPositivePoint != null)
                {
                    //Add Positive Point in grid
                    foreach (var item in lstPositivePoint)
                    {
                        HtmlTableRow tRow = new HtmlTableRow();

                        tRow.Attributes.Add("class", "positivePointItem");

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = Convert.ToString(this.positivePoint_table.Rows.Count) });

                        HtmlTableCell description = new HtmlTableCell();

                        string actions = "<span class='btn btn-default editPositivePoint'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removePositivePoint'><i class='glyphicon glyphicon-remove'></i></span>";

                        description.InnerHtml = "<span class='positivePointDescription'>" + Convert.ToString(item) + "</span>";

                        tRow.Cells.Add(description);

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = actions });

                        this.positivePoint_table.Rows.Add(tRow);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->FillAreaOfImprovementGrid)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return false;
        }
        private bool FillAreaOfImprovementGrid(List<string> lstAreaOfImprovement)
        {
            try
            {
                if (lstAreaOfImprovement != null)
                {
                    //Add Area Of Improvement in grid
                    foreach (var item in lstAreaOfImprovement)
                    {
                        HtmlTableRow tRow = new HtmlTableRow();

                        tRow.Attributes.Add("class", "areaOfImprovementItem");

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = Convert.ToString(this.areaOfImprovement_table.Rows.Count) });

                        HtmlTableCell description = new HtmlTableCell();

                        string actions = "<span class='btn btn-default editAreaOfImprovement'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeAreaOfImprovement'><i class='glyphicon glyphicon-remove'></i></span>";

                        description.InnerHtml = "<span class='areaOfImprovementDescription'>" + Convert.ToString(item) + "</span>";

                        tRow.Cells.Add(description);

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = actions });

                        this.areaOfImprovement_table.Rows.Add(tRow);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->FillAreaOfImprovementGrid)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return false;
        }
        private bool FillContactGrid(List<MSAContact> lstMSAContact)
        {
            try
            {
                if (lstMSAContact != null)
                {
                    //Add contacts in grid
                    foreach (var contact in lstMSAContact)
                    {
                        HtmlTableRow tRow = new HtmlTableRow();

                        tRow.Attributes.Add("class", "contactItem");

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = Convert.ToString(this.contactDetails_table.Rows.Count) });

                        HtmlTableCell contactId = new HtmlTableCell();
                        HtmlTableCell contactDetail = new HtmlTableCell();

                        string actions = "<span class='btn btn-default editContact'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeContact'><i class='glyphicon glyphicon-remove'></i></span>";

                        contactId.InnerHtml = "<span class='contactId'>" + Convert.ToString(contact.ContactId) + "</span>";
                        contactId.Attributes.Add("style", "display:none");

                        contactDetail.InnerHtml = "<span class='contactDetail'>" + Convert.ToString(contact.ContactDetail) + "</span>";

                        tRow.Cells.Add(contactId);
                        tRow.Cells.Add(contactDetail);

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = actions });

                        this.contactDetails_table.Rows.Add(tRow);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->FillContactGrid)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return false;
        }
        private bool FillRecommendationGrid(List<MSARecommendation> lstMSARecommendation)
        {
            try
            {
                if (lstMSARecommendation != null)
                {
                    //Add recommendations in grid
                    foreach (var recommendation in lstMSARecommendation)
                    {
                        HtmlTableRow tRow = new HtmlTableRow();

                        tRow.Attributes.Add("class", "recommendationItem");

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = Convert.ToString(this.recommendationDetails_table.Rows.Count) });

                        HtmlTableCell recommendationId = new HtmlTableCell();
                        HtmlTableCell recommendationNo = new HtmlTableCell();
                        HtmlTableCell description = new HtmlTableCell();
                        HtmlTableCell typeOfVoilation = new HtmlTableCell();
                        HtmlTableCell responsiblePersonUsername = new HtmlTableCell();
                        HtmlTableCell responsiblePersonEmail = new HtmlTableCell();
                        HtmlTableCell responsibleSection = new HtmlTableCell();
                        HtmlTableCell responsibleSectionId = new HtmlTableCell();
                        HtmlTableCell responsibleDepartment = new HtmlTableCell();
                        HtmlTableCell injuryClassification = new HtmlTableCell();
                        HtmlTableCell responsibleDepartmentId = new HtmlTableCell();
                        HtmlTableCell consentTaken = new HtmlTableCell();
                        HtmlTableCell targetDate = new HtmlTableCell();
                        HtmlTableCell observationCategory = new HtmlTableCell();
                        HtmlTableCell observationSubCategory = new HtmlTableCell();
                        HtmlTableCell observationSpot = new HtmlTableCell();
                        HtmlTableCell status = new HtmlTableCell();

                        string actions = "<span class='btn btn-default editRecommendation' ><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeRecommendation'><i class='glyphicon glyphicon-remove'></i></span>";

                        recommendationId.InnerHtml = "<span class='recommendationId'>" + Convert.ToString(recommendation.RecommendationId) + "</span>";
                        recommendationId.Attributes.Add("style", "display:none");

                        recommendationNo.InnerHtml = "<span class='recommendationNo'>" + Convert.ToString(recommendation.RecommendationNo) + "</span>";
                        recommendationNo.Attributes.Add("style", "display:none");

                        description.Attributes.Add("class", "td-description");
                        description.InnerHtml = "<span class='description'>" + Convert.ToString(recommendation.Description) + "</span>";
                        typeOfVoilation.InnerHtml = "<span class='typeOfVoilation'>" + Convert.ToString(recommendation.TypeOfVoilation) + "</span>";
                        responsiblePersonUsername.InnerHtml = "<span class='username'>" + Convert.ToString(recommendation.RPUsername) + "</span>";

                        responsiblePersonEmail.InnerHtml = "<span class='email'>" + Convert.ToString(recommendation.RPEmail) + "</span>";
                        responsiblePersonEmail.Attributes.Add("style", "display:none");

                        responsibleSection.InnerHtml = "<span class='sectionName'>" + Convert.ToString(recommendation.SectionName) + "</span>";

                        responsibleSectionId.InnerHtml = "<span class='sectionId'>" + Convert.ToString(recommendation.SectionId) + "</span>";
                        responsibleSectionId.Attributes.Add("style", "display:none");

                        responsibleDepartment.InnerHtml = "<span class='departmentName'>" + Convert.ToString(recommendation.DepartmentName) + "</span>";
                        injuryClassification.InnerHtml = "<span class='injuryClass'>" + Convert.ToString(recommendation.InjuryClass) + "</span>";

                        responsibleDepartmentId.InnerHtml = "<span class='departmentId'>" + Convert.ToString(recommendation.DepartmentId) + "</span>";
                        responsibleDepartmentId.Attributes.Add("style", "display:none");

                        consentTaken.InnerHtml = "<span class='consentTaken'>" + ((recommendation.ConsentTaken == true) ? "Yes" : "No") + "</span>";
                        targetDate.InnerHtml = "<span class='targetDate'>" + Convert.ToString(recommendation.TargetDate) + "</span>";
                        observationCategory.InnerHtml = "<span class='category'>" + Convert.ToString(recommendation.ObservationCategory) + "</span>";

                        observationSubCategory.InnerHtml = "<span class='subCategory'>" + Convert.ToString(recommendation.ObservationSubcategory) + "</span>";
                        //observationSubCategory.Attributes.Add("style", "display:none");

                        observationSpot.InnerHtml = "<span class='observationSpot'>" + ((recommendation.ObservationSpot == true) ? "Yes" : "No") + "</span>";
                        status.InnerHtml = "<span class='status'>" + Convert.ToString(recommendation.Status) + "</span>";

                        tRow.Cells.Add(recommendationId);
                        tRow.Cells.Add(description);
                        tRow.Cells.Add(typeOfVoilation);
                        tRow.Cells.Add(responsiblePersonUsername);
                        tRow.Cells.Add(responsibleSection);
                        tRow.Cells.Add(responsibleSectionId);
                        tRow.Cells.Add(responsibleDepartment);
                        tRow.Cells.Add(injuryClassification);
                        tRow.Cells.Add(responsibleDepartmentId);
                        tRow.Cells.Add(consentTaken);
                        tRow.Cells.Add(targetDate);
                        tRow.Cells.Add(observationCategory);
                        tRow.Cells.Add(observationSubCategory);
                        tRow.Cells.Add(observationSpot);
                        tRow.Cells.Add(status);

                        tRow.Cells.Add(new HtmlTableCell() { InnerHtml = actions });

                        switch (recommendation.ValidationStatus)
                        {
                            case 0:
                                {
                                    break;
                                }
                            case 1:
                                {
                                    tRow.Attributes.Add("style", "background-color: rgba(238, 118, 173, 0.88)");
                                    message_div.InnerHtml = "Responsible Persons in Highlighted Recommendations needs more permission. Please Contact the Administrator!";
                                    break;
                                }
                            case 2:
                                {
                                    tRow.Attributes.Add("style", "background-color: rgba(238, 118, 173, 0.88)");
                                    message_div.InnerHtml = "Target Date in Highlighted Recommendations are not valid. Please Contact the Administrator!";
                                    break;
                                }
                            case 3:
                                {
                                    tRow.Attributes.Add("style", "background-color: rgba(238, 118, 173, 0.88)");
                                    message_div.InnerHtml = "Target Date in Highlighted Recommendations must be greater than or equal to MSA date!";
                                    break;
                                }
                            default:
                                {
                                    break;
                                }

                        }

                        this.recommendationDetails_table.Rows.Add(tRow);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->FillRecommendationGrid)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return false;
        }
        private List<MSAContact> GetFormattedContactsByMSA(SPWeb oSPWeb, int msaId)
        {
            try
            {
                string listName = "MSAContactDetail";
                // Fetch the List
                SPList splistMSAContactDetail = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                List<MSAContact> lstMSAContact = new List<MSAContact>();

                if (splistMSAContactDetail != null)
                {
                    SPQuery query = new SPQuery();
                    SPListItemCollection spListItems;
                    // Include only the fields you will use.
                    query.ViewFields = "<FieldRef Name='ID'/><FieldRef Name='ContactDetail'/>";
                    query.ViewFieldsOnly = true;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<Where>")
                         .Append("  <Eq>")
                         .Append("    <FieldRef Name='MSAID' />")
                         .Append("    <Value Type='Number'>" + msaId + "</Value>")
                         .Append("  </Eq>")
                         .Append("</Where>");

                    query.Query = sb.ToString();
                    spListItems = splistMSAContactDetail.GetItems(query);

                    for (int i = 0; i < spListItems.Count; i++)
                    {
                        SPListItem listItem = spListItems[i];
                        MSAContact contact = new MSAContact();
                        contact.ContactId = Convert.ToInt32(listItem["ID"]);
                        contact.ContactDetail = Convert.ToString(listItem["ContactDetail"]);

                        lstMSAContact.Add(contact);
                    }
                }

                return lstMSAContact;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->GetFormattedContactsByMSA)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

            return null;
        }
        private List<MSAContact> GetFormattedContacts(string contacts, String[] pattern1, String[] pattern2)
        {
            try
            {
                List<MSAContact> lstMsaContacts = new List<MSAContact>();

                var lstContact = contacts.Split(pattern1, StringSplitOptions.None);

                foreach (var item in lstContact)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        var contactStr = item.Split(pattern2, StringSplitOptions.None);
                        if (contactStr.Length > 0)
                        {
                            MSAContact contact = new MSAContact();
                            contact.ContactId = Int32.Parse(contactStr[0]);
                            contact.ContactDetail = contactStr[1];

                            lstMsaContacts.Add(contact);
                        }
                    }
                }

                return lstMsaContacts;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->GetFormattedContacts)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

            return null;
        }
        private PairId GetFormattedIds(string ids, String[] pattern1, String[] pattern2)
        {
            try
            {
                PairId pairIds = new PairId();
                pairIds.ContactIds = new List<int>();
                pairIds.RecommendationIds = new List<int>();

                var pairOfIdsStr = ids.Split(pattern1, StringSplitOptions.None);

                if (pairOfIdsStr.Length > 1)
                {
                    var contactIds = pairOfIdsStr[0].Split(pattern2, StringSplitOptions.None); ;
                    var recommendadtionIds = pairOfIdsStr[1].Split(pattern2, StringSplitOptions.None); ;

                    foreach (var item in contactIds)
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            pairIds.ContactIds.Add(Int32.Parse(item));
                        }
                    }

                    foreach (var item in recommendadtionIds)
                    {
                        if (!String.IsNullOrEmpty(item))
                        {
                            pairIds.RecommendationIds.Add(Int32.Parse(item));
                        }
                    }
                }
                return pairIds;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->GetFormattedIds)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

            return null;
        }
        private List<MSARecommendation> GetFormattedRecommendationsByMSA(SPWeb oSPWeb, int msaId)
        {
            try
            {
                string listName = "MSARecommendation";
                // Fetch the List
                SPList splistMSARecommendation = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                List<MSARecommendation> lstMSARecommendation = new List<MSARecommendation>();

                if (splistMSARecommendation != null)
                {
                    SPQuery query = new SPQuery();
                    SPListItemCollection spListItems;
                    // Include only the fields you will use.
                    StringBuilder vf = new StringBuilder();
                    vf.Append("<FieldRef Name='ID'/>")
                        .Append("<FieldRef Name='RecommendationNo'/>")
                        .Append("<FieldRef Name='TargetDate'/>")
                        .Append("<FieldRef Name='MSARecommendationDescription'/>")
                        .Append("<FieldRef Name='TypeOfVoilation'/>")
                        .Append("<FieldRef Name='ResponsiblePerson'/>")
                        .Append("<FieldRef Name='AssigneeEmail'/>")
                        .Append("<FieldRef Name='Assignee'/>")
                        .Append("<FieldRef Name='ResponsibleSection'/>")
                        .Append("<FieldRef Name='ResponsibleDepartment'/>")
                        .Append("<FieldRef Name='InjuryClass'/>")
                        .Append("<FieldRef Name='ObservationCategory'/>")
                        .Append("<FieldRef Name='ObservationSubcategory'/>")
                        .Append("<FieldRef Name='ConsentTaken'/>")
                        .Append("<FieldRef Name='ObservationSpot'/>")
                        .Append("<FieldRef Name='Status'/>");

                    query.ViewFields = vf.ToString();
                    query.ViewFieldsOnly = true;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<Where>")
                         .Append("  <Eq>")
                         .Append("    <FieldRef Name='MSAID' />")
                         .Append("    <Value Type='Number'>" + msaId + "</Value>")
                         .Append("  </Eq>")
                         .Append("</Where>");

                    query.Query = sb.ToString();
                    spListItems = splistMSARecommendation.GetItems(query);

                    for (int i = 0; i < spListItems.Count; i++)
                    {
                        SPListItem listItem = spListItems[i];
                        MSARecommendation recommendation = new MSARecommendation();
                        recommendation.RecommendationId = Convert.ToInt32(listItem["ID"]);
                        recommendation.RecommendationNo = Convert.ToString(listItem["RecommendationNo"]);

                        string targetDateStr = Convert.ToString(listItem["TargetDate"]);

                        if (!String.IsNullOrEmpty(targetDateStr))
                        {
                            DateTime date;
                            bool bValid = DateTime.TryParse(targetDateStr, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                            if (bValid)
                            {
                                recommendation.TargetDate = date.ToShortDateString();
                            }
                            else
                            {
                                recommendation.TargetDate = Convert.ToDateTime(targetDateStr).ToShortDateString();
                            }
                        }

                        recommendation.Description = Convert.ToString(listItem["MSARecommendationDescription"]);
                        recommendation.TypeOfVoilation = Convert.ToString(listItem["TypeOfVoilation"]);
                        recommendation.RPUsername = Convert.ToString(listItem["ResponsiblePerson"]);
                        recommendation.RPEmail = Convert.ToString(listItem["AssigneeEmail"]);
                        recommendation.AssigneeUsername = Convert.ToString(listItem["Assignee"]);
                        recommendation.AssigneeEmail = Convert.ToString(listItem["AssigneeEmail"]);
                        recommendation.InjuryClass = Convert.ToString(listItem["InjuryClass"]);
                        recommendation.ObservationCategory = Convert.ToString(listItem["ObservationCategory"]);
                        recommendation.ObservationSubcategory = Convert.ToString(listItem["ObservationSubcategory"]);
                        recommendation.ConsentTaken = Convert.ToBoolean(listItem["ConsentTaken"]);
                        recommendation.ObservationSpot = Convert.ToBoolean(listItem["ObservationSpot"]);
                        recommendation.Status = Convert.ToString(listItem["Status"]);

                        if (listItem["ResponsibleSection"] != null)
                        {
                            recommendation.SectionId = Convert.ToInt32(listItem["ResponsibleSection"]);

                            //Section
                            listName = "Section";
                            // Fetch the List
                            SPList spList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                            if (spList != null && recommendation.SectionId > 0)
                            {
                                SPListItem spListItem = spList.GetItemById(recommendation.SectionId);

                                if (spListItem != null)
                                {
                                    recommendation.SectionName = Convert.ToString(spListItem["Title"]);
                                }
                            }
                        }

                        if (listItem["ResponsibleDepartment"] != null)
                        {
                            recommendation.DepartmentId = Convert.ToInt32(listItem["ResponsibleDepartment"]);

                            //Department
                            listName = "Department";
                            // Fetch the List
                            SPList spList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                            if (spList != null && recommendation.DepartmentId > 0)
                            {
                                SPListItem spListItem = spList.GetItemById(recommendation.DepartmentId);

                                if (spListItem != null)
                                {
                                    recommendation.DepartmentName = Convert.ToString(spListItem["Title"]);
                                }
                            }
                        }

                        lstMSARecommendation.Add(recommendation);
                    }
                }

                return lstMSARecommendation;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->GetFormattedRecommendationsByMSA)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

            return null;
        }
        private List<MSARecommendation> GetFormattedRecommendations(string recommendatons, String[] pattern1, String[] pattern2)
        {
            try
            {
                List<MSARecommendation> lstMSARecommendation = new List<MSARecommendation>();

                var lstRecommendation = recommendatons.Split(pattern1, StringSplitOptions.None);

                foreach (var item in lstRecommendation)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        var recommendationStr = item.Split(pattern2, StringSplitOptions.None);
                        if (recommendationStr.Length > 0)
                        {
                            MSARecommendation recommendation = new MSARecommendation();

                            recommendation.RecommendationId = String.IsNullOrEmpty(recommendationStr[0]) ? 0 : Int32.Parse(recommendationStr[0]);
                            recommendation.Description = recommendationStr[1];
                            recommendation.TypeOfVoilation = recommendationStr[2];
                            recommendation.RPUsername = recommendationStr[3];
                            recommendation.RPEmail = recommendationStr[4];
                            recommendation.AssigneeUsername = recommendationStr[3];
                            recommendation.AssigneeEmail = recommendationStr[4];
                            recommendation.SectionId = String.IsNullOrEmpty(recommendationStr[5]) ? 0 : Int32.Parse(recommendationStr[5]);
                            recommendation.SectionName = recommendationStr[6];
                            recommendation.InjuryClass = recommendationStr[7];
                            recommendation.ObservationCategory = recommendationStr[8];
                            recommendation.ObservationSubcategory = recommendationStr[9];
                            recommendation.DepartmentId = String.IsNullOrEmpty(recommendationStr[10]) ? 0 : Int32.Parse(recommendationStr[10]);
                            recommendation.DepartmentName = recommendationStr[11];
                            recommendation.TargetDate = recommendationStr[12];
                            recommendation.ObservationSpot = recommendationStr[13].Equals("yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            recommendation.ConsentTaken = recommendationStr[14].Equals("yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            recommendation.Status = recommendationStr[15];
                            recommendation.RecommendationNo = recommendationStr[16];
                            recommendation.IsSavedAsDraft = recommendationStr[17].Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false;
                            recommendation.ValidationStatus = 0;

                            lstMSARecommendation.Add(recommendation);
                        }
                    }
                }
                return lstMSARecommendation;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->GetFormattedRecommendations)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

            return null;
        }
        public bool SaveMSADetails(List<MSAContact> contacts, List<MSARecommendation> recommendations, bool isSavedAsDraft, String[] pattern1, String[] pattern2, int? msaId = null)
        {
            bool isSaved = false;
            try
            {
                List<Message> lstMessage = null;

                using (SPSite oSPsite = new SPSite(SPContext.Current.Web.Url))
                {
                    using (SPWeb oSPWeb = oSPsite.OpenWeb())
                    {
                        string msaDateStr = this.msaDate_dtc.SelectedDate != null ? this.msaDate_dtc.SelectedDate.ToShortDateString() : null;
                        string accompaniedBy = this.accompaniedBy_tf.Value;
                        string designation = this.designation_tf.Value;
                        SPUser auditedBy = null;
                        string areaAudited = null;
                        string startTimeStr = this.startTime_dtc.SelectedDate != null ? this.startTime_dtc.SelectedDate.ToShortTimeString() : null;
                        string endTimeStr = this.endTime_dtc.SelectedDate != null ? this.endTime_dtc.SelectedDate.ToShortTimeString() : null;
                        //start

                        string noOfUnsafeActs = null;
                        string noOfUnsafeConditions = null;
                        string noOfFatalityInjury = null;
                        string noOfSeriousInjury = null;

                        if (!String.IsNullOrEmpty(this.hdnCounts.Value))
                        {
                            var temp = this.hdnCounts.Value.Split('~');

                            if (temp.Length > 3)
                            {
                                noOfUnsafeActs = temp[0];
                                noOfUnsafeConditions = temp[1];
                                noOfFatalityInjury = temp[2];
                                noOfSeriousInjury = temp[3];
                            }
                        }

                        //end
                        string positivePoints = this.hdnPositivePointList.Value;
                        string areaOfImprovement = this.hdnAreaOfImprovementList.Value;
                        string msaQualityScore = null;

                        string p1 = "~|~";
                        if (pattern1.Length > 0)
                        {
                            p1 = pattern1[0];
                        }

                        //Positive Point List
                        List<string> lstPositivePoint = Utility.GetFormattedDataList(this.hdnPositivePointList.Value, p1, true);

                        //Area Of Improvement List
                        List<string> lstAreaOfImprovement = Utility.GetFormattedDataList(this.hdnAreaOfImprovementList.Value, p1, true);

                        //MSA Quality Score

                        int noOfPositivePoint = 0;
                        int noOfAreaImprovement = 0;
                        int noOfSafetyContacts = 0;
                        int noOfUnsafeActs_c = 0;
                        int noOfUnSafeConditions_c = 0;
                        int noOfSeriousInjury_c = 0;
                        int noOfFatality_c = 0;
                        int noOfSeriousInjury_c_ct = 0;
                        int noOfFatality_c_ct = 0;
                        int noOfImediateClosure = 0;
                        int noOfConsentTaken = 0;
                        double auditMinutes = 0;

                        DateTime startTime_temp = this.startTime_dtc.SelectedDate;
                        DateTime endTime_temp = this.endTime_dtc.SelectedDate;

                        TimeSpan timeDiff = endTime_temp.Subtract(startTime_temp);

                        if (timeDiff != null)
                        {
                            auditMinutes = timeDiff.TotalMinutes;
                        }

                        if (contacts != null)
                        {
                            noOfSafetyContacts = contacts.Count;
                        }
                        if (lstPositivePoint != null)
                        {
                            noOfPositivePoint = lstPositivePoint.Count;
                        }
                        if (lstAreaOfImprovement != null)
                        {
                            noOfAreaImprovement = lstAreaOfImprovement.Count;
                        }

                        foreach (var item in recommendations)
                        {
                            if (item.ConsentTaken == true)
                            {
                                noOfConsentTaken++;
                            }

                            if (item.ObservationSpot == true)
                            {
                                noOfImediateClosure++;
                            }


                            if (!String.IsNullOrEmpty(item.TypeOfVoilation))
                            {
                                if (item.TypeOfVoilation.Equals("Unsafe Act", StringComparison.OrdinalIgnoreCase))
                                {
                                    noOfUnsafeActs_c++;
                                }
                                else if (item.TypeOfVoilation.Equals("Unsafe Condition", StringComparison.OrdinalIgnoreCase))
                                {
                                    noOfUnSafeConditions_c++;
                                }
                            }

                            if (!String.IsNullOrEmpty(item.InjuryClass))
                            {
                                if (item.InjuryClass.Equals("Serious Injury", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (item.ConsentTaken == true)
                                    {
                                        noOfSeriousInjury_c_ct++;
                                    }
                                    noOfSeriousInjury_c++;
                                }
                                else if (item.InjuryClass.Equals("Fatality", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (item.ConsentTaken == true)
                                    {
                                        noOfFatality_c_ct++;
                                    }
                                    noOfFatality_c++;
                                }
                            }
                        }

                        //Update Values

                        noOfUnsafeActs = Convert.ToString(noOfUnsafeActs_c);
                        noOfUnsafeConditions = Convert.ToString(noOfUnSafeConditions_c);
                        noOfFatalityInjury = Convert.ToString(noOfFatality_c);
                        noOfSeriousInjury = Convert.ToString(noOfSeriousInjury_c);


                        msaQualityScore = Convert.ToString(CalculateMSAQualityScore(auditMinutes, noOfSafetyContacts, accompaniedBy, noOfPositivePoint, noOfAreaImprovement, noOfFatality_c_ct, noOfUnSafeConditions_c, noOfSeriousInjury_c_ct, noOfImediateClosure, noOfConsentTaken));


                        if (this.auditedBy_PeopleEditor.ResolvedEntities != null && this.auditedBy_PeopleEditor.ResolvedEntities.Count > 0)
                        {
                            var auditedBy_PE = (PickerEntity)this.auditedBy_PeopleEditor.ResolvedEntities[0];

                            if (auditedBy_PE != null)
                            {
                                auditedBy = Utility.GetUser(oSPWeb, auditedBy_PE.Key);

                                if (auditedBy == null)
                                {
                                    foreach (var item in auditedBy_PE.EntityDataElements)
                                    {
                                        if (Convert.ToString(item.First).Equals("Email", StringComparison.OrdinalIgnoreCase))
                                        {
                                            auditedBy = Utility.GetUser(oSPWeb, null, Convert.ToString(item.Second));
                                        }
                                    }
                                }

                                if (auditedBy == null)
                                {
                                    message_div.InnerHtml = "Information of Audited By is incomplete or needs more permission. Please Contact the Administrator!";
                                    isSaved = false;
                                }
                            }
                        }

                        if (this.areaAudited_ddl.SelectedItem != null && this.areaAudited_ddl.SelectedIndex > 0)
                        {
                            areaAudited = this.areaAudited_ddl.SelectedItem.Text;
                        }

                        //Validate MSA Details
                        //Success
                        if (IsValidMSA(msaDateStr, accompaniedBy, designation, auditedBy, areaAudited, startTimeStr, endTimeStr, noOfUnsafeActs, noOfUnsafeConditions, noOfFatalityInjury, noOfSeriousInjury, positivePoints, areaOfImprovement, msaQualityScore) && IsValidMSAData(oSPWeb, contacts, recommendations))
                        {
                            string listName = "MSA";

                            // Fetch the List
                            SPList list = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                            if (list != null)
                            {
                                SPListItem spListItem = null;

                                //Add a new item 
                                if (msaId == null)
                                {
                                    spListItem = list.Items.Add();
                                }
                                //Update existing item
                                else
                                {
                                    spListItem = list.Items.GetItemById((int)msaId);
                                }

                                if (spListItem != null)
                                {
                                    if (!String.IsNullOrEmpty(msaDateStr))
                                    {
                                        DateTime date;
                                        bool bValid = DateTime.TryParse(msaDateStr, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                                        if (bValid)
                                        {
                                            spListItem["MSADate"] = date;
                                        }
                                        else
                                        {
                                            spListItem["MSADate"] = Convert.ToDateTime(msaDateStr);
                                        }
                                    }
                                    if (auditedBy != null)
                                    {
                                        string tempUsername = Utility.GetUsername(auditedBy.LoginName, true);
                                        spListItem["AuditedBy"] = tempUsername;
                                    }
                                    if (!String.IsNullOrEmpty(areaAudited))
                                    {
                                        spListItem["AreaAudited"] = areaAudited;
                                    }
                                    if (!String.IsNullOrEmpty(accompaniedBy))
                                    {
                                        spListItem["AccompaniedBy"] = accompaniedBy;
                                    }
                                    if (!String.IsNullOrEmpty(designation))
                                    {
                                        spListItem["Designation"] = designation;
                                    }
                                    if (!String.IsNullOrEmpty(startTimeStr))
                                    {
                                        spListItem["StartTime"] = startTimeStr;
                                    }
                                    if (!String.IsNullOrEmpty(endTimeStr))
                                    {
                                        spListItem["EndTime"] = endTimeStr;
                                    }
                                    if (!String.IsNullOrEmpty(noOfUnsafeActs))
                                    {
                                        spListItem["NoOfUnsafeActs"] = noOfUnsafeActs;
                                    }
                                    if (!String.IsNullOrEmpty(noOfUnsafeConditions))
                                    {
                                        spListItem["NoOfUnsafeConditions"] = noOfUnsafeConditions;
                                    }
                                    if (!String.IsNullOrEmpty(noOfFatalityInjury))
                                    {
                                        spListItem["NoOfFatalityInjury"] = noOfFatalityInjury;
                                    }
                                    if (!String.IsNullOrEmpty(noOfSeriousInjury))
                                    {
                                        spListItem["NoOfSeriousInjury"] = noOfSeriousInjury;
                                    }
                                    if (!String.IsNullOrEmpty(positivePoints))
                                    {
                                        spListItem["PositivePoints"] = positivePoints;
                                    }
                                    if (!String.IsNullOrEmpty(areaOfImprovement))
                                    {
                                        spListItem["AreaOfImprovement"] = areaOfImprovement;
                                    }
                                    if (!String.IsNullOrEmpty(msaQualityScore))
                                    {
                                        spListItem["MSAQualityScore"] = msaQualityScore;
                                    }
                                    if (!String.IsNullOrEmpty(this.hdnScheduleId.Value))
                                    {
                                        spListItem["ScheduleId"] = Convert.ToInt32(this.hdnScheduleId.Value);
                                    }

                                    if (!String.IsNullOrEmpty(this.hdnFilesNames.Value))
                                    {
                                        var fileNames = hdnFilesNames.Value.Split('~');

                                        foreach (var item in fileNames)
                                        {
                                            if (!String.IsNullOrEmpty(item))
                                            {
                                                spListItem.Attachments.Delete(item);
                                            }
                                        }
                                    }

                                    if (this.fileUploadControl.HasFiles)
                                    {
                                        foreach (var uploadedFile in fileUploadControl.PostedFiles)
                                        {
                                            Stream fs = uploadedFile.InputStream;
                                            byte[] _bytes = new byte[fs.Length];
                                            fs.Position = 0;
                                            fs.Read(_bytes, 0, (int)fs.Length);
                                            fs.Close();
                                            fs.Dispose();

                                            spListItem.Attachments.Add(uploadedFile.FileName, _bytes);
                                        }
                                    }


                                    spListItem["NoOfSafetyContacts"] = noOfSafetyContacts;
                                    spListItem["IsSavedAsDraft"] = isSavedAsDraft;

                                    //Update added record
                                    oSPWeb.AllowUnsafeUpdates = true;
                                    spListItem.Update();
                                    oSPWeb.AllowUnsafeUpdates = false;

                                    if (msaId == null)
                                    {
                                        msaId = Convert.ToInt32(spListItem["ID"]);
                                    }

                                    this.hdnMSAId.Value = Convert.ToString(msaId);

                                    isSaved = true;

                                    PairId pairIds = null;

                                    if (!String.IsNullOrEmpty(this.hdnIdList.Value))
                                    {
                                        pairIds = GetFormattedIds(this.hdnIdList.Value, pattern1, pattern2);
                                    }

                                    if (pairIds != null)
                                    {
                                        //MSAContacts
                                        isSaved = SaveMSAContacts(oSPWeb, contacts, (int)msaId, pairIds.ContactIds);
                                    }
                                    else
                                    {
                                        isSaved = SaveMSAContacts(oSPWeb, contacts, (int)msaId);
                                    }

                                    if (isSaved)
                                    {
                                        if (auditedBy != null)
                                        {
                                            if (pairIds != null)
                                            {
                                                //MSARecommendations
                                                lstMessage = SaveMSARecommendations(oSPWeb, recommendations, (int)msaId, auditedBy.Email, pairIds.RecommendationIds);
                                            }
                                            else
                                            {
                                                lstMessage = SaveMSARecommendations(oSPWeb, recommendations, (int)msaId, auditedBy.Email);
                                            }

                                            if (lstMessage == null)
                                            {
                                                isSaved = false;
                                            }
                                        }
                                        else
                                        {
                                            isSaved = false;
                                        }
                                    }


                                    //Roll Back in case of error
                                    if (isSaved == false)
                                    {
                                        //Write some code here
                                    }
                                    else
                                    {
                                        if (isSaved && !String.IsNullOrEmpty(Convert.ToString(spListItem["ScheduleId"])))
                                        {
                                            string spListNameMS = "MSASchedule";

                                            // Fetch the List
                                            SPList spListMS = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, spListNameMS));

                                            if (spListMS != null)
                                            {
                                                int sid = Convert.ToInt32(spListItem["ScheduleId"]);
                                                SPListItem spListItemMS = spListMS.GetItemById(sid);

                                                if (spListItemMS != null)
                                                {
                                                    DateTime endTime;
                                                    DateTime msaDate;

                                                    if (spListItemMS["End Time"] != null && spListItem["MSADate"] != null)
                                                    {
                                                        endTime = Convert.ToDateTime(spListItemMS["End Time"]);
                                                        msaDate = Convert.ToDateTime(spListItem["MSADate"]);

                                                        if (endTime.Date >= msaDate.Date)
                                                        {
                                                            spListItemMS["MSA Status"] = spListMS.Fields["MSA Status"].GetFieldValue("Completed Before Due Date");
                                                        }
                                                        else
                                                        {
                                                            spListItemMS["MSA Status"] = spListMS.Fields["MSA Status"].GetFieldValue("Completed After Due Date");
                                                        }
                                                    }

                                                    oSPWeb.AllowUnsafeUpdates = true;
                                                    spListItemMS.Update();
                                                    oSPWeb.AllowUnsafeUpdates = false;
                                                }
                                            }
                                        }

                                        if (isSavedAsDraft == false)
                                        {
                                            if (lstMessage != null && lstMessage.Count > 0)
                                            {
                                                isSaved = Email.SendEmail(lstMessage);
                                            }

                                            if (isSaved)
                                            {
                                                SPUser currentUser = oSPWeb.CurrentUser;

                                                if (currentUser != null)
                                                {
                                                    //Decide values on the basis of operation
                                                    string msaLink = Utility.GetRedirectUrl("MSAFormLink");

                                                    StringBuilder linkSB = new StringBuilder();
                                                    linkSB.Append(msaLink)
                                                                .Append("?MSAID=")
                                                                .Append(spListItem.ID);

                                                    string subject = Utility.GetValueByKey("MSAFormSaved_Subject");
                                                    string body = Utility.GetValueByKey("MSAFormSaved");

                                                    body = body.Replace("~|~", linkSB.ToString());

                                                    if (String.IsNullOrEmpty(body))
                                                    {
                                                        body = linkSB.ToString();
                                                    }

                                                    if (String.IsNullOrEmpty(subject))
                                                    {
                                                        subject = "MSA of Area Audited: " + areaAudited;
                                                    }

                                                    Message message = new Message();
                                                    message.Subject = subject;
                                                    message.From = currentUser.Email;
                                                    message.To = currentUser.Email;
                                                    message.Body = body;

                                                    isSaved = Email.SendEmail(message);
                                                }
                                            }

                                            if (!isSaved)
                                            {
                                                message_div.InnerHtml = "MSA Saved Successfully but Email Sending Failed, Please Contact your Administrator.";
                                                DisableControls();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Failure
                        if (!isSaved)
                        {
                            //Retain Status of the page
                            bool statusContacts = FillContactGrid(contacts);
                            bool statusRecommendations = FillRecommendationGrid(recommendations);
                            bool statusAreaOfImprovement = FillAreaOfImprovementGrid(lstAreaOfImprovement);
                            bool statusPositivePoint = FillPositivePointGrid(lstPositivePoint);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->SaveMSADetails)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
            return isSaved;
        }

        private bool IsValidMSA(string msaDate,
                                string accompaniedBy,
                                string designation,
                                SPUser auditedBy_SPUser,
                                string areaAuditedListItem,
                                string startTimeStr,
                                string endTimeStr,
                                string noOfUnsafeActs,
                                string noOfUnsafeConditions,
                                string noOfFatalityInjury,
                                string noOfSeriousInjury,
                                string positivePoints,
                                string areaOfImprovement,
                                string msaQualityScore)
        {
            try
            {
                if (auditedBy_SPUser != null && !String.IsNullOrEmpty(auditedBy_SPUser.Email))
                {
                    if (!String.IsNullOrEmpty(endTimeStr) && !String.IsNullOrEmpty(startTimeStr))
                    {
                        if (Convert.ToDateTime(endTimeStr) != null && Convert.ToDateTime(startTimeStr) != null)
                        {
                            DateTime date;
                            bool bValid = DateTime.TryParse(msaDate, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                            if (!bValid)
                            {
                                Convert.ToDateTime(msaDate);
                            }

                            if (this.fileUploadControl.HasFiles)
                            {
                                int maxFileLimit = 20971520;

                                foreach (var uploadedFile in fileUploadControl.PostedFiles)
                                {
                                    if (uploadedFile.ContentLength > maxFileLimit)
                                    {
                                        message_div.InnerHtml = "Attachment file size limit is 20MB. Please reattach files.";
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->IsValidMSA)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
                DisableControls();
            }
            return false;
        }

        private bool IsValidMSAData(SPWeb oSPWeb, List<MSAContact> contactList, List<MSARecommendation> recommendationList)
        {
            bool isValid = true;

            try
            {
                foreach (var recommendation in recommendationList)
                {
                    SPUser responsiblePerson = null;

                    if (!String.IsNullOrEmpty(recommendation.RPUsername))
                    {
                        responsiblePerson = Utility.GetUser(null, recommendation.RPUsername);

                        if (responsiblePerson == null && !String.IsNullOrEmpty(recommendation.RPEmail))
                        {
                            responsiblePerson = Utility.GetUser(oSPWeb, null, recommendation.RPEmail);
                        }
                    }

                    if (responsiblePerson == null)
                    {
                        recommendation.ValidationStatus = 1;
                        isValid = false;
                    }
                    else
                    {
                        recommendation.ResponsiblePerson = responsiblePerson;
                    }


                    if (!String.IsNullOrEmpty(recommendation.TargetDate))
                    {
                        DateTime date;
                        bool bValid = DateTime.TryParse(recommendation.TargetDate, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                        if (!bValid)
                        {
                            bValid = DateTime.TryParse(recommendation.TargetDate, new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out date);

                            if (!bValid)
                            {
                                recommendation.ValidationStatus = 2;
                                isValid = false;
                            }
                        }

                        string msaDateStr = this.msaDate_dtc.SelectedDate != null ? this.msaDate_dtc.SelectedDate.ToShortDateString() : null;

                        DateTime msaDate;
                        bool bValidMsaDate = DateTime.TryParse(msaDateStr, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out msaDate);

                        if (!bValidMsaDate)
                        {
                            msaDate = Convert.ToDateTime(msaDateStr);
                        }

                        if (date < msaDate)
                        {
                            recommendation.ValidationStatus = 3;
                            isValid = false;
                        }
                    }
                    else
                    {
                        recommendation.ValidationStatus = 2;
                        isValid = false;
                    }

                }


                return isValid;
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->IsValidMSAData)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            return isValid;
        }

        public bool SaveMSAContacts(SPWeb oSPWeb, List<MSAContact> contacts, int msaId, List<int> contactIds = null)
        {
            try
            {
                if (oSPWeb != null && msaId > 0)
                {
                    string listName = "MSAContactDetail";

                    // Fetch the List
                    SPList spList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                    foreach (var item in contacts)
                    {
                        SPListItem itemToAdd = null;

                        if (item.ContactId > 0)
                        {
                            itemToAdd = spList.GetItemById(item.ContactId);
                            if (contactIds != null && contactIds.Count > 0)
                            {
                                contactIds.Remove(item.ContactId);
                            }
                        }
                        else
                        {
                            //Add a new item in the List
                            itemToAdd = spList.Items.Add();
                        }

                        if (itemToAdd != null)
                        {
                            itemToAdd["MSAID"] = Convert.ToInt32(msaId);
                            itemToAdd["ContactDetail"] = item.ContactDetail;

                            oSPWeb.AllowUnsafeUpdates = true;
                            itemToAdd.Update();
                            oSPWeb.AllowUnsafeUpdates = false;
                        }
                    }

                    if (contactIds != null && contactIds.Count > 0)
                    {
                        foreach (var id in contactIds)
                        {
                            var spListItem = spList.GetItemById(id);

                            if (spListItem != null)
                            {
                                oSPWeb.AllowUnsafeUpdates = true;
                                spListItem.Delete();
                                oSPWeb.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->SaveMSAContacts)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
            return false;
        }

        public List<Message> SaveMSARecommendations(SPWeb oSPWeb, List<MSARecommendation> recommendations, int msaId, string sentFrom, List<int> recommendationIds = null)
        {
            try
            {
                List<Message> lstMessage = new List<Message>();

                if (oSPWeb != null)
                {
                    string listName = "MSARecommendation";

                    // Fetch the List
                    SPList spList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));
                    int itemCount = spList.ItemCount + 1;


                    foreach (var item in recommendations)
                    {
                        Message message = new Message();

                        SPListItem itemToAdd = null;

                        if (item.RecommendationId > 0)
                        {
                            itemToAdd = spList.GetItemById(item.RecommendationId);
                            if (recommendationIds != null && recommendationIds.Count > 0)
                            {
                                recommendationIds.Remove(item.RecommendationId);
                            }
                        }
                        else
                        {
                            //Add a new item in the List
                            itemToAdd = spList.Items.Add();
                        }

                        if (itemToAdd != null)
                        {
                            SPUser responsiblePerson = null;

                            if (!String.IsNullOrEmpty(item.RPUsername))
                            {
                                responsiblePerson = Utility.GetUser(oSPWeb, item.RPUsername);

                                if (responsiblePerson == null && !String.IsNullOrEmpty(item.RPEmail))
                                {
                                    responsiblePerson = Utility.GetUser(oSPWeb, null, item.RPEmail);
                                    if (responsiblePerson != null)
                                    {
                                        item.RPUsername = Utility.GetUsername(responsiblePerson.LoginName, true);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                            else
                            {
                                return null;
                            }

                            if (responsiblePerson == null)
                            {
                                message_div.InnerHtml = "Information of Responsible Person is incomplete or needs more permission. Please Contact the Administrator!";
                                return null;
                            }

                            itemToAdd["MSAID"] = Convert.ToInt32(msaId);


                            string tempRecommendationNo = "";

                            if (item.SectionId > 0)
                            {
                                //Section
                                listName = "Section";
                                // Fetch the List
                                SPList spSectionList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                                if (spList != null && item.SectionId > 0)
                                {
                                    SPListItem spSectionListItem = spSectionList.GetItemById(item.SectionId);

                                    if (spSectionListItem != null)
                                    {
                                        tempRecommendationNo += Convert.ToString(spSectionListItem["SectionCode"]);
                                    }
                                }
                            }

                            tempRecommendationNo += "-";

                            if (item.DepartmentId > 0)
                            {
                                //Department
                                listName = "Department";
                                // Fetch the List
                                SPList spDepartmentList = oSPWeb.GetList(string.Format("{0}/Lists/{1}/AllItems.aspx", oSPWeb.Url, listName));

                                if (spList != null && item.DepartmentId > 0)
                                {
                                    SPListItem spDepartmentListItem = spDepartmentList.GetItemById(item.DepartmentId);

                                    if (spDepartmentListItem != null)
                                    {
                                        tempRecommendationNo += Convert.ToString(spDepartmentListItem["DepartmentCode"]);
                                    }
                                }
                            }

                            itemToAdd["ResponsiblePerson"] = item.RPUsername;
                            itemToAdd["Assignee"] = item.RPUsername;
                            if (responsiblePerson != null && !String.IsNullOrEmpty(responsiblePerson.Email))
                            {
                                itemToAdd["AssigneeEmail"] = responsiblePerson.Email;
                            }
                            else if (!String.IsNullOrEmpty(item.RPEmail) && !item.RPEmail.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                itemToAdd["AssigneeEmail"] = item.RPEmail;
                            }
                            else
                            {
                                message_div.InnerHtml = "Responsible Person Email Address not available";
                                return null;
                            }

                            itemToAdd["MSARecommendationDescription"] = item.Description;
                            itemToAdd["TypeOfVoilation"] = item.TypeOfVoilation;
                            itemToAdd["ResponsibleSection"] = item.SectionId;
                            itemToAdd["ResponsibleDepartment"] = item.DepartmentId;
                            itemToAdd["InjuryClass"] = item.InjuryClass;
                            itemToAdd["ObservationCategory"] = item.ObservationCategory;
                            itemToAdd["ObservationSubcategory"] = item.ObservationSubcategory;
                            itemToAdd["ConsentTaken"] = item.ConsentTaken;


                            if (!String.IsNullOrEmpty(item.TargetDate))
                            {
                                DateTime date;
                                bool bValid = DateTime.TryParse(item.TargetDate, new CultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out date);

                                if (bValid)
                                {
                                    itemToAdd["TargetDate"] = date;
                                }
                                else
                                {
                                    itemToAdd["TargetDate"] = Convert.ToDateTime(item.TargetDate);
                                }
                            }

                            itemToAdd["ObservationSpot"] = item.ObservationSpot;
                            itemToAdd["Status"] = item.Status;
                            itemToAdd["IsSavedAsDraft"] = item.IsSavedAsDraft;

                            oSPWeb.AllowUnsafeUpdates = true;
                            itemToAdd.Update();
                            oSPWeb.AllowUnsafeUpdates = false;

                            string itemID = Convert.ToString(itemToAdd.ID);

                            int length = itemID.Length;

                            string recommendationNo = "";

                            for (int i = 0; i < 6 - length; i++)
                            {
                                recommendationNo += "0";
                            }

                            recommendationNo += itemID + "-" + tempRecommendationNo;

                            itemToAdd["RecommendationNo"] = recommendationNo;

                            oSPWeb.AllowUnsafeUpdates = true;
                            itemToAdd.Update();
                            oSPWeb.AllowUnsafeUpdates = false;

                            if (!Convert.ToString(itemToAdd["Status"]).Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            {
                                StringBuilder linkSB = new StringBuilder();

                                string recommendationLink = Utility.GetRedirectUrl("MSARecommendationFormLink");

                                linkSB.Append(recommendationLink)
                                    .Append("?MSARID=")
                                    .Append(itemToAdd.ID);

                                string body = Utility.GetValueByKey("From_AuditedBy_To_ResponsiblePerson_RE");

                                body = body.Replace("~|~", linkSB.ToString());

                                if (String.IsNullOrEmpty(body))
                                {
                                    body = linkSB.ToString();
                                }

                                SPUser toUser = null;

                                if (!String.IsNullOrEmpty(item.RPUsername) && !item.RPUsername.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    toUser = Utility.GetUser(oSPWeb, item.RPUsername);
                                }

                                message.From = sentFrom;

                                if (toUser != null)
                                {
                                    message.To = toUser.Email;
                                }
                                else if (!String.IsNullOrEmpty(item.RPEmail) && !item.RPEmail.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    message.To = item.RPEmail;
                                }

                                message.Subject = Utility.GetValueByKey("From_AuditedBy_To_ResponsiblePerson_RE_Subject");
                                message.Body = body;

                                lstMessage.Add(message);
                            }
                            else if(Convert.ToString(itemToAdd["ObservationSpot"]).Equals("True", StringComparison.OrdinalIgnoreCase))
                            {
                                StringBuilder linkSB1 = new StringBuilder();

                                string recommendationLink1 = Utility.GetRedirectUrl("MSARecommendationFormLink");

                                linkSB1.Append(recommendationLink1)
                                    .Append("?MSARID=")
                                    .Append(itemToAdd.ID);

                                string body = Utility.GetValueByKey("From_AuditedBy_To_ResponsiblePerson_OnSpot");

                                body = body.Replace("~|~", linkSB1.ToString());

                                if (String.IsNullOrEmpty(body))
                                {
                                    body = linkSB1.ToString();
                                }

                                SPUser toUser = null;

                                if (!String.IsNullOrEmpty(item.RPUsername) && !item.RPUsername.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    toUser = Utility.GetUser(oSPWeb, item.RPUsername);
                                }

                                message.From = sentFrom;

                                if (toUser != null)
                                {
                                    message.To = toUser.Email;
                                }
                                else if (!String.IsNullOrEmpty(item.RPEmail) && !item.RPEmail.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    message.To = item.RPEmail;
                                }

                                message.Subject = Utility.GetValueByKey("From_AuditedBy_To_ResponsiblePerson_OnSpot_Subject");
                                message.Body = body;

                                lstMessage.Add(message);                                
                            }
                        }
                    }
                    if (recommendationIds != null && recommendationIds.Count > 0)
                    {
                        foreach (var id in recommendationIds)
                        {
                            var spListItem = spList.GetItemById(id);

                            if (spListItem != null)
                            {
                                oSPWeb.AllowUnsafeUpdates = true;
                                spListItem.Delete();
                                oSPWeb.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                    return lstMessage;
                }
            }

            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->SaveMSARecommendations)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
            return null;
        }


        //Events
        protected void btnSaveAsDraft_Click(object sender, EventArgs e)
        {
            try
            {
                var pattern1 = new[] { "~|~" };
                var pattern2 = new[] { "*|*" };

                string contactListStr = this.hdnContactList.Value;
                string recommendationListStr = this.hdnRecommendationList.Value;

                var contactList = this.GetFormattedContacts(contactListStr, pattern1, pattern2);
                var recommendationList = this.GetFormattedRecommendations(recommendationListStr, pattern1, pattern2);

                bool isSaved = false;
                if (contactList != null && recommendationList != null)
                {

                    if (!String.IsNullOrEmpty(this.hdnMSAId.Value))
                    {
                        isSaved = SaveMSADetails(contactList, recommendationList, true, pattern1, pattern2, Convert.ToInt32(this.hdnMSAId.Value));
                    }
                    else
                    {
                        isSaved = SaveMSADetails(contactList, recommendationList, true, pattern1, pattern2);
                    }
                }

                if (isSaved)
                {
                    string redirectUrl = Utility.GetRedirectUrl("MSAForm_SaveAsDraft_Redirect");

                    if (!String.IsNullOrEmpty(redirectUrl))
                    {
                        DisableControls();
                        Page.Response.Redirect(redirectUrl, false);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(message_div.InnerHtml.Replace("\r", " ").Replace("\n", " ").Trim()))
                    {
                        message_div.InnerHtml = "Operation Save Failed. Kindly verify that you provide valid information.";
                        DisableControls();
                    }

                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->btnSaveAsDraft_Click)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var pattern1 = new[] { "~|~" };
                var pattern2 = new[] { "*|*" };

                string contactListStr = this.hdnContactList.Value;
                string recommendationListStr = this.hdnRecommendationList.Value;

                var contactList = this.GetFormattedContacts(contactListStr, pattern1, pattern2);
                var recommendationList = this.GetFormattedRecommendations(recommendationListStr, pattern1, pattern2);

                bool isSaved = false;
                if (contactList != null && recommendationList != null)
                {
                    if (!String.IsNullOrEmpty(this.hdnMSAId.Value))
                    {
                        isSaved = SaveMSADetails(contactList, recommendationList, false, pattern1, pattern2, Convert.ToInt32(this.hdnMSAId.Value));
                    }
                    else
                    {
                        isSaved = SaveMSADetails(contactList, recommendationList, false, pattern1, pattern2);
                    }
                }

                if (isSaved)
                {
                    string redirectUrl = Utility.GetRedirectUrl("MSAForm_Save_Redirect");

                    if (!String.IsNullOrEmpty(redirectUrl))
                    {
                        DisableControls();
                        Page.Response.Redirect(redirectUrl, false);
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(message_div.InnerHtml.Replace("\r", " ").Replace("\n", " ").Trim()))
                    {
                        message_div.InnerHtml = "Operation Save Failed. Kindly verify that you provide valid information.";
                        DisableControls();
                    }
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->btnSave_Click)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                string redirectUrl = Utility.GetRedirectUrl("MSAForm_Cancel_Redirect");

                if (!String.IsNullOrEmpty(redirectUrl))
                {
                    DisableControls();
                    Page.Response.Redirect(redirectUrl, false);
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("SL.FG.FFL(MSAForm->btnCancel_Click)", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);

                message_div.InnerHtml = "Something went wrong!!! Please Contact the administrator.";
                DisableControls();
            }
        }
    }
}
