//Скрипт для стрелки "НАВЕРХ"
$(document).ready(function () {

        $(window).scroll(function () {
            if ($(this).scrollTop() > 60) {
                $('.scrollup1').fadeIn();
            } else {
                $('.scrollup1').fadeOut();
            }
        });

        $('.scrollup1').click(function () {
            $("html, body").animate({ scrollTop: 0 }, 600);
            return false;
        });

    });
