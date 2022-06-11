function getUserCount(users, selectedYear) {
    let selectedGroupYear = [];
    for (var i = 0; i < users.length; i++) {
        if (new Date(users[i].createdDate).getFullYear() == selectedYear) {
            selectedGroupYear.push(users[i]);
        }
    }

    $('#new-users-year').text(selectedGroupYear.length);
}

function renderStatistics(dataObject, selectedYear, isUpdate) {
    $('#myChart').remove();
    $('#chart-year').append(' <canvas id="myChart" style="width:100%;max-width:700px"></canvas>');
    $('#total-year-count-object').text(dataObject.facilityPlayers.length);
    $('#total-year-count-label').text('Total Active Players');
    $('#total-new-year-label').text('Total New Players');
    getAllActiveUsersCount(dataObject.facilityPlayers);
    getUserCount(dataObject.facilityPlayerGroupLastSeven, selectedYear);
    let playerCount = dataObject.facilityPlayerGroup;
    let facilitiesCount = dataObject.facilitiesGroup;
    let coachesCount = dataObject.coachesGroup;
    let playerDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    let facilityDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    let coachDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    for (var i = 0; i < playerCount.length; i++) {
        if (parseInt(selectedYear) == playerCount[i].year) {
            for (var j = 0; j <= playerDataStat.length; j++) {
                if (playerCount[i].month - 1 === j) {
                    playerDataStat[j] = playerCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < facilitiesCount.length; i++) {
        if (parseInt(selectedYear) == facilitiesCount[i].year) {
            for (var j = 0; j <= facilityDataStat.length; j++) {
                if (facilitiesCount[i].month - 1 === j) {
                    facilityDataStat[j] = facilitiesCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < coachesCount.length; i++) {
        if (parseInt(selectedYear) == coachesCount[i].year) {
            for (var j = 0; j < coachDataStat.length; j++) {
                if (coachesCount[i].month - 1 === j) {
                    coachDataStat[j] = coachesCount[i].objectCount;
                }
            }
        }
    }

    const months = Array.from({ length: 12 }, (item, i) => {
        return new Date(0, i).toLocaleString('en-GB', { month: 'long' })
    });
    const labels = months;
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Players',
                data: playerDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Coach',
                data: coachDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
            },
            {
                label: 'Facilities',
                data: facilityDataStat,
                fill: false,
                backgroundColor: 'rgb(255,196,0)',
                borderColor: 'rgb(255,196,0)',
            }
        ]
    };
    
    
        new Chart("myChart", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }

                }
            }
        });
    
}

$('#pills-lastmonth-tab').click(function () {
    $('#this-year-stats').attr('hidden', true);
    $('#last-month-stats').attr('hidden', false);
    $('#this-month-stats').attr('hidden', true);
    $('#last-sevenday-stats').attr('hidden', true);
});

$('#pills-year-tab').click(function () {
    $('#this-year-stats').attr('hidden', false);
    $('#last-month-stats').attr('hidden', true);
    $('#this-month-stats').attr('hidden', true);
    $('#last-sevenday-stats').attr('hidden', true);
});

$('#pills-thismonth-tab').click(function () {
    $('#this-year-stats').attr('hidden', true);
    $('#last-month-stats').attr('hidden', true);
    $('#this-month-stats').attr('hidden', false);
    $('#last-sevenday-stats').attr('hidden', true);
});

$('#pills-7days-tab').click(function () {
    $('#this-year-stats').attr('hidden', true);
    $('#last-month-stats').attr('hidden', true);
    $('#this-month-stats').attr('hidden', true);
    $('#last-sevenday-stats').attr('hidden', false);
});

