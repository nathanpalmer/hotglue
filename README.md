# HotGlue

HotGlue makes working with smaller, modular JavaScript in an ASP.NET application easier and with no manual configuration.

# Usage

You reference your main JavaScript file.

**Page.aspx**

    <%= HotGlue.Script.Reference("app.js") >
    
In your main JavaScript file you reference others.

**app.js**

    /// <reference path="log.js"/>

    log('Logging works everywhere, thanks Paul Irish!');    
    
**log.js**

	// usage: log('inside coolFunc',this,arguments);
	// http://paulirish.com/2009/log-a-lightweight-wrapper-for-consolelog/
	window.log = function(){
  		log.history = log.history || [];   // store logs to an array for reference
		log.history.push(arguments);
		if(this.console){
			console.log( Array.prototype.slice.call(arguments) );
		}
	};
	
The references get pulled in the correct order.

**HTML (debug)**

	<script src="/Scripts/log.js" />
	<script src="/Scripts/app.js" />
	
When working with smaller more modular JavaScript files this can easily lead to many, many files. When running in production (with debug=false) they are automatically concatenated.

**HTML**

	<script src="/Scripts/app.js-glue" />

# Modules

HotGlue also supports, and recommends, the use of [Common-JS style modules](http://wiki.commonjs.org/wiki/Modules/1.1). This is taken from their examples and works out of the box with HotGlue (you need to use .js on the refs.)

**program.js**

	var inc = require('increment.js').increment;
	var a = 1;
	inc(a); // 2
	
**increment.js**

	var add = require('math.js').add;
	exports.increment = function(val) {
		return add(val, 1);
	}
	
**math.js**

	exports.add = function() {
    	var sum = 0, i = 0, args = arguments, l = args.length;
    	while (i < l) {
        	sum += args[i++];
    	}
    	return sum;
	};
	
**HTML**

	<script src="/Scripts/math.js-module" />
	<script src="/Scripts/increment.js-module" />
	<script src="/Scripts/get.js-require" />
	<script src="/Scripts/program.js" />
	
It also properly handles asyncronously loading scripts and waiting for them to load based on the dependency tree. This is currently only implemented in the LAB.js script reference service.

**HTML**

    $LAB
       .script('dep1.js')
       .script('dep2.js').wait()
       .script('module1.js').wait()
       .script('program.js');
	
# Services

HotGlue is separated into services that provide the functionality you see here.

## Find References
*Interface: IFindReference*

*Using the keyword 'library' will include the file, but the file will not be parsed for additional references.*

### TripleSlashReference

       /// <reference path="dep.js"/>
       /// <reference path="jquery.js" library/>
       
### SlashSlashReference
  
       //= require dep.js
       //= library jquery.js
      
   OR
      
       #= require dep.js 
       #= library jquery.js
       
### RequireReference
 
       var module = require('module.js');

## Referencers
*Interface: IGenerateScriptReference*

### HTMLGenerateScriptReference
 
       <script src="module.js"/>
       
### LABjsScriptReference
 
       .script("module.js")
       
   OR
   
       .script("module.js").wait()
   
## Compilers
*Interface: ICompile*

Currently there are 4 compilers built into HotGlue. Two of them are enabled by default.

### JQueryTemplateCompiler
 
Enabled by default. Allows you to create this.
 
 **widget.tmpl**
 
 	<div>
 		${name}
 	</div>
 
 and use it like this
 
 **app.js**
 
 	var template = require('widget.tmpl');
 	
 	$("#container").html(template({ name: "Awesome" }));
 
### CoffeeScriptCompiler

Well, this compiles CoffeeScript. Enabled by default.

**module.coffee**

	number = -42 if opposite
	
**compiled module.coffee-module**

	if (opposite) {
  		number = -42;
	}

### UglifyCompressor

This uses the uglify compressor to minify.

### YUICompressor

This uses the YUICompressor to minify.

# Configuration
The default configuration of HotGlue should work for MOST cases. If it doesn't you can create a configuration section and override a lot of things.

Add the hotglue section to Web.config.

	<configuration>
  		<configSections>
    		<section name="hotglue" type="HotGlue.Web.HotGlueConfigurationSection, HotGlue.Web"/>
	    </configSections>
	    …
	</configuration>

Then throw in some config! In all of these cases you can omit a section and the default will be used. You do NOT have to override all in order to make a small change.

	<hotglue>
		<!-- Where it looks for JavaScript files -->
		<scriptPath>/Scripts</scriptPath>

		<!-- If the script file isn't found in scriptPath it will also look int scriptSharedPath -->
		<scriptSharedPath/Scripts/Shared</scriptSharedPath>
		
		<!-- Service that generates the script references -->
		<generate type="HotGlue.HTMLGenerateScriptReference, HotGlue.Core"/>
		
		<!-- Compilers that each file runs through -->
        <compilers>
        	<compiler extension=".tmpl" type="HotGlue.Compilers.JQueryTemplateCompiler, HotGlue.Core"/>
      		<compiler extension=".coffee" type="HotGlue.Compilers.CoffeeScriptCompiler, HotGlue.Core"/>
      		<compiler extension=".js" type="HotGlue.Compilers.UglifyCompressor, HotGlue.Core" mode="release"/>
      		<compiler extension=".js" type="HotGlue.Compilers.YUICompressor, HotGlue.Core" mode="release"/>
	    </compilers>

	    <!-- References that are used to find references -->
		<referencers>
			<reference type="HotGlue.RequireReference, HotGlue.Core"/>
			<reference type="HotGlue.SlashSlashEqualReference, HotGlue.Core"/>
			<reference type="HotGlue.TripleSlashReference, HotGlue.Core"/>
		</referencers>
	</hotglue>

# Install

You need to reference HotGlue.Core and HotGlue.Web. Then add the http handlers to your Web.config.

**Web.config**

	<system.web>
        <compilation debug="true" targetFramework="4.0"/>
        <httpHandlers>
            <add type="HotGlue.Web.HotGlueHandler, HotGlue.Web, Version=1.0.0.0, Culture=neutral" verb="GET" path="*.js-glue"/>
            <add type="HotGlue.Web.HotGlueModuleHandler, HotGlue.Web" verb="GET" path="*.*-module"/>
            <add type="HotGlue.Web.HotGlueRequireHandler, HotGlue.Web" verb="GET" path="*.js-require"/>
        </httpHandlers>
    </system.web>
    <system.webServer>
        <handlers>
            <add name="HotGlue" type="HotGlue.Web.HotGlueHandler, HotGlue.Web, Version=1.0.0.0, Culture=neutral" verb="GET" path="*.js-glue"/>
            <add name="HotGlueModule" type="HotGlue.Web.HotGlueModuleHandler, HotGlue.Web" verb="GET" path="*.*-module"/>
            <add name="HotGlueRequire" type="HotGlue.Web.HotGlueRequireHandler, HotGlue.Web" verb="GET" path="*.js-require"/>
        </handlers>
    </system.webServer>
