
$(document).ready(function () {
    // creating a division which activates on settings icon clicked
    var div = document.createElement("div");
    div.id = "settingsBack";
    document.body.insertBefore(div, document.body.firstChild);
    var isGlobalAdmin = (typeof userType != 'undefined' && userType === "GlobalAdmin");
    var isDistrictAdmin = (typeof userType != 'undefined' && userType === "DistrictAdmin");
    var userName = $('body div.navbar div.pull-right span#username').text();
    var statusTable = $('#statusTable').DataTable({
        "sDom": '<"top"iflp<"clear">>rt<"bottom"iflp<"clear">>',//Creates multiple controls on top and bottom
        "processing": true, // Feature control the processing indicator
        "serverSide": true, // Feature control DataTables' server-side processing mode
        "paging": true,     // Enable table pagination
        "searching": false, // Feature control search (filtering) abilities
        "ordering": false,  // Feature control ordering (sorting) abilities in DataTables
        "pagingType": "full_numbers",   // 'First', 'Previous', 'Next' and 'Last' buttons, plus page numbers
        "lengthMenu": [10, 25, 50, 100],// Change the options in the page length select list
        "ajax": {
            "url": "/Admin/Dashboard",
            "type": "POST",
            "data": function (d) {
                d.DistrictFilter = $('#district-multiselect').val() != null
                                        ? $('#district-multiselect').val()
                                        : [];
                d.SchoolFilter = $('#school-multiselect').val() != null
                                        ? $('#school-multiselect').val()
                                        : [];
                return d;
            },
            "error": function (xhr) {
                //session timeout check
                if (xhr.readyState == 4 && xhr.status == 200) {
                    var responseHeader = xhr.getResponseHeader("X-Responded-JSON");
                    if (responseHeader != null) {
                        var responseStatus = JSON.parse(responseHeader);
                        if (responseStatus != null && responseStatus.status != null && responseStatus.status == 401) {
                            location.href = responseStatus.headers.location;
                        }
                    }
                }
            }
        },
        "columns": [
                        { "data": "Created" },                                  //0
                        { "data": "DistrictName" },                             //1
                        { "data": "SchoolName" },                               //2
                        { "data": "DeviceId", "sClass": "device" },             //3
                        { "data": "DeviceName" },                               //4
                        { "data": "DeviceType" },                               //5
                        { "data": "DeviceOSVersion" },                          //6
                        { "data": "Username", "sClass": "uname" },              //7
                        { "data": "UserType", "sClass": "utype" },              //8
                        { "data": "ConfiguredGrade" },                          //9
                        { "data": "LocationName" },                             //10
                        { "data": "WifiBSSID" },                                //11
                        { "data": "WifiSSID" },                                 //12
                        { "data": "DownloadRequestType", "sClass": "reqType" }, //13
                        { "data": "DownloadRequestCount" },                     //14
                        { "data": "CanRevoke" }                                 //15
        ],
        "columnDefs": [
                        {
                            //Format the date for Date Created
                            "targets": [0],
                            "data": null,
                            "render": function (data) { return renderUtcDateAsLocalDate(data); }
                        },
                        {
                            //Hide District Name if it's district/school admin
                            "targets": [1],
                            "visible": isGlobalAdmin,
                        },
                        {
                            //Hide School Name Column if it's school admin
                            "targets": [2],
                            "visible": isGlobalAdmin || isDistrictAdmin,
                        },
                        {
                            //Revoke Button based on CanRevoke
                            "targets": [15],
                            "data": null,
                            "render": function (data) { return renderLicenseAction(data); }
                        }
        ]
    });

    $('#district-multiselect').multiselect({
        enableCaseInsensitiveFiltering: true,
        maxHeight: 400,
        filterPlaceholder: "search district",
        onChange: function() {
            $.post('/Admin/GetSchoolsByDistricts', //Url
                    {
                        value: {
                            District: $('#district-multiselect').val() != null 
                                        ? $('#district-multiselect').val()
                                        : [],
                            School: $('#school-multiselect').val() != null
                                        ? $('#school-multiselect').val()
                                        : []
                        }
                    },                          //Data
                    function (response) {
                        $('#school-multiselect').empty();
                        $.each(response, function (index, element) {
                            $('#school-multiselect').append("<option value=" + element.SchoolId + ">" + element.SchoolName + "</option>");
                            if (element.Checked) {
                                $('#school-multiselect').multiselect('select', element.SchoolId);
                            }
                        });
                        $('#school-multiselect').multiselect('rebuild');
                        statusTable.ajax.reload();
                    }                           //Callback
            );
        },

      /*  buttonText: function (options) {
            if (options.length === 0) {
                return "Filter District";
            } else {
                return "FILTER APPLIED (" + options.length + ")";
            }*/

        buttonText: function(options) {
            return RenderButtonTest(options, "District &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
        },
        onDropdownHide: function () {
            $("#DistrictDropdown .multiselect-search").val('');
            $("#DistrictDropdown .multiselect-container > li").removeClass('filter-hidden');
            $("#DistrictDropdown .multiselect-container > li").css("display", "list-item");
        }
    });

    $('#school-multiselect').multiselect({
        enableCaseInsensitiveFiltering: true,
        maxHeight: 400,
        filterPlaceholder: "search school",
        onChange: function () {
            statusTable.ajax.reload();
        },
/*
        buttonText: function (options) {
            if (options.length === 0) {
                return "Filter School";
            } else {
                return "FILTER APPLIED (" + options.length + ")";
            }
*/
        buttonText: function(options) {
            return RenderButtonTest(options, "School &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
        },
        onDropdownHide: function () {
            $("#SchoolDropdown .multiselect-search").val('');
            $("#SchoolDropdown .multiselect-container > li").removeClass('filter-hidden');
            $("#SchoolDropdown .multiselect-container > li").css("display", "list-item");
        }
    });

    $('#accesspoint-multiselect').multiselect({
        enableCaseInsensitiveFiltering: true,
        maxHeight: 400,
        filterPlaceholder: "search access point",
        //onChange: function () {
        //    statusTable.ajax.reload();
        //},
        buttonText: function (options) {
            return RenderButtonTest(options, "Access Point");
        },
        onDropdownHide: function () {
            $("#AccessPointDropdown .multiselect-search").val('');
            $("#AccessPointDropdown .multiselect-container > li").removeClass('filter-hidden');
            $("#AccessPointDropdown .multiselect-container > li").css("display", "list-item");
        }
    });

    /* SELECTING THE BTN SETTINGS */
    $("#btn_id_settings").click(function () {
        $('#settingsBack').css("display","block");
        $('#btn_id_settings').css("background-image", "url('../../Content/images/Settings_Button_Active.png')");

    });
    $("#settingsBack").click(function () {
        $('#settingsBack').css("display", "none");
        $('#div_id_popupSettings').css("display", "none");
        $('#div_id_access_popupSettings').css("display", "none");
        $('#btn_id_settings').css("background-image", "url('../../Content/images/Settings_Button_Normal.png')");
    });


    $("#statusTable tbody").on("click", "button.licenseAction", function () {
        var revokeButton = $(this);
        var deviceId = revokeButton.closest('tr').children('td.device').text();
        var requestPayload = { "deviceId": deviceId, "RequestType": "3" };

        $("#expire-dialog").dialog({
            title: "Confirmation",
            buttons: {
                Yes: function () {
                    $.ajax({
                        type: "PUT",
                        contentType: "application/json; charset=utf-8",
                        url: "/api/v1/devices/" + deviceId,
                        data: JSON.stringify(requestPayload),
                        dataType: "json",
                        success: function (data) {
                            var response = JSON.parse(JSON.stringify(data));
                            if (response != null && !response.CanDownloadLearningContent) {
                                revokeButton.closest('tr').children('td.utype').text(userType);
                                revokeButton.closest('tr').children('td.uname').text(userName);
                                revokeButton.closest('tr').children('td.reqType').text('Revoked License');
                                revokeButton.remove();
                            }
                        },
                        error: function (err) {
                            alert("An error occured. Could not revoke license for the device: " + deviceId);
                        }
                    });

                    $(this).dialog('close');
                },
                No: function () {
                    $(this).dialog('close');
                }
            },
            dialogClass: 'dialog_css',
            width: 400,
            closeOnEscape: false,
            draggable: false,
            resizable: false,
            modal: true
        });

        return (false);
    });

});

function renderLicenseAction(data) {
    var link = '';
    if (data != null && data) {
        link = '<button class="licenseAction">Expire</button>';
    }
    return link;
}

function renderUtcDateAsLocalDate(data) {
    var localDate = '';
    if (data != null && data) {
        var currentDate = new Date();
        var timeZoneOffSet = (currentDate.getTimezoneOffset() / 60) * -1;
        var utcDate = moment(data).utcOffset(timeZoneOffSet);
        if (utcDate != null && utcDate.year() > 1970) {
            localDate = utcDate.format("MM/DD/YYYY hh:mm:ss A");
        }
    }
    return localDate;
}

function TryParseInt(str, defaultValue) {
    var retValue = defaultValue;
    if (str !== null) {
        if (str.length > 0) {
            if (!isNaN(str)) {
                retValue = parseInt(str);
            }
        }
    }
    return retValue;
}

function RenderButtonTest(options, filterName) {
    if (options.length === 0) {
        return "Filter " + filterName;
    } else {
        return "FILTER APPLIED (" + options.length + ")";
    }
}

