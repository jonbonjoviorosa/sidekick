$('#save-user-coach').click(function (e) {
    console.log('test');
    e.preventDefault();
    let formData = new FormData();
    formData.ImageUrl = $('#ImageUrl').val();
    formData.FirstName = $('#FirstName').val();
    formData.LastName = $('#LastName').val();
    formData.AreaId = parseInt($('#AreaId').val());
    formData.Password = $('#Password').val();
    formData.ConfirmPassword = $('#ConfirmPassword').val();
    formData.MobileNumber = $('#MobileNumber').val();
    formData.Description = $('#Description').val();
    formData.Email = $('#Email').val();
    formData.NationalityId = $('#NationalityId').val();
    formData.UserId = $('#UserId').val();
    if (formData.UserId !== undefined) {
        if (formData.Password == '' && formData.Password == '') {
            formData.Password = 'test123';
            formData.ConfirmPassword = 'test123';
            formData.IsPasswordEdit = false;
        } else { formData.IsPasswordEdit = true; }
    }
    formData.IsActive = $('#IsActive').prop('checked');

    formData.GymIds = [];
    let gyms = $('select[name="GymIds"]').val();
    for (var i = 0; i < gyms.length; i++) {
        formData.GymIds.push(gyms[i]);
    }

    formData.SpecialtyIds = [];
    let specialties = $('select[name="SpecialtyIds"]').val();
    for (var i = 0; i < specialties.length; i++) {
        formData.SpecialtyIds.push(specialties[i]);
    }

    formData.LanguageIds = [];
    let languages = $('select[name="LanguageIds"]').val();
    for (var i = 0; i < languages.length; i++) {
        formData.LanguageIds.push(languages[i]);
    }

    $.ajax({
        url: "AddOrEditUserCoach",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(formData),
        dataType: 'json',
        success: function (result) {
            console.log('test');
            if (result.modelError != null) {
                
                let errorMessage = [];
                for (var i = 0; i < result.modelError.length; i++) {
                    errorMessage.push('<p>' + result.modelError[i].key + ' is required </p>');
                    
                }
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(errorMessage);
                $("#errorIcon").show();
                $("#errorCoachBtn").show();
            }
            else if (result.status == 'Error') {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#errorIcon").show();
                $("#errorCoachBtn").show();
            }
            else {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#errorIcon").hide();
                $("#errorCoachBtn").hide();
                $("#successAddCoachBtn").show();
                $("#successIcon").show();
            }
        },
        failure: function (result) {
            console.log(result);
        }
    });
});

$("#successAddCoachBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Coach");
});

$('.btn-delete-coach').click(function () {
    $("#YesNoModal").modal('show');

    var coachUserId = $(this).closest('td').find('#coach-userId').val()
    $("#btnYesDelete").click(function () {
        $("#YesNoModal").modal('hide');

        changeStatus(coachUserId, false, true);
    });
});

$('.btn-delete-player').click(function () {
    $("#YesNoModal").modal('show');
    $('#modalMsg').text('Do you want to delete this player?');
    var playerUserId = $(this).closest('td').find('#player-userId').val()
    $("#btnYesDelete").click(function () {
        $("#YesNoModal").modal('hide');
       

        playerChangeStatus(playerUserId, false, true);
    });
});


$('#IsActive').change(function () {
    if ($(this).prop('checked')) {
        $('#label-text').text('Active');
    } else {
        $('#label-text').text('Inactive');
    }
});

function changeStatus(rid, stat) {
    var parameters = {
        GuID: rid,
        IsEnabled: stat
    };

    $.ajax(
        {
            type: 'POST',
            url: BASEPATH + '/Coach/Status',
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
                    $("#errorBtn").show();
                }
                else if (result.status == 'Cancelled') {
                    $("#GenericModal").modal('show');
                    $("#savingGenericLoader").hide();
                    $("#GenericModalTitle").text(result.message);
                    $("#errorIcon").show();
                    $("#errorBtn").show();
                }
                else {
                    $("#errorIcon").hide();
                    $("#savingGenericLoader").hide();
                    $("#errorBtn").hide();
                    $("#GenericModal").modal('show');
                    $("#successIcon").show();
                    $("#GenericModalTitle").text("Successfully Set Coach to Inactive");

                    setTimeout(function () { window.location.replace(BASEPATH + "/Coach") }, 3000);
                }
            },
            failed: function (e, x, h) {
                console.log(e); console.log(h); console.log(x);
            }
        }
    );
}

function playerChangeStatus(rid, stat) {
    var parameters = {
        GuID: rid,
        IsEnabled: stat
    };

    $.ajax(
        {
            type: 'POST',
            url: BASEPATH + '/Player/Status',
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
                    $("#errorAddPlayerBtn").show();
                }
                else if (result.status == 'Cancelled') {
                    $("#GenericModal").modal('show');
                    $("#savingGenericLoader").hide();
                    $("#GenericModalTitle").text(result.message);
                    $("#errorIcon").show();
                    $("#errorAddPlayerBtn").show();
                }
                else {
                    $("#errorIcon").hide();
                    $("#savingGenericLoader").hide();
                    $("#errorAddPlayerBtn").hide();
                    $("#GenericModal").modal('show');
                    $("#successIcon").show();
                    $("#successAddPlayerBtn").show();
                    $("#GenericModalTitle").text(result.message);
                    
                    setTimeout(function () { window.location.replace(BASEPATH + "/Player") }, 3000);
                }
            },
            failed: function (e, x, h) {
                console.log(e); console.log(h); console.log(x);
            }
        }
    );
}