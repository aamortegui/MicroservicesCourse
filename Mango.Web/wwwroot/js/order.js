var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else {
        if (url.includes("readyforpickup")) {
            loadDataTable("readyforpickup");
        } else {
            if (url.includes("cancelled")) {
                loadDataTable("cancelled");
            } else {
                loadDataTable("all");
            }
        }
    }
});

function loadDataTable(status)
{//It's mandatory that define columns here are the same number and type in OrderIndex.html. Otherwise it will generate an error
    dataTable = $('#tblData').DataTable({
        "order": [[0, "desc"]], //Order by first column (orderHeaderId) in descending order
        "ajax": {
            url: "/order/getall?status=" + status
        },
        "columns": [
            { data: 'orderHeaderId', "width": "5%"},
            { data: 'email', "width": "25%" },
            { data: 'name', "width": "20%" },
            { data: 'phone', "width": "10%" },
            { data: 'status', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'orderHeaderId',
                "render": function (data) {
                    //alt+96 is the same as ` in the keyboard
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                    </div>`
                },
                "width": "10%"
            }
        ]
    })
}