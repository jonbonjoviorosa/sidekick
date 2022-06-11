$('#save-commission-play').click(function (e) {
    e.preventDefault();
    let formData = new FormData();
    formData.Plays = [];
    $('input[id="commission-play-value"]').each(function (i) {
        formData.Plays.push({ SportId: "", ComissionPerPlayer: 0 });
        formData.Plays[i].ComissionPerPlayer = $(this).val() == "" ? 0 : $(this).val();
    });

    $('input[id="sport-id"]').each(function (i) {
        formData.Plays[i].SportId =  $(this).val();
    });

    $.ajax({
        url: '/Commission/AddOrEditCommissionPlay',
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (result) {
            if (result.message == 'Invalid Object!') {
                $('#GenericModalMsg').html('There is an error saving commission plays');

                $("#GenericModal").modal('show');

                $("#errorIcon").show();
                $("#errorBtn").show();
            }
            else {
                $("#GenericModal").modal('show');
                $('#GenericModalMsg').html(result.message);
                $("#errorIcon").hide();
                $("#errorBtn").hide();
                $("#successIcon").show();
                $("#successBtn").show();
            }
        },
        failure: function (result) {
            console.log(result);
        }
    });
});

$("#errorBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Commission");
});

$("#successBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Commission");
});

$('#print').click(function () {
    //var CommissionTable = document.getElementById("dtMainList");
    //let win = window.open("");
    //win.document.write(CommissionTable.outerHTML);
    //win.print();
    //win.close();
    window.print();
});