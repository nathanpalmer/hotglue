//= require ../shared/jquery-1.7.2.js
//= require ../shared/jquery.tmpl.js
//= require ../shared/mustache.js
//= require ../shared/handlebars.js
//= require ../shared/dust-full-0.3.0.js
var jqueryTemplate = require('data.tmpl-jquery');
var mustacheTemplate = require('data.tmpl-mustache');
var handlebarsTemplate = require('data.tmpl-handlebars');
var dustTemplate = require('data.tmpl-handlebars');

var title = "Hello, World";

$(document).ready(function () {
    $("#jquery").html(jqueryTemplate({ title: title }));
    $("#mustache").html(mustacheTemplate({ title: title }));
    $("#handlebars").html(handlebarsTemplate({ title: title }));
    $("#dust").html(dustTemplate({ title: title }));
});
