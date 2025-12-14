$(document).ready(function ($) {
    var agents;
    var table = $("#Customers-Table table");
    var agentColIndex = table.find('th:contains("Agent")').index();
    var idColIndex = table.find('th:contains("CustomerId")').index();

    $.getJSON('/customers/GetCustomerAgents', function () {
    }).done(function (data) {
        agents = data;
        UpdateTable();
    });

    function UpdateTable() {

        table.find('td:nth-child(' + (agentColIndex + 1) + ')').each(function () {
            $(this).data("customer-id", $(this).parents("tr").find("td").eq(idColIndex).text().trim());
            if ($(this).text()) {
                $(this).data("agent-id", $(this).text());
                var agent = agents.find(x => x.id === $(this).text());
                $(this).html('<a class="set-agent">' + agent.initials + '</a>');
            }
            else
                $(this).html('<a class="set-agent not-set">Add</a>');

        });
    }

    $(document).on("click", ".set-agent", function () {

        if ($(this).find("select").length)
            return false;

        $(".select-agent").remove();

        //$(this).empty();

        var agentId = $(this).parents("td").data("agent-id");

        console.log(agentId);

        var s = $('<select />');

        $('<option value="">None</option>').appendTo(s);

        agents.forEach(agent => $('<option value="' + agent.id + '"   ' + (agent.id === agentId ? ' selected="true"' : '') + ' >' + agent.name + '</option>').appendTo(s));

        var d = $('<div class="select-agent" />');
        s.appendTo(d);
        d.appendTo($(this));

        var left = $(document).outerWidth() - $(window).width();
        $('html, body').animate({ scrollLeft: $(this).offset().left }, 800);
    });

    $(document).on("change", ".select-agent select", function () {

        var ddl = $(this);
        var td = ddl.parents("td");
        var customerId = td.data("customer-id");
        var display = td.data("agent-display");
        var agentId = ddl.val();

        $.getJSON('/customers/UpdateCustomerAgent?customerAgentId=' + agentId + '&customerId=' + customerId, function (data) {
        }).done(function () {
            var agent = agents.find(x => x.id === agentId);
            td.data("agent-id", agentId);
            if (agent)
                td.html('<a class="set-agent">' + (display === 'name' ? agent.name : agent.initials) + '</a>');
            else
                td.html('<a class="set-agent not-set">Add</a>');
        })
            .fail(function () {
                alert("Could not update agent");
            });
    });

    $(document).mouseup(function (e) {
        var container = $(".select-agent");
        if (!container.is(e.target)
            && container.has(e.target).length === 0) {
            container.remove();
        }
    });
});