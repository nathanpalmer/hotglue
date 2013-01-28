//= require ../shared/jquery-1.7.2.js

//= require ../shared/jquery.tmpl.js
var jqueryTemplate = require('data.tmpl-jquery');

//= require ../shared/mustache.js
var mustacheTemplate = require('data.tmpl-mustache');

//= require ../shared/handlebars.js
var handlebarsTemplate = require('data.tmpl-handlebars');

//= require ../shared/dust-full-0.3.0.js
var dustTemplate = require('data.tmpl-dust');

//= require ../shared/underscore.js
var underscoreTemplate = require('data.tmpl-underscore');

//= require ../shared/ejs.js
var ejsTemplate = require('data.tmpl-ejs');

//= require ../shared/jquery.nano.js
var nanoTemplate = require('data.tmpl-nano');

//= require ../shared/jsrender.js
var jsrenderTemplate = require('data.tmpl-jsrender');


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

    tryRender("#underscore", function () { return underscoreTemplate({ title: title }); });
    tryRender("#ejs", function () { return ejsTemplate({ title: title }); });
    tryRender("#nano", function () { return nanoTemplate({ title: title }); });
    tryRender("#jsrender", function () { return jsrenderTemplate({ title: title }); });
});
