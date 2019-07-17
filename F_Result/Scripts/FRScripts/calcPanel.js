$(function () {
    //Инициализация панели добавления отзыва

    window._currList = []; //Массив типов валют

    var navbarHeight = $('#mainNavbar').height();
    $('#draggableCalc').css('top', navbarHeight + 10).css('left', 10);
    $('#draggableCalc').draggable({
        containment: "window",
        scrollSensitivity: 20,
        scrollSpeed: 20,
        cursor: "crosshair",
        opacity: 0.5
    });

    $('#balanceDate').dateRangePicker(
                {
                    customTopBar: 'Расчетная дата остатка',
                    showWeekNumbers: true,
                    startOfWeek: 'monday',
                    singleDate: true,
                    singleMonth: true,
                    language: 'ru',
                    startOfWeek: 'monday',
                    separator: ' - ',
                    format: 'DD/MM/YYYY',
                    autoClose: true,
                    monthSelect: true,
                    yearSelect: true,
                    setValue: function (s) {
                        $(this).val(s);
                    },
                    customOpenAnimation: function (cb) {
                        $(this).fadeIn(300, cb);
                    },
                    customCloseAnimation: function (cb) {
                        $(this).fadeOut(300, cb);
                    },
                    showShortcuts: false,
                    time: {
                        enabled: false
                    },
                    extraClass: 'date-range-pickerM'
                })

    var nowDate = moment(new Date).format('DD/MM/YYYY');
    $('#balanceDate').val(nowDate);


});


//Отображение панели добавления отзыва
$('body').on('click', '#calcBtn', function (e) {
    e.preventDefault();

    $('#calcCurr').append('<div class="form-horizontal" id="currForm"></div>');

    $.each(_currList, function (key, value) {

        var input = '<div class="form-group">';
        input += '<label for="curr' + value + '" class="control-label col-md-2 text-white">' + value + '</label>';
        input += '<div class="col-md-8"><input class="calcCurrInput form-control" id="curr"' + value + ' placeholder="0,00" onkeyup = "reppoint(this)"  @onchange = "reppoint(this)"></div>';
        input += '</div>'
        $('#currForm').append(input);
    });

    var btnString = '<button id="closeBalanceBtn" class="btn btn-sm btn-primary"  title="Скрыть панель добавления остатка"><i class="glyphicon glyphicon glyphicon-remove"></i>&nbsp;Отмена</button>&nbsp;';
    btnString += '<button id="saveBalanceBtn" class="btn btn-sm btn-primary" title="Сохранить остаток"><i class="glyphicon glyphicon-floppy-save"></i>&nbsp;Cохранить</button></div>';
    $('#currForm').append(btnString);

    $('#saveBalanceBtn').bind("click", function () {
        $('.calcCurrInput').bind("click", function () {
            this.value = "";
            $(this).css({ "background-color": "white", "color": "", "border": "" });
        });
    });

    $('#saveBalanceBtn').bind("click", function () {
        this.blur();
        var isEmpty = true;
        var isNotValid = false;
        $(".calcCurrInput").each(function (index) {

            if (this.value !== '') {
                isEmpty = false;
            }

            if (!this.value.match(/^[0-9]*\,[0-9]{2}$/g) && !this.value.match(/^[0-9]*$/g) && !this.value.match(/^[0-9]*\,[0-9]{1}$/g)) {
                $(this).css({ "border": "1px solid red", "background-color": "rgba(255, 99, 132, 0.8)", "color": "white" });
                isNotValid = true;
            }

        });

        if (isNotValid) { return false; }

        if (isEmpty) {
            alert('Введите данные остатка!');
            return false;
        }
        else {

            var _balanceDate = $('#balanceDate').val();
            saveBalanceFunc(_balanceDate);


            $('#addCalcPanel').slideUp(500, function () {
                $('#addBalance').removeAttr('disabled');
            });
        }

    });

    $('#closeBalanceBtn').bind("click", function () {
        this.blur();
        $('#addCalcPanel').slideUp(500, function () {
            $('#addBalance').removeAttr('disabled');
        });

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
    $(this).attr('disabled', "true").blur();
    $('#addCalcPanel').slideDown(500);

});


// Функция сохранения остатка в БД
function saveBalanceFunc(_balanceDate) {

    var _url = "http://" + $(location).attr('host') + "/BalanceCalculator/AddBalance";

    $('#loadingImg').show();

    $.ajax({
        url: _url,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            balanceDate: _balanceDate
        }),
        success: function (data) {
            if (!data.result) {
                $('#loadingImg').hide(function () {
                    alert(data.message);
                });
                return false;
            }
            $('#loadingImg').hide();
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


////Добавление отзыва
//$('#sendComment').on('click', function () {
//    var _url = "http://" + $(location).attr('host') + "/Feedback/CommentsCreate";

//    if ($('#commentText').val().length == 0) {
//        alert('Введите текст отзыва');
//        return false;
//    }

//    $('#loadingImg').show();
//    var _cmText = $('#commentText').val();
//    var _cmUrl = $('#cmUrl').val();

//    $.ajax({
//        url: _url,
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        data: JSON.stringify({
//            cmText: _cmText,
//            cmUrl: _cmUrl
//        }),
//        success: function (data) {
//            if (!data.result) {
//                $('#loadingImg').hide(function () {
//                    alert(data.message);
//                });
//                return false;
//            }
//            $('#commentText').val('').trigger('change');
//            $('#draggable').fadeOut(500, function () {
//                $('#loadingImg').hide();
//            });
//        }
//    });
//});

////Утверждение отзыва
//function commentAccept(status, cmId) {
//    $('#loadingImg').show();
//    $.ajax({
//        url: "../Feedback/CommentAccept",
//        type: "POST",
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        data: JSON.stringify({
//            CommentId: cmId,
//            State: status
//        }),
//        success: function (data) {
//            if (!data.result) {
//                $('#loadingImg').hide(function () {
//                    alert(data.message);
//                });
//                return false;
//            }
//            $('#loadingImg').hide();
//            $('#gridtable').DataTable().draw();
//        }
//    });
//}