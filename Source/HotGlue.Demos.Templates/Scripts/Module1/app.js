//= require ../shared/jquery-1.7.2.js
//= require ../shared/jquery.tmpl.js
//= require ../shared/mustache.js
//= require ../shared/handlebars.js
//= require ../shared/dust-full-0.3.0.js
//= require ../shared/underscore.js
var jqueryTemplate = require('data.tmpl-jquery');
var mustacheTemplate = require('data.tmpl-mustache');
var handlebarsTemplate = require('data.tmpl-handlebars');
var dustTemplate = require('data.tmpl-dust');
var underscoreTemplate = require('data.tmpl-underscore');

var title = "Hello, World";

function tryRender(element, template) {
    try {
        $(element).html(template());
    } catch (e) {
        $(element).html(e.toString());
        console.error(e);
    } 
}

$(document).ready(function () {
    tryRender("#jquery", function() { return jqueryTemplate({ title: title }); });
    tryRender("#mustache", function() { return mustacheTemplate({ title: title }); });
    tryRender("#handlebars", function() { return handlebarsTemplate({ title: title }); });
    
    try {
        dustTemplate({ title: title }, function (error, output) {
            if (error) {
                $("#dust").html(error.message);
            } else {
                $("#dust").html(output);
            }
        });
    } catch (e) {
        $("#dust").html(e.toString());
        console.error(e);
    }


    tryRender("#underscore", function() { return underscoreTemplate({ title: title }); });
});
