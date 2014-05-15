define(['helpers'], function(helpers){
    var AjaxDataSource = function (options) {
        this._companyId = options.companyId;
        this._userId = options.userId;
        this._formatter = options.formatter;
        this._columns = options.columns;
        this._loaded = options.loaded;
    };

    AjaxDataSource.prototype = {

        /**
         * Returns stored column metadata
         */
        columns: function () {
            return this._columns;
        },

        /**
         * Called when Datagrid needs data. Logic should check the options parameter
         * to determine what data to return, then return data by calling the callback.
         * @param {object} options Options selected in datagrid (ex: {pageIndex:0,pageSize:5,search:'searchterm'})
         * @param {function} callback To be called with the requested data.
         */
        data: function (options, callback) {
            var self = this;

            var req = {
                CompanyId: self._companyId,
                UserId: self._userId,
                PageNumber: options.pageIndex+1,
                PageSize: options.pageSize,
            }

            var keywordFilter, locationFilter;

            //keyword filter
            if (options.search) {
                keywordFilter = {
                    FilterColType: "Or",
                    FilterCol: []
                }

                for (var i = 0; i < ExelareSettings.searchColumns.length; i++) {
                    keywordFilter.FilterCol.push({
                        DataType: "String",
                        FieldName: "Requirements." + ExelareSettings.searchColumns[i],
                        FieldValue1: options.search
                    });
                }
            }

            //location filter
            if (options.location) {
                locationFilter = {
                    FilterColType: "Or",
                    FilterCol: [{
                        DataType: "String",
                        FieldName: "Requirements.Location",
                        FieldValue1: options.location
                    }]
                }
            }

            //jobId provided
            if (options.jobId) {
                req.FilterBy = {
                    DataType: "String",
                    FilterType: "=",
                    FieldName: "Requirements.ReqID",
                    FieldValue1: options.jobId
                }
            }
            else if (options.location || options.search) {
                req.FilterBy = {
                    FilterColType: "And",
                    FilterCol: []
                }

                if (options.search) req.FilterBy.FilterCol.push(keywordFilter);
                if (options.location) req.FilterBy.FilterCol.push(locationFilter);
            }



            //sort jobs
            if (options.sortProperty) {
                req.OrderBy = [{
                    fieldname: options.sortProperty,
                    direction: options.sortDirection
                }]
            }

            helpers.getService("jobs", req, function (resp) {
                // Prepare data to return to Datagrid
                var data = resp.Records;
                var count = resp.RecordCount;
                var startIndex = (req.PageNumber - 1) * req.PageSize;
                var endIndex = startIndex + req.PageSize;
                var end = (endIndex > count) ? count : endIndex;
                var pages = Math.ceil(count / req.PageSize);
                var page = req.PageNumber;
                var start = startIndex + 1;

                // Allow client code to format the data
                //if (self._formatter) self._formatter(data);
                
                ExelareSettings.LastResponse = resp;
                // Return data to Datagrid
                callback({ data: data, start: start, end: end, count: count, pages: pages, page: page, schema: resp.Schema });
            });

        }
    };
    return AjaxDataSource;
});


