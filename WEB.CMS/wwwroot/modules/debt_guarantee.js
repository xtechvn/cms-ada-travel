﻿$(document).ready(function () {
    _debt_guarantee.Init();
    $("#OrderNo").select2({
        theme: 'bootstrap4',
        placeholder: "Mã đơn hàng",
        /* tags: true,*/
        ajax: {
            url: "/Order/OrderNoSuggestion",
            type: "post",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                var query = {
                    txt_search: params.term,
                    systemtype: $('input[name="SysTemType"]:checked').val(),
                }
                return query;
            },
            processResults: function (response) {
                return {
                    results: $.map(response.data, function (item) {
                        return {
                            text: item.orderno,
                            id: item.orderid,
                        }
                    })
                };
            },

            cache: true
        }
    });
});
let PageIndex = 1;
var _debt_guarantee = {
    Init: function () {
        var _searchModel = {
            Code: null,
            Status: null,
            OrderId: null,
            PageIndex: 1,
            PageSize: 20,
        }
        _debt_guarantee.Search(_searchModel)
    },
    Search: function (input) {
        $('#imgLoading').show();
        $.ajax({
            url: "/DebtGuarantee/GetList",
            type: "post",
            data: input,
            success: function (result) {
                $('#grid_data').html(result);

                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    SearchData: function () {
        var searchModel = _debt_guarantee.getModel();
        _debt_guarantee.Search(searchModel);
        $(document).load().scrollTop(0);

    },
    getModel: function () {
        var _searchModel = {
            Code: $('#Code').val(),
            Status: $('#Status').val(),
            OrderId: $('#OrderId').val(),
            PageIndex: PageIndex,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        }
    },
    SearchData: function () {

    },
    OnPaging: function (value) {
        if (value > 0) {
            PageIndex = value;
            this.SearchData();
        }
    },
    onSelectPageSize: function () {
        _debt_guarantee.SearchData();
    },
}