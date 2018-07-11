// -------Скрипт для работы с плавающей панелью---------
$(function () {
    //Объявление глобальных переменных
    window._flag = true;
    window._wksIDs = []; //Массив сотрудников таблицы отчета
    window.filterWksIds = []; //Массив выбранных ID сотрудников в плавающей панели

    //Обработчик ввода в поле фильтра таблицы
    $('body').on('keyup change', '#prgtable_filter>label>input[type="search"]', function () {
        $('#allPrgCHB').prop('checked', 'checked').trigger('change');
    });

    $('#showButtonW').on('click', function (e) {
        e.stopImmediatePropagation();
        if ($('#flowPanel').hasClass('open')) {
            $('#flowPanel').animate({ right: -360 }, 500).removeClass('open');
            $('#showButtonW').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        } else {
            $('#flowPanel').animate({ right: 0 }, 500).addClass('open');
            $('#showButtonW').removeClass('fa-arrow-left').addClass('fa-arrow-right');
        }
    });

    $('body').on('click', function (e) {
        $('#flowPanel').animate({ right: -360 }, 500).removeClass('open');
        $('#showButtonW').removeClass('fa-arrow-right').addClass('fa-arrow-left');
    }).on('click', '#flowPanel', function (e) {
        e.stopPropagation();
    });


    //Сброс/Установка всех чекбоксов выбора проекта
    $('#allPrgCHB').on('change', function () {
        window.filterWksIDs = [];
        $('input:checkbox.prgSelectCHB').each(function () {
            $(this).prop('checked', $('#allPrgCHB').prop('checked'));
            if ($(this).is(":checked")) {
                window.filterWksIDs.push($(this).data('wksid'));
                console.log(window.filterWksIds);
            };
        });
        if (window.filterWksIDs.length > 0) {
                $("#gridtable").DataTable().draw();
        }
    });

    //Проверка состояния всех чекбоксов таблицы
    $('body').on('change', 'input:checkbox.prgSelectCHB', function () {
        var allChecked = true;
        window.filterWksIDs = [];
        $('input:checkbox.prgSelectCHB').each(function () {
            if (!$(this).is(":checked")) {
                allChecked = false;
            } else if ($(this).is(":checked")) {
                window.filterWksIDs.push($(this).data('wksid'));
            };
        });
        if (allChecked) {
            $('#allPrgCHB').prop('checked', 'checked');
        }
        else if (!allChecked) {
            $('#allPrgCHB').prop('checked', false);
        }
        if (window.filterWksIDs.length > 0) {
            console.log(window.filterWksIDs);
                $("#gridtable").DataTable().draw();
        };
    });

});

//-----------------------------------------------------------------
//Функция создания таблицы со списком сотрудников
function wksTableCrate() {
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
        data: _wksIDs,
        order: [[1, "asc"]],
        columns: [
            { data: 'WorkerId', bSortable: true, sWidth: '20%' },
            { data: 'WorkerName', bSortable: true, sWidth: '60%' },
            {
                data: null, sWidth: '20%',
                bSortable: false,
                render: function (data, type, row, meta) {
                    var chkStr = '<div style="width:100%; text-align:center;"><input data-wksid="' + row['WorkerId'] + '" class="prgSelectCHB" checked type="checkbox"></div>';
                    return chkStr;
                }
            }
        ],
        fnDrawCallback: function (settings) {
            $('#flowPanel').show();
        }
    });

};
