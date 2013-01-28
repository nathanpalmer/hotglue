//= require ../shared/jquery-1.7.2.js
//= require ../shared/jquery.tmpl.js
//= require ../shared/mustache.js
//= require ../shared/handlebars.js
var jqueryTemplate = require('data.tmpl-jquery');
var mustacheTemplate = require('data.tmpl-mustache');
var handlebarsTemplate = require('data.tmpl-handlebars');

var title = "Hello, World";

$(document).ready(function () {
    $("#jquery").html(jqueryTemplate({ title: title }));
    $("#mustache").html(mustacheTemplate({ title: title }));
    $("#handlebars").html(handlebarsTemplate({ title: title }));
});
