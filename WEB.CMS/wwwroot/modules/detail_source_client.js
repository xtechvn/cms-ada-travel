$(document).ready(function () {
    _detail_source_client.Init()
});
$('#import-file-ws').change(function () {
    $('#file_import_hotel_error_message').addClass('mfp-hide');
    _detail_source_client.UploadFileExcel();
});
var _detail_source_client = {
    Init: function () {

        _detail_source_client.SearchClient();
    },
    SearchClient: function () {
        var input = {
            MaKH: null,
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
            UtmSource: $('#UtmSource').val(),
        }
            ;
        $.ajax({
            url: "/CustomerManagerManual/ListClientBySource",
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
                    }
                    else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");

            }
        });

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
            MinAmount: -1,
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
    ImportWSExcel: function (id) {
        let title = 'Tải lên danh sách khách hàng';
        let url = '/CustomerManagerManual/ImportWSExcel';
        let param = { type :id};
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
        if ($('#Userid').select2("val") == undefined || $('#Userid').select2("val") == null || $('#Userid').select2("val") == '') {
            _msgalert.error('Vui lòng chọn trưởng nhóm');
            return false;
        }
        let url = '/CustomerManagerManual/ImportWSExcelUpload';
        let data = [];
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
                UtmSource: $('#Client_Source').val(),
                DepartmentId: $('#DepartmentId').val(),
                UserId: $('#Userid').select2("val"),
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
    loadkh: function () {
        window.location.reload();
    },
    loaduser: function () {
        $('#Userid').html('')
        $("#Userid").select2({
            theme: 'bootstrap4',
            placeholder: "Trưởng nhóm",
            ajax: {
                url: "/CustomerManagerManual/GetListUserByDepartmentId",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        DepartmentId: $('#DepartmentId').val(),
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.fullName + ' - ' + item.userName,
                                id: item.id,
                            }
                        })
                    };
                },
                
            }
        });
    },
    ClientOnPaging: function (value) {
        var input = {
            MaKH: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            AgencyType: null,
            ClientType: null,
            PermissionType: null,
            PageIndex: value,
            PageSize: 10,
            MinAmount: null,
            MaxAmount: null,
            UtmSource: $('#UtmSource').val(),
        }
            ;
        _global_function.AddLoading()
        $.ajax({
            url: "/CustomerManagerManual/ListClientBySource",
            type: "Post",
            data: input,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#imgLoading_client').hide();
                $('#grid_data_client').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    }
                    else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");

            }
        });
    },
    onSelectPageSize: function (value) {

        var input = {
            MaKH: null,
            CreatedBy: null,
            TenKH: null,
            Email: null,
            Phone: null,
            AgencyType: null,
            ClientType: null,
            PermissionType: null,
            PageIndex: 1,
            PageSize: value,
            MinAmount: null,
            MaxAmount: null,
            UtmSource: $('#UtmSource').val(),
        }
            ;
        _global_function.AddLoading()
        $.ajax({
            url: "/CustomerManagerManual/ListClientBySource",
            type: "Post",
            data: input,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#imgLoading_client').hide();
                $('#grid_data_client').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    }
                    else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");

            }
        });

    },
}