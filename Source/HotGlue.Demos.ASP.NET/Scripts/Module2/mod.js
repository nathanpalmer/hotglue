//= require dep1.js

var alertFactory = require('alert.js');

module.exports.alert = function() {
    alertFactory.alert('test');
};