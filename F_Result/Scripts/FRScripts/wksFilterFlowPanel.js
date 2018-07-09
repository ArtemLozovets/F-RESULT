// -------Скрипт для работы с плавающей панелью---------
$(function () {
    //Объявление глобальных переменных
    window._flag = true;
    window._wksIDs = []; //Массив сотрудников таблицы отчета
    window.filterWksIDs = []; //Массив выбранных ID сотрудников в плавающей панели

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
        order: [[0, "desc"]],
        'columnDefs': [
            { 'orderData': [1], 'targets': [1] }
        ],
        columns: [
            { data: 'WorkerId', bSortable: true, sWidth: '20%' },
            { data: 'WorkerName', bSortable: true, sWidth: '60%' },
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
