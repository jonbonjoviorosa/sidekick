//Custom JS functions for Facility Users

var BASEPATH = "";

//GENERIC MODAL
$(document).ready(function () {
    var ShowModal = $("#ShowGenericModal").val();
    if (ShowModal == 'true') {
        $("#GenericModal").modal('show');
        $("#successIcon").show();
        $("#successBtn").show();
    }

    //duplicate entry
    if (ShowModal == 'false') {
        $("#GenericModal").modal('show');
        $("#errorIcon").show();
        $("#errorBtn").show();
    }
});

$('.toggle-password').on('click', function () {
    $(this).toggleClass('fa-eye fa-eye-slash');
    let input = $($(this).attr('toggle'));
    if (input.attr('type') == 'password') {
        input.attr('type', 'text');
    }
    else {
        input.attr('type', 'password');
    }
});

function limitText(limitField, limitCount, limitNum) {
    if (limitField.value.length > limitNum) {
        limitField.value = limitField.value.substring(0, limitNum);
    } else {
        limitCount.value = limitNum - limitField.value.length;
    }
}

//Generic Variables and Methods to be used on filtering Dates in DataTable
function renderDatePicker() {
    $("#minDate, #maxDate").datepicker({
        minDate: new Date(1999, 10 - 1, 25),
        dateFormat: 'dd/mm/yy',
    });
}

function renderBookingDatePicker() {
    $("#minDateBooking, #maxDateBooking").datepicker({
        minDate: new Date(1999, 10 - 1, 25),
        dateFormat: 'dd/mm/yy',
    });
}

function renderPlayBookingDatePicker() {
    $("#PlayBookingminDate, #PlayBookingmaxDate").datepicker({
        minDate: new Date(1999, 10 - 1, 25),
        dateFormat: 'dd/mm/yy',
    });
}


function renderDatePickerBanner() {
    $("#minDateBanner, #maxDateBanner").datepicker({
        minDate: new Date(1999, 10 - 1, 25),
        dateFormat: 'dd/mm/yy',
    });
}

$('#minDate, #maxDate').on('change', function () {
    let minDate = $('#minDate').val();
    let maxDate = $('#maxDate').val();
    let table = $('#dtMainList').DataTable();
    if (minDate === '' || maxDate === '') {
        $.fn.dataTable.ext.search = [];
    }
    $.fn.dataTable.ext.search = [];
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = transformDateTable(minDate);
            var max = transformDateTable(maxDate);
            var date = transformDateTable(data[5]);

            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        }
    );
    table.draw();
});

$('#minDateBooking, #maxDateBooking').on('change', function () {
    let minDate = $('#minDateBooking').val();
    let maxDate = $('#maxDateBooking').val();
    let table = $('#dtBookingList').DataTable();
    if (minDate === '' || maxDate === '') {
        $.fn.dataTable.ext.search = [];
    }
    $.fn.dataTable.ext.search = [];
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = transformDateTable(minDate);
            var max = transformDateTable(maxDate);
            var date = transformDateTable(data[4]);
            console.log('adfasdf'+date);
            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        }
    );
    table.draw();
});

$('#PlayBookingminDate, #PlayBookingmaxDate').on('change', function () {
    let minDate = $('#PlayBookingminDate').val();
    let maxDate = $('#PlayBookingmaxDate').val();
    let table = $('#dtPlayBookingList').DataTable();
    if (minDate === '' || maxDate === '') {
        $.fn.dataTable.ext.search = [];
    }
    $.fn.dataTable.ext.search = [];
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = transformDateTable(minDate);
            var max = transformDateTable(maxDate);
            var date = transformDateTable(data[5]);
            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        }
    );
    table.draw();
});

$('#minDateBanner, #maxDateBanner').on('change', function () {
    let minDate = $('#minDateBanner').val();
    let maxDate = $('#maxDateBanner').val();
    let table = $('#dtMainList').DataTable();
    if (minDate === '' || maxDate === '') {
        $.fn.dataTable.ext.search = [];
    }
    $.fn.dataTable.ext.search = [];
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = transformDateTable(minDate);
            var max = transformDateTable(maxDate);
            var date = transformDateTable(data[2]);

            if (
                (min === null && max === null) ||
                (min === null && date <= max) ||
                (min <= date && max === null) ||
                (min <= date && date <= max)
            ) {
                return true;
            }
            return false;
        }
    );
    table.draw();
});