function renderLastMonthStatistics(dataObject, selectedYear, isUpdate) {
    let playerCount = dataObject.facilityPlayerGroupLastMonth;
    let facilitiesCount = dataObject.facilitiesGroupLastMonth;
    let coachesCount = dataObject.coachesGroupLastMonth;
    let playerDataStat = [0, 0, 0, 0];
    let facilityDataStat = [0, 0, 0, 0];
    let coachDataStat = [0, 0, 0, 0];
    let newUserCount = 0;
    for (var i = 0; i < playerCount.length; i++) {     
        if (parseInt(selectedYear) == playerCount[i].year) {
            for (var j = 0; j <= playerDataStat.length; j++) {
                let date = new Date();
                let lastMonth = date.getMonth() - 1;
                if (lastMonth == playerCount[i].lastMonth - 1 && playerCount[i].weekNumber - 1 == j) {
                    playerDataStat[j] = playerCount[i].objectCount;
                    newUserCount += playerCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < facilitiesCount.length; i++) {
        if (parseInt(selectedYear) == facilitiesCount[i].year) {
            for (var j = 0; j <= facilityDataStat.length; j++) {
                let date = new Date();
                let lastMonth = date.getMonth() - 1;
                if (lastMonth == facilitiesCount[i].lastMonth - 1 && facilitiesCount[i].weekNumber - 1 == j) {
                    facilityDataStat[j] = facilitiesCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < coachesCount.length; i++) {
        if (parseInt(selectedYear) == coachesCount[i].year) {
            for (var j = 0; j < coachDataStat.length; j++) {
                let date = new Date();
                let lastMonth = date.getMonth() - 1;
                if (lastMonth == coachesCount[i].lastMonth - 1 && coachesCount[i].weekNumber - 1 == j) {
                    coachDataStat[j] = coachesCount[i].objectCount;
                    newUserCount += coachesCount[i].objectCount;
                }
            }
        }
    }

    $('#total-users-last-month').text(newUserCount);

    const labels = ["Week 1", "Week 2", "Week 3", "Week 4"];
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Players',
                data: playerDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
                tension: 0.8
            },
            {
                label: 'Coach',
                data: coachDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
                tension: 0.8
            },
            {
                label: 'Facilities',
                data: facilityDataStat,
                fill: false,
                backgroundColor: 'rgb(255,196,0)',
                borderColor: 'rgb(255,196,0)',
                tension: 0.8
            }
        ]
    };

    if (isUpdate) {
        $('#myChartLastMonth').remove();
        $('#chart-last-month').append(' <canvas id="myChartLastMonth"></canvas>');
        new Chart("myChartLastMonth", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]

                }
            }
        });
    }
    else {
        new Chart("myChartLastMonth", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]
                }
            }
        });
    }
}

function renderThisMonthStatistics(dataObject, selectedYear, isUpdate) {
    let playerCount = dataObject.facilityPlayerGroupThisMonth;
    let facilitiesCount = dataObject.facilitiesGroupThisMonth;
    let coachesCount = dataObject.coachesGroupThisMonth;
    let playerDataStat = [0, 0, 0, 0];
    let facilityDataStat = [0, 0, 0, 0];
    let coachDataStat = [0, 0, 0, 0];
    let newUserCount = 0;
    for (var i = 0; i < playerCount.length; i++) {
        if (parseInt(selectedYear) == playerCount[i].year) {
            for (var j = 0; j <= playerDataStat.length; j++) {
                let date = new Date();
                let thisMonth = date.getMonth();
                if (thisMonth == playerCount[i].lastMonth -1 && playerCount[i].weekNumber - 1 == j) {
                    playerDataStat[j] = playerCount[i].objectCount;
                    newUserCount += playerCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < facilitiesCount.length; i++) {
        if (parseInt(selectedYear) == facilitiesCount[i].year) {
            for (var j = 0; j <= facilityDataStat.length; j++) {
                let date = new Date();
                let thisMonth = date.getMonth();
                if (thisMonth == facilitiesCount[i].lastMonth -1 && facilitiesCount[i].weekNumber - 1 == j) {
                    facilityDataStat[j] = facilitiesCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < coachesCount.length; i++) {
        if (parseInt(selectedYear) == coachesCount[i].year) {
            for (var j = 0; j < coachDataStat.length; j++) {
                let date = new Date();
                let thisMonth = date.getMonth();
                if (thisMonth == coachesCount[i].lastMonth - 1 && coachesCount[i].weekNumber - 1 == j) {
                    coachDataStat[j] = coachesCount[i].objectCount;
                    newUserCount += coachesCount[i].objectCount;
                }
            }
        }
    }

    $('#total-users-this-month').text(newUserCount);

    const labels = ["Week 1", "Week 2", "Week 3", "Week 4"];
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Players',
                data: playerDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
                tension: 0.8
            },
            {
                label: 'Coach',
                data: coachDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
                tension: 0.8
            },
            {
                label: 'Facilities',
                data: facilityDataStat,
                fill: false,
                backgroundColor: 'rgb(255,196,0)',
                borderColor: 'rgb(255,196,0)',
                tension: 0.8
            }
        ]
    };

    if (isUpdate) {
        $('#myChartThisMonth').remove();
        $('#chart-this-month').append(' <canvas id="myChartThisMonth"></canvas>');
        new Chart("myChartThisMonth", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]

                }
            }
        });
    }
    else {
        new Chart("myChartThisMonth", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]
                }
            }
        });
    }
}

