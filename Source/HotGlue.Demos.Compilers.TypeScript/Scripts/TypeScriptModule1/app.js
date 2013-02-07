//= require ../shared/jquery-1.7.2.js
var greeter = require('greeter.ts');

$(document).ready(function () {
    $("#container").html(greeter.greet());
});
