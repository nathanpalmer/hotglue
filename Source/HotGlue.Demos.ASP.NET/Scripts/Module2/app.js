//= require jquery-1.7.2.js
//= require jquery.tmpl.js
//= require dep1.js

var mod = require('mod.js');
var template = require('data.tmpl');

$(document).ready(function () {
    $("#container").html(template({title:"Something Else"}));
    mod.alert();
});
