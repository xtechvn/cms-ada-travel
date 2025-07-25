﻿let isPickerApprove = false;
var _supplier_detail = {
    Init: function () {
        this.elModal = $('#global_modal_popup');
        this.supplier_id = $('#global_supplier_id').val();
        this.validImageTypes = ['image/gif', 'image/jpeg', 'image/png'];
        this.validImageSize = 5 * 1024 * 1024;
        this.noImageSrc = "/images/icons/noimage.png";
    },

    GetFormData: function ($form) {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};

        $.map(unindexed_array, function (n, i) {
            indexed_array[n['name']] = n['value'];
        });

        return indexed_array;
    },
   

}

var _supplier_contact = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val()
        }
        this.ReloadListing(this.search_params);
    },

    OnAddOrUpdate: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} liên hệ`;
        let url = '/Supplier/ContactUpsert';

        this.modal.find('.modal-title').html(title);
        this.modal.find('.modal-dialog').css('max-width', '600px');

        _ajax_caller.get(url, { id: id, supplier_id: this.search_params.supplier_id }, function (result) {
            _supplier_contact.modal.find('.modal-title').html(title);
            _supplier_contact.modal.find('.modal-body').html(result);
            _supplier_contact.modal.modal('show');
        });
    },

    Upsert: function () {
        let url = '/Supplier/ContactUpsert';
        let Form = $('#form_supplier_contact');
        Form.validate({
            rules: {
                Name: "required",
                Mobile: {
                    required: true,
                    minlength: 10,
                    maxlength: 11,
                    digits: true
                },
                Email: {
                    email: true
                },
            },
            messages: {
                Name: "Vui lòng nhập tên liên hệ",
                Mobile: {
                    required: "Vui lòng nhập số điện thoại",
                    exactlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
                    digits: "Số điện thoại phải là kí tự dạng số"
                },
                Email: {
                    email: 'Email không đúng định dạng'
                }
            }
        });

        if (!Form.valid()) return;

        let formData = _supplier_detail.GetFormData(Form);

        _ajax_caller.post(url, { model: formData }, function (result) {
            if (result.isSuccess) {
                _msgalert.success(result.message);
                _supplier_contact.modal.modal('hide');
                _supplier_contact.ReloadListing();
            } else {
                _msgalert.error(result.message);
            }
        });
    },

    Delete: function (id) {
        let url = '/Supplier/ContactDelete';
        let title = 'Xác nhận xóa liên hệ';
        let description = 'Bạn có chắc chắn muốn thông tin?';
        _msgconfirm.openDialog(title, description, function () {
            _ajax_caller.post(url, { id: id }, function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    _supplier_contact.ReloadListing();
                } else {
                    _msgalert.error(result.message);
                }
            });
        });
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/ContactListing", input, function (result) {
            $('#grid_supplier_contact').html(result);
        });
    },

    ReloadListing: function () {
        //this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    }
}

var _supplier_payment = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val()
        }
        this.ReloadListing(this.search_params);
    },

    OnAddOrUpdate: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} thông tin thanh toán`;
        let url = '/Supplier/PaymentUpsert';

        this.modal.find('.modal-title').html(title);
        this.modal.find('.modal-dialog').css('max-width', '600px');

        _ajax_caller.get(url, { id: id, supplier_id: this.search_params.supplier_id }, function (result) {
            _supplier_payment.modal.find('.modal-title').html(title);
            _supplier_payment.modal.find('.modal-body').html(result);
            _supplier_payment.modal.modal('show');
        });
    },

    Upsert: function () {
        let url = '/Supplier/PaymentUpsert';
        let Form = $('#form_supplier_payment');
        Form.validate({
            rules: {
                AccountName: "required",
                AccountNumber: {
                    required: true,
                    maxlength: 20,
                  /*  digits: true*/
                },
                BankId: "required"
            },
            messages: {
                AccountName: "Vui lòng nhập chủ tài khoản ngân hàng",
                AccountNumber: {
                    required: "Vui lòng nhập số tài khoản",
                    maxlength: "Số tài khoản chỉ chứa tối đa 20 kí tự",
                   /* digits: "Số tài khoản phải là kí tự dạng số"*/
                },
                BankId: "Vui lòng nhập tên ngân hàng"
            }
        });

        if (!Form.valid()) return;

        let formData = _supplier_detail.GetFormData(Form);

        _ajax_caller.post(url, { model: formData }, function (result) {
            if (result.isSuccess) {
                _msgalert.success(result.message);
                _supplier_payment.modal.modal('hide');
                _supplier_payment.ReloadListing();
            } else {
                _msgalert.error(result.message);
            }
        });
    },

    Delete: function (id) {
        let url = '/Supplier/PaymentDelete';
        let title = 'Xác nhận xóa liên hệ';
        let description = 'Bạn có chắc chắn muốn thông tin?';
        _msgconfirm.openDialog(title, description, function () {
            _ajax_caller.post(url, { id: id }, function (result) {
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    _supplier_payment.ReloadListing();
                } else {
                    _msgalert.error(result.message);
                }
            });
        });
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/PaymentListing", input, function (result) {
            $('#grid_supplier_payment').html(result);
        });
    },

    ReloadListing: function () {
        //this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    }
}

