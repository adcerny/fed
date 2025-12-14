/*
 * ---------------------------------------- *
 * fed JavaScripts                       *
 * ---------------------------------------- *
 */
// project namespace
window.fed || (window.fed = {});

$(document).ready(function () {
    $.extend(true, fed, {
        cache: toolkit.cache,
        config: toolkit.config,
        helpers: toolkit.helpers
    });

    $("article > hgroup").click(function () {
        $(this).next("div").slideToggle("fast");
    });

    // custom cache
    fed.cache.$window = $(window);
    fed.cache.$html = $('html');
    fed.cache.$body = fed.cache.$html.find('body');
    fed.cache.$sortingListMod = fed.cache.$body.find('.sorting-list-mod');

    // custom delays
    fed.cache.timer = {
        veryFast: 150,
        fast: 300,
        moderated: 500,
        long: 1000
    };
});