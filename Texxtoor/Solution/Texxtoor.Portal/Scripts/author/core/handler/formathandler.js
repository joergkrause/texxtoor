/* genericcontenthandler.js is part of Aloha Editor project http://aloha-editor.org
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

  my.FormatHandler = (function () {


    /**
     * Tags used for semantic formatting
     * @type {Array.<String>}
     * @see GenericContentHandler#transformFormattings
     */
    var formattingTags = ['strong', 'em', 's', 'u', 'strike'];

    /**
     * Checks whether the markup describes a paragraph that is propped by
     * a <br> tag but is otherwise empty.
     * 
     * Will return true for:
     *
     * <p id="foo"><br class="bar" /></p>
     *
     * as well as:
     *
     * <p><br></p>
     *
     * @param {string} html Markup
     * @return {boolean} True if html describes a propped paragraph.
     */
    function isProppedParagraph(html) {
      return my.ContentHandlerUtils.isProppedParagraph(html);
    }


    /**
     * Transforms all tables in the given content to make them ready to for
     * use with Aloha's table handling.
     *
     * Cleans tables of their unwanted attributes.
     * Normalizes table cells.
     *
     * @param {jQuery.<HTMLElement>} $content
     */
    function prepareTables($content) {
      // Because Aloha does not provide a way for the editor to
      // manipulate borders, cellspacing, cellpadding in tables.
      // @todo what about width, height?
      $content.find('table')
        .removeAttr('cellpadding')
        .removeAttr('cellspacing')
        .removeAttr('border')
        .removeAttr('border-top')
        .removeAttr('border-bottom')
        .removeAttr('border-left')
        .removeAttr('border-right');

      $content.find('td').each(function () {
        var td = this;

        // Because cells with a single empty <p> are rendered to appear
        // like empty cells, it simplifies the handeling of cells to
        // normalize these table cells to contain actual white space
        // instead.
        if (isProppedParagraph(td.innerHTML)) {
          td.innerHTML = '&nbsp;';
        }

        // Because a single <p> wrapping the contents of a <td> is
        // initially superfluous and should be stripped out.
        var $p = $('>p', td);
        if (1 === $p.length) {
          $p.contents().unwrap();
        }
      });

      // Because Aloha does not provide a means for editors to manipulate
      // these properties.
      $content.find('td,tr')
        .removeAttr('width')
        .removeAttr('height')
        .removeAttr('valign');

      // Because Aloha table handling simply does not regard colgroups.
      // @TODO Use sanitize.js?
      $content.find('colgroup').remove();
    }

    /**
     * Return true if the nodeType is allowed in the settings,
     * Aloha.settings.contentHandler.allows.elements
     * 
     * @param {String} nodeType	The tag name of the element to evaluate
     * 
     * @return {Boolean}
     */
    function isAllowedNodeName(nodeType) {
      return !!(
        my.Settings.contentHandler
        && my.Settings.contentHandler.allows
        && my.Settings.contentHandler.allows.elements
        && ($.inArray(
                    nodeType.toLowerCase(),
                my.Settings.contentHandler.allows.elements
                   ) !== -1
           )
      );
    }
    var blocksSelector = my.Html.BLOCKLEVEL_ELEMENTS.join();
    var emptyBlocksSelector = my.Html.BLOCKLEVEL_ELEMENTS.join(':empty,')
                            + ':empty';
    var NOT_ALOHA_BLOCK_FILTER = ':not(.aloha-block)';

    var isNotIgnorableWhitespace =
        my.Functions.complement(my.Html.isIgnorableWhitespace);

    /**
     * Removes the <br> tag that is at the end of the given container.
     * Invisible white spaces are ignored.
     *
     * @param {number} i Index of element in its collection. (Unused)
     * @param {HTMLElement} element The container in which to remove the <br>.
     */
    function removeTrailingBr(i, element) {
      var node = my.Html.findNodeRight(
        element.lastChild,
        isNotIgnorableWhitespace
      );
      if (node && 'br' === node.nodeName.toLowerCase()) {
        $(node).remove();
      }
    }

    /**
     * Prepares this content for editing
     *
     * @param {number} i Index of element in its collection. (Unused)
     * @param {HTMLElement} element
     */
    function prepareForEditing(i, element) {
      var $element = $(element);

      $element.filter(emptyBlocksSelector).remove();

      if ($.browser.msie) {
        // Because even though content edited by Aloha Editor is no longer
        // exported with propping <br>'s that are annotated with
        // "aloha-end-br" classes,  this clean-up still needs to be done for
        // content that was edited using legacy Aloha Editor.
        $element.filter('br.aloha-end-br').remove();

        // Because IE's Trident engine goes against W3C's HTML specification
        // by rendering empty block-level elements with height if they are
        // contentEditable.  Propping <br> elements therefore result in 2
        // lines being displayed rather than 1 (which was the intention of
        // having the propping <br> element is).  Because these empty
        // content editable block-level elements are not rendered invisibly
        // in IE, we can remove the propping <br> in otherwise empty
        // block-level elements.
        $element.filter(blocksSelector).each(removeTrailingBr);
      }

      $element.children(NOT_ALOHA_BLOCK_FILTER).each(prepareForEditing);
    }

    /**
     * Prepares the content for editing in IE versions older than version 8.
     *
     * Ensure that all empty blocklevel elements must contain a zero-width
     * whitespace.
     *
     * @param {number} i Unused
     * @param {HTMLElement} element
     */
    function prepareEditingInOldIE(i, element) {
      var $element = $(element);
      $element.filter(emptyBlocksSelector).append('\u200b');
      $element.children(NOT_ALOHA_BLOCK_FILTER).each(prepareEditingInOldIE);
    }

    /**
     * For a given DOM element, will make sure that it, and every one of its
     * child nodes, which is a block-level element ends with a <br> node.
     *
     * This ensures that a block is rendered visibly (with atleast one character
     * height).
     *
     * @param {number} i Unused
     * @param {HTMLElement} element
     */
    function propBlockElements(i, element) {
      var $element = $(element);
      $element.filter(emptyBlocksSelector).append('<br/>');
      $element.children(NOT_ALOHA_BLOCK_FILTER).each(propBlockElements);
    }

    return {

      handleContent: function (content, options) {
        if (!options) {
          return handleContentInner(content);
        }
        var $content = my.ContentHandlerUtils.wrapContent(content);
        if (!$content) {
          return content;
        }
        switch (options.command) {
          case 'initEditable':
            $content.children(NOT_ALOHA_BLOCK_FILTER)
                    .each(prepareForEditing);

            if ($.browser.msie && $.browser.version <= 7) {
              $content.children(NOT_ALOHA_BLOCK_FILTER)
                      .each(prepareEditingInOldIE);
            }
            break;
          case 'getContents':
            $content.children(NOT_ALOHA_BLOCK_FILTER)
                    .each(propBlockElements);
            break;
        }
        return $content.html();
      },

      /**
       * Transforms pasted content to make it safe and ready to be used in
       * Aloha Editables.
       *
       * @param {jQuery.<HTMLElement>|string} content
       * @return {string} Clean HTML
       */
      handleContentInner: function (content) {
        var $content = my.Utils.wrapContent(content);
        if (!$content) {
          return content;
        }

        // If an aloha-block is found inside the pasted content, no modify
        // should be made in the pasted content because it can be assumed
        // this is content deliberately placed by Aloha and should not be
        // cleaned.
        if ($content.find('.aloha-block').length) {
          return $content.html();
        }

        prepareTables($content);

        this.cleanLists($content);
        this.removeComments($content);
        this.unwrapTags($content);
        this.removeStyles($content);
        this.removeNamespacedElements($content);
        //this.transformLinks($content);

        var transformFormatting = true;

        if (my.Settings.contentHandler
          && my.Settings.contentHandler.handler
          && my.Settings.contentHandler.handler.generic
          && typeof my.Settings.contentHandler.handler.generic.transformFormattings !== 'undefinded'
          && !my.Settings.contentHandler.handler.generic.transformFormattings) {
          transformFormatting = false;
        }

        if (transformFormatting) {
          this.transformFormattings($content);
        }

        return $content.html();
      },

      /**
       * Cleans lists.
       * The only allowed children of ol or ul elements are li's. Everything
       * else will be removed.
       *
       * See http://validator.w3.org/check with following invalid markup for
       * example:
       * <!DOCTYPE html><head><title></title></head><ul><li>ok</li><ol></ol></ul>
       *
       * @param {jQuery.<HTMLElement>} $content
       */
      cleanLists: function ($content) {
        $content.find('ul,ol').find('>:not(li)').remove();
      },

      /**
       * Transform formattings
       * @param content
       */
      transformFormattings: function (content) {
        // find all formattings we will transform
        // @todo this makes troubles -- don't change semantics! at least in this way...

        var selectors = [],
          i
        ;

        for (i = 0; i < formattingTags.length; i++) {
          if (!isAllowedNodeName(formattingTags[i])) {
            selectors.push(formattingTags[i]);
          }
        }

        content.find(selectors.join(',')).each(function () {
          if (this.nodeName === 'STRONG') {
            // transform strong to b
            Aloha.Markup.transformDomObject($(this), 'b');
          } else if (this.nodeName === 'EM') {
            // transform em to i
            Aloha.Markup.transformDomObject($(this), 'i');
          } else if (this.nodeName === 'S' || this.nodeName == 'STRIKE') {
            // transform s and strike to del
            Aloha.Markup.transformDomObject($(this), 'del');
          } else if (this.nodeName === 'U') {
            // transform u?
            $(this).contents().unwrap();
          }
        });
      },

      /**
       * Transform links
       * @param content
       */
      transformLinks: function (content) {
        // find all links and remove the links without href (will be destination anchors from word table of contents)
        // aloha is not supporting anchors at the moment -- maybe rewrite anchors in headings to "invisible"
        // in the test document there are anchors for whole paragraphs --> the whole P appear as link
        content.find('a').each(function () {
          if (typeof $(this).attr('href') === 'undefined') {
            $(this).contents().unwrap();
          }
        });
      },

      /**
       * Remove all comments
       * @param content
       */
      removeComments: function (content) {
        var that = this;

        // ok, remove all comments
        content.contents().each(function () {
          if (this.nodeType === 8) {
            $(this).remove();
          } else {
            // do recursion
            that.removeComments($(this));
          }
        });
      },

      /**
       * Remove some unwanted tags from content pasted
       * @param content
       */
      unwrapTags: function (content) {
        var that = this;

        // Note: we exclude all elements (they will be spans) here, that have the class aloha-wai-lang
        // TODO find a better solution for this (e.g. invent a more generic aloha class for all elements, that are
        // somehow maintained by aloha, and are therefore allowed)
        content.find('span,font,div').not('.aloha-wai-lang').each(function () {
          if (this.nodeName == 'DIV') {
            // safari and chrome cleanup for plain text paste with working linebreaks
            if (this.innerHTML === '<br>') {
              $(this).contents().unwrap();
            } else {
              $(my.Markup.transformDomObject($(this), 'p').append('<br>')).contents().unwrap();
            }
          } else {
            $(this).contents().unwrap();
          }
        });
      },

      /**
       * Remove styles
       * @param content
       */
      removeStyles: function (content) {
        var that = this;

        // completely remove style tags
        content.children('style').filter(function () {
          return this.contentEditable !== 'false';
        }).remove();

        // remove style attributes and classes
        content.children().filter(function () {
          return this.contentEditable !== 'false';
        }).each(function () {
          $(this).removeAttr('style').removeClass();
          that.removeStyles($(this));
        });
      },

      /**
       * Remove all elements which are in different namespaces
       * @param content
       */
      removeNamespacedElements: function ($content) {
        // get all elements
        $content.find('*').each(function () {
          // try to determine the namespace prefix ('prefix' works for W3C
          // compliant browsers, 'scopeName' for IE)

          var nsPrefix = this.prefix ? this.prefix
              : (this.scopeName ? this.scopeName : undefined);
          // when the prefix is set (and different from 'HTML'), we remove the
          // element
          if ((nsPrefix && nsPrefix !== 'HTML') || this.nodeName.indexOf(':') >= 0) {
            var $this = $(this), $contents = $this.contents();
            if ($contents.length) {
              // the element has contents, so unwrap the contents
              $contents.unwrap();
            } else {
              // the element is empty, so remove it
              $this.remove();
            }
          }
        });
      }
    };

  })();

  return my;

}(EDITOR || {}));