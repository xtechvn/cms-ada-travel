var _customer_managerV2_html = {
    html_Client_option: '<option class="select2-results__option" value="{Client_id}">{Client_name}</option>',
    html_Client_option_B2C: '<option class="select2-results__option" value="{Client_id}">B2C - {Client_name}</option>',

}
let fieldsV2 = {
    STT: true,
    MaKH: true,
    TenKH: true,
    LienHe: true,
    DoiTuong: false,
    LoaiKH: false,
    NhomKH: false,
    TongTT: false,
    VNPhuTrach: true,
    NgayTao: false,
    NgayCN: false,
    NgayDuyet: false,
    NguoiTao: false,
    Status: true,
    LastOrderDate: false,
    LastOrderDateToDay: false,
}
let _searchModel = {
    MaKH: null,
    UserId: null,
    CreatedBy: null,
    TenKH: null,
    Email: null,
    Phone: null,
    AgencyType: null,
    ClientType: null,
    PermissionType: null,
    PageIndex: 1,
    PageSize: 10,
    MinAmount: null,
    MaxAmount: null,
    ClientStatus: null,

};
let listClientTypeV2 = [];
let listClientStatusV2 = [];
let listPermisionTypeV2 = [];
let listUtmSourceV2 = [];
let cookieNameV2 = 'customer_manager_filterV2';
let cookiesearchModel = 'customer_manager_searchModelV2';
var timer;
let isPickerApprove = false;
var x = localStorage.getItem("cookiesearchModelV2");
var textClient = localStorage.getItem("textClient");
var textNV = localStorage.getItem("textNV");
var textNT = localStorage.getItem("textNT");
$(document).ready(function () {

    var user_Id = $('#id_userid').val();
    if (user_Id == null) {
        
        _customer_managerV2.LoaddataClient();
        
    }
   
    if (_customer_managerV2.getCookie('customer_manager_filterV2') != null) {
        let cookie = _customer_managerV2.getCookie(cookieNameV2)
        fieldsV2 = JSON.parse(cookie)
    } else {
        _customer_managerV2.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10)
    }

    if (_customer_managerV2.getCookie('customer_manager_searchModelV2') != null) {
        let cookie2 = _customer_managerV2.getCookie(cookiesearchModel)
        _searchModel = JSON.parse(cookie2)
    } else {
        _customer_managerV2.setCookie(cookiesearchModel, JSON.stringify(_searchModel), 10)
    }
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
    _customer_managerV2.checkShowHide();

    $("#client").select2({
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
    $("#txtNguoiTao").select2({
        theme: 'bootstrap4',
        placeholder: "Tên nhân viên, Email",
        ajax: {
            url: "/Order/UserSuggestion",
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
                            text: item.fullname + ' - ' + item.email,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });
    $("#CreatedBy").select2({
        theme: 'bootstrap4',
        placeholder: "Người tạo",
       
        ajax: {
            url: "/Order/UserSuggestion",
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
                            text: item.fullname ,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });
    $("#filter-client").select2({
        theme: 'bootstrap4',
        placeholder: "Bộ lọc đã lưu",
        maximumSelectionLength: 1,
        ajax: {
            url: "/CustomerManagerManual/GetSuggestionUserCache",
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
                    results: $.map(response, function (item) {
                        return {
                            text: item.CacheName,
                            id: item._id,
                        }
                    })
                };
            },
        },

    });
    _customer_managerV2.ClearlocalStorage();
    //var model = [{ url: '/', name: 'Trang chủ' }, { url: '/CustomerManagerManual/Index', name: 'Quản lý khách hàng', activated: true }]
    //_global_function.RenderBreadcumb(model)

    const selectClientType = document.querySelector(".select-btn-ClientType");
    const itemsClientType = document.querySelectorAll(".item-ClientType");
    const selectClientStatus = document.querySelector(".select-btn-ClientStatus");
    const itemsClientStatus = document.querySelectorAll(".item-ClientStatus");
    const selectPermisionType = document.querySelector(".select-btn-PermisionType");
    const itemsPermisionType = document.querySelectorAll(".item-PermisionType");
    const selectUtmSource = document.querySelector(".select-btn-UtmSource");
    const itemsUtmSource = document.querySelectorAll(".item-UtmSource");

    $(document).click(function (event) {
        var $target = $(event.target);

        if (!$target.closest('#ClientType').length) {
            if ($('#list-item-ClientType').is(":visible") && !$target[0].id.includes('ClientType_data') && !$target[0].id.includes('ClientType')
                && !$target[0].id.includes('list-item-ClientType') && !$target[0].id.includes('checkbox_ClientType')) {
                selectClientType.classList.toggle("open");
            }
        }


    });
    $(document).click(function (event) {
        var $target = $(event.target);

        if (!$target.closest('#ClientStatus').length) {
            if ($('#list-item-ClientStatus').is(":visible") && !$target[0].id.includes('ClientStatus_data') && !$target[0].id.includes('ClientStatus')
                && !$target[0].id.includes('list-item-ClientStatus') && !$target[0].id.includes('checkbox_ClientStatus')) {
                selectClientStatus.classList.toggle("open");
            }
        }
    });
    $(document).click(function (event) {
        var $target = $(event.target);

        if (!$target.closest('#PermisionType').length) {
            if ($('#list-item-PermisionType').is(":visible") && !$target[0].id.includes('PermisionType_data') && !$target[0].id.includes('PermisionType')
                && !$target[0].id.includes('list-item-PermisionType') && !$target[0].id.includes('checkbox_PermisionType')) {
                selectPermisionType.classList.toggle("open");
            }
        }
    });
    $(document).click(function (event) {
        var $target = $(event.target);

        if (!$target.closest('#UtmSource').length) {
            if ($('#list-item-UtmSource').is(":visible") && !$target[0].id.includes('UtmSource_data') && !$target[0].id.includes('UtmSource')
                && !$target[0].id.includes('list-item-UtmSource') && !$target[0].id.includes('checkbox_UtmSource')) {
                selectUtmSource.classList.toggle("open");
            }
        }
    });
    selectClientType.addEventListener("click", (e) => {
        e.preventDefault();
        selectClientType.classList.toggle("open");
    });
    selectClientStatus.addEventListener("click", (e) => {
        e.preventDefault();
        selectClientStatus.classList.toggle("open");
    });
    selectPermisionType.addEventListener("click", (e) => {
        e.preventDefault();
        selectPermisionType.classList.toggle("open");
    });
    selectUtmSource.addEventListener("click", (e) => {
        e.preventDefault();
        selectUtmSource.classList.toggle("open");
    });
    itemsClientType.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");

            let checked = document.querySelectorAll("#list-item-ClientType .checked"),
                btnText = document.querySelector(".btn-text-ClientType");
            let listService = []
             listClientTypeV2= []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (listClientTypeV2 && id.includes('ClientType_data_')) {
                    listService.push(checked[i]);
                }
                listClientTypeV2.push(parseInt(id.replace('ClientType_data_', '')))

            }
            if (listService.length > 0) {
                btnText.innerText = `${listService.length} Selected`;
            } else {
                btnText.innerText = "Tất cả hình thức thanh toán";
            }


        });
    })
    itemsClientStatus.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");

            let checked = document.querySelectorAll("#list-item-ClientStatus .checked"),
                btnText = document.querySelector(".btn-text-ClientStatus");
            let listService = []
             listClientStatusV2= []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (listClientStatusV2 && id.includes('ClientStatus_data_')) {
                    listService.push(checked[i]);
                }
                listClientStatusV2.push(parseInt(id.replace('ClientStatus_data_', '')))

            }
            if (listService.length > 0) {
                btnText.innerText = `${listService.length} Selected`;
            } else {
                btnText.innerText = "Tất cả Trạng thái";
            }


        });
    })
    itemsPermisionType.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");

            let checked = document.querySelectorAll("#list-item-PermisionType .checked"),
                btnText = document.querySelector(".btn-text-PermisionType");
            var listService = []
            listPermisionTypeV2 = []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (listPermisionTypeV2 && id.includes('PermisionType_data_')) {
                    listService.push(checked[i]);
                }
                listPermisionTypeV2.push(parseInt(id.replace('PermisionType_data_', '')))

            }
            if (listService.length > 0) {
                btnText.innerText = `${listService.length} Selected`;
            } else {
                btnText.innerText = "Tất cả nhóm khách hàng";
            }


        });
    })
    itemsUtmSource.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");

            let checked = document.querySelectorAll("#list-item-UtmSource .checked"),
                btnText = document.querySelector(".btn-text-UtmSource");
            let listService = []
            listUtmSourceV2 = []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (listUtmSourceV2 && id.includes('UtmSource_data_')) {
                    listService.push(checked[i]);
                }
                listUtmSourceV2.push(parseInt(id.replace('UtmSource_data_', '')))

            }
            if (listService.length > 0) {
                btnText.innerText = `${listService.length} Selected`;
            } else {
                btnText.innerText = "Tất cả Nguồn khách hàng";
            }


        });
    })
});

