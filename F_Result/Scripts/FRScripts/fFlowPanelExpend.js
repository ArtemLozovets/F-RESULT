//Скрипты для работы с плавающей панелью фильтрации по статьям доходов/расходов.

$(function () {

    //Объявление глобальных переменных
    window._flagExpend = true;
    window._expendIDs = []; //Массив проектов таблицы отчета
    window.filterExpendIDs = []; //Массив выбранных ID проектов в плавающей панели

    //Обработчик ввода в поле фильтра таблицы
    $('body').on('keyup change', '#expend_table_filter>label>input[type="search"]', function () {
        $('#allExpendCHB').prop('checked', 'checked').trigger('change');
    });

    //Блок скриптов для работы с плавающей панелью
    $('#showButtonExpend').on('click', function (e) {
        e.stopImmediatePropagation();
        if ($('#flowPanelExpend').hasClass('open')) {
            $('#flowPanelExpend').animate({ left: -360 }, 500).removeClass('open');
            $('#showButtonExpend').removeClass('fa-arrow-left').addClass('fa-arrow-right');
        } else {
            $('#flowPanelExpend').animate({ left: 0 }, 500).addClass('open');
            $('#showButtonExpend').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        }
    });

    $('body').on('click', function (e) {
        $('#flowPanelExpend').animate({ left: -360 }, 500).removeClass('open');
        $('#showButtonExpend').removeClass('fa-arrow-left').addClass('fa-arrow-right');
    }).on('click', '#flowPanelExpend', function (e) {
        e.stopPropagation();
    });

    //Сброс/Установка всех чекбоксов выбора проекта
    //$('#allPrgCHB').on('change', function () {
    //    window.filterPrjIDs = [];
    //    $('input:checkbox.prgSelectCHB').each(function () {
    //        $(this).prop('checked', $('#allPrgCHB').prop('checked'));
    //        if ($(this).is(":checked")) {
    //            window.filterPrjIDs.push($(this).data('prjid'));
    //        };
    //    });
    //    if (window.filterPrjIDs.length > 0) {
    //        if ($('#flowPanel').data('mode') === 'chart') {
    //            var _Year = $('#YDDL').val();
    //            var chartType = $("#chartType").val();
    //            GetChart(_Year, chartType);
    //        }
    //        else {
    //            $("#gridtable").DataTable().draw();
    //        }
    //    };
    //});

    //Проверка состояния всех чекбоксов таблицы
    //$('body').on('change', 'input:checkbox.prgSelectCHB', function () {
    //    var allChecked = true;
    //    window.filterPrjIDs = [];
    //    $('input:checkbox.prgSelectCHB').each(function () {
    //        if (!$(this).is(":checked")) {
    //            allChecked = false;
    //        } else if ($(this).is(":checked")) {
    //            window.filterPrjIDs.push($(this).data('prjid'));
    //        };
    //    });
    //    if (allChecked) {
    //        $('#allPrgCHB').prop('checked', 'checked');
    //    }
    //    else if (!allChecked) {
    //        $('#allPrgCHB').prop('checked', false);
    //    }
    //    if (window.filterPrjIDs.length > 0) {
    //        if ($('#flowPanel').data('mode') === 'chart') {
    //            var _Year = $('#YDDL').val();
    //            var chartType = $("#chartType").val();
    //            GetChart(_Year, chartType);
    //        }
    //        else {
    //            $("#gridtable").DataTable().draw();
    //        }
    //    };
    //});

});

//Функция очистки массивов, сброса глобальных переменных и реинициализации таблицы отчета
function globalExpendVarsClear() {
    window._prjIDs = [];
    window.filterPrjIDs = [];
    window._flag = true;
    $('#allPrgCHB').prop('checked', 'checked');
    if ($('#flowPanel').data('mode') === 'chart') {
        var _Year = $('#YDDL').val();
        var chartType = $("#chartType").val();
        GetChart(_Year, chartType);
    }
    else {
        $("#gridtable").DataTable().draw();
    }
}

//-----------------------------------------------------------------
//Функция создания таблицы со списком проектов
function expendTableCrate() {
    var prgTable = $('#prgtable').DataTable({
        "scrollY": "360px",
        "scrollCollapse": true,
        scrollX: '100%',
        dom: "<<'col-sm-12'f>><'row'<'col-sm-12'tr>>",
        language: {
            zeroRecords: "Записи отсутствуют",
            infoEmpty: "Записи отсутствуют",
            search: "",
            searchPlaceholder: "Проект...",
        },
        autoWidth: true,
        paging: false,
        processing: false,
        serverSide: false,
        data: _prjIDs,
        order: [[0, "desc"]],
        'columnDefs': [
            { 'orderData': [0], 'targets': [2] }
        ],
        columns: [
            { data: 'IPA', bSortable: true, visible: false, searchable: false },
            { data: 'PrjId', bSortable: true, visible: false, searchable: false },
            { data: 'ProjectName', bSortable: true, sWidth: '80%' },
            {
                data: null, sWidth: '20%',
                bSortable: false,
                render: function (data, type, row, meta) {
                    var chkStr = '<div style="width:100%; text-align:center;"><input data-prjid="' + row['PrjId'] + '" class="prgSelectCHB" checked type="checkbox"></div>';
                    return chkStr;
                }
            }
        ],
        fnDrawCallback: function (settings) {
            $('#flowPanel').show();
        }
    });

};

