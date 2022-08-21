var XrmPathGlobalFunctions = /** @class */ (function () {
    function XrmPathGlobalFunctions() {
    }
    XrmPathGlobalFunctions.prototype.HasValue = function (item) {
        var hasValue = true;
        if (typeof item == 'undefined') {
            hasValue = false;
        }
        else if (item == null) {
            hasValue = false;
        }
        else if (String(item) === '') {
            hasValue = false;
        }
        return hasValue;
    };
    XrmPathGlobalFunctions.prototype.ToBoolean = function (item) {
        var boolValue = false;
        var self = this;
        if (self.HasValue(item)) {
            if (item === "1" || item === 1 || item === true || item === "True" || item === "true" || item === "Yes" || item === "yes") {
                boolValue = true;
            }
        }
        return boolValue;
    };
    XrmPathGlobalFunctions.prototype.ToggleElements = function (elementsToHide, elementsToShow) {
        var isVisible = $(elementsToShow).is(":visible");
        $(elementsToHide).hide();
        //console.log(isVisible);
        if (!isVisible) {
            $(elementsToShow).show();
        }
        return false;
    };
    XrmPathGlobalFunctions.prototype.Contains = function (array, checkValue) {
        var containsValue = false;
        for (var _i = 0, array_1 = array; _i < array_1.length; _i++) {
            var value = array_1[_i];
            if (value === checkValue) {
                containsValue = true;
                return true;
            }
        }
        return containsValue;
    };
    XrmPathGlobalFunctions.prototype.GetQueryStringParam = function (paramName) {
        var searchString = window.location.search.substring(1), i, val;
        if (searchString == '') {
            searchString = window.location.hash.replace('#', '');
        }
        var params = searchString.split("&");
        for (var _i = 0, params_1 = params; _i < params_1.length; _i++) {
            var item = params_1[_i];
            val = item.split("=");
            if (val[0] == paramName) {
                return val[1];
            }
        }
        return "";
    };
    XrmPathGlobalFunctions.prototype.GetQueryStringParamFromUrl = function (paramName, url) {
        var self = this;
        if (!self.HasValue(paramName)) {
            return "";
        }
        if (!self.HasValue(url)) {
            url = window.location.href;
        }
        if (url.indexOf("?") > -1) {
            var indexOf = url.indexOf("?");
            url = url.substring(indexOf + 1, url.length);
        }
        else {
            return "";
        }
        var searchString = url, i, val;
        if (searchString == '') {
            searchString = url.replace('#', '');
        }
        var params = searchString.split("&");
        for (var _i = 0, params_2 = params; _i < params_2.length; _i++) {
            var item = params_2[_i];
            val = item.split("=");
            if (val[0] == paramName) {
                return val[1];
            }
        }
        return "";
    };
    XrmPathGlobalFunctions.prototype.GetQueryStringParamList = function (uri) {
        uri = uri.split('?')[uri.split('?').length - 1];
        var params = {}, queries, currentQ, i, l;
        queries = uri.split("&");
        for (var _i = 0, queries_1 = queries; _i < queries_1.length; _i++) {
            var item = queries_1[_i];
            currentQ = item.split('=');
            params[currentQ[0]] = currentQ[1];
        }
        return params;
    };
    XrmPathGlobalFunctions.prototype.UpdateQueryStringParameter = function (uri, key, value) {
        var redirectUri = uri;
        var re = new RegExp("([?&])" + key + "=.*?(&|#|$)", "i");
        if (value === undefined) {
            if (uri.match(re)) {
                redirectUri = uri.replace(re, '$1$2');
            }
            else {
                redirectUri = uri;
            }
        }
        else {
            if (uri.match(re)) {
                redirectUri = uri.replace(re, '$1' + key + "=" + value + '$2');
            }
            else {
                var hash = '';
                if (uri.indexOf('#') !== -1) {
                    hash = uri.replace(/.*#/, '#');
                    uri = uri.replace(/#.*/, '');
                }
                var separator = uri.indexOf('?') !== -1 ? "&" : "?";
                redirectUri = uri + separator + key + "=" + value + hash;
            }
        }
        return redirectUri;
    };
    XrmPathGlobalFunctions.prototype.PushParamToQueryString = function (parameter, value) {
        var hash = window.location.hash.substr(1, window.location.hash.length);
        var params = hash.split('&');
        var alreadyExists = false;
        for (var _i = 0, params_3 = params; _i < params_3.length; _i++) {
            var item = params_3[_i];
            if (item.split('=')[0] == parameter) {
                item = item.split('=')[0] + '=' + value;
                alreadyExists = true;
            }
        }
        if (!alreadyExists) {
            params.push(parameter + '=' + value);
        }
        hash = params.join('&');
        window.location.hash = hash;
    };
    XrmPathGlobalFunctions.prototype.RedirectUpdateQueryStringParameter = function (uri, key, value) {
        var self = this;
        var redirectUri = self.UpdateQueryStringParameter(uri, key, value);
        redirectUri = encodeURI(redirectUri);
        location.href = redirectUri;
        return false;
    };
    XrmPathGlobalFunctions.prototype.FindObjectByKey = function (array, key, value) {
        var self = this;
        if (self.HasValue(array) && array.length > 0) {
            var item = array.find(function (i) { return i[key] === value; });
            if (XrmPathApp.HasValue(item)) {
                return item;
            }
        }
        return null;
    };
    XrmPathGlobalFunctions.prototype.FindObjectWithChildrenByKey = function (array, key, value) {
        var self = this;
        if (self.HasValue(array) && array.length > 0) {
            console.log(array);
            for (var _i = 0, array_2 = array; _i < array_2.length; _i++) {
                var item = array_2[_i];
                if (item[key] === value) {
                    return item;
                }
                else {
                    var children = item.Children;
                    if (children.length > 0) {
                        for (var _a = 0, children_1 = children; _a < children_1.length; _a++) {
                            var childItem = children_1[_a];
                            if (childItem[key] === value) {
                                return childItem;
                            }
                        }
                    }
                }
            }
        }
        return null;
    };
    XrmPathGlobalFunctions.prototype.FilterObjectByKey = function (array, key, value) {
        var self = this;
        if (self.HasValue(array) && array.length > 0) {
            var itemList = array.filter(function (i) { return i[key] === value; });
            if (XrmPathApp.HasValue(itemList) && itemList.length > 0) {
                return itemList;
            }
        }
        return null;
    };
    XrmPathGlobalFunctions.prototype.ReplaceAll = function (str, find, replace) {
        var self = this;
        return str.replace(new RegExp(self.EscapeRegExp(find), 'g'), replace);
    };
    XrmPathGlobalFunctions.prototype.EscapeRegExp = function (str) {
        return str.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
    };
    XrmPathGlobalFunctions.prototype.TrackPageView = function (id, title) {
        var self = this;
        var contentId = 0;
        if (self.HasValue(id)) {
            contentId = (id / 1);
        }
        else {
            var contentIdElement = $("#contentId");
            if (contentIdElement.length > 0) {
                contentId = $(contentIdElement.first()).val();
            }
        }
        if (contentId === 0) {
            return false;
        }
        var text = "";
        if (self.HasValue(title)) {
            text = title;
        }
        //var data = { Id: contentId, Value: "", Text: text };
        var data = new GenericModel(contentId, "", text);
        var url = "/api/page/PostPageView/";
        self.PostApiRequest(url, data).then(function (e) { });
        //$.post(url, data, function(e) { });
        return false;
    };
    XrmPathGlobalFunctions.prototype.ArrayToString = function (arr, joinText) {
        var csv = arr.join(joinText);
        return csv;
    };
    XrmPathGlobalFunctions.prototype.StringToArray = function (stringValue, separator) {
        var self = this;
        var output = [];
        if (self.HasValue(stringValue) && self.HasValue(separator)) {
            output = stringValue.split(separator);
        }
        return output;
    };
    XrmPathGlobalFunctions.prototype.JoinObject = function (arr, attr, joinText) {
        var self = this;
        var out = [];
        for (var _i = 0, arr_1 = arr; _i < arr_1.length; _i++) {
            var item = arr_1[_i];
            if (self.HasValue(attr)) {
                out.push(item[attr]);
            }
            else {
                out.push(item);
            }
        }
        return out.join(joinText);
    };
    XrmPathGlobalFunctions.prototype.CreateGuid = function () {
        function _p8(s) {
            var p = (Math.random().toString(16) + "000000000").substr(2, 8);
            return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
        }
        return _p8() + _p8(true) + _p8(true) + _p8();
    };
    XrmPathGlobalFunctions.prototype.GetMonthFromName = function (monthName) {
        var month = 0;
        if (monthName === "January") {
            month = 1;
        }
        else if (monthName === "February") {
            month = 2;
        }
        else if (monthName === "March") {
            month = 3;
        }
        else if (monthName === "April") {
            month = 4;
        }
        else if (monthName === "May") {
            month = 5;
        }
        else if (monthName === "June") {
            month = 6;
        }
        else if (monthName === "July") {
            month = 7;
        }
        else if (monthName === "August") {
            month = 8;
        }
        else if (monthName === "September") {
            month = 9;
        }
        else if (monthName === "October") {
            month = 10;
        }
        else if (monthName === "November") {
            month = 11;
        }
        else if (monthName === "December") {
            month = 12;
        }
        return month;
    };
    XrmPathGlobalFunctions.prototype.TriggerLoading = function (showElement, hideElement, disableButtonElement, enableButtonElement) {
        var self = this;
        if (self.HasValue(disableButtonElement)) {
            var buttonDisabledObj = $(disableButtonElement);
            buttonDisabledObj.prop("disabled", true);
            buttonDisabledObj.addClass("buttonDisabled");
        }
        if (self.HasValue(enableButtonElement)) {
            var buttonEnabledObj = $(enableButtonElement);
            buttonEnabledObj.prop("disabled", false);
            buttonEnabledObj.removeClass("buttonDisabled");
        }
        if (self.HasValue(hideElement)) {
            var hidingObj = $(hideElement);
            hidingObj.hide();
        }
        if (self.HasValue(showElement)) {
            var showObj = $(showElement);
            showObj.show();
        }
    };
    XrmPathGlobalFunctions.prototype.FormatCurrency = function (total) {
        var neg = false;
        if (total < 0) {
            neg = true;
            total = Math.abs(total);
        }
        return (neg ? "-$" : '$') + parseFloat(total.toString()).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
    };
    XrmPathGlobalFunctions.prototype.FormatDateTimeRange = function (dateStart, dateEnd) {
        var startDateTime = new moment(dateStart).format('LLL'); // April 17, 2019 1:25 PM
        var endDateTime = new moment(dateEnd).format('LLL');
        var startDate = new moment(dateStart).format('LL'); // April 17, 2019
        var endDate = new moment(dateEnd).format('LL');
        var startTime = new moment(dateStart).format('LT'); // 1:26 PM
        var endTime = new moment(dateEnd).format('LT');
        var dateRange = "";
        if (startDateTime == endDateTime) {
            //exact same start and end date/time
            dateRange = startDateTime;
        }
        else if (startDate == endDate) {
            //same start date, different times
            dateRange = startDate + ' - ' + startTime + ' to ' + endTime;
        }
        else {
            //different date and time
            dateRange = startDateTime + ' to ' + endDateTime;
        }
        return dateRange;
    };
    XrmPathGlobalFunctions.prototype.FormatDateRange = function (dateStart, dateEnd) {
        //var startDateTime = new moment(dateStart).format('LLL');    // April 17, 2019 1:25 PM
        //var endDateTime = new moment(dateEnd).format('LLL');
        var startDate = new moment(dateStart).format('LL'); // April 17, 2019
        var endDate = new moment(dateEnd).format('LL');
        var dateRange = "";
        if (startDate == endDate) {
            //same start date, different times
            dateRange = startDate;
        }
        else {
            //different date and time
            dateRange = startDate + ' to ' + endDate;
        }
        return dateRange;
    };
    XrmPathGlobalFunctions.prototype.FormatTimeRange = function (dateStart, dateEnd) {
        //var startDateTime = new moment(dateStart).format('LLL');    // April 17, 2019 1:25 PM
        //var endDateTime = new moment(dateEnd).format('LLL');
        //var startDate = new moment(dateStart).format('LL');         // April 17, 2019
        //var endDate = new moment(dateEnd).format('LL');
        var startTime = new moment(dateStart).format('LT'); // 1:26 PM
        var endTime = new moment(dateEnd).format('LT');
        var dateRange = "";
        if (startTime == endTime) {
            //same start date, different times
            dateRange = startTime;
        }
        else {
            //different date and time
            dateRange = startTime + ' to ' + endTime;
        }
        return dateRange;
    };
    XrmPathGlobalFunctions.prototype.GoogleTrackSearchTerm = function (searchTerm) {
        var self = this;
        if (self.HasValue(searchTerm) && self.GoodAnalyticsEnabled()) {
            var searchUrl = '/search.aspx?searchTerm=' + searchTerm;
            ga('send', 'pageview', searchUrl);
        }
        return false;
    };
    XrmPathGlobalFunctions.prototype.GoogleTrackUserClick = function (url) {
        var self = this;
        if (self.HasValue(url) && self.GoodAnalyticsEnabled()) {
            var currentDomain = 'http://' + window.location.hostname;
            var currentDomainSecure = 'https://' + window.location.hostname;
            var action = "";
            var category = "";
            var external = true;
            var label = url;
            //evaluate url and determine what to track it under
            if (url.endsWith(".pdf") || url.endsWith(".doc") || url.endsWith(".docx") || url.endsWith(".xls") || url.endsWith(".xlsx")) {
                //track click as file
                category = "Files";
                var filename = url.substring(url.lastIndexOf('/') + 1);
                label = filename; //set file name
                if (url.startsWith("/") || url.startsWith(currentDomain) || url.startsWith(currentDomainSecure)) {
                    external = false;
                    action = "Internal";
                }
                else if (url.startsWith("http://") || url.startsWith("https://") || url.startsWith("mailto:")) {
                    external = true;
                    action = "External";
                    if (url.startsWith("mailto:")) {
                        action = "Email";
                        label = url.substring(url.lastIndexOf('mailto:') + 7);
                    }
                }
            }
            else {
                //track click as link.
                category = "Links";
                label = url;
                if ((url.startsWith("http://") || url.startsWith("mailto:") || url.startsWith("https://")) && !url.startsWith(currentDomain) && !url.startsWith(currentDomainSecure)) {
                    external = true;
                    action = "External";
                    if (url.startsWith("mailto:")) {
                        action = "Email";
                        label = url.substring(url.lastIndexOf('mailto:') + 7);
                    }
                }
                else if (url.startsWith("/") || url.startsWith(currentDomain) || url.startsWith(currentDomainSecure)) {
                    // don't track internal links as google should be tracking those.
                    //external = false;
                    //action = "Internal";
                    return true;
                }
            }
            if (self.HasValue(action)) {
                self.GoogleTrackCustomEvent(category, action, label);
            }
            else {
                category = "Unknown File/Link";
                action = external ? "External" : "Internal";
                self.GoogleTrackCustomEvent(category, action, url);
            }
        }
        return true;
    };
    XrmPathGlobalFunctions.prototype.GoogleTrack404Error = function (url) {
        var self = this;
        if (self.HasValue(url)) {
            //track 404 in google
            var category = "Error";
            var action = "404 - Page not found";
            var label = url;
            self.GoogleTrackCustomEvent(category, action, label);
        }
    };
    XrmPathGlobalFunctions.prototype.GoodAnalyticsEnabled = function () {
        var googleAnalyticsEnabled = false;
        return googleAnalyticsEnabled;
    };
    XrmPathGlobalFunctions.prototype.GoogleTrackCustomEvent = function (category, action, label) {
        var self = this;
        if (self.GoodAnalyticsEnabled()) {
            //console.log("Tracking: " + category + '/' + action + '/' + label);
            ga('send', {
                hitType: 'event',
                eventCategory: category,
                eventAction: action,
                eventLabel: label
            });
        }
        return false;
    };
    XrmPathGlobalFunctions.prototype.GoogleTrackAdvancedSearchTerm = function (searchTermAny, searchTermAll, searchTermExact, searchTermNone) {
        var self = this;
        if (self.GoodAnalyticsEnabled()) {
            var category = "Advanced Search";
            var action = "";
            var label = "";
            var wordList = [];
            if (self.HasValue(searchTermAny)) {
                action = "Any of these words";
                wordList = self.SmartSplit(searchTermAny);
                for (var _i = 0, wordList_1 = wordList; _i < wordList_1.length; _i++) {
                    var value = wordList_1[_i];
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
            if (self.HasValue(searchTermAll)) {
                action = "All these words";
                wordList = self.SmartSplit(searchTermAll);
                for (var _a = 0, wordList_2 = wordList; _a < wordList_2.length; _a++) {
                    var value = wordList_2[_a];
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
            if (self.HasValue(searchTermExact)) {
                action = "This exact word or phrase";
                wordList = self.SmartSplit(searchTermExact, false);
                for (var _b = 0, wordList_3 = wordList; _b < wordList_3.length; _b++) {
                    var value = wordList_3[_b];
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
            if (self.HasValue(searchTermNone)) {
                action = "None of these words";
                wordList = self.SmartSplit(searchTermAny);
                for (var _c = 0, wordList_4 = wordList; _c < wordList_4.length; _c++) {
                    var value = wordList_4[_c];
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
        }
        return false;
    };
    XrmPathGlobalFunctions.prototype.SmartSplit = function (str, splitNoQuotes) {
        var self = this;
        var defaultArray = [str]; //string of 1 array
        if (!self.HasValue(splitNoQuotes)) {
            splitNoQuotes = true; //default is true
        }
        if (str.indexOf('\"') || splitNoQuotes) {
            var aStr = str.match(/\w+|"[^"]+"/g), i = aStr.length;
            while (i--) {
                aStr[i] = aStr[i].replace(/"/g, "");
            }
            return aStr;
        }
        else {
            return defaultArray;
        }
    };
    XrmPathGlobalFunctions.prototype.JSON2CSV = function (objArray) {
        var array = typeof objArray != 'object' ? JSON.parse(objArray) : objArray;
        var str = '';
        var line = '';
        for (var i = 0; i < array.length; i++) {
            var line = '';
            if ($("#quote").is(':checked')) {
                for (var index in array[i]) {
                    var value = array[i][index] + "";
                    line += '"' + value.replace(/"/g, '""') + '",';
                }
            }
            else {
                for (var index in array[i]) {
                    line += array[i][index] + ',';
                }
            }
            line = line.slice(0, -1);
            str += line + '\r\n';
        }
        return str;
    };
    XrmPathGlobalFunctions.prototype.TrimToLength = function (str, length) {
        if (str.length > length) {
            var trimmedString = str.substr(0, length);
            trimmedString = trimmedString.substr(0, Math.min(trimmedString.length, trimmedString.lastIndexOf(" ")));
            return trimmedString + "...";
        }
        else {
            return str;
        }
    };
    //http://api.jquery.com/jquery.ajax/
    //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/resolve
    XrmPathGlobalFunctions.prototype.GetApiRequest = function (url, async, dataType) {
        var self = this;
        if (!self.HasValue(async)) {
            async = true;
        }
        if (!self.HasValue(dataType)) {
            dataType = "json";
        }
        var promise = new Promise(function (resolve, reject) {
            $.ajax({
                type: 'GET',
                async: async,
                url: url,
                dataType: dataType,
                success: function (result) {
                    return resolve(result);
                },
                failure: function (result) {
                    return resolve(result);
                },
                error: function (result) {
                    return resolve(result);
                }
            });
        });
        return promise;
    };
    //http://api.jquery.com/jquery.ajax/
    //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/resolve
    XrmPathGlobalFunctions.prototype.PostApiRequest = function (url, data) {
        var self = this;
        if (!self.HasValue(data)) {
            data = null;
        }
        var promise = new Promise(function (resolve, reject) {
            $.ajax({
                type: 'POST',
                url: url,
                data: data,
                success: function (result) {
                    return resolve(result);
                },
                failure: function (result) {
                    return resolve(result);
                },
                error: function (result) {
                    return resolve(result);
                }
            });
        });
        return promise;
    };
    //http://api.jquery.com/jquery.ajax/
    //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/resolve
    XrmPathGlobalFunctions.prototype.PostApiFileRequest = function (url, data, dataFile) {
        var self = this;
        if (!self.HasValue(data)) {
            data = null;
        }
        if (!self.HasValue(dataFile)) {
            dataFile = null;
        }
        var promise = new Promise(function (resolve, reject) {
            $.ajax({
                type: "POST",
                url: url,
                contentType: false,
                processData: false,
                data: dataFile,
                success: function (result) {
                    return resolve(result);
                },
                failure: function (result) {
                    return resolve(result);
                },
                error: function (result) {
                    return resolve(result);
                }
            });
        });
        return promise;
    };
    XrmPathGlobalFunctions.prototype.GetXmlHttpRequest = function (url) {
        // Return a new promise.
        return new Promise(function (resolve, reject) {
            // Do the usual XHR stuff
            var req = new XMLHttpRequest();
            req.open('GET', url);
            req.onload = function () {
                // This is called even on 404 etc
                // so check the status
                if (req.status == 200) {
                    // Resolve the promise with the response text
                    resolve(req.response);
                }
                else {
                    // Otherwise reject with the status text
                    // which will hopefully be a meaningful error
                    reject(Error(req.statusText));
                }
            };
            // Handle network errors
            req.onerror = function () {
                reject(Error("Network Error"));
            };
            // Make the request
            req.send();
        });
    };
    XrmPathGlobalFunctions.prototype.DisableButtonElements = function (elements) {
        //console.log(elements);
        if (elements != null && elements.length != null && elements.length > 0) {
            for (var _i = 0, elements_1 = elements; _i < elements_1.length; _i++) {
                var element = elements_1[_i];
                var item = $(element);
                if (!item.hasClass("button-disabled")) {
                    item.addClass("button-disabled");
                }
                item.attr("href", "javascript:void(0)");
                item.click(function () {
                    return false;
                });
            }
        }
    };
    XrmPathGlobalFunctions.prototype.GetSessionKey = function () {
        var url = "/api/membership/getsessionkey";
        XrmPathApp.GetApiRequest(url).then(function (sessionKey) {
            return sessionKey;
        });
        return null;
    };
    XrmPathGlobalFunctions.prototype.LocalStorageSetValue = function (variableName, value) {
        var storeValue = JSON.stringify(value);
        window.localStorage.setItem(variableName, storeValue);
    };
    XrmPathGlobalFunctions.prototype.LocalStorageGetValue = function (variableName) {
        var localStorageValue = window.localStorage.getItem(variableName);
        var objValue = JSON.parse(localStorageValue);
        return objValue;
    };
    return XrmPathGlobalFunctions;
}());
var XrmPathGlobal = new XrmPathGlobalFunctions();
//export let XrmPathApp = XrmPathGlobal;
var XrmPathApp = XrmPathGlobal;
//# sourceMappingURL=globalFunctions.js.map