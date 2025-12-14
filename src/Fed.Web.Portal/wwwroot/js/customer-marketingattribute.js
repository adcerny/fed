
var attributes;


$("#marketingattributestable").on("click", "a.update-attribute", function () {

    var attributeRow = $("#marketingattributestable").find("tr.to-update");
    console.log(attributeRow);
    if (attributeRow.length > 0) {
        attributeRow.removeClass("to-update");
    }
    $(this).parents("tr").addClass("to-update");
    var attributeId = $(this).data("attribute-id");
    var attributeName = $(this).data("attribute-name");
    var attributeDescription = $(this).data("attribute-description");

    $(".attribute-form").addClass("hidden");
    $(".no-attributes").addClass("hidden");
    $('#updatemarketingattributetable').show();
    $('#updateattributeid').val(attributeId);
    $('#update-attribute-name').val(attributeName);
    $('#update-attribute-description').val(attributeDescription);
   });

$("#updatemarketingattributetable").on("click", "a#updateattribute", function () {

    var attributeRow = $("#marketingattributestable").find("tr.to-update");
    $.ajax({
        url: '/customerMarketingAttribute/update',
        type: 'post',
        dataType: 'json',
        contentType: 'application/json',
        success: function (attribute) {
            attributeRow.find("td:first").html($('#update-attribute-name').val());
            attributeRow.find("td:nth-child(2)").html($('#update-attribute-description').val());
            $("#show-attribute-form").removeClass("hidden");
            $(".attribute-form").addClass("hidden");
            $(".no-attributes").addClass("hidden");
            $('#updatemarketingattributetable').hide();
            attributeRow.removeClass("to-update");            
            attributeRow.find("td:nth(2)").children("a.update-attribute").data("attribute-name", $('#update-attribute-name').val());
            attributeRow.find("td:nth(2)").children("a.update-attribute").data("attribute-description", $('#update-attribute-description').val());
            $("#update-attribute-name").val('');
            $("#update-attribute-description").val('');
        },
        error: function () {
            alert("Could not update attribute");
        },
        data: JSON.stringify({ id: $('#updateattributeid').val(), name: $('#update-attribute-name').val(), description: $('#update-attribute-description').val() })
    });
});

$("#updatemarketingattributetable").on("click", "a#cancel", function () {
    var attributeRow = $("#marketingattributestable").find("tr.to-update");
    $('#updatemarketingattributetable').hide();
    attributeRow.removeClass("to-update");
    $("#show-attribute-form").removeClass("hidden");
});



$("#show-attribute-form").on("click", function () {
    $(this).addClass("hidden");
    $('.attribute-form input').val('');
    $(".attribute-form").removeClass("hidden");
    $('#updatemarketingattributetable').hide();
    var attributeRow = $("#marketingattributestable").find("tr.to-update");
    if (attributeRow.length > 0) {
        attributeRow.removeClass("to-update");
    }
});

$("a#cancel-add-attribute").on("click", function () {
    $("#show-attribute-form").removeClass("hidden");
    $('.attribute-form input').val('');
    $(".attribute-form").addClass("hidden");
    $('#updateattributetable').hide();
    var categoryRow = $("#marketingattributestable").find("tr.to-update");
    if (categoryRow.length > 0) {
        categoryRow.removeClass("to-update");
    }
});


$("#add-attribute").on("click", function () {
    var name = $('#attribute-name').val();
    var description = $('#attribute-description').val();

    if (name.length == 0) {
        alert("Please enter an attribute name");
        return false;
    }

    $.ajax({
        url: '/customerMarketingAttribute/create',
        type: 'post',
        dataType: 'json',
        contentType: 'application/json',
        success: function (attribute) {
            $('<tr class="attribute-details"><td>' + attribute.name + '</td><td>' + attribute.description + '</td><td><a data-attribute-id="' + attribute.id + '" data-attribute-name="' + attribute.name + '" data-attribute-description="' + attribute.description + '" class="update-attribute">Update</a></td></tr>').insertBefore('#marketingattributestable tr.attribute-form');
            $("#show-attribute-form").removeClass("hidden");
            $(".attribute-form").addClass("hidden");
            $(".no-attributes").addClass("hidden");
            $("#attribute-name").val('');
            $("#attribute-description").val('');
        },
        error: function () {
            alert("Could not create attribute");
        },
        data: JSON.stringify({ name: name, description: description })
    });

});