function transformDateTable(strDate) {
    if (strDate === '') return null;
    var dateMomentObject = moment(strDate, "DD/MM/YYYY");
    var dateObject = dateMomentObject.toDate()

    return dateObject;
}

//FOR DATATABLES RETAIN CURRENT PAGE
function retainDataTablePages() {
    let SESSION_PRODUCT_PAGES = 'SESSION_PRODUCT_PAGES';
    let SESSION_PRODUCT_PAGE = 'SESSION_PRODUCT_PAGE';
    let SESSION_PRODUCT_PAGE_LENGTH = 'SESSION_PRODUCT_PAGE_LENGTH';

    let pageLength = 10;
    let currentPageLength = localStorage.getItem(SESSION_PRODUCT_PAGE_LENGTH);
    if (currentPageLength) {
        pageLength = +currentPageLength;
    }

    var dtMainList = $('#dtMainList').DataTable({
        "orderCellsTop": true,
        "search": true,
        "info": true,
        "lengthChange": false,
        "columnDefs": [
            { orderable: true, className: 'reorder', targets: 0 },
        ],
        "autoWidth": true,
        "pageLength": pageLength,
        "pagingType": "full_numbers",
        "sPaginationType": "listbox",
        "aaSorting": [],
    });

    $('#dtMainList').on('page.dt', function () {
        var info = dtMainList.page.info();
        localStorage.setItem(SESSION_PRODUCT_PAGES, info.pages);
        localStorage.setItem(SESSION_PRODUCT_PAGE, info.page);
    });

    $('#dtMainList').on('length.dt', function (e, settings, len) {
        localStorage.setItem(SESSION_PRODUCT_PAGE_LENGTH, len);
    });

    let currentPage = localStorage.getItem(SESSION_PRODUCT_PAGE);
    if (currentPage) {
        dtMainList.page(+currentPage).draw('page');
    }

    addExtraButtons();
    $('#dtMainList').on("draw.dt", function (e) {
        addExtraButtons();
    });



    function addExtraButtons() {

        $(".dataTables_paginate .first").after("<li class='paginate_button page-item'><a class='page-link quick_previous'><<</a></li>");
        $(".dataTables_paginate .last").before("<li class='paginate_button page-item'><a class='page-link quick_next'>>></a></li>");
        var currentPage = dtMainList.page.info();
        if (currentPage.pages - 1 == currentPage.page) {
            $(".quick_next").addClass("disabled")
        } else if (currentPage.page == 0) {
            $(".quick_previous").addClass("disabled")
        }

        $(".quick_next").on("click", quickNext);
        $(".quick_previous").on("click", quickPrevious);

        function quickNext(e) {

            var pageToGoTo = (currentPage.page + 2) >= currentPage.pages ? currentPage.pages - 1 : (currentPage.page + 2);
            dtMainList.page(pageToGoTo).draw(false);
        }

        function quickPrevious(e) {

            var pageToGoTo = (currentPage.page - 2) <= 0 ? 0 : (currentPage.page - 2);
            dtMainList.page(pageToGoTo).draw(false);
        }
    }
}

function validateImages(inputId) {
    var input = document.getElementById(inputId);
    var files = input.files;

    //validate extension type
    var ext = files[0].type;
    if (ext.split('/')[0] === 'image') {

        var uploadURL = "/Home/";
        if (inputId.includes("Banner") || inputId.includes("Icon") || inputId.includes("ImageUrl") || inputId.includes("ProfileImage")) {
            uploadURL = BASEPATH + "/Attachment/" + inputId;

            //validate size 150KB /149000
            //if (files[0].size < 499000) { //temp 500KB

            //validate dimension
            var reader = new FileReader();
            reader.readAsDataURL(files[0]);
            reader.onload = function (frEvent) {
                var image = new Image();
                image.src = frEvent.target.result;
                image.onload = function () {

                    var height = this.height;
                    var width = this.width;

                    uploadImages(inputId, uploadURL);

                    //if (width == 188 && height == 355) {
                    //    //upload image
                    //    uploadShopImageFiles(inputId, uploadURL);
                    //}
                    //else {
                    //    $("#GenericModal").modal('show');
                    //    $("#GenericModalTitle").text('Upload Failed! ');
                    //    $("#GenericModalMsg").text('Required dimension: 355px x 188px');
                    //    $("#errorIcon").show();
                    //    $("#errorBtn").show();
                    //}
                }
            }
            //}
            //else {
            //    $("#GenericModal").modal('show');
            //    $("#GenericModalTitle").text('Upload Failed! ');
            //    $("#GenericModalMsg").text('Exceeded the 150KB maximum size.');
            //    $("#errorIcon").show();
            //    $("#errorBtn").show();
            //}
        }
    }
    else {
        $("#GenericModal").modal('show');
        $("#GenericModalTitle").text('Upload Failed! ');
        $("#GenericModalMsg").text('Accepts JPEG/JPG or PNG types only.');
        $("#errorIcon").show();
        $("#errorBtn").show();
    }
}

