//= require ../shared/jquery-1.7.2.js
//= require ../shared/mustache.js
var template = require('data.tmpl');

$(document).ready(function () {
    $("#container").html(template({ title: "Something Else" }));
    mod.alert();
});
