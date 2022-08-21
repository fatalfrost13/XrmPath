//declare reserved variables that TS will not recognize unless we import them
declare var moment: any;
declare var ga: any;
declare var $: any;
declare var Promise: any;

class XrmPathGlobalFunctions {

    constructor () {

    }

    public HasValue(item: any){
        var hasValue = true;

        if (typeof item == 'undefined') {
            hasValue = false;
        } else if (item == null) {
            hasValue = false;
        } else if (String(item) === '') {
            hasValue = false;
        }
        return hasValue;
    }

    ToBoolean(item: any){
        var boolValue = false;
        let self = this;
        if (self.HasValue(item)) {
            if (item === "1" || item === 1 || item === true || item === "True" || item === "true" || item === "Yes" || item === "yes") {
                boolValue = true;
            }
        }
        return boolValue;
    }

    ToggleElements(elementsToHide: string, elementsToShow: string) {
        var isVisible = $(elementsToShow).is(":visible");
        $(elementsToHide).hide();
        //console.log(isVisible);
        if (!isVisible) {
            $(elementsToShow).show();
        }
        return false;
    }

    Contains(array: any, checkValue: string) {
        var containsValue = false;
        for(let value of array){
            if (value === checkValue) {
                containsValue = true;
                return true;
            }
        }
        return containsValue;
    }

    GetQueryStringParam(paramName: string) {
        var searchString = window.location.search.substring(1), i, val;

        if (searchString == ''){
            searchString = window.location.hash.replace('#', '');
        }

        var params = searchString.split("&");

        for(let item of params){
            val = item.split("=");
            if (val[0] == paramName) {
                return val[1];
            }
        }

        return "";
    }

    GetQueryStringParamFromUrl(paramName: string, url: string) {
        let self = this;
        if (!self.HasValue(paramName))
        {
            return "";
        }

        if (!self.HasValue(url))
        {
            url = window.location.href;
        }

        if (url.indexOf("?") > -1)
        {
            var indexOf = url.indexOf("?");
            url = url.substring(indexOf + 1, url.length);
        }
        else
        {
            return "";
        }

        var searchString = url, i, val;

        if (searchString == ''){
            searchString = url.replace('#', '');
        }

        var params = searchString.split("&");

        for(let item of params){
            val = item.split("=");
            if (val[0] == paramName) {
                return val[1];
            }
        }

        return "";
    }

    GetQueryStringParamList(uri: string) {
        uri = uri.split('?')[uri.split('?').length - 1];
        var params = {}, queries, currentQ, i, l;
        queries = uri.split("&");
        for(let item of queries){
            currentQ = item.split('=');
            params[currentQ[0]] = currentQ[1];
        }
        return params;
    }

