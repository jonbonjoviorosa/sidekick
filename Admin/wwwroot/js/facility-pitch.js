function displayPitch() {
    $('#selected-pitch').change(function () {
        let selected = $('#selected-pitch').val();
        $('input[id="sport-name"]').each(function (i) {
            let counter = i + 1;
            let sportName = $(this).val();
            let pitchDivName = $('#pitch-model-' + counter);
            if (selected == sportName) {
                $(pitchDivName).attr('hidden', false)
            }
            else {
                $(pitchDivName).attr('hidden', true)
            }
        });

        if (selected == 'AllSports') {
            $('div[id*="pitch-model-"]').each(function () {
                $(this).attr('hidden', false)
            });
        }
    });
}

function bindPitchDetails(timingId, maxPlayers) {
    $.ajax({
        url: "GetFacilityPitchTiming?id=" + timingId,
        type: "GET",
        contentType: 'application/json',
        data: timingId,
        success: function (result) {

            console.log(result);

            $('#render-player-detail').remove();
            $('#render-player').html('<div class="row py-3" id="render-player-detail"></div>');
            for (var i = 0; i < result.payload.players.length; i++) {
                let img = "~/img/default-img.jpg";
                if (result.payload.players[i].profileImgUrl != null) {
                    img = result.payload.players[i].profileImgUrl;
                }
                $('#render-player-detail').append('<div class="col-sm-4">' +

                    '<div class="row mx-0 mb-3 align-items-center">' +
                    '<div class="col-3 px-0">' +
                    '<img src="' + img + '" class="w-100 circle">' +
                    '</div>' +
                    '<div class="col-9 px-2">' +
                    '<p class="medium mb-0">' + result.payload.players[i].firstName + ' ' + result.payload.players[i].lastName + '</p>' +
                    '</div>' +
                    '</div>' +
                    '</div>');
            }
            $('#facilityPitchTimingId').val(timingId);
            let pitchStart = new Date(result.payload.timing.timeStart);
            let pitchEnd = new Date(result.payload.timing.timeEnd);
            let date = new Date(result.payload.timing.date);
            $('#booking-time').text(pitchStart.toLocaleTimeString([], { timeStyle: 'short' }) + ' - ' + pitchEnd.toLocaleTimeString([], { timeStyle: 'short' }));
            $('#booking-date').text(date.toLocaleDateString('en-GB', {
                day: 'numeric', month: 'short', year: 'numeric'
            }));
            if (result.payload.timing.isFree) {
                $('#booking-price').text('Free');
            }
            else {
                $('#booking-price').text(result.payload.timing.customPrice + ' AED');
            }
            
            $('#booking-player-count').text(result.payload.players.length + '/' + maxPlayers + ' Players');


            console.log(result);
        },
        failure: function (result) {
            console.log(result);
        }
    });
}

$("#successCancelBooking").on('click', function () {
    location.reload();
});

$('#cancel-booking').click(function () {
    let facilityPitchTimingId = $('#facilityPitchTimingId').val();
    $.ajax({
        url: "CancelBooking?id=" + facilityPitchTimingId,
        type: "GET",
        contentType: 'application/json',
        data: facilityPitchTimingId,
        success: function (result) {
            if (result.statusCode == 200) {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html('Cancel Success');
                $("#successIcon").show();
                $('#successCancelBooking').show();
            }
            else {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#errorIcon").show();
                $('#errorCancelBooking').show();
            }
            
            console.log(result);
        },
        failure: function (result) {
            console.log(result);
        }
    });
});


