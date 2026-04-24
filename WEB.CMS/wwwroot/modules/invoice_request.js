let isPickerCreatedInvoiceRequest = false
let isPickerPayment = false
let isPickerPlan = false
let isPickerApproved = false
let listStatusType = []
let isResetTab = true
let listUserCreate = []

let fields = {
    invoiceRequestNo: null,
    invoiceNo: null,
    invoiceCode: null,
    orderNo: null,
    status: -1,
    statusMulti: [],
    createByIds: null,
    verifyByIds: null,
    clientId: null,
    currentPage: 1,
    pageSize: 20,
    createDateFromStr: null,
    createDateToStr: null,
    planDateFromStr: null,
    planDateToStr: null,
    exportDateFromStr: null,
    exportDateToStr: null,
    verifyDateFromStr: null,
    verifyDateToStr: null,
    isHasBill: null,
}
let cookieFilterInvoiceRequestName = 'invoice_request_search_cache'
let cookieInvoiceRequest_filter_Client = 'cookieInvoiceRequest_filter_Client';
let cookieInvoiceRequest_filter_CreateBy = 'cookieInvoiceRequest_filter_CreateBy';
let cookieInvoiceRequest_filter_VerifyBy = 'cookieInvoiceRequest_filter_VerifyBy';

$(document).ready(function () {
    $('input').attr('autocomplete', 'off');
    $('input').keyup(function (e) {
        if (e.which === 13) {
            _invoice_request_service.OnPaging(1);
        }
    });
    $('input').attr('autocomplete', 'off');
    //multi select
    const selectBtnStatusType = document.querySelector(".select-btn-status-type")
    const itemsStatusType = document.querySelectorAll(".item-status-type");
    $(document).click(function (event) {
        var $target = $(event.target);
        if (!$target.closest('#select-btn-status-type').length) {//checkbox_trans_type_
            if ($('#list-item-status').is(":visible") && !$target[0].id.includes('status_type_text') && !$target[0].id.includes('status_type')
                && !$target[0].id.includes('list-item-status') && !$target[0].id.includes('checkbox_status_type')) {
                selectBtnStatusType.classList.toggle("open");
            }
        }
        if (!$target.closest('#select-btn-service-type').length) {
            if ($('#list-item-service').is(":visible") && !$target[0].id.includes('service_type_text') && !$target[0].id.includes('service_type')
                && !$target[0].id.includes('list-item-service') && !$target[0].id.includes('checkbox_service_type')) {
                selectBtnServiceType.classList.toggle("open");
            }
        }
        if (!$target.closest('#select-btn-payment-type').length) {
            if ($('#list-item-payment').is(":visible") && !$target[0].id.includes('payment_type_text') && !$target[0].id.includes('payment_type')
                && !$target[0].id.includes('list-item-payment') && !$target[0].id.includes('checkbox_payment_type')) {
                selectBtnPaymentType.classList.toggle("open");
            }
        }
    });
    if (selectBtnStatusType !== null && selectBtnStatusType !== undefined)
        selectBtnStatusType.addEventListener("click", (e) => {
            e.preventDefault();
            selectBtnStatusType.classList.toggle("open");
        });

    itemsStatusType.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");
            let checked = document.querySelectorAll(".checked"),
                btnText = document.querySelector(".btn-text-status-type");
            listStatusType = []
            let checked_list = []

            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (id.includes('status_type_')) {
                    checked_list.push(checked[i]);
                    listStatusType.push(parseInt(id.replace('status_type_', '')))
                }
            }
            _invoice_request_service.SearchParam.statusMulti = listStatusType

            if (checked_list && checked_list.length > 0) {
                btnText.innerText = `${checked_list.length} Selected`;
            } else {
                btnText.innerText = "Tất cả trạng thái";
            }
        });
    })
    //end multi select
    var SearchParam = _invoice_request_service.GetParam()
    _invoice_request_service.Init(SearchParam);
    $(".input").on('keyup', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            _invoice_request_service.OnPaging(1)
        }
    });
    setTimeout(function () {
        $('#token-input-client').css('height', 30)
        $('#token-input-supplier').css('height', 30)
    }, 800)

    $("#token-input-client").select2({
        theme: 'bootstrap4',
        placeholder: "Tên KH, Điện Thoại, Email",
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
    $("#createdBy").select2({
        theme: 'bootstrap4',
        placeholder: "Người tạo",
        ajax: {
            url: "/PaymentRequest/GetUserSuggestionList",
            type: "post",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                var query = {
                    name: params.term,
                }
                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (response) {
                return {
                    results: $.map(response, function (item) {
                        return {
                            text: item.fullname,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });
    $("#approveBy").select2({
        theme: 'bootstrap4',
        placeholder: "Người duyệt",
        ajax: {
            url: "/User/GetUserSuggestionList",
            type: "post",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                var query = {
                    name: params.term,
                }
                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (response) {
                return {
                    results: $.map(response, function (item) {
                        return {
                            text: item.fullname,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });

    if (_global_function.getCookie(cookieFilterInvoiceRequestName) != null && window.location.href.indexOf("InvoiceRequest/Index") != -1) {
        let cookie = JSON.parse(_global_function.getCookie(cookieFilterInvoiceRequestName))
        fields = cookie
        _invoice_request_service.GetCacheFilter()
        _global_function.eraseCookie(cookieFilterInvoiceRequestName)
        
        // Pass the cached fields directly or recreate them with GetParam.
        var SearchParam = _invoice_request_service.GetParam()
        // Override with safe cached paging if any
        SearchParam.currentPage = fields.currentPage || 1;
        if (fields.status !== undefined) {
             SearchParam.status = fields.status;
        }
        _invoice_request_service.Init(SearchParam);
    } else {
        var SearchParam = _invoice_request_service.GetParam()
        _invoice_request_service.Init(SearchParam);
    }
})
var _invoice_request_service = {
    Init: function (objSearch) {
        $('#divClient').show()
        $('#divSupplier').show()
        this.SearchParam = objSearch;
        this.Search(objSearch);
    },
    ActionSearch: function () {
        isResetTab = true
        this.OnPaging(1)
    },
    GetParam: function () {
        var select_client = $('#token-input-client').select2("val");
        var createdBy = $('#createdBy').select2("val");
        var approveBy = $('#approveBy').select2("val");
        if (select_client != null) {
            let cookiename = {
                id: select_client[0],
                nameselect: $('#token-input-client').select2('data')[0].text
            }
            _global_function.eraseCookie(cookieInvoiceRequest_filter_Client)
            window.localStorage.setItem(cookieInvoiceRequest_filter_Client, JSON.stringify(cookiename))
        } else {
            window.localStorage.removeItem(cookieInvoiceRequest_filter_Client)
        }
        if (createdBy != null) {
            let cookiename = {
                id: createdBy[0],
                nameselect: $('#createdBy').select2('data')[0].text
            }
            _global_function.eraseCookie(cookieInvoiceRequest_filter_CreateBy)
            window.localStorage.setItem(cookieInvoiceRequest_filter_CreateBy, JSON.stringify(cookiename))
        } else {
            window.localStorage.removeItem(cookieInvoiceRequest_filter_CreateBy)
        }
        if (approveBy != null) {
            let cookiename = {
                id: approveBy[0],
                nameselect: $('#approveBy').select2('data')[0].text
            }
            _global_function.eraseCookie(cookieInvoiceRequest_filter_VerifyBy)
            window.localStorage.setItem(cookieInvoiceRequest_filter_VerifyBy, JSON.stringify(cookiename))
        } else {
            window.localStorage.removeItem(cookieInvoiceRequest_filter_VerifyBy)
        }

        var objSearch = {
            invoiceRequestNo: $('#billNo').val(),
            invoiceNo: $('#content').val(),
            invoiceCode: $('#invoiceCode').val(),
            orderNo: $('#orderNo').val(),
            status: -1,
            statusMulti: listStatusType,
            createByIds: $('#createdBy').select2("val"),
            verifyByIds: $('#approveBy').select2("val"),
            clientId: $('#token-input-client').val(),
            currentPage: 1,
            pageSize: 20
        }

        if ($('#filter_date_create_daterangepicker').data('daterangepicker') !== undefined &&
            $('#filter_date_create_daterangepicker').data('daterangepicker') != null && isPickerCreatedInvoiceRequest) {
            objSearch.createDateFromStr = $('#filter_date_create_daterangepicker').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            objSearch.createDateToStr = $('#filter_date_create_daterangepicker').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            objSearch.createDateFromStr = null
            objSearch.createDateToStr = null
        }
        if ($('#filter_plan_date').data('daterangepicker') !== undefined &&
            $('#filter_plan_date').data('daterangepicker') != null && isPickerPlan) {
            objSearch.planDateFromStr = $('#filter_plan_date').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            objSearch.planDateToStr = $('#filter_plan_date').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            objSearch.planDateFromStr = null
            objSearch.planDateToStr = null
        }
        if ($('#filter_export_date').data('daterangepicker') !== undefined &&
            $('#filter_export_date').data('daterangepicker') != null && isPickerPayment) {
            objSearch.exportDateFromStr = $('#filter_export_date').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            objSearch.exportDateToStr = $('#filter_export_date').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            objSearch.exportDateFromStr = null
            objSearch.exportDateToStr = null
        }
        if ($('#filter_date_approve').data('daterangepicker') !== undefined &&
            $('#filter_date_approve').data('daterangepicker') != null && isPickerApproved) {
            objSearch.verifyDateFromStr = $('#filter_date_approve').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            objSearch.verifyDateToStr = $('#filter_date_approve').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            objSearch.verifyDateFromStr = null
            objSearch.verifyDateToStr = null
        }
        var isHasBill = $('#isHasBill').val()
        if (parseInt(isHasBill) == -1)
            objSearch.isHasBill = null
        if (parseInt(isHasBill) == 0)
            objSearch.isHasBill = false
        if (parseInt(isHasBill) == 1)
            objSearch.isHasBill = true
        return objSearch
    },
    SetCacheFilter: function (objSearch) {
        fields.invoiceRequestNo = objSearch.invoiceRequestNo
        fields.invoiceNo = objSearch.invoiceNo
        fields.invoiceCode = objSearch.invoiceCode
        fields.orderNo = objSearch.orderNo
        fields.status = objSearch.status
        fields.createByIds = objSearch.createByIds
        fields.verifyByIds = objSearch.verifyByIds
        fields.statusMulti = objSearch.statusMulti
        fields.clientId = objSearch.clientId
        fields.currentPage = objSearch.currentPage
        fields.createDateFromStr = objSearch.createDateFromStr
        fields.createDateToStr = objSearch.createDateToStr
        fields.planDateFromStr = objSearch.planDateFromStr
        fields.planDateToStr = objSearch.planDateToStr
        fields.exportDateFromStr = objSearch.exportDateFromStr
        fields.exportDateToStr = objSearch.exportDateToStr
        fields.verifyDateFromStr = objSearch.verifyDateFromStr
        fields.verifyDateToStr = objSearch.verifyDateToStr
        fields.isHasBill = objSearch.isHasBill
        _global_function.setCookie(cookieFilterInvoiceRequestName, JSON.stringify(fields), 100)
    },
    GetCacheFilter: function () {
        let cookie = _global_function.getCookie(cookieFilterInvoiceRequestName)
        fields = JSON.parse(cookie)
        $('#billNo').val(fields.invoiceRequestNo)
        $('#content').val(fields.invoiceNo)
        $('#invoiceCode').val(fields.invoiceCode)
        $('#orderNo').val(fields.orderNo)
        if (window.localStorage.getItem(cookieInvoiceRequest_filter_Client) != null) {
            var cookie1 = window.localStorage.getItem(cookieInvoiceRequest_filter_Client)
            var client = JSON.parse(cookie1)
            $('#token-input-client').html('<option selected value = ' + client.id + '> ' + client.nameselect + '</option>')
            _global_function.eraseCookie(cookieInvoiceRequest_filter_Client)
        }
        if (window.localStorage.getItem(cookieInvoiceRequest_filter_CreateBy) != null) {
            var cookie1 = window.localStorage.getItem(cookieInvoiceRequest_filter_CreateBy)
            var createBy = JSON.parse(cookie1)
            $('#createdBy').html('<option selected value = ' + createBy.id + '> ' + createBy.nameselect + '</option>')
        }
        if (window.localStorage.getItem(cookieInvoiceRequest_filter_VerifyBy) != null) {
            var cookie1 = window.localStorage.getItem(cookieInvoiceRequest_filter_VerifyBy)
            var approveBy = JSON.parse(cookie1)
            $('#approveBy').html('<option selected value = ' + approveBy.id + '> ' + approveBy.nameselect + '</option>')
            _global_function.eraseCookie(cookieInvoiceRequest_filter_VerifyBy)
        }
        
        if (fields.statusMulti != null && fields.statusMulti !== undefined && fields.statusMulti.length > 0) {
            var btnTextStatus = document.querySelector(".btn-text-status-type");
            btnTextStatus.innerText = `${fields.statusMulti.length} Selected`;
            for (var i = 0; i < fields.statusMulti.length; i++) {
                $('#status_type_' + fields.statusMulti[i] + '').addClass('checked')
            }
            listStatusType = fields.statusMulti
        }
        
        if (fields.createDateFromStr != null && fields.createDateFromStr != undefined && fields.createDateFromStr !== '') {
            $('input[name="datetimeCreate"]').daterangepicker({
                autoUpdateInput: true,
                autoApply: true,
                showDropdowns: true,
                drops: 'down',
                locale: {
                    format: 'DD/MM/YYYY'
                }
            });
            $('input[name="datetimeCreate"]').data('daterangepicker').setStartDate(fields.createDateFromStr);
            $('input[name="datetimeCreate"]').data('daterangepicker').setEndDate(fields.createDateToStr);
        }
        if (fields.exportDateFromStr != null && fields.exportDateFromStr != undefined && fields.exportDateFromStr !== '') {
            $('input[name="datetimePayment"]').daterangepicker({
                autoUpdateInput: true,
                autoApply: true,
                showDropdowns: true,
                drops: 'down',
                locale: {
                    format: 'DD/MM/YYYY'
                }
            });
            $('input[name="datetimePayment"]').data('daterangepicker').setStartDate(fields.exportDateFromStr);
            $('input[name="datetimePayment"]').data('daterangepicker').setEndDate(fields.exportDateToStr);
        }
        if (fields.verifyDateFromStr != null && fields.verifyDateFromStr != undefined && fields.verifyDateFromStr !== '') {
            $('input[name="datetimeApprove"]').daterangepicker({
                autoUpdateInput: true,
                autoApply: true,
                showDropdowns: true,
                drops: 'down',
                locale: {
                    format: 'DD/MM/YYYY'
                }
            });
            $('input[name="datetimeApprove"]').data('daterangepicker').setStartDate(fields.verifyDateFromStr);
            $('input[name="datetimeApprove"]').data('daterangepicker').setEndDate(fields.verifyDateToStr);
        }

        if (fields.isHasBill) {
            $("#isHasBill").val(1).prop('selected', true);
            $("#select2-isHasBill-container").html(' Có hóa đơn ')
        }
        if (fields.isHasBill === false) {
            $("#isHasBill").val(0).prop('selected', true);
            $("#select2-isHasBill-container").html(' Chưa có hóa đơn ')
        }
    },
    BackToList: function () {
        window.location.href = '/InvoiceRequest/Index'
    },
    Export: function () {
        $('#btnExport').prop('disabled', true);
        $('#icon-export').removeClass('fa-file-excel-o');
        $('#icon-export').addClass('fa-spinner fa-pulse');
        var objSearch = this.GetParam()
        objSearch.currentPage = 1;
        this.SearchParam = objSearch
        _global_function.AddLoading()
        $.ajax({
            url: "/InvoiceRequest/ExportExcel",
            type: "Post",
            data: this.SearchParam,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#btnExport').prop('disabled', false);
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    },
    Search: function (input, is_count_status = true, isClickSearch = false) {
        window.scrollTo(0, 0);
        //$('#imgLoading').show();
        _global_function.AddLoading()
        $.ajax({
            url: "/InvoiceRequest/Search",
            type: "Post",
            data: input,
            success: function (result) {
                _global_function.RemoveLoading()
                //$('#imgLoading').hide();
                $('#grid_data_invoice').html(result);
                if (isClickSearch) {
                    var modelCache = _invoice_request_service.GetParam()
                    _invoice_request_service.SetCacheFilter(modelCache)
                }
            }
        });
        if (is_count_status) {
            this.OnCountStatus()
            this.SetActive(input.status !== undefined && input.status !== null ? input.status : -1)
        }
    },
    OnPaging: function (value, isClickSearch = true) {
        var objSearch = this.GetParam()
        objSearch.currentPage = value;
        this.SearchParam = objSearch
        this.Search(objSearch, true, isClickSearch);
    },
    OnChangeAccountClientId: function (value) {
        listUserCreate = []
        var searchobj = this.SearchParam;
        var listId = value.split(',')
        searchobj.createByIds = []
        for (var i = 0; i < listId.length; i++) {
            searchobj.createByIds.push(parseInt(listId[i]))
        }
        searchobj.currentPage = 1;
        listUserCreate = searchobj.createByIds
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },
    OnChangeClient: function (value) {
        var searchobj = this.SearchParam;
        var listId = value.split(',')
        searchobj.clientId = null
        for (var i = 0; i < listId.length; i++) {
            searchobj.createByIds.push(parseInt(listId[i]))
        }
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },
    Add: function () {
        let title = 'Thêm yêu cầu xuất hóa đơn';
        let url = '/InvoiceRequest/Add';
        var param = {
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    Edit: function (invoiceRequestId) {
        let title = 'Chỉnh sửa yêu cầu xuất hóa đơn';
        let url = '/InvoiceRequest/Edit?invoiceRequestId=' + invoiceRequestId;
        var param = {
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    OnCountStatus: function () {
        var objSearch = this.GetParam();
        _global_function.AddLoading()
        $.ajax({
            url: "/InvoiceRequest/CountStatus",
            type: "Post",
            data: objSearch,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#countSttDraft').text('Lưu nháp (0)')
                $('#countSttWaitDepartmentLeadApprove').text('Chờ TBP duyệt (0)')
                $('#countSttWaitDirectorApprove').text('Chờ KTT duyệt (0)')
                $('#countSttReject').text('Từ chối (0)')
                $('#countSttApprove').text('Đã duyệt (0)')
                $('#countSttFinish').text('Hoàn thành (0)')
                $('#countSttAll').text('Tất cả (0)')
                if (result.data.length > 0) {
                    for (var i = 0; i < result.data.length; i++) {
                        if (result.data[i].status == -1) {
                            $('#countSttAll').text('Tất cả (' + result.data[i].total + ')')
                        }
                        if (result.data[i].status == 0) {
                            $('#countSttDraft').text('Lưu nháp (' + result.data[i].total + ')')
                        }
                        if (result.data[i].status == 1) {
                            $('#countSttReject').text('Từ chối (' + result.data[i].total + ')')
                        }
                        if (result.data[i].status == 2) {
                            $('#countSttWaitDepartmentLeadApprove').text('Chờ TBP duyệt (' + result.data[i].total + ')')
                        }
                        //if (result.data[i].status == 3) {
                        //    $('#countSttWaitDirectorApprove').text('Chờ GD duyệt (' + result.data[i].total + ')')
                        //}
                        if (result.data[i].status == 4) {
                            $('#countSttApprove').text('Đã duyệt (' + result.data[i].total + ')')
                        }
                        if (result.data[i].status == 5) {
                            $('#countSttFinish').text('Hoàn thành (' + result.data[i].total + ')')
                        }
                        if (result.data[i].status == 6) {
                            $('#countSttWaitDirectorApprove').text('Chờ KTT duyệt (' + result.data[i].total + ')')
                        }
                    }
                }
            }
        });
    },
    OnSearchStatus: function (status) {
        $('#status').val(-1)
        isResetTab = false
        var objSearch = this.SearchParam;
        objSearch.currentPage = 1;
        objSearch.status = status;
        this.Search(objSearch, false);
        this.SetActive(status)
    },
    SetActive: function (status) {
        var objSearch = this.SearchParam
        if (!isResetTab && objSearch.currentPage > 1)
            return
        let status_choose = $('#status').val()
        $('#countSttAll').removeClass('active')
        $('#countSttDraft').removeClass('active')
        $('#countSttWaitDepartmentLeadApprove').removeClass('active')
        $('#countSttWaitDirectorApprove').removeClass('active')
        $('#countSttApprove').removeClass('active')
        $('#countSttReject').removeClass('active')
        if (status == -1) {
            $('#countSttAll').addClass('active')
        }
        if (status == 0) {
            $('#countSttDraft').addClass('active')
            if (status_choose !== undefined && status_choose !== null && status_choose !== '' && status_choose !== '1')
                $('#countSttDraft').addClass('disabled-a-tag')
            else
                $('#countSttDraft').removeClass('disabled-a-tag')
        }
        if (status == 2) {
            $('#countSttWaitDepartmentLeadApprove').addClass('active')
            if (status_choose !== undefined && status_choose !== null && status_choose !== '' && status_choose !== '2')
                $('#countSttWaitDepartmentLeadApprove').addClass('disabled-a-tag')
            else
                $('#countSttWaitDepartmentLeadApprove').removeClass('disabled-a-tag')
        }
        if (status == 3) {
            $('#countSttWaitDirectorApprove').addClass('active')
            if (status_choose !== undefined && status_choose !== null && status_choose !== '' && status_choose !== '3')
                $('#countSttWaitDirectorApprove').addClass('disabled-a-tag')
            else
                $('#countSttWaitDirectorApprove').removeClass('disabled-a-tag')
        }
        if (status == 1) {
            $('#countSttReject').addClass('active')
            if (status_choose !== undefined && status_choose !== null && status_choose !== '' && status_choose !== '1')
                $('#countSttReject').addClass('disabled-a-tag')
            else
                $('#countSttReject').removeClass('disabled-a-tag')
        }
        if (status == 4) {
            $('#countSttApprove').addClass('active')
            if (status_choose !== undefined && status_choose !== null && status_choose !== '' && status_choose !== '4')
                $('#countSttApprove').addClass('disabled-a-tag')
            else
                $('#countSttApprove').removeClass('disabled-a-tag')
        }
    },
    Approve: function (status) {
        _msgconfirm.openDialog("Phê duyệt yêu cầu xuất hóa đơn",
            "Bạn có chắc muốn phê duyệt mã yêu cầu " + $('#invoiceRequestNo').val() + " không?",
            function () {
                $.ajax({
                    url: "/InvoiceRequest/Approve",
                    type: "Post",
                    data: { 'invoiceRequestId': $('#requestId').val(), 'status': status },
                    success: function (result) {
                        if (result.isSuccess === true) {
                            _msgalert.success(result.message);
                            $.magnificPopup.close();
                            setTimeout(function () {
                                window.location.reload()
                            }, 1000)
                        } else {
                            _msgalert.error(result.message);
                        }
                    }
                });
            });
    },
    FinishRequest: function (id, status) {
        _msgconfirm.openDialog("Hoàn thành yêu cầu xuất hóa đơn",
            "Bạn có chắc muốn hoàn thành yêu cầu " + $('#invoiceRequestNo').val() + " không?",
            function () {
                _global_function.AddLoading()
                $.ajax({
                    url: "/InvoiceRequest/FinishRequest",
                    type: "Post",
                    data: {'id': id, 'status': status,},
                    success: function (result) {
                        _global_function.RemoveLoading()
                        if (result.isSuccess) {
                            _msgalert.success(result.message);
                            _global_function.ConfirmFileUpload($('.attachment-detail'), id)
                            setTimeout(function () { window.location.reload() }, 500)
                        } else {
                            _msgalert.error(result.message);
                        }
                    }
                });
            });
    },
    Delete: function (invoiceRequestId, invoiceRequestNo, isFromOrder = false) {
        if (invoiceRequestNo !== undefined && invoiceRequestNo !== null && invoiceRequestNo !== '')
            $('#invoiceRequestNo').val(invoiceRequestNo)
        else
            InvoiceRequestNo = $('#invoiceRequestNo').val()
        if (invoiceRequestId == null || invoiceRequestId == 0 || invoiceRequestId == '')
            invoiceRequestId = parseInt($('#requestId').val())
        _msgconfirm.openDialog("Xác nhận xóa yêu cầu xuất hóa đơn",
            "Bạn có chắc muốn xóa phiếu yêu cầu " + invoiceRequestNo + " không?",
            function () {
                $.ajax({
                    url: "/InvoiceRequest/Delete",
                    type: "Post",
                    data: { 'invoiceRequestId': invoiceRequestId },
                    success: function (result) {
                        if (result.isSuccess === true) {
                            _msgalert.success(result.message);
                            $.magnificPopup.close();
                            if (isFromOrder) {
                                setTimeout(function () {
                                    window.location.reload()
                                }, 1000)
                            } else {
                                setTimeout(function () {
                                    window.location.href = "/InvoiceRequest/Index"
                                }, 1000)
                            }
                        } else {
                            _msgalert.error(result.message);
                        }
                    }
                });
            });
    },
    Reject: function () {
        let title = 'Từ chối duyệt yêu cầu';
        let url = '/InvoiceRequest/Reject';
        var param = {
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    RejectRequest: function () {
        _invoice_request_service.ClearError()
        if ($('#noteReject').val() === null || $('#noteReject').val() === undefined || $('#noteReject').val() === '') {
            _invoice_request_service.DisplayError('validate-note-reject', 'Vui lòng nhập lý do từ chối')
            return
        }
        var note = $('#noteReject').val()
        if (note.length > 300) {
            _invoice_request_service.DisplayError('validate-note-reject', 'Nội dung từ chối không được quá 300 kí tự')
            return
        }
        _global_function.AddLoading()
        $.ajax({
            url: "/InvoiceRequest/RejectRequest",
            type: "Post",
            data: { 'invoiceRequestId': $('#requestId').val(), 'note': $('#noteReject').val() },
            success: function (result) {
                _global_function.RemoveLoading()
                if (result.isSuccess === true) {
                    _msgalert.success(result.message);
                    $.magnificPopup.close();
                    setTimeout(function () {
                        window.location.href = "/InvoiceRequest/Index"
                    }, 1000)
                } else {
                    _msgalert.error(result.message);
                }
            }
        });
    },
    ClearError: function () {
        $(".validate-note-reject").find('p').remove();
    },
    DisplayError: function (className, message) {
        $("." + className).find('p').remove();
        $("." + className).append("<p>" + message + "</p>");
        $("." + className).css("color", "red");
        $("." + className).css("font-size", "13px");
        $("." + className).css("margin-top", "3px");
    },
    History: function () {
        let title = 'Lịch sử duyệt yêu cầu xuất hóa đơn';
        let url = '/InvoiceRequest/History?requestId=' + $('#requestId').val();
        var param = {
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    FileAttachment: function (data_id, type, readonly = false, allowPreview = true) {
        _global_function.RenderFileAttachment($('.attachment_file'), data_id, type, readonly, allowPreview)
    },
}