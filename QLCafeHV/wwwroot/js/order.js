function openOnlineOrders() {
    $("#modalContentOnline").load("/Employee/Home/OnlineOrders", function () {
        var modal = new bootstrap.Modal(document.getElementById('orderOnlineModal'));
        modal.show();
    });
}

function openOfflineOrders() {
    $("#modalContentOffline").load("/Employee/Home/OfflineOrders", function () {
        var modal = new bootstrap.Modal(document.getElementById('orderOfflineModal'));
        modal.show();
    });
}
let reloadInterval;

function openOnlineOrders() {
    $("#modalContent").load("/Employee/Home/OnlineOrders", function () {

        var modalEl = document.getElementById('orderModal');
        var modal = new bootstrap.Modal(modalEl);
        modal.show();

        // auto reload
        reloadInterval = setInterval(function () {
            $("#modalContent").load("/Employee/Home/OnlineOrders");
        }, 5000);

        // khi đóng modal thì clear
        modalEl.addEventListener('hidden.bs.modal', function () {
            clearInterval(reloadInterval);
        });
    });
}