
var categories;


$("#productcategoriestable").on("click", "a.update-category", function () {

    var categoryRow = $("#productcategoriestable").find("tr.to-update");
    console.log(categoryRow);
    if (categoryRow.length > 0) {
        categoryRow.removeClass("to-update");
    }
    $(this).parents("tr").addClass("to-update");
    var categoryId = $(this).data("category-id");
    var categoryName = $(this).data("category-name")

    $(".category-form").addClass("hidden");
    $(".no-categories").addClass("hidden");
    $('#updatecategorytable').show();
    $('#updatecategoryid').val(categoryId);
    $('#update-category-name').val(categoryName);
  

   });

$("#updatecategorytable").on("click", "a#updatecategory", function () {

    var categoryRow = $("#productcategoriestable").find("tr.to-update");
    $.ajax({
        url: '/productcategory/update',
        type: 'post',
        dataType: 'json',
        contentType: 'application/json',
        success: function (category) {
            categoryRow.find("td:first").html($('#update-category-name').val());
            $("#show-category-form").removeClass("hidden");
            $(".category-form").addClass("hidden");
            $(".no-categories").addClass("hidden");
            $('#updatecategorytable').hide();
            categoryRow.removeClass("to-update");            
            categoryRow.find("td:nth(1)").children("a.update-category").data("category-name", $('#update-category-name').val());
            $("#update-category-name").val('');
        },
        error: function () {
            alert("Could not update category");
        },
        data: JSON.stringify({ id: $('#updatecategoryid').val(), name: $('#update-category-name').val()})
    });
});

$("#updatecategorytable").on("click", "a#cancel", function () {
    var categoryRow = $("#productcategoriestable").find("tr.to-update");
    $('#updatecategorytable').hide();
    categoryRow.removeClass("to-update");
    $("#show-category-form").removeClass("hidden");
});

$("#show-category-form").on("click", function () {
    $(this).addClass("hidden");
    $('.category-form input').val('');
    $(".category-form").removeClass("hidden");
    $('#updatecategorytable').hide();
    var categoryRow = $("#productcategoriestable").find("tr.to-update");
    if (categoryRow.length > 0) {
        categoryRow.removeClass("to-update");
    }
});

$("a#cancel-add-category").on("click", function () {
    $("#show-category-form").removeClass("hidden");
    $('.category-form input').val('');
    $(".category-form").addClass("hidden");
    $('#updatecategorytable').hide();
    var categoryRow = $("#productcategoriestable").find("tr.to-update");
    if (categoryRow.length > 0) {
        categoryRow.removeClass("to-update");
    }
});

$("#add-category").on("click", function () {
    var name = $('#category-name').val();

    if (name.length == 0) {
        alert("Please enter an category name");
        return false;
    }

    $.ajax({
        url: '/productcategory/create',
        type: 'post',
        dataType: 'json',
        contentType: 'application/json',
        success: function (category) {
            $('<tr class="category-details"><td>' + category.name + '</td><td><a data-category-id="' + category.id + '" data-category-name="' + category.name + '" class="update-category">Update</a></td></tr>').insertBefore('#productcategoriestable tr.category-form');
            $("#show-category-form").removeClass("hidden");
            $(".category-form").addClass("hidden");
            $(".no-categories").addClass("hidden");
            $("#category-name").val('');
        },
        error: function () {
            alert("Could not create category");
        },
        data: JSON.stringify({ name: name })
    });

});
