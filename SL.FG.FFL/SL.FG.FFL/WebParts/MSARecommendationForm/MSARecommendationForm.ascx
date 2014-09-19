<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MSARecommendationForm.ascx.cs" Inherits="SL.FG.FFL.WebParts.MSARecommendationForm.MSARecommendationForm" %>


<link href="/_layouts/15/SL.FG.FFL/CSS/FGStyle.css" rel="stylesheet" />

<style type="text/css">
    [id$=responsiblePerson_PeopleEditor_upLevelDiv] {
        border-radius: 5px !important;
        width: 100%;
        background-color: lightgray !important;
    }

    [id$=responsiblePerson_PeopleEditor_TopSpan] {
        border-radius: 5px !important;
        width: 100%;
        background-color: lightgray !important;
    }



</style>


<div class="container">
    <div class="row">
        <div class="col-lg-12">
            <div id="message_div" runat="server" class="messageDiv">
            </div>
            <div class="panel panel-success">
                <div class="panel-heading">
                    <h5>MSA Recommendation Form</h5>
                </div>
                <div class="panel-body">
                    <div id="printableArea">
                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Recommendation No</label>
                                    <input type='text' class="form-control disableControl" id="recommendationNo_tf" runat="server" disabled  />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Approval Authority</label>
                                    <asp:DropDownList ID="approvalAuthority_ddl" runat="server" CssClass="form-control" AutoPostBack="false" />
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label>Description</label>
                            <textarea class="form-control disableControl" id="description_ta" runat="server" rows="5" disabled></textarea>
                        </div>
                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Target Date</label>
                                    <input type='text' class="form-control disableControl" id="targetDate_tf" runat="server" disabled />
                                </div>
                            </div>
                            <div class="col-lg-6 table-responsive">
                                <label>Responsible Person<span style="color: red">&nbsp;*</span></label>
                                <SharePoint:PeopleEditor runat="server" ID="responsiblePerson_PeopleEditor" AllowEmpty="false" SelectionSet="User" AllowTypeIn="false"
                                    Rows="1" MultiSelect="false" ShowButtons="false" CssClass="disableControl" />
                            </div>
                        </div>
                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Responsible Department</label>
                                    <input type='text' class="form-control disableControl" id="responsibleDepartment_tf" runat="server" disabled />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Responsible Section</label>
                                    <input type='text' class="form-control disableControl" id="responsibleSection_tf" runat="server" disabled />
                                </div>
                            </div>
                        </div>

                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Status</label>
                                    <select class="form-control disableControl" id="status_ddl" runat="server" disabled>
                                        <option>Pending</option>
                                        <option>In Progress</option>
                                        <option>Completed</option>
                                    </select>
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Injury Classification</label>
                                    <input id="injuryClassification_tf" type='text' class="form-control disableControl" runat="server" disabled />
                                </div>
                            </div>
                        </div>

                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Violation Category</label>
                                    <input type='text' class="form-control disableControl" id="typeOfVoilation_tf" runat="server" disabled />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Observation Category</label>
                                    <input type='text' class="form-control disableControl" id="observationCategoryA_tf" runat="server" disabled />
                                </div>
                            </div>
                        </div>

                        <div class="form-group">
                            <label>Last Comment</label>
                            <textarea id="lastStatement_ta" class="form-control disableControl" runat="server" disabled></textarea>
                        </div>

                        <div class="form-group">
                            <label>Closure Justification<span style="color: red">&nbsp;*</span></label>
                            <textarea id="closureJustification_ta" class="form-control" runat="server"></textarea>
                        </div>

                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Closure Date</label>
                                    <div class="form-group">
                                        <SharePoint:DateTimeControl ID="closureDate_dtc" runat="server" CssClassTextBox="form-control disableControl" AutoPostBack="false" UseTimeZoneAdjustment="false" LocaleId="2057"  />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="form-group row">
                            <div class="col-lg-6">
                                <div class="form-group">
                                    <label>Assigned By</label>
                                    <input type='text' class="form-control disableControl" id="initiatedBy_tf" runat="server" disabled />
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="form-group" id="approvedBy_div" runat="server" visible="false">
                                    <label>Approved By</label>
                                    <input type='text' class="form-control disableControl" id="approvedBy_tf" runat="server" disabled />
                                </div>
                            </div>
                        </div>

                        <div class="form-group" id="viewHistory_div" style="display: none;">
                            <p class="dataHeading">Closure Justification History</p>
                            <div id="history_div" runat="server"></div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>Attachment</label>
                        <div>
                            <table id="grdAttachments" runat="server">
                            </table>
                        </div>
                        <asp:FileUpload ID="fileUploadControl" runat="server" AllowMultiple="true" />
                    </div>
                </div>

                <asp:HiddenField ID="hdnFilesNames" runat="server" Value="" />
                <asp:HiddenField ID="hdnRecommendationId" runat="server" Value="" />
                <asp:HiddenField ID="hdnApprovalAuthorityEmail" runat="server" Value="" />

                <div class="form-group pull-right" style="margin-top: 15px;">
                    <input id="print_btn" value="Print" type="button" class="btnPrint" onclick="printContent();" />
                    <asp:Button ID="btnSend" runat="server" Text="Send" OnClick="btnSend_Click" OnClientClick="return isActionConfirmed('Send');" CssClass="btnSend" />
                    <asp:Button ID="btnApprove" runat="server" Text="Approve" OnClick="btnApprove_Click" OnClientClick="return isActionConfirmed('Approve');" CssClass="btnApprove" />
                    <asp:Button ID="btnReject" runat="server" Text="Reject" OnClick="btnReject_Click" OnClientClick="return isActionConfirmed('Reject');" CssClass="btnReject" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" OnClientClick="return isActionConfirmed('Cancel');" CssClass="btnCancel" />
                    <input id="viewHistory_btn" value="View History" type="button" class="btnViewHistory" />
                    <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" OnClientClick="return isActionConfirmed('Save');" CssClass="btnSave" Visible="false" />
                </div>
            </div>
        </div>
    </div>
