$(document).ready(function () {
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
                    systemtype: "",
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

    _report_order.Init();
});

var _report_order = {
    Init: function () {
        _report_order.SearchData()
    },
    ExportExcel: function () {
        objSearch = _report_order.GetParam();
        objSearch.PageIndex = -1;

        this.searchModel = objSearch
        _global_function.AddLoading()
        $.ajax({
            url: "/ReportOrder/ExportExcel",
            type: "Post",
            data: this.searchModel,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#btnExport').prop('disabled', false);
                if (result.status==0) {
                    _msgalert.success(result.msg);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.msg);
                    _global_function.RemoveLoading()
                }

            }
        });
    },
    SearchData: function () {
        objSearch = this.GetParam();
        if (objSearch.PageSize == undefined) {
            objSearch.PageSize = 20;
        }
        _report_order.Search(objSearch);
    },
    Search: function (input) {
        _global_function.AddLoading()
        $.ajax({
            url: "/ReportOrder/Search",
            type: "post",
            data: input,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#imgLoading').hide();
                $('#grid-data').html(result);
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");
            },
        });
    },
    GetParam: function () {
        var FromDate; //Ngày bat dau
        var ToDate; //Ngày hết hạn

        if ($('#report_date').data('daterangepicker') !== undefined && $('#report_date').data('daterangepicker') != null) {
            FromDate = $('#report_date').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDate = $('#report_date').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            FromDate = null
            ToDate = null
        }

        let _searchModel = {
            OrderId: $('#OrderNo').select2("val"),
            CreateDateFrom: FromDate,
            CreateDateTo: ToDate,
            PageIndex: 1,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        };
        return _searchModel;
    },
    ClientOnPaging: function (values) {
        objSearch = this.GetParam();
        objSearch.PageIndex = values;
        if (objSearch.PageSize == undefined) {
            objSearch.PageSize = 20;
        }

        _report_order.Search(objSearch);
    },
    onSelectPageSize: function () {
        objSearch = this.GetParam();
        _report_order.Search(objSearch);
    },
}