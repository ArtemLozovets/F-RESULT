$(function () {
    //Инициализация панели добавления отзыва
    var navbarHeight = $('#mainNavbar').height();
    $('#draggable').css('top', navbarHeight + 10).css('left', 10);
    $('#draggable').draggable({
        containment: "window",
        scrollSensitivity: 20,
        scrollSpeed: 20,
        cursor: "crosshair",
        opacity: 0.5
    });
});

//Отображение панели добавления отзыва
$('#addComment').on('click', function (e) {
    e.preventDefault();
    $('#draggable').fadeIn(500);
    $('#commentText').focus();
});

//Скрытие панели добавления отзыва
$('#hidePanel').on('click', function () {
    $('#draggable').fadeOut(500);
});


//Добавление отзыва
$('#sendComment').on('click', function () {
    var _url = "http://" + $(location).attr('host') + "/Feedback/CommentsCreate";
    
    if ($('#commentText').val().length == 0) {
        alert('Введите текст отзыва');
        return false;
    }

    $('#loadingImg').show();
    var _cmText = $('#commentText').val();
    var _cmUrl = $('#cmUrl').val();

    $.ajax({
        url: _url,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            cmText: _cmText,
            cmUrl: _cmUrl
        }),
        success: function (data) {
            if (!data.result) {
                $('#loadingImg').hide(function () {
                    alert(data.message);
                });
                return false;
            }
            $('#commentText').val('').trigger('change');
            $('#draggable').fadeOut(500, function () {
                $('#loadingImg').hide();
            });
        }
    });
});

//Утверждение отзыва
function commentAccept(status, cmId) {
    $('#loadingImg').show();
    $.ajax({
        url: "../Feedback/CommentAccept",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            CommentId: cmId,
            State: status
        }),
        success: function (data) {
            if (!data.result) {
                $('#loadingImg').hide(function () {
                    alert(data.message);
                });
                return false;
            }
            $('#loadingImg').hide();
            $('#gridtable').DataTable().draw();
        }
    });
}

//Скрипт отображения количества оставшихся символов при вводе отзыва
function DisplayCount(elem, maxCount) {
    if (maxCount == null) {
        maxCount = 1000;
    }
    var currCount = $(elem).val().length;
    if (currCount > 0) {
        $("#sendComment").attr('disabled', false);
    }
    else {
        $("#sendComment").attr('disabled', true);
    }

    var leftCount = maxCount - currCount;

    if ($(elem).val().length > maxCount) {
        $(elem).val($(elem).val().substr(0, maxCount));
    } else {
        $("#charNum").text('Доступно ' + leftCount + ' сим.');
    }
}
