var fundDetailPage = {
    currentStartDate: moment(),
    currentEndDate: moment().add(6, 'days'),
    fundId: parseInt($('#fund-id').val()),

    init: function () {
        this.updateNavTitle();
        this.bindEvents();
        
        // Ghi đè hàm Search của hotelRoomFund để refresh trang chi tiết thay vì quay về index
        if (typeof hotelRoomFund !== 'undefined') {
            hotelRoomFund.Search = function() {
                fundDetailPage.loadData();
            };
        }

        // Auto-load reservations for this fund
        if (typeof userReserveHotelRoomFund !== 'undefined') {
            var hotelId = $('#detail-hotel-id').val();
            var supplierId = $('#detail-supplier-id').val();
            
            // We need to pass these to Search to filter the results
            userReserveHotelRoomFund.Search({ HotelId: hotelId, SupplierId: supplierId });
        }
    },

    bindEvents: function () {
        var self = this;
        $('#btn-prev-week').click(function () {
            var diff = self.currentEndDate.diff(self.currentStartDate, 'days') + 1;
            self.currentStartDate = self.currentStartDate.clone().subtract(diff, 'days');
            self.currentEndDate = self.currentEndDate.clone().subtract(diff, 'days');
            self.loadData();
        });
        $('#btn-next-week').click(function () {
            var diff = self.currentEndDate.diff(self.currentStartDate, 'days') + 1;
            self.currentStartDate = self.currentStartDate.clone().add(diff, 'days');
            self.currentEndDate = self.currentEndDate.clone().add(diff, 'days');
            self.loadData();
        });
        $('#btn-today').click(function () {
            self.currentStartDate = moment();
            self.currentEndDate = moment().add(6, 'days');
            self.loadData();
        });

        // Chọn khoảng ngày linh động
        $('#datepicker-nav').daterangepicker({
            autoApply: true,
            showDropdowns: true,
            startDate: self.currentStartDate,
            endDate: self.currentEndDate,
            locale: {
                format: 'DD/MM/YYYY',
                separator: ' - ',
                applyLabel: "Áp dụng",
                cancelLabel: "Hủy",
                daysOfWeek: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
                monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"]
            }
        }, function (start, end) {
            self.currentStartDate = start;
            self.currentEndDate = end;
            self.loadData();
        });
    },

    updateNavTitle: function () {
        var title = this.currentStartDate.format('DD/MM/YYYY') + ' - ' + this.currentEndDate.format('DD/MM/YYYY');
        $('#datepicker-nav').val(title);
    },

    loadData: function () {
        var self = this;
        var startStr = this.currentStartDate.format('DD/MM/YYYY');
        var endStr = this.currentEndDate.format('DD/MM/YYYY');
        self.updateNavTitle();

        _ajax_caller.post('/HotelRoomFund/GetDetailData', {
            id: self.fundId,
            startDateStr: startStr,
            endDateStr: endStr
        }, function (result) {
            if (result.status === 0) {
                self.renderGrid(result.dates, result.categories);
            } else {
                _msgalert.error(result.msg || 'Có lỗi xảy ra');
            }
        });
    },

    renderGrid: function (dates, categories) {
        var viDays = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];

        var html = '<table class="room-calendar-table">';
        html += '<thead><tr><th class="th-room">HẠNG PHÒNG</th>';

        for (var i = 0; i < dates.length; i++) {
            var d = dates[i];
            var dateParts = d.date.split('/');
            var dateObj = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
            var dayLabel = viDays[dateObj.getDay()];
            var todayClass = d.isToday ? 'is-today' : '';
            html += '<th class="' + todayClass + '">' + dayLabel;
            html += '<span class="day-number">' + (d.day < 10 ?  d.day : d.day) + '</span>';
            if (d.isToday) {
                html += '<span class="today-badge">HÔM NAY</span>';
            }
            html += '</th>';
        }
        html += '</tr></thead><tbody>';

        if (categories.length === 0) {
            html += '<tr><td colspan="' + (dates.length + 1) + '">';
            html += '<div class="empty-state"><i class="fa fa-calendar-o"></i>';
            html += '<p>Chưa có dữ liệu quỹ phòng cho khoảng thời gian này</p></div>';
            html += '</td></tr>';
        } else {
            for (var c = 0; c < categories.length; c++) {
                var cat = categories[c];
                html += '<tr><td class="td-room">';
                html += '<div style="display:flex;align-items:center;">';
                html += '<span class="room-icon"></span>';
                html += '<div><div class="room-name">' + cat.roomName + '</div>';
                html += '<div class="room-capacity">Capacity: ' + cat.totalCapacity + ' phòng</div></div>';
                html += '</div></td>';

                for (var j = 0; j < cat.dailyData.length; j++) {
                    var dd = cat.dailyData[j];
                    var pct = dd.allocated > 0 ? Math.round((dd.booked / dd.allocated) * 100) : 0;
                    var level = 'level-low';
                    if (pct >= 100) level = 'level-full';
                    else if (pct >= 80) level = 'level-high';
                    else if (pct >= 50) level = 'level-medium';
                    var todayCell = dd.isToday ? 'is-today-cell' : '';
                    var bookedStr = dd.booked < 10 ?  Math.floor(dd.booked) : Math.floor(dd.booked);
                    var allocatedStr = Math.floor(dd.allocated);

                    html += '<td><div class="alloc-cell ' + level + ' ' + todayCell + '" ';
                    html += 'title="Đã đặt: ' + dd.booked + ' / Quỹ: ' + dd.allocated + ' (' + pct + '%)">';
                    html += bookedStr + '/' + allocatedStr;
                    html += '</div></td>';
                }
                html += '</tr>';
            }
        }

        html += '</tbody></table>';
        $('#calendar-grid-container').html(html);
    },

    Edit: function() {
        if (typeof hotelRoomFund !== 'undefined') {
            hotelRoomFund.AddOrUpdate(this.fundId);
        } else {
            _msgalert.error('Không tìm thấy module hotelRoomFund');
        }
    },

    Reserve: function() {
        if (typeof userReserveHotelRoomFund !== 'undefined') {
            var hotelId = $('#detail-hotel-id').val();
            var supplierId = $('#detail-supplier-id').val();
            userReserveHotelRoomFund.AddOrUpdate(0, hotelId, supplierId);
        } else {
            _msgalert.error('Không tìm thấy module userReserveHotelRoomFund');
        }
    },

    ReserveRoom: function(hotelRoomId) {
        if (typeof userReserveHotelRoomFund !== 'undefined') {
            var hotelId = $('#detail-hotel-id').val();
            var supplierId = $('#detail-supplier-id').val();
            userReserveHotelRoomFund.AddOrUpdate(0, hotelId, supplierId, hotelRoomId);
        } else {
            _msgalert.error('Không tìm thấy module userReserveHotelRoomFund');
        }
    }
};

$(document).ready(function () {
    fundDetailPage.init();
});
