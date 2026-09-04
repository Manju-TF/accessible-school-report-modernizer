(function () {
    function startPdfDownload(url) {
        var frame = document.createElement('iframe');
        frame.title = 'PDF download';
        frame.setAttribute('aria-hidden', 'true');
        frame.style.cssText = 'position:absolute;width:0;height:0;border:0;visibility:hidden';
        frame.src = url;
        document.body.appendChild(frame);
        window.setTimeout(function () {
            if (frame.parentNode) {
                frame.parentNode.removeChild(frame);
            }
        }, 60000);
    }

    window.downloadReportPdf = startPdfDownload;

    document.addEventListener('click', function (event) {
        var link = event.target.closest('a[href^="/downloads/"]');
        if (!link || event.button !== 0) {
            return;
        }

        if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
            return;
        }

        event.preventDefault();
        event.stopImmediatePropagation();
        startPdfDownload(link.href);
    }, true);
})();
