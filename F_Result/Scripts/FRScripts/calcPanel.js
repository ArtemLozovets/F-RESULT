$(function () {
    //Инициализация панели добавления отзыва

    window._currList = []; //Массив типов валют

    $('.calcCurrInput').bind("change keyup input click", function () {
        alert('VALID');
        if (this.value.match(/[^0-9]/g)) {
            this.value = this.value.replace(/[^0-9]/g, '');
        }
    });

    var navbarHeight = $('#mainNavbar').height();
    $('#draggableCalc').css('top', navbarHeight + 10).css('left', 10);
    $('#draggableCalc').draggable({
        containment: "window",
        scrollSensitivity: 20,
        scrollSpeed: 20,
        cursor: "crosshair",
        opacity: 0.5
    });

});


//Отображение панели добавления отзыва
$('body').on('click', '#calcBtn', function (e) {
    e.preventDefault();

    $.each(_currList, function (key, value) {
        input = jQuery('<input class="calcCurrInput form-control" id="curr"' + value + ' placeholder="' + value + '" onkeyup = "reppoint(this)"  @onchange = "reppoint(this)">');
        $('#calcCurr').append(input);
    });

    //$('.calcCurrInput').bind("change keyup input click", function () {
    //    if (this.value.match(/[^0-9,\.]/g)) {
    //        //^[0-9]*\.[0-9]{2}$
    //        this.value = this.value.replace(/[^0-9,\.]/g, '');
    //    }
    //});

    $('.calcCurrInput').bind("blur", function () {
        if (!this.value.match(/^[0-9]*\.[0-9]{2}$/g)) {
            this.value = 'ERROR';
        }
    });

    $(this).attr('disabled', 'true');
    $('#draggableCalc').fadeIn(500);
});

//Скрытие панели калькулятора
$('#hideCalc').on('click', function () {
    $('#draggableCalc').fadeOut(500, function () {
        $('#addCalcPanel').hide();
        $('#calcCurr').empty();
        $('#calcBtn').removeAttr('disabled');
    });
});


//Добавление остатка
$('body').on('click', '#addBalance', function (e) {
    e.preventDefault();
    this.blur();
    $('#addCalcPanel').slideDown(500);

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

        //Separator replace. Convert "." to ","
        function reppoint(elem) {
            var d = elem.value;
            if (d.indexOf(".") > 0) {
                var outstr = d.replace(".", ",");
                elem.value = outstr;
            }
        }
