// #region Функция експорта данных в Excel данных базовых таблиц

function Export2Excel(controllerName) {
    $('#loadingImg').show();
    var api = $('#gridtable').dataTable().api();
    var json = api.ajax.json();
    var IDs = json.idslist;

    if (controllerName == null) {
        alert("Не указано имя метода контроллера!");
        $('#loadingImg').hide();
        return;
    }

    $.ajax({
        url: controllerName,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({ IDs: json.idslist, sortColumn: json.sortcolumn, sortColumnDir: json.sortdir }),
        success: function (data) {
            if (!data.result) {
                $('#loadingImg').hide(function () {
                    alert(data.message);
                });
                return false;
            }
            window.location.href = '../Export/GetFile?FName=' + data.filename;
            $('#loadingImg').hide();
        }
    });
}

// #endregion

// #region Функция експорта данных отчета "Бюджетирование" в Excel

function APBExport2Excel(controllerName) {
    $('#loadingImg').show();
    var api = $('#gridtable').dataTable().api();
    var json = api.ajax.json();
    var IDs = json.idslist;

    if (controllerName == null) {
        alert("Не указано имя метода контроллера!");
        $('#loadingImg').hide();
        return;
    }

    $.ajax({
        url: controllerName,
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            IDs: json.idslist
            , sortColumn: json.sortcolumn
            , sortColumnDir: json.sortdir
            , startPeriod: json.StartPeriod
            , endPeriod: json.EndPeriod
            , Period: json.Period
            , ProjectName: json.ProjectName
            , isAllTimes: json.IsAllTimes
        }),
        success: function (data) {
            if (!data.result) {
                $('#loadingImg').hide(function () {
                    alert(data.message);
                });
                return false;
            }
            window.location.href = '../Export/GetFile?FName=' + data.filename;
            $('#loadingImg').hide();
        }
    });
}

// #endregion


