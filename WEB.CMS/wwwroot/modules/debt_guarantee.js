$(document).ready(function () {
    _debt_guarantee.Init();
    $("#OrderId").select2({
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
let isPickerApprove = false;
var _debt_guarantee = {
    Init: function () {
        var _searchModel = {
            Code: null,
            Status: null,
            OrderId: null,
            CreateTime: null,
            ToDateTime: null,
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
            CreateTime: null,
            ToDateTime: null,
            PageIndex: PageIndex,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        }
        if (isPickerApprove) {
            _searchModel.CreateTime = $('#filter_date_daterangepicker').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            _searchModel.ToDateTime = $('#filter_date_daterangepicker').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");

        }
        return _searchModel;
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