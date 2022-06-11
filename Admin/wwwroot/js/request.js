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

$("#errorBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Request");
});

$("#successBtn").on('click', function () {
    window.location.replace(BASEPATH + "/Request");
});


function viewReport(reportedByUser,
    reportedUser,
    reason,
    reportedDate,
    status,
    reportedByUserId,
    reportedUserId,
    type) {
    $('#reportedBy').text('Reported by: ' + reportedByUser);
    $('#reportedUser').text('Reported User :  ' + reportedUser);
    let date = transform(reportedDate);
    let time = new Date(reportedDate).toLocaleTimeString();
    $('#reportedDate').text('Date & Time Reported : ' + date + ', ' + time);
    $('#Reasons').text(reason);
    console.log('test');
    $('#Status').val(status).change();
    $('#ReportedByUserId').val(reportedByUserId);
    $('#ReportedUserId').val(reportedUserId);
    $('#Status').val(status)
    $('#type').val(type)
    $('#reqId').val(reportedUserId)
}

function transform(reportedDate) {
    if (reportedDate === '') return null;
    var dateMomentObject = moment(new Date(reportedDate)).format('LL');

    return dateMomentObject;
}

function deleteReport() {
    console.log('test');
    let formData = new FormData();
    formData.ReportedByUserId = $('#ReportedByUserId').val();
    formData.ReportedUserId = $('#ReportedUserId').val();
    $.ajax({
        url: "Request/DeleteReport",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(formData),
        dataType: 'json',
        success: function (result) {
            if (result.message == 'Invalid Object!') {
                $('#GenericModalMsg').html('There is an error updating the report');

                $("#GenericModal").modal('show');

                $("#errorIcon").show();
                $("#errorBtn").show();
            }
            else {
                $('#userModal').modal('hide');
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
}

function retainReposrtListDataTablePages() {
    let SESSION_PRODUCT_PAGES = 'SESSION_PRODUCT_PAGES';
    let SESSION_PRODUCT_PAGE = 'SESSION_PRODUCT_PAGE';
    let SESSION_PRODUCT_PAGE_LENGTH = 'SESSION_PRODUCT_PAGE_LENGTH';

    let pageLength = 10;
    let currentPageLength = localStorage.getItem(SESSION_PRODUCT_PAGE_LENGTH);
    if (currentPageLength) {
        pageLength = +currentPageLength;
    }

    var dtMainList = $('#dtMainListReportList').DataTable({
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

    $('#dtMainListReportList').on('page.dt', function () {
        var info = dtMainList.page.info();
        localStorage.setItem(SESSION_PRODUCT_PAGES, info.pages);
        localStorage.setItem(SESSION_PRODUCT_PAGE, info.page);
    });

    $('#dtMainListReportList').on('length.dt', function (e, settings, len) {
        localStorage.setItem(SESSION_PRODUCT_PAGE_LENGTH, len);
    });

    let currentPage = localStorage.getItem(SESSION_PRODUCT_PAGE);
    if (currentPage) {
        dtMainList.page(+currentPage).draw('page');
    }

    addExtraButtons();
    $('#dtMainListReportList').on("draw.dt", function (e) {
        addExtraButtons();
    });



    function addExtraButtons() {

        $("#dtMainListReportList_paginate .first").after("<li class='paginate_button page-item'><a class='page-link quick_previous'><<</a></li>");
        $("#dtMainListReportList_paginate .last").before("<li class='paginate_button page-item'><a class='page-link quick_next'>>></a></li>");
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

function retainUserRequestListDataTablePages() {
    let SESSION_PRODUCT_PAGES = 'SESSION_PRODUCT_PAGES';
    let SESSION_PRODUCT_PAGE = 'SESSION_PRODUCT_PAGE';
    let SESSION_PRODUCT_PAGE_LENGTH = 'SESSION_PRODUCT_PAGE_LENGTH';

    let pageLength = 10;
    let currentPageLength = localStorage.getItem(SESSION_PRODUCT_PAGE_LENGTH);
    if (currentPageLength) {
        pageLength = +currentPageLength;
    }

    var dtMainListUserRequest = $('#dtMainListUserRequests').DataTable({
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

    $('#dtMainListUserRequest').on('page.dt', function () {
        var info = dtMainListUserRequest.page.info();
        localStorage.setItem(SESSION_PRODUCT_PAGES, info.pages);
        localStorage.setItem(SESSION_PRODUCT_PAGE, info.page);
    });

    $('#dtMainListUserRequest').on('length.dt', function (e, settings, len) {
        localStorage.setItem(SESSION_PRODUCT_PAGE_LENGTH, len);
    });

    let currentPage = localStorage.getItem(SESSION_PRODUCT_PAGE);
    if (currentPage) {
        dtMainListUserRequest.page(+currentPage).draw('page');
    }

    addExtraButtons();
    $('#dtMainListUserRequest').on("draw.dt", function (e) {
        addExtraButtons();
    });



    function addExtraButtons() {

        $("#dtMainListUserRequests_paginate .first").after("<li class='paginate_button page-item'><a class='page-link quick_previous'><<</a></li>");
        $("#dtMainListUserRequests_paginate .last").before("<li class='paginate_button page-item'><a class='page-link quick_next'>>></a></li>");
        var currentPage = dtMainListUserRequest.page.info();
        if (currentPage.pages - 1 == currentPage.page) {
            $(".quick_next").addClass("disabled")
        } else if (currentPage.page == 0) {
            $(".quick_previous").addClass("disabled")
        }

        $(".quick_next").on("click", quickNext);
        $(".quick_previous").on("click", quickPrevious);

        function quickNext(e) {

            var pageToGoTo = (currentPage.page + 2) >= currentPage.pages ? currentPage.pages - 1 : (currentPage.page + 2);
            dtMainListUserRequest.page(pageToGoTo).draw(false);
        }

        function quickPrevious(e) {

            var pageToGoTo = (currentPage.page - 2) <= 0 ? 0 : (currentPage.page - 2);
            dtMainListUserRequest.page(pageToGoTo).draw(false);
        }
    }
}

function retainCoachRequestListDataTablePages() {
    let SESSION_PRODUCT_PAGES = 'SESSION_PRODUCT_PAGES';
    let SESSION_PRODUCT_PAGE = 'SESSION_PRODUCT_PAGE';
    let SESSION_PRODUCT_PAGE_LENGTH = 'SESSION_PRODUCT_PAGE_LENGTH';

    let pageLength = 10;
    let currentPageLength = localStorage.getItem(SESSION_PRODUCT_PAGE_LENGTH);
    if (currentPageLength) {
        pageLength = +currentPageLength;
    }

    var dtMainListCoachRequest = $('#dtMainListCoachRequest').DataTable({
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

    $('#dtMainListCoachRequest').on('page.dt', function () {
        var info = dtMainListCoachRequest.page.info();
        localStorage.setItem(SESSION_PRODUCT_PAGES, info.pages);
        localStorage.setItem(SESSION_PRODUCT_PAGE, info.page);
    });

    $('#dtMainListCoachRequest').on('length.dt', function (e, settings, len) {
        localStorage.setItem(SESSION_PRODUCT_PAGE_LENGTH, len);
    });

    let currentPage = localStorage.getItem(SESSION_PRODUCT_PAGE);
    if (currentPage) {
        dtMainListCoachRequest.page(+currentPage).draw('page');
    }

    addExtraButtons();
    $('#dtMainListCoachRequest').on("draw.dt", function (e) {
        addExtraButtons();
    });



    function addExtraButtons() {

        $("#dtMainListCoachRequest_paginate .first").after("<li class='paginate_button page-item'><a class='page-link quick_previous'><<</a></li>");
        $("#dtMainListCoachRequest_paginate .last").before("<li class='paginate_button page-item'><a class='page-link quick_next'>>></a></li>");
        var currentPage = dtMainListCoachRequest.page.info();
        if (currentPage.pages - 1 == currentPage.page) {
            $(".quick_next").addClass("disabled")
        } else if (currentPage.page == 0) {
            $(".quick_previous").addClass("disabled")
        }

        $(".quick_next").on("click", quickNext);
        $(".quick_previous").on("click", quickPrevious);

        function quickNext(e) {

            var pageToGoTo = (currentPage.page + 2) >= currentPage.pages ? currentPage.pages - 1 : (currentPage.page + 2);
            dtMainListCoachRequest.page(pageToGoTo).draw(false);
        }

        function quickPrevious(e) {

            var pageToGoTo = (currentPage.page - 2) <= 0 ? 0 : (currentPage.page - 2);
            dtMainListCoachRequest.page(pageToGoTo).draw(false);
        }
    }
}