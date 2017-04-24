var EDITOR = (function (my) {

  my.Utils = {

    /**
     * Takes over all properties from the 'properties' object to the target object.
     * If a property in 'target' with the same name as a property in 'properties' is already defined it is overridden.
     *
     * Example:
     *
     * var o1 = {a : 1, b : 'hello'};
     * var o2 = {a : 3, c : 'world'};
     *
     * GENTICS.Utils.applyProperties(o1, o2);
     *
     * Will result in an o1 object like this:
     *
     * {a : 3, b: 'hello', c: 'world'}
     *
     * @static
     * @return void
     */
    applyProperties: function (target, properties) {
      var name;
      for (name in properties) {
        if (properties.hasOwnProperty(name)) {
          target[name] = properties[name];
        }
      }
    },

    /**
     * Generate a unique hexadecimal string with 4 charachters
     * @return {string}
     */
    uniqueString4: function () {
      return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    },

    /**
     * Generate a unique value represented as a 32 character hexadecimal string,
     * such as 21EC2020-3AEA-1069-A2DD-08002B30309D
     * @return {string}
     */
    guid: function () {
      var s4 = this.uniqueString4;
      return (s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4());
    }

  };
  
  return my;

}(EDITOR || {}));