$(function () {
    $(".tablesorter-metro-dark").tablesorter();
});
$(document).ready(function ($) {
    $(".anchor-row td:not(.button)").click(function () {
        window.location = $(this).parent().data("href");
    });
    $(".delete-button").click(function () {
        var orderId = $(this).data("id");
        var orderDesc = $(this).data("company-name") + "'s " + $(this).data("order-name");
        var shortId = $(this).data("customer-short-id");
        if (confirm("Are you sure you want to delete " + orderDesc + "?"))
            $.ajax({
                url: '/recurringOrders/' + orderId,
                type: 'DELETE',
                success: function (result) {
                    window.location.href = '/customers/' + shortId;
                },
                fail: function (result) {
                    alert("Could not delete " + orderDesc);
                }
            });
    });

    $('form').submit(function () {

        if (!$('#HiddenItems input').length) {
            alert('Please add some products to the order.');
            return false;
        }
    });
});