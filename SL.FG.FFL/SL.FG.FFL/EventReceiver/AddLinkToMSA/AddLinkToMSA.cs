using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.Administration;

namespace SL.FG.FFL.EventReceiver.AddLinkToMSA
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class AddLinkToMSA : SPItemEventReceiver
    {
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {

            try
            {
                SPList spList = properties.List;

                if (spList.Title.Equals("MSA Schedule"))
                {

                    SPFieldUrlValue spFieldURL = new SPFieldUrlValue();
                    spFieldURL.Url = "/sites/ffl/Pages/MSA.aspx?SID=" + properties.ListItemId;
                    spFieldURL.Description = "Please click here";

                    SPListItem spListItem = properties.ListItem;
                    spListItem["MSAFormLink"] = spFieldURL;
                    spListItem.Update();
                }
                else
                {
                    properties.Status = SPEventReceiverStatus.CancelNoError;
                }

            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("MSAEventReceiver", TraceSeverity.Unexpected, EventSeverity.Error), TraceSeverity.Unexpected, ex.Message, ex.StackTrace);
            }
            finally
            {
                base.ItemAdded(properties);
            }
        }
    }
}