function MessageBox(msg, caption, buttonConfig, icon) {
    HideProgress();
    if($('#msgbox').length > 0) {

        var html = '';
        for (var i = 0; i < buttonConfig.length; i++) {
            if (i > 0) html += '&nbsp;&nbsp;';
            html += '<button type="button" class="btn ' + buttonConfig[i][2] + '"';
            html += ' data-dismiss="modal"';
            // Buttons with no call-back function specified just cause the modal to disappear ie equvalent to a Cancel button
            if (buttonConfig[i][1] !== '') html += ' onclick="' + buttonConfig[i][1] + '"';
            html += '>' + buttonConfig[i][0] + '</button>';
        }
        var msgHtml = '';
        if (icon) {
            msgHtml = '<table style="width:100%" border="0">';
            msgHtml += '<tr><td style="width:50px;vertical-align:middle"><img src="/Content/' + icon + '.png" alt=""/></td>';
            msgHtml += '<td style="width:auto">' + msg + '</td></tr>';
            msgHtml += '</table>';
        } else {
            msgHtml = msg;
        }
        msgHtml = msgHtml.replace(/&lt;/g, '<');
        msgHtml = msgHtml.replace(/&gt;/g, '>');

        var msgboxclass = 'modal-dialog modal-dialog-centered modal-';
        if (msg.length > 60) {
            msgboxclass += 'lg';
        } else {
            msgboxclass += 'sm';
        }

        $('#msgboxclass').attr('class', msgboxclass);

        $('#modalcaption').html(caption);
        $('#modaltext').html(msgHtml);
        $('#modalbuttons').html(html);

        $('#msgbox').modal({
            backdrop: 'static',
            keyboard: false
        });
    } else {
        alert('Error: A <div> with an id of "msgbox" could not be found - unable to display MessageBox!');
    }
}

function Confirm(msg, caption, id, fn) {
    var buttons = [['Yes', fn + '(' + id + ')', 'btn-primary'],
                   ['No', '', 'btn-default']];
    MessageBox(msg, caption, buttons, 'IconQuestion');
    return false;
}

function Confirm2(msg, caption, idx, id, fn) {
    var buttons = [['Yes', fn + '(' + idx + ',' + id + ')', 'btn-primary'],
    ['No', '', 'btn-default']];
    MessageBox(msg, caption, buttons, 'IconQuestion');
    return false;
}

function Confirm3(msg, caption, idx, fn, url) {
    var buttons = [['Yes', fn + '(' + idx + ',\'' + url + '\')', 'btn-primary'],
    ['No', '', 'btn-default']];
    MessageBox(msg, caption, buttons, 'IconQuestion');
    return false;
}

function Confirm4(msg, caption, fn) {
    var buttons = [['Yes', fn + '()', 'btn-primary'],
    ['No', '', 'btn-default']];
    MessageBox(msg, caption, buttons, 'IconQuestion');
    return false;
}

function Alert(msg, caption) {
    var buttons = [['Ok', '', 'btn-primary']];
    MessageBox(msg, caption, buttons, 'IconExclamation');
    return false;
}
