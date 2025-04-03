window.imageEvents = {
  attachImageLoadEvents: function (imageId, dotNetRef) {
    const img = document.getElementById(imageId);
    console.log("yo");

    if (!img) return;

    img.onload = () => {
      console.log("Image loaded successfully.");
      dotNetRef.invokeMethodAsync("OnImageLoadSuccess");
    };

    img.onerror = () => {
      console.log("Image fail :(.");
      dotNetRef.invokeMethodAsync("OnImageLoadError");
    };
  }
}

window.getInnerText = (element) => {
    if (!element) {
        return "";
    }
    return element.innerText || "";
};

window.getPlainText = function (elementId) {
    const element = document.getElementById(elementId);
    return element ? element.innerText : "";
};