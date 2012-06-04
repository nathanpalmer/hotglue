//= require ../shared/jquery-1.7.2.js
//= require ../shared/jquery.tmpl.js
//= require ../shared/dep1.js
var mod1 = require('../module3/mod1.js');
var coffeemod = require('../coffeemodule1/mod4.coffee');

var mod = require('mod.js');
var template = require('data.tmpl');

$(document).ready(function () {
    $("#container").html(template({ title: "Something Else" }));
    mod.alert();

    var ctx = new coffeemod();
});
