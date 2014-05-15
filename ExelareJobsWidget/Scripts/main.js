function loadCss(url) {
    var link = document.createElement("link");
    link.type = "text/css";
    link.rel = "stylesheet";
    link.href = url;
    document.getElementsByTagName("head")[0].appendChild(link);
}

//IE console fix
if (!window.console) console = { log: function (obj) { }};

require.config({
    baseUrl: 'http://cbiz-srv2.network80.com/jobsWidget/Scripts',
    paths: {
        "jquery" : 'lib/jquery-2.1.0',
        "bootstrap": 'lib/bootstrap.min',
        "helpers": "utils/helpers",
        'datasource': 'utils/AjaxDataSource',
        'underscore': 'lib/underscore-min',
        'knockout': 'lib/knockout-3.1.0',
        "settingsMgr": "utils/SettingsMgr",
        "jobDetailModel": "models/JobDetailModel",
        "jobListModel": "models/JobListModel",
        "appFormModel": "models/AppFormModel",
        "jquery-bbq": "lib/jquery.ba-bbq.min",
        "jquery-browser-fix": "lib/jquery.browser.fix"
    },
    shim: {
        "bootstrap": {
            deps: ['jquery']
        },
        "jquery-bbq": {
            deps: ["jquery",
                    "jquery-browser-fix"]
        },
        "jquery-browser-fix": {
            deps: ["jquery"]
        }
    }
});

//

define(["jquery", "knockout", "bootstrap", "jquery-bbq", "settingsMgr"],
    function ($, ko, _1,_2, setMgr) {
       var self = this;
    
       $(document).ready(function () {
               self.SettingsMgr = setMgr;

               //initialization:
               var container = $('#exelare_jobs_container');
               var args = {};

               if (window.location.hash) {
                   args = $.deparam.fragment();
                   console.log(args);
               }

               if (!args.companyId) args.companyId = container.attr('data-companyId');
               if (!args.userId) args.userId = container.attr('data-userId');

               self.SettingsMgr(args, function (setts) {
                   console.log(setts);

                   loadCss(setts.mapUrl('Content/bootstrap.css'));
                   loadCss(setts.mapUrl('Content/styles.css'));

                   self.ExelareSettings = setts;

                   require(["jobDetailModel", "jobListModel", "appFormModel", "helpers"], function (jobDetailModel, jobListModel, appFormModel, helpers) {

                       //creating classes
                       self.JobDetailModel = jobDetailModel;
                       self.JobListModel = jobListModel;
                       self.AppFormModel = appFormModel;
                       self.helpers = helpers;

                       //loading job list template
                       $.get(setts.mapPathOf("jobListingTemplate"), function (html) {
                           container.append(html);
                           require(["widgets/ShareThisLoader"], function () { });

                           var jobsList = new JobListModel({ companyId: ExelareSettings.curCompanyId, userId: ExelareSettings.curUserId, jobId: args.viewJob });

                           ko.applyBindings(jobsList, $('#jobsList')[0]);
                       });

                       //loading job detail template
                       $.get(setts.mapPathOf("jobDetailTemplate"), function (html) {
                           container.append(html);
                       })

                       //loading application form template
                       $.get(setts.mapPathOf("applicationFormTemplate"), function (html) {
                           container.append(html);
                       })
                   });
               });
        });
});