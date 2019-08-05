$(function () {
    //Инициализация панели добавления остатка

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

    // DateRange picker
    var nowDate = moment(new Date).format('DD/MM/YYYY');
    $('#balanceDate').val(nowDate);

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
          .bind('datepicker-change', function (event, obj) {
              //console.log(obj);
              getBalanceFunc(obj.value);
          });

    getBalanceFunc($('#balanceDate').val());
});

//Отображение калькулятора остатка
$('body').on('click', '#calcBtn', function (e) {
    e.preventDefault();

    //Отображение полей добавления остатка
    $('#calcCurr').append('<div class="form-horizontal" id="currForm"></div>');

    $.each(_currList, function (key, value) {

        var input = '<div class="form-group">';
        input += '<label for="curr' + value + '" class="control-label col-md-2 text-white">' + value + '</label>';
        input += '<div class="col-md-8"><input class="calcCurrInput form-control" id="curr' + value + '" placeholder="Сумма остатка в ' + value + ' (0,00)" onkeyup = "reppoint(this)"  @onchange = "reppoint(this)"></div>';
        input += '</div>'
        $('#currForm').append(input);
    });

    var btnString = '<button id="closeBalanceBtn" class="btn btn-sm btn-primary"  title="Скрыть панель добавления остатка"><i class="glyphicon glyphicon glyphicon-remove"></i>&nbsp;Отмена</button>&nbsp;';
    btnString += '<button id="saveBalanceBtn" class="btn btn-sm btn-primary" title="Сохранить остаток"><i class="glyphicon glyphicon-floppy-save"></i>&nbsp;Cохранить</button></div>';
    $('#currForm').append(btnString);

    //Сохранение введенных остатков
    $('#saveBalanceBtn').bind("click", function () {
        this.blur();
        var _isEmpty = true;
        var _isNotValid = false;
        var _currData = {};

        $('.calcCurrInput').bind("click", function () {
            this.value = "";
            $(this).css({ "background-color": "white", "color": "", "border": "" });
        });

        $(".calcCurrInput").each(function (index) {

            if (this.value !== '') {
                _isEmpty = false;
            }

            if ((!this.value.match(/^[0-9]*\,[0-9]{2}$/g) && !this.value.match(/^[0-9]*$/g) && !this.value.match(/^[0-9]*\,[0-9]{1}$/g)) || (this.value === '')) {
                $(this).css({ "border": "1px solid red", "background-color": "rgba(255, 99, 132, 0.8)", "color": "white" });
                _isNotValid = true;
            }
            else {
                var _currName = $(this).attr('id').replace('curr', '');
                _currData[_currName] = this.value;
            }

        });

        if (_isNotValid) { return false; }

        if (_isEmpty) {
            alert('Введите данные остатка!');
            return false;
        }
        else {

            var _balanceDate = $('#balanceDate').val();

            saveBalanceFunc(_balanceDate, _currData);

            $('#addCalcPanel').slideUp(500, function () {
                $('#addBalance').removeAttr('disabled');
                $('#balanceBlock').slideDown(500);
            });
        }

    });

    $('#closeBalanceBtn').bind("click", function () {
        this.blur();
        $('#addCalcPanel').slideUp(500, function () {
            $('#addBalance').removeAttr('disabled');
            $('#balanceBlock').slideDown(500);
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
        $('#addBalance').removeAttr('disabled');
        $('#calcBtn').removeAttr('disabled');
    });
});

//Отображение вычисленного остатка
function showBalance(){

}

//Добавление остатка
$('body').on('click', '#addBalance', function (e) {
    e.preventDefault();
    $(this).attr('disabled', "true").blur();
    $('#balanceBlock').slideUp(500, function () {
        $('#addCalcPanel').slideDown(500);
    });
});


// Функция получения остатка
function getBalanceFunc(_balanceDate) {
    var _url = "http://" + $(location).attr('host') + "/BalanceCalculator/GetBalance";

    $('#loadingImg').show();

    $.ajax({
        url: _url,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            balanceDate: _balanceDate,
        }),
        success: function (data) {
            if (!data.result) {
                $('#loadingImg').hide(function () {
                    alert(data.message);
                });
                return false;
            }
            else {
                //Отображение вычисленного остатка

                var _balObj = JSON.parse(data.balanceValue);
                console.log('OBJ=>', _balObj);

                $.each(data.balanceValue, function (key, value) {
                    var balance = '<div class="form-group">';
                    balance += '<label for="curr' + window._currList[0] + '" class="control-label col-md-2 text-white">' + window._currList[0] + '</label>';
                    balance += '</div>';
                    $('#showBalance').append(balance);
                });

                $('#loadingImg').hide();
            }
        }
    });
}


// Функция сохранения остатка в БД
function saveBalanceFunc(_balanceDate, _currData) {

    var _url = "http://" + $(location).attr('host') + "/BalanceCalculator/AddBalance";

    $('#loadingImg').show();

    $.ajax({
        url: _url,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            balanceDate: _balanceDate,
            currData: JSON.stringify(_currData)
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
