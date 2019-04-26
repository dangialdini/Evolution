function DisplayError(iconType, msgText) {
    var html = '';

    if (!msgText || msgText.trim() === '') {
        iconType = parseInt(GetCookie('ErrorIcon'));
        msgText = GetCookie('ErrorText');
    }
    SetSessionError(0, '', '');

    if (msgText.trim() !== '') {
        var cssClass = 'errNone';
        var iconFile = '';
        var altText = '';

        if (iconType === 1) {
            cssClass = 'errExcl';
            iconFile = 'IconExclamation.png';
            altText = 'Error';
        } else if (iconType === 2) {
            cssClass = 'errInfo';
            iconFile = 'IconInformation.png';
            altText = 'Information';
        }

        html = '<table style="cellpadding:0; cellspacing:0;"><tr>';
        if (iconType !== 0) {
            html += '<td style="vertical-align:top;">';
            html += '&nbsp;<img src="/Content/' + iconFile + '" alt="' + altText + '"';
            html += '/>&nbsp;&nbsp;</td>';
        }
        html += '<td valign="middle"><span class="' + cssClass + '">' + msgText.replace(/\|\|/g, '<br/>') + '</span></td></tr>';
        html += "</table>\r\n";

        $('#div-error').addClass('bg-danger');
    }
    $('#lblError').html(html);
    HideProgress();
    _holdError = false;
}
function DisplayExclamationError(msgText) {
    DisplayError(1, msgText);
}
function DisplayInformationError(msgText) {
    DisplayError(2, msgText);
}
function HideError() {
    if(!_holdError) DisplayError(0, '');
}

function SetSessionError(iconType, msgText, errorField) {
    SetCookie('ErrorIcon', iconType, 1);
    SetCookie('ErrorText', msgText, 1);
    SetCookie('ErrorField', errorField, 1);
}

var _holdError = false;
function HoldError() {
    _holdError = true;
}
function UnHoldError() {
    _holdError = false;
}