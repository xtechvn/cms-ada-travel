$(document).ready(function () {
    all_code.Init()
    $('#Type').select2()
});
var all_code = {
    Init: function () {

        all_code.SearchData();
    },
    SearchData: function () {
        var type = $('#Type').val();
            all_code.Search(type)
    },
    Search: function (type) {
        window.scrollTo(0, 0);
        $.ajax({
            url: "/AllCode/Search",
            type: "Post",
            data: { type: type},
            success: function (result) {
                $('#grid_data').html(result);
            }
        });
    },
    OpenPopup: function (id) {
        let title = 'Thêm mới AllCode';
        if (id != 0) {
            title = 'Cập nhật AllCode';
        }
        let url = '/AllCode/AddOrUpdate';
        let param = {
            id: id,
        };
        _magnific.OpenSmallPopup(title, url, param);

    },
    OnDelete: function (id) {
        _global_function.AddLoading()
        $.ajax({
            url: '/AllCode/Delete',
            type: "post",
            data: { id: id },
            success: function (result) {
                _global_function.RemoveLoading()
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    all_code.SearchData()
                } else {
                    _msgalert.error(result.msg);
                }

            }
        });
    },
    Summit: function () {
        let FromCreate = $('#All_Code_Detail');
        FromCreate.validate({
            rules: {

                "Type": "required",
                "CodeValue": "required",
                "Description": "required",
            },
            messages: {
                "Type": "Type không được bỏ trống",
                "CodeValue": "CodeValue không được bỏ trống",
                "Description": "Description không được bỏ trống",

            }
        });
        if (FromCreate.valid()) {
            var object_summit = {
                Id: $('#id').val(),
                Type: $('#Type').val(),
                CodeValue: $('#CodeValue').val(),
                Description: $('#Description').val(),
            }
            _global_function.AddLoading()
            $.ajax({
                url: '/AllCode/Setup',
                type: "post",
                data: { model: object_summit },
                success: function (result) {
                     _global_function.RemoveLoading()
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                        $.magnificPopup.close()
                        all_code.SearchData()
                    } else {
                        _msgalert.error(result.msg);
                    }
                   
                }
            });
        }
    },
}