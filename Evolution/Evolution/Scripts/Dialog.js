var _focusField = '';

function Dialog(url, size, focusField) {    // Size is 'lg' or 'sm'
    HideError();
    if ($('#dialog').length > 0) {
        if (!size) size = 'sm';
        var dlgboxclass = 'modal-dialog modal-dialog-centered modal-' + size;
        $('#dlgboxclass').attr('class', dlgboxclass);

        var tempUrl = url;
        if (tempUrl.indexOf('?') == -1) {
            tempUrl += '?';
        } else {
            tempUrl += '&';
        }
        tempUrl += 'd=' + new Date().getTime();    // Bypasses caching
        var content = 
        $('#modal-content').load(tempUrl);

        $('#dialog').modal({
            backdrop: 'static',
            keyboard: false
        });
        if (focusField) {
            _focusField = focusField;
            setTimeout(OnDlgSetFocus, 500);
        } else {
            HideProgress();
        }

    } else {
        HideProgress();
        alert('No <div> element named "dialog" could be found to display the Dialog!');
    }
}

function HideDialog() {
    $('#btnCloseDialog').click();
}

function OnDlgSetFocus() {
    $('#' + _focusField).focus()
    HideProgress();
}