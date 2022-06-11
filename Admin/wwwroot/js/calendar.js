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

function formatDate(date) {
    let month = '' + (date.getMonth() + 1);
    let day = '' + date.getDate();
    let year = date.getFullYear();
    console.log('test');
    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;

    return [year, month, day].join('-');
}

$("#start").datepicker({
    onSelect: function (data) {
        console.log(data);
        $('#end').datepicker('option', 'minDate', data);
    },
    minDate: 0
});

$("#end").datepicker({
    minDate: 0
});

$('#selected-duration').change(function () {
    let selected = $(this).val();
    switch (parseInt(selected)) {
        case 5:
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
        case 12:
            $('#selected-duration-season > option').each(function () {
                if ($(this).val() == 'Month') {
                    $(this).prop('disabled', true);
                    $(this).css({ 'background-color': '#e9ecef' });
                }
            });

            $('#selected-duration-season').prop('selectedIndex', 0);
            break;
        default:
            $('#selected-duration-season > option').each(function () {
                $(this).prop('disabled', false);
                $(this).css({ 'background-color': 'white' })
            });
            break;
    }
});

$("#successBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Calendar");
});

$("#calendarErrorBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Calendar");
});

$('#yes-button').click(function () {
    let id = $('#id-value').val();
    let isFacilityPitch = $('#isFacilityPitch').val();
    var parameters = {
        GuID: id,
        IsEnabled: false
    };

    if (isFacilityPitch == 'cancel-facility-pitch') {
        $.ajax(
            {
                type: 'POST',
                url: BASEPATH + '/BookingPitch/Delete',
                contentType: 'application/json',
                data: JSON.stringify(parameters),
                beforeSend: function () {
                    $('#cancelModal').modal('hide');
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
                        $("#calendarErrorBtn").show();
                    }
                    else if (result.status == 'Cancelled') {
                        $("#GenericModal").modal('show');
                        $("#savingGenericLoader").hide();
                        $("#GenericModalTitle").text(result.message);
                        $("#errorIcon").show();
                        $("#calendarErrorBtn").show();
                    }
                    else {
                        $("#errorIcon").hide();
                        $("#savingGenericLoader").hide();
                        $("#calendarErrorBtn").hide();
                        $("#GenericModal").modal('show');
                        $("#successIcon").show();
                        $("#GenericModalTitle").text("Success");

                        setTimeout(function () { window.location.replace(BASEPATH + "/Calendar") }, 3000);
                    }
                },
                failed: function (e, x, h) {
                    console.log(e); console.log(h); console.log(x);
                }
            }
        );
    }
    else {
        $.ajax(
            {
                type: 'POST',
                url: BASEPATH + 'Calendar/DeleteSlot',
                contentType: 'application/json',
                data: JSON.stringify(parameters),
                beforeSend: function () {
                    $('#cancelModal').modal('hide');
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
                        $("#calendarErrorBtn").show();
                    }
                    else if (result.status == 'Cancelled') {
                        $("#GenericModal").modal('show');
                        $("#savingGenericLoader").hide();
                        $("#GenericModalTitle").text(result.message);
                        $("#errorIcon").show();
                        $("#calendarErrorBtn").show();
                    }
                    else {
                        $("#errorIcon").hide();
                        $("#savingGenericLoader").hide();
                        $("#calendarErrorBtn").hide();
                        $("#GenericModal").modal('show');
                        $("#successIcon").show();
                        $("#GenericModalTitle").text("Success");

                        setTimeout(function () { window.location.replace(BASEPATH + "/Calendar") }, 3000);
                    }
                },
                failed: function (e, x, h) {
                    console.log(e); console.log(h); console.log(x);
                }
            }
        );
    }

});


$('#cancel-slot').click(function () { 
    $('#id-value').val($('#UnavailableSlotId').val());
    $('#slotModal').modal('hide');
});

$('#no-button').click(function () {
    $('#slotModal').modal('show');
    $('#no-button').attr('hidden', false);
});

