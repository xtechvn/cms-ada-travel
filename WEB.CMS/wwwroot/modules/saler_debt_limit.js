let isResetTab = false;
let PageIndex = 1;
$(document).ready(function () {
    Saler_Dbt_Limit.Init();
});
var Saler_Dbt_Limit = {
    Init: function () {
        var _searchModel = {
            Code: $('#Code').val(),
            Status: [2, 3].toString(),
            OrderId: $('#OrderId').select2("val"),
            ClientId: $('#ClientId').select2("val"),
            DepartmentId: $('#DepartmentId').val(),
            CreateTime: null,
            ToDateTime: null,
            PageIndex: 1,
            PageSize: 20,
        }
        Saler_Dbt_Limit.Search(_searchModel);
    },
    Search: function (input) {
        $('#imgLoading').show();
        $.ajax({
            url: "/order/SearchSaleDebtLimit",
            type: "post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid-data').html(result);
                var total = $('#data-total-record').val();
                $('#total-article-filter').text(total);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id') + 1;
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    } else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                Saler_Dbt_Limit.checkCheckBox();
                Saler_Dbt_Limit.SetActive(input.StatusTab);
                $('#selectPaggingOptions').val(input.pageSize).attr("selected", "selected");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    OnSearchStatus: function (status) {
        tabActive = status;

        PageIndex = 1;
        //pageSize = 1;
        this.SearchData();

    },
    onSelectPageSize: function () {
        this.SearchData(1);
    },
    OnPaging: function (value) {
        pageSize = 1;
        if (value > 0) {
            PageIndex = value;
            this.SearchData(value);
        }
    },
    SearchData: function (value) {
        var _searchModel = {
            Code: $('#Code').val(),
            Status: [2,3].toString(),
            OrderId: $('#OrderId').select2("val"),
            ClientId: $('#ClientId').select2("val"),
            DepartmentId: $('#DepartmentId').val(),
            CreateTime: null,
            ToDateTime: null,
            PageIndex: value,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        }

        let objSearch = {
            searchModel: _searchModel,
        };
        this.Search(objSearch);
        this.changeSetting(15);
        $(document).load().scrollTop(0);

    },
    checkCheckBox: function () {
        var len = $(".grid-slide input[type='checkbox']:checked").length;

        if (len > 9) {
            $('.table-responsive').removeClass('table-gray');
            $('.table-responsive').addClass('table-scroll');
        } else {
            $('.table-responsive').removeClass('table-scroll');
            $('.table-responsive').addClass('table-gray');
        }
    },
    SetActive: function (status) {
        $('#countSttAll').removeClass('active')
        $('#countSttCheck').removeClass('active')
        $('#countSttNotDone').removeClass('active')
        $('#countSttDone').removeClass('active')
        $('#countSttErr').removeClass('active')
        $('#countSttErr').removeClass('active')
        $('#countSttNews').removeClass('active')
        if (status == 99)
            $('#countSttAll').addClass('active')
        if (status == 1)
            $('#countSttCheck').addClass('active')
        if (status == 2)
            $('#countSttNotDone').addClass('active')
        if (status == 3)
            $('#countSttDone').addClass('active')
        if (status == 4)
            $('#countSttErr').addClass('active')
        if (status == 0)
            $('#countSttNews').addClass('active')
    },
} 
