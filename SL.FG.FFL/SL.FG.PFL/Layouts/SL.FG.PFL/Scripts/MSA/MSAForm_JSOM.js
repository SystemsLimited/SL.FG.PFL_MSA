function init() {

    var clientContext1 = new SP.ClientContext();
    var targetListName1 = "Department";
    var targetList1 = clientContext1.get_web().get_lists().getByTitle(targetListName1);

    var query = "<View>\
                            <Query>\
                               <Where>\
                                  <Eq>\
                                     <FieldRef Name='DepartmentDescription' />\
                                     <Value Type='Text'>HOD</Value>\
                                  </Eq>\
                               </Where>\
                            </Query>\
                        </View>";

    var camlQuery = new SP.CamlQuery();
    camlQuery.set_viewXml(query);

    var targetListItems1 = targetList1.getItems(camlQuery);
    clientContext1.load(targetListItems1, 'Include(ID,Title)');

    clientContext1.executeQueryAsync(
        Function.createDelegate(this, function () { success_Department(targetListItems1); }),
        Function.createDelegate(this, this.failed));


    var clientContext2 = new SP.ClientContext();
    var targetListName2 = "Section";
    var targetList2 = clientContext2.get_web().get_lists().getByTitle(targetListName2);

    var targetListItems2 = targetList2.getItems('');

    clientContext2.load(targetListItems2, 'Include(ID,Title)');

    clientContext2.executeQueryAsync(
        Function.createDelegate(this, function () { success_Section(targetListItems2); }),
        Function.createDelegate(this, this.failed));
}



function success_Department(targetListItems) {

    var listItems = "<option value='0'>Please Select</option>";

    var listItemEnumerator = targetListItems.getEnumerator();

    var oListItem = listItemEnumerator.get_current();

    while (listItemEnumerator.moveNext()) {
        var oListItem = listItemEnumerator.get_current();
        var ID = oListItem.get_item('ID');
        var Title = oListItem.get_item('Title')
        listItems += "<option value=" + ID + ">" + Title + "</option>";
    }

    $('[id$=responsibleDepartment_ddl]').html(listItems);
}

function success_Section(targetListItems) {

    var listItems = "<option value='0'>Please Select</option>";

    var listItemEnumerator = targetListItems.getEnumerator();

    var oListItem = listItemEnumerator.get_current();

    while (listItemEnumerator.moveNext()) {
        var oListItem = listItemEnumerator.get_current();
        var ID = oListItem.get_item('ID');
        var Title = oListItem.get_item('Title')
        listItems += "<option value=" + ID + ">" + Title + "</option>";
    }
    $('[id$=responsibleSection_ddl]').html(listItems);

}

function success_ObservationB(targetListItems) {

    var listItems = "<option value='0'>Please Select</option>";

    var listItemEnumerator = targetListItems.getEnumerator();

    var oListItem = listItemEnumerator.get_current();

    while (listItemEnumerator.moveNext()) {
        var oListItem = listItemEnumerator.get_current();
        var ID = oListItem.get_item('Value');
        var Title = oListItem.get_item('Value');
        listItems += "<option value=" + ID + ">" + Title + "</option>";
    }

    $('[id$=observationCategoryB_ddl]').html(listItems);
}

function failed(sender, args) {
    //alert('Request failed. \nError: ' + args.get_message() + '\nStackTrace: ' + args.get_stackTrace());
}


init();