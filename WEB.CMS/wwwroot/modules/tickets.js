
var _ticket_service = {
    Init: function () {
        this.modal_element = $('#global_modal_popup');
        this.OnSearch();
        // Xóa lỗi khi user nhập lại
        $(document).on('input change', '#form_ticket input, #form_ticket select', function () {
            $(this).removeClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
            $(this).parent().find('.select2-container').removeClass('is-invalid');
        });


        // === MỞ POPUP CHỈNH SỬA VÉ ===
        


    },

    GetParam: function () {
        let services = $('#sl_search_service').val();
        //let provinces = $('#sl_search_province').val();
        //let stars = $('#sl_search_star').val();
        //let brands = $('#sl_search_brand').val();
        let users = $('#sl_search_suggest_user').val();

        let objSearch = {
            FullName: $('#ip_search_fullname').val() != undefined ? $('#ip_search_fullname').val().trim().replaceAll(/  +/g, ' ') : null,
            ServiceType: services != null ? services.join(',') : "",
            //ProvinceId: provinces != null ? provinces.join(',') : "",
            //RatingStar: stars != null ? stars.join(',') : "",
            //ChainBrands: brands != null ? brands.join(',') : "",
            SalerId: users != null ? users.join(',') : "",
            currentPage: 1,
            pageSize: 10
        }
        return objSearch;
    },

    Search: function (input) {
        window.scrollTo(0, 0);
        $('#imgLoading').show();
        $.ajax({
            url: "/Tickets/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid_data').html(result);
            }
        });
    },

    OnSearch: function () {
        let objSearch = this.GetParam();
        this.SearchParam = objSearch;
        this.Search(objSearch);
    },

    OnPaging: function (value) {
        var objSearch = this.GetParam()
        objSearch.currentPage = value;
        this.SearchParam = objSearch
        this.Search(objSearch);
    },

    ReLoad: function () {
        this.Search(this.SearchParam);
    },

    OnChangeFullNameSearch: function (value) {
        var searchobj = this.SearchParam;
        searchobj.FullName = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeServiceSearch: function (value) {
        var searchobj = this.SearchParam;
        searchobj.ServiceType = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    OnChangeSalerSearch: function (value) {
        var searchobj = this.SearchParam;
        searchobj.SalerId = value;
        searchobj.currentPage = 1;
        this.SearchParam = searchobj;
        this.Search(searchobj);
    },

    GetFormData: function ($form) {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};

        $.map(unindexed_array, function (n, i) {
            indexed_array[n['name']] = n['value'];
        });

        return indexed_array;
    },

    Export: function () {
        $('#btnExport').prop('disabled', true);
        $('#icon-export').removeClass('fa-file-excel-o');
        $('#icon-export').addClass('fa-spinner fa-pulse');
        var objSearch = this.GetParam()
        objSearch.currentPage = 1;
        objSearch.pageSize = 100000000;
        this.SearchParam = objSearch
        $.ajax({
            url: "/Tickets/ExportExcel",
            type: "Post",
            data: this.SearchParam,
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

    modal_element: $('#global_modal_popup'),
  LoadFilterDropdowns : function () {
        // Load GroupProduct theo parentId
        function loadSelect(selector, parentId, placeholder) {
            $(selector).prop('disabled', true);
            $.get('/Tickets/GetCategories', { parentId: parentId }, function (res) {
                let html = `<option value="">${placeholder || '-- Chọn --'}</option>`;
                if (res.success && res.data) {
                    res.data.forEach(x => { html += `<option value="${x.id}">${x.name}</option>`; });
                }
                $(selector).html(html).prop('disabled', false).trigger('change.select2');
            });
        }

        // Trạng thái (nếu bạn muốn load động thay vì lấy từ ViewBag)
        // $.get('/Tickets/GetTicketStatus', function (res) {
        //     if (res.success) {
        //         let html = '<option value="">Trạng thái</option>';
        //         res.data.forEach(x => html += `<option value="${x.id}">${x.name}</option>`);
        //         $('[name="statusFilter"]').html(html).trigger('change.select2');
        //     }
        // });
      // ✅ Load các dropdown filter
      loadSelect('[name="productFilter"]', 85, 'Tên sản phẩm'); // 🔥 parentId = 85
        loadSelect('[name="categoryFilter"]', 76, 'Danh mục');
        loadSelect('[name="playZoneFilter"]', 77, 'Khu vực vui chơi');

        // Khi đổi danh mục -> load loại vé con
        $(document).off('change', '[name="categoryFilter"]').on('change', '[name="categoryFilter"]', function () {
            const parentId = $(this).val();
            if (parentId) loadSelect('[name="ticketTypeFilter"]', parentId, 'Loại vé');
            else $('[name="ticketTypeFilter"]').html('<option value="">Loại vé</option>');
        });
    },

    // Gọi AJAX lọc + phân trang
    applyFilter : function (pageIndex) {
        const supplierId = $('#hdSupplierId').val();

        const data = {
            supplierId: supplierId,
            pageIndex: pageIndex || 1,
            pageSize: 10,
            search: $('[name="keywordFilter"]').val() || null,
            status: $('[name="statusFilter"]').val() || null,
            playZoneId: $('[name="playZoneFilter"]').val() || null,
            categoryId: $('[name="categoryFilter"]').val() || null,
            ticketTypeId: $('[name="ticketTypeFilter"]').val() || null,
            //productId: $('[name="productFilter"]').val() || null,          // ✅ thêm
            //targetAudience: $('[name="targetAudienceFilter"]').val() || null // ✅ thêm
            expiredDate: $('[name="expiredDateFilter"]').val() || null
        };

        _global_function?.AddLoading && _global_function.AddLoading();
        $.ajax({
            url: '/Tickets/Filter',
            type: 'GET',
            data: data,
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (html) {
                $('#ticket_table_wrapper').html(html);
            },
            error: function (xhr) {
                _msgalert?.error && _msgalert.error('Lỗi tải dữ liệu: ' + xhr.statusText);
            },
            complete: function () {
                _global_function?.RemoveLoading && _global_function.RemoveLoading();
            }
        });
    },


  
    // ---- Mở popup Thêm / Sửa vé ----
    ShowAddOrUpdate: function (id) {
        debugger;
        var url = '/Tickets/AddOrUpdate';
        var supplierId = $('#hdSupplierId').val() || 0; // lấy từ hidden trên trang Detail

        _ajax_caller.get(url, { id: id, supplierId: supplierId }, function (html) {
            // Gắn nội dung modal
            $('#global_modal_popup').html(html);

            // Xóa toàn bộ sự kiện cũ để tránh lặp
            $('#global_modal_popup').off('shown.bs.modal');
            $('#global_modal_popup').off('hidden.bs.modal');

            // Hiển thị modal
            $('#global_modal_popup').modal('show');

            // Khi modal hiển thị xong
            $('#global_modal_popup').on('shown.bs.modal', function () {

                // Khởi tạo select2 trong phạm vi modal
                $('#global_modal_popup .select2').each(function () {
                    const $el = $(this);
                    if ($el.data('select2')) $el.select2('destroy');
                    $el.select2({
                        width: '100%',
                        dropdownParent: $('#global_modal_popup')
                    });
                });

                // 🔥 Chỉ gọi 1 trong 2 — không bao giờ cả hai
                if (id > 0) {
                    _ticket_service.LoadTicketDetail(id);
                } else {
                    _ticket_service.LoadDropdowns();
                }
            });

            // Khi modal đóng thì dọn sạch nội dung (tránh rác HTML)
            $('#global_modal_popup').on('hidden.bs.modal', function () {
                $('#global_modal_popup').html('');
            });
        });
    },


    // ==== HÀM TÁCH RIÊNG ĐỂ GẮN EVENT CHO DANH MỤC ====
    BindCategoryChange: function () {
        $(document).off('change', '#ddlCategory').on('change', '#ddlCategory', function () {
            let parentId = $(this).val();

            if (parentId) {
                $.ajax({
                    url: '/Tickets/GetCategories?parentId=' + parentId,
                    type: 'GET',
                    success: function (res) {
                        let html = '<option value="">-- Chọn loại vé --</option>';
                        if (res.success && res.data) {
                            $.each(res.data, function (i, item) {
                                html += `<option value="${item.id}">${item.name}</option>`;
                            });
                        }
                        $('#ddlTicketType').html(html).trigger('change.select2');
                    },
                    error: function () {
                        $('#ddlTicketType').html('<option value="">Lỗi tải loại vé</option>');
                    }
                });
            } else {
                $('#ddlTicketType').html('<option value="">-- Chọn loại vé --</option>');
            }
        });
    },



    LoadDropdowns: function () {
        debugger
        function loadSelect(selectId, parentId, selectedId) {
            $.ajax({
                url: '/Tickets/GetCategories?parentId=' + parentId,
                type: 'GET',
                success: function (res) {
                    let html = '<option value="">-- Chọn --</option>';
                    if (res.success && res.data) {
                        $.each(res.data, function (i, item) {
                            html += `<option value="${item.id}" ${selectedId == item.id ? 'selected' : ''}>${item.name}</option>`;
                        });
                    }
                    $(selectId).html(html).trigger('change.select2');
                },
                error: function () {
                    $(selectId).html('<option value="">Lỗi tải dữ liệu</option>');
                }
            });
        }
        // ✅ Load đầy đủ các dropdown
        loadSelect('#ddlProduct', 85);   // 🔥 thêm: tên sản phẩm
        // load mặc định
        loadSelect('#ddlCategory', 76);
        loadSelect('#ddlPlayZone', 77);

        // 👉 Gắn event cho ddlCategory
        _ticket_service.BindCategoryChange();
    },



    LoadTicketDetail: function (id) {
        debugger
        $.ajax({
            url: '/Tickets/GetDetail',
            type: 'GET',
            data: { id: id },
            success: function (res) {
                debugger
                if (res.isSuccess && res.data) {
                    const t = res.data;
                    let form = $('#form_ticket');

                    // Điền các field cơ bản
                    form.find('[name="TicketCode"]').val(t.ticketCode);
                    form.find('[name="ImportDate"]').val(t.importDate?.split('T')[0]);
                    form.find('[name="ExpiredDate"]').val(t.expiredDate?.split('T')[0]);
                    form.find('[name="SoldDuration"]').val(t.soldDuration);
                    form.find('[name="QRCode"]').val(t.qrCode);
                    form.find('[name="Status"]').val(t.status).trigger('change');
                    form.find('[name="SupplierId"]').val(t.supplierId);
                    // ✅ Điền thêm 3 field mới
                    form.find('[name="ProductId"]').val(t.productId).trigger('change');
                    form.find('[name="ImportPrice"]').val(t.importPrice);
                    form.find('[name="TargetAudience"]').val(t.targetAudience).trigger('change');

                    // 🔥 Gọi load dropdown có selectedId để nó render đúng
                    function loadSelect(selectId, parentId, selectedId) {
                        return $.ajax({
                            url: '/Tickets/GetCategories?parentId=' + parentId,
                            type: 'GET',
                            success: function (res) {
                                var html = '<option value="">-- Chọn --</option>';
                                if (res.success && res.data) {
                                    $.each(res.data, function (i, item) {
                                        html += `<option value="${item.id}" ${selectedId == item.id ? 'selected' : ''}>${item.name}</option>`;
                                    });
                                }
                                $(selectId).html(html).trigger('change.select2');
                            }
                        });
                    }

                    // Load danh mục, khu vui chơi, loại vé theo dữ liệu có sẵn
                    $.when(
                        loadSelect("#ddlCategory", 76, t.category),
                        loadSelect("#ddlPlayZone", 77, t.playZone),
                         loadSelect("#ddlProduct", 85, t.productId)
                    ).done(function () {
                        // Sau khi danh mục load xong -> load loại vé con
                        loadSelect("#ddlTicketType", t.category, t.ticketType);
                    });

                    // 👉 Gắn lại event change cho ddlCategory để reload loại vé khi đổi
                    _ticket_service.BindCategoryChange();
                }
            }
        });
    },



    OnSave: function () {
        var form = $('#form_ticket');
        var formData = _ticket_service.GetFormData(form);
        var isValid = true;

        // Xóa lỗi cũ
        form.find('.is-invalid').removeClass('is-invalid');
        form.find('.invalid-feedback').remove();

        function showError(fieldName, message) {
            const field = form.find(`[name="${fieldName}"]`);
            field.addClass('is-invalid');
            if (field.next('.invalid-feedback').length === 0) {
                field.after(`<div class="invalid-feedback">${message}</div>`);
            }
            isValid = false;
        }

        // --- VALIDATION ---
        if (!formData.TicketCode?.trim()) showError('TicketCode', 'Vui lòng nhập mã vé.');
        if (!formData.ImportDate) showError('ImportDate', 'Vui lòng chọn ngày nhập.');
        if (!formData.ExpiredDate) showError('ExpiredDate', 'Vui lòng chọn ngày hết hạn.');

        if (formData.ImportDate && formData.ExpiredDate) {
            const importDate = new Date(formData.ImportDate);
            const expiredDate = new Date(formData.ExpiredDate);
            if (importDate > expiredDate) showError('ExpiredDate', 'Ngày hết hạn phải lớn hơn hoặc bằng ngày nhập.');
        }

        if (!formData.Category) showError('Category', 'Vui lòng chọn danh mục.');
        if (!formData.TicketType) showError('TicketType', 'Vui lòng chọn loại vé.');
        if (!formData.PlayZone) showError('PlayZone', 'Vui lòng chọn khu vực vui chơi.');
        if (!formData.Status) showError('Status', 'Vui lòng chọn trạng thái.');

        // Nếu có lỗi thì dừng
        if (!isValid) return;

        // --- GỬI LÊN SERVER ---
        var url = formData.TicketId > 0 ? '/Tickets/Update' : '/Tickets/Create';
        var supplierId = formData.SupplierId || $('#hdSupplierId').val();

        _global_function.AddLoading();

        $.ajax({
            url: url,
            type: 'POST',
            data: { model: formData },
            success: function (res) {
                _global_function.RemoveLoading();
                if (res.isSuccess) {
                    _msgalert.success(res.message);
                    $('#global_modal_popup').modal('hide');

                    // ✅ Reload lại bảng vé (PartialView)
                    $.ajax({
                        url: '/Tickets/Detail',
                        type: 'GET',
                        data: { id: supplierId },
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }, // Quan trọng!
                        success: function (html) {
                            $('#ticket_table_wrapper').html(html);
                            //_msgalert.success("Danh sách vé đã được cập nhật!");
                        },
                        error: function () {
                            _msgalert.error('Không thể tải lại danh sách vé.');
                        }
                    });
                } else {
                    _msgalert.error(res.message);
                }
            },
            error: function (xhr) {
                _global_function.RemoveLoading();
                _msgalert.error("Lỗi: " + xhr.statusText);
            }
        });
    },




    // ===== Xem vé (popup có QRCode) =====
    ShowTicketPopup: function (id, status) {
        if (status != 1) {
            _msgalert.warning("Vé đã được sử dụng hoặc không hợp lệ!");
            return;
        }
        $.get('/Tickets/ShowTicket', { id: id }, function (html) {
            $('#global_modal_popup').html(html);
            $('#global_modal_popup').modal('show');
        });
    },


    GetFormData: function ($form) {
        var unindexed_array = $form.serializeArray();
        var indexed_array = {};
        $.map(unindexed_array, function (n) {
            indexed_array[n['name']] = n['value'];
        });
        return indexed_array;
    },

    OnAdd: function () {
        let Form = $('#form_supplier');
        Form.validate({
            rules: {
                FullName: "required",
                Email: {
                    email: true
                },
                Phone: {
                    minlength: 10,
                    maxlength: 11,
                    digits: true
                }
                //ContactName: "required",
                //ContactPhone: {
                //    required: true,
                //    exactlength: 10,
                //    digits: true
                //},
                //ContactEmail: {
                //    email: true
                //},
                //BankAccountName: "required",
                //BankAccountNumber: {
                //    required: true,
                //    maxlength: 20,
                //    digits: true
                //},
                //BankId: "required"
            },
            messages: {
                FullName: "Vui lòng nhập tên nhà cung cấp",
                Email: {
                    email: 'Email không đúng định dạng'
                },
                Phone: {
                    exactlength: "Số điện thoại phải nhập đúng 10 / 11 kí tự dạng số",
                    digits: "Số điện thoại phải là kí tự dạng số"
                }
                //ContactName: "Vui lòng nhập tên liên hệ",
                //ContactPhone: {
                //    required: "Vui lòng nhập số điện thoại",
                //    exactlength: "Số điện thoại phải nhập đúng 10 kí tự dạng số",
                //    digits: "Số điện thoại phải là kí tự dạng số"
                //},
                //ContactEmail: {
                //    email: 'Email không đúng định dạng'
                //},
                //BankAccountName: "Vui lòng nhập chủ tài khoản ngân hàng",
                //BankAccountNumber: {
                //    required: "Vui lòng nhập số tài khoản",
                //    maxlength: "Số tài khoản chỉ chứa tối đa 20 kí tự",
                //    digits: "Số tài khoản phải là kí tự dạng số"
                //},
                //BankId: "Vui lòng nhập tên ngân hàng"
            }
        });

        if (!Form.valid()) { return; }

        let formData = this.GetFormData(Form);

        formData['SupplierCode'] = 'SUPPLIER_CODE';

        let url = formData.SupplierId > 0 ? "/Supplier/Update" : "/Supplier/Create";
        _global_function.AddLoading()
        _ajax_caller.post(url, { model: formData }, function (result) {
            _global_function.RemoveLoading()

            if (result.isSuccess) {
                _msgalert.success(result.message);
                _ticket_service.modal_element.modal('hide');
                _ticket_service.ReLoad();
            } else {
                _msgalert.error(result.message);
            }

        });
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
        if (fields.STT === true) {
            $('#STT').prop('checked', true);
        } else {
            $('#STT').prop('checked', false);
        }
        if (fields.SupplierId === true) {
            $('#SupplierId').prop('checked', true);
        } else {
            $('#SupplierId').prop('checked', false);
        }
        if (fields.SupplierName === true) {
            $('#SupplierName').prop('checked', true);
        } else {
            $('#SupplierName').prop('checked', false);
        }
        if (fields.EstablistYear === true) {
            $('#EstablistYear').prop('checked', true);
        } else {
            $('#EstablistYear').prop('checked', false);
        }
        if (fields.Address === true) {
            $('#Address').prop('checked', true);
        } else {
            $('#Address').prop('checked', false);
        }
        if (fields.Contact === true) {
            $('#Contact').prop('checked', true);
        } else {
            $('#Contact').prop('checked', false);
        }
        if (fields.Service === true) {
            $('#Service').prop('checked', true);
        } else {
            $('#Service').prop('checked', false);
        }
        if (fields.SalerId === true) {
            $('#SalerId').prop('checked', true);
        } else {
            $('#SalerId').prop('checked', false);
        }
        if (fields.CreateBy === true) {
            $('#CreateBy').prop('checked', true);
        } else {
            $('#CreateBy').prop('checked', false);
        }
        if (fields.CreateDate === true) {
            $('#CreateDate').prop('checked', true);
        } else {
            $('#CreateDate').prop('checked', false);
        }
        if (fields.UpdatedBy === true) {
            $('#UpdatedBy').prop('checked', true);
        } else {
            $('#UpdatedBy').prop('checked', false);
        }
        if (fields.UpdatedDate === true) {
            $('#UpdatedDate').prop('checked', true);
        } else {
            $('#UpdatedDate').prop('checked', false);
        }
    },

    ChangeSetting: function (position) {
        this.ShowHideColumn();
        switch (position) {
            case 1:
                if ($('#STT').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 2:
                if ($('#SupplierId').is(":checked")) {
                    fields.SupplierId = true
                } else {
                    fields.SupplierId = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 3:
                if ($('#SupplierName').is(":checked")) {
                    fields.SupplierName = true
                } else {
                    fields.SupplierName = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 4:
                if ($('#EstablistYear').is(":checked")) {
                    fields.EstablistYear = true
                } else {
                    fields.EstablistYear = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 5:
                if ($('#Address').is(":checked")) {
                    fields.Address = true
                } else {
                    fields.Address = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 6:
                if ($('#Contact').is(":checked")) {
                    fields.Contact = true
                } else {
                    fields.Contact = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 7:
                if ($('#Service').is(":checked")) {
                    fields.Service = true
                } else {
                    fields.Service = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;

            case 8:
                if ($('#SalerId').is(":checked")) {
                    fields.SalerId = true
                } else {
                    fields.SalerId = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 9:
                if ($('#CreateBy').is(":checked")) {
                    fields.CreateBy = true
                } else {
                    fields.CreateBy = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;

            case 10:
                if ($('#CreateDate').is(":checked")) {
                    fields.CreateDate = true
                } else {
                    fields.CreateDate = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            case 11:
                if ($('#UpdatedBy').is(":checked")) {
                    fields.UpdatedBy = true
                } else {
                    fields.UpdatedBy = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;

            case 12:
                if ($('#UpdatedDate').is(":checked")) {
                    fields.UpdatedDate = true
                } else {
                    fields.UpdatedDate = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(fields), 10);
                break;
            default:
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
}

var _changeInterval = null;
$("#ip_search_fullname").keyup(function (e) {
    if (e.which === 13) {
        _ticket_service.OnChangeFullNameSearch(e.target.value);
    } else {
        clearInterval(_changeInterval);
        _changeInterval = setInterval(function () {
            _ticket_service.OnChangeFullNameSearch(e.target.value);
            clearInterval(_changeInterval);
        }, 1000);
    }
});

$('#sl_search_service').change(function () {
    let values = $(this).val();
    let str_values = values != null ? values.join(',') : "";
    _ticket_service.OnChangeServiceSearch(str_values);
});

$('#sl_search_suggest_user').change(function () {
    let values = $(this).val();
    let str_values = values != null ? values.join(',') : "";
    _ticket_service.OnChangeSalerSearch(str_values);
});

$('.select-service-type').click(function (e) {
    var seft = $(this);
    seft.toggleClass("open");
});

$(document).ready(function () {
    $('input').attr('autocomplete', 'off');
    _ticket_service.Init();

    $("#sl_search_service").select2({
        placeholder: "Tất cả dịch vụ",
        multiple: true
    });

    $("#sl_search_suggest_user").select2({
        //theme: 'bootstrap4',
        placeholder: "Người tạo",
        multiple: true,
        maximumSelectionLength: 3,
        ajax: {
            url: "/Order/UserSuggestion",
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
                            text: `${item.fullname} - ${item.username}`,
                            id: item.id,
                        }
                    })
                };
            }
        }
    });
   

});