function removeSlot(e, timingId) {
    let idToRemove = $('#timingIdToRemove').val();
    if (idToRemove == '') {
        idToRemove = timingId
    }
    else {
        idToRemove = idToRemove + ";" + timingId;
    }

    $('#timingIdToRemove').val(idToRemove);
    e.parentElement.parentElement.parentElement.remove();
}

$('#submit-pitch').click(function (e) {
    console.log(e);
    e.preventDefault();
    let isEqual = false;
    let overlappingTimeSlot = false;
    let formData = new FormData();
    formData.FacilityId = $('#FacilityId').val();
    formData.FacilityPitchId = $('#FacilityPitchId').val();
    formData.Name = $('#pitch-name').val();
    formData.SportId = $('#SportId').val();
    formData.MaxPlayers = $('#MaxPlayers').val();
    formData.TeamSize = $('#TeamSize').val();
    formData.SurfaceId = $('#SurfaceId').val();
    formData.LocationId = $('#LocationId').val();
    formData.IsFixedPrice = $('#fixedCheck').prop('checked');
    formData.FixedPrice = $('#FixedPrice').val();
    formData.FacilityPitchTimings = [];
    formData.IsEnabled = $('#IsActive').prop('checked');
    $('select[name="Day"]').each(function (i) {
        formData.FacilityPitchTimings.push({ Day: "", TimeStart: "", TimeEnd: "", CustomPrice: "" });
        formData.FacilityPitchTimings[i].Day = $(this).val();
    });
    $('select[name="TimeStart"]').each(function (i) {
        formData.FacilityPitchTimings[i].TimeStart = $(this).val();
    });
    $('select[name="TimeEnd"]').each(function (i) {
        formData.FacilityPitchTimings[i].TimeEnd = $(this).val();
    });
    $('input[name="CustomPrice"]').each(function (i) {
        formData.FacilityPitchTimings[i].CustomPrice = $(this).val();
    });
    $('input[id="FacilityPitchTimingId"]').each(function (i) {
        if ($(this).val() == '') {
            $(this).val('00000000-0000-0000-0000-000000000000');
        }
        formData.FacilityPitchTimings[i].FacilityPitchTimingId = $(this).val();
    });
    $('input[id="facilityPitchId"]').each(function (i) {
        if ($(this).val() == '') {
            $(this).val('00000000-0000-0000-0000-000000000000');
        }
        formData.FacilityPitchTimings[i].FacilityPitchId = $(this).val();
    });

    formData.TimingIdsToRemove = $('#timingIdToRemove').val();
    formData.Id = parseInt($('#Id').val());

    isEqual = checkIfEqualTimeSlot(formData);
    overlappingTimeSlot = checkOverLappingTimeSlot(formData);

    if (isEqual || overlappingTimeSlot) {
        $("#GenericModal").modal('show');
        $('#GenericModalMsg').html('There is either Overlapping or Duplicate Time Slot within the same day. Please choose another time slot with another Day');
        $("#errorIcon").show();
        $("#errorBtn").show();
    }
    else {
        if (validation(formData)) {
            $.ajax({
                url: "Add",
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(formData),
                success: function (result) {
                    if (result.message == 'Invalid Object!') {
                        $('#GenericModalMsg').html('There is an error saving a new pitch');

                        $("#GenericModal").modal('show');

                        $("#errorIcon").show();
                        $("#facility-pitch-error-btn").show();
                    }
                    else if (result.message == 'Slot is occupied') {
                        $('#GenericModalMsg').html(result.message);

                        $("#GenericModal").modal('show');

                        $("#errorIcon").show();
                        $("#facility-pitch-error-btn").show();
                    }
                    else {
                        $("#GenericModal").modal('show');
                        $('#GenericModalMsg').html(result.message);
                        $("#errorIcon").hide();
                        $("#facility-pitch-error-btn").hide();
                        $("#successIcon").show();
                        $("#successBtn").show();
                    }
                },
                failure: function (result) {
                    console.log(result);
                }
            });
        }
       
    }

});

function validation(formData) {
    var flag = true
    $("#errName").addClass("text-hide");
    $("#errSportId").addClass("text-hide");
    $("#errMaxPlayers").addClass("text-hide");
    $("#errTeamSize").addClass("text-hide");
    $("#errSurface").addClass("text-hide");
    $("#errLocationId").addClass("text-hide");
    if (formData.Name === "") {
        $("#errName").removeClass("text-hide");
        flag = false
    }
    if (formData.SportId === null) {
        $("#errSportId").removeClass("text-hide");
        flag = false
    }
    if (formData.MaxPlayers === null) {
        $("#errMaxPlayers").removeClass("text-hide");
        flag = false
    }
    if (formData.TeamSize === null) {
        $("#errTeamSize").removeClass("text-hide");
        flag = false
    }
    if (formData.SurfaceId === null) {
        $("#errSurface").removeClass("text-hide");
        flag = false
    }
    if (formData.LocationId === null) {
        $("#errLocationId").removeClass("text-hide");
        flag = false
    }
    return flag
}

function checkOverLappingTimeSlot(formData) {
    for (var i = 0; i < formData.FacilityPitchTimings.length; i++) {
        for (var j = 1; j < formData.FacilityPitchTimings.length; j++) {
            if (i != j) {
                if (formData.FacilityPitchTimings[i].Day == formData.FacilityPitchTimings[j].Day) {
                    return compareTimeSlots(formData.FacilityPitchTimings[i], formData.FacilityPitchTimings[j]);
                }
            }
        }
    }
}

function compareTimeSlots(firstNum, secondNum) {
    return getMinutes(firstNum.TimeEnd) > getMinutes(secondNum.TimeStart) && getMinutes(secondNum.TimeEnd) > getMinutes(firstNum.TimeStart);
}

function getMinutes(text) {
    let split = text.split(":").map(Number);
    return split[0] * 60 + split[1];
}

function checkIfEqualTimeSlot(formData) {
    for (var i = 0; i < formData.FacilityPitchTimings.length; i++) {
        for (var j = 1; j < formData.FacilityPitchTimings.length; j++) {
            if (i != j) {
                if (formData.FacilityPitchTimings[i].Day == formData.FacilityPitchTimings[j].Day
                    && formData.FacilityPitchTimings[i].TimeStart == formData.FacilityPitchTimings[j].TimeStart
                    && formData.FacilityPitchTimings[i].TimeEnd == formData.FacilityPitchTimings[j].TimeEnd) {

                    return true;
                }
                else {

                    return false;
                }
            }
        }
    }
}

$('#fixedCheck').change(function () {
    if ($('#fixedCheck').is(':checked')) {
        $('#showFixedPriceInput').attr('hidden', false);
        $('input[name="CustomPrice"]').each(function (i) {
            $(this).attr('disabled', true);
        });
    }
    else {
        $('#showFixedPriceInput').attr('hidden', true);
        $('input[name="CustomPrice"]').each(function (i) {
            $(this).attr('disabled', false);
        });
    }
});

function addSlot() {
    $.ajax({
        url: "AddTimingSlot",
        cache: false,
        success: function (html) {
            $('#pitch-timing').append(html)
            if ($('#fixedCheck').is(':checked')) {
                $('input[name="CustomPrice"]').each(function (i) {
                    $(this).prop('disabled', true);
                });
            }

            $('select[name="TimeEnd"]').each(function (i) {
                if ($(this).val() == "00:00:00") {
                    $(this).val("23:30:00");
                }

            });

            $('div[id="remove-slot"]').each(function (i) {
                if (i != 0) {
                    $(this).attr('hidden', false);
                }
            });
        }
    });
}

