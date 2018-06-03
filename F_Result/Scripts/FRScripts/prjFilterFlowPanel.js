﻿//Скрипты для работы с плавающей панелью фильтрации проектов.

$(function () {

    //Объявление глобальных переменных
    window._flag = true;
    window._prjIDs = []; //Массив проектов таблицы отчета
    window.filterPrjIDs = []; //Массив выбранных ID проектов в плавающей панели

    //Обработчик ввода в поле фильтра таблицы
    $('body').on('keyup change', '#prgtable_filter>label>input[type="search"]', function () {
        $('#allPrgCHB').prop('checked', 'checked').trigger('change');
    });

    //Блок скриптов для работы с плавающей панелью
    $('#showButton').on('click', function (e) {
        e.stopImmediatePropagation();
        if ($('#flowPanel').hasClass('open')) {
            $('#flowPanel').animate({ right: -360 }, 500).removeClass('open');
            $('#showButton').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        } else {
            $('#flowPanel').animate({ right: 0 }, 500).addClass('open');
            $('#showButton').removeClass('fa-arrow-left').addClass('fa-arrow-right');
        }
    });

    $('body').on('click', function (e) {
        $('#flowPanel').animate({ right: -360 }, 500).removeClass('open');
        $('#showButton').removeClass('fa-arrow-right').addClass('fa-arrow-left');
    }).on('click', '#flowPanel', function (e) {
        e.stopPropagation();
    });

    //Сброс/Установка всех чекбоксов выбора проекта
    $('#allPrgCHB').on('change', function () {
        window.filterPrjIDs = [];
        $('input:checkbox.prgSelectCHB').each(function () {
            $(this).prop('checked', $('#allPrgCHB').prop('checked'));
            if ($(this).is(":checked")) {
                window.filterPrjIDs.push($(this).data('prjid'));
            };
        });
        if (window.filterPrjIDs.length > 0) {
            var table = $("#gridtable").DataTable();
            table.draw();
        };
    });

    //Проверка состояния всех чекбоксов таблицы
    $('body').on('change', 'input:checkbox.prgSelectCHB', function () {
        var allChecked = true;
        window.filterPrjIDs = [];
        $('input:checkbox.prgSelectCHB').each(function () {
            if (!$(this).is(":checked")) {
                allChecked = false;
            } else if ($(this).is(":checked")) {
                window.filterPrjIDs.push($(this).data('prjid'));
            };
        });
        if (allChecked) {
            $('#allPrgCHB').prop('checked', 'checked');
        }
        else if (!allChecked) {
            $('#allPrgCHB').prop('checked', false);
        }
        if (window.filterPrjIDs.length > 0) {
            var table = $("#gridtable").DataTable();
            table.draw();
        };
    });

});

//Функция очистки массивов, сброса глобальных переменных и реинициализации таблицы отчета
function globalVarsClear() {
    window._prjIDs = [];
    window.filterPrjIDs = [];
    window._flag = true;
    $('#allPrgCHB').prop('checked', 'checked');
    var table = $("#gridtable").DataTable();
    table.draw();
}

//-----------------------------------------------------------------
//Функция создания таблицы со списком проектов
function prgTableCrate() {
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

