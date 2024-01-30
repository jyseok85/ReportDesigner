﻿var GLOBAL = {};
GLOBAL.DotNetReference = null;
GLOBAL.SetDotnetReference = function (pDotNetReference) {
    GLOBAL.DotNetReference = pDotNetReference;
};

window.browserResize = {
    getInnerHeight: function () {
        return window.innerHeight;
    },
    getInnerWidth: function () {
        return window.innerWidth;
    },
    registerResizeCallback: function () {
        window.addEventListener("resize", browserResize.resized);
    },
    resized: function () {
        //1.Static 함수를 호출하는 방법
        //DotNet.invokeMethodAsync('{ASSEMBLY NAME}', '{.NET METHOD ID}', {ARGUMENTS});
        /* DotNet.invokeMethodAsync('ReportDesigner.Blazor.Common', 'OnBrowserResize').then(data => data);*/

        //2.Blazor 객체를 통하여 호출하는 방법
        GLOBAL.DotNetReference.invokeMethodAsync('OnBrowserResize');
    }
}

window.getDivSize = function (id) {
    var client = document.getElementById(id).getBoundingClientRect();
    return { width: client.width, height: client.height };
}
//window.initResizeObserver = function (elementRef) {
//    const element = elementRef.current;

//    const resizeObserver = new ResizeObserver(entries => {
//        const entry = entries[0];
//        const { width, height } = entry.contentRect;
//        GLOBAL.DotNetReference.invokeMethodAsync('UpdateSize', width, height);
//    });

//    resizeObserver.observe(element);
//};