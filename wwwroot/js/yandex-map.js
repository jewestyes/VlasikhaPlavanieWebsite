document.addEventListener("DOMContentLoaded", function () {
    var script = document.createElement("script");
    script.type = "text/javascript";
    script.charset = "utf-8";
    script.async = true;
    script.src = "https://api-maps.yandex.ru/services/constructor/1.0/js/?um=constructor%3A9dc7a9176f86ac7cba00634c044a2b923ef568203026665366bebd4d4c01af17&amp;width=100%25&amp;height=100%25&amp;lang=en_FR&amp;scroll=true";
    document.getElementById("yandex-map").appendChild(script);
});
