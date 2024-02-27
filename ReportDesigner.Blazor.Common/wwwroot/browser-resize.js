var GLOBAL = {};
GLOBAL.DotNetReference = null;
window.addEventListener("pointerdown", (event) => {
    console.log("pointerdown:" + event.target.className);
});
window.addEventListener("dragenter", (event) => {
    console.log("dragenter:" + event.target.className);
});
GLOBAL.SetDotnetReference = function (pDotNetReference) {
    GLOBAL.DotNetReference = pDotNetReference;    
};

const observer = new ResizeObserver(entries => {
    var isFired = false;
    for (let entry of entries) {
        isFired = true;
    }

    if (isFired) {
        GLOBAL.DotNetReference.invokeMethodAsync('OnInnerTextControlResized');
    }
    //todo : 한번 호출되는건 확인했다. 저 루프 안으로 넣어서 바로 사이즈 넘기면 좀더 깔끔하게 동작할듯 한데..

}
);

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

window.getEventTarget = (event) => {
    return event.target.id;
}

window.GetInnerTextWidth = function (parentId) {
    var parent = document.getElementById(parentId);
    if (parent == null) {
        console.log("parent is null");
        return;
    }
    var target = parent.querySelector('.component-text-inner');
    if (target == null) {
        console.log("target is null");
		return;
    }
    return {
        outer: parent.offsetWidth,
        inner: target.offsetWidth,
    }
}
window.GetInnerTextHeight = function (parentId) {
    var parent = document.getElementById(parentId);
    if (parent == null) {
        console.log("parent is null");
        return;
    }
    var target = parent.querySelector('.component-text-inner');
    if (target == null) {
        console.log("target is null");
        return;
    }
    return {
        outer: parent.offsetHeight,
        inner: target.offsetHeight,
    }
}

window.GetControlRatio = function (parentId) {
    var parent = document.getElementById(parentId);
    if (parent == null) {
        console.log("parent is null");
        return;
    }
    var target = parent.querySelector('.component-text-inner');
    if (target == null) {
        console.log("target is null");
        return;
    }

    return parent.offsetWidth / target.offsetWidth * 100;
}
window.RegisterInnerTextResizeCallback = function (parentId) {
    var parent = document.getElementById(parentId);
    if (parent == null) {
        console.log("parent is null");
        return;
    }
    var target = parent.querySelector('.component-text-inner');
    if (target == null) {
        console.log("target is null");
        return;
    }
    observer.observe(parent);
    observer.observe(target);
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