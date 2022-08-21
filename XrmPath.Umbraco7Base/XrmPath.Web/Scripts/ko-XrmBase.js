(function () {

    ko.bindingHandlers.fadeVisible = {
        init: function (element, valueAccessor) {
            // Start visible/invisible according to initial value
            var shouldDisplay = valueAccessor();
            $(element).toggle(shouldDisplay);
        },
        update: function (element, valueAccessor) {
            // On update, fade in/out
            var shouldDisplay = valueAccessor();
            shouldDisplay ? $(element).fadeIn() : $(element).hide();
        }
    };

    ko.bindingHandlers.crossfade = {
        init: function (element, valueAccessor) {
            // Start visible/invisible according to initial value
            var shouldDisplay = valueAccessor();
            $(element).toggle(shouldDisplay);
        },
        update: function (element, valueAccessor) {
            // On update, fade in/out
            var shouldDisplay = valueAccessor();
            shouldDisplay ? $(element).fadeIn() : $(element).fadeOut();
        }
    };

    // for sliding
    ko.bindingHandlers.slide = {
        init: function (element, valueAccessor) {
            // Start visible/invisible according to initial value
            //var shouldDisplay = valueAccessor();
        },
        update: function (element, valueAccessor) {
            // On update, fade in/out
            var shouldDisplay = valueAccessor();
            //console.log($(element).position().left);
            shouldDisplay ? $(".slides").animate({ "left": -$(element).position().left }) : false;
        }
    };

    ko.bindingHandlers.className = {
        update: function (element, valueAccessor) {
            var elem = $(element), val =
			ko.utils.unwrapObservable(valueAccessor());
            //console.log(element, val)
            elem.addClass(val);
        }
    };

    //to find 1 object in an observableArray.
    //observableArray.find("propertyID", valueToMatch);
    ko.observableArray.fn.find = function (prop, valueToMatch) {
        return ko.utils.arrayFirst(this(), function (item) {
            return item[prop] === valueToMatch;
        });
    };

    ko.bindingHandlers.enterkey = {
        init: function (element, valueAccessor, allBindings, viewModel) {
            var callback = valueAccessor();
            $(element).keypress(function (event) {
                var keyCode = (event.which ? event.which : event.keyCode);
                if (keyCode === 13) {
                    callback.call(viewModel);
                    return false;
                }
                return true;
            });
        }
    };

})();