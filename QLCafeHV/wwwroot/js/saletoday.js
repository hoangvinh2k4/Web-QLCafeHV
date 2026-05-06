$(document).ready(function () {

    $('#btnTodaySales').click(function () {

        $.get('/Employee/Order/GetShiftSummary', function (res) {

            if (res.success) {

                $('#openAmount').text(res.openAmount.toLocaleString('vi-VN'));
                $('#totalSales').text(res.totalSales.toLocaleString('vi-VN'));
                $('#expectedAmount').text(res.expected.toLocaleString('vi-VN'));

                $('#actualAmount').val('');
                $('#difference').val('');

                $('#actualAmount').data('expected', res.expected);

                var modal = new bootstrap.Modal(document.getElementById('todaySalesModal'));
                modal.show();
            }
            else {
                Swal.fire('Thông báo', res.message, 'warning');
            }
        });

    });

    $('#actualAmount').on('input', function () {

        let value = $(this).val().replace(/\D/g, '');

        let formatted = Number(value).toLocaleString('vi-VN');

        $(this).val(formatted);

        let expected = $(this).data('expected') || 0;
        let actual = parseFloat(value) || 0;

        let diff = actual - expected;

        $('#difference').val(diff.toLocaleString('vi-VN'));

        if (diff < 0) {
            $('#difference').css('color', 'red');
        }
        else if (diff > 0) {
            $('#difference').css('color', 'green');
        }
        else {
            $('#difference').css('color', 'black');
        }
    });

});

