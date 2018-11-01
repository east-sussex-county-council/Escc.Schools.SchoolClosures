$(function () {
    // Set up or remove 5 minute timer
    var timer = null;
    function clicked() {
        if (this.checked) {
            timer = setInterval(reload, 300000);
        }
        else {
            clearInterval(timer);
        }
    }

    // Reload the page
    function reload() {
        var currentUrl = document.location.href;
        if (currentUrl.indexOf('?') > -1) currentUrl = currentUrl.substring(0, currentUrl.indexOf('?'));
        document.location.href = currentUrl + "?refresh";
    }

    // Create checkbox
    $("#updates").after('<label for="refresh"><input type="checkbox" id="refresh" /> Tick here to reload the page automatically every five minutes</label>');

    // Wire up checkbox, and recheck to keep refreshing
    var checkbox = $("#refresh");
    var checked = (document.location.href.substring(document.location.href.length - 8) == "?refresh");
    checkbox.click(clicked).attr('checked', checked);
    if (checked) {
        // Assign function to checkbox so 'this' works
        checkbox[0].refreshIt = clicked;
        checkbox[0].refreshIt();
    }
})