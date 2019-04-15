//Скрипты для работы с плавающей панелью фильтрации по организациям.

$(function () {

    //Объявление глобальных переменных
    window._flagOrg = true;
    window._orgIDs = []; //Массив проектов таблицы отчета
    window.filterOrgIDs = []; //Массив выбранных ID статей в плавающей панели

    //Обработчик ввода в поле фильтра таблицы
    $('body').on('keyup change', '#org_Table_filter>label>input[type="search"]', function () {
        $('#allOrgCHB').prop('checked', 'checked').trigger('change');
    });

    //Блок скриптов для работы с плавающей панелью
    $('#showButtonOrg').on('click', function (e) {
        e.stopImmediatePropagation();
        $('#flowPanel').animate({ right: -360 }, 500).removeClass('open');
        $('#showButton').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        $('#flowPanelExpend').animate({ right: -360 }, 500).removeClass('open');
        $('#showButtonExpend').removeClass('fa-arrow-right').addClass('fa-arrow-left');

        if ($('#flowPanelOrg').hasClass('open')) {
            $('#flowPanelOrg').animate({ right: -360 }, 500, function () {
                $('#flowPanel').css('z-index', '9997').animate({ opacity: 1 }, 600);
                $('#flowPanelExpend').css('z-index', '9999').animate({ opacity: 1 }, 600);
                $(this).css('z-index', '9998');
            }).removeClass('open');
            $('#showButtonOrg').removeClass('fa-arrow-right').addClass('fa-arrow-left');
        } else {
            $('#flowPanelOrg').animate({ right: 0, opacity: 1 }, 500, function () {
                $('#flowPanel').css('z-index', '9998').animate({ opacity: 0.5 }, 600);
                $('#flowPanelExpend').css('z-index', '9999').animate({ opacity: 0.5 }, 600);
                $(this).css('z-index', '9997');
            }).addClass('open');
            $('#showButtonOrg').removeClass('fa-arrow-left').addClass('fa-arrow-right');
        }
    });

    $('body').on('click', function (e) {
        $('#flowPanelOrg').animate({ right: -360, opacity: 1  }, 500).removeClass('open');
        $('#showButtonOrg').removeClass('fa-arrow-right').addClass('fa-arrow-left');
    }).on('click', '#flowPanelOrg', function (e) {
        e.stopPropagation();
    });

    //Сброс/Установка всех чекбоксов выбора проекта
    $('#allOrgCHB').on('change', function () {
        window.filterOrgIDs = [];
        $('input:checkbox.orgSelectCHB').each(function () {
            $(this).prop('checked', $('#allOrgCHB').prop('checked'));
            if ($(this).is(":checked")) {
                window.filterOrgIDs.push($(this).data('orgid'));
            };
        });
        if (window.filterOrgIDs.length > 0) {
            $("#gridtable").DataTable().draw();
        };
    });

    //Проверка состояния всех чекбоксов таблицы
    $('body').on('change', 'input:checkbox.orgSelectCHB', function () {
        var allChecked = true;
        window.filterOrgIDs = [];
        $('input:checkbox.orgSelectCHB').each(function () {
            if (!$(this).is(":checked")) {
                allChecked = false;
            } else if ($(this).is(":checked")) {
                window.filterOrgIDs.push($(this).data('orgid'));
            };
        });
        if (allChecked) {
            $('#allOrgCHB').prop('checked', 'checked');
        }
        else if (!allChecked) {
            $('#allOrgCHB').prop('checked', false);
        }
        if (window.filterOrgIDs.length > 0) {
            $("#gridtable").DataTable().draw();
        };
    });

});

//Функция очистки массивов, сброса глобальных переменных и реинициализации таблицы отчета
function globalOrgVarsClear() {
    window._orgIDs = [];
    window.filterOrgIDs = [];
    window._flagOrg = true;
    $('#allOrgCHB').prop('checked', 'checked');
    $("#gridtable").DataTable().draw();
}

//-----------------------------------------------------------------
//Функция создания таблицы со списком статей
function orgTableCrate() {
    var orgTable = $('#org_Table').DataTable({
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
        data: _orgIDs,
        order: [[ 1, "asc" ]],
        columns: [
            { data: 'AtId', bSortable: true, visible: false, searchable: false },
            { data: 'AtName', bSortable: true, sWidth: '80%' },
            {
                data: null, sWidth: '20%',
                bSortable: false,
                render: function (data, type, row, meta) {
                    var chkStr = '<div style="width:100%; text-align:center;"><input data-orgid="' + row['AtId'] + '" class="orgSelectCHB" checked type="checkbox"></div>';
                    return chkStr;
                }
            }
        ],
        fnDrawCallback: function (settings) {
            $('#flowPanelOrg').show();
        }
    });

};

