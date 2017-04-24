/*!
* Aloha Editor
* Author & Copyright (c) 2012-2013 Gentics Software GmbH
* aloha-sales@gentics.com
* Licensed unter the terms of http://www.aloha-editor.com/license.html
*
* @overview
* Utility functions for content handling.
*/
var EDITOR = (function (my) {

  my.ContentHandlerUtils = (function () {

    /**
     * A genericn entry point for current content handler
     **/
    function handleContent(content, options, obj) {
      return options.contenthandler.handleContent(content, options, obj);
    }
    
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
      var trimmed = $.trim(html);
      if (!trimmed) {
        return false;
      }
      var node = $('<div>' + trimmed + '</div>')[0];
      var containsSingleP = node.firstChild === node.lastChild
                         && 'p' === node.firstChild.nodeName.toLowerCase();
      if (containsSingleP) {
        var kids = node.firstChild.children;
        return (kids && 1 === kids.length &&
            'br' === kids[0].nodeName.toLowerCase());
      }
      return false;
    }

    /**
      * This assures that regular content is always wrapped in at least one paragraph.
      * Always returns a jQuery object.
      *
      * @param {string} html Markup
      **/
    function wrapContent(content) {
      if (typeof content === 'string') {        
        return $('<p>').append(content);
      }
      return $('<p>').append(content);
    }

    return {
      handleContent: handleContent,
      wrapContent: wrapContent,
      isProppedParagraph: isProppedParagraph
    };
    
  })();
  
  return my;
  
}(EDITOR || {}));