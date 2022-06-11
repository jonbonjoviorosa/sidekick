function populatePlayerGraph(dataObject) {
    let playerCount = dataObject;
    let playerDataStat = [0, 0, 0, 0, 0, 0, 0];
    let today = new Date();
    let lastSevenDays = 7;
    let labelSevenDays = [];
    for (var i = 0; i < lastSevenDays; i++) {
        var currentDate = new Date();
        currentDate.setDate(today.getDate() - i);
        var labelDate = currentDate.getDate();
        labelSevenDays.push(labelDate);
    }

    let selectedYear = new Date();
    let newUserCount = 0;
    let newPlayerToday = 0;
    for (var i = 0; i < playerCount.length; i++) {
        if (parseInt(selectedYear.getFullYear()) == playerCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (playerCount[i].day == labelSevenDays[j]) {
                    playerDataStat[j] = playerCount[i].objectCount;
                    newUserCount += playerCount[i].objectCount;
                }

               
            }
        }

        if (playerCount[i].day == selectedYear.getDate()) {
            newPlayerToday = playerCount[i].objectCount;
        }
    }
    let playerPercentage = (newUserCount / 7) * 100
    $('#new-player-today').text(newPlayerToday);
    $('#player-percentage').text('+' + parseInt(playerPercentage) + '%');
    var xValues = labelSevenDays;
    var yValues = playerDataStat;

    new Chart("chart_3", {
        type: "line",
        data: {
            labels: xValues,
            datasets: [{
                fill: true,
                backgroundColor: "rgba(188,31,45,.5)",
                borderColor: "rgba(188,31,45,0.1)",
                data: yValues
            }]
        },
        options: {
            legend: { display: false },
            scales: {
                xAxes: [{
                    gridLines: {
                        drawOnChartArea: false
                    }
                }],
                yAxes: [{
                    gridLines: {
                        drawOnChartArea: false
                    }
                }],
            }
        }
    });
}

function populatePitchGraph(dataObject) {
    let pitchCount = dataObject;
    let pitchDataStat = [0, 0, 0, 0, 0, 0, 0];
    let today = new Date();
    let lastSevenDays = 7;
    let labelSevenDays = [];
    for (var i = 0; i < lastSevenDays; i++) {
        var currentDate = new Date();
        currentDate.setDate(today.getDate() - i);
        var labelDate = currentDate.getDate();
        labelSevenDays.push(labelDate);
    }

    let selectedYear = new Date();
    let newPitchCount = 0;
    let newPitchToday = 0;
    for (var i = 0; i < pitchCount.length; i++) {
        if (parseInt(selectedYear.getFullYear()) == pitchCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (pitchCount[i].day == labelSevenDays[j]) {
                    pitchDataStat[j] = pitchCount[i].objectCount;
                    newPitchCount += pitchCount[i].objectCount;
                }

                
            }
        }

        if (pitchCount[i].day == selectedYear.getDate()) {
            newPitchToday = pitchCount[i].objectCount;
        }
    }
    let pitchPercentage = (newPitchCount / 7) * 100
    $('#new-pitch-today').text(newPitchToday);
    $('#pitch-percentage').text('+' + parseInt(pitchPercentage) + '%');
    var xValues = labelSevenDays;
    var yValues = pitchDataStat;

    new Chart("chart_2", {
        type: "line",
        data: {
            labels: xValues,
            datasets: [{
                fill: true,
                backgroundColor: "rgba(255,196,0,.5)",
                borderColor: "rgba(255,196,0,0.1)",
                data: yValues
            }]
        },
        options: {
            legend: { display: false },
            scales: {
                xAxes: [{
                    gridLines: {
                        drawOnChartArea: false
                    }
                }],
                yAxes: [{
                    gridLines: {
                        drawOnChartArea: false
                    }
                }],
            }
        }
    });
}

function populateBookingsGraph(dataObject) {
    let bookingCount = dataObject;
    let bookingDataStat = [0, 0, 0, 0, 0, 0, 0];
    let today = new Date();
    let lastSevenDays = 7;
    let labelSevenDays = [];
    for (var i = 0; i < lastSevenDays; i++) {
        var currentDate = new Date();
        currentDate.setDate(today.getDate() - i);
        var labelDate = currentDate.getDate();
        labelSevenDays.push(labelDate);
    }

    let selectedYear = new Date();
    let newBookingCount = 0;
    let newBookingToday = 0;
    for (var i = 0; i < bookingCount.length; i++) {
        if (parseInt(selectedYear.getFullYear()) == bookingCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (bookingCount[i].day == labelSevenDays[j]) {
                    bookingDataStat[j] = bookingCount[i].objectCount;
                    newBookingCount += bookingCount[i].objectCount;
                }

                
            }
        }

        if (bookingCount[i].day == selectedYear.getDate()) {
            newBookingToday = bookingCount[i].objectCount;
        }
    }
    let bookingPercentage = (newBookingCount / 7) * 100
    $('#new-booking-today').text(newBookingToday);
    $('#booking-percentage').text('+' + parseInt(bookingPercentage) + '%');
    var xValues = labelSevenDays;
    var yValues = bookingDataStat;

    new Chart("chart_1", {
        type: "line",
        data: {
            labels: xValues,
            datasets: [{
                fill: true,
                backgroundColor: "rgba(83,212,143,.5)",
                borderColor: "rgba(83,212,143,.7)",
                data: yValues
            }]
        },
        options: {
            legend: { display: false },
            scales: {
                xAxes: [{
                    gridLines: {
                        drawOnChartArea: false
                    }
                }],
                yAxes: [{
                    gridLines: {
                        drawOnChartArea: false
                    }
                }],
            }
        }
    });
}

function populateNotifcations(facilityUser) {
    var facilityId = facilityUser.facilityUserInfo.facilityId;
    $.ajax({
        url: "GetNotifications?id=" + facilityId,
        type: "GET",
        contentType: 'application/json',
        data: facilityId,
        success: function (result) {
            if (result.payload.length != 0) {
                for (var i = 0; i < result.payload.length; i++) {
                    var date = result.payload[i].createdDate;
                    $('#populate-notifications').append(
                        '<div class="row mx-0 mb-3">' +

                        '<div class="col-3 px-0">' +
                        '<img src="' + result.payload[i].userImage + '" class="w-70 circle border-green">' +
                        '</div>' +
                        '<div class="col-9 px-0">' +
                        '<p class="medium pt-1 mb-0"><b>' + result.payload[i].name + '  </b>' + result.payload[i].notificationTitle + '</p>' +
                        '<p class="text-grey small mt-n1 mb-0">' + moment(new Date(date)).fromNow() + '</p></div>' +
                        '</div>');
                }
            }
            else {
                $('#populate-notifications').append(
                    '<div class="row mx-0 mb-0">' +
                    '<div class="col-12 px-0"><p class= "medium pt-1 mb-0">No Available Notificaions</p></div></div>');
                    
            }
            
            
        },
        failure: function (result) {
            console.log(result);
        }
    });
}