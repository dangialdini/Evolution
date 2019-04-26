function DisplayProgress() {
    var img = $('#divProgress');
    img.show();
    CentreProgress();
    //$('body').append('<div id="overlay" style="background-color:red;position:absolute;top:0;left:0;height:100%;width:100%;z-index:999></div>');
}
function HideProgress() {
    var img = $('#divProgress');
    img.hide();
    $("#overlay").remove();
}
function CentreProgress() {
    var img = $('#imgProgress');

    var w = 0; var h = 0; var ww = 50;
    //IE
    if (!window.innerWidth) {
        if (!(document.documentElement.clientWidth === 0)) {
            //strict mode
            w = document.documentElement.clientWidth; h = document.documentElement.clientHeight;
        } else {
            //quirks mode
            w = document.body.clientWidth; h = document.body.clientHeight;
        }
    } else {
        //w3c
        w = window.innerWidth; h = window.innerHeight;
    }
    if (!window.pageYOffset) {
        //strict mode
        if (!(document.documentElement.scrollTop === 0)) {
            offsetY = document.documentElement.scrollTop; offsetX = document.documentElement.scrollLeft;
        } else {
            //quirks mode
            offsetY = document.body.scrollTop; offsetX = document.body.scrollLeft;
        }
    } else {
        //w3c
        offsetX = window.pageXOffset; offsetY = window.pageYOffset;
    }

    var left = w / 2 + offsetX - ww;
    var top = h / 2 + offsetY - ww;

    img.css({ left: left, top: top, position: 'absolute' });
}
$(window).scroll(function () {
    CentreProgress();
});
$(window).resize(function () {
    CentreProgress();
});
