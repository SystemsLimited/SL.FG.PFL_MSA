
$(document).ready(function () {

    ////Disable the date controls,
    //$('[id$=msaDate_dtcDate]').attr('disabled', 'disabled');
    //$('[id$=targetDate_dtcDate]').attr('disabled', 'disabled');

    function convertStringToDate(str) {
        try {
            var temp = str.split('/');

            if (temp.length > 2) {
                var d = new Date(temp[2], (parseInt(temp[1]) - 1), temp[0]);
                return d;
            }
        }
        catch (ex) {
        }
        return null;
    }

    function deleteContact() {
        var par = $(this).closest('tr');
        par.remove();

        var count = $("[id$=contactDetails_table] tr.contactItem").length;
        $('[id$=noOfSafetyContactsMade_span]').text(count);
    }

    function deleteRecommendation() {
        var par = $(this).closest('tr');
        par.remove();

        var count = $("[id$=recommendationDetails_table] tr.recommendationItem").length;
        $('[id$=noOfRecommendations_span]').text(count);

        updateRecommendationControls();
    }

    function deletePositivePoint() {
        var par = $(this).closest('tr');
        par.remove();

        var count = $("[id$=positivePoint_table] tr.positivePointItem").length;
        $('[id$=noOfPositivePoint_span]').text(count);
    }

    function deleteAreaOfImprovement() {
        var par = $(this).closest('tr');
        par.remove();

        var count = $("[id$=areaOfImprovement_table] tr.areaOfImprovementItem").length;
        $('[id$=noOfAreaOfImprovement_span]').text(count);
    }

    function isDuplicateContact(contactDetail) {
        var flag = false;
        $("[id$=contactDetails_table] tr.contactItem").each(function () {
            $this = $(this);

            var temp = $this.find("span.contactDetail").html();
            if (contactDetail.toLowerCase() == temp.toLowerCase()) {
                flag = true;
            }
        });
        return flag;
    }

    function updateRecommendationControls() {
        var html = $("[id$=responsiblePerson_PeopleEditor]").html();
        var emailList = extractEmails(html);

        if (emailList != 'undefined' && emailList != "" && emailList != null) {
            $('[id$=responsiblePersonEmail_tf]').val(emailList[0]);
        }

        var username = $('[id*=responsiblePerson_PeopleEditor_TopSpan_i]').attr('sid');

        if (username != 'undefined' && username != null && username != "") {
            var temp = username.split('|');

            if (temp.length > 1) {
                username = temp[1];
            }
        }

        if (username != 'undefined' && username != "" && username != null) {
            $('[id$=responsiblePersonUsername_hd]').val(username);
        }
    }


    function addContact() {
        var contactDetail = $('[id$=contactDetail_tf]').val();

        if (contactDetail != 'undefined' && contactDetail != "") {

            contactDetail = $.trim(contactDetail);
            var flag = false;

            if (isDuplicateContact(contactDetail)) {
                var answer = confirm("Duplicate Contact: Are you sure you want to add?")
                if (answer) {
                    flag = false;
                }
                else {
                    flag = true;
                }
            }

            if (flag == false) {
                var count = $("[id$=contactDetails_table] tr.contactItem").length + 1;
                var contactId = $('#contactId_hd').val();
                var actions = "<span class='btn btn-default editContact'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeContact'><i class='glyphicon glyphicon-remove'></i></span>";
                var data = "<tr class='contactItem'><td>" + count + "</td><td style='display:none;'><span class='contactId'>" + contactId + "</span></td><td><span class='contactDetail'>" + contactDetail + "</span></td><td>" + actions + "</td></tr>"
                $("[id$=contactDetails_table]").append(data);
                $('[id$=contactDetail_tf]').val("");

                $('[id$=noOfSafetyContactsMade_span]').text(count);
            }
        }
        else {
            alert("Please enter valid Contact Detail");
            $('[id$=contactDetail_tf]').focus();
        }
    }

    function addPositivePoint() {
        var positivePoint = $('[id$=positivePoint_tf]').val();

        if (positivePoint != 'undefined' && positivePoint != "") {

            var count = $("[id$=positivePoint_table] tr.positivePointItem").length + 1;
            var actions = "<span class='btn btn-default editPositivePoint'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removePositivePoint'><i class='glyphicon glyphicon-remove'></i></span>";
            var data = "<tr class='positivePointItem'><td>" + count + "</td><td><span class='positivePointDescription'>" + positivePoint + "</span></td><td>" + actions + "</td></tr>"
            $("[id$=positivePoint_table]").append(data);
            $('[id$=positivePoint_tf]').val("");

            $('[id$=noOfPositivePoint_span]').text(count);

        }
        else {
            alert("Please enter valid data");
            $('[id$=positivePoint_tf]').focus();
        }
    }

    function addAreaOfImprovement() {
        var areaOfImprovement = $('[id$=areaOfImprovement_tf]').val();

        if (areaOfImprovement != 'undefined' && areaOfImprovement != "") {

            var count = $("[id$=areaOfImprovement_table] tr.areaOfImprovementItem").length + 1;
            var actions = "<span class='btn btn-default editAreaOfImprovement'><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeAreaOfImprovement'><i class='glyphicon glyphicon-remove'></i></span>";
            var data = "<tr class='areaOfImprovementItem'><td>" + count + "</td><td><span class='areaOfImprovementDescription'>" + areaOfImprovement + "</span></td><td>" + actions + "</td></tr>"
            $("[id$=areaOfImprovement_table]").append(data);
            $('[id$=areaOfImprovement_tf]').val("");

            $('[id$=noOfAreaOfImprovement_span]').text(count);

        }
        else {
            alert("Please enter valid data");
            $('[id$=areaOfImprovement_tf]').focus();
        }
    }

    $("[id$=recommendationDetails_table]").on("click", ".removeRecommendation", deleteRecommendation);
    $("[id$=contactDetails_table]").on("click", ".removeContact", deleteContact);
    $("[id$=positivePoint_table]").on("click", ".removePositivePoint", deletePositivePoint);
    $("[id$=areaOfImprovement_table]").on("click", ".removeAreaOfImprovement", deleteAreaOfImprovement);

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
    //Add Positive Point in grid
    $('[id$=addPositivePoint_span]').on('click', function () {
        addPositivePoint();
    });
    $('[id$=positivePoint_tf]').on('keypress', function (e) {
        if (e.which == 13) {
            addPositivePoint();
            e.preventDefault();
        }
    });

    //Add Area of Improvements in grid
    $('[id$=addAreaOfImprovement_span]').on('click', function () {
        addAreaOfImprovement();
    });

    $('[id$=areaOfImprovement_tf]').on('keypress', function (e) {
        if (e.which == 13) {
            addAreaOfImprovement();
            e.preventDefault();
        }
    });


    //Add Contact in grid
    $('[id$=addContactDetail_span]').on('click', function () {
        addContact();
    });

    $('[id$=contactDetail_tf]').on('keypress', function (e) {
        if (e.which == 13) {
            addContact();
            e.preventDefault();
        }
    });

    //Get username and email of the selected user
    $('[id$=responsiblePerson_PeopleEditor]').on('focusout', function () {
        updateRecommendationControls();
    });

    //Add Recommendation in Grid
    $('[id$=addRecommendation_btn]').on('click', function () {
        updateRecommendationControls();

        var controlList = '';
        var errorFlag = false;
        var message = '**** Please Provide value for the required fields ****';

        if ($('[id$=typeOfVoilation_ddl] option:selected').val() == "0") {
            errorFlag = true;
            var controlName = "Type of Voilation";
            controlList += controlName + ": ";
        }
        if ($('[id$=injuryClassification_ddl] option:selected').val() == "0") {
            errorFlag = true;
            var controlName = "Injury Classification";
            controlList += controlName + ": ";
        }
        if ($('[id$=observationCategoryB_ddl] option:selected').val() == "0") {
            errorFlag = true;
            var controlName = "Observation Subcategory";
            controlList += controlName + ": ";
        }
        if ($('[id$=observationCategoryA_ddl] option:selected').val() == "0") {
            errorFlag = true;
            var controlName = "Observation Category";
            controlList += controlName + ": ";
        }
        if ($('[id$=responsibleDepartment_ddl] option:selected').val() == "0") {
            errorFlag = true;
            var controlName = "Responsible Department";
            controlList += controlName + ": ";
        }
        if ($('[id$=responsibleSection_ddl] option:selected').val() == "0") {
            errorFlag = true;
            var controlName = "Responsible Section";
            controlList += controlName + ": ";
        }
        if ($("[id$=responsiblePersonUsername_hd]").val() == "") {
            errorFlag = true;
            var controlName = "Responsible Person";
            controlList += controlName + ": ";
        }
        if ($("[id$=targetDate_dtcDate]").val() == "") {
            errorFlag = true;
            var controlName = "Target Date";
            controlList += controlName + ": ";
        }
        if ($("[id$=description_ta]").val() == "") {
            errorFlag = true;
            var controlName = "Description";
            controlList += controlName + ": ";
        }

        if ($("[id$=targetDate_dtcDate]").val() == "") {
            errorFlag = true;
            var controlName = "Target Date";
            controlList += controlName + ": ";
        }

        if (errorFlag == false && $("[id$=targetDate_dtcDate]").val() != "" && $("[id$=msaDate_dtcDate]").val()) {
            try {
                var targetDate = convertStringToDate($("[id$=targetDate_dtcDate]").val());
                var msaDate = convertStringToDate($("[id$=msaDate_dtcDate]").val());

                if (targetDate != null && msaDate != null && targetDate < msaDate) {
                    errorFlag = true;
                    message = 'Target date must be greater than or equal to MSA date.';
                }
            }
            catch (ex) {
                errorFlag = true;
                message += '**** Enter Valid Date ****';
            }
        }

        if (errorFlag == false && $("[id$=targetDate_dtcDate]").val() != "") {
            try {
                var targetDate = convertStringToDate($("[id$=targetDate_dtcDate]").val());
                var currentDate = new Date();

                var observationSpot = "Yes";

                var selected2 = $("input[type='radio'][name='observationSpot']:checked");
                if (selected2.length > 0) {
                    observationSpot = selected2.val();
                }

                if (observationSpot == "Yes" && targetDate != null && targetDate.toDateString() != currentDate.toDateString()) {
                    errorFlag = true;
                    message = 'In case of on spot Closure, Target date must be equal to current date.';
                }
            }
            catch (ex) {
            }
        }

        if (errorFlag == false) {

            var responsiblePersonUsername = $('[id$=responsiblePersonUsername_hd]').val()
            var responsiblePersonEmail = $('[id$=responsiblePersonEmail_tf]').val();
            var typeOfVoilation = $('[id$=typeOfVoilation_ddl] option:selected').val();
            var injuryClassification = $('[id$=injuryClassification_ddl] option:selected').val();
            var observationCategory = $('[id$=observationCategoryA_ddl] option:selected').val();
            var observationSubCategory = $('[id$=observationCategoryB_ddl] option:selected').val();
            var description = $('[id$=description_ta]').val();
            var status = $('[id$=status_ddl] option:selected').val();

            var responsibleDepartment = $('[id$=responsibleDepartment_ddl] option:selected').text();
            var responsibleDepartmentId = $('[id$=responsibleDepartment_ddl] option:selected').val();

            if (injuryClassification != 'undefined' && injuryClassification == "0") {
                injuryClassification = "";
            }

            if (observationCategory != 'undefined' && observationCategory == "0") {
                observationCategory = "";
            }

            if (observationSubCategory != 'undefined' && observationSubCategory == "0") {
                observationSubCategory = "";
            }

            if (typeOfVoilation != 'undefined' && typeOfVoilation == "0") {
                typeOfVoilation = "";
            }

            if (responsibleDepartmentId != 'undefined' && responsibleDepartmentId == "0") {
                responsibleDepartmentId = "0";
                responsibleDepartment = "";
            }

            var responsibleSection = $('[id$=responsibleSection_ddl] option:selected').text();
            var responsibleSectionId = $('[id$=responsibleSection_ddl] option:selected').val();

            if (responsibleSectionId != 'undefined' && responsibleSectionId == "0") {
                responsibleSectionId = "0";
                responsibleSection = "";
            }

            var recommendationId = $('#recommendationId_hd').val();;

            var consentTaken = "Yes";

            var selected1 = $("input[type='radio'][name='consentTaken']:checked");
            if (selected1.length > 0) {
                consentTaken = selected1.val();
            }

            var targetDate = $('[id$=targetDate_dtcDate]').val();
            var recommendationNo = $('[id$=recommendationNo_tf]').val();

            var observationSpot = "Yes";

            var selected2 = $("input[type='radio'][name='observationSpot']:checked");
            if (selected2.length > 0) {
                observationSpot = selected2.val();
            }

            //add recommendation in grid
            if (true) {
                var count = $("[id$=recommendationDetails_table] tr.recommendationItem").length + 1;
                var actions = "<span class='btn btn-default editRecommendation' ><i class='glyphicon glyphicon-pencil'></i></span><span class='btn btn-danger removeRecommendation'><i class='glyphicon glyphicon-remove'></i></span>";
                var data = "<tr class='recommendationItem'><td>" + count + "</td><td style='display:none;'><span class='recommendationId'>" + recommendationId + "</span></td><td class='td-description'><span class='description'>" + description + "</span></td><td><span class='typeOfVoilation'>" + typeOfVoilation + "</span></td><td><span class='username'>" + responsiblePersonUsername + "</span></td><td style='display:none;'><span class='email'>" + responsiblePersonEmail + "</td><td><span class='sectionName'>" + responsibleSection + "</span></td><td style='display:none'><span class='sectionId'>" + responsibleSectionId + "</span></td><td><span class='departmentName'>" + responsibleDepartment + "</span></td><td><span class='injuryClass'>" + injuryClassification + "</span></td><td style='display:none'><span class='departmentId'>" + responsibleDepartmentId + "</span></td><td><span class='consentTaken'>" + consentTaken + "</span></td><td><span class='targetDate'>" + targetDate + "</span></td><td><span class='category'>" + observationCategory + "</span></td><td><span class='subCategory'>" + observationSubCategory + "</span></td><td><span class='observationSpot'>" + observationSpot + "</span></td><td><span class='status'>" + status + "</span></td><td style='display:none;'><span class='recommendationNo'>" + "" + "</span></td><td>" + actions + "</td></tr>";
                $("[id$=recommendationDetails_table] tbody").append(data);

                $('[id$=recommendationNo_tf]').val("");

                $('[id$=noOfRecommendations_span]').text(count);
            }
            else {
                alert("Please enter valid Recommendation");
            }

            updateInjuryAndVoilationCount();

            $('[id$=typeOfVoilation_ddl]').val("0");
            $('[id$=injuryClassification_ddl]').val("0");
            $('[id$=observationCategoryA_ddl]').val("0");
            $('[id$=observationCategoryB_ddl]').val("0");
            $('[id$=description_ta]').val("");
            $('[id$=responsibleDepartment_ddl]').val("0");
            $('[id$=responsibleSection_ddl]').val("0");
            //clear client people picker
            $('.sp-peoplepicker-delImage[id$=_DeleteUserLink]').trigger('click');
            $('[id$=responsiblePersonEmail_tf]').val("");
            $('[id$=responsiblePersonUsername_hd]').val("");


            $('[id$=observationSpotNo_rb]').prop("checked", "checked");
            $('[id$=consentTakenNo_rb]').prop("checked", "checked");

            var currentDateTime = new Date();
            var currentDate = currentDateTime.format("dd/MM/yyyy");

            $('.panel-collapse').collapse('hide');
            $('.panel-title').attr('data-toggle', 'collapse');

            if (currentDate != null && currentDate != "") {
                $('[id$=targetDate_dtcDate]').val(currentDate);
            }

            alert('Recommendation added successfully!');
        }
        else
            ValidationSummary(message, controlList);
    });

    //Change Status in case of observation spot closure (yes)
    $("input[type='radio'][name='observationSpot']").on('change', function () {
        var selected2 = $("input[type='radio'][name='observationSpot']:checked");
        if (selected2.length > 0) {
            observationSpot = selected2.val();
            if (observationSpot == "Yes") {
                $("#status_ddl").get(0).selectedIndex = 2; //completed
            }
            else {
                $("#status_ddl").get(0).selectedIndex = 0; //pending
            }
        }
    });

    //Populate Observations B on the basis of Observation A
    $('[id$=observationCategoryA_ddl]').on('change', function () {

        var observationCategoryA = $('[id$=observationCategoryA_ddl] option:selected').text();

        if (observationCategoryA != 'undefined' && observationCategoryA != "" && $('[id$=observationCategoryA_ddl] option:selected').val() != "0") {

            var targetListName = "CommonDictionary";
            clientContext = new SP.ClientContext();
            var targetList = clientContext.get_web().get_lists().getByTitle(targetListName);

            var query = "<View>\
                            <Query>\
                               <Where>\
                                  <Eq>\
                                     <FieldRef Name='Title' />\
                                     <Value Type='Text'>" + observationCategoryA + "</Value>\
                                  </Eq>\
                               </Where>\
                               <OrderBy>\
                                    <FieldRef Name='SortOrder' Ascending='TRUE'/>\
                               </OrderBy>\
                            </Query>\
                        </View>";

            var camlQuery = new SP.CamlQuery();
            camlQuery.set_viewXml(query);

            targetListItems = targetList.getItems(camlQuery);

            clientContext.load(targetListItems, 'Include(Title, Value)');

            clientContext.executeQueryAsync(
                Function.createDelegate(this,
                function () {
                    _returnParam = success_ObservationB(targetListItems);
                }),
                Function.createDelegate(this, this.failed));
        }
        else {
            $('[id$=observationCategoryB_ddl]').html("");
        }
    });

    $('.panel-collapse').collapse('hide');
    $('.panel-title').attr('data-toggle', 'collapse');

    var count = $("[id$=contactDetails_table] tr.contactItem").length;
    $('[id$=noOfSafetyContactsMade_span]').text(count);

    count = $("[id$=recommendationDetails_table] tr.recommendationItem").length;
    $('[id$=noOfRecommendations_span]').text(count);

    count = $("[id$=positivePoint_table] tr.positivePointItem").length;
    $('[id$=noOfPositivePoint_span]').text(count);

    count = $("[id$=areaOfImprovement_table] tr.areaOfImprovementItem").length;
    $('[id$=noOfAreaOfImprovement_span]').text(count);

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
});