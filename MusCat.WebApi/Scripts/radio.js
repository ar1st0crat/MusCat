let audio = $('#player')[0];
document.upcoming = {};
document.isPlaying = false;

window.onload = updateSong;
$('#player').bind('ended', updateSong);

function fetchAudioAndPlay(offset) {
    fetch('/api/radio/stream')
        .then(response => response.blob())
        .then(blob => URL.createObjectURL(blob))
        .then(url => {
            audio.src = url;
            audio.play();
            audio.currentTime = offset;
        });
}

function updateSong() {
    $.getJSON('/api/radio/current', function (data) {
        audio.muted = true;
        // update main panel
        $('h3').text(data.Name);
        $('h5').text(data.Performer);
        $('time').text(data.Year);
        $('#current').css('background-image', 'url("/api/albums/' + data.AlbumId + '/cover")');
        // fetch audio
        fetchAudioAndPlay(data.Offset);
        if (document.isPlaying) audio.muted = false;
    });

    $.getJSON('/api/radio/upcoming', updateUpcomingList);
}

function updateUpcomingList(data) {
    document.upcoming = data;
    $('#upcoming').empty();

    let items = [];
    let sign = 1;
    let i = 0;

    $.each(data, function (_, song) {
        items.push('<p>' +
            '<span>' + song.Performer + '</span>' +
            '<span>' + song.Name + '</span>' +
            '<span>[' + song.Duration + ']</span>' +
            '</p>');

        let img = document.createElement('img');
        img.ondblclick = function () {
            removeSong(song);
        }
        img.setAttribute('src', '/api/albums/' + song.AlbumId + '/cover');
        img.setAttribute('width', '80px');
        img.setAttribute('height', '80px');
        img.style.position = 'absolute';
        img.style.left = +((i % 4) * 60) + 'px';
        img.style.top = +(Math.floor(i / 4) * 60) + 'px';
        img.style.transform = 'rotate(' + +(Math.random() * 20) * sign + 'deg)';
        sign = -sign;
        i++;

        $('#upcoming').append(img);
    });

    $('<ul/>', {
        'class': 'upcoming-list',
        html: items.join('')
    })
        .appendTo('#upcoming');
}

function removeSong(song) {
    $.when($.ajax('/api/radio/remove/' + song.Id))
        .then(function () {
            $.getJSON('/api/radio/upcoming', updateUpcomingList);
        });
}

function pauseOrResume() {
    if (document.isPlaying) {
        audio.muted = true;
        $('#playback').attr('src', '../../Content/play.png');
        document.isPlaying = false;
    }
    else {
        audio.muted = false;
        audio.play();
        $('#playback').attr('src', '../../Content/pause.png');
        document.isPlaying = true;
    }
}
