define({
    resourceUrl: "http://cbiz-srv2.network80.com/jobsWidget/",
    serviceUrl: "http://cbiz-srv2.network80.com/jobsWidget/api/",
    searchColumns: ["JobTitle", "Description", "Location"],
    applyUrl: "http://apps.exelare.com/cbizjobs/index.aspx?",
    companyName: "cBizSoft, Inc.",
    jobListingTemplate: "Templates/JobListing.html",
    jobDetailTemplate: "Templates/JobDetail.html",
    applicationFormTemplate: "Templates/ApplicationForm.html",
    mapPathOf: function (propName) {
        return this.resourceUrl + this[propName];
    },
    mapUrl: function (url) {
        return this.resourceUrl + url;
    }
});