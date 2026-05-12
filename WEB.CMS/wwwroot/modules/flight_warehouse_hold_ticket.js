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

    OnPagingHoldTicket: function (pageIndex, bookingId) {
        this.LoadHoldTickets(bookingId, pageIndex);
    },

    CalculateHoldTicketPrice: function () {
        var modal = $('#magnific-popup-small');
        var groupObjectSelect = modal.find('#hold-ticket-group-object');
        var groupObject = groupObjectSelect.val();
        
        if (!groupObject) {
            modal.find('.hold-ticket-price-table-container').hide();
            return;
        }

        modal.find('.hold-ticket-price-table-container').show();

        var totalQty = parseInt(modal.find('#hold-ticket-total-qty').val()) || 0;
        modal.find('#hold-ticket-adt-qty').val(totalQty); // Sync total to adult qty

        var adtQty = totalQty;
        var chdQty = parseInt(modal.find('#hold-ticket-chd-qty').val()) || 0;
        var infQty = parseInt(modal.find('#hold-ticket-inf-qty').val()) || 0;

        var adtAmount = parseFloat(modal.find('.adt-amount-val').val()) || 0;
        var adtSell = parseFloat(modal.find('.adt-sell-val').val()) || 0;
        
        var chdAmount = parseFloat(modal.find('.chd-amount-val').val()) || 0;
        var chdSell = parseFloat(modal.find('.chd-sell-val').val()) || 0;
        
        var infAmount = parseFloat(modal.find('.inf-amount-val').val()) || 0;
        var infSell = parseFloat(modal.find('.inf-sell-val').val()) || 0;

        var totalAmount = (adtAmount * adtQty) + (chdAmount * chdQty) + (infAmount * infQty);
        var totalSell = (adtSell * adtQty) + (chdSell * chdQty) + (infSell * infQty);
        
        // Lợi nhuận = Giá bán - Giá nhập
        var totalProfit = totalSell - totalAmount;

        modal.find('#hold-ticket-total-amount').text(totalSell.toLocaleString('en-US') + ' đ');
        modal.find('#hold-ticket-total-profit').text(totalProfit.toLocaleString('en-US') + ' đ');
        
        if (totalProfit < 0) {
            modal.find('#hold-ticket-total-profit').addClass('text-danger').removeClass('text-success');
        } else {
            modal.find('#hold-ticket-total-profit').addClass('text-success').removeClass('text-danger');
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

        var obj = {
            FlightWarehouseBookingId: bookingId,
            GroupObject: groupObject,
            AdultQuantity: parseInt(modal.find('#hold-ticket-adt-qty').val()) || 0,
            ChildQuantity: parseInt(modal.find('#hold-ticket-chd-qty').val()) || 0,
            InfantQuantity: parseInt(modal.find('#hold-ticket-inf-qty').val()) || 0,
            AdultAmount: parseFloat(modal.find('.adt-amount-val').val()) || 0,
            AdultPrice: parseFloat(modal.find('.adt-sell-val').val().replace(/,/g, '')) || 0,
            ChildAmount: parseFloat(modal.find('.chd-amount-val').val()) || 0,
            ChildPrice: parseFloat(modal.find('.chd-sell-val').val().replace(/,/g, '')) || 0,
            InfantAmount: parseFloat(modal.find('.inf-amount-val').val()) || 0,
            InfantPrice: parseFloat(modal.find('.inf-sell-val').val().replace(/,/g, '')) || 0,
            Status: 0 // 0: Pending, 1: Success, 2: Cancelled
        };

        var totalQty = parseInt(modal.find('#hold-ticket-total-qty').val()) || 0;
        if (totalQty <= 0) {
            _msgalert.error("Vui lòng nhập số lượng vé giữ");
            return;
        }



        $.ajax({
            url: "/FlightWarehouse/UpsertHoldTicket",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(obj),
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    $.magnificPopup.close();
                    _flight_warehouse_hold_ticket.LoadHoldTickets(bookingId);
                } else {
                    _msgalert.error(result.msg);
                }
            }
        });
    }
};
