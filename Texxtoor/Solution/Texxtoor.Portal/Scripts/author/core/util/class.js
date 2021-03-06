﻿/* Simple JavaScript Inheritance
 * By John Resig http://ejohn.org/
 * MIT Licensed.
 */
// Inspired by base2 and Prototype
/*
 * MODIFICATIONS: 
 * * The name of the "constructor" method was changed from "init" to "_constructor"
 * * Mixin Support using https://gist.github.com/1006243
 * * Modified to be a require.js module
 */
var EDITOR = (function (my) {

  my.Class = (function () {
    var initializing = false,
      fnTest = /xyz/.test(function () {
        xyz;
      }) ? /\b_super\b/ : /.*/;

    // The base Class implementation (does nothing)
    // with doing that Class is available in the global namespace.
    this.clss = function () { };

    // Create a new Class that inherits from this class
    clss.extend = function () {
      var _super = this.prototype;

      // Instantiate a base class (but only create the instance,
      // don't run the init constructor)
      initializing = true;
      var prototype = new this();
      initializing = false;

      // Copy the properties over onto the new prototype
      for (var i = 0; i < arguments.length; i++) {
        var prop = arguments[i];
        for (var name in prop) {
          // Check if we're overwriting an existing function
          prototype[name] = typeof prop[name] == "function" && typeof _super[name] == "function" && fnTest.test(prop[name]) ? (function (name, fn) {
            return function () {

              var tmp = this._super;

              // Add a new ._super() method that is the same method
              // but on the super-class
              this._super = _super[name];

              // The method only need to be bound temporarily, so we
              // remove it when we're done executing
              var ret = fn.apply(this, arguments);
              this._super = tmp;

              return ret;
            };
          })(name, prop[name]) : prop[name];
        }
      }

      // The dummy class constructor
      function clss() {
        // All construction is actually done in the _constructor method
        if (!initializing && this._constructor) this._constructor.apply(this, arguments);
      }

      // Populate our constructed prototype object
      clss.prototype = prototype;

      // Enforce the constructor to be what we expect
      clss.constructor = my.Class;

      // And make this class extendable
      clss.extend = arguments.callee;

      return clss;

    };

    return this.clss;

  })();


  return my;

}(EDITOR || {}));