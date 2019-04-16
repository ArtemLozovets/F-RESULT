//Скрипты для работы с плавающей панелью фильтрации по статьям доходов/расходов.

$(function () {

    //Объявление глобальных переменных
    window._flagExpend = true;
    window._expendIDs = []; //Массив проектов таблицы отчета
    window.filterExpendIDs = []; //Массив выбранных ID статей в плавающей панели

    //Обработчик ввода в поле фильтра таблицы
    $('body').on('keyup change', '#expend_Table_filter>label>input[type="search"]', function () {
        $('#allExpendCHB').prop('checked', 'checked').trigger('change');
    });

    //Блок скриптов для работы с плавающей панелью
    $('#showButtonExpend').on('click', function (e) {
        e.stopImmediatePropagation();
        $('#flowPanel').animate({ right: -360 }, 500).removeClass('open');
        $('#showButton').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        $('#flowPanelOrg').animate({ right: -360 }, 500).removeClass('open');
        $('#showButtonOrg').removeClass('fa-arrow-right').addClass('fa-arrow-left');

        if ($('#flowPanelExpend').hasClass('open')) {
            $('#flowPanelExpend').animate({ right: -360 }, 500, function () {
                $('#flowPanel').css('z-index', '9997').animate({ opacity: 1 }, 600);
                $('#flowPanelOrg').css('z-index', '9998').animate({ opacity: 1 }, 600);
                $(this).css('z-index', '9999');
            }).removeClass('open');
            $('#showButtonExpend').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        } else {
            $('#flowPanelExpend').animate({ right: 0, opacity: 1 }, 500, function () {
                $('#flowPanel').css('z-index', '9998').animate({ opacity: 0.5 }, 600);
                $('#flowPanelOrg').css('z-index', '9999').animate({ opacity: 0.5 }, 600);
                $(this).css('z-index', '9997');
            }).addClass('open');
            $('#showButtonExpend').removeClass('fa-arrow-left').addClass('fa-arrow-right');
        }
    });

    $('body').on('click', function (e) {
        $('#flowPanelExpend').animate({ right: -360, opacity: 1 }, 500).removeClass('open');
        $('#showButtonExpend').removeClass('fa-arrow-right').addClass('fa-arrow-left');
    }).on('click', '#flowPanelExpend', function (e) {
        e.stopPropagation();
    });

    //Сброс/Установка всех чекбоксов выбора проекта
    $('#allExpendCHB').on('change', function () {
        window.filterExpendIDs = [];
        $('input:checkbox.expendSelectCHB').each(function () {
            $(this).prop('checked', $('#allExpendCHB').prop('checked'));
            if ($(this).is(":checked")) {
                window.filterExpendIDs.push($(this).data('atid'));
            };
        });
        if (window.filterExpendIDs.length > 0) {
            $("#gridtable").DataTable().draw();
        };
    });

    //Проверка состояния всех чекбоксов таблицы
    $('body').on('change', 'input:checkbox.expendSelectCHB', function () {
        var allChecked = true;
        window.filterExpendIDs = [];
        $('input:checkbox.expendSelectCHB').each(function () {
            if (!$(this).is(":checked")) {
                allChecked = false;
            } else if ($(this).is(":checked")) {
                window.filterExpendIDs.push($(this).data('atid'));
            };
        });
        if (allChecked) {
            $('#allExpendCHB').prop('checked', 'checked');
        }
        else if (!allChecked) {
            $('#allExpendCHB').prop('checked', false);
        }
        if (window.filterExpendIDs.length > 0) {
            $("#gridtable").DataTable().draw();
        };
    });

});

//Функция очистки массивов, сброса глобальных переменных и реинициализации таблицы отчета
function globalExpendVarsClear() {
    window._expendIDs = [];
    window.filterExpendIDs = [];
    window._flagExpend = true;
    $('#allExpendCHB').prop('checked', 'checked');
}

//-----------------------------------------------------------------
//Функция создания таблицы со списком статей
function expendTableCrate() {
    var expendTable = $('#expend_Table').DataTable({
        "scrollY": "360px",
        "scrollCollapse": true,
        scrollX: '100%',
        dom: "<<'col-sm-12'f>><'row'<'col-sm-12'tr>>",
        language: {
            zeroRecords: "Записи отсутствуют",
            infoEmpty: "Записи отсутствуют",
            search: "",
            searchPlaceholder: "Статья...",
        },
        autoWidth: true,
        paging: false,
        processing: false,
        serverSide: false,
        data: _expendIDs,
        columns: [
            { data: 'AtId', bSortable: true, visible: false, searchable: false },
            { data: 'AtName', bSortable: true, sWidth: '80%' },
            {
                data: null, sWidth: '20%',
                bSortable: false,
                render: function (data, type, row, meta) {
                    var chkStr = '<div style="width:100%; text-align:center;"><input data-atid="' + row['AtId'] + '" class="expendSelectCHB" checked type="checkbox"></div>';
                    return chkStr;
                }
            }
        ],
        fnDrawCallback: function (settings) {
            $('#flowPanelExpend').show();
        }
    });

};

