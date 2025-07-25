$(document).ready(function () {
    _debt_guarantee.Init();
    $("#OrderId").select2({
        theme: 'bootstrap4',
        placeholder: "Mã đơn hàng",
        maximumSelectionLength: 1,
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
    $("#ClientId").select2({
        theme: 'bootstrap4',
        placeholder: "khách hàng",
        maximumSelectionLength: 1,
        ajax: {
            url: "/Contract/ClientSuggestion",
            type: "post",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                var query = {
                    txt_search: params.term,
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (response) {
                return {
                    results: $.map(response.data, function (item) {
                        return {
                            text: item.clientname + ' - ' + item.email + ' - ' + item.phone,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });
    const selectBtnStatus = document.querySelector(".select-btn-Status-type")
    const itemsStatus = document.querySelectorAll(".item-Status-type");
    $(document).click(function (event) {

        var $target = $(event.target);
        if (!$target.closest('#select-btn-Status-type').length) {//checkbox_trans_type_
            if ($('#list-item-Status').is(":visible") && !$target[0].id.includes('Status_type_text') && !$target[0].id.includes('Status_type')
                && !$target[0].id.includes('list-item-Status') && !$target[0].id.includes('checkbox_Status_type')) {
                selectBtnStatus.classList.toggle("open");
            }
        }


    });
    selectBtnStatus.addEventListener("click", (e) => {
        e.preventDefault();
        selectBtnStatus.classList.toggle("open");
    });
    itemsStatus.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");
            let checked = document.querySelectorAll(".checked"),
                btnText = document.querySelector(".btn-text-Status-type");
            listStatus = []
            let checked_list = []

            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (id.includes('Status_type_')) {
                    checked_list.push(checked[i]);
                    listStatus.push(parseInt(id.replace('Status_type_', '')))
                }
            }

            if (checked_list && checked_list.length > 0) {
                btnText.innerText = `${checked_list.length} Selected`;
            } else {
                btnText.innerText = "Tất cả trạng thái";
            }
        });
    })
});
let listStatus = [];
let PageIndex = 1;
let isPickerApprove = false;
var _debt_guarantee = {
    Init: function () {
        var _searchModel = {
            Code: null,
            Status: null,
            ClientId: null,
            DepartmentId: null,
            CreateTime: null,
            ToDateTime: null,
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
            Status: listStatus.toString(),
            OrderId: $('#OrderId').select2("val"),
            ClientId: $('#ClientId').select2("val"),
            DepartmentId: $('#DepartmentId').val(),
            CreateTime: null,
            ToDateTime: null,
            PageIndex: PageIndex,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        }
        if (_searchModel.PageSize == undefined) _searchModel.PageSize = 20;
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
    Export: function () {
        var searchModel = _debt_guarantee.getModel();
        searchModel.PageIndex = -1;
        _global_function.AddLoading()
        $.ajax({
            url: "/DebtGuarantee/ExportExcel",
            type: "Post",
            data: searchModel,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#btnExport').prop('disabled', false);
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
                $('#icon-export').removeClass('fa-spinner fa-pulse');
                $('#icon-export').addClass('fa-file-excel-o');
            }
        });
    }
}