mod4 = require('mod5.coffee')
#= require spine.js

class App extends Spine.Controller
    constructor: ->
        super
        @log("Initialized")

module.exports = App