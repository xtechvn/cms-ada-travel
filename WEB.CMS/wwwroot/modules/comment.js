$(document).ready(function () {
    const requestId = $("#comment-section").data("request-id");
    if (requestId) {
        loadComments(requestId);
        startSSE(requestId)
    }
});

// Hàm tải danh sách comment
function loadComments(requestId) {
    $.ajax({
        url: "/RequestHotelBooking/LoadComments",
        type: "POST",
        data: { requestId },
        success: function (result) {
            if (result.status === 1 && result.data.length > 0) {
                $(".comment-list-wrapper").empty(); // Xóa comment cũ
                result.data.forEach(renderComment); // Render từng comment
            }
        },
        error: function () {
            _msgalert.error(result.msg);;
        }
    });
}
// SSE - Nhận comment mới từ server
let lastAddedCommentId = null; // Biến lưu ID của comment vừa thêm
function startSSE(requestId) {

    const eventSource = new EventSource(`/RequestHotelBooking/GetCommentsStream?requestId=${requestId}`);

    eventSource.onmessage = function (event) {
        const newComment = JSON.parse(event.data);
        renderComment(newComment); // Hiển thị comment mới ngay lập tức
    };

    eventSource.onerror = function () {
        console.error("SSE connection error.");
        eventSource.close();
    };
}

function renderComment(comment) {
    debugger
    var commentId = comment.Id || comment.id; // Lấy Id của comment
    // Kiểm tra xem comment này đã tồn tại trong DOM chưa
    if ($(`#comment-${commentId}`).length > 0) {
        return; // Nếu đã tồn tại thì không render lại
    }

    var username = comment.Username || comment.UserName || comment.userName|| "Unknown User";
    var avatarInitial = username.charAt(0).toUpperCase();

    // Chuyển đổi thời gian theo múi giờ Việt Nam
    var createdDate = new Date(comment.createdDate);
    var formattedDate = createdDate.toLocaleString('vi-VN', {
        hour: '2-digit',
        minute: '2-digit',
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour12: false,
        timeZone: 'Asia/Ho_Chi_Minh' // Chỉnh theo múi giờ
    });

    var html = `
        <div id="comment-${commentId}" class="info-basic flex flex-nowrap w-100">
            <div class="ava">
                <span class="thumb_img thumb_5x5">
                    <div class="initial-avatar">${avatarInitial}</div>
                </span>
            </div>
            <div class="content w-100">
                <div class="flex justify-content-between">
                    <p><b>${username}</b></p>
                    <span style="color:#698096">${formattedDate}<b class="ml-2">Phản hồi</b></span>
                </div>
                <div>${comment.Content || comment.content}</div>
                ${renderAttachments(comment.AttachFiles || comment.attachFiles)}
            </div>
        </div>
        <div class="line-bottom mt16 mb16"></div>
    `;

    $(".comment-list-wrapper").append(html);
}


function renderAttachments(files) {
    if (!files || files.length === 0) return "";
    return `
        <div class="attachments mt10">
            ${files.map(file => renderFileLink(file)).join("")}
        </div>
    `;
}

function renderFileLink(file) {
    // Trích xuất phần mở rộng của file từ tên file
    const fileExtension = file.Name||file.name.split('.').pop().toUpperCase();
    return `
        <div class="file-preview">
            <a href="${file.Url||file.url}" class="attachment-file" target="_blank">
                <div class="file-info">
                    <div class="file-type">${fileExtension}</div>
                    <div>
                        <span class="file-name">${file.Name || file.name}</span>
                       
                    </div>
                </div>
                <div class="file-icon-download">
                    <i class="icon-download"></i>
                </div>
            </a>
        </div>
    `;
}
$('#attachFiles').change(function (event) {
    
    var _validFileExtensions = ["jpg", "jpeg", "bmp", "gif", "png", "pdf", "doc", "docx", "txt", "xls", "xlsx"];

    if (event.target.files && event.target.files[0]) {
        var fileType = event.target.files[0].name.split('.').pop();

        if (event.target.files[0].size > (1024 * 1024)) {
            _msgalert.error('File upload hiện tại có kích thước (' + Math.round(event.target.files[0].size / 1024 / 1024, 2) + ' Mb) vượt quá 1 Mb. Bạn hãy chọn lại ảnh khác');
            $(this).val('');
        }

        if (!_validFileExtensions.includes(fileType)) {
            _msgalert.error('File upload phải thuộc các định dạng : jpg, jpeg, bmp, gif, png ,pdf,doc,docx,txt,xls,xlsx');
            $(this).val('');
        }

        if (_validFileExtensions.includes(fileType) && event.target.files[0].size <= (1024 * 1024)) {
            $('.wrap-croppie').show();
            $('.wrap-image-preview').hide();
            $('#btn-cropimage').show();
            $('#btn-cancel-crop').show();
            var reader = new FileReader();
            reader.onload = function (e) {
                uploadCrop.croppie('bind', {
                    url: e.target.result
                });
            };
            reader.readAsDataURL(event.target.files[0]);
        }
    }
})

