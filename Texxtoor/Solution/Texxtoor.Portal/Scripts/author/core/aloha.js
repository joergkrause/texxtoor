/* aloha.js is part of Aloha Editor project http://aloha-editor.org
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
var EDITOR = (function (my) {

  my.Initialization = {
    /**
     * A list of all stages that are passed into the Initialization.start()
     * function.  Unless failure happens, every single one of these phases
     * will be passed.
     *
     * @type {Array.<object>}
     */
    phases: [],

    /**
     * Completed phases.
     *
     * This array grows as the initialization process progresses through
     * the initialization phases.  Each phases which is completed is pushed
     * to the bottom of the list.
     *
     * @type {Array.<object>}
     */
    completed: [],

    /**
     * Starts the initialization phases.
     *
     * @param {object.<object>} phases Initialization phases.
     * @param {function} callback Callback function to be invoked when
     *                            initialization is completed.
     */
    start: function (phases, callback) {
      my.Initialization.phases = my.Initialization.phases.concat(phases);
      my.Initialization.proceed(0, phases, callback);
    },

    /**
     * Proceeds to next initialization phase.
     *
     * @param {number} index The current initialization phase, as an index
     *                       into `phases'.
     * @param {Array.<object>} phases
     * @param {function=} callback Callback function to invoke at the end
     *                             of the initialization phases.
     */
    proceed: function (index, phases, callback) {
      if (index < phases.length) {
        var phase = phases[index];
        var next = function () {
          my.Initialization.proceed(++index, phases, callback);
        };
        var event = function () {
          my.Initialization.completed.push(phase);
          if (phase.event) {
            trigger(phase.event);
          }
        };
        if (phase.fn) {
          phase.fn(event, next);
        } else {
          event();
          next();
        }
      } else if (callback) {
        callback();
      }
    },

    /**
     * Retreives an phase object whose `event' property string matches the
     * given event name.
     *
     * @param {string} event The event name.
     * @return {object} A phase object or null.
     */
    getPhaseByEvent: function (event) {
      var i;
      for (i = 0; i < my.Initialization.phases.length; i++) {
        if (event === my.Initialization.phases[i].event) {
          return my.Initialization.phases[i];
        }
      }
      return null;
    },

    /**
     * Given and the name of an event, returns a corresponding readiness
     * state concerning what should be done with that event at the current
     * stage in the initialization phase.
     *
     * @param {string} event Name of event.
     * @return {string} One of either "immediate", "deferred", or "normal".
     */
    getReadiness: function (event) {
      var i;
      for (i = 0; i < my.Initialization.completed.length; i++) {
        if (event === my.Initialization.completed[i].event) {
          return 'immediate';
        }
      }
      return my.Initialization.getPhaseByEvent(event) ? 'deferred'
        : 'normal';
    }
  }; // end Initialization

  /**
   * Merges properites of all given arguments into a new one.
   * Duplicate properties will be "seived" out.
   * Works in a similar way to $.extend.
   * Necessary because we must not assume that jquery was already
   * loaded.
   */

  my.mergeObjects = function () {
    var clone = {};
    var objects = Array.prototype.slice.call(arguments);
    var name;
    var i;
    var obj;
    for (i = 0; i < objects.length; i++) {
      obj = objects[i];
      for (name in obj) {
        if (obj.hasOwnProperty(name)) {
          clone[name] = objects[i][name];
        }
      }
    }
    return clone;
  };

  my.Features = {};
  my.Defaults = {};
  my.Settings = my.settings || {};

  /**
   *
   * @param {string} event Name of event
   * @param {function} fn Event handler
   */
  my.bind = function (event, fn) {
    switch (my.Initialization.getReadiness(event)) {
      case 'deferred':
        var phase = my.Initialization.getPhaseByEvent(event);
        if (!phase.deferred) {
          phase.deferred = $.Deferred();
        }
        phase.deferred.done(fn);
        break;
      case 'immediate':
        fn();
        break;
      case 'normal':
        $(document, 'body').bind(event, fn);
        break;
      default:
        throw 'Unknown readiness';
    }
    return this;
  };


  my.trigger = function (type, data) {
    var phase = my.Initialization.getPhaseByEvent(type);
    if (phase) {
      if (phase.deferred) {
        $(phase.deferred.resolve);
        phase.deferred = null;
      }
    }
    $(document, 'body').trigger(type, data);
    return this;
  };

  my.unbind = function (typeOrEvent) {
    $(document, 'body').unbind(typeOrEvent);
    return this;
  };

  my.ready = function (fn) {
    this.bind('aloha-ready', fn);
    return this;
  };

  return my;

}(EDITOR || {}));