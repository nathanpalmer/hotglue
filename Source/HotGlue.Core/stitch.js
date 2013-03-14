if (typeof (__hotglue_assets) === 'undefined') __hotglue_assets = {};
(function (assets) {
    if (!this.require) {
        var index = { },
            modules = { };
        
        this.cache = { };
        this.require = function (name) {
            var key = index[name],
                module = cache[key];
            var fn;
            
            if (module) {
                return module;
            } else if (fn = modules[key]) {
                module = { id: key, exports: {} };
                try {
                    cache[key] = module.exports;
                    fn(module.exports, function (name) {
                        return require(name);
                    }, module);
                    return cache[key] = module.exports;
                } catch (err) {
                    delete cache[key];
                    throw err;
                }
            } else {
                throw 'module \'' + name + '\' not found';
            }
        };
        this.generate = this.require;
        this.require.define = function (bundle) {
            for (var name in bundle) {
                modules[name] = bundle[name].item;
                for(var i in bundle[name].keys) {
                    var key = bundle[name].keys[i];
                    index[key] = name;
                }
            }
        };
    }
    return this.require.define;
}).call(this)(__hotglue_assets)