function renderCalendar(calendarEl, bookedPitch) {
    var calendar = new FullCalendar.Calendar(calendarEl, {
        /*initialDate: '2020-09-12',*/
        initialDate: new Date(),
        initialView: 'timeGridWeek',
        nowIndicator: true,
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'timeGridWeek,timeGridDay'
        },
        dateClick: function (e) {
            console.log(e);
        },
        navLinks: true, // can click day/week names to navigate views
        editable: false,
        selectable: true,
        selectMirror: true,
        dayMaxEvents: true, // allow "more" link when too many events
        businessHours: {
            daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
            startTime: '08:00',
            endTime: '24:00',
        },
        events: bookedPitch,
        eventClick: function (info) {
            $('#cancel-slot').removeClass('disabled');
            let hasTiming = info.event.groupId.split(";");
            if (hasTiming[1] == "can-cancel") {
                window.location.href = "/Pitches/FacilityPitchDetail" + info.event.id
                //$("#no-button-exit").val('booking-slot');
                //$('#no-timing-button').attr('hidden', false);
                //$('#no-button').attr('hidden', true);
                ///*$('#cancelModal').modal('show');*/
                //$('#id-value').val(hasTiming[0]);
                //$('#unavailable-slot').text('Facility Pitch Slot');
                //$('#isFacilityPitch').val('cancel-facility-pitch');
            }
            else {
                $('#no-button').attr('hidden', false);
                $('#no-timing-button').attr('hidden', true);
                $.ajax({
                    url: "Calendar/GetSlot?id=" + info.event.groupId,
                    type: "GET",
                    contentType: 'application/json',
                    data: info.event.groupId,
                    success: function (result) {
                        $('#selected-start-time').val(result.payload.starts.split("T")[1]);
                        $('#selected-end-time').val(result.payload.ends.split("T")[1]);
                        let startDate = getFormattedDate(new Date(result.payload.starts));
                        $('#start').val(startDate);
                        $('#end').val(getFormattedDate(new Date(result.payload.ends)));
                        $('#repeatCheck').prop('checked', result.payload.repeatEveryWeek);
                        $('#title').val(result.payload.title);
                        $('#notes').val(result.payload.notes);
                        if (result.payload.allPitches) {
                            $('#selected-pitch').val("AllPitches");
                        }
                        else {
                            $('#selected-pitch').val(result.payload.facilityPitchId);
                        }
                        $('#selected-duration-season').val(result.payload.during);
                        $('#UnavailableSlotId').val(info.event.groupId);

                        $('#slotModal').modal('show');
                        $('#IsActiveDay').prop('checked', result.payload.allDay);
                        if ($('#IsActiveDay').prop('checked')) {
                            $('#end').attr('disabled', true);
                            $('#start').attr('disabled', true);
                            $('#selected-start-time').attr('disabled', true);
                            $('#selected-end-time').attr('disabled', true);
                            $('#end').val(" ");
                            $('#start').val(" ");
                            $('#selected-start-time').val(" ");
                            $('#selected-end-time').val(" ");
                            $('#startDay').val(getFormattedDate(new Date(info.event.start)))
                        }
                        else {
                            $('#end').attr('disabled', false);
                            $('#start').attr('disabled', false);
                            $('#selected-start-time').attr('disabled', false);
                            $('#selected-end-time').attr('disabled', false);
                        }
                    }
                });
            }
        }
    });

    calendar.render();
}

function getFormattedDate(date) {
    let year = date.getFullYear();
    let month = (1 + date.getMonth()).toString().padStart(2, '0');
    let day = date.getDate().toString().padStart(2, '0');

    return month + '/' + day + '/' + year;
}

