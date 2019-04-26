// This module depends on Error.js
var _divName = null;
var _success = null;
var _fail = null;
var _param1 = null;
var _param2 = null;

function PostForm(button, divName, formName, url, success, fail, param1, param2) {
    // button = the JQuery button object which was pressed to call this function so that we can get its name/value to pass to the http call
    // divName = div containing the form (this will be filled with the content returned from the http call)
    // formName = name of the form to be sent
    // url = url to send it to
    // success = callback function to call after content has been successfully received back (not the string name)
    // fail = callback function to call if content failed to be retrieved (not the string name)
    // param1/2 = parameters for success callback
    DisplayProgress();

    _divName = divName;
    _success = success;
    _fail = fail;
    _param1 = param1;
    _param2 = param2;
    var form = $('#' + formName).serialize() + '&' + button.attr('name') + '=' + button.val();

    $.ajax({
        url: url,
        type: 'post',
        contentType: 'application/x-www-form-urlencoded',
        data: form
    })
    .done(function (data, status, xhr) {
        if (data.Error) {
            if (data.Error.IsError == 'true' || data.Error.IsError == true) {
                if (data.Error.Message) {
                    // If a failure callback was passed, call it
                    if (_fail) {
                        _fail(data.Error.Icon, data.Error.Message);
                    } else {
                        DisplayError(data.Error.Icon, data.Error.Message);
                    }
                } else if (data.Error.URL) {
                    // Call returned a URL to redirect to
                    window.location.href = data.Error.URL;
                }
            } else {
                if (data.Error.Message) {
                    // If a success callback was passed, call it
                    if (_success) {
                        if (_param1 && _param2) {
                            _success(data, _param1, _param2);
                        } else if (_param1) {
                            _success(data, _param1);
                        } else {
                            _success(data);
                        }
                    }
                } else if (data.Error.URL) {
                    // Call returned a URL to redirect to
                    window.location.href = data.Error.URL;
                }
            }
        } else {
            // Load the returned content into the parameter div
            if (_divName) $('#' + _divName).html(data);

            // If a callback was passed, call it
            if (_success) {
                if (_param1 && _param2) {
                    _success(data, _param1, _param2);
                } else if (_param1) {
                    _success(data, _param1);
                } else {
                    _success(data);
                }
            }
        }
        _divName = null;
        _success = null;
        _fail = null;
        _param1 = null;
        _param2 = null;
        HideProgress();
    })
    .fail(function (xhr, status, error) {
        var errMsg = 'Failed to POST form: ' + status + ' : ' + error;
        // If a failure callback was passed, called it
        if (_fail) {
            _fail(1, errMsg);
        } else {
            DisplayExclamationError(errMsg);
        }
    });
}

function IsErrorSummary(data) {
    // Checks for the presence of an MVC error summary
    var rc = false;
    if (data.indexOf('<div class="validation-summary-errors') != -1 &&
        data.indexOf('data-valmsg-summary="true"><ul><li style="display:none">') == -1) {
        rc = true;
    }
    return rc;
}
