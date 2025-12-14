
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


$("#customerAgents").on("click", "a.delete-agent", function () {

    var agentRow = $(this).parents("tr");
    var agentId = $(this).data("agent-id");

    var r = confirm("Are you sure you want to delete the agent " + $(this).data("agent-name"));
    if (r === true) {
        $.getJSON('/customers/DeleteCustomerAgent?customerAgentId=' + agentId, function (data) {
        }).done(function () {
            agents = $.grep(agents, function (e) {
                return e.id !== agentId;
            });
            agentRow.remove();
            table.find('*[data-agent-id="' + agentId + '"]').html('<a class="set-agent">Add</a>');
                
            if (!agents.length)
                $(".no-agents").removeClass("hidden");
        }).fail(function () {
            alert("Could not delete agent");
        });
    }
});

$("#show-agent-form").on("click", function () {
    $(this).addClass("hidden");
    $('.agent-form input').val('');
    $(".agent-form").removeClass("hidden");
});

$("#add-agent").on("click", function () {
    var name = $('#agent-name').val();
    var email = $('#agent-email').val();

    if (name.length === 0) {
        alert("Please enter an agent name");
        return false;
    }
    if (!isEmail(email)) {
        alert("Please enter a valid email");
        return false;
    }

    $.ajax({
        url: '/customers/CreateCustomerAgent',
        type: 'post',
        dataType: 'json',
        contentType: 'application/json',
        success: function (agent) {
            agents.push(agent);
            $('<tr class="agent-details"><td>' + agent.name + '</td><td>' + agent.email + '</td><td><a data-agent-id="' + agent.id + '" data-agent-name="' + agent.name + '" class="delete-agent">delete</a></td></tr>').insertBefore('#customer-agents tr.agent-form');
            $("#show-agent-form").removeClass("hidden");
            $(".agent-form").addClass("hidden");
            $(".no-agents").addClass("hidden");
        },
        error: function () {
            alert("Could not create agent");
        },
        data: JSON.stringify({ name: name, email: email })
    });

});

function isEmail(email) {
    var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    return regex.test(email);
}
