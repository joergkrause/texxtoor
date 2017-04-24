/* registry.js is part of Aloha Editor project http://aloha-editor.org
 *
 * Aloha Editor is a WYSIWYG HTML5 inline editing library and editor. 
 * Copyright (c) 2010-2012 Gentics Software GmbH, Vienna, Austria.
 * Contributors http://aloha-editor.org/contribution.php 
 * 
 * Aloha Editor is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or any later version.
 *
 * Aloha Editor is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 * 
 * As an additional permission to the GNU GPL version 2, you may distribute
 * non-source (e.g., minimized or compacted) forms of the Aloha-Editor
 * source code without the copy of the GNU GPL normally required,
 * provided you include this license notice and a URL through which
 * recipients can access the Corresponding Source.
 */
/*global define:true */
/**
 * Registry base class.
 * TODO: document that it also contains Observable.
 *
 */
var EDITOR = (function (my) {

  my.Registry = (function (){
    
    //*** THIS PART WAS "OBERVABLE"

    var _eventHandlers = null;
    
    /**
		 * Object containing the registered entries by key.
		 */
    var _entries = [];

    /**
		 * Array containing the registered ids in order
		 * of registry
		 */
    var _ids = [];
    
    function Registry(){}

    Registry.prototype = {
      /**
       * Attach a handler to an event
       *
       * @param {String} eventType A string containing the event name to bind to
       * @param {Function} handler A function to execute each time the event is triggered
       * @param {Object} scope Optional. Set the scope in which handler is executed
       */
      bind: function(eventType, handler, scope) {
        _eventHandlers = _eventHandlers || {};
        if (!this._eventHandlers[eventType]) {
          _eventHandlers[eventType] = [];
        }
        _eventHandlers[eventType].push({
          handler: handler,
          scope: (scope || window)
        });
      },

      /**
       * Remove a previously-attached event handler
       *
       * @param {String} eventType A string containing the event name to unbind
       * @param {Function} handler The function that is to be no longer executed. Optional. If not given, unregisters all functions for the given event.
       */
      unbind: function(eventType, handler) {
        _eventHandlers = _eventHandlers || {};
        if (!_eventHandlers[eventType]) {
          return;
        }
        if (!handler) {
          // No handler function given, unbind all event handlers for the eventType
          _eventHandlers[eventType] = [];
        } else {
          _eventHandlers[eventType] = $.grep(_eventHandlers[eventType], function(element) {
            if (element.handler === handler) {
              return false;
            }
            return true;
          });
        }
      },

      /**
       * Execute all handlers attached to the given event type.
       * All arguments except the eventType are directly passed to the callback function.
       *
       * @param (String} eventType A string containing the event name for which the event handlers should be invoked.
       */
      trigger: function(eventType) {
        _eventHandlers = _eventHandlers || {};
        if (!_eventHandlers[eventType]) {
          return;
        }

        // preparedArguments contains all arguments except the first one.
        var preparedArguments = [];
        $.each(arguments, function(i, argument) {
          if (i > 0) {
            preparedArguments.push(argument);
          }
        });

        $.each(_eventHandlers[eventType], function(index, element) {
          element.handler.apply(element.scope, preparedArguments);
        });
      },

      /**
       * Clears all event handlers. Call this method when cleaning up.
       */
      unbindAll: function() {
        _eventHandlers = null;
      },

      //*** END OBSERVABLE

      /**
       * Register an entry with an id
       * 
       * @event register
       * @param id id of the registered entry
       * @param entry registered entry
       */
      register: function(id, entry) {
        // TODO check whether an entry with the id is already registered
        _entries[id] = entry;
        _ids.push(id);
        this.trigger('register', entry, id);
      },

      /**
       * Unregister the entry with given id
       * 
       * @event unregister
       * @param id id of the registered entry
       */
      unregister: function(id) {
        // TODO check whether an entry was registered
        var i, oldEntry = _entries[id];
        delete _entries[id];
        for (i in _ids) {
          if (_ids.hasOwnProperty(i) && _ids[i] === id) {
            _ids.splice(i, 1);
            break;
          }
        }
        this.trigger('unregister', oldEntry, id);
      },

      /**
       * Get the entry registered with the given id
       * 
       * @param id id of the registered entry
       * @return registered entry
       */
      get: function(id) {
        return _entries[id];
      },

      /**
       * Check whether an entry was registered with given id
       * 
       * @param id id to check
       * @return true if an entry was registered, false if not
       */
      has: function(id) {
        return (_entries[id] ? true : false);
      },

      /**
       * Get an object mapping the ids (properties) to the registered entries
       * Note, that iterating over the properties of the returned object
       * will return the entries in an unspecified order
       * 
       * @return object containing the registered entries
       */
      getEntries: function() {
        // clone the entries so the user does not accidentally modify our _entries object.
        return jQuery.extend({}, _entries);
      },

      /**
       * Get the ids of the registered objects as array.
       * The array will contain the ids in order of registry
       * 
       * @return array if registered ids
       */
      getIds: function() {
        return jQuery.extend([], _ids);
      }
      
    };

    return new Registry();

  })();

  return my;

}(EDITOR || {}));