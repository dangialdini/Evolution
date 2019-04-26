function CopyToClipboard(element, bReplaceCommasWithTabs) {
    var ctrl = $('#' + element);
    var text = ctrl.val();
    if (bReplaceCommasWithTabs) {
        ctrl.val(text.replace(/, /g, '\t'));
        ctrl.select();
        ctrl.focus();
        document.execCommand("copy");
    } else {
        ctrl.select();
        ctrl.focus();
        document.execCommand("copy");
    }
    ctrl.val(text);
}