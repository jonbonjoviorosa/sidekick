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

$('#ByFacility').change(function () {
    if ($(this).prop('checked')) {
        $('#FacilityId').prop('disabled', true);
        $('#byFCheck').prop('checked', false);
    } else {
        $('#FacilityId').prop('disabled', false);
    }
});

$('#byFCheck').change(function () {
    if ($(this).prop('checked')) {
        $('#FacilityId').prop('disabled', false);
        $('#ByFacility').prop('checked', false);
    } else {
        $('#FacilityId').prop('disabled', true);
    }
});

$("#errorBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Promo");
});

$("#successAddPromoBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Promo");
});

function renderPromoValues(promo) {
    $('#IsActive').prop('checked', promo.isActive);
    if (promo.facilityId != '00000000-0000-0000-0000-000000000000') {
        $('#FacilityPitchId').prop('disabled', false);
        $('#byFCheck').prop('checked', true);
        
        $('#FacilityId').val(promo.facilityId);
    }
    else {
        $('#FacilityId').val("");
        
    }

    if (promo.isActive) {
        $('#active-label').text('Active');
    }
    else {
        $('#active-label').text('Inactive');
    }

    $('#ByFacility').prop('checked', promo.byFacility == 'false' ? false : true);
    $('#start').val(transformDate(promo.startsFrom));
    $('#end').val(transformDate(promo.validTo));
    $('#CoachId').val(promo.coachId);
}

function transformDate(strDate) {
    var date = new Date(strDate);
    let dateString = [
        date.getMonth() + 1,
        date.getDate(),
        date.getFullYear(),
    ].join('/')

    return dateString;
}