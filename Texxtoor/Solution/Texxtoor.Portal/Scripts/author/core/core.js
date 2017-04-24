/*core.js is part of Aloha Editor project http://aloha-editor.org
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

  my.Core = (function () {

    /**
     * Checks whether the current user agent is supported.
     *
     * @return {boolean} True if Aloha supports the current browser.
     */
    function isBrowserSupported() {
      var browser = $.browser;
      var version = browser.version;
      return !(
        // Chrome/Safari 4
        (browser.webkit && parseFloat(version) < 532.5) ||
        // FF 3.5
        (browser.mozilla && parseFloat(version) < 1.9) ||
        // IE 7
        (browser.msie && version < 7) ||
        // Right now Opera needs some work
        (browser.opera && version < 11)
      );
    }

    /**
     * Checks whether the given jQuery event originates from an Aloha dialog
     * element.
     *
     * This is used to facilitate a hackish way of preventing blurring
     * editables when interacting with Aloha UI modals.
     *
     * @param {jQuery<Event>} $event
     * @return {boolean} True if $event is initiated from within an Aloha
     *                   dialog element.
     */
    function originatesFromDialog($event) {
      var $target = $($event.target);
      return $target.is('.aloha-dialog')
        || $target.closest('.aloha').length;
    }

    /**
     * Registers events on the documents to cause editables to be blurred when
     * clicking outside of editables.
     *
     * Hack: Except when the click originates from a modal dialog.
     */
    function registerEvents() {
      $('html').mousedown(function ($event) {
        if (my.Core.activeEditable && !my.Core.eventHandled
            && !originatesFromDialog($event)) {
          my.Core.deactivateEditable();
        }
      }).mouseup(function () {
        my.Core.eventHandled = false;
      });
    }

    /**
     * Initialize Aloha.
     *
     * @param {function} event Event to trigger after completing tasks.
     * @param {function} next Function to call after completing tasks.
     */
    function initAloha(event, next) {
      if (!isBrowserSupported()) {
        var console = window.console;
        if (console) {
          var fn = console.error ? 'error' : console.log ? 'log' : null;
          if (fn) {
            console[fn]('This browser is not supported');
          }
        }
        return;
      }

      // Because different css is to be applied based on what the user-agent
      // supports.  For example: outlines do not render in IE7.
      if ($.browser.webkit) {
        $('html').addClass('aloha-webkit');
      } else if ($.browser.opera) {
        $('html').addClass('aloha-opera');
      } else if ($.browser.msie) {
        $('html').addClass('aloha-ie' + parseInt($.browser.version, 10));
      } else if ($.browser.mozilla) {
        $('html').addClass('aloha-mozilla');
      }

      if (navigator.appVersion.indexOf('Win') !== -1) {
        my.Core.OSName = 'Win';
      } else if (navigator.appVersion.indexOf('Mac') !== -1) {
        my.Core.OSName = 'Mac';
      } else if (navigator.appVersion.indexOf('X11') !== -1) {
        my.Core.OSName = 'Unix';
      } else if (navigator.appVersion.indexOf('Linux') !== -1) {
        my.Core.OSName = 'Linux';
      }

      registerEvents();
      Settings.base = my.Core.getAlohaUrl();
      console.init();

      // Initialize error handler for general javascript errors.
      if (Settings.errorhandling) {
        window.onerror = function (msg, url, line) {
          console.error(Engine, 'Error message: ' + msg + '\nURL: ' +
                                 url + '\nLine Number: ' + line);
          return true;
        };
      }

      event();
      next();
    }


    /**
     * Begin initialize editables.
     *
     * @param {function} event Event to trigger after completing tasks.
     * @param {function} next Function to call after completing tasks.
     */
    function initEditables(event, next) {
      var i;
      for (i = 0; i < my.Core.editables.length; i++) {
        if (!my.Core.editables[i].ready) {
          my.Core.editables[i].init();
        }
      }
      event();
      next();
    }

    return {

      initEditables: initEditables,
      initAloha: initAloha,

      /**
       * The Aloha Editor Version we are using
       * It should be set by us and updated for the particular branch
       * @property
       */
      version: '0.23.12',

      /**
       * Array of editables that are managed by Aloha
       * @property
       * @type Array
       */
      editables: [],

      /**
       * The currently active editable is referenced here
       * @property
       * @type Aloha.Editable
       */
      activeEditable: null,

      /**
       * settings object, which will contain all Aloha settings
       * @cfg {Object} object Aloha's settings
       */
      settings: {},

      /**
       * defaults object, which will contain all Aloha defaults
       * @cfg {Object} object Aloha's settings
       */
      defaults: {},

      /**
       * Namespace for ui components
       */
      ui: {},

      /**
       * This represents the name of the users OS. Could be:
       * 'Mac', 'Linux', 'Win', 'Unix', 'Unknown'
       * @property
       * @type string
       */
      OSName: 'Unknown',

      /**
       * A list of loaded plugin names, available after the STAGES.PLUGINS
       * initialization phase.
       *
       * @type {Array.<string>}
       * @internal
       */
      loadedPlugins: [],

      /**
       * Maps names of plugins (link) to the base URL (../plugins/common/link).
       */
      _pluginBaseUrlByName: {},

      /**
       * Start the initialization process.
       */
      init: function () {
        my.Engine.initialize(phases);
      },


      /**
       * Activates editable and deactivates all other Editables.
       *
       * @param {Editable} editable the Editable to be activated
       */
      activateEditable: function (editable) {
        // Because editables may be removed on blur, Aloha.editables.length
        // is not cached.
        var editables = my.Core.editables;
        var i;
        for (i = 0; i < editables.length; i++) {
          if (editables[i] !== editable && editables[i].isActive) {
            editables[i].blur();
          }
        }
        my.Core.activeEditable = editable;
      },

      /**
       * Returns the current Editable
       * @return {Editable} returns the active Editable
       */
      getActiveEditable: function () {
        return my.Core.activeEditable;
      },

      /**
       * Deactivates the active Editable.
       *
       * TODO: Would be better named "deactivateActiveEditable".
       */
      deactivateEditable: function () {
        if (my.Core.activeEditable) {
          my.Core.activeEditable.blur();
          my.Core.activeEditable = null;
        }
      },

      /**
       * Gets an editable by an ID or null if no Editable with that ID
       * registered.
       *
       * @param {string} id The element id to look for.
       * @return {Aloha.Editable|null} An editable, or null if none if found
       *                               for the given id.
       */
      getEditableById: function (id) {
        // Because if the element is a textarea, then it's necessary to
        // route to the editable div.
        var $editable = $('#' + id);
        if ($editable.length
            && 'textarea' === $editable[0].nodeName.toLowerCase()) {
          id = id + '-aloha';
        }
        var i;
        for (i = 0; i < my.Core.editables.length; i++) {
          if (my.Core.editables[i].getId() === id) {
            return my.Core.editables[i];
          }
        }
        return null;
      },

      /**
       * Checks whether an object is a registered Aloha Editable.
       * @param {jQuery} obj the jQuery object to be checked.
       * @return {boolean}
       */
      isEditable: function (obj) {
        var i, editablesLength;

        for (i = 0, editablesLength = my.Core.editables.length; i < editablesLength; i++) {
          if (my.Core.editables[i].originalObj.get(0) === obj) {
            return true;
          }
        }
        return false;
      },

      /**
       * Gets the nearest editable parent of the DOM element contained in the
       * given jQuery object.
       *
       * @param {jQuery} $element jQuery unit set containing DOM element.
       * @return {Aloha.Editable} Editable, or null if none found.
       */
      getEditableHost: (function () {
        var getEditableOf = function (editable) {
          var i;
          for (i = 0; i < my.Core.editables.length; i++) {
            if (my.Core.editables[i].originalObj[0] === editable) {
              return my.Core.editables[i];
            }
          }
          return null;
        };

        return function ($element) {
          if (!$element || 0 === $element.length) {
            return null;
          }
          var editable = getEditableOf($element[0]);
          if (!editable) {
            $element.parents().each(function (__unused__, node) {
              editable = getEditableOf(node);
              if (editable) {
                return false;
              }
            });
          }
          return editable;
        };


      }()),

      /**
       * Logs a message to the console.
       *
       * @param {string} level Level of the log
       *                       ("error", "warn" or "info", "debug").
       * @param {object} component Component that calls the log.
       * @param {string} message Log message.
       * @hide
       */
      log: function (level, component, message) {
        if (typeof console.Log !== 'undefined') {
          console.log(level, component, message);
        }
      },

      /**
       * Register the given editable.
       *
       * @param {Editable} editable to register.
       * @hide
       */
      registerEditable: function (editable) {
        my.Core.editables.push(editable);
      },

      /**
       * Unregister the given editable. It will be deactivated and removed
       * from editables.
       *
       * @param {Editable} editable The editable to unregister.
       * @hide
       */
      unregisterEditable: function (editable) {
        var index = $.inArray(editable, my.Core.editables);
        if (index !== -1) {
          my.Core.editables.splice(index, 1);
        }
      },

      /**
       * Check whether at least one editable was modified.
       *
       * @return {boolean} True when at least one editable was modified,
       *                   false otherwise.
       */
      isModified: function () {
        var i;
        for (i = 0; i < my.Core.editables.length; i++) {
          if (my.Core.editables[i].isModified
              && my.Core.editables[i].isModified()) {
            return true;
          }
        }
        return false;
      },

      /**
       * Determines the Aloha Url.
       *
       * @return {String} Aloha's baseUrl setting or "" if not set.
       */
      getAlohaUrl: function (suffix) {
        return Settings.baseUrl || '';
      },

      /**
       * Gets the plugin's url.
       *
       * @param {string} name The name with which the plugin was registered
       *                      with.
       * @return {string} The fully qualified url of this plugin.
       */
      getPluginUrl: function (name) {
        if (!name) {
          return null;
        }
        var url = Aloha.settings._pluginBaseUrlByName[name];
        if (url) {
          // Check if url is absolute and attach base url if it is not.
          if (!url.match("^(\/|http[s]?:).*")) {
            url = this.getAlohaUrl() + '/' + url;
          }
        }
        return url;
      },

      /**
       * Disable object resizing by executing command 'enableObjectResizing',
       * if the browser supports this.
       */
      disableObjectResizing: function () {
        try {
          // This will disable browsers image resizing facilities in
          // order disable resize handles.
          var supported;
          try {
            supported = document.queryCommandSupported('enableObjectResizing');
          } catch (e) {
            supported = false;
            console.log('enableObjectResizing is not supported.');
          }
          if (supported) {
            document.execCommand('enableObjectResizing', false, false);
            console.log('enableObjectResizing disabled.');
          }
        } catch (e2) {
          console.error(e2, 'Could not disable enableObjectResizing');
          // this is just for others, who will not support disabling enableObjectResizing
        }
      },

      /**
       * Human-readable string representation of this.
       *
       * @hide
       */
      toString: function () {
        return 'Aloha';
      }
    };

  })();

  return my;

}(EDITOR || {}));