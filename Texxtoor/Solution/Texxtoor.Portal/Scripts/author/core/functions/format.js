/* format.js is part of Aloha Editor project http://aloha-editor.org
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

  my.Format = (function () {

    var commandsByElement = {
      'b': 'bold',
      'strong': 'bold',
      'i': 'italic',
      'em': 'italic',
      'del': 'strikethrough',
      'sub': 'subscript',
      'sup': 'superscript',
      'u': 'underline',
      's': 'strikethrough'
    };
    var componentNameByElement = {
      'strong': 'strong',
      'em': 'emphasis',
      's': 'strikethrough2'
    };
    var textLevelSemantics = {
      'u': true,
      'em': true,
      'strong': true,
      'b': true,
      'i': true,
      'cite': true,
      'q': true,
      'code': true,
      'abbr': true,
      'del': true,
      's': true,
      'sub': true,
      'sup': true
    };
    var blockLevelSemantics = {
      'p': true,
      'h1': true,
      'h2': true,
      'h3': true,
      'h4': true,
      'h5': true,
      'h6': true,
      'pre': true
    };
    var interchangeableNodeNames = {
      "B": ["STRONG", "B"],
      "I": ["EM", "I"],
      "STRONG": ["STRONG", "B"],
      "EM": ["EM", "I"]
    };

    function formatInsideTableWorkaround(button) {
      var selectedCells = $('.aloha-cell-selected');
      if (selectedCells.length > 0) {
        var cellMarkupCounter = 0;
        selectedCells.each(function () {
          var cellContent = $(this).find('div'),
          cellMarkup = cellContent.find(button);
          if (cellMarkup.length > 0) {
            // unwrap all found markup text
            // <td><b>text</b> foo <b>bar</b></td>
            // and wrap the whole contents of the <td> into <b> tags
            // <td><b>text foo bar</b></td>
            cellMarkup.contents().unwrap();
            cellMarkupCounter++;
          }
          cellContent.contents().wrap('<' + button + '></' + button + '>');
        });

        // remove all markup if all cells have markup
        if (cellMarkupCounter === selectedCells.length) {
          selectedCells.find(button).contents().unwrap();
        }
        return true;
      }
      return false;
    }

    function textLevels(formatPlugin, button) {
      if (formatInsideTableWorkaround(button)) {
        return false;
      }
      formatPlugin.addMarkup(button);
      return false;
    }

    function blockLevel(formatPlugin, button) {
      if (formatInsideTableWorkaround(button)) {
        return false;
      }

      formatPlugin.changeMarkup(button);

      // setting the focus is needed for mozilla to have a working rangeObject.select()
      if (my.Core.activeEditable && $.browser.mozilla) {
        my.Core.activeEditable.obj.focus();
      }

      // triggered for numerated-headers plugin
      if (my.Core.activeEditable) {
        my.Core.trigger('aloha-format-block');
      }
    }

    function changeMarkup(button) {
      my.Selection.changeMarkupOnSelection($('<' + button + '>'));
    }

    function updateUiAfterMutation(formatPlugin, rangeObject) {
      // select the modified range
      rangeObject.select();
      // update Button toggle state. We take 'Aloha.Selection.getRangeObject()'
      // because rangeObject is not up-to-date
      onSelectionChanged(formatPlugin, my.Selection.getRangeObject());
    }

    function format(formatPlugin, rangeObject, markup) {
      my.Dom.addMarkup(rangeObject, markup);
      updateUiAfterMutation(formatPlugin, rangeObject);
    }

    function addMarkup(button) {
      var formatPlugin = this;
      var markup = $('<' + button + '>');
      var rangeObject = my.Selection.rangeObject;

      if (typeof button === "undefined" || button == "") {
        return;
      }

      // check whether the markup is found in the range (at the start of the range)
      var nodeNames = interchangeableNodeNames[markup[0].nodeName] || [markup[0].nodeName];
      var foundMarkup = rangeObject.findMarkup(function () {
        return -1 !== my.Arrays.indexOf(nodeNames, this.nodeName);
      }, my.Core.activeEditable.obj);

      if (foundMarkup) {
        // remove the markup
        if (rangeObject.isCollapsed()) {
          // when the range is collapsed, we remove exactly the one DOM element
          my.Dom.removeFromDOM(foundMarkup, rangeObject, true);
        } else {
          // the range is not collapsed, so we remove the markup from the range
          my.Dom.removeMarkup(rangeObject, $(foundMarkup), Core.activeEditable.obj);
        }
        updateUiAfterMutation(formatPlugin, rangeObject);
      } else {
        // when the range is collapsed, extend it to a word
        if (rangeObject.isCollapsed()) {
          my.Dom.extendToWord(rangeObject);
          if (rangeObject.isCollapsed()) {
            if (StateOverride.enabled()) {
              StateOverride.setWithRangeObject(
                commandsByElement[button],
                rangeObject,
                function (command, rangeObject) {
                  format(formatPlugin, rangeObject, markup);
                }
              );
              return;
            }
          }
        }
        format(formatPlugin, rangeObject, markup);
      }
    }

    function onSelectionChanged(formatPlugin, rangeObject) {
      var effectiveMarkup,
          foundMultiSplit, i, j, multiSplitItem;

      $.each(formatPlugin.buttons, function (index, button) {
        var statusWasSet = false;
        var nodeNames = interchangeableNodeNames[button.markup[0].nodeName] || [button.markup[0].nodeName];
        for (i = 0; i < rangeObject.markupEffectiveAtStart.length; i++) {
          effectiveMarkup = rangeObject.markupEffectiveAtStart[i];
          for (j = 0; j < nodeNames.length; j++) {
            if (Selection.standardTextLevelSemanticsComparator(effectiveMarkup, $('<' + nodeNames[j] + '>'))) {
              button.handle.setState(true);
              statusWasSet = true;
            }
          }
        }
        if (!statusWasSet) {
          button.handle.setState(false);
        }
      });

      if (formatPlugin.multiSplitItems.length > 0) {
        foundMultiSplit = false;

        // iterate over the markup elements
        for (i = 0; i < rangeObject.markupEffectiveAtStart.length && !foundMultiSplit; i++) {
          effectiveMarkup = rangeObject.markupEffectiveAtStart[i];

          for (j = 0; j < formatPlugin.multiSplitItems.length && !foundMultiSplit; j++) {
            multiSplitItem = formatPlugin.multiSplitItems[j];

            if (!multiSplitItem.markup) {
              continue;
            }

            // now check whether one of the multiSplitItems fits to the effective markup
            if (Selection.standardTextLevelSemanticsComparator(effectiveMarkup, multiSplitItem.markup)) {
              formatPlugin.multiSplitButton.setActiveItem(multiSplitItem.name);
              foundMultiSplit = true;
            }
          }
        }

        if (!foundMultiSplit) {
          formatPlugin.multiSplitButton.setActiveItem(null);
        }
      }
    }

    /**
     * register the plugin with unique name
     */
    return {
      /**
       * default button configuration
       */
      config: ['b', 'i', 'sub', 'sup', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'pre', 'removeFormat'],

      /**
       * available options / buttons
       * 
       * @todo new buttons needed for 'code'
       */
      availableButtons: ['u', 'strong', 'del', 'em', 'b', 'i', 's', 'sub', 'sup', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'pre', 'removeFormat'],

      /**
       * HotKeys used for special actions
       */
      hotKey: {
        formatBold: 'ctrl+b',
        formatItalic: 'ctrl+i',
        formatParagraph: 'alt+ctrl+0',
        formatH1: 'alt+ctrl+1',
        formatH2: 'alt+ctrl+2',
        formatH3: 'alt+ctrl+3',
        formatH4: 'alt+ctrl+4',
        formatH5: 'alt+ctrl+5',
        formatH6: 'alt+ctrl+6',
        formatPre: 'ctrl+p',
        formatDel: 'ctrl+d',
        formatSub: 'alt+shift+s',
        formatSup: 'ctrl+shift+s'
      },

      /**
       * Initialize the plugin and set initialize flag on true
       */
      init: function () {
        // Prepare
        var me = this;

        if (typeof this.settings.hotKey !== 'undefined') {
          $.extend(true, this.hotKey, this.settings.hotKey);
        }

        // apply specific configuration if an editable has been activated
        bind('aloha-editable-activated', function (e, params) {
          me.applyButtonConfig(params.editable.obj);

          // handle hotKeys
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatBold, function () { me.addMarkup('b'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatItalic, function () { me.addMarkup('i'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatParagraph, function () { me.changeMarkup('p'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatH1, function () { me.changeMarkup('h1'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatH2, function () { me.changeMarkup('h2'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatH3, function () { me.changeMarkup('h3'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatH4, function () { me.changeMarkup('h4'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatH5, function () { me.changeMarkup('h5'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatH6, function () { me.changeMarkup('h6'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatPre, function () { me.changeMarkup('pre'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatDel, function () { me.addMarkup('del'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatSub, function () { me.addMarkup('sub'); return false; });
          params.editable.obj.bind('keydown.aloha.format', me.hotKey.formatSup, function () { me.addMarkup('sup'); return false; });
        });

        bind('aloha-editable-deactivated', function (e, params) {
          params.editable.obj.unbind('keydown.aloha.format');
        });
      },

      /**
       * applys a configuration specific for an editable
       * buttons not available in this configuration are hidden
       * @param {Object} id of the activated editable
       * @return void
       */
      applyButtonConfig: function (obj) {
        var config = this.getEditableConfig(obj),
            button, i, len;

        if (typeof config === 'object') {
          var config_old = [];
          $.each(config, function (j, button) {
            if (!(typeof j === 'number' && typeof button === 'string')) {
              config_old.push(j);
            }
          });

          if (config_old.length > 0) {
            config = config_old;
          }
        }
        this.formatOptions = config;

        // now iterate all buttons and show/hide them according to the config
        for (button in this.buttons) {
          if (this.buttons.hasOwnProperty(button)) {
            if ($.inArray(button, config) !== -1) {
              this.buttons[button].handle.show();
            } else {
              this.buttons[button].handle.hide();
            }
          }
        }

        // and the same for multisplit items
        len = this.multiSplitItems.length;
        for (i = 0; i < len; i++) {
          if ($.inArray(this.multiSplitItems[i].name, config) !== -1) {
            this.multiSplitButton.showItem(this.multiSplitItems[i].name);
          } else {
            this.multiSplitButton.hideItem(this.multiSplitItems[i].name);
          }
        }
      },

      // duplicated code from link-plugin
      //Creates string with this component's namepsace prefixed the each classname
      nsClass: function () {
        var stringBuilder = [], prefix = pluginNamespace;
        $.each(arguments, function () {
          stringBuilder.push(this == '' ? prefix : prefix + '-' + this);
        });
        return $.trim(stringBuilder.join(' '));
      },

      // duplicated code from link-plugin
      nsSel: function () {
        var stringBuilder = [], prefix = pluginNamespace;
        $.each(arguments, function () {
          stringBuilder.push('.' + (this == '' ? prefix : prefix + '-' + this));
        });
        return $.trim(stringBuilder.join(' '));
      },

      addMarkup: addMarkup,
      changeMarkup: changeMarkup,

      /**
       * Removes all formatting from the current selection.
       */
      removeFormat: function () {
        var formats = ['u', 'strong', 'em', 'b', 'i', 'q', 'del', 's', 'code', 'sub', 'sup', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'pre', 'quote', 'blockquote'],
            rangeObject = Selection.rangeObject,
            i;

        // formats to be removed by the removeFormat button may now be configured using Aloha.settings.plugins.format.removeFormats = ['b', 'strong', ...]
        if (this.settings.removeFormats) {
          formats = this.settings.removeFormats;
        }

        if (rangeObject.isCollapsed()) {
          return;
        }

        for (i = 0; i < formats.length; i++) {
          my.Dom.removeMarkup(rangeObject, $('<' + formats[i] + '>'), Core.activeEditable.obj);
        }

        // select the modified range
        rangeObject.select();
        // TODO: trigger event - removed Format
      },

      /**
       * toString method
       * @return string
       */
      toString: function () {
        return 'format';
      }
    };

  })();
  
  return my;

}(EDITOR || {}));