var _profile = {
    Init: function () {
        this.Search();
    },
    Search: function (pageIndex = 1) {
        let obj = {
            name: $('#SearchName').val(),
            status: $('#SearchStatus').val(),
            pageIndex: pageIndex,
            pageSize: 10
        };
        _ajax_caller.get("/Profile/Search", obj, function (result) {
            $('#grid-data').html(result);
        });
    },
    OnPaging: function (value) {
        this.Search(value);
    },
    ShowUpsert: function (id) {
        let title = id > 0 ? "Cập nhật hồ sơ" : "Tạo mới hồ sơ";
        let url = '/Profile/Upsert';
        var param = {
            id: id
        };
        _magnific.OpenLargerPopup(title, url, param);
    },
    InitUpsert: function (id, attachmentType) {
        _global_function.RenderFileAttachment($('.attachment-addnew'), id, attachmentType);

        $('#form-upsert').validate({
            rules: {
                DocumentName: "required",
                Category: "required"
            },
            messages: {
                DocumentName: "Vui lòng nhập tên hồ sơ",
                Category: "Vui lòng chọn danh mục"
            }
        });
    },
    OnSave: function (isSendApproval) {
        if (!$('#form-upsert').valid()) return;

        var files = _global_function.GetAttachmentFiles($('.attachment-addnew'));
        if (files.length == 0 && !$('#FilePath').val()) {
            $('#validate-file').show();
            return;
        }
        $('#validate-file').hide();

        var filePath = files.length > 0 ? files[0].path : $('#FilePath').val();

        var model = {
            Id: $('#Id').val(),
            DocumentName: $('#DocumentName').val(),
            Category: $('#Category').val(),
            Description: $('#Description').val()
        };

        _global_function.AddLoading();
        _ajax_caller.post("/Profile/Save", { model: model, filePdf: filePath, isSendApproval: isSendApproval }, function (result) {
            _global_function.RemoveLoading();
            if (result.status == 0) {
                _msgalert.success(result.msg);
                _global_function.ConfirmFileUpload($('.attachment-addnew'), result.data);
                location.reload();
            } else {
                _msgalert.error(result.msg);
            }
        });
    },
    OnApprove: function (id) {
        let title = "";
        let url = '/Profile/ApprovePopup';
        var param = {
            id: id
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    InitApprovePopup: function (id) {
        var canvas = document.getElementById('signature-pad-popup');
        if (canvas) {
            var signaturePad = new SignaturePad(canvas);

            function resizeCanvas() {
                var ratio = Math.max(window.devicePixelRatio || 1, 1);
                canvas.width = canvas.offsetWidth * ratio;
                canvas.height = canvas.offsetHeight * ratio;
                canvas.getContext("2d").scale(ratio, ratio);
                signaturePad.clear();
            }

            window.addEventListener("resize", resizeCanvas);
            resizeCanvas();

            $('#clear-signature-popup').on('click', function () {
                signaturePad.clear();
            });

            $('#btn-approve-signed-popup').on('click', function () {
                if (signaturePad.isEmpty()) {
                    _msgalert.error("Vui lòng ký tên trước khi duyệt");
                    return;
                }

                var signatureData = signaturePad.toDataURL();
                _profile.ApproveSubmit(id, signatureData);
            });
        }
    },
    ApproveSubmit: function (id, signatureData) {
        _ajax_caller.post("/Profile/Approve", { id: id, signatureBase64: signatureData }, function (result) {
            _global_function.RemoveLoading();
            if (result.status == 0) {
                _msgalert.success(result.msg);
                location.reload();
            } else {
                _msgalert.error(result.msg);
            }
        });
    },
    Reject: function (id) {
        let title = "";
        let url = '/Profile/RejectPopup';
        var param = {
            id: id
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    InitRejectPopup: function (id) {
        $('#btn-reject-submit-popup').on('click', function () {
            let reason = $('#RejectReason').val();
            if (!reason || reason.trim() == "") {
                $('#validate-reason').show();
                return;
            }
            $('#validate-reason').hide();
            _profile.RejectSubmit(id, reason);
        });
    },
    RejectSubmit: function (id, reason) {
        _ajax_caller.post("/Profile/Reject", { id: id, reason: reason }, function (result) {
            _global_function.RemoveLoading();
            if (result.status == 0) {
                _msgalert.success(result.msg);
                location.reload();
            } else {
                _msgalert.error(result.msg);
            }
        });
    },
    Finish: function (id) {
        let title = "Hoàn thành hồ sơ";
        let url = '/Profile/FinishPopup';
        var param = {
            id: id
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    InitFinishPopup: function (id) {
        $('#btn-finish-submit-popup').on('click', function () {
            let physicalLocation = $('#PhysicalStorageLocation').val();
            let digitalLocation = $('#DigitalStorageLocation').val();

            _profile.FinishSubmit(id, physicalLocation, digitalLocation);
        });
    },
    FinishSubmit: function (id, physicalLocation, digitalLocation) {
        _global_function.AddLoading();
        _ajax_caller.post("/Profile/Finish", { id: id, physicalLocation: physicalLocation, digitalLocation: digitalLocation }, function (result) {
            _global_function.RemoveLoading();
            if (result.status == 0) {
                _msgalert.success(result.msg);
                location.reload();
            } else {
                _msgalert.error(result.msg);
            }
        });
    },
    ClosePopup: function () {
        $.magnificPopup.close();
    }
};

$(document).ready(function () {
    _profile.Init();
});
