//creating job details model

define(['knockout'], function (ko) {
    return function (item, schema) {
        var self = this;
        var sourceItem = item;

        //mapping field names to ko.observable
        for (var f in schema) {
            if (item[f])
            {
                self[f] = ko.observable(item[f]);
            }
            else {
                self[f] = ko.observable("");
            }
        }

        self.applyUrl = ko.computed(function () {
            return ExelareSettings.applicationFormTemplate;
        });
        self.htmlDescription = ko.computed(function () {
            return (self.Description() ? self.Description().replace(/\n/g, '<br />') : '');
        });

        self.PostDate = ko.computed(function () {
            return (new Date(self.CreateDate())).toLocaleDateString();
        });

        self.viewJobUrlCmd = ko.computed(function () {
            return "#viewJob=" + self.ReqID();
        })

        self.ApplyLinkedInMeta = ko.computed(function () {
            return "companyID:"+ExelareSettings.curCompanyId+";submitType:2";
        })

        self.RecruiterCompanyName = ko.computed(function () {
            return ExelareSettings.companyName;
        })
        

        self.isShareJob = ko.computed(function () {
            return $.deparam.fragment().viewJob!=null;
        })

        self.viewJob = function () {
            var detailsModal = $('#detailsModal');

            ko.cleanNode(detailsModal[0]);
            ko.applyBindings(self, detailsModal[0]);
            detailsModal.modal('show');
        };

        self.applyJob = function () {
            var formModal = $('#appFormModal');
            var appFormMdl = new AppFormModel(self);

            $('#detailsModal').modal('hide');

            ko.cleanNode(formModal[0]);
            ko.applyBindings(appFormMdl, formModal[0]);
            formModal.modal('show');
        };
    };
});

