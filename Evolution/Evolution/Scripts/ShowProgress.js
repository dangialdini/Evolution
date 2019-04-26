function ShowProgress(sender, args) {
    $('#lblError').html('');
    DisplayProgress();
    return true;
}
function ShowProgressWithConfirm(msg) {
    var rc = confirm(msg.replace(/\|\|/g, "\n"));
    if (rc) {
        ShowProgress(null, null);
    }
    return rc;
}