function renderLastSevenDaysStatistics(dataObject, selectedYear, isUpdate) {
    let playerCount = dataObject.facilityPlayerGroupLastSeven;
    let facilitiesCount = dataObject.facilitiesGroupLastSeven;
    let coachesCount = dataObject.coachesGroupLastSeven;
    let playerDataStat = [0, 0, 0, 0, 0, 0, 0];
    let facilityDataStat = [0, 0, 0, 0, 0, 0, 0];
    let coachDataStat = [0, 0, 0, 0, 0, 0, 0];
    let today = new Date();
    let lastSevenDays = 7;
    let labelSevenDays = [];
    for (var i = 0; i < lastSevenDays; i++) {
        var currentDate = new Date();
        currentDate.setDate(today.getDate() - i);
        var labelDate = currentDate.getDate();
        labelSevenDays.push(labelDate);
    }
    let newUserCount = 0;
    for (var i = 0; i < playerCount.length; i++) {
        if (parseInt(selectedYear) == playerCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (playerCount[i].day == labelSevenDays[j]) {
                    playerDataStat[j] = playerCount[i].objectCount;
                    newUserCount += playerCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < facilitiesCount.length; i++) {
        if (parseInt(selectedYear) == facilitiesCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (facilitiesCount[i].day == labelSevenDays[j]) {
                    facilityDataStat[j] = facilitiesCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < coachesCount.length; i++) {
        if (parseInt(selectedYear) == coachesCount[i].year) {
            for (var j = 0; j < labelSevenDays.length; j++) {
                if (coachesCount[i].day == labelSevenDays[j]) {
                    coachDataStat[j] = coachesCount[i].objectCount;
                    newUserCount += coachesCount[i].objectCount;
                }
            }
        }
    }

    $('#total-users-last-7days').text(newUserCount);

  
    const labels = labelSevenDays;
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Players',
                data: playerDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
                tension: 0.8
            },
            {
                label: 'Coach',
                data: coachDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
                tension: 0.8
            },
            {
                label: 'Facilities',
                data: facilityDataStat,
                fill: false,
                backgroundColor: 'rgb(255,196,0)',
                borderColor: 'rgb(255,196,0)',
                tension: 0.8
            }
        ]
    };

    if (isUpdate) {
        $('#myChartLastSevenDays').remove();
        $('#chart-last-seven-days').append(' <canvas id="myChartLastSevenDays"></canvas>');
        new Chart("myChartLastSevenDays", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]

                }
            }
        });
    }
    else {
        new Chart("myChartLastSevenDays", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'User Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]
                }
            }
        });
    }
}

