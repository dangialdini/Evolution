Date.prototype.stdTimezoneOffset = function () {
    var jan = new Date(this.getFullYear(), 0, 1);
    var jul = new Date(this.getFullYear(), 6, 1);
    return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
}

Date.prototype.dst = function () {
    return this.getTimezoneOffset() < this.stdTimezoneOffset();
}

function GetUTCTimeZoneOffset() {
    var intOffset = new Date().getTimezoneOffset();
    return intOffset * -1;
}

function GetDSTOffset() {
    var rc = 0;
    var intOffset = new Date().getTimezoneOffset();
    var now = new Date();
    if (now.dst()) {
        if (intOffset < 0) {    // So we know which side of Zulu we are
            rc = +60;
        } else {
            rc = -60;
        }
    }
    return rc;
}
