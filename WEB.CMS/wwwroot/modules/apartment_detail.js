var _order_detail_create = {
    _deletedLedgerIds: [],
    Initialization: function () {
        _order_detail_create.ClientSuggesstion()
        _order_detail_common.Select2WithFixedOptionAndNoSearch($("#branch"))
        $('#main-staff').select2();
        $('#sub-staff').select2();

        _order_detail_create.DynamicBindClientInput();
        _order_detail_create.UserSuggesstion();
        $('#client-select').on('select2:select', function (e) {
            var data = e.params.data;
            if (data && data.address) {
                $('#order_label').val(data.address);
            }
            $('#btn_summit_order').prop('disabled', false);
            $('.error_client_select').hide();
        });

        $('#client-select').on('select2:clear', function () {
            $('#order_label').val('');
            $('#btn_summit_order').prop('disabled', true);
        });


        //================= FORMAT TIỀN VNĐ REALTIME + CỘNG TỔNG =================
        $(document).on("input",
            ".room-price, .service-fee, .total-paid, .expense-amount",
            function () {
                let $this = $(this);
                let raw = $this.val().replace(/\./g, "");
                if (raw === "") { $this.val(""); return; }
                $this.val(_order_detail_create.formatCurrencyVND(raw));

                let $row = $this.closest("tr");

                // Nếu sửa giá phòng hoặc phí DV -> cập nhật Tổng phải thu & summary
                if ($this.hasClass("room-price") || $this.hasClass("service-fee")) {
                    _order_detail_create.updateThuRowTotal($row);
                    _order_detail_create.updateSummary();
                }

                // Nếu sửa Tổng đã thanh toán -> cập nhật Tổng đã thanh toán của cả bảng
                if ($this.hasClass("total-paid")) {
                    _order_detail_create.updateSumThu();
                }

                // Nếu sửa tiền chi -> cập nhật Tổng chi & summary
                if ($this.hasClass("expense-amount")) {
                    _order_detail_create.updateSummary();
                }
            });

        // =============== ADD ROW THU ====================
        $(document).on("click", "#addThu", function () {
            debugger
            let row = `
<tr class="ledger-row">
    <td></td>
    <td>
        <input type="hidden" class="ledger-id" value="0" />
        <input class="form-control customer"/>
    </td>
    <td><input type="date" class="form-control date-contract"/></td>
    <td><input type="date" class="form-control date-expire"/></td>
    <td><input class="form-control room-price"/></td>
    <td><input class="form-control service-fee"/></td>

    <!-- TỔNG PHẢI THU -->
    <td><input class="form-control total-must-pay" readonly /></td>

    <!-- TỔNG ĐÃ THANH TOÁN -->
    <td><input class="form-control total-paid"/></td>

    <td>
        <a href="javascript:;" class="red remove-row">
            <i class="fa fa-times"></i>
        </a>
    </td>
</tr>`;


            $("#tblThu tbody tr:last").before(row);
            _order_detail_create.reIndex();
            _order_detail_create.updateSummary();
        });


        // =============== ADD ROW CHI ====================
        $(document).on("click", "#addChi", function () {
            debugger
            let row = `
    <tr class="ledger-row">
        <td></td>
        <td>
            <input type="hidden" class="ledger-id" value="0" />
            <select class="form-control expense-type">
                <option value="1">Hoa hồng</option>
                <option value="2">Dịch vụ</option>
            </select>
        </td>
        <td><input class="form-control expense-desc"/></td>
        <td><input class="form-control expense-amount"/></td>
        <td><input type="date" class="form-control expense-date"/></td>
        <td>
            <a href="javascript:;" class="red remove-row">
                <i class="fa fa-times"></i>
            </a>
        </td>
    </tr>`;

            $("#tblChi tbody tr:last").before(row);
            _order_detail_create.reIndex();
            _order_detail_create.updateSummary();
        });


        // =============== REMOVE ROW ====================
        $(document).on("click", ".remove-row", function () {
            let id = parseInt($(this).closest("tr").find(".ledger-id").val());

            if (id > 0) {
                _order_detail_create._deletedLedgerIds.push(id);
            }

            $(this).closest("tr").remove();
            _order_detail_create.reIndex();

            _order_detail_create.updateSumThu();
            _order_detail_create.updateSummary();
        });

        // Tính lại Tổng phải thu, Tổng chi, Lợi nhuận, Tổng đã thanh toán khi mở popup
        $("#tblThu tbody tr.ledger-row").each(function () {
            _order_detail_create.updateThuRowTotal($(this));
        });
        _order_detail_create.updateSumThu();
        _order_detail_create.updateSummary();
    },
    //=============== FORMAT VNĐ ====================
    formatCurrencyVND: function (value) {
        return value.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    },

    parseMoney: function (value) {
        if (!value) return 0;
        return parseFloat(value.replace(/\./g, "")) || 0;
    },
    // Hàm đánh lại STT cho tất cả row "ledger-row"
    reIndex: function () {
        $("#tblThu tbody tr.ledger-row").each(function (i) {
            $(this).find("td:first").text(i + 1);
        });

        $("#tblChi tbody tr.ledger-row").each(function (i) {
            $(this).find("td:first").text(i + 1);
        });
    },
    // Cập nhật tổng phải thu 1 dòng
    updateThuRowTotal: function ($row) {
        let room = _order_detail_create.parseMoney($row.find(".room-price").val());
        let fee = _order_detail_create.parseMoney($row.find(".service-fee").val());
        let total = room + fee;

        let $totalInput = $row.find(".total-must-pay");
        if ($totalInput.length) {
            if (total > 0) {
                $totalInput.val(_order_detail_create.formatCurrencyVND(total.toString()));
            } else {
                $totalInput.val("");
            }
        }
    },

    // Tổng đã thanh toán (bảng Thu)
    updateSumThu: function () {
        let sum = 0;
        $("#tblThu tbody tr.ledger-row").each(function () {
            let val = $(this).find(".total-paid").val();
            sum += _order_detail_create.parseMoney(val);
        });

        $("#sumThu").text(_order_detail_create.formatCurrencyVND(sum.toString()));
    },

    // Tổng chi (bảng Chi)
    updateSumChi: function () {
        let sum = 0;
        $("#tblChi tbody tr.ledger-row").each(function () {
            let val = $(this).find(".expense-amount").val();
            sum += _order_detail_create.parseMoney(val);
        });

        $("#sumChi").text(_order_detail_create.formatCurrencyVND(sum.toString()));
        return sum;
    },

    // Tổng phải thu, tổng đã chi, lợi nhuận
    updateSummary: function () {
        // Tổng phải thu = sum(total-must-pay)
        let sumMust = 0;
        $("#tblThu tbody tr.ledger-row").each(function () {
            let val = $(this).find(".total-must-pay").val();
            sumMust += _order_detail_create.parseMoney(val);
        });

        // Tổng chi
        let sumChi = _order_detail_create.updateSumChi();

        // Lợi nhuận
        let profit = sumMust - sumChi;

        $("#sumThuMust").text(_order_detail_create.formatCurrencyVND(sumMust.toString()));
        $("#sumChiTotal").text(_order_detail_create.formatCurrencyVND(sumChi.toString()));
        $("#sumProfit").text(_order_detail_create.formatCurrencyVND(profit.toString()));
    },

    ClientSuggesstion: function () {
        $('#client-select').select2({
            placeholder: 'Chọn căn hộ',
            allowClear: true,
            width: '100%',
            minimumInputLength: 0,
            dropdownParent: $('#create_order_manual'), // 🔥 BẮT BUỘC khi dùng trong modal
            ajax: {
                url: '/Apartment/Suggest',
                type: 'GET',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        term: params.term || '',
                        size: 20
                    };
                },
                processResults: function (data) {
                    // Controller đã trả đúng { id, text, address } rồi
                    return {
                        results: $.map(data, function (item) {
                            return {
                                id: item.id,
                                text: item.text,      // 🔥 text ở đây chắc chắn có
                                address: item.address // mình giữ lại để fill địa chỉ
                            };
                        })
                    };
                },
                cache: true
            }
        });
    },
    openRoomLedger: function (roomId) {
        $.ajax({
            url: "/Apartment/RoomLedgerPopup",
            type: "GET",
            data: { roomId: roomId },
            success: function (html) {
                debugger
                // XÓA modal cũ nếu tồn tại
                $("#myModal5").remove();

                // append modal mới vào body
                $("body").append(html);

                // mở modal
                $("#myModal5").modal("show");
            }
        });
    },

    saveRoomLedger: function () {
        debugger
        let orderId = $("#orderId").val();  // hoặc ViewBag.OrderId đưa vào input hidden
        // Validate form
        if (!_order_detail_create.validateLedger()) return;
        let model = {
            RoomId: parseInt($("#ledger_roomId").val()),
            HotelId: parseInt($("#ledger_hotelId").val()),   // <<< THÊM VÔ NÈ
            Thu: [],
            Chi: [],
            DeletedIds: _order_detail_create._deletedLedgerIds
        };

        // === Collect Thu ===
        $("#tblThu tbody tr.ledger-row").each((i, row) => {
            let $r = $(row);

            let customer = $r.find(".customer").val().trim();
            let price = $r.find(".room-price").val().trim();
            let paid = $r.find(".total-paid").val().trim();

            // Skip row rỗng
            if (!customer && !price && !paid) return;

            model.Thu.push({
                Id: parseInt($r.find(".ledger-id").val()) || 0,
                CustomerName: customer,
                ContractDate: $r.find(".date-contract").val() || null,
                ExpireDate: $r.find(".date-expire").val() || null,
                RoomPrice: _order_detail_create.parseMoney(price),
                ServiceFee: _order_detail_create.parseMoney($r.find(".service-fee").val()),
                TotalPaid: _order_detail_create.parseMoney(paid)
            });
        });

        // === Collect Chi ===
        $("#tblChi tbody tr.ledger-row").each((i, row) => {
            debugger
            let $r = $(row);

            let desc = $r.find(".expense-desc").val().trim();
            let amount = $r.find(".expense-amount").val().trim();

            if (!desc && !amount) return;

            model.Chi.push({
                Id: parseInt($r.find(".ledger-id").val()) || 0,
                ExpenseTypeId: parseInt($r.find(".expense-type").val()),
                Description: desc,
                ExpenseAmount: _order_detail_create.parseMoney(amount),
                ExpenseDate: $r.find(".expense-date").val() || null
            });
        });

        // DEBUG
        console.log("DATA SEND:", model);

        $.ajax({
            url: "/Apartment/SaveRoomLedger",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(model),

            success: function (res) {
                debugger
                if (res.status) {
                    debugger
                    toastr.success(res.msg);
                    $("#myModal5").modal("hide");

                    
                    // 🔥 TỰ ĐỘNG RELOAD LẠI GIAO DIỆN PACKAGES KHÔNG CẦN F5
                    //_order_detail_create.reloadPackages();
                    setTimeout(function () {
                        window.location.href = "/Apartment/" + orderId;
                    }, 1000);
                  

                } else {
                    toastr.error(res.msg);
                }
            },

            error: function (err) {
                console.error("SAVE ERROR:", err);
                toastr.error("Có lỗi khi lưu ledger");
            }
        });


    },
    reloadPackages: function () {
        debugger
        

        $.ajax({
            url: "/Apartment/Packages",
            type: "POST",
            data: { orderId: orderId },
            success: function (html) {
                $("#packages-container").html(html);
                // 🔥 chính là DIV ôm cái bảng PackagesApartment
            }
        });
    },

    

    // Validate dữ liệu nhập
    validateLedger: function () {
        let hasThu = false, hasChi = false, ok = true;

        // ==== Validate Thu ====
        $("#tblThu tbody tr.ledger-row").each(function () {
            let customer = $(this).find(".customer").val().trim();
            let price = $(this).find(".room-price").val().trim();
            let paid = $(this).find(".total-paid").val().trim();

            if (!customer && !price && !paid) return true;

            hasThu = true;

            if (!customer) { toastr.error("Nhập tên khách hàng ở bảng Thu"); ok = false; return false; }
            if (!price) { toastr.error("Nhập giá phòng ở bảng Thu"); ok = false; return false; }
            if (!paid) { toastr.error("Nhập tiền đã thu ở bảng Thu"); ok = false; return false; }
        });
        if (!ok) return false;

        // ==== Validate Chi ====
        $("#tblChi tbody tr.ledger-row").each(function () {
            let desc = $(this).find(".expense-desc").val().trim();
            let amount = $(this).find(".expense-amount").val().trim();

            if (!desc && !amount) return true;

            hasChi = true;

            if (!desc) { toastr.error("Nhập mô tả cho dòng Chi"); ok = false; return false; }
            if (!amount) { toastr.error("Nhập số tiền cho dòng Chi"); ok = false; return false; }
        });
        if (!ok) return false;

        // ==== Block nếu cả Thu + Chi đều không có dữ liệu ====
        if (!hasThu && !hasChi) {
            toastr.error("Vui lòng nhập ít nhất 1 dòng Thu hoặc 1 dòng Chi");
            return false;
        }

        return true;
    },



    ClientTemplateResult: function (item) {
        if (item.loading) {
            return item.text;
        }
        var type_name = '';
        var color_text = '';

        if ([1, 2, 3, 4].includes(item.clienttype)) {
            type_name = 'Khách hàng B2B'
        }
        else if (item.clienttype == 5) type_name = 'Khách hàng B2C'
        else if (item.clienttype == 6) type_name = 'Saler'
        var $container = $(
            _order_detail_html.html_option_client_suggesstion.replaceAll('{if_danger}', color_text).replaceAll('{Name}', item.clientname).replaceAll('{ClientType}', type_name).replaceAll('{Email}', item.email).replaceAll('{phone}', item.phone)
        );
        return $container;

    },
    ClientTemplateSelection: function (item) {
        $.ajax({
            url: "GetActiveContractByClientId",
            type: "post",
            data: { client_id: item.id },
            success: function (result) {
                if (result != undefined && result.status == 0) {
                    $('.error_client_select').hide()
                    $('#btn_summit_order').removeAttr('disabled')
                }
                else {
                    $('.error_client_select').show()
                    $('.error_client_select_p').html(result.msg)
                    $('#btn_summit_order').attr('disabled', 'disabled');
                }

            }
        });

        return (item.clientname + ' ( ' + item.phone + ' - ' + item.email + ' )')
    },
    ClientPreOptionSuggesstion: function (item) {
        if (item.loading) {
            return item.text;
        }
        var type_name = '';
        var color_text = ''
        if (item.clienttype <= 0 || item.clienttype == undefined) {
            type_name = 'Khách hàng chưa được kích hoạt'
            color_text = 'red'
        }
        else if (item.clienttype == 5) type_name = 'Khách hàng B2C'
        else type_name = 'Khách hàng B2B'
        return _order_detail_html.html_option_client_suggesstion.replaceAll('{if_danger}', color_text).replaceAll('{Name}', item.clientname).replaceAll('{ClientType}', type_name).replaceAll('{Email}', item.email).replaceAll('{phone}', item.phone)
    },
    Summit: function () {
        let Form = $("#form-create-order-manual");

        // validate basic
        Form.validate({
            rules: {
                "order_label": {
                    required: true,
                }
            },
            messages: {
                "order_label": {
                    required: "Địa chỉ không được để trống",
                }
            }
        });

        // check đã chọn căn chưa
        var hotelId = $('#client-select').val();
        if (!hotelId || parseInt(hotelId) <= 0) {
            _msgalert.error("Vui lòng chọn căn hộ cho đơn hàng này");
            return;
        }

        var order_label = $('#order_label').val();
        if (!order_label || order_label.trim() === '') {
            _msgalert.error("Vui lòng nhập địa chỉ cho đơn hàng này");
            return;
        }
        if (order_label.trim().length > 200) {
            _msgalert.error("Địa chỉ không được vượt quá 200 ký tự");
            return;
        }

        $('#btn_summit_order').hide();
        $('.img_loading_summit').show();

        var summit_model = {
            hotel_id: parseInt(hotelId),       // Id căn hộ (Hotel.Id)
            label: $('#order_label').val(),    // Địa chỉ
            note: $('#note').val()             // Ghi chú
        };

        $.ajax({
            url: "/Apartment/CreateApartmentOrder",       // action mới trong OrderManualController
            type: "post",
            data: summit_model,
            success: function (result) {
                $('.img_loading_summit').hide();
                if (result.status != 0) {
                    _msgalert.error(result.msg);
                    $('#btn_summit_order').show();
                    return;
                }
                _msgalert.success(result.msg);
                $('#btn_summit_order').prop("onclick", null).off("click");
                $('#btn_summit_order').text('Tạo đơn hàng căn hộ thành công');
                $('#btn_summit_order').show();

                setTimeout(function () {
                    window.location.href = "/Apartment/" + result.order_id;
                }, 1000);
            },
            error: function () {
                $('.img_loading_summit').hide();
                $('#btn_summit_order').show();
                _msgalert.error("Có lỗi xảy ra khi tạo đơn căn hộ");
            }
        });
    },

    DynamicBindClientInput: function () {
        $('body').on('click', '.modal-order', function (event) {
            if (!$(event.target).hasClass('modal-dialog')) {
                $(event.target).closest('.modal').removeClass('show');
                setTimeout(function () {
                    $(event.target).closest('.modal').remove();
                }, 300);
            }

        });


    },
    UserSuggesstion: function () {
        $('#main-staff').select2();
        $('#sub-staff').select2();

        $.ajax({
            url: "UserSuggestion",
            type: "post",
            data: { txt_search: "" },
            success: function (result) {
                if (result != undefined && result.data != undefined && result.data.length > 0) {
                    result.data.forEach(function (item) {
                        $('#main-staff').append(_order_detail_html.html_user_option.replaceAll('{user_id}', item.id).replace('{user_email}', item.email).replace('{user_name}', item.username).replace('{user_phone}', item.phone == undefined ? "" : ' - ' + item.phone))
                        $('#sub-staff').append(_order_detail_html.html_user_option.replaceAll('{user_id}', item.id).replace('{user_email}', item.email).replace('{user_name}', item.username).replace('{user_phone}', item.phone == undefined ? "" : ' - ' + item.phone))

                    });
                    $("#main-staff").trigger('change');
                    $("#sub-staff").trigger('change');
                    $('#main-staff').val(result.selected).trigger('change');
                }
                else {
                    $("#main-staff").trigger('change');
                    $("#sub-staff").trigger('change');
                }

            }
        });
    }
}