// Xử lý gửi comment
function addComment(requestId) {
    var formData = new FormData();
    formData.append("requestId", requestId);
    formData.append("content", $("#commentContent").val());

    var files = $("#attachFiles")[0].files;
    for (var i = 0; i < files.length; i++) {
        formData.append("attachFiles", files[i]);
    }

    $.ajax({
        url: "/RequestHotelBooking/AddComment",
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (result) {
            debugger
            if (result.status === 0) {
                _msgalert.success(result.msg);
               // renderComment(result.data); // Thêm comment xuống dưới
                //lastAddedCommentId = result.data.Id; // Ghi nhớ ID comment vừa thêm
                resetForm();
            } else {
                _msgalert.error(result.msg);
            }
        },
        error: function () {
            _msgalert.error(result.msg);
        }
    });
}

function resetForm() {
    $("#commentContent").val("");
    $("#attachFiles").val("");
    $("#attachmentPreviews").html("");
}
// Hàm hiển thị preview file đính kèm trước khi upload
function previewAttachments() {
    const previewContainer = document.getElementById('attachmentPreviews');
    const files = document.getElementById('attachFiles').files;
    const VALID_EXTENSIONS = ["jpg", "jpeg", "bmp", "gif", "png", "pdf", "doc", "docx", "txt", "xls", "xlsx"];
    const MAX_FILE_SIZE = 25 * 1024 * 1024; // 25MB

    try {
        // Reset preview container
        previewContainer.innerHTML = "";

        // Validate tổng dung lượng files
        let totalSize = 0;
        Array.from(files).forEach(file => {
            totalSize += file.size;
        });

        if (totalSize > MAX_FILE_SIZE) {
            _msgalert.error(`Tổng dung lượng file vượt quá 25MB`);
            document.getElementById('attachFiles').value = '';
            document.getElementById('attachmentPreviews').style.display = 'none';
        }

        // Preview từng file
        Array.from(files).forEach(file => {
            // Validate định dạng file
            const fileType = file.name.split('.').pop().toLowerCase();
            if (!VALID_EXTENSIONS.includes(fileType)) {
                _msgalert.error(`File "${file.name}" không đúng định dạng. Chỉ chấp nhận: ${VALID_EXTENSIONS.join(', ')}`);
                document.getElementById('attachFiles').value = '';
                document.getElementById('attachmentPreviews').style.display = 'none';



            }

            const reader = new FileReader();
            reader.onload = function (e) {
                if (file.type.startsWith('image/')) {
                    previewContainer.innerHTML += `
                       <img src="${e.target.result}" 
                            style="max-width: 100px; max-height: 100px; margin: 5px;" 
                            alt="${file.name}" />`;
                } else {
                    previewContainer.innerHTML += `
                       <div><strong>${file.name}</strong> (${(file.size / 1024).toFixed(2)} KB)</div>`;
                }
            };
            reader.readAsDataURL(file);
        });

    } catch (error) {
        // Reset input và preview nếu có lỗi
        document.getElementById('attachFiles').value = '';
        previewContainer.innerHTML = '';
        alert(error.message);
    }
}

