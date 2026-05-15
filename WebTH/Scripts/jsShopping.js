$(document).ready(function () {
    ShowCount();

    function getCsrfToken() {
        return $('#ajaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
    }

    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;
        var tQuantity = $('#quantity_value').text();
        if (tQuantity != '') {
            quantity = parseInt(tQuantity);
        }

        $.ajax({
            url: '/shoppingcart/addtocart',
            type: 'POST',
            data: { id: id, quantity: quantity, __RequestVerificationToken: getCsrfToken() },
            success: function (rs) {
                if (rs.Success) {
                    $('#checkout_items').html(rs.Count);
                    showCustomAlert(rs.msg, true);
                }
                else {
                    showCustomAlert(rs.msg, false);
                }
            }
        });
    });

    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var quantity = $('#Quantity_' + id).val();
        Update(id, quantity);
    });

    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        var conf = confirm('Bạn có chắc muốn xóa hết sản phẩm trong giỏ hàng không?');
        if (conf == true) {
            DeleteAll();
        }
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa này khỏi giỏ hàng không?');
        if (conf == true) {
            $.ajax({
                url: '/shoppingcart/Delete',
                type: 'POST',
                data: { id: id, __RequestVerificationToken: getCsrfToken() },
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        $('#trow_' + id).remove();
                        LoadCart();
                    }
                }
            });
        }
    });
});


function ShowCount() {
    $.ajax({
        url: '/shoppingcart/ShowCount',
        type: 'GET',
        success: function (rs) {
            $('#checkout_items').html(rs.Count);
        }
    });
}

function DeleteAll() {
    var token = $('#ajaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
    $.ajax({
        url: '/shoppingcart/DeleteAll',
        type: 'POST',
        data: { __RequestVerificationToken: token },
        success: function (rs) {
            if (rs.Success) {
                LoadCart();
            }
        }
    });
}

//function Update(id, quantity) {
//    var token = $('#ajaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
//    $.ajax({
//        url: '/shoppingcart/Update',
//        type: 'POST',
//        data: { id: id, quantity: quantity, __RequestVerificationToken: token },
//        success: function (rs) {
//            if (rs.Success) {
//                LoadCart();
//            }
//        }
//    });
//}

function Update(id, quantity) {
    // Tìm token bảo mật an toàn hơn
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/shoppingcart/Update',
        type: 'POST',
        data: { id: id, quantity: quantity, __RequestVerificationToken: token },
        success: function (rs) {
            if (rs.Success) {
                // Nếu thành công -> Tải lại bảng giỏ hàng ngầm và báo thành công
                LoadCart();
                showCustomAlert("Cập nhật số lượng thành công!", true);
            } else {
                // Nếu thất bại (VD: Bạn nhập 10 nhưng kho chỉ còn 5) -> Báo lỗi
                showCustomAlert(rs.msg, false);

                // Khôi phục lại số lượng cũ trên ô input để khách khỏi bỡ ngỡ
                LoadCart();
            }
        },
        error: function () {
            showCustomAlert("Có lỗi xảy ra khi kết nối đến máy chủ!", false);
        }
    });
}

function LoadCart() {
    $.ajax({
        url: '/shoppingcart/Partial_Item_Cart',
        type: 'GET',
        success: function (rs) {
            $('#load_data').html(rs);
        }
    });
}

function showCustomAlert(message, isSuccess) {
    // Create modal HTML
    var modalHtml = '<div class="custom-alert-modal" style="display:none; position:fixed; top:0; left:0; right:0; bottom:0; background:rgba(0,0,0,0.5); z-index:9999; display:flex; align-items:center; justify-content:center;">' +
        '<div style="background:white; border-radius:12px; padding:30px; max-width:400px; box-shadow:0 10px 40px rgba(0,0,0,0.2); animation:slideInUp 0.3s ease;">' +
        '<div style="text-align:center; margin-bottom:20px;">' +
        (isSuccess ? '<i class="fa fa-check-circle" style="font-size:50px; color:#4CAF50; margin-bottom:15px;"></i>' : '<i class="fa fa-exclamation-circle" style="font-size:50px; color:#ff9800; margin-bottom:15px;"></i>') +
        '</div>' +
        '<h3 style="text-align:center; color:#333; margin-bottom:15px; font-size:18px; font-weight:600;">' + (isSuccess ? 'Thành công' : 'Thông báo') + '</h3>' +
        '<p style="text-align:center; color:#666; margin-bottom:25px; font-size:14px; line-height:1.5;">' + message + '</p>' +
        '<div style="text-align:center;">' +
        '<button class="custom-alert-btn" style="background:linear-gradient(135deg, #fe4c50 0%, #ff6c70 100%); color:white; border:none; padding:12px 30px; border-radius:8px; font-size:14px; font-weight:600; cursor:pointer; transition:all 0.3s ease; box-shadow:0 4px 12px rgba(254, 76, 80, 0.3);">OK</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '<style>' +
        '@keyframes slideInUp { ' +
        'from { transform: translateY(30px); opacity: 0; } ' +
        'to { transform: translateY(0); opacity: 1; } ' +
        '}' +
        '.custom-alert-btn:hover { ' +
        'transform: translateY(-2px); ' +
        'box-shadow: 0 6px 16px rgba(254, 76, 80, 0.4); ' +
        'background: linear-gradient(135deg, #ff6c70 0%, #ff8c90 100%) !important; ' +
        '} ' +
        '.custom-alert-btn:active { ' +
        'transform: translateY(0); ' +
        'box-shadow: 0 2px 8px rgba(254, 76, 80, 0.3); ' +
        '}' +
        '</style>';

    // Remove existing modal if any
    $('.custom-alert-modal').remove();

    // Add to body
    $('body').append(modalHtml);

    // Show modal
    $('.custom-alert-modal').show();

    // Close on button click
    $('.custom-alert-btn').on('click', function () {
        $('.custom-alert-modal').fadeOut(200, function () {
            $(this).remove();
        });
    });

    // Close on outside click
    $('.custom-alert-modal').on('click', function (e) {
        if (e.target === this) {
            $(this).fadeOut(200, function () {
                $(this).remove();
            });
        }
    });

    // Close on ESC key
    $(document).on('keydown.customAlert', function (e) {
        if (e.keyCode === 27) {
            $('.custom-alert-modal').fadeOut(200, function () {
                $(this).remove();
            });
            $(document).off('keydown.customAlert');
        }
    });
}
