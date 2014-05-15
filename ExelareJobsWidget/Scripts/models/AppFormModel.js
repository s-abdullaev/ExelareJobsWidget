define(['knockout'], function (ko) {
    return function (job) {
        var self = this;
        var applyJob = job;
        var formControls = $('#appForm [data-fieldname]');

        for (var i = 0; i < formControls.length; i++) {
            var fieldName = $(formControls[i]).attr("data-fieldname");

            self[fieldName] = ko.observable("");
        }

        self.applyJobTitle = ko.computed(function () {
            return (applyJob.JobTitle());
        });

        self.isReady = ko.observable(true);

        self.submitResume = function () {
            if (typeof FormData == "undefined") {
                alert("Your browser is too old. Please, upgrade it to later version, or use Chrome/Firefox/Safari.");
                return;
            }

            self.isReady(false);
            $('#btnResumeSubmit').button('loading');

            setTimeout(function () {
                var formData = new FormData();

                for (var i = 0; i < formControls.length; i++) {
                    var fieldName = $(formControls[i]).attr("data-fieldname");

                    formData.append("CandidateInfo." + fieldName, self[fieldName]());
                }

                formData.append("AccountInfo.SubmitType", $("#appForm").attr("data-submitType"));
                formData.append("AccountInfo.CompanyID", ExelareSettings.curCompanyId);
                formData.append("AccountInfo.UserID", ExelareSettings.curUserId);

                formData.append("JobInfo.ReqID", job.ReqID());
                formData.append("ResumeFile", $("#ctrlResume")[0].files[0]);

                $.ajax({
                    url: ExelareSettings.serviceUrl + "form",
                    type: 'POST',
                    data: formData,
                    async: false,
                    cache: false,
                    contentType: false,
                    processData: false
                }).done(function (data) {
                    self.isReady(true);
                    $('#btnResumeSubmit').button('reset');
                    $('#appFormModal').modal('hide');
                    $('#successModal').modal('show');
                }).fail(function (data) {
                    self.isReady(true);
                    $('#btnResumeSubmit').button('reset');
                    $('#appFormModal').modal('hide');
                    $('#errorModal').modal('show');
                });
            }, 10);
            //$("#appForm").submit();
            //$("#appFormModal").modal('hide');
        };
    };
});

