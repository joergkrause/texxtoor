(function ($) {

  $.fn.replace = function (o) { return this.after(o).remove(); };

  /**
   * jQuery between Extension
   *
   * insert either html code, a dom object OR a jQuery object inside of an existing text node.
   * if the chained jQuery object is not a text node, nothing will happen.
   *
   * @param content HTML Code, DOM object or jQuery object to be inserted
   * @param offset character offset from the start where the content should be inserted
   */
  $.fn.between = function (content, offset) {
    var offSize, fullText;

    if (this[0].nodeType !== 3) {
      // we are not in a text node, just insert the element at the corresponding position
      offSize = this.children().size();
      if (offset > offSize) {
        offset = offSize;
      }
      if (offset <= 0) {
        this.prepend(content);
      } else {
        this.children().eq(offset - 1).after(content);
      }
    } else {
      // we are in a text node so we have to split it at the correct position
      if (offset <= 0) {
        this.before(content);
      } else if (offset >= this[0].length) {
        this.after(content);
      } else {
        fullText = this[0].data;
        this[0].data = fullText.substring(0, offset);
        this.after(fullText.substring(offset, fullText.length));
        this.after(content);
      }
    }
  };

  /**
   * Make the object contenteditable. Care about browser version (name of contenteditable attribute depends on it)
   */
  $.fn.contentEditable = function (b) {
    // ie does not understand contenteditable but contentEditable
    // contentEditable is not xhtml compatible.
    var $el = jQuery(this);
    var ce = 'contenteditable';

    // Check
    if (jQuery.browser.msie && parseInt(jQuery.browser.version, 10) == 7) {
      ce = 'contentEditable';
    }

    if (typeof b === 'undefined') {

      // For chrome use this specific attribute. The old ce will only
      // return 'inherit' for nested elements of a contenteditable.
      // The isContentEditable is a w3c standard compliant property which works in IE7,8,FF36+, Chrome 12+
      if (typeof $el[0] === 'undefined') {
        console.warn('The jquery object did not contain any valid elements.'); // die silent
        return undefined;
      }
      if (typeof $el[0].isContentEditable === 'undefined') {
        console.warn('Could not determine whether the is editable or not. I assume it is.');
        return true;
      }

      return $el[0].isContentEditable;
    }

    if (b === '') {
      $el.removeAttr(ce);
    } else {
      if (b && b !== 'false') {
        b = 'true';
      } else {
        b = 'false';
      }
      $el.attr(ce, b);
    }

    return $el;
  };

  var XMLSerializer = window.XMLSerializer;



  /**
   * jQuery Aloha Plugin.
   *
   * Makes the elements in a jQuery selection set Aloha editables.
   *
   * @return jQuery container of holding DOM elements that have been
   *         aloha()fied.
   * @api
   */
  $.fn.aloha = function () {
    var $elements = this;
    $elements.each(function (_, elem) {
      // this attribute controls the readonly function for regular editables
      var el = $(elem).find('[data-content-editable="true"]');
      if (!EDITOR.Core.isEditable(el)) {
        EDITOR.Editable.init($(el));
      }
    });
    EDITOR.trigger('aloha-ready');
    return $elements;
  };

  /**
   * jQuery Extension
   * new Event which is triggered whenever a selection (length >= 0) is made in
   * an Aloha Editable element
   */
  $.fn.contentEditableSelectionChange = function (callback) {
    var that = this;

    // update selection when keys are pressed
    this.keyup(function (event) {
      var rangeObject = EDITOR.Selection.getRangeObject();
      callback(event);
    });

    // update selection on doubleclick (especially important for the first automatic selection, when the Editable is not active yet, but is at the same time activated as the selection occurs
    this.dblclick(function (event) {
      callback(event);
    });

    // update selection when text is selected
    this.mousedown(function (event) {
      // remember that a selection was started
      that.selectionStarted = true;
    });

    jQuery(document).mouseup(function (event) {
      EDITOR.Selection.eventOriginalTarget = that;
      if (that.selectionStarted) {
        callback(event);
      }
      EDITOR.Selection.eventOriginalTarget = false;
      that.selectionStarted = false;
    });

    return this;
  };

  /**
   * Fetch the outerHTML of an Element
   * @version 1.0.0
   * @date February 01, 2011
   * @package jquery-sparkle {@link http://www.balupton/projects/jquery-sparkle}
   * @author Benjamin Arthur Lupton {@link http://balupton.com}
   * @copyright 2011 Benjamin Arthur Lupton {@link http://balupton.com}
   * @license MIT License {@link http://creativecommons.org/licenses/MIT/}
   * @return {String} outerHtml
   */
  $.fn.outerHtml = jQuery.fn.outerHtml || function () {
    var $el = jQuery(this),
      el = $el.get(0);
    if (typeof el.outerHTML != 'undefined') {
      return el.outerHTML;
    }
    try {
      // Gecko-based browsers, Safari, Opera.
      return (new XMLSerializer()).serializeToString(el);
    } catch (e) {
      try {
        // Internet Explorer.
        return el.xml;
      } catch (e2) { }
    }
  };

  $.fn.zap = function () {
    return this.each(function () {
      jQuery(this.childNodes).insertBefore(this);
    }).remove();
  };

  $.fn.textNodes = function (excludeBreaks, includeEmptyTextNodes) {
    var ret = [],
      doSomething = function (el) {
        var i, childLength;
        if ((el.nodeType === 3 && jQuery.trim(el.data) && !includeEmptyTextNodes) || (el.nodeType === 3 && includeEmptyTextNodes) || (el.nodeName == "BR" && !excludeBreaks)) {
          ret.push(el);
        } else {
          for (i = 0, childLength = el.childNodes.length; i < childLength; ++i) {
            doSomething(el.childNodes[i]);
          }
        }
      };

    doSomething(this[0]);

    return jQuery(ret);
  };

})(jQuery);

