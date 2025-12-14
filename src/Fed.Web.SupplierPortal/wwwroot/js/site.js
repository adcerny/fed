
if (!window.fed) {
    fed = {};
}

(function ($) {

    fed = {
        
    }

    $(document).ready(function () {
        $("article > hgroup").click(function () {
            $(this).next("div").slideToggle("fast");
        });
    });
})(jQuery);