$('#saveUnavailableSlot').click(function (e) {
    e.preventDefault();
    let formData = new FormData();

    formData.AllDay = $('#IsActiveDay').prop('checked');
    if (formData.AllDay) {
        formData.Starts = $("#startDay").val() +  " 0:00:00";
        formData.Ends = $("#startDay").val() +" 23:50:00";
    }
    else {
        let start = new Date($('#start').val());
        let end = new Date($('#end').val());
        let startTime = $('#selected-start-time').val();
        let endTime = $('#selected-end-time').val();
        start.setHours(startTime.split(":")[0], startTime.split(":")[1])
        end.setHours(endTime.split(":")[0], endTime.split(":")[1])
        formData.Starts = start.toLocaleString();
        formData.Ends = end.toLocaleString();
    }

    if ($('#UnavailableSlotId').val() != '') {
        formData.UnavailableSlotId = $('#UnavailableSlotId').val();
    }
    else {
        formData.UnavailableSlotId = '00000000-0000-0000-0000-000000000000';
    }

    formData.RepeatEveryWeek = $('#repeatCheck').prop('checked');
    formData.During = $('#selected-duration-season').val();
    let pitchSelection = $('#selected-pitch').val();
    if (pitchSelection == 'AllPitches') {
        formData.AllPitches = true;
    } else {
        formData.FacilityPitchId = pitchSelection;
        formData.AllPitches = false;
    }
    formData.Title = $('#title').val();
    formData.Notes = $('#notes').val();

    $.ajax({
        url: "Calendar/SaveUnavailableSlot",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (result) {
            if (result.message == 'Invalid Object!') {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html("Please don't leave Title and Description blank.");
                $("#errorIcon").show();
                $("#calendarErrorBtn").show();
                $('#slotModal').hide();
            }
            else if (result.payload == null) {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#errorIcon").show();
                $("#calendarErrorBtn").show();
                $('#slotModal').hide();
            }
            else {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#successIcon").show();
                $("#successBtn").show();
                $('#slotModal').hide()
            }
        },
        failure: function (result) {
            console.log(result);
        }
    })
});

$('#click-unavailable-slot').click(function () {
    $('form').trigger('reset');
    $('#start').val(moment(new Date()).format('MM/DD/YYYY'));
    $('#end').val(moment(new Date()).format('MM/DD/YYYY'));
    $('#selected-end-time').val("23:30:00");
    $('#cancel-slot').addClass('disabled');
});

