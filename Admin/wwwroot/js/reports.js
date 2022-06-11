
$('#end').val("");
$('#start').val("");

function renderStatistics(dataObject, dateToday, isUpdate) {
    $('#myChart').remove();
    console.log('test');
    let selectedYear = dateToday.getFullYear();
    $('#chart-year').append(' <canvas id="myChart" style="width:100%;max-width:700px"></canvas>');
    let playerCount = dataObject.facilityPlayerGroup;
    let playerDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    for (var i = 0; i < playerCount.length; i++) {
        if (parseInt(selectedYear) == playerCount[i].year) {
            for (var j = 0; j <= playerDataStat.length; j++) {
                if (playerCount[i].month - 1 === j) {
                    playerDataStat[j] = playerCount[i].objectCount;
                }
            }
        }
    }

    let facilitiesCount = dataObject.bookingsGroupByYear;
    let facilityDataStat = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    for (var i = 0; i < facilitiesCount.length; i++) {
        if (parseInt(selectedYear) == facilitiesCount[i].year) {
            for (var j = 0; j <= facilityDataStat.length; j++) {
                if (facilitiesCount[i].month - 1 === j) {
                    facilityDataStat[j] = facilitiesCount[i].objectCount;
                }
            }
        }
    }

    const months = Array.from({ length: 12 }, (item, i) => {
        return new Date(0, i).toLocaleString('en-GB', { month: 'long' })
    });

    for (var i = 0; i < months.length; i++) {
        if (dateToday.getMonth() == i) {
            $('#players-period').text(playerDataStat[i]);
        }
    }

    for (var i = 0; i < months.length; i++) {
        if (dateToday.getMonth() == i) {
            $('#slot-booked-period').text(facilityDataStat[i]);
        }
    }

    const labels = months;
    const data = {
        labels: labels,
        datasets: [
            {
                label: 'Number of Players',
                data: playerDataStat,
                fill: false,
                borderColor: 'rgb(255,196,0)',
                tension: 0.1
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
                    stacked: true
                }]
            }
        }
    });


}

function renderRangeStatistics(dataObject, fromDate, toDate, update)
{
    var from = moment(new Date(fromDate)).format('MM-DD-YYYY');
    var to = moment(new Date(toDate)).format('MM-DD-YYYY');

    window.location = '/Facility/Reports?dateFrom=' + from + "&dateTo=" + to
    //console.log('test');
    //let playerCount = dataObject.facilityPlayerGroup;

    //let rangeFromDate = fromDate;
    //let rangeToDate = toDate;
    //var playerPeriodCount = 0;
    //for (var i = 0; i < playerCount.length; i++) {
    //    let playerCreatedDate = new Date(playerCount[i].year.toString() +'/'+playerCount[i].month.toString() +'/1');

    //    if (playerCreatedDate >= rangeFromDate && playerCreatedDate <= rangeToDate) {
    //        playerPeriodCount += playerCount[i].objectCount;
    //    }
    //}

    //let facilitiesCount = dataObject.bookingsGroupByYear;
    //var bookingsPeriodCount = 0;
    //for (var i = 0; i < facilitiesCount.length; i++) {
    //    let bookingsCreatedDate = new Date(facilitiesCount[i].year.toString() + '/' + facilitiesCount[i].month.toString() + '/1');

    //    if (bookingsCreatedDate >= rangeFromDate && bookingsCreatedDate <= rangeToDate) {
    //        bookingsPeriodCount += facilitiesCount[i].objectCount;
    //    }
    //}

    //$('#players-period').text(playerPeriodCount);
    //$('#slot-booked-period').text(bookingsPeriodCount);
}