var _supplier_order = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val(),
            page_index: 1,
            page_size: 10
        }
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDate: null,
            EndDate: null,
        }
        this.ReloadListing(this.search_params);
        _supplier_order.FlyListing(model);
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/OrderListing", input, function (result) {
            $('#grid_supplier_order').html(result);
        });

    },

    ReloadListing: function () {
        //this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    },
    FlyListing: function (input) {
        $('#gray-data').html(' <div class="form-group flex-end mt20 block text-center"> <i class= "fa fa-refresh fa-spin" style = "color:#1E5846" ></i>  </div > ')
         _ajax_caller.post("/Supplier/FlyListing", input, function (result) {
            $('#gray-data').html(result);
        });
    },
    FlyPaging: function (Index) {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: Index,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } 
        _supplier_order.FlyListing(model);
    },
    HotelListing: function (input) {
        $('#gray-data').html('<div class="form-group flex-end mt20 block text-center"> <i class= "fa fa-refresh fa-spin" style = "color:#1E5846" ></i>  </div > ')
        _ajax_caller.post("/Supplier/HotelListing", input, function (result) {
            $('#gray-data').html(result);
        });
    },
    HotelPaging: function (Index) {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: Index,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } 
        _supplier_order.HotelListing(model);
    },
    OtherListing: function (input) {
        $('#gray-data').html('<div class="form-group flex-end mt20 block text-center"> <i class= "fa fa-refresh fa-spin" style = "color:#1E5846" ></i>  </div > ')
        _ajax_caller.post("/Supplier/OtherListing", input, function (result) {
            $('#gray-data').html(result);
        });
    },
    OtherPaging: function (Index) {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: Index,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } 
        _supplier_order.OtherListing(model);
    },
    TourListing: function (input) {
        $('#gray-data').html('<div class="form-group flex-end mt20 block text-center"> <i class= "fa fa-refresh fa-spin" style = "color:#1E5846" ></i>  </div > ')
        _ajax_caller.post("/Supplier/TourListing", input, function (result) {
            $('#gray-data').html(result);
        });
    },
    TourPaging: function (Index) {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: Index,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } 
        _supplier_order.TourListing(model);
    },
    VinWonderListing: function (input) {
        $('#gray-data').html('<div class="form-group flex-end mt20 block text-center"> <i class= "fa fa-refresh fa-spin" style = "color:#1E5846" ></i>  </div > ')
        _ajax_caller.post("/Supplier/VinWonderListing", input, function (result) {
            $('#gray-data').html(result);
        });
    },
    VinWonderPaging: function (Index) {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: Index,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        _supplier_order.VinWonderListing(model);
    },
    SearchData: function (Index) {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } 
        var type = $('#tab-order-supplier .active').attr('data-tab-id');
        if (type == undefined || type == null) type = 0;
        switch (parseFloat( type)) {
            case 0: {
                _supplier_order.FlyListing(model);
            } break;
            case 1: {
                _supplier_order.HotelListing(model);
            } break;
            case 2: {
                _supplier_order.TourListing(model);
            } break;
            case 3: {
                _supplier_order.OtherListing(model);

            } break;
            case 4: {
                _supplier_order.VinWonderListing(model);
            } break;
        }
    },
    Export: function () {
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        }
        var type = $('#tab-order-supplier .active').attr('data-tab-id');
        if (type == undefined || type == null) type = 0;
        type = parseFloat(type);
        $.ajax({
            url: "/Supplier/ExportExcelOrder",
            type: "Post",
            data: { searchModel: model, type: type }, 
            success: function (result) {
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
}

var _supplier_service_detail = {

    Init: function () {
        this.modal = $('#global_modal_popup');
        this._ChangeIntervalName = null;
        this.search_params = {
            supplier_id: $('#global_supplier_id').val(),
            service_name: null,
            service_type: 1,
            page_index: 1,
            page_size: 10
        }
        this.ReloadListing(this.search_params);
    },

    OnAddOrUpdate: function () {
        let title = `Cập nhật dịch vụ`;
        let url = '/Supplier/ServiceUpsert';

        this.modal.find('.modal-title').html(title);
        this.modal.find('.modal-dialog').css('max-width', '600px');

        _ajax_caller.get(url, { supplier_id: this.search_params.supplier_id }, function (result) {
            _supplier_contact.modal.find('.modal-title').html(title);
            _supplier_contact.modal.find('.modal-body').html(result);
            _supplier_contact.modal.modal('show');
        });
    },

    Upsert: function () {
        let url = '/Supplier/ServiceUpsert';
        let arr_hotel_id = [];
        $('#grid_selected_hotel .item_selected_hotel').each(function () {
            arr_hotel_id.push($(this).data('value'));
        });

        if (arr_hotel_id.length <= 0) {
            _msgalert.error("Bạn phải chọn dịch vụ khách sạn");
            return;
        }

        let data = {
            supplier_id: this.search_params.supplier_id,
            hotel_ids: arr_hotel_id.join()
        };

        _ajax_caller.post(url, data, function (result) {
            if (result.isSuccess) {
                _msgalert.success(result.message);
                _supplier_service_detail.modal.modal('hide');
                _supplier_service_detail.ReloadListing();
            } else {
                _msgalert.error(result.message);
            }
        });
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/ServiceListing", input, function (result) {
            $('#grid_supplier_service_detail').html(result);
        });
    },

    ReloadListing: function () {
        this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    },

    OnChangeServiceType: function (value) {
        this.search_params.service_type = value;
        this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    OnChangeNameSearch: function (value) {
        this.search_params.service_name = value;
        this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    OnKeyUpSearchName: function (e) {
        if (e.which === 13) {
            _supplier_service_detail.OnChangeNameSearch(e.target.value);
        } else {
            clearInterval(this._ChangeIntervalName);
            this._ChangeIntervalName = setInterval(function () {
                _supplier_service_detail.OnChangeNameSearch(e.target.value);
                clearInterval(this._ChangeIntervalName);
            }, 1000);
        }
    }
}

var _supplier_ticket = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this.search_params = {
            supplier_id: $('#global_supplier_id').val(),
            page_index: 1,
            page_size: 10
        }
        this.ReloadListing(this.search_params);
    },

    Listing: function (input) {
        _ajax_caller.post("/Supplier/TicketListing", input, function (result) {
            $('#grid_supplier_ticket').html(result);
        });
    },

    ReloadListing: function () {
        this.search_params.page_index = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.page_index = input;
        this.Listing(this.search_params);
    }
}

