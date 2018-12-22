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
        if ($('#flowPanelOrg').hasClass('open')) {
            //Закрываем панель
            $('#flowPanelOrg').animate({ left: -360, zIndex: 10001 }, 500, function () {
                $('#flowPanelExpend').animate({ zIndex: 1001 }, 500, function () { $('#flowPanelOrg').css('z-index', '9999') });
                $('#showButtonOrg').animate({ marginTop: 140, opacity: 1 }, 600, function () { $('#showButtonExpend').animate({ opacity: 1 }, 600) });
            }).removeClass('open');
            $('#showButtonOrg').removeClass('fa-arrow-left').addClass('fa-arrow-right');
        } else {
            //Открываем панель
            $('#showButtonOrg').animate({ marginTop: 0, opacity: 1 }, 600, function () {
                $('#flowPanelOrg').animate({ left: 0 }, 500, function () {
                    $('#showButtonExpend').animate({ opacity: 0.5 }, 600);
                    $('#flowPanelExpend').animate({ left: -360 }, 500, function () {
                        $('#showButtonExpend').removeClass('fa-arrow-left').addClass('fa-arrow-right').animate({ opacity: 0.5, marginTop: 0 }, 600);
                    }).removeClass('open').animate({ zIndex: 10001 }, 500, function () { $('#flowPanelOrg').css('z-index', '9999') });
                })
                .addClass('open');
                $('#showButtonOrg').removeClass('fa-arrow-right').addClass('fa-arrow-left');
            });
        }
    });

    $('body').on('click', function (e) {
        $('#flowPanelOrg').animate({ left: -360, zIndex: 10001 }, 500, function () {
            $('#showButtonOrg').animate({ marginTop: 140, opacity: 1 }, 600, function () { $(this).removeClass('fa-arrow-left').addClass('fa-arrow-right') })
        }).removeClass('open');
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

