fed.helpers = {
    // check to see if element is above the window
    elemAboveTheWindow: function ($context) {
        if (($context.offset().top + $context.height()) > fed.cache.$window.scrollTop()) {
            return true;
        }
    },
    // check if element is visible on the screen
    elemIsInView: function ($context) {
        if (fed.cache.$window.scrollTop() + fed.cache.$window.height() > $context.offset().top) {
            return true;
        }
    },    
    // changes state of header on window scroll
    fixedHeader: function ($context) {
        var contextOuterHeight = $context.outerHeight() * 2,
            elementSpacing = $context.hasClass('fixed') ? contextOuterHeight : contextOuterHeight + $context.offset().top      

        if (fed.cache.$window.scrollTop() > elementSpacing) {
            if (!$context.hasClass('fixed')) {
                $context.removeClass('slide').addClass('fixed');
                setTimeout(function () {
                    $context.addClass('slide');
                }, fed.cache.timer.fast);
            }
        } else if (fed.cache.$window.scrollTop() < elementSpacing) {
            if ($context.hasClass('fixed')) {
                $context.removeClass('slide fixed');
                setTimeout(function () {
                    $context.addClass('slide');
                }, fed.cache.timer.fast);
            }
        }
    },
    generateMarkup: function (id, data) {
        var source = document.getElementById(id).innerHTML,
            template = Handlebars.compile(source);

        return template(data);
    }
}