var _order_detail_create_service = {
    Initialization: function (hotel_booking_id) {
        $('body').on('click', '.onclick-button', function (event) {
            if (!$(this).hasClass("active")) {
                $(this).addClass("active");
                $(this).next('.form-down').slideDown();
                $('.form-down input').focus();

            } else {
                $(this).removeClass("active");
                $(this).next('.form-down').slideUp();
            }
        });
    },
    ServiceHotel: function (hotel_booking_id) {
        _global_function.AddLoading()
        if (hotel_booking_id == undefined || parseInt(hotel_booking_id) < 0) return;
        if ($('#AddHotelService').length) {
            $('#AddHotelService').removeClass('show')
            setTimeout(function () {
                $('#AddHotelService').remove();
            }, 300);

        }
        $.ajax({
            url: "AddHotelService",
            type: "post",
            data: { hotel_booking_id: hotel_booking_id },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_hotel.Initialization(hotel_booking_id);
                    _global_function.RemoveLoading()

                }, 300);

            }
        });
    },
    FlyingTicket: function (order_id, group_fly) {
        _global_function.AddLoading()

        if ($('#FlyBooking-Service').length) {
            $('#FlyBooking-Service').removeClass('show')
            setTimeout(function () {
                $('#FlyBooking-Service').remove();
            }, 300);

        }
        $.ajax({
            url: "AddFlyBookingService",
            type: "post",
            data: {
                order_id: order_id,
                group_fly: group_fly
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_fly.Initialization(order_id, group_fly);
                    _global_function.RemoveLoading()

                }, 300);

            }
        });
    },
    Tour: function (order_id, tour_id) {
        _global_function.AddLoading()

        if ($('#add-service-tour').length) {
            $('#add-service-tour').removeClass('show')
            setTimeout(function () {
                $('#add-service-tour').remove();
            }, 300);

        }
        $.ajax({
            url: "AddTourService",
            type: "post",
            data: {
                order_id: order_id,
                tour_id: tour_id
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_tour.Initialization(order_id, tour_id);
                    _global_function.RemoveLoading()

                }, 300);

            }
        });
    },
    OtherService: function (order_id, booking_id) {
        _global_function.AddLoading()

        if ($('#add-service-other').length) {
            $('#add-service-other').removeClass('show')
            setTimeout(function () {
                $('#add-service-other').remove();
            }, 300);

        }
        $.ajax({
            url: "AddOtherService",
            type: "post",
            data: {
                order_id: order_id,
                other_booking_id: booking_id
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_other.Initialization(order_id, booking_id);
                    _global_function.RemoveLoading()

                }, 300);

            }
        });
    },
    VinWonderService: function (order_id, booking_id) {
        _global_function.AddLoading()

        if ($('#add-service-other').length) {
            $('#add-service-other').removeClass('show')
            setTimeout(function () {
                $('#add-service-other').remove();
            }, 300);

        }
        $.ajax({
            url: "AddVinWonderService",
            type: "post",
            data: {
                order_id: order_id,
                booking_id: booking_id
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_vinwonder.Initialization(order_id, booking_id);
                    _global_function.RemoveLoading()

                }, 300);

            }
        });
    },
    WaterSportService: function (order_id, booking_id) {
        if ($('#add-service-watersport').length) {
            $('#add-service-watersport').removeClass('show')
            setTimeout(function () {
                $('#add-service-watersport').remove();
            }, 300);

        }
        $.ajax({
            url: "AddWaterSportService",
            type: "post",
            data: {
                order_id: order_id,
                booking_id: booking_id
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_watersport.Initialization(order_id, booking_id);
                }, 300);

            }
        });
    },
    StopScrollingBody: function () {
        $('body').addClass('stop-scrolling');
    },
    StartScrollingBody: function () {
        $('body').removeClass('stop-scrolling');

    },
    DeleteHotel: function (hotel_booking_id) {
        var title = _order_detail_html.confirmbox_delete_service_title.replaceAll('{service}', 'Khách sạn')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "DeleteService",
                type: "post",
                data: { service_type: 1, id: hotel_booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    DeleteFlyBookingDetail: function (order_id, group_booking_id) {
        var title = _order_detail_html.confirmbox_delete_service_title.replaceAll('{service}', 'Vé máy bay')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "DeleteService",
                type: "post",
                data: { service_type: 3, id: group_booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    DeleteTour: function (tour_id) {
        var title = _order_detail_html.confirmbox_delete_service_title.replaceAll('{service}', 'Tour')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "DeleteService",
                type: "post",
                data: { service_type: 5, id: tour_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    DeleteOtherBookingDetail: function (id) {
        var title = _order_detail_html.confirmbox_delete_service_title.replaceAll('{service}', '')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "DeleteService",
                type: "post",
                data: { service_type: 9, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    DeleteVinwondereBookingDetail: function (id) {
        var title = _order_detail_html.confirmbox_delete_service_title.replaceAll('{service}', '')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "DeleteService",
                type: "post",
                data: { service_type: 6, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    DeleteWaterSportBookingDetail: function (id) {
        var title = _order_detail_html.confirmbox_delete_service_title.replaceAll('{service}', '')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "DeleteService",
                type: "post",
                data: { service_type: 9, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    CancelHotel: function (hotel_booking_id) {
        var title = _order_detail_html.confirmbox_cancel_service_title.replaceAll('{service}', 'Khách sạn')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "CancelService",
                type: "post",
                data: { service_type: 1, id: hotel_booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    CancelFlyBookingDetail: function (group_booking_id) {
        var title = _order_detail_html.confirmbox_cancel_service_title.replaceAll('{service}', 'Vé máy bay')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "CancelService",
                type: "post",
                data: { service_type: 3, id: group_booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    CancelTour: function (tour_id) {
        var title = _order_detail_html.confirmbox_cancel_service_title.replaceAll('{service}', 'Tour')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "CancelService",
                type: "post",
                data: { service_type: 5, id: tour_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    CancelOthers: function (id) {
        var title = _order_detail_html.confirmbox_cancel_service_title.replaceAll('{service}', '')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "CancelService",
                type: "post",
                data: { service_type: 9, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    CancelVinwonder: function (id) {
        var title = _order_detail_html.confirmbox_cancel_service_title.replaceAll('{service}', '')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "CancelService",
                type: "post",
                data: { service_type: 6, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
    CancelWaterSport: function (id) {
        var title = _order_detail_html.confirmbox_cancel_service_title.replaceAll('{service}', '')
        var description = _order_detail_html.confirmbox_delete_service_description
        _msgconfirm.openDialog(title, description, function () {
            _global_function.AddLoading()
            $.ajax({
                url: "CancelService",
                type: "post",
                data: { service_type: 9, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _global_function.RemoveLoading()

                }
            })
        })
    },
}


