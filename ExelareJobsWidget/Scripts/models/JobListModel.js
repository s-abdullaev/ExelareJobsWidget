//creating job details model

define(['knockout', 'datasource'], function (ko, AjaxDataSource) {
        return function (creds) {

        var self = this;
        var dataSource = new AjaxDataSource({
            companyId: creds.companyId,
            userId: creds.userId
        });

        var options = {
            pageIndex: 0,
            pageSize: 10
        }

        if (creds.jobId) options.jobId = creds.jobId
            
        var load = function (opts) {
            helpers.displayLoader(true);
            dataSource.data(opts, function (resp) {
                var recs = [];
                for (var i = 0; i < resp.data.length; i++) {
                    recs.push(new JobDetailModel(resp.data[i], resp.schema));
                }

                self.pageNumber(resp.page);
                self.totalPages(resp.pages);
                self.records(recs);

                helpers.displayLoader(false);

                //apply linkedin fix start
                setTimeout(function () {
                    if (IN) {
                        for (var t = 0; t < recs.length; t++) {
                            try { IN.parse(); } catch (e) { }
                        }
                    }
                },5);
                //apply linkedin fix end
            });
        }

        self.pageNumber = ko.observable(0);
        self.totalPages = ko.observable(0);
        self.searchKeywords = ko.observable("");
        self.searchLocation = ko.observable("");
        self.records = ko.observableArray([]);

            //initialise
        load(options);

        self.search = function () {
            location.hash = "";
            options = {
                pageIndex: 0,
                pageSize: 10
            }

            options.search = self.searchKeywords();
            options.location = self.searchLocation();

            load(options);
        }

        self.clear = function () {
            location.hash = "";
            self.searchKeywords("");
            self.searchLocation("");

            options = {
                pageIndex: 0,
                pageSize: 10
            }

            load(options);
        }

        self.prevPage = function () {
            if (self.hasPrevPage()) {
                options.pageIndex--;
                load(options);
            };
        }

        self.nextPage = function () {
            if (self.hasNextPage()) {
                options.pageIndex++;
                load(options);
            };
        }

        self.hasNextPage = ko.computed(function () {
            return (parseInt(self.pageNumber()) + 1) <= self.totalPages();
        });

        self.hasPrevPage = ko.computed(function () {
            return (parseInt(self.pageNumber())-1) > 0;
        });

    };
})