function renderBookingsStatisticsYear(dataObject, selectedYear) {
    $('#myChart').remove();
    $('#chart-year').append(' <canvas id="myChart" style="width:100%;max-width:700px"></canvas>');
    $('#total-year-count-label').text('Total Bookings');
    $('#total-new-year-label').text('Total Play Booking');
    /*getAllActiveBookingsCount(dataObject.bookings);*/

    /*getBookingsCountYear(parseInt(selectedYear), dataObject.playBookingsLastSeven);*/

    $('#total-active-label').text('Total Train Booking');

    let playBookingsCount = dataObject.playBookingsThisYear;
    let trainBookingsCount = dataObject.trainBookingsThisYear;
    let playBookingDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    let trainBookingDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    let bookingsCountThisYear = 0;

    for (var i = 0; i < playBookingsCount.length; i++) {
        if (parseInt(selectedYear) == playBookingsCount[i].year) {
            for (var j = 0; j <= playBookingDataStat.length; j++) {
                if (playBookingsCount[i].month - 1 === j) {
                    playBookingDataStat[j] = playBookingsCount[i].objectCount;
                    bookingsCountThisYear += playBookingsCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < trainBookingsCount.length; i++) {
        if (parseInt(selectedYear) == trainBookingsCount[i].year) {
            for (var j = 0; j <= trainBookingDataStat.length; j++) {
                if (trainBookingsCount[i].month - 1 === j) {
                    trainBookingDataStat[j] = trainBookingsCount[i].objectCount;
                    bookingsCountThisYear += trainBookingsCount[i].objectCount
                }
            }
        }
    }

    $('#total-year-count-object').text(bookingsCountThisYear);

    const months = Array.from({ length: 12 }, (item, i) => {
        return new Date(0, i).toLocaleString('en-GB', { month: 'long' })
    });
    const data = {
        labels: months,
        datasets: [
            {
                label: 'Play',
                data: playBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Train',
                data: trainBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
            }
        ]
    };

    new Chart("myChart", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Booking Reports'
            },
            scales: {
                yAxes: [{
                    stacked: true,
                    beginAtZero: true
                    
                    }]
            }
        }
    });
}

//function getBookingsCountYear(selectedYear, bookings) {
//    let selectedGroupYear = [];
//    for (var i = 0; i < bookings.length; i++) {
//        if (new Date(bookings[i].createdDate).getFullYear() == selectedYear && bookings[i].isEnabled == true) {
//            selectedGroupYear.push(bookings[i]);
//        }
//    }

//    $('#new-users-year').text(selectedGroupYear.length);
//}

function getAllActiveBookingsCount(bookings) {
    let selectedGroupYear = [];
    for (var i = 0; i < bookings.length; i++) {
        if (bookings[i].isEnabled == true) {
            selectedGroupYear.push(bookings[i]);
        }
    }

    $('#total-active').text(selectedGroupYear.length);
    $('#total-active-label').text('Total Train Booking');
}

function getAllActiveUsersCount(users) {
    let selectedGroupYear = [];
    for (var i = 0; i < users.length; i++) {
        if (users[i].isEnabled == true) {
            selectedGroupYear.push(users[i]);
        }
    }

    $('#total-active').text(selectedGroupYear.length);
    $('#total-active-label').text('Average Number Transaction per Player');
}

function renderLastMonthBookingsStatistics(dataObject, selectedYear) {
    $('#myChartLastMonth').remove();
    $('#chart-last-month').append(' <canvas id="myChartLastMonth"></canvas>');
    let playBookingCount = dataObject.playBookingsLastMonth;
    let trainBookingCount = dataObject.trainBookingsLastMonth;
    let playBookingDataStat = [0, 0, 0, 0];
    let trainBookingDataStat = [0, 0, 0, 0];
    let newBookingCount = 0;
    for (var i = 0; i < playBookingCount.length; i++) {
        if (parseInt(selectedYear) == playBookingCount[i].year) {
            for (var j = 0; j <= playBookingDataStat.length; j++) {
                let date = new Date();
                let lastMonth = date.getMonth() - 1;
                if (lastMonth == playBookingCount[i].lastMonth - 1 && playBookingCount[i].weekNumber - 1 == j) {
                    playBookingDataStat[j] = playBookingCount[i].objectCount;
                    newBookingCount += playBookingCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < trainBookingCount.length; i++) {
        if (parseInt(selectedYear) == trainBookingCount[i].year) {
            for (var j = 0; j <= trainBookingDataStat.length; j++) {
                let date = new Date();
                let lastMonth = date.getMonth() - 1;
                if (lastMonth == trainBookingCount[i].lastMonth - 1 && trainBookingCount[i].weekNumber - 1 == j) {
                    trainBookingDataStat[j] = trainBookingCount[i].objectCount;
                    newBookingCount += trainBookingCount[i].objectCount;
                }
            }
        }
    }
  
    $('#total-users-last-month').text(newBookingCount);

    const data = {
        labels: ["Week 1", "Week 2", "Week 3", "Week 4"],
        datasets: [
            {
                label: 'Play',
                data: playBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Train',
                data: trainBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
            }
        ]
    };

    new Chart("myChartLastMonth", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Booking Reports'
            },
            scales: {
                yAxes: [{
                    stacked: true
                }]

            }
        }
    });
}

