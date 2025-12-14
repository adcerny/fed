$(function () {
    $(".tablesorter-metro-dark").tablesorter();
});
$(document).ready(function ($) {
    $(".anchor-row td:not(.button)").click(function () {
        window.location = $(this).parent().data("href");
    });
    $(".delete-button").click(function () {
        var orderDesc = $(this).data("company-name") + "'s " + $(this).data("order-name");
        if (confirm("Are you sure you want to delete " + orderDesc + "?"))
            $.ajax({
                url: $(this).closest("tr").data("href"),
                type: 'DELETE',
                success: function (result) {
                    location.reload();
                },
                fail: function (result) {
                    alert("Could not delete " + orderDesc);
                }
            });
    });
});