function renderBookedPitch(bookedPitch, obj, unavailableSlots, timings) {
    if (unavailableSlots.length != 0) {
        for (var i = 0; i < unavailableSlots.length; i++) {

            let startRecurring = formatDate(new Date(unavailableSlots[i].starts));
            let endRecurring = formatDate(new Date(unavailableSlots[i].ends));
            let startTime = new Date(unavailableSlots[i].starts).toLocaleString('en-GB');
            let endTime = new Date(unavailableSlots[i].ends).toLocaleString('en-GB');
            let start = startRecurring + " " + startTime.split(" ")[1];
            let end = endRecurring + " " + endTime.split(" ")[1];
            if (unavailableSlots[i].allDay) {
                let isRepeat = unavailableSlots[i].repeatEveryWeek;
                if (isRepeat) {
                    let isRepeatDay = new Date(startRecurring).getDay();
                    bookedPitch.push(
                        {
                            groupId: unavailableSlots[i].unavailableSlotId,
                            title: unavailableSlots[i].facilityPitchName,
                            start: startRecurring,
                            end: endRecurring,
                            backgroundColor: '#FFC400',
                            allDay: unavailableSlots[i].allDay,
                            daysOfWeek: [isRepeatDay],
                            /*description: unavailableSlots[i].notes*/
                        });
                }
                else {
                    bookedPitch.push(
                        {
                            groupId: unavailableSlots[i].unavailableSlotId,
                            title: unavailableSlots[i].facilityPitchName,
                            allDay: unavailableSlots[i].allDay,
                            start: startRecurring,
                            end: endRecurring,
                            backgroundColor: '#FFC400',
                            description: unavailableSlots[i].notes
                        });
                }

            }
            else {
                let isRepeat = unavailableSlots[i].repeatEveryWeek;
                if (isRepeat) {
                    let isRepeatDay = new Date(startRecurring).getDay();
                    bookedPitch.push(
                        {
                            groupId: unavailableSlots[i].unavailableSlotId,
                            title: unavailableSlots[i].facilityPitchName,
                            startTime: startTime.split(" ")[1],
                            endTime: endTime.split(" ")[1],
                            backgroundColor: '#FFC400',
                            allDay: false,
                            daysOfWeek: [isRepeatDay],
                            /*description: unavailableSlots[i].notes*/
                        });
                }
                else {
                    /*let during = unavailableSlots[i].during;*/
                    bookedPitch.push(
                        {
                            groupId: unavailableSlots[i].unavailableSlotId,
                            title: unavailableSlots[i].facilityPitchName,
                            start: start,
                            end: end,
                            //startTime: startTime,
                            //endTime: endTime,
                            //startRecur: startRecurring,
                            //endRecur: endRecurring,
                            //startTime: startTime.split(" ")[1],
                            //endTime: endTime.split(" ")[1],
                            backgroundColor: '#FFC400',
                            allDay: false,
                            /*daysOfWeek: [isRepeatDay],*/
                            /*description: unavailableSlots[i].notes*/
                        });
                }

            }
        }
    }
    if (timings.length != 0) {
        console.log(timings)
        for (var i = 0; i < timings.length; i++) {
            let startTime = new Date(timings[i].timeStart).toLocaleString('en-GB');
            let endTime = new Date(timings[i].timeEnd).toLocaleString('en-GB');
            let startRecurring = formatDate(new Date(timings[i].timeStart));
            let endRecurring = formatDate(new Date(timings[i].timeEnd));
            let start = startRecurring + " " + startTime.split(" ")[1];
            let end = endRecurring + " " + endTime.split(" ")[1];
            let isRepeat = timings[i].isRepeatEveryWeek;
            if (isRepeat) {
                let isRepeatDay = new Date(startRecurring).getDay();
                bookedPitch.push(
                    {
                        groupId: timings[i].facilityPitchTimingId + ";can-cancel",
                        title: timings[i].playerCount + " - " + timings[i].facilityPitchName,
                        startTime: startTime.split(" ")[1],
                        endTime: endTime.split(" ")[1],
                        backgroundColor: '#28a745',
                        allDay: false,
                        daysOfWeek: [timings[i].day],
                        id: "?facilityPitchId=" + timings[i].facilityPitchId + ";" + timings[i].sportId + ";" + timings[i].facilityPitchTimingId + ";" + timings[i].bookingId
                        /*description: unavailableSlots[i].notes*/
                    });
            }
            else {
                bookedPitch.push(
                    {
                        groupId: timings[i].facilityPitchTimingId + ";can-cancel",
                        title: timings[i].playerCount + " - " + timings[i].facilityPitchName,
                        start: start,
                        end: end,
                        backgroundColor: '#28a745',
                        allDay: false,
                        id: "?facilityPitchId=" + timings[i].facilityPitchId + ";" + timings[i].sportId + ";" + timings[i].facilityPitchTimingId + ";" + timings[i].bookingId
                    });
            }
        }
    }
    //if (obj.length != 0) {

    //    for (var i = 0; i < obj.length; i++) {
    //        let startRecurring = formatDate(new Date(obj[i].pitchStart));
    //        let endRecurring = formatDate(new Date(obj[i].pitchEnd));
    //        let startTime = new Date(obj[i].pitchStart).toLocaleString('en-GB');
    //        let endTime = new Date(obj[i].pitchEnd).toLocaleString('en-GB');
    //        let start = startRecurring + " " + startTime.split(" ")[1];
    //        let end = endRecurring + " " + endTime.split(" ")[1];
    //        bookedPitch.push(
    //            {
    //                title: obj[i].name,
    //                start: start,
    //                end: end,

    //                backgroundColor: '#FFC400',
    //            });
    //    }
    //}  
}

$('#IsActiveDay').change(function () {
    if ($(this).prop('checked')) {
        $('#end').attr('disabled', true);
        $('#start').attr('disabled', true);
        $('#selected-start-time').attr('disabled', true);
        $('#selected-end-time').attr('disabled', true);
        $('#end').val(" ");
        $('#start').val(" ");
        $('#selected-start-time').val(" ");
        $('#selected-end-time').val(" ");
    }
    else {
        $('#end').attr('disabled', false);
        $('#start').attr('disabled', false);
        $('#selected-start-time').attr('disabled', false);
        $('#selected-end-time').attr('disabled', false);
        $('form').trigger('reset');
        $('#start').val(moment(new Date()).format('MM/DD/YYYY'));
        $('#end').val(moment(new Date()).format('MM/DD/YYYY'));
        $('#selected-end-time').val("23:30:00");
        $('#cancel-slot').addClass('disabled');
    }

});