function renderCurrentMonthBookingsStatistics(dataObject, selectedYear) {
    $('#myChartThisMonth').remove();
    $('#chart-this-month').append(' <canvas id="myChartThisMonth"></canvas>');
    let playBookingCount = dataObject.playBookingsCurrentMonth;
    let trainBookingCount = dataObject.trainBookingsCurrentMonth; 
    let playBookingDataStat = [0, 0, 0, 0];
    let trainBookingDataStat = [0, 0, 0, 0];
    let newBookingCount = 0;
    for (var i = 0; i < playBookingCount.length; i++) {
        if (parseInt(selectedYear) == playBookingCount[i].year) {
            for (var j = 0; j <= playBookingDataStat.length; j++) {
                let date = new Date();
                let thisMonth = date.getMonth();
                if (thisMonth == playBookingCount[i].lastMonth - 1 && playBookingCount[i].weekNumber - 1 == j) {
                    playBookingDataStat[j] = playBookingCount[i].objectCount;
                    newBookingCount += playBookingCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < trainBookingCount.length; i++) {
        if (parseInt(selectedYear) == trainBookingCount[i].year) {
            for (var j = 0; j <= trainBookingDataStat.length; j++) {
                let date = new Date();
                let thisMonth = date.getMonth();
                if (thisMonth == trainBookingCount[i].lastMonth - 1 && trainBookingCount[i].weekNumber - 1 == j) {
                    trainBookingDataStat[j] = trainBookingCount[i].objectCount;
                    newBookingCount += trainBookingCount[i].objectCount;
                }
            }
        }
    }

    $('#total-users-this-month').text(newBookingCount);

    const data = {
        labels: ["Week 1", "Week 2", "Week 3", "Week 4"],
        datasets: [
            {
                label: 'Play',
                data: playBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Train',
                data: trainBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
            }
        ]
    };

    new Chart("myChartThisMonth", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Booking Reports'
            },
            scales: {
                yAxes: [{
                    stacked: true
                }]
            }
        }
    });
}

function renderLastSevenDaysBookingsStatistics(dataObject, selectedYear) {
    $('#myChartLastSevenDays').remove();
    $('#chart-last-seven-days').append(' <canvas id="myChartLastSevenDays"></canvas>');
    let playBookingCount = dataObject.playBookingsLastSeven;
    let trainBookingCount = dataObject.trainBookingsLastSeven;

    let playBookingDataStat = [0, 0, 0, 0, 0, 0, 0];
    let trainBookingDataStat = [0, 0, 0, 0, 0, 0, 0];
    let today = new Date();
    let lastSevenDays = 7;
    let labelSevenDays = [];
    for (var i = 0; i < lastSevenDays; i++) {
        var currentDate = new Date();
        currentDate.setDate(today.getDate() - i);
        var labelDate = currentDate.getDate();
        labelSevenDays.push(labelDate);
    }
    let newBookingCount = 0;
    let playLastSeven = 0;
    let trainLastSeven = 0;
    for (var i = 0; i < playBookingCount.length; i++) {
        if (parseInt(selectedYear) == playBookingCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (playBookingCount[i].day == labelSevenDays[j]) {
                    playBookingDataStat[j] = playBookingCount[i].objectCount;
                    playLastSeven += playBookingCount[i].objectCount;
                }
            }
        }
    }

    for (var i = 0; i < trainBookingCount.length; i++) {
        if (parseInt(selectedYear) == trainBookingCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (trainBookingCount[i].day == labelSevenDays[j]) {
                    trainBookingDataStat[j] = trainBookingCount[i].objectCount;
                    trainLastSeven += trainBookingCount[i].objectCount;
                }
            }
        }
    }

    $('#new-users-year').text(playLastSeven);
    $('#total-active').text(trainLastSeven);

    $('#total-users-last-7days').text(newBookingCount);

    const labels = labelSevenDays;
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Play',
                data: playBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Train',
                data: trainBookingDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
            }
        ]
    };

    new Chart("myChartLastSevenDays", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Booking Reports'
            },
            scales: {
                yAxes: [{
                    stacked: true
                }]
            }
        }
    });
}