var _customer_managerV2 = {

    LoaddataClient: function () {
        var objSearch = this.SearchParam;
        var x = _customer_managerV2.getCookie("cookiesearchModelV2");
        if (x != null) {
            
            input = JSON.parse(x)
            _searchModel.MaKH = input.MaKH
            _searchModel.CreatedBy = input.CreatedBy
            _searchModel.TenKH = input.TenKH
            _searchModel.Email = input.Email
            _searchModel.Phone = input.Phone
            _searchModel.AgencyType = input.AgencyType
            _searchModel.ClientType = input.ClientType
            _searchModel.PermissionType = input.PermissionType
            _searchModel.UtmSource = input.UtmSource
            _searchModel.PageIndex = 1
            _searchModel.PageSize = 10
            _searchModel.MinAmount = input.MinAmount
            _searchModel.MaxAmount = input.MaxAmount
            _searchModel.CreateDate = input.CreateDate
            _searchModel.EndDate = input.EndDate
            _searchModel.ClientStatus = input.ClientStatus
            objSearch = _searchModel
            if (input.UtmSource.length > 0) {
                var btnTextUtmSource = document.querySelector(".btn-text-UtmSource");
                btnTextUtmSource.innerText = `${input.UtmSource.length} Selected`;
                for (var i = 0; i < input.UtmSource.length; i++) {
                    $('#UtmSource_data_' + input.UtmSource[i] + '').addClass('checked')
                }
            } 
            if (input.PermissionType.length > 0) {
                var btnTextPermissionType = document.querySelector(".btn-text-PermisionType");
                btnTextPermissionType.innerText = `${input.PermissionType.length} Selected`;
                for (var i = 0; i < input.PermissionType.length; i++) {
                    $('#PermisionType_data_' + input.PermissionType[i] + '').addClass('checked')
                }
            } 
            if (input.ClientStatus.length > 0) {
                var btnTextClientStatus = document.querySelector(".btn-text-ClientStatus");
                btnTextClientStatus.innerText = `${input.ClientStatus.length} Selected`;
                for (var i = 0; i < input.ClientStatus.length; i++) {
                    $('#ClientStatus_data_' + input.ClientStatus[i] + '').addClass('checked')
                }
            } 
            if (input.ClientType.length > 0) {
                var btnTextClientType = document.querySelector(".btn-text-ClientType");
                btnTextClientType.innerText = `${input.ClientType.length} Selected`;
                for (var i = 0; i < input.ClientType.length; i++) {
                    $('#ClientType_data_' + input.ClientType[i] + '').addClass('checked')
                }
            } 
        }
        else {
        let _searchModel = {
            MaKH: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            AgencyType: null,
            ClientType: null,
            PermissionType: null,
            UtmSource: null,
            PageIndex: 1,
            PageSize: 10,
            MinAmount: null,
            MaxAmount: null,
            ClientStatus: null,
            };
            objSearch = _searchModel;
        }
      
        this.SearchClient(objSearch);
        this.setValueFilter(objSearch);

    },
    Loaddata: function () {
        let _searchModel = {
            id: $('#id_userid').val()
        };
        var objSearch = this.SearchParam;

        objSearch = _searchModel;
        this.SearchDetailCustomerManager(objSearch);

    },
    SearchClient: function (input) {
        if(localStorage.getItem("cookiesearchModelV2") != null){
            var cookieModel = localStorage.getItem("cookiesearchModelV2");
            input = JSON.parse(cookieModel);

            if (input.AgencyType != null) {
                $('#AgencyType').val(input.AgencyType).attr("selected", "selected");
                var text = $("#AgencyType").find(':selected').text();
                console.log(text)
                $('#select2-AgencyType-container').html($("#AgencyType").find(':selected').text());
            }

            if (input.ClientType != null) {
                $('#ClientType').val(input.ClientType).attr("selected", "selected");
                $('#select2-ClientType-container').html($("#ClientType").find(':selected').text());
            }
            if (input.ClientStatus != null) {
                $('#ClientStatus').val(input.ClientStatus).attr("selected", "selected");
                $('#select2-ClientStatus-container').html($("#ClientStatus").find(':selected').text());
            }
            if (input.CreateDate != null)
                $('#createdate').html(input.CreateDate + " - " + input.EndDate);
            if (input.MinAmount != null)
                $('#minamount').html(input.MinAmount);
            if (input.MaxAmount != null)
                $('#maxamount').html(input.MaxAmount);
            if (input.PermissionType != null) {
                $('#PermisionType').val(input.PermissionType).attr("selected", "selected");
                $('#select2-PermisionType-container').html($("#PermisionType").find(':selected').text());
            }
            if (window.localStorage.getItem("textClient") != null) {
                var cookie1 = window.localStorage.getItem("textClient")
                var SaleName = JSON.parse(cookie1)
                $('#client').html('<option selected value = ' + input.MaKH + '> ' + SaleName + '</option>')
            }
            if (window.localStorage.getItem("textNT") != null) {
                var cookie1 = window.localStorage.getItem("textNT")
                var SaleName = JSON.parse(cookie1)

                var arrText = SaleName.split(',').map(x => x.trim());
                // tách theo dấu ,
                var arrIdCreatedBy = input.CreatedBy.split(',').map(x => x.trim());
                var html_CreatedBy = ``
                for (let i = 0; i < arrIdCreatedBy.length; i++) {

                    html_CreatedBy += ('<option selected value = ' + arrIdCreatedBy[i] + '> ' + arrText[i] + '</option>')
                }
                $('#CreatedBy').html(html_CreatedBy)
               
            }
            if (window.localStorage.getItem("textNV") != null) {
                var cookie1 = window.localStorage.getItem("textNV")
                var SaleName = JSON.parse(cookie1)
                // tách theo dấu ,
                var arrText = SaleName.split(',').map(x => x.trim());
                // tách theo dấu ,
                var arrId = input.UserId.split(',').map(x => x.trim());
                var html_txtNguoiTao=``
                for (let i = 0; i < arrId.length; i++) {

                    html_txtNguoiTao+=('<option selected value = ' + arrId[i] + '> ' + arrText[i] + '</option>')
                }
                $('#txtNguoiTao').html(html_txtNguoiTao)
            }
        }
        $.ajax({
            url: "/CustomerManagerManual/ListClient",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_client').hide();
                $('#grid_data_client').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    } else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");
         
            }
        });
       
    },
    SearchPaymentAccount: function (input) {
        $.ajax({
            url: "/CustomerManager/ListPaymentAccount",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_deposit_history').hide();
                $('#grid_data_payment_account').html(result);
            }
        });
    },

    Search: function (input) {
        _global_function.AddLoading()
        $.ajax({
            url: "/CustomerManager/ListDepositHistory",
            type: "Post",
            data: input,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#imgLoading_deposit_history').hide();
                $('#grid_data_deposit_history').html(result);
            }
        });
    },
    SearchOrder: function (input) {
        $.ajax({
            url: "/CustomerManager/ListOrder",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_order').hide();
                $('#grid_data_order').html(result);
            }
        });
    },
    SearchDetailCustomerManager: function (input) {
        $.ajax({
            url: "/CustomerManager/DetailCustomerManager",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_customer_managerV2').hide();
                $('#grid_detail_customer_managerV2').html(result);
            }
        });
    },
    DepositHistoryOnPaging: function (value) {
        let _searchModel = {
            id: $('#id_userid').val(),
            currentPage: value,
        };
        var objSearch = this.SearchParam
        objSearch = _searchModel
        this.Search(objSearch);
    },

    OrederOnPaging: function (value) {
        let _searchModel = {
            id: $('#id_userid').val(),
            currentPage: value,
        };
        var objSearch = this.SearchParam
        objSearch = _searchModel
        this.SearchOrder(objSearch);
    },
    ClientOnPaging: function (value) {
        var CreateDate;
        var EndDate;
        var MaKH_data= $('#client').select2("val");
        var UserId_data = $('#txtNguoiTao').select2("val") != null ? $('#txtNguoiTao').select2("val").toString() : null;
        var CreatedBy_data = $('#CreatedBy').select2("val") != null ? $('#CreatedBy').select2("val").toString() : null;
        var CacheName_data = $('#filter-client').select2("val");
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            CreateDate = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EndDate = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            CreateDate = null
            EndDate = null
        }
        let _searchModel = {
            MaKH: null,
            CreatedBy: null,
            UserId: null ,
            TenKH: null,
            Email: null,
            Phone: null,
            CacheName: null,
            DepartmentId: $('#DepartmentId').val(),
            AgencyType: $('#AgencyType').val(),
            ClientType: listClientTypeV2,
            ClientStatus: listClientStatusV2,
            PermissionType: listPermisionTypeV2,
            UtmSource: listUtmSourceV2,
            CreateDate: CreateDate,
            EndDate: EndDate,
            MinAmount: $('#minamount').val().replaceAll(',', ''),
            MaxAmount: $('#maxamount').val().replaceAll(',', ''),
            PageIndex: value,
            PageSize: parseFloat($("#selectPaggingOptions").find(':selected').val()),
        };
        if (MaKH_data != null) { _searchModel.MaKH = MaKH_data[0] }
        if (UserId_data != null) { _searchModel.UserId = UserId_data }
        if (CreatedBy_data != null) { _searchModel.CreatedBy = CreatedBy_data}
        if (CacheName_data != null && CacheName_data[0] != null) {
            _searchModel.CacheName = CacheName_data[0]

        }
        _searchModel.currentPage = value;
        var objSearch = this.SearchParam
        objSearch = _searchModel
        this.SearchClient(objSearch);
    },
    ShowHideColumn: function () {
        $('.checkbox-tb-column').each(function () {
            let seft = $(this);
            let id = seft.data('id');
            if (seft.is(':checked')) {
                $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
            } else {
                $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
            }
        });
    },
    checkShowHide: function () {
        if (fieldsV2.STT === true) {
            $('.grid-slide #STT').prop('checked', true);
        } else {
            $('.grid-slide #STT').prop('checked', false);
        }

        if (fieldsV2.MaKH === true) {
            $('.grid-slide #MaKH').prop('checked', true);
        } else {
            $('.grid-slide #MaKH').prop('checked', false);
        }
        if (fieldsV2.TenKH === true) {
            $('.grid-slide #TenKH').prop('checked', true);
        } else {
            $('.grid-slide #TenKH').prop('checked', false);
        }
        if (fieldsV2.LienHe === true) {
            $('.grid-slide #LienHe').prop('checked', true);
        } else {
            $('.grid-slide #LienHe').prop('checked', false);
        }
        if (fieldsV2.DoiTuong === true) {
            $('.grid-slide #DoiTuong').prop('checked', true);
        } else {
            $('.grid-slide #DoiTuong').prop('checked', false);
        }
        if (fieldsV2.LoaiKH === true) {
            $('.grid-slide #LoaiKH').prop('checked', true);
        } else {
            $('.grid-slide #LoaiKH').prop('checked', false);
        }
        if (fieldsV2.NhomKH === true) {
            $('.grid-slide #NhomKH').prop('checked', true);
        } else {
            $('.grid-slide #NhomKH').prop('checked', false);
        }
        if (fieldsV2.TongTT === true) {
            $('.grid-slide #TongTT').prop('checked', true);
        } else {
            $('.grid-slide #TongTT').prop('checked', false);
        }
        if (fieldsV2.VNPhuTrach === true) {
            $('.grid-slide #VNPhuTrach').prop('checked', true);
        } else {
            $('.grid-slide #VNPhuTrach').prop('checked', false);
        }
        if (fieldsV2.NgayTao === true) {
            $('.grid-slide #NgayTao').prop('checked', true);
        } else {
            $('.grid-slide #NgayTao').prop('checked', false);
        }
        if (fieldsV2.NgayCN === true) {
            $('.grid-slide #NgayCN').prop('checked', true);
        } else {
            $('.grid-slide #NgayCN').prop('checked', false);
        }
        if (fieldsV2.NgayDuyet === true) {
            $('.grid-slide #NgayDuyet').prop('checked', true);
        } else {
            $('.grid-slide #NgayDuyet').prop('checked', false);
        }
        if (fieldsV2.NguoiTao === true) {
            $('.grid-slide #NguoiTao').prop('checked', true);
        } else {
            $('.grid-slide #NguoiTao').prop('checked', false);
        }
        if (fieldsV2.Status === true) {
            $('.grid-slide #Status').prop('checked', true);
        } else {
            $('.grid-slide #Status').prop('checked', false);
        }

    },
    ChangeSetting: function (position) {
        this.ShowHideColumn();
        switch (position) {
            case 1:
                if ($('.grid-slide #STT').is(":checked")) {
                    fieldsV2.STT = true
                } else {
                    fieldsV2.STT = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 2:
                if ($('.grid-slide #MaKH').is(":checked")) {
                    fieldsV2.MaKH = true
                } else {
                    fieldsV2.MaKH = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 3:
                if ($('.grid-slide #TenKH').is(":checked")) {
                    fieldsV2.TenKH = true
                } else {
                    fieldsV2.TenKH = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 4:
                if ($('.grid-slide #LienHe').is(":checked")) {
                    fieldsV2.LienHe = true
                } else {
                    fieldsV2.LienHe = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 5:
                if ($('.grid-slide #DoiTuong').is(":checked")) {
                    fieldsV2.DoiTuong = true
                } else {
                    fieldsV2.DoiTuong = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 6:
                if ($('.grid-slide #LoaiKH').is(":checked")) {
                    fieldsV2.LoaiKH = true
                } else {
                    fieldsV2.LoaiKH = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 7:
                if ($('.grid-slide #NhomKH').is(":checked")) {
                    fieldsV2.NhomKH = true
                } else {
                    fieldsV2.NhomKH = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;

            case 8:
                if ($('.grid-slide #TongTT').is(":checked")) {
                    fieldsV2.TongTT = true
                } else {
                    fieldsV2.TongTT = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 9:
                if ($('.grid-slide #VNPhuTrach').is(":checked")) {
                    fieldsV2.VNPhuTrach = true
                } else {
                    fieldsV2.VNPhuTrach = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;

            case 10:
                if ($('.grid-slide #NgayTao').is(":checked")) {
                    fieldsV2.NgayTao = true
                } else {
                    fieldsV2.NgayTao = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 11:
                if ($('.grid-slide #NgayCN').is(":checked")) {
                    fieldsV2.NgayCN = true
                } else {
                    fieldsV2.NgayCN = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;

            case 12:
                if ($('.grid-slide #NguoiTao').is(":checked")) {
                    fieldsV2.NguoiTao = true
                } else {
                    fieldsV2.NguoiTao = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 13:
                if ($('.grid-slide #Status').is(":checked")) {
                    fieldsV2.Status = true
                } else {
                    fieldsV2.Status = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 17:
                if ($('.grid-slide #LastOrderDate').is(":checked")) {
                    fieldsV2.LastOrderDate = true
                } else {
                    fieldsV2.LastOrderDate = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            case 18:
                if ($('.grid-slide #LastOrderDateToDay').is(":checked")) {
                    fieldsV2.LastOrderDateToDay = true
                } else {
                    fieldsV2.LastOrderDateToDay = false
                }
                this.eraseCookie(cookieNameV2);
                this.setCookie(cookieNameV2, JSON.stringify(fieldsV2), 10);
                break;
            default:
        }
    },
    OpenPopup: function (id) {
        let title = 'Thêm mới/Cập nhật khách hàng';
        let url = '/CustomerManagerManual/CustomerManagerDetail';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);
   
    },
    Showlabel: function (input) {
        var label = document.getElementById('sothue_cmnd');
        if (input == 0) {
            label.innerHTML = "Mã số thuế"
            var x = document.getElementById("id_loaikhach").options.length;
            $("#id_loaikhach option").remove();
            $.ajax({
                url: "/CustomerManager/ListClientType",
                type: "post",
                success: function (result) {
                    if (result != undefined && result.data != undefined && result.data.length > 0) {
                        
                        if (x < result.data.length) {
                            $('#id_loaikhach').append('<option value="">Tất cả loại khách hàng</option>')
                            result.data.forEach(function (item) {
                            if (item.codeValue != 6) {
                                $('#id_loaikhach').append(_customer_managerV2_html.html_Client_option.replaceAll('{Client_id}', item.codeValue).replace('{Client_name}', item.description))
                            }

                            });
                        }
                    }
                    else {
                        $("#id_loaikhach").trigger('change');
                       
                    }

                }
            });
        }
        if (input == 1) {
            label.innerHTML = "Số chứng minh nhân dân";
            var x = document.getElementById("id_loaikhach").options.length;
            $("#id_loaikhach option").remove();
            $.ajax({
                url: "/CustomerManager/ListClientType",
                type: "post",
                success: function (result) {
                    if (result != undefined && result.data != undefined && result.data.length > 0) {
                       
                        if (x < result.data.length) {
                            $('#id_loaikhach').append('<option value="">Tất cả loại khách hàng</option>')
                        result.data.forEach(function (item) {
                            if (item.codeValue != 6) {
                                $('#id_loaikhach').append(_customer_managerV2_html.html_Client_option.replaceAll('{Client_id}', item.codeValue).replace('{Client_name}', item.description))
                            } else {
                              /*  $('#id_loaikhach').append(_customer_managerV2_html.html_Client_option_B2C.replaceAll('{Client_id}', item.codeValue).replace('{Client_name}', item.description))*/
                            }

                        });
                        $("#id_loaikhach").trigger('change');

                        }
                    }
                    else {
                        $("#id_loaikhach").trigger('change');

                    }

                }
            });
        }

    },
    SearchData: function () {
        var CreateDate;
        var EndDate;
        var CacheName_data = $('#filter-client').select2("val");
        var MaKH_data = $('#client').select2("val");
        textClient = $('#client').find(':selected').text();
        var UserId_data = $('#txtNguoiTao').select2("val") != null ? $('#txtNguoiTao').select2("val").toString() : null;
        textNV = $('#txtNguoiTao').select2('data').map(item => item.text).join(',');;
        var CreatedBy_data = $('#CreatedBy').select2("val") != null ? $('#CreatedBy').select2("val").toString() : null;
        textNT = $('#CreatedBy').find(':selected').text();
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            CreateDate = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EndDate = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            CreateDate = null
            EndDate = null
        }
        let _searchModel = {
            MaKH:  null ,
            UserId: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            CacheName: null,
            DepartmentId: $('#DepartmentId').val(),
            AgencyType: $('#AgencyType').val(),
            ClientType: listClientTypeV2,
            ClientStatus: listClientStatusV2,
            PermissionType: listPermisionTypeV2,
            UtmSource: listUtmSourceV2,
            CreateDate: CreateDate,
            EndDate: EndDate,
            MinAmount: $('#minamount').val().replaceAll(',', ''),
            MaxAmount: $('#maxamount').val().replaceAll(',', ''),
            PageIndex: 1,
            PageSize: parseFloat($("#selectPaggingOptions").find(':selected').val()),
        };
        if (MaKH_data != null && MaKH_data[0] != null) {
            _searchModel.MaKH = MaKH_data[0]
            window.localStorage.setItem("textClient", JSON.stringify(textClient));
        }
        else {
            window.localStorage.removeItem("textClient")
        }
        if (UserId_data != null && UserId_data != null) {
            _searchModel.UserId = UserId_data
            window.localStorage.setItem("textNV", JSON.stringify(textNV));
        }
        else {
            window.localStorage.removeItem("textNV")
        }
        if (CreatedBy_data != null && CreatedBy_data!= null) {
            _searchModel.CreatedBy = CreatedBy_data
            window.localStorage.setItem("textNT", JSON.stringify(textNT));
        }
        else {
            window.localStorage.removeItem("textNT")
        }
        if (CacheName_data != null && CacheName_data[0] != null) {
            _searchModel.CacheName = CacheName_data[0]

        }
        var objSearch = this.SearchParam;
        objSearch = _searchModel;
        _customer_managerV2.setCookie("cookiesearchModelV2", JSON.stringify(_searchModel),1)
        window.localStorage.setItem("cookiesearchModelV2", JSON.stringify(_searchModel));
        $('.form-down-filter').attr("style", "width: 600px; display: none;")
        $('.onclick-active').addClass("onclick")
        $('.onclick').removeClass("onclick-active")
        $(".onclick-active").addClass('onclick');
        $(".onclick-active").removeClass('onclick-active');
        $(".form-down").slideUp();
        $(".onclick-togle, .dropdown .dropbtn,.down-up .onclick").removeClass('active');
        $(".dropdown.active").find('.dropdown-content').slideUp();
        $(".select--v2__content").slideUp();
        this.SearchClient(objSearch);
        
    },

    OnCreateClient: function () {
        var Id = $('#CustomerManager_Id').val();
        var JoinDate = $('#JoinDate').val();
        var id_ClientType = $('#id_ClientType').val();
        var optradio = $('input[name="optradio"]:checked').val();
        var Client_name = $('#Client_name').val();
        var Maso_Id = $('#Maso_Id').val();
        var phone = $('#phone').val();
        var email = $('#email').val();
        var DiaChi_giaodich = $('#DiaChi_giaodich').val();
        var DC_hoadon = $('#DC_hoadon').val();
        var id_loaikhach = $('#id_loaikhach').val();
        var id_nhomkhach = $('#id_nhomkhach').val();
        var pass = $('#pass').val();
        var pass_backup = $('#pass_backup').val();
        var So_tk = $('#So_tk').val();
        var Name_tk = $('#Name_tk').val();
        var Name_nh = $('#Name_nh').val();
        var diachi_chinhanh = $('#diachi_chinhanh').val();
        var Note = $('#Note').val();
        var user_Id = $('#id_userid').val();
        var UtmSource = $('#UtmSource').val();
        let FromCreate = $('#CustomerManager_Detail');
        FromCreate.validate({
            rules: {

                "Client_name": "required",
              
                "phone": {
                    required: true,
                    number: true,
                },
                //"email": {
                //    required: true,
                //    email: true,
                //},
                
                "id_loaikhach": "required",
                "id_nhomkhach": "required",
              
                "So_tk": { number: true,},

            },
            messages: {
                "Client_name": "Tên khách hàng không được bỏ trống",
               
                "phone": {
                    required: "Số điện thoại không được bỏ trống",
                    number: "Nhập đúng định dạng số",
                },
                //"email": {
                //    required: "Email không được bỏ trống",
                //    email: "Nhập đúng định dạnh email",
                //},
               
                "id_loaikhach": "Vui lòng chọn loại khách hàng",
                "id_nhomkhach": "Vui lòng chọn nhóm khách hàng",
               
                "So_tk": {
                    number: "Nhập đúng định dạng số",
                },
            }
        });
        if (FromCreate.valid()) {
            if (So_tk == "" && Name_tk == "" && Name_nh == "" && diachi_chinhanh == "" || So_tk != "" && Name_tk != "" && Name_nh != "" && diachi_chinhanh != "")
            {
                var object_summit = {
                    Id: Id,
                    JoinDate: JoinDate,
                    Client_name: Client_name,
                    id_ClientType: id_loaikhach,
                    AgencyType: optradio,
                    phone: phone,
                    email: email,
                    diachi_chinhanh: diachi_chinhanh,
                    DC_hoadon: DC_hoadon,
                    DiaChi_giaodich: DiaChi_giaodich,
                    id_loaikhach: optradio,
                    id_nhomkhach: id_nhomkhach,
                    pass: pass,
                    pass_backup: pass_backup,
                    So_tk: So_tk,
                    Name_tk: Name_tk,
                    Name_nh: Name_nh,
                    Note: Note,
                    Maso_Id: Maso_Id,
                    UtmSource: UtmSource,
                }
                let data = JSON.stringify(object_summit)
                _global_function.AddLoading()
                $.ajax({
                    url: '/CustomerManagerManual/Setup',
                    type: "post",
                    data: { data },
                    success: function (result) {

                        if (result.stt_code === 1) {
                            _global_function.RemoveLoading()
                            _msgalert.error(result.msg);
                        }
                        if (result.stt_code === 3) {
                            _global_function.RemoveLoading()
                            _msgalert.error(result.msg);
                        }

                        if (result.stt_code === 0) {
                            _global_function.RemoveLoading()
                            _msgalert.success(result.msg);
                            setTimeout(function () {
                                $.magnificPopup.close();
                                location.reload();
                            }, 300);

                        }
                    }
                });
            }
            else {

                if (So_tk == "") { $('#So_tk_err').show().text("Số tài khoản không được bỏ trống") }
                else { $('#So_tk-error').hide() }
                if (Name_tk == "") { $('#Name_tk_err').show().text("Tên chủ tài khoản không được bỏ trống") }
                else { $('#Name_tk-error').hide() }
                if (Name_nh == "") { $('#Name_nh_err').show().text("Tên ngân hàng được bỏ trống") }
                else { $('#Name_nh-error').hide() }
                if (diachi_chinhanh == "") { $('#diachi_chinhanh_err').show().text("Tên chi nhánh không được bỏ trống") }
                else { $('#diachi_chinhanh-error').hide() }
            }
            
          
        }
       

        
    },
    OnChangeUserId: function (value, type) {
        if (type == 1) {
            $('#OrderNo').val(value.name)
        }
        else {
            $('#OrderNo').val('')

        }
    },
    OnChangeClientType: function () {
        var id_loaikhach = $('#id_loaikhach').val();
        if (id_loaikhach == 5) {
            $('#id_nhomkhach').attr('disabled', 'disabled');
            $('#id_nhomkhach').html(`<option value="0" selected="selected">Không được công nợ</option>`);
       
            
        } else {
            $('#id_nhomkhach').html(`
                                <option value="">Tất cả nhóm khách hàng</option>
                                        <option value="0">Không được công nợ</option>
                                        <option value="1">Được công nợ</option>
                                        <option value="2">Phải ký quỹ</option>
                                        <option value="3">Không phải ký quỹ</option>
                            `);
            $('#id_nhomkhach').removeAttr("disabled");
        }
    },

    setCookie: function (name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    },
    getCookie: function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },
    eraseCookie: function (name) {
        document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    },
    saveCookieFilter: function () {
        this.setCookie(cookieFilterName, JSON.stringify(this.getModel()), 1)

    },
 
    SetActive: function (status) {
        $('.data_order').removeClass('active')
        
        $('#data_order_'+status).addClass('active')
        
    },
    OnStatuse: function (value) {
        if (value != 99) {
            let _searchModel = {
                ClientType: value,
            };
            var objSearch = this.SearchParam;

            objSearch = _searchModel;


            this.SearchClient(objSearch);
        }
        else {
            this.SearchClient(objSearch);
        }

    },
 
    onSelectPageSize: function (value) {
          
        var CreateDate;
        var EndDate;
        var MaKH_data = $('#client').select2("val");
        var UserId_data = $('#txtNguoiTao').select2("val") != null ? $('#txtNguoiTao').select2("val").toString() : null
        var CreatedBy_data = $('#CreatedBy').select2("val") != null ? $('#CreatedBy').select2("val").toString() : null;
        var CacheName_data = $('#filter-client').select2("val");
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            CreateDate = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EndDate = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            CreateDate = null
            EndDate = null
        }
        let _searchModel = {
            MaKH: null,
            UserId: null,
            TenKH: null,
            Email: null,
            Phone: null,
            CacheName: null,
            DepartmentId: $('#DepartmentId').val(),
            AgencyType: $('#AgencyType').val(),
            ClientType: listClientTypeV2,
            ClientStatus: listClientStatusV2,
            PermissionType: listPermisionTypeV2,
            UtmSource: listUtmSourceV2,
            CreateDate: CreateDate,
            EndDate: EndDate,
            MinAmount: $('#minamount').val().replaceAll(',', ''),
            MaxAmount: $('#maxamount').val().replaceAll(',', ''),
            PageIndex: 1,
            currentPage: 1,
            PageSize: value,
        };
        if (MaKH_data != null) { _searchModel.MaKH = MaKH_data[0] }
        if (UserId_data != null) { _searchModel.UserId = UserId_data }
        if (CreatedBy_data != null) { _searchModel.CreatedBy = CreatedBy_data}
        if (CacheName_data != null && CacheName_data[0] != null) {

            _searchModel.CacheName = CacheName_data[0]
        }
        _searchModel.PageSize = parseFloat($("#selectPaggingOptions").find(':selected').val()) 
       
        var objSearch = this.SearchParam;
        objSearch = _searchModel
        this.SearchClient(objSearch);
        this.setValueFilter(objSearch);
       
    },
    setValueFilter: function (objSearch) {

        $('#selectPaggingOptions').val(objSearch.PageSize).attr("selected", "selected");
    },
    ClearlocalStorage: function () {

        localStorage.clear();
    },
    OnResetStatus: function (id, status) {
        let title = 'Xác nhận tài khoản';
        let description = 'Bạn có chắc chắn muốn thay đổi trạng thái của tài khoản này?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/CustomerManager/ResetStatusAc",
                type: "post",
                data: { clientId: id, Status: status },
                success: function (result) {
                    if (result.status==0) {
                        _msgalert.success(result.msg);
                       
                    } else {
                        _msgalert.error(result.msg);
                    }
                }
            });
            if (status == 0) {
                $('#Status_0_' + id).removeClass('status-grey');
                $('#Status_0_' + id).addClass('status-green');
                $('#Status_1_' + id).removeClass('status-red');
                $('#Status_1_' + id).addClass('status-grey');
            }
            if (status == 1) {
                $('#Status_0_' + id).removeClass('status-green');
                $('#Status_0_' + id).addClass('status-grey');
                $('#Status_1_' + id).removeClass('status-grey');
                $('#Status_1_' + id).addClass('status-red');
            }
        });
    },
    Export: function () {
        var CreateDate;
        var EndDate;
        var MaKH_data = $('#client').select2("val");
        textClient = $('#client').find(':selected').text();
        var UserId_data = $('#txtNguoiTao').select2("val") != null ? $('#txtNguoiTao').select2("val").toString() : null;
        textNT = $('#txtNguoiTao').find(':selected').text();
        var CreatedBy_data = $('#CreatedBy').select2("val") != null ? $('#CreatedBy').select2("val").toString() : null;
        textNV = $('#CreatedBy').find(':selected').text();
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            CreateDate = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EndDate = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            CreateDate = null
            EndDate = null
        }
        var CacheName_data = $('#filter-client').select2("val");
        let _searchModel = {
            MaKH: null,
            UserId: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            AgencyType: $('#AgencyType').val(),
            ClientType: listClientTypeV2,
            ClientStatus: listClientStatusV2,
            PermissionType: listPermisionTypeV2,
            UtmSource: listUtmSourceV2,
            CreateDate: CreateDate,
            EndDate: EndDate,
            MinAmount: $('#minamount').val().replaceAll(',', ''),
            MaxAmount: $('#maxamount').val().replaceAll(',', ''),
            PageIndex: 1,
            PageSize: $("#countClient").val(),
            CacheName: null,
            DepartmentId: $('#DepartmentId').val(),
        };
        if (MaKH_data != null) { _searchModel.MaKH = MaKH_data[0] }
        if (UserId_data != null) { _searchModel.UserId = UserId_data }
        if (CreatedBy_data != null) { _searchModel.CreatedBy = CreatedBy_data}
        if (CacheName_data != null && CacheName_data[0] != null) {

            _searchModel.CacheName = CacheName_data[0]
        }
        var objSearch = this.SearchParam;
        objSearch = _searchModel;
        $('#btnExport').prop('disabled', true);
        $('#icon-export').removeClass('fa-file-excel-o');
        $('#icon-export').addClass('fa-spinner fa-pulse');
       
        objSearch.PageIndex = 1;
        this.searchModel = objSearch
        _global_function.AddLoading()
        $.ajax({
            url: "/CustomerManager/ExportExcel",
            type: "Post",
            data: { searchModel: this.searchModel, field: fieldsV2 },
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
    },

    SearchData2: function () {
        $('.form-down-filter').attr("style","width: 600px; display: none;")
        $('.onclick-active').addClass("onclick")
        $('.onclick').removeClass("onclick-active")
        var CreateDate;
        var EndDate;
        var CacheName_data = $('#filter-client').select2("val");
        var MaKH_data = $('#client').select2("val");
        textClient = $('#client').find(':selected').text();
        var UserId_data = $('#txtNguoiTao').select2("val") != null ? $('#txtNguoiTao').select2("val").toString() : null;
        textNV = $('#txtNguoiTao').find(':selected').text();
        var CreatedBy_data = $('#CreatedBy').select2("val") != null ? $('#CreatedBy').select2("val").toString() : null;
        textNT = $('#CreatedBy').find(':selected').text();
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            CreateDate = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EndDate = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            CreateDate = null
            EndDate = null
        }
        let _searchModel = {
            MaKH: null,
            UserId: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            CacheName: null,
            DepartmentId: $('#DepartmentId').val(),
            AgencyType: $('#AgencyType').val(),
            ClientType: listClientTypeV2,
            ClientStatus: listClientStatusV2,
            PermissionType: listPermisionTypeV2,
            UtmSource: listUtmSourceV2,
            CreateDate: CreateDate,
            EndDate: EndDate,
            MinAmount: $('#minamount').val().replaceAll(',', ''),
            MaxAmount: $('#maxamount').val().replaceAll(',', ''),
            PageIndex: 1,
            PageSize: parseFloat($("#selectPaggingOptions").find(':selected').val()),
        };
        if (MaKH_data != null && MaKH_data[0] != null) {
            _searchModel.MaKH = MaKH_data[0]

        }
        else {
            window.localStorage.removeItem("textClient")
        }
        if (UserId_data != null && UserId_data != null) {
            _searchModel.UserId = UserId_data

        }
        else {
            window.localStorage.removeItem("textNV")
        }
        if (CreatedBy_data != null && CreatedBy_data!= null) {
            _searchModel.CreatedBy = CreatedBy_data

        }
        else {
            window.localStorage.removeItem("textNT")
        }
        if (CacheName_data != null && CacheName_data[0] != null) {

            _searchModel.CacheName = CacheName_data[0]
        }
        var objSearch = this.SearchParam;
        objSearch = _searchModel;
        _customer_managerV2.setCookie("cookiesearchModelV2", JSON.stringify(_searchModel), 1)
        localStorage.setItem("cookiesearchModelV2", JSON.stringify(_searchModel));
        $(".onclick-active").addClass('onclick');
        $(".onclick-active").removeClass('onclick-active');
        $(".form-down").slideUp();
        $(".onclick-togle, .dropdown .dropbtn,.down-up .onclick").removeClass('active');
        $(".dropdown.active").find('.dropdown-content').slideUp();
        $(".select--v2__content").slideUp();
        this.SearchClient(objSearch);

    },
    SeverFilter: function () {
        var CreateDate;
        var EndDate;
        var Cache_Name = $('#Cache_name').val();
        if (Cache_Name == null || Cache_Name == "") {
            $('#Cache_name-error').show();
            return false;
        } else {
            $('#Cache_name-error').hide();
        }
        var MaKH_data = $('#client').select2("val");
        textClient = $('#client').find(':selected').text();
        var UserId_data = $('#txtNguoiTao').select2("val") != null ? $('#txtNguoiTao').select2("val").toString() : null;
        textNV = $('#txtNguoiTao').find(':selected').text();
        var CreatedBy_data = $('#CreatedBy').select2("val") != null ? $('#CreatedBy').select2("val").toString() : null;
        textNT = $('#CreatedBy').find(':selected').text();
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            CreateDate = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EndDate = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            CreateDate = null
            EndDate = null
        }
        let _searchModel = {
            MaKH: MaKH_data,
            UserId: UserId_data,
            CreatedBy: CreatedBy_data,
            TenKH: textClient,
            Email: null,
            Phone: null,
            CacheName: Cache_Name,
            DepartmentId: $('#DepartmentId').val(),
            AgencyType: $('#AgencyType').val(),
            ClientType: listClientTypeV2,
            ClientStatus: listClientStatusV2,
            PermissionType: listPermisionTypeV2,
            UtmSource: listUtmSourceV2,
            CreateDate: CreateDate,
            EndDate: EndDate,
            MinAmount: $('#minamount').val().replaceAll(',', ''),
            MaxAmount: $('#maxamount').val().replaceAll(',', ''),
            PageIndex: 1,
            PageSize: parseFloat($("#selectPaggingOptions").find(':selected').val()),

        };

        var objSearch = this.SearchParam;
        objSearch = _searchModel;

        $.ajax({
            url: "/CustomerManagerManual/InsertLogCache",
            type: "Post",
            data: { searchModel: objSearch },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.msg)
                }
            }
        });
    },
    Clearboloc: function (id) {
        document.getElementById(id).reset();
 
        $('#minamount').val('');
        $('#maxamount').val('');
        $('#DepartmentId').val(0);

        $("#list-item-ClientType .item").removeClass("checked");
        $("#list-item-ClientStatus .item").removeClass("checked");
        $("#list-item-PermisionType .item").removeClass("checked");
        $("#list-item-UtmSource .item").removeClass("checked");


        listClientTypeV2 = [];
        listClientStatusV2 = [];
        listPermisionTypeV2 = [];
        listUtmSourceV2 =
        document.querySelector(".btn-text-ClientType").innerText = "Tất cả hình thức thanh toán";
        document.querySelector(".btn-text-ClientStatus").innerText = "Tất cả Trạng thái";
        document.querySelector(".btn-text-PermisionType").innerText = "Tất cả Trạng thái";
        document.querySelector(".btn-text-UtmSource").innerText = "Tất cả Nguồn khách hàng";
    },
    eraseCookie: function (name) {
        document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    },
    getCookie: function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },
    setCookie: function (name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    },
}

//// validate

function delay_callback(callback, ms) {
    var timer = 0;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
} 
