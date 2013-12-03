var Demo;
(function (Demo) {
    var ViewModel = (function () {
        function ViewModel() { }
        return ViewModel;
    })();    
    var Main = (function () {
        function Main() { }
        Main.prototype.run = function () {
        };
        return Main;
    })();
    Demo.Main = Main;    
})(Demo || (Demo = {}));
//@ sourceMappingURL=app.js.map
