$(document).ready(function () {
    $(".add-to-fresh-sales").on("click", ".add", function () {

        var td = $(this).parent();
        $.getJSON('/customers/AddCrmLead/' + td.data("customer-id"), function (data) {
            if (data)
                td.addClass("added");
            else
                showFailure();
        }).fail(function (data) {
            showFailure();
        });
    });
});

function showFailure() {
    alert("Customer could not be added to Freshsales");
}