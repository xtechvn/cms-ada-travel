var _flight_warehouse_hold_ticket = {
    LoadHoldTickets: function (bookingId, pageIndex = 1) {
        var obj = {
            FlightWarehouseBookingId: bookingId,
            pageIndex: pageIndex,
            pageSize: 10
        };
        $.ajax({
            url: "/FlightWarehouse/HoldTicketList",
            type: "POST",
            data: obj,
            success: function (result) {
                $('#grid_hold_ticket').html(result);
            }
        });
    },

    HoldTicketPopup: function (bookingId) {
        let title = 'Yêu cầu giữ vé';
        let url = '/FlightWarehouse/HoldTicketPopup';
        let param = {
            bookingId: bookingId,
        };
        _magnific.OpenSmallPopup(title, url, param);
    },

    AgencyHoldPopup: function (bookingId) {
        let title = 'Đại lý giữ chỗ';
        let url = '/FlightWarehouse/AgencyHoldPopup';
        let param = {
            bookingId: bookingId,
        };
        _magnific.OpenSmallPopup(title, url, param);
    },

    SubmitAgencyHold: function (bookingId) {
        var quantity = $('#agency-hold-quantity').val();
        if (quantity === "" || parseInt(quantity) < 0) {
            _msgalert.error("Số lượng không hợp lệ");
            return;
        }

        $.ajax({
            url: "/FlightWarehouse/UpdateAgencyHold",
            type: "POST",
            data: {
                bookingId: bookingId,
                quantity: quantity
            },
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    $.magnificPopup.close();
                    location.reload();
                } else {
                    _msgalert.error(result.msg);
                }
            }
        });
    },

    OnPagingHoldTicket: function (pageIndex, bookingId) {
        this.LoadHoldTickets(bookingId, pageIndex);
    },

    CalculateHoldTicketPrice: function () {
        var modal = $('#magnific-popup-small');
        var groupObject = modal.find('#hold-ticket-group-object').val();

        if (!groupObject) {
            modal.find('#hold-ticket-price-table-container').hide();
            modal.find('#hold-ticket-summary-container').hide();
            return;
        }

        // Cập nhật giá cơ bản từ dữ liệu ẩn dựa trên đối tượng đã chọn
        var priceSource = modal.find('#price-data-' + groupObject);
        if (priceSource.length > 0) {
            // .xxx-sell-val lưu Giá Bán (Amount)
            // .xxx-net-val lưu Giá Nhập (Price)
            modal.find('.adt-sell-val').val(priceSource.data('adt-sell'));
            modal.find('.adt-net-val').val(priceSource.data('adt-net'));
            modal.find('.adt-sell-text').text(parseFloat(priceSource.data('adt-sell')).toLocaleString('en-US'));

            modal.find('.chd-sell-val').val(priceSource.data('chd-sell'));
            modal.find('.chd-net-val').val(priceSource.data('chd-net'));
            modal.find('.chd-sell-text').text(parseFloat(priceSource.data('chd-sell')).toLocaleString('en-US'));

            modal.find('.inf-sell-val').val(priceSource.data('inf-sell'));
            modal.find('.inf-net-val').val(priceSource.data('inf-net'));
            modal.find('.inf-sell-text').text(parseFloat(priceSource.data('inf-sell')).toLocaleString('en-US'));
        }

        modal.find('#hold-ticket-price-table-container').show();
        modal.find('#hold-ticket-summary-container').show();

        var adtQty = parseInt(modal.find('#hold-ticket-adt-qty').val()) || 0;
        var chdQty = parseInt(modal.find('#hold-ticket-chd-qty').val()) || 0;
        var infQty = parseInt(modal.find('#hold-ticket-inf-qty').val()) || 0;

        var adtSell = parseFloat(modal.find('.adt-sell-val').val()) || 0;
        var chdSell = parseFloat(modal.find('.chd-sell-val').val()) || 0;
        var infSell = parseFloat(modal.find('.inf-sell-val').val()) || 0;

        var adtNet = parseFloat(modal.find('.adt-net-val').val()) || 0;
        var chdNet = parseFloat(modal.find('.chd-net-val').val()) || 0;
        var infNet = parseFloat(modal.find('.inf-net-val').val()) || 0;

        var adtSubtotal = adtQty * adtSell;
        var chdSubtotal = chdQty * chdSell;
        var infSubtotal = infQty * infSell;

        modal.find('.adt-subtotal').text(adtSubtotal.toLocaleString('en-US'));
        modal.find('.chd-subtotal').text(chdSubtotal.toLocaleString('en-US'));
        modal.find('.inf-subtotal').text(infSubtotal.toLocaleString('en-US'));

        var totalPax = adtQty + chdQty + infQty;
        var capacityQty = adtQty + chdQty;

        var totalSell = adtSubtotal + chdSubtotal + infSubtotal;
        var totalNet = (adtQty * adtNet) + (chdQty * chdNet) + (infQty * infNet);
        var totalProfit = totalSell - totalNet;

        modal.find('#summary-total-pax').text(totalPax);
        modal.find('#summary-capacity-qty').text(capacityQty);
        modal.find('#summary-total-net').text(totalNet.toLocaleString('en-US') + ' VND');
        modal.find('#summary-total-sell').text(totalSell.toLocaleString('en-US') + ' VND');

        var profitText = totalProfit.toLocaleString('en-US') + ' VND';
        modal.find('#summary-total-profit').text(profitText);

        if (totalProfit < 0) {
            modal.find('#summary-total-profit').addClass('text-danger').removeClass('text-success');
        } else {
            modal.find('#summary-total-profit').addClass('text-success').removeClass('text-danger');
        }
    },

    SubmitHoldTicket: function () {
        var modal = $('#magnific-popup-small');
        var bookingId = modal.find('#hold-ticket-booking-id').val();
        var groupObject = modal.find('#hold-ticket-group-object').val();

        if (!groupObject) {
            _msgalert.error("Vui lòng chọn đối tượng");
            return;
        }

        var adtQty = parseInt(modal.find('#hold-ticket-adt-qty').val()) || 0;
        var chdQty = parseInt(modal.find('#hold-ticket-chd-qty').val()) || 0;
        var infQty = parseInt(modal.find('#hold-ticket-inf-qty').val()) || 0;

        if (adtQty + chdQty + infQty <= 0) {
            _msgalert.error("Vui lòng nhập số lượng khách");
            return;
        }

        var obj = {
            FlightWarehouseBookingId: parseFloat(bookingId),
            GroupObject: parseFloat(groupObject),
            ExpirationDateStr: modal.find('#hold-ticket-limit-date').val(),
            AdultQuantity: adtQty,
            ChildQuantity: chdQty,
            InfantQuantity: infQty,
            // Theo quy ước: AdultAmount là Giá Bán, AdultPrice là Giá Nhập
            AdultAmount: parseFloat(modal.find('.adt-sell-val').val()) * adtQty || 0,
            AdultPrice: parseFloat(modal.find('.adt-net-val').val()) * adtQty || 0,
            ChildAmount: parseFloat(modal.find('.chd-sell-val').val()) * chdQty || 0,
            ChildPrice: parseFloat(modal.find('.chd-net-val').val()) * chdQty || 0,
            InfantAmount: parseFloat(modal.find('.inf-sell-val').val()) * infQty || 0,
            InfantPrice: parseFloat(modal.find('.inf-net-val').val()) * infQty || 0,
            TotalTicket: adtQty + chdQty,
            Status: 0
        };

        $.ajax({
            url: "/FlightWarehouse/UpsertHoldTicket",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(obj),
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    $.magnificPopup.close();
                    if (typeof _flight_warehouse_hold_ticket.LoadHoldTickets === 'function') {
                        _flight_warehouse_hold_ticket.LoadHoldTickets(bookingId);
                    }
                } else {
                    _msgalert.error(result.msg);
                }
            }
        });
    }
};
