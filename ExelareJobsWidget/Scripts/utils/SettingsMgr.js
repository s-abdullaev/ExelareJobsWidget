define(['jquery'], function ($) {
    var urlExists = function (url, callback1, callback2) {
        $.ajax({
            type: 'HEAD',
            url: url,
            crossDomain: true,
            success: function () {
                callback1();
            },
            error: function () {
                callback2();
            }
        });
    }


    return function (args, callback) {
        var setts = {};
        require(["http://cbiz-srv2.network80.com/jobsWidget/settings.js"], function (glb_sets) {
            console.log(glb_sets);

            $.extend(setts, glb_sets);

            setts.curCompanyId = args.companyId;
            setts.curUserId = args.userId;

            //dynamic settings
            var settingsPath = "http://cbiz-srv2.network80.com/jobsWidget/CustomSettings/" + args.companyId + "/settings.js"

//            urlExists(settingsPath, function () {
                require([settingsPath], function (loc_sets) {
                    $.extend(setts, loc_sets);
                    callback(setts);
                });
  //          }, function () {
  //              callback(setts);
 //           });
        });
    }
})






