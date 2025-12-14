$(document).ready(function ($) {
    var attributes;
    var table = $("#Customers-Table table");
    var attributeColIndex = table.find('th:contains("Marketing Attribute")').index();
    var idColIndex = table.find('th:contains("CustomerId")').index();

    $.getJSON('/customermarketingattributes', function () {
    }).done(function (data) {
        attributes = data;
        UpdateTable();
    });

    function UpdateTable() {
        table.find('td:nth-child(' + (attributeColIndex + 1) + ')').each(function () {
           if ($(this).text() === "") {
                $(this).html('<a class="set-attribute not-set">Add</a>');
            }
            else {
                $(this).data("attribute-id", $(this).text());
                var attribute = attributes.find(x => x.id === $(this).text());
                $(this).html('<a class="set-attribute">' + attribute.name + '</a>');
            }


        });
    }

    table.on("click", ".set-attribute", function () {

        if ($(this).find("select").length)
            return false;

        $(".select-attribute").remove();

        //$(this).empty();

        var attributeId = $(this).parents("td").data("attribute-id");

        console.log(attributeId);

        var s = $('<select />');

        $('<option value="">None</option>').appendTo(s);

        attributes.forEach(attribute => $('<option value="' + attribute.id + '"   ' + (attribute.id == attributeId ? ' selected="true"' : '') + ' >' + attribute.name + '</option>').appendTo(s));

        var d = $('<div class="select-attribute" data-customer-id="' + $(this).parents("tr").find("td").eq(idColIndex).text().trim() + '"/>');
        s.appendTo(d);
        d.appendTo($(this));

        var left = $(document).outerWidth() - $(window).width();
        $('html, body').animate({ scrollLeft: $(this).offset().left }, 800);
    });

    table.on("change", ".select-attribute select", function () {

        var ddl = $(this);
        var td = ddl.parents("td");
        var customerId = ddl.parents("div").data("customer-id");
        var attributeId = ddl.val();

        $.getJSON('/customers/UpdateCustomerMarketingAttribute?customerMarketingAttributeId=' + attributeId + '&customerId=' + customerId, function (data) {
        }).done(function () {
            var attribute = attributes.find(x => x.id === attributeId);
            td.data("attribute-id", attributeId)
            if (attribute)
                td.html('<a class="set-attribute">' + attribute.name + '</a>');
            else
                td.html('<a class="set-attribute not-set">Add</a>');
        })
            .fail(function () {
                alert("Could not update attribute");
            });
    });

    $(document).mouseup(function (e) {
        var container = $(".select-attribute");
        if (!container.is(e.target)
            && container.has(e.target).length === 0) {
            container.remove();
        }
    });
});