function renderPaymentStatistics(dataObject, selectedYear, isUpdate) {
    $('#myChart').remove();
    $('#chart-year').append(' <canvas id="myChart" style="width:100%;max-width:700px"></canvas>');
    $('#total-year-count-object').text(dataObject.totalRevenue + " AED");
    $('#total-year-count-label').text('Total Revenue');
    $('#total-new-year-label').text('Total Play Revenue');
    $('#total-active-label').text('Total Train Revenue');
    $('#total-active').text(dataObject.totalTrainRevenue + " AED");
    //getAllActiveUsersCount(dataObject.facilityPlayers);
    //getUserCount(dataObject.facilityPlayerGroupLastSeven, selectedYear);
    let trainPaymentCount = dataObject.trainPaymentsGroupYear;
    //let facilitiesCount = dataObject.facilitiesGroup;
    //let coachesCount = dataObject.coachesGroup;
    let trainDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    let playDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    for (var i = 0; i < trainPaymentCount.length; i++) {
        if (parseInt(selectedYear) == trainPaymentCount[i].year) {
            for (var j = 0; j <= trainDataStat.length; j++) {
                if (trainPaymentCount[i].month - 1 === j) {
                    trainDataStat[j] = trainPaymentCount[i].totalAmount;
                }
            }
        }
    }

    //for (var i = 0; i < facilitiesCount.length; i++) {
    //    if (parseInt(selectedYear) == facilitiesCount[i].year) {
    //        for (var j = 0; j <= facilityDataStat.length; j++) {
    //            if (facilitiesCount[i].month - 1 === j) {
    //                facilityDataStat[j] = facilitiesCount[i].objectCount;
    //            }
    //        }
    //    }
    //}

    const months = Array.from({ length: 12 }, (item, i) => {
        return new Date(0, i).toLocaleString('en-GB', { month: 'long' })
    });
    const labels = months;
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Train',
                data: trainDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Play',
                data: playDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
            }
        ]
    };


    new Chart("myChart", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Income Reports'
            },
            scales: {
                y: {
                    beginAtZero: true
                }

            }
        }
    });
}

function renderPaymentLastMonthStatistics(dataObject, selectedYear, isUpdate) {
    $('#myChartLastMonth').remove();
    $('#chart-last-month').append(' <canvas id="myChartLastMonth"></canvas>');
    let trainCount = dataObject.trainPaymentsGroupLastMonth;
    let playCount = dataObject.playPaymentsGroupLastMonth;
    let trainDataStat = [0, 0, 0, 0];
    let playDataStat = [0, 0, 0, 0];
    let newUserCount = 0;
    for (var i = 0; i < trainCount.length; i++) {
        if (parseInt(selectedYear) == trainCount[i].year) {
            for (var j = 0; j <= trainDataStat.length; j++) {
                let date = new Date();
                let lastMonth = date.getMonth() - 1;
                if (lastMonth == trainCount[i].lastMonth - 1 && trainCount[i].weekNumber - 1 == j) {
                    trainDataStat[j] = trainCount[i].totalAmount;
                    
                }
            }
        }
    }

    //for (var i = 0; i < facilitiesCount.length; i++) {
    //    if (parseInt(selectedYear) == facilitiesCount[i].year) {
    //        for (var j = 0; j <= facilityDataStat.length; j++) {
    //            let date = new Date();
    //            let lastMonth = date.getMonth() - 1;
    //            if (lastMonth == facilitiesCount[i].lastMonth - 1 && facilitiesCount[i].weekNumber - 1 == j) {
    //                facilityDataStat[j] = facilitiesCount[i].objectCount;
    //            }
    //        }
    //    }
    //}

/*    $('#total-users-last-month').text(newUserCount);*/

    const labels = ["Week 1", "Week 2", "Week 3", "Week 4"];
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Train',
                data: trainDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
                tension: 0.8
            },
            {
                label: 'Play',
                data: playDataStat,
                fill: false,
                backgroundColor: 'rgb(49,150,158)',
                borderColor: 'rgb(49,150,158)',
                tension: 0.8
            }
        ]
    };

    if (isUpdate) {
        $('#myChartLastMonth').remove();
        $('#chart-last-month').append(' <canvas id="myChartLastMonth"></canvas>');
        new Chart("myChartLastMonth", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'Income Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]

                }
            }
        });
    }
    else {
        new Chart("myChartLastMonth", {
            type: 'line',
            data: data,
            options: {
                title: {
                    display: true,
                    text: 'Income Reports'
                },
                scales: {
                    yAxes: [{
                        stacked: true
                    }]
                }
            }
        });
    }
}