function populateFields(facilityPitch) {
    var facilityPitchTimings = facilityPitch.facilityPitchTimings;
    $('#TimeEnd').val("23:30:00");

    $('select[name="TimeStart"]').each(function (i) {
        for (var x = 0; x < facilityPitchTimings.length; x++) {
            if (i == x) {
                let parsedTimeStart = new Date(facilityPitchTimings[x].timeStart);
                let time = parsedTimeStart.toLocaleTimeString([], { hour: '2-digit', minute: "2-digit", second: "2-digit", hour12: false });
                $(this).val(time == '24:00:00' ? '00:00:00' : time);
            }
        }
    });

    $('select[name="TimeEnd"]').each(function (i) {
        for (var x = 0; x < facilityPitchTimings.length; x++) {
            if (i == x) {
                let parsedTimeEnd = new Date(facilityPitchTimings[i].timeEnd);
                let time = parsedTimeEnd.toLocaleTimeString([], { hour: '2-digit', minute: "2-digit", second: "2-digit", hour12: false });
                $(this).val(time == '24:00:00' ? '23:30:00' : time);
            }
        }
    });

    $('input[id="FacilityPitchTimingId"]').each(function (i) {
        for (var x = 0; x < facilityPitchTimings.length; x++) {
            if (i == x) {
                console.log(facilityPitchTimings[x].facilityPitchTimingId);
                $(this).val(facilityPitchTimings[x].facilityPitchTimingId);
            }
        }
    });

    $('input[id="facilityPitchId"]').each(function (i) {
        $(this).val(facilityPitch.facilityPitchId);
    });

    $('#MaxPlayers').val(facilityPitch.maxPlayers.toString());

    if ($('#fixedCheck').prop('checked')) {
        $('#showFixedPriceInput').attr('hidden', false);
        $('input[name="CustomPrice"]').each(function (i) {
            for (var x = 0; x < facilityPitchTimings.length; x++) {
                if (i == x) {
                    $(this).val('0');
                    $(this).attr('disabled', true);
                }
            }
        });
    }

    $('#IsActive').prop('checked', facilityPitch.isEnabled);
    if (facilityPitch.isEnabled) {
        $('#lblActive').html('Active').addClass('white-text');
    }

    $('#MaxPlayers').val(facilityPitch.maxPlayers);
}

function uploadImages(inputId, uploadURL) {
    var input = document.getElementById(inputId);
    var files = input.files;
    var formData = new FormData();

    for (var i = 0; i !== files.length; i++) {
        formData.append("files", files[i]);
    }

    $("#imageLoading" + inputId).show();

    $.ajax(
        {
            url: uploadURL,
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                $("#imgPrev" + inputId).attr("src", data.publicUrl);
                $("#imgPrev" + inputId).show();
                $("#imageLoading" + inputId).hide();
                if (inputId.includes("ImageUrl")) {
                    $("#ImageUrl").val(data.publicUrl);
                    console.log(data.publicUrl);
                }
                else if (inputId.includes("ShopIcon")) {
                    $("#ShopIcon").val(data.publicUrl);
                }
                else if (inputId.includes("UploadProfileImage")) {
                    $("#ImageUrl").val(data.publicUrl);
                }
             else {
                $("#ImageUrl").val(data.publicUrl);
                console.log(data.publicUrl);
            }
        },
        failed: function () {
            $("#imgPrev" + inputId).hide();
            $("#imageLoading" + inputId).show();
        }
        }
    );
}

//checkbox inactive/active generic to all
$(document).ready(function () {
    if ($("#IsEnabled").val() == "True") {
        $("#IsEnabled").val("true");
        $('#IsActive').prop('checked', true);
        $("#lblActive").text("Active");
        $("#lblActive").addClass("white-text");
    } else {
        $('#IsActive').prop('checked', false);
        $("#IsEnabled").val("false");
        $("#lblActive").text("Inactive");
        $("#lblActive").addClass("red-text");
        $("#lblActive").removeClass("white-text");
    }

    $("#IsActive").change(function () {
        if ($(this).prop("checked") == true) {
            $("#IsEnabled").val("true");
            $("#lblActive").text("Active");
            $("#lblActive").addClass("white-text");
        } else {
            $("#IsEnabled").val("false");
            $("#lblActive").text("Inactive");
            $("#lblActive").addClass("red-text");
            $("#lblActive").removeClass("white-text");
        }
    });



});