</div>

<script src="/_layouts/15/SL.FG.FFL/Scripts/jQuery.js"></script>


<script type="text/javascript">
    function isActionConfirmed(action) {

        var message = "MSA Recommendation: Are you sure you want to perform this action?";
        var flag = false;

        if (typeof action != 'undefined' && action != null && action != "") {
            if (action == "Save") {
                message = "Do you want to Save?";
                flag = true;
            }
            else if (action == "Approve") {
                message = "Do you want to Approve?";
                flag = true;
            }
            else if (action == "Reject") {
                message = "Do you want to Reject?";
                flag = true;
            }
            else if (action == "Send") {
                message = "Do you want to Send?";
                flag = true;
            }
        }

        var confirm = window.confirm(message);
        if (!confirm) {
            return false;
        }

        if (flag == true && $('[id$=closureJustification_ta]').val() == "")
        {
            alert("Please Provide value for the required fields");
            $('[id$=closureJustification_ta]').focus();
            return false;
        }

        if (flag == true && $('[id$=approvalAuthority_ddl] option:selected').val() == "0") {
            alert("Please Provide value for the required fields");
            $('[id$=approvalAuthority_ddl]').focus();
            return false;
        }
        return true;
    }

    function printContent() {
        var data = $("#printableArea").html();

        var popupWindow = window.open('MSA Recommendation', 'printwin', 'left=10,top=10,width=1000,height=1000');
        popupWindow.document.write('<HTML>\n<HEAD>\n');
        popupWindow.document.write('<TITLE>MSA Recommendation Form</TITLE>\n');
        popupWindow.document.write('<URL></URL>\n');
        popupWindow.document.write("<link href='/_layouts/15/SL.FG.FFL/CSS/BS3/bootstrap3.min.css' rel='stylesheet'/>\n");
        popupWindow.document.write("<link href='/_layouts/15/SL.FG.FFL/CSS/FGStyle.css' rel='stylesheet'/>\n");
        popupWindow.document.write('<script>\n');
        popupWindow.document.write('function print_win(){\n');
        popupWindow.document.write('\n window.print();\n');
        popupWindow.document.write('}\n');
        popupWindow.document.write('<\/script>\n');
        popupWindow.document.write('</HEAD>\n');
        popupWindow.document.write('<BODY onload="print_win()" style="margin: 10px 10px 10px 10px; overflow:scroll;">\n');
        popupWindow.document.write(data);
        popupWindow.document.write('</BODY>\n');
        popupWindow.document.write('</HTML>\n');
        popupWindow.document.close();
    }

    $(document).ready(function () {

        $('[id$=viewHistory_btn]').on('click', function () {
            $('[id$=viewHistory_div]').toggle();
        });

        $('span.removeLink').on('click', function () {
            var par = $(this).closest('tr');
            var fileName = par.find('span.fileName');

            if (fileName != 'undefined' && fileName != "" && fileName != null) {
                var filenames = $('[id$=hdnFilesNames]').val();
                filenames += "~" + fileName.text();

                $('[id$=hdnFilesNames]').val(filenames);
            }
            par.remove();
        });

        // Capturing when the user modifies a field
        var warnMessage = 'You have unsaved changes on this page!';
        var formModified = new Boolean();
        formModified = false;
        $('input:not(:button,:submit),textarea,select').on('change', function () {
            formModified = true;
        });
        // Checking if the user has modified the form upon closing window
        $('input:submit').on('click', function (e) {
            formModified = false;
        });
        window.onbeforeunload = function () {
            if (formModified != false) return warnMessage;
        }

        $('.panel-collapse').collapse('show');

        $('[id$=responsiblePerson_PeopleEditor_downlevelTextBox]').attr("disabled", "disabled");
    });
</script>