var _supplier_program = {
    Init: function () {
        this.modal = $('#global_modal_popup');
        this._ChangeIntervalName = null;
        this.search_params = {
            SupplierID: $('#global_supplier_id').val(),
            ProgramCode: null,
            ServiceType: "1",
            PageIndex: 1,
            PageSize: 10
        }
        this.ReloadListing(this.search_params);
    },

    Listing: function (input) {
        _ajax_caller.post("/Programs/SearchProgramsBySupplierId", input, function (result) {
            $('#grid_supplier_program').html(result);
        });
    },

    ReloadListing: function () {
        this.search_params.PageIndex = 1;
        this.Listing(this.search_params);
    },

    Paging: function (input) {
        this.search_params.PageIndex = input;
        this.Listing(this.search_params);
    }
}
$(document).ready(function () {
    $('input').attr('autocomplete', 'off');
    //_supplier_detail.Init();
    _supplier_contact.Init();
    _supplier_payment.Init();
    _supplier_order.Init();
    _supplier_ticket.Init();
    _supplier_service_detail.Init();
    _supplier_program.Init();

    $("body").on('click', ".supplier-button-fly", function (ev, picker) {
        $(".supplier-detail-change-tab-button").removeClass("active")
        $(".supplier-button-fly").addClass("active")
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        }
        _supplier_order.FlyListing(model);
    });
    $("body").on('click', ".supplier-button-hotel", function (ev, picker) {
        $(".supplier-detail-change-tab-button").removeClass("active")
        $(".supplier-button-hotel").addClass("active")
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        }
        _supplier_order.HotelListing(model);
    });
    $("body").on('click', ".supplier-button-tour", function (ev, picker) {
        $(".supplier-detail-change-tab-button").removeClass("active")
        $(".supplier-button-tour").addClass("active")
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        }
        _supplier_order.TourListing(model);
    });
    $("body").on('click', ".supplier-button-other", function (ev, picker) {
        $(".supplier-detail-change-tab-button").removeClass("active")
        $(".supplier-button-other").addClass("active")
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        }
        _supplier_order.OtherListing(model);
    });
    $("body").on('click', ".supplier-button-vinWonder", function (ev, picker) {
        $(".supplier-detail-change-tab-button").removeClass("active")
        $(".supplier-button-vinWonder").addClass("active")
        var model = {
            SupplierId: $('#global_supplier_id').val(),
            PageIndex: 1,
            PageSize: 10,
            CreateDateStr: null,
            EndDateStr: null,
        }
        if ($('#createdate').data('daterangepicker') !== undefined &&
            $('#createdate').data('daterangepicker') != null && isPickerApprove) {
            model.CreateDateStr = $('#createdate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            model.EndDateStr = $('#createdate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        }
        _supplier_order.VinWonderListing(model);
    });
});