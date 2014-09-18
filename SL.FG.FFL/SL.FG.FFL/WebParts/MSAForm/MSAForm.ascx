<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MSAForm.ascx.cs" Inherits="SL.FG.FFL.WebParts.MSAForm.MSAForm" %>

<script type="text/javascript">
    function isActionConfirmed(action) {

        var message = "MSA: Are you sure you want to perform this action?";

        if (typeof action != 'undefined' && action != null && action != "") {
            if (action == "Save") {
                message = "Do you want to Submit?";
            }
            else if (action == "SaveAsDraft") {
                message = "Do you want to Save as Draft?";
            }
        }

        var confirm = window.confirm(message);
        if (!confirm) {
            return false;
        }
        return true;
    }

    //Extract Email from content
    function extractEmails(text) {
        return text.match(/([a-zA-Z0-9._-]+@[a-zA-Z0-9._-]+\.[a-zA-Z0-9._-]+)/gi);
    }

    //Extract Username from content
    function extractUsernames(option) {
        var username;
        if (option == "1") {
            username = $("[id$=responsiblePerson_PeopleEditor] span.ms-entity-resolved").attr("title");
        }
        return username;
    }

    function ValidationHandler(control, controlName) {
        alert(controlName + ":  " + "Please Provide value for the required field.");
        control.focus();
    }

    function ValidationSummary(message, controls) {

        alert("Message: " + message + "\n\n" + controls);
    }

    function getAuditHours() {
        var startTime_hours = $('[id$=startTime_dtcDateHours]').val();
        var startTime_minutes = $('[id$=startTime_dtcDateMinutes]').val();
        var endTime_hours = $('[id$=endTime_dtcDateHours]').val();
        var endTime_minutes = $('[id$=endTime_dtcDateMinutes]').val();

        startTime_hours = startTime_hours.split(' ');
        endTime_hours = endTime_hours.split(' ');

        var totalMinutes = 0;

        if (startTime_minutes > 0 && endTime_minutes > 0) {
            if (startTime_minutes > endTime_minutes) {
                startTime_minutes = 60 - startTime_minutes;
                totalMinutes = parseInt(endTime_minutes) + parseInt(startTime_minutes)
            }
            else if (startTime_minutes == endTime_minutes) {
                if (startTime_hours[0] == endTime_hours[0]) {
                    totalMinutes = 0;
                }
                else {
                    totalMinutes = 60;
                }
            }
            else if (startTime_minutes < endTime_minutes) {

                totalMinutes = parseInt(endTime_minutes) - parseInt(startTime_minutes);
            }
        }
        else {
            if (startTime_minutes == 0) {
                totalMinutes = parseInt(endTime_minutes)
            }
            else {
                startTime_minutes = 60 - startTime_minutes;
                totalMinutes = parseInt(startTime_minutes)
            }
        }


        var noOfHours;

        var startTimeHour = startTime_hours[0];
        var startTimeFrame = startTime_hours[1];

        var endTimeHour = endTime_hours[0];
        var endTimeFrame = endTime_hours[1];

        if (startTimeFrame == 'AM' && endTimeFrame == 'AM') {
            noOfHours = (endTimeHour - startTimeHour);
        }
        else if (startTimeFrame == 'PM' && endTimeFrame == 'PM') {
            noOfHours = (endTimeHour - startTimeHour);
        }
        else if (startTimeFrame == 'AM' && endTimeFrame == 'PM') {
            if (startTimeHour == '12') {
                noOfHours = (parseInt(endTimeHour) + parseInt(startTimeHour));
            }
            else {
                noOfHours = (parseInt(endTimeHour) - parseInt(startTimeHour) + 12);
            }
        }
        else if (startTimeFrame == 'PM' && endTimeFrame == 'AM') {
            if (startTimeHour == '12') {
                noOfHours = (parseInt(endTimeHour) + parseInt(startTimeHour));
            }
            else {
                noOfHours = (parseInt(endTimeHour) - parseInt(startTimeHour) + 12);
            }
        }

        if (noOfHours < 0) {
            noOfHours = 12 + noOfHours;
        }

        if (parseInt(totalMinutes) < 60 && startTime_minutes > endTime_minutes) {
            noOfHours = parseFloat(noOfHours) - 1;
        }

        if (noOfHours != 'undefined' && totalMinutes != 'undefined') {
            noOfHours = parseFloat(noOfHours) + parseFloat((totalMinutes) / 60.0);
        }

        return noOfHours;
    }


    function updateInjuryAndVoilationCount() {
        try {
            var noOfUnsafeAct = 0;
            var noOfUnsafeCondition = 0;
            var noOfSeriousInjury = 0;
            var noOfFatalityPotentials = 0;
            //var auditHours = getAuditHours();


            $("[id$=recommendationDetails_table] tr.recommendationItem").each(function () {
                $this = $(this)
                var typeOfVoilation = $this.find("span.typeOfVoilation").html();
                var injuryClass = $this.find("span.injuryClass").html();

                if (typeOfVoilation != 'undefined' && typeOfVoilation == "Unsafe Act") {
                    noOfUnsafeAct = noOfUnsafeAct + 1;
                }

                if (typeOfVoilation != 'undefined' && typeOfVoilation == "Unsafe Condition") {
                    noOfUnsafeCondition = noOfUnsafeCondition + 1;
                }

                if (injuryClass != 'undefined' && injuryClass == "Serious Injury") {
                    noOfSeriousInjury = noOfSeriousInjury + 1;
                }

                if (injuryClass != 'undefined' && injuryClass == "Fatality") {
                    noOfFatalityPotentials = noOfFatalityPotentials + 1;
                }
            }); 
            
            $("[id$=noOfUnsafeConditions_tf]").text((parseFloat(noOfUnsafeCondition)));
            $("[id$=noOfSeriousInjury_tf]").text((parseFloat(noOfSeriousInjury)));
            $("[id$=noOfFatalityInjury_tf]").text((parseFloat(noOfFatalityPotentials)));
            $("[id$=noOfUnsafeActs_tf]").text((parseFloat(noOfUnsafeAct)));

        }
        catch (ex)
        { }
    }

    function SaveMSADetails(isSavedAsDraft) {
        try {
            updateInjuryAndVoilationCount();
        }
        catch (ex)
        { }
        var errorFlag = false;
        var controlList = '';

        var message = '';

        if (isSavedAsDraft == false) {
            //MSA Details
            if ($("[id$=msaDate_tf]").val() == "") {
                errorFlag = true;
                var controlName = "MSA Date";
                controlList += controlName + ": ";
            }
            if ($('[id$=areaAudited_ddl] option:selected').val() == "0") {
                errorFlag = true;
                var controlName = "Area Audited";
                controlList += controlName + ": ";
            }

            if (errorFlag == true) {
                message += "**** Please Provide value for the required fields ****";
            }

        }

        if (errorFlag == false) {
            var action = "";

            if (isSavedAsDraft == true) {
                action = "SaveAsDraft";
            }
            else {
                action = "Save";
            }

            if (!isActionConfirmed(action)) {
                return false;
            }

            var contactList = '';
            $("[id$=contactDetails_table] tr.contactItem").each(function () {
                $this = $(this)
                var contactId = $this.find("span.contactId").html();
                var contactDetail = $this.find("span.contactDetail").html();

                contactList = contactList + contactId + "*|*" + contactDetail + "~|~";
            });
            $("[id$=hdnContactList]").val(contactList);

            var recommendationList = '';
            $("[id$=recommendationDetails_table] tr.recommendationItem").each(function () {
                $this = $(this)
                var recommendationId = $this.find("span.recommendationId").html();
                var recommendationNo = $this.find("span.recommendationNo").html();
                var description = $this.find("span.description").html();
                var typeOfVoilation = $this.find("span.typeOfVoilation").html();
                var username = $this.find("span.username").html();
                var email = $this.find("span.email").html();
                var sectionId = $this.find("span.sectionId").html();
                var sectionName = $this.find("span.sectionName").html();
                var injuryClass = $this.find("span.injuryClass").html();
                var category = $this.find("span.category").html();
                var subCategory = $this.find("span.subCategory").html();
                var consentTaken = $this.find("span.consentTaken").html();
                var departmentId = $this.find("span.departmentId").html();
                var departmentName = $this.find("span.departmentName").html();
                var targetDate = $this.find("span.targetDate").html();
                var observationSpot = $this.find("span.observationSpot").html();
                var status = $this.find("span.status").html();

                if (recommendationId == 'undefined') {
                    recommendationId = 0;
                }
                if (recommendationNo == 'undefined') {
                    recommendationNo = "";
                }
                if (description == 'undefined') {
                    description = "";
                }
                if (typeOfVoilation == 'undefined') {
                    typeOfVoilation = "";
                }
                if (username == 'undefined') {
                    username = "";
                }
                if (email == 'undefined') {
                    email = "";
                }
                if (sectionId == 'undefined') {
                    sectionId = '0';
                }
                if (sectionName == 'undefined') {
                    sectionName = "";
                }
                if (injuryClass == 'undefined') {
                    injuryClass = "";
                }
                if (category == 'undefined') {
                    category = "";
                }
                if (subCategory == 'undefined') {
                    subCategory = "";
                }
                if (consentTaken == 'undefined') {
                    consentTaken = "no";
                }
                if (departmentId == 'undefined') {
                    departmentId = '0';
                }
                if (departmentName == 'undefined') {
                    departmentName = "";
                }
                if (targetDate == 'undefined') {
                    targetDate = "";
                }
                if (observationSpot == 'undefined') {
                    observationSpot = "no";
                }
                if (status == 'undefined') {
                    status = "";
                }

                recommendationList = recommendationList +
                    recommendationId + "*|*" +
                    description + "*|*" +
                    typeOfVoilation + "*|*" +
                    username + "*|*" +
                    email + "*|*" +
                    sectionId + "*|*" +
                    sectionName + "*|*" +
                    injuryClass + "*|*" +
                    category + "*|*" +
                    subCategory + "*|*" +
                    departmentId + "*|*" +
                    departmentName + "*|*" +
                    targetDate + "*|*" +
                    observationSpot + "*|*" +
                    consentTaken + "*|*" +
                    status + "*|*" +
                    recommendationNo + "*|*" +
                    isSavedAsDraft + "~|~";
            });

            $("[id$=hdnRecommendationList]").val(recommendationList);

            var positivePointList = '';
            $("[id$=positivePoint_table] tr.positivePointItem").each(function () {
                $this = $(this)
                var positivePoint = $this.find("span.positivePointDescription").html();

                positivePointList = positivePointList + positivePoint + "~|~";
            });

            $("[id$=hdnPositivePointList]").val(positivePointList);

            var areaOfImprovementList = '';
            $("[id$=areaOfImprovement_table] tr.areaOfImprovementItem").each(function () {
                $this = $(this)
                var areaOfImprovement = $this.find("span.areaOfImprovementDescription").html();

                areaOfImprovementList = areaOfImprovementList + areaOfImprovement + "~|~";
            });

            $("[id$=hdnAreaOfImprovementList]").val(areaOfImprovementList);


            var counts = '';
            var delimeter = '~';

            var noOfUnsafeConditions = $("[id$=noOfUnsafeConditions_tf]").text();
            var noOfSeriousInjury = $("[id$=noOfSeriousInjury_tf]").text();
            var noOfFatalityInjury = $("[id$=noOfFatalityInjury_tf]").text();
            var noOfUnsafeActs = $("[id$=noOfUnsafeActs_tf]").text();


            if (noOfUnsafeConditions != 'undefined' && noOfUnsafeConditions != "") {
                counts += noOfUnsafeConditions + delimeter;
            }
            else {
                counts += '0' + delimeter;
            }

            if (noOfSeriousInjury != 'undefined' && noOfSeriousInjury != "") {
                counts += noOfSeriousInjury + delimeter;
            }
            else {
                counts += '0' + delimeter;
            }

            if (noOfFatalityInjury != 'undefined' && noOfFatalityInjury != "") {
                counts += noOfFatalityInjury + delimeter;
            }
            else {
                counts += '0' + delimeter;
            }

            if (noOfUnsafeActs != 'undefined' && noOfUnsafeActs != "") {
                counts += noOfUnsafeActs + delimeter;
            }
            else {
                counts += '0' + delimeter;
            }

            $("[id$=hdnCounts]").val(counts);

            return true;
        }
        else {
            ValidationSummary(message, controlList);
            return false;
        }
    }


</script>

<link href="/_layouts/15/SL.FG.FFL/CSS/FGStyle.css" rel="stylesheet" />

<style type="text/css">
    .editRecommendation {
        display: none !important;
    }

    .editContact {
        display: none !important;
    }

    .editAreaOfImprovement {
        display: none !important;
    }

    .editPositivePoint {
        display: none !important;
    }

    [id$=responsiblePerson_PeopleEditor_TopSpan] {
        border-radius: 5px !important;
        width: 100%;
    }

    [id$=auditedBy_PeopleEditor_upLevelDiv] {
        border-radius: 5px !important;
        width: 100%;
    }

    .panel-title:hover {
        cursor: pointer;
    }

    .btnAdd {
        border-radius: 5px !important;
        background-color: #808080 !important;
        color: white !important;
        margin: 0px !important;
    }
</style>

<div class="container containerMaxWidth">
    <div class="row">
        <div class="col-lg-12">
            <div id="message_div" runat="server" class="messageDiv">
            </div>
            <div class="panel panel-success">
                <div class="panel-heading">
                    <h5>MSA</h5>
                </div>
                <div class="panel-body">
                    <div class="form-group row">
                        <div class="col-lg-6">
                            <label>MSA Date <span style="color: red">&nbsp;*</span></label>
                            <div class="form-group">
                                <SharePoint:DateTimeControl ID="msaDate_dtc" runat="server" DateOnly="true" CssClassTextBox="form-control" AutoPostBack="false" IsRequiredField="true" UseTimeZoneAdjustment="false" LocaleId="2057"  />
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="form-group">
                                <label>Accompanied By</label>
                                <input type='text' class="form-control" id="accompaniedBy_tf" runat="server" maxlength="30" title="Max Length: 30" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-6 table-responsive">
                            <label>Audited By<span style="color: red">&nbsp;*</span></label>
                            <SharePoint:PeopleEditor runat="server" ID="auditedBy_PeopleEditor" AllowEmpty="false" SelectionSet="User"
                                Rows="1" MultiSelect="false" AllowTypeIn="false" ShowButtons="false" />
                        </div>
                        <div class="col-lg-6">
                            <div class="form-group">
                                <label>Designation</label>
                                <input type='text' class="form-control" id="designation_tf" runat="server" maxlength="30" title="Max Length: 30" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-6">
                            <div class="form-group">
                                <label>Area Audited<span style="color: red">&nbsp;*</span></label>
                                <asp:DropDownList ID="areaAudited_ddl" runat="server" CssClass="form-control" AutoPostBack="false" />
                            </div>
                        </div>
                        <div class="col-lg-3">
                            <label>Start Time<span style="color: red">&nbsp;*</span></label>
                            <div class="form-group">
                                <SharePoint:DateTimeControl ID="startTime_dtc" runat="server" TimeOnly="true" CssClassTextBox="form-control" AutoPostBack="false" IsRequiredField="true" />
                            </div>
                        </div>
                        <div class="col-lg-3">
                            <label>End Time<span style="color: red">&nbsp;*</span></label>
                            <div class="form-group">
                                <SharePoint:DateTimeControl ID="endTime_dtc" runat="server" TimeOnly="true" CssClassTextBox="form-control" AutoPostBack="false" IsRequiredField="true" />
                            </div>
                        </div>
                    </div>

                    <div class="form-group row">
                        <div class="col-lg-12">
                            <div class="panel panel-success">
                                <div class="panel-heading">
                                    <h5>Positive Points</h5>
                                </div>
                                <div class="panel-body">
                                    <div class="form-group">
                                        <div class="form-group">
                                            <div class='input-group'>
                                                <input type='text' class="form-control" id="positivePoint_tf" runat="server" placeholder="Please Enter..." />
                                                <span id="addPositivePoint_span" class="input-group-addon">&nbsp;Add
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="form-group table-responsive" style="overflow-x: scroll;">
                                        <table id="positivePoint_table" class="table" runat="server">
                                            <tr>
                                                <th>No</th>
                                                <th>Description</th>
                                                <th>Actions</th>
                                            </tr>
                                        </table>
                                        Total: <span id="noOfPositivePoint_span" runat="server"></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-12">
                            <div class="panel panel-success">
                                <div class="panel-heading">
                                    <h5>Areas of Improvement</h5>
                                </div>
                                <div class="panel-body">
                                    <div class="form-group">
                                        <div class="form-group">
                                            <div class='input-group'>
                                                <input type='text' class="form-control" id="areaOfImprovement_tf" runat="server" placeholder="Please Enter..." />
                                                <span id="addAreaOfImprovement_span" class="input-group-addon">&nbsp;Add
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="form-group table-responsive" style="overflow-x: scroll;">
                                        <table id="areaOfImprovement_table" class="table" runat="server">
                                            <tr>
                                                <th>No</th>
                                                <th>Description</th>
                                                <th>Actions</th>
                                            </tr>
                                        </table>
                                        Total: <span id="noOfAreaOfImprovement_span" runat="server"></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-12">
                            <div class="panel panel-success">
                                <div class="panel-heading">
                                    <h5>Detail of Contacts</h5>
                                </div>
                                <div class="panel-body">
                                    <div class="form-group">
                                        <div class="form-group">
                                            <div class='input-group'>
                                                <input type='text' class="form-control" id="contactDetail_tf" runat="server" placeholder="Please Enter Contact Detail" />
                                                <span id="addContactDetail_span" class="input-group-addon">&nbsp;Add
                                                </span>
                                                <input id="contactId_hd" type="hidden" value="0" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="form-group table-responsive" style="overflow-x: scroll;">
                                        <table id="contactDetails_table" class="table" runat="server">
                                            <tr>
                                                <th>No</th>
                                                <th>Details</th>
                                                <th>Actions</th>
                                            </tr>
                                        </table>
                                        No. of Safety Contacts made <span id="noOfSafetyContactsMade_span" runat="server"></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-12">
                            <div class="panel panel-success">
                                <div class="panel-heading">
                                    <div class="row">
                                        <div class="col-lg-9">
                                            <h5>Recommendation Entry</h5>
                                        </div>
                                        <div class="col-lg-3">
                                            <span class="panel-title pull-right"
                                                data-toggle="collapse"
                                                data-target="#collapseOne">
                                                <i class='glyphicon glyphicon-sort'></i>
                                            </span>
                                        </div>
                                    </div>
                                </div>
                                <div id="collapseOne" class="panel-collapse collapse">
                                    <div class="panel-body">
                                        <div class="form-group row">
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Type of Violation<span style="color: red">&nbsp;*</span></label>
                                                    <select class="form-control" id="typeOfVoilation_ddl">
                                                        <option value="0">Please Select</option>
                                                        <option value="Safety Rule Violation">Safety Rule Violation</option>
                                                        <option value="Unsafe Act">Unsafe Act</option>
                                                        <option value="Unsafe Condition">Unsafe Condition</option>
                                                    </select>
                                                </div>
                                            </div>
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Injury Classification<span style="color: red">&nbsp;*</span></label>
                                                    <select class="form-control" id="injuryClassification_ddl">
                                                        <option value="0">Please Select</option>
                                                        <option value="Fatality">Fatality</option>
                                                        <option value="Serious Injury">Serious Injury</option>
                                                        <option value="Minor Injury">Minor Injury</option>
                                                        <option value="None">None</option>
                                                    </select>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group row">
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Observation Category<span style="color: red">&nbsp;*</span></label>
                                                    <select class="form-control" id="observationCategoryA_ddl">
                                                        <option value="0">Please Select</option>
                                                        <option value="PPEs">PPEs</option>
                                                        <option value="Positions of people">Positions of people</option>
                                                        <option value="Reactions of people">Reactions of people</option>
                                                        <option value="Tools/Equipment">Tools/Equipment</option>
                                                        <option value="Procedures">Procedures</option>
                                                        <option value="Housekeeping">Housekeeping</option>
                                                        <option value="OHIH">OHIH</option>
                                                        <option value="Environment">Environment</option>
                                                    </select>
                                                </div>
                                            </div>
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Observation Sub-Category<span style="color: red">&nbsp;*</span></label>
                                                    <select class="form-control" id="observationCategoryB_ddl">
                                                    </select>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label>Description<span style="color: red">&nbsp;*</span></label>
                                            <textarea class="form-control" id="description_ta" rows="5"></textarea>
                                        </div>
                                        <div class="form-group row">
                                            <div class="col-lg-6 table-responsive">
                                                <label>Responsible Person<span style="color: red">&nbsp;*</span></label>
                                                <SharePoint:ClientPeoplePicker runat="server" ID="responsiblePerson_PeopleEditor" Rows="1" VisibleSuggestions="3" AllowMultipleEntities="false" PrincipalAccountType="User"  />
                                                <input id="responsiblePersonUsername_hd" type="hidden" value="" />
                                                <input id="recommendationId_hd" type="hidden" value="0" />
                                            </div>
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Responsible Department<span style="color: red">&nbsp;*</span></label>
                                                    <select name="responsibleDepartment_ddl" class="form-control" id="responsibleDepartment_ddl" runat="server">
                                                    </select>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group row">
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Responsible Section<span style="color: red">&nbsp;*</span></label>
                                                    <select name="responsibleSection_ddl" class="form-control" id="responsibleSection_ddl" runat="server">
                                                    </select>
                                                </div>
                                            </div>
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Responsible Person Email</label>
                                                    <input id="responsiblePersonEmail_tf" type='text' class="form-control disabled" disabled />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group row">
                                            <div class="col-lg-6">
                                                <label>Consent Taken from Responsible Person</label>
                                                <div class="form-group">
                                                    <div class="form-inline col-lg-6">
                                                        <label>Yes</label>
                                                        <input type="radio" name="consentTaken" id="consentTakenYes_rb" value="Yes">
                                                    </div>
                                                    <div class="form-inline col-lg-6">
                                                        <label>No</label>
                                                        <input type="radio" name="consentTaken" id="consentTakenNo_rb" value="No" checked>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-lg-6">
                                                <label>On Spot Closure</label>
                                                <div class="form-group">
                                                    <div class="form-inline col-lg-6">
                                                        <label>Yes</label>
                                                        <input type="radio" name="observationSpot" id="observationSpotYes_rb" value="Yes">
                                                    </div>
                                                    <div class="form-inline col-lg-6">
                                                        <label>No</label>
                                                        <input type="radio" name="observationSpot" id="observationSpotNo_rb" value="No" checked>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group row">
                                            <div class="col-lg-6">
                                                <label>Status</label>
                                                <select class="form-control" id="status_ddl" disabled>
                                                    <option>Pending</option>
                                                    <option>In Progress</option>
                                                    <option>Completed</option>
                                                </select>
                                            </div>
                                            <div class="col-lg-6">
                                                <div class="form-group">
                                                    <label>Recommendation Number</label>
                                                    <input type='text' class="form-control" id="recommendationNo_tf" disabled />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="form-group row">
                                            <div class="col-lg-6">
                                                <label>Target Date<span style="color: red">&nbsp;*</span></label>
                                                <div class="form-group">
                                                    <SharePoint:DateTimeControl ID="targetDate_dtc" runat="server" DateOnly="true" CssClassTextBox="form-control" AutoPostBack="false" UseTimeZoneAdjustment="false" LocaleId="2057"  />
                                                </div>
                                            </div>
                                            <div class="col-lg-6">
                                                <label>&nbsp</label>
                                                <div class="form-group">
                                                    <input type="button" value="Add" class="btnAdd pull-right" id="addRecommendation_btn" />
                                                </div>
                                            </div>
                                        </div>
                                        <div class="panel panel-success">
                                            <div class="panel-body">
                                                <div class="form-group table-responsive" style="overflow-x: scroll;">
                                                    <table class="table" id="recommendationDetails_table" runat="server">
                                                        <tr>
                                                            <th>No</th>
                                                            <th>Description</th>
                                                            <th>Type of Violation</th>
                                                            <th>Responsible Person</th>
                                                            <th>Responsible Section</th>
                                                            <th>Responsible Department</th>
                                                            <th>Injury Classification</th>
                                                            <th>Consent Taken</th>
                                                            <th>Target Date</th>
                                                            <th>Observation Category</th>
                                                            <th>Subcategory</th>
                                                            <th>On Spot Closure</th>
                                                            <th>Status</th>
                                                            <th>Actions</th>
                                                        </tr>
                                                    </table>
                                                    No. of Recommendations <span id="noOfRecommendations_span" runat="server"></span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-6">
                            <div class="col-lg-6">
                                <label>No of Unsafe acts Identified</label>
                            </div>
                            <div class="col-lg-6">
                                <span id="noOfUnsafeActs_tf" runat="server">0</span>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="col-lg-6">
                                <label>No of Unsafe condition Identified</label>
                            </div>
                            <div class="col-lg-6">
                                <span id="noOfUnsafeConditions_tf" runat="server">0</span>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-6">
                            <div class="col-lg-6">
                                <label>No of Serious Injury Potential observation</label>
                            </div>
                            <div class="col-lg-6">
                                <span id="noOfSeriousInjury_tf" runat="server">0</span>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="col-lg-6">
                                <label>No of Fatality Injury Potential observation</label>
                            </div>
                            <div class="col-lg-6">
                                <span id="noOfFatalityInjury_tf" runat="server">0</span>
                            </div>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-lg-6">
                            <div class="form-group">
                                <label>Attachment</label>
                                <div>
                                    <table id="grdAttachments" runat="server">
                                    </table>
                                </div>
                                <asp:FileUpload ID="fileUploadControl" runat="server" AllowMultiple="true" />
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="form-group" id="msaQualityScore_div" runat="server" visible="false">
                                <div class="col-lg-6">
                                    <label>MSA Quality Score</label>
                                </div>
                                <div class="col-lg-6">
                                    <span id="msaQualityScore_tf" runat="server">0</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <asp:HiddenField ID="hdnMSAId" runat="server" Value="" />
            <asp:HiddenField ID="hdnScheduleId" runat="server" Value="" />
            <asp:HiddenField ID="hdnCounts" runat="server" Value="" />
            <asp:HiddenField ID="hdnIdList" runat="server" Value="" />
            <asp:HiddenField ID="hdnContactList" runat="server" Value="" />
            <asp:HiddenField ID="hdnRecommendationList" runat="server" Value="" />
            <asp:HiddenField ID="hdnPositivePointList" runat="server" Value="" />
            <asp:HiddenField ID="hdnAreaOfImprovementList" runat="server" Value="" />
            <asp:HiddenField ID="hdnFilesNames" runat="server" Value="" />
            <br />
            <br />
            <div class="form-group pull-right">
                <asp:Button ID="btnSaveAsDraft" runat="server" Text="Save As Draft" OnClick="btnSaveAsDraft_Click" OnClientClick="return SaveMSADetails(true);" CssClass="btnSaveAsDraft" />
                <asp:Button ID="btnSave" runat="server" Text="Submit" OnClick="btnSave_Click" OnClientClick="return SaveMSADetails(false);" CssClass="btnSave" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" OnClientClick="return isActionConfirmed();" CssClass="btnCancel" />
            </div>
        </div>
    </div>
</div>

<script src="/_layouts/15/SL.FG.FFL/Scripts/MicrosoftAjax.js" type="text/javascript">
</script>
<script
    type="text/javascript"
    src="/_layouts/15/sp.runtime.js">
</script>
<script
    type="text/javascript"
    src="/_layouts/15/sp.js">
</script>

<script src="/_layouts/15/SL.FG.FFL/Scripts/MSA/MSAForm_JSOM.js"></script>

<script src="/_layouts/15/SL.FG.FFL/Scripts/jQuery.js"></script>

<script src="/_layouts/15/SL.FG.FFL/Scripts/MSA/MSAForm.js"></script>




