//Скрипт для очистки блока сообщений

$('body').click(function () {
    $('#resultMessage').fadeOut(1000,
        function () {
            $('#MessageOk').html('');
            $('#MessageError').html('');
            $('#resultMessage').fadeIn(1000)
        });
});