     UpdateQueryStringParameter(uri: string, key: string, value: string) {
        var redirectUri = uri;
        var re = new RegExp("([?&])" + key + "=.*?(&|#|$)", "i");
        if (value === undefined) {
            if (uri.match(re)) {
                redirectUri = uri.replace(re, '$1$2');
            } else {
                redirectUri = uri;
            }
        } else {
            if (uri.match(re)) {
                redirectUri = uri.replace(re, '$1' + key + "=" + value + '$2');
            } else {
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
    }

    PushParamToQueryString(parameter: string, value: string) {
        var hash = window.location.hash.substr(1, window.location.hash.length)
        var params = hash.split('&');
        var alreadyExists = false;
        for (let item of params) {
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
    }

    RedirectUpdateQueryStringParameter(uri: string, key: string, value: string) {
        let self = this;
        var redirectUri = self.UpdateQueryStringParameter(uri, key, value);
        redirectUri = encodeURI(redirectUri);
        location.href = redirectUri;
        return false;
    }

    FindObjectByKey(array: any, key: string, value: any) {
        let self = this;
        if (self.HasValue(array) && array.length > 0) {
            var item = array.find(i => i[key] === value);
            if (XrmPathApp.HasValue(item)){
                return item;
            }
        }
        return null;
    }

    FindObjectWithChildrenByKey(array: any, key: string, value: any) {
        let self = this;
        if (self.HasValue(array) && array.length > 0) {
            console.log(array);
            for(let item of array){
                if (item[key] === value) {
                    return item;
                } else {
                    var children = item.Children;
                    if (children.length > 0) {
                        for (let childItem of children){
                            if (childItem[key] === value) {
                                return childItem;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }

    FilterObjectByKey(array: any, key: string, value: any) {
        let self = this;
        if (self.HasValue(array) && array.length > 0) {
            var itemList = array.filter(i => i[key] === value);
            if (XrmPathApp.HasValue(itemList) && itemList.length > 0){
                return itemList;
            }
        }
        return null;
    }

    ReplaceAll(str: string, find: string, replace: string) {
        let self = this;
        return str.replace(new RegExp(self.EscapeRegExp(find), 'g'), replace);
    }

    EscapeRegExp(str: string) {
        return str.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
    }

    TrackPageView(id?: number, title?: string) {

        let self = this;
        var contentId = 0;

        if (self.HasValue(id)) {
            contentId = (id / 1);
        } else {
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
        self.PostApiRequest(url, data).then(function(e){});
        //$.post(url, data, function(e) { });
        return false;
    }


    ArrayToString(arr: any, joinText: string) {
        var csv = arr.join(joinText);
        return csv;
    }

    StringToArray(stringValue: string, separator: string) {
        let self = this;
        var output = [];
        if (self.HasValue(stringValue) && self.HasValue(separator)) {
            output = stringValue.split(separator);
        }
        return output;
    }

    JoinObject(arr: string, attr: string, joinText: string) {
        let self = this;
        var out = [];
        for(let item of arr){
            if (self.HasValue(attr)){
                out.push(item[attr]);
            }
            else{
                out.push(item);
            }
        }
        return out.join(joinText);
    }

    CreateGuid() {
        function _p8(s?: any) {
            var p = (Math.random().toString(16) + "000000000").substr(2, 8);
            return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
        }

        return _p8() + _p8(true) + _p8(true) + _p8();
    }


    GetMonthFromName(monthName: string) {
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
    }


    TriggerLoading(showElement?: string, hideElement?: string, disableButtonElement?: string, enableButtonElement?: string) {

        let self = this;

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

    }

    FormatCurrency(total: number) 
    {
        var neg = false;
        if(total < 0) {
            neg = true;
            total = Math.abs(total);
        }
        return (neg ? "-$" : '$') + parseFloat(total.toString()).toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, "$1,").toString();
    }

    FormatDateTimeRange(dateStart: any, dateEnd: any)
    {
        var startDateTime = new moment(dateStart).format('LLL');    // April 17, 2019 1:25 PM
        var endDateTime = new moment(dateEnd).format('LLL');
        var startDate = new moment(dateStart).format('LL');         // April 17, 2019
        var endDate = new moment(dateEnd).format('LL');
        var startTime = new moment(dateStart).format('LT');         // 1:26 PM
        var endTime = new moment(dateEnd).format('LT');

        var dateRange = "";

        if (startDateTime == endDateTime){
            //exact same start and end date/time
            dateRange =  startDateTime;
        }
        else if(startDate == endDate)
        {
            //same start date, different times
            dateRange =  startDate + ' - ' + startTime + ' to ' + endTime;
        }
        else
        {
            //different date and time
            dateRange = startDateTime + ' to ' + endDateTime;
        }
        return dateRange;
    }

    FormatDateRange(dateStart: any, dateEnd: any)
    {
        //var startDateTime = new moment(dateStart).format('LLL');    // April 17, 2019 1:25 PM
        //var endDateTime = new moment(dateEnd).format('LLL');
        var startDate = new moment(dateStart).format('LL');         // April 17, 2019
        var endDate = new moment(dateEnd).format('LL');
        var dateRange = "";

        if(startDate == endDate)
        {
            //same start date, different times
            dateRange =  startDate;
        }
        else
        {
            //different date and time
            dateRange = startDate + ' to ' + endDate;
        }
        return dateRange;
    }

    FormatTimeRange(dateStart: any, dateEnd: any)
    {
        //var startDateTime = new moment(dateStart).format('LLL');    // April 17, 2019 1:25 PM
        //var endDateTime = new moment(dateEnd).format('LLL');
        //var startDate = new moment(dateStart).format('LL');         // April 17, 2019
        //var endDate = new moment(dateEnd).format('LL');
        var startTime = new moment(dateStart).format('LT');         // 1:26 PM
        var endTime = new moment(dateEnd).format('LT');

        var dateRange = "";

        if(startTime == endTime)
        {
            //same start date, different times
            dateRange =  startTime;
        }
        else
        {
            //different date and time
            dateRange = startTime + ' to ' + endTime;
        }
        return dateRange;
    }

    GoogleTrackSearchTerm(searchTerm: string) {

        let self = this;

        if (self.HasValue(searchTerm) && self.GoodAnalyticsEnabled()) {
            var searchUrl = '/search.aspx?searchTerm=' + searchTerm;
            ga('send', 'pageview', searchUrl);
        }
        return false;
    }

    GoogleTrackUserClick(url: any) {

        let self = this;
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
                label = filename;    //set file name

                if (url.startsWith("/") || url.startsWith(currentDomain) || url.startsWith(currentDomainSecure)) {
                    external = false;
                    action = "Internal";
                } else if (url.startsWith("http://") || url.startsWith("https://") || url.startsWith("mailto:")) {
                    external = true;
                    action = "External";

                    if (url.startsWith("mailto:")) {
                        action = "Email";
                        label = url.substring(url.lastIndexOf('mailto:') + 7);
                    }
                }

            } else {

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

                } else if (url.startsWith("/") || url.startsWith(currentDomain) || url.startsWith(currentDomainSecure)) {
                    // don't track internal links as google should be tracking those.
                    //external = false;
                    //action = "Internal";
                    return true;
                }

            }

            if (self.HasValue(action)) {
                self.GoogleTrackCustomEvent(category, action, label);
            } else {
                category = "Unknown File/Link";
                action = external ? "External" : "Internal";
                self.GoogleTrackCustomEvent(category, action, url);
            }
        }
        return true;
    }

    GoogleTrack404Error(url: string) {

        let self = this;
        if (self.HasValue(url)) {
            //track 404 in google
            var category = "Error";
            var action = "404 - Page not found";
            var label = url;
            self.GoogleTrackCustomEvent(category, action, label);
        }
    }

    GoodAnalyticsEnabled() {
        var googleAnalyticsEnabled = false;
        return googleAnalyticsEnabled;
    }

    GoogleTrackCustomEvent(category: string, action: string, label: string) {

        let self = this;
        if (self.GoodAnalyticsEnabled()) {
            //console.log("Tracking: " + category + '/' + action + '/' + label);
            ga('send',
                {
                    hitType: 'event',
                    eventCategory: category,
                    eventAction: action,
                    eventLabel: label
                });
        }
        return false;
    }

    GoogleTrackAdvancedSearchTerm(searchTermAny: string, searchTermAll: string, searchTermExact: string, searchTermNone: string) {
        
        let self = this;
        if (self.GoodAnalyticsEnabled()) {

            var category = "Advanced Search";
            var action = "";
            var label = "";
            var wordList = [];

            if (self.HasValue(searchTermAny)) {
                action = "Any of these words";
                wordList = self.SmartSplit(searchTermAny);

                for(let value of wordList){
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }

            }
            if (self.HasValue(searchTermAll)) {
                action = "All these words";
                wordList = self.SmartSplit(searchTermAll);

                for(let value of wordList){
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
            if (self.HasValue(searchTermExact)) {
                action = "This exact word or phrase";
                wordList = self.SmartSplit(searchTermExact, false);
                for(let value of wordList){
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
            if (self.HasValue(searchTermNone)) {
                action = "None of these words";
                wordList = self.SmartSplit(searchTermAny);
                for(let value of wordList){
                    label = value;
                    self.GoogleTrackCustomEvent(category, action, label);
                }
            }
        }

        return false;
    }

    SmartSplit(str: string, splitNoQuotes?: boolean) {

        let self = this;
        var defaultArray = [str];   //string of 1 array
        
        if (!self.HasValue(splitNoQuotes)) {
            splitNoQuotes = true;   //default is true
        }

        if (str.indexOf('\"') || splitNoQuotes) {
            var aStr = str.match(/\w+|"[^"]+"/g), i = aStr.length;
            while (i--) {
                aStr[i] = aStr[i].replace(/"/g, "");
            }
            return aStr;
        } else {
            return defaultArray;
        }

    }


    JSON2CSV(objArray: any) {

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
            } else {
                for (var index in array[i]) {
                    line += array[i][index] + ',';
                }
            }

            line = line.slice(0, -1);
            str += line + '\r\n';
        }
        return str;
    }

    TrimToLength(str: string, length: number) {
        if (str.length > length) {
            var trimmedString = str.substr(0, length);
            trimmedString = trimmedString.substr(0, Math.min(trimmedString.length, trimmedString.lastIndexOf(" ")));
            return trimmedString + "...";
        } else {
            return str;
        }
    }


    //http://api.jquery.com/jquery.ajax/
    //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/resolve
    GetApiRequest(url: string, async?: any, dataType?: any){

        let self = this;
        if (!self.HasValue(async)){
            async = true;
        }

        if (!self.HasValue(dataType)){
            dataType = "json";
        }

        var promise = new Promise(function (resolve, reject) 
        {
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
    }

    //http://api.jquery.com/jquery.ajax/
    //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/resolve
    PostApiRequest(url: string, data?: any){

        let self = this;
        if (!self.HasValue(data)){
            data = null;
        }

        var promise = new Promise(function (resolve, reject) 
        {
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
    }

    //http://api.jquery.com/jquery.ajax/
    //https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/resolve
    PostApiFileRequest(url: string, data?: any, dataFile?: any){

        let self = this;
        if (!self.HasValue(data)){
            data = null;
        }
        if (!self.HasValue(dataFile)){
            dataFile = null;
        }
        var promise = new Promise(function (resolve, reject) 
        {
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
    }

    GetXmlHttpRequest(url: string) {
   
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
    }

    DisableButtonElements(elements: any){
        //console.log(elements);
        if (elements != null && elements.length != null && elements.length > 0){
            for (let element of elements){
                var item = $(element);
                if (!item.hasClass("button-disabled")){
                    item.addClass("button-disabled");
                }
                item.attr("href", "javascript:void(0)");
                item.click(function(){
                    return false;
                }); 
            }
  
        }
    }

    GetSessionKey(){
        var url = "/api/membership/getsessionkey";
        XrmPathApp.GetApiRequest(url).then(function(sessionKey){
            return sessionKey;
        });
        return null;
    }

    LocalStorageSetValue(variableName: string, value: any)
    {
        var storeValue = JSON.stringify(value);
        window.localStorage.setItem(variableName, storeValue);
    }

    LocalStorageGetValue(variableName: string)
    {
        var localStorageValue = window.localStorage.getItem(variableName);
        var objValue = JSON.parse(localStorageValue);
        return objValue;
    }
}

let XrmPathGlobal = new XrmPathGlobalFunctions();
//export let XrmPathApp = XrmPathGlobal;
let XrmPathApp = XrmPathGlobal; 