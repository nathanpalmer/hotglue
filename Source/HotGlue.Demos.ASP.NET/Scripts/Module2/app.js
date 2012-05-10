//= require dep1.js

var mod = require('mod.js');
var template = require('data.tmpl');

$(document).ready(function () {
    $("#container").html(template({title:"Something Else"}));
    mod.alert();
});