function renderPaymentThisMonthStatistics(dataObject, selectedYear) {
    $('#myChartThisMonth').remove();
    $('#chart-this-month').append(' <canvas id="myChartThisMonth"></canvas>');
    let trainCount = dataObject.trainPaymentsGroupThisMonth;
    let playCount = dataObject.playPaymentsGroupThisMonth
    let trainDataStat = [0, 0, 0, 0];
    let playDataStat = [0, 0, 0, 0];
    let newBookingCount = 0;
    for (var i = 0; i < trainCount.length; i++) {
        if (parseInt(selectedYear) == trainCount[i].year) {
            for (var j = 0; j <= trainDataStat.length; j++) {
                let date = new Date();
                let thisMonth = date.getMonth();
                if (thisMonth == trainCount[i].lastMonth - 1 && trainCount[i].weekNumber - 1 == j) {
                    trainDataStat[j] = trainCount[i].totalAmount;
/*                    newBookingCount += bookingCount[i].objectCount;*/
                }
            }
        }
    }



    const data = {
        labels: ["Week 1", "Week 2", "Week 3", "Week 4"],
        datasets: [
            {
                label: 'Train',
                data: trainDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            },
            {
                label: 'Play',
                data: playDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
            }
        ]
    };

    new Chart("myChartThisMonth", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Income Reports'
            },
            scales: {
                yAxes: [{
                    stacked: true
                }]
            }
        }
    });
}

function renderPaymentLastSevenDaysStatistics(dataObject, selectedYear) {
    $('#myChartLastSevenDays').remove();
    $('#chart-last-seven-days').append(' <canvas id="myChartLastSevenDays"></canvas>');
    let trainCount = dataObject.trainPaymentLastSevenDays;
    let playCount = dataObject.playPaymentLastSevenDays
    let trainDataStat = [0, 0, 0, 0, 0, 0, 0];
    let playDataStat = [0, 0, 0, 0, 0, 0, 0];
    let today = new Date();
    let lastSevenDays = 7;
    let labelSevenDays = [];
    for (var i = 0; i < lastSevenDays; i++) {
        var currentDate = new Date();
        currentDate.setDate(today.getDate() - i);
        var labelDate = currentDate.getDate();
        labelSevenDays.push(labelDate);
    }
    let newBookingCount = 0;
    for (var i = 0; i < trainCount.length; i++) {
        if (parseInt(selectedYear) == trainCount[i].year) {
            for (var j = 0; j <= labelSevenDays.length; j++) {
                if (trainCount[i].day == labelSevenDays[j]) {
                    trainDataStat[j] = trainCount[i].totalAmount;
                    /*newBookingCount += bookingCount[i].objectCount;*/
                }
            }
        }
    }

/*    $('#total-users-last-7days').text(newBookingCount);*/

    const labels = labelSevenDays;
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Train',
                data: trainDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
                tension: 0.8
            },
            {
                label: 'Play',
                data: playDataStat,
                fill: false,
                backgroundColor: 'rgb(65,139,189)',
                borderColor: 'rgb(65,139,189)',
                tension: 0.8
            }
        ]
    };

    new Chart("myChartLastSevenDays", {
        type: 'line',
        data: data,
        options: {
            title: {
                display: true,
                text: 'Income Reports'
            },
            scales: {
                yAxes: [{
                    stacked: true
                }]
            }
        }
    });
}
