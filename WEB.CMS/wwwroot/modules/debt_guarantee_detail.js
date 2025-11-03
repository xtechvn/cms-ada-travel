$(document).ready(function () {

    $("body").on('click', ".tn-duyet", function (ev, picker) {
        var title = 'Xác nhận bảo lãnh';
        _msgconfirm.openDialog(title, 'Xác nhận thay đổi trạng thái không?', function () {
            _debt_guarantee_detail.updatestatus(2)
        })
        
    });
    $("body").on('click', ".tp-duyet", function (ev, picker) {
        var title = 'Xác nhận bảo lãnh';
        _msgconfirm.openDialog(title, 'Xác nhận thay đổi trạng thái không?', function () {
            _debt_guarantee_detail.updatestatus(3)
        })
     
    });
    $("body").on('click', ".tn-tuchoi-duyet", function (ev, picker) {
        var title = 'Xác nhận từ chối bảo lãnh';
        _msgconfirm.openDialog(title, 'Xác nhận thay đổi trạng thái không?', function () {
            _debt_guarantee_detail.updatestatus(5)
        })
        
    });
    $("body").on('click', ".tp-tuchoi-duyet", function (ev, picker) {
        var title = 'Xác nhận từ chối bảo lãnh';
        _msgconfirm.openDialog(title, 'Xác nhận thay đổi trạng thái không?', function () {
            _debt_guarantee_detail.updatestatus(5)
        })

    });
 

});

var _debt_guarantee_detail = {
    updatestatus: function (status) {
        _global_function.AddLoading()
        var id = $('#Id').val();
        $.ajax({
            url: "/DebtGuarantee/UpdateStatus",
            type: "post",
            data: { id: id, status: status },
            success: function (result) {
                _global_function.RemoveLoading()
                if (result.status === 0) {
                    _msgalert.success(result.smg);
                    setTimeout(function () {
                        window.location.reload();
                    }, 300);
                }
                else {
                    _msgalert.error(result.smg);

                }

            },
        });
    },
}