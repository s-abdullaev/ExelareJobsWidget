define(['jquery'], function ($) {
    return {
        getService: function (svcName, args, callback, method) {
            var self = this;
            if (!method) method = "POST";

            $.ajax({
                type: method,
                url: ExelareSettings.serviceUrl + svcName,
                data: args
            }).done(function (msg) {
                console.log(msg);
                if (msg.ErrorMsg) {
                    self.displayLoader(false);
                    alert(msg.ErrorMsg);
                } else {
                    callback(msg);
                }
            }).fail(function (resp) {
                alert(resp.ErrorMsg);
                console.log(resp);
            });
        },
        displayLoader: function (c) {
            console.log($("#loadingModal"));
            console.log(c);
            if (c) {
                $("#loadingModal").modal('show');
            } else {
                $("#loadingModal").modal('hide');
            }
        }
    }
});



