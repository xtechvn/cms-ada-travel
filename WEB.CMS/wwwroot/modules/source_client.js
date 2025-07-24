$(document).ready(function () {
    _source_client.Init()
 
});
$('#import-file-ws').change(function () {
    $('#file_import_hotel_error_message').addClass('mfp-hide');
    _source_client.UploadFileExcel();
});
var _source_client = {
    Init: function () {

        _source_client.SearchData();
    },
    SearchData: function () {
        var type = "CLIENT_SOURCE";
        _source_client.Search(type)
    },
    Search: function (type) {
        window.scrollTo(0, 0);
        $.ajax({
            url: "/CustomerManagerManual/SearchSource",
            type: "Post",
            data: { type: type },
            success: function (result) {
                $('#grid_data').html(result);
            }
        });
    },
    OpenPopup: function (id) {
        let title = 'Thêm mới nguồn khách hàng';
        if (id != 0) {
            title = 'Cập nhật nguồn khách hàng';
        }
        let url = '/CustomerManagerManual/AddOrUpdateSource';
        let param = {
            id: id,
        };
        _magnific.OpenSmallPopup(title, url, param);

    },
    OpenPopupDetail: function (id) {
        let title = 'Danh sách khách hàng';
 
        let url = '/CustomerManagerManual/ListClientSource';
        let param = {
            Id: id
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
                    _source_client.SearchData()
                } else {
                    _msgalert.error(result.msg);
                }

            }
        });
    },
    Summit: function () {
        let FromCreate = $('#_source_client_Detail');
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
                        _source_client.SearchData()
                    } else {
                        _msgalert.error(result.msg);
                    }

                }
            });
        }
    },
   
    Export: function () {
        var fieldsV2 = {
            STT: true,
            MaKH: true,
            TenKH: true,
            LienHe: true,
            DoiTuong: true,
            LoaiKH: true,
            NhomKH: true,
            TongTT: true,
            VNPhuTrach: true,
            NgayTao: false,
            NgayCN: false,
            NgayDuyet: false,
            NguoiTao: false,
            Status: true,
        }
        var _searchModel = {
            MaKH: null,
            UserId: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            AgencyType: -1,
            ClientType: -1,
            PermissionType: -1,
            CreateDate: null,
            EndDate: null,
            MinAmount:-1,
            MaxAmount: -1,
            PageIndex: 1,
            PageSize: $("#countClient").val(),
            UtmSource: $('#UtmSource').val(),
        };
    
        var objSearch = this.SearchParam;
        objSearch = _searchModel;
        $('#btnExport').prop('disabled', true);



        _global_function.AddLoading()
        $.ajax({
            url: "/CustomerManager/ExportExcel",
            type: "Post",
            data: { searchModel: _searchModel, field: fieldsV2 },
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
    ImportWSExcel: function () {
        let title = 'Tải lên danh sách khách hàng';
        let url = '/CustomerManagerManual/ImportWSExcel';
        let param = {};
        _magnific.OpenSmallPopup(title, url, param);
    },
    UploadFileExcel: function () {
        let url = '/CustomerManagerManual/ImportWSExcelListing';
        let file = document.getElementById("import-file-ws").files[0];

        var file_type = file['type'];
        if (file_type !== "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
            $('#import-ws-error').removeClass('mfp-hide');
            $('#grid_ws').html('');
            return;
        }
        $('#import-ws-error').hide()

        let formData = new FormData();
        formData.append("file", file)
        _global_function.AddLoading()

        $.ajax({
            url: url,
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                _global_function.RemoveLoading()

                if (result != null) {
                    $('#grid_ws').html(result);
                    $('#confirm-ws-import').show()
                } else {
                    $('#import-ws-error').removeClass('mfp-hide');
                }
            }, error: function (error) {
                console.log(error);
                $('#import-ws-error').removeClass('mfp-hide');
            }
        });
    },
    ConfirmImport: function () {
        let url = '/CustomerManagerManual/ImportWSExcelUpload';
        let data = [];
        if ($('#Userid').val() == undefined || $('#Userid').val() == null) {
            _msgalert.error("Chưa chọn trưởng nhóm");
            return false;
        }
        $('#import-ws-error').hide()
        $('#confirm-ws-import').hide()
        $('#grid_ws tbody tr').each(function () {
            let seft = $(this);
            data.push({
                Client_name: seft.find('.ClientName').text(),
                email: seft.find('.Email').text(),
                phone: seft.find('.Phone').text(),
                Note: seft.find('.Note').text(),
                Status: 0,
                id_loaikhach: 1,
                id_nhomkhach: 0,
                id_ClientType: 5,
                AgencyType: 1,
                UtmSource: $('#UtmSource').val(),
                DepartmentId: $('#DepartmentId').val(),
                UserId: $('#Userid').val(),
            });
        });
       
        _global_function.AddLoading()
        $.ajax({
            url: url,
            type: "POST",
            data: { model: JSON.stringify(data) },
            success: function (result) {
                _global_function.RemoveLoading()
                $('#data_upload_ex').html(result)
            }
        });
    },

}
var _detail_source_client = {
    Summit: function () {
        let FromCreate = $('#_source_client_Detail');
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
                        _source_client.SearchData()
                    } else {
                        _msgalert.error(result.msg);
                    }

                }
            });
        }
    },
}