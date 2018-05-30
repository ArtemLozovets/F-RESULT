// #region Функция експорта в Excel данных базовых таблиц

function Export2Excel(controllerName) {
    var api = $('#gridtable').dataTable().api();
    var json = api.ajax.json();
    var paramStr = JSON.stringify({ IDs: json.idslist, sortColumn: json.sortcolumn, sortColumnDir: json.sortdir });
    fnExport2Excel(controllerName, paramStr);
}

// #endregion

// #region Функция експорта данных отчета "Бюджетирование" в Excel

function APBExport2Excel(controllerName) {
    var api = $('#gridtable').dataTable().api();
    var json = api.ajax.json();

    var idsArray = new Array();
    var arr_from_json = JSON.parse(json.prjlist);
    
    for (var i = 0; i < arr_from_json.length; i++) {
        idsArray.push(arr_from_json[i].PrjId);
    }

    var startPeriod = json.StartPeriod;
    var endPeriod = json.EndPeriod;

    var paramStr = JSON.stringify({
        IDs: idsArray //!!!-Список проектов-!!!
            , sortColumn: json.sortcolumn
            , sortColumnDir: json.sortdir
            , startPeriod: startPeriod
            , endPeriod: endPeriod
            , Period: json.Period
            , ProjectName: json.ProjectName
            , isAllTimes: json.isAllTimes
});

console.log(paramStr);

fnExport2Excel(controllerName, paramStr);
};

// #endregion


// #region Функция експорта данных отчета "Прибыльность" в Excel

function APPExport2Excel(controllerName) {
    var api = $('#gridtable').dataTable().api();
    var json = api.ajax.json();

    var idsArray = new Array();
    var arr_from_json = JSON.parse(json.prjlist);

    for (var i = 0; i < arr_from_json.length; i++) {
        idsArray.push(arr_from_json[i].PrjId);
    }

    var baseDate = json.baseDate;

    var paramStr = JSON.stringify({
        IDs: idsArray //!!!-Список проектов-!!!
            , sortColumn: json.sortcolumn
            , sortColumnDir: json.sortdir
            , repDate: moment(json.repDate).format('YYYYMMDD')
            , ProjectName: json.ProjectName
    });

    console.log(paramStr);

    fnExport2Excel(controllerName, paramStr);
};

// #endregion

//#region Функция експорта в Excel

function fnExport2Excel(controllerName, paramStr) {
    $('#loadingImg').show();

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
        data: paramStr,
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