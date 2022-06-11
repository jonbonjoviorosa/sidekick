$("#start").datepicker({
    onSelect: function (data) {
        console.log(data);
        $('#end').datepicker('option', 'minDate', data);
    },
    minDate: 0
});



function removeSlot(e) {
    e.parentElement.parentElement.parentElement.remove();
}

function populateFacilityPitches() {
 /*   $("#FacilityPitchId").empty();*/
    var facilityId = $('#FacilityId').val();
    var sportId = $('#SportId').val();
    $('#start').val("");
    $('#End').val("23:30:00");

    $.ajax({
        url: "GetFacilityPitches/",
        type: "GET",
        contentType: 'application/json',
        data: {
            'facilityId': facilityId,
            'sportId': sportId
        },
        success: function (result) {
            console.log(result);
            $('#FacilityPitchId').html('');
            $('#FacilityPitchId').append('<option value="0" disabled selected>Select an option</option>');
            for (var i = 0; i < result.length; i++) {
                $('#FacilityPitchId').append('<option value="' + result[i].value + '">' + result[i].text + '</option>');
            }
        },
        failure: function (result) {
            console.log(result);
        }
    });
}

function populatePricePerPlayer() {
    let sportId = $('#SportId').val();
    $.ajax({
        url: "GetCommissionPrice?id=" + sportId,
        type: "GET",
        contentType: 'application/json',
        data: sportId,
        success: function (result) {
            if (result == null) {
                $('#TotalPrice').val("0");
            }
            else { $('#TotalPrice').val(result.comissionPerPlayer); }
        },
        failure: function (result) {
            console.log(result);
        }
    });
}

$('#FacilityId').change(function () {
    populateFacilityPitchSports();
});

function populateFacilityPitchSports() {
/*    $("#FacilityPitchId").empty();*/
    var facilityId = $('#FacilityId').val();
    $('#start').val("");
    $('#End').val("23:30:00");

    $.ajax(
        {
            url: "GetFacilityPitchSports?facilityId=" + facilityId,
            type: "GET",
            contentType: 'application/json',
            data: facilityId,
            success: function (result) {
                $('#SportId').html('');
                $('#SportId').append('<option value="0" disabled selected>Select an option</option>');
                for (var i = 0; i < result.length; i++) {
                    $('#SportId').append('<option value="' + result[i].value + '">' + result[i].text + '</option>');
                }
            },
            failed: function (e, x, h) {
                console.log(e); console.log(h); console.log(x);
            }
        }
    );
}

$('#SportId').change(function () {
    populatePricePerPlayer();
    populateFacilityPitches();
});

function addPlayer() {
    console.log('test');
    $.ajax({
        url: "AddPlayer",
        cache: false,
        success: function (html) {
            $('#add-player').append(html);

            //$('div[id="remove-slot"]').each(function (i) {
            //    if (i != 0) {
            //        $(this).attr('hidden', false);
            //    }
            //});
        }
    });
}

$('#freeCheck').change(function () {
    if ($(this).prop('checked')) {
        $('#check-if-free').removeAttr('hidden');
    }
    else {
        $('#check-if-free').attr('hidden', true);
    }
});

$('#save-slot').click(function (e) {
    e.preventDefault();
    console.log('test');
    let formData = new FormData();
    formData.FacilityId = $('#FacilityId').val();
    formData.FacilityPitchId = $('#FacilityPitchId').val() == '' ? '00000000-0000-0000-0000-000000000000' : $('#FacilityPitchId').val();;
    formData.SportId = $('#SportId').val();
   /* formData.MaxPlayers = parseInt($('#MaxPlayers').val());*/
    formData.Date = $('#start').val();
    formData.IsRepeatEveryWeek = $('#repeatCheck').prop('checked');
    formData.Start = $('#Start').val();
    formData.End = $('#End').val();
    formData.Day = $('#Day').val();
    formData.IsFree = $('#freeCheck').prop('checked');
    let players = [];
    if (formData.IsFree) {       
        $('select[id="facility-player"]').each(function (i) {
            players.push($(this).val());
        });
    }
    
    formData.PlayerIds = players.join(";");
    formData.TotalPrice = parseInt($('#TotalPrice').val());
   
    formData.Description = $('#Description').val();
    console.log(formData);
    $.ajax({
        url: "AddFacilityPitchSlot",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (result) {
            if (result.statusCode == 400) {
                $('#GenericModalMsg').html(result.message);
                $("#GenericModal").modal('show');
                $("#errorIcon").show();
                $("#errorAddSlotBtn").show();
                $("#successAddSlotBtn").hide();
            }
            else {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#errorIcon").hide();
                $("#facility-pitch-error-btn").hide();
                $("#successIcon").show();
                $("#successAddSlotBtn").show();
                $("#errorAddSlotBtn").hide();
            }
        },
        failure: function (result) {
            console.log(result);
        }
    });
});

$("#successAddSlotBtn").on('click', function () {
    window.location.replace(BASEPATH + "/BookingPitch");
});

function populateEditSlot(obj) {
    $('#start').val(transformDate(obj.date));
    let parsedTimeStart = new Date(obj.start);
    let parsedTimeEnd = new Date(obj.end);
    let timeStart = parsedTimeStart.toLocaleTimeString([], { hour: '2-digit', minute: "2-digit", second: "2-digit", hour12: false });
    let timeEnd = parsedTimeEnd.toLocaleTimeString([], { hour: '2-digit', minute: "2-digit", second: "2-digit", hour12: false });
    $('select[name="Start"]').val(timeStart == '24:00:00' ? '00:00:00' : timeStart);
    $('select[name="End"]').val(timeEnd == '24:00:00' ? '00:00:00' : timeEnd);
}

$('.btn-delete-slot').click(function () {
    $("#YesNoModal").modal('show');
    $('#modalMsg').text('Do you want to delete it?');

    var timingId = $(this).closest('td').find('#facilitypitch-timingId').val()
    $("#btnYesDelete").click(function () {
        $("#YesNoModal").modal('hide');

        deleteSlot(timingId, false, true);
    });
});

function deleteSlot(rid, stat) {
    var parameters = {
        GuID: rid,
        IsEnabled: stat
    };

    $.ajax(
        {
            type: 'POST',
            url: BASEPATH + '/BookingPitch/Delete',
            contentType: 'application/json',
            data: JSON.stringify(parameters),
            beforeSend: function () {
                $("#GenericModal").modal('show');
                $("#GenericModalTitle").text("Please wait...");
                $("#savingGenericLoader").show();
            },
            success: function (result) {
                if (result.statusCode != 200) {
                    $("#GenericModal").modal('show');
                    $("#savingGenericLoader").hide();
                    $("#GenericModalTitle").text(result.message);
                    $("#errorIcon").show();
                    $("#errorAddSlotBtn").show();
                }
                else if (result.status == 'Cancelled') {
                    $("#GenericModal").modal('show');
                    $("#savingGenericLoader").hide();
                    $("#GenericModalTitle").text(result.message);
                    $("#errorIcon").show();
                    $("#errorAddSlotBtn").show();
                }
                else {
                    $("#errorIcon").hide();
                    $("#savingGenericLoader").hide();
                    $("#errorBtn").hide();
                    $("#GenericModal").modal('show');
                    $("#successIcon").show();
                    $("#GenericModalTitle").text("Success");

                    setTimeout(function () { window.location.replace(BASEPATH + "/BookingPitch") }, 3000);
                }
            },
            failed: function (e, x, h) {
                console.log(e); console.log(h); console.log(x);
            }
        }
    );
}