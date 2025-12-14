$(document).ready(function () {
    var supplierList = $('#supplierslist');
    supplierList.insertAfter($('.reportForm'));    

    var idColIndex = $('.dynamicTable table').find('th:contains("Id")').index();

    $('input[name=suppliers]').on('change', function (c) {
        var val = $(this).val();
        console.log(val);
        $('.dynamicTable table tr').each(function () {
            if ($(this).find("td").eq(idColIndex).text() == val) {
                $(this).toggle();
            }
        });
    });

});