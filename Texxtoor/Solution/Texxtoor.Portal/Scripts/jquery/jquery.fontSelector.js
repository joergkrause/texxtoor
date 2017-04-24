/**
* Font selector plugin
* turns an ordinary input field into a list of web-safe fonts
* Usage: $('select').fontSelector();
*
* Author : James Carmichael
* Website : http://www.siteclick.co.uk
* License : MIT
*
* This script was found at: http://www.fullfatcode.com/2011/04/10/jquery-font-selector-version-2/
*
* Changed 4/6/12 by S Swett: had to customize to really get it to work well. Added options.
*/
jQuery.fn.fontSelector = function (options) {

  // S Swett added all these options 4/6/12; values below are defaults

  options = $.extend({
    inputElementId: "fontInput",
    popupElementId: "fontPopup",
    onChangeCallbackFunction: new Function(""),
    fontsArray: new Array(
    'Arial=Arial,Helvetica,sans-serif',
    'Arial Black=Arial Black,Gadget,sans-serif',
    'Comic Sans MS=Comic Sans MS,cursive',
    'Courier New=Courier New,Courier,monospace',
    'Georgia=Georgia,serif',
    'Impact=Impact,Charcoal,sans-serif',
    'Lucida Console=Lucida Console,Monaco,monospace',
    'Lucida Sans Unicode=Lucida Sans Unicode,Lucida Grande,sans-serif',
    'Palatino Linotype=Palatino Linotype,Book Antiqua,Palatino,serif',
    'Tahoma=Tahoma,Geneva,sans-serif',
    'Times New Roman=Times New Roman,Times,serif',
    'Trebuchet MS=Trebuchet MS,Helvetica,sans-serif',
    'Verdana=Verdana,Geneva,sans-serif')
  }, options);

  return this.each(function () {

    // Get input field
    var sel = this;

    // Add a ul to hold fontsArray
    var ul = $("<ul class='fontselector'></ul>");
    $('body').prepend(ul);
    $(ul).hide();

    jQuery.each(options.fontsArray, function (i, item) {

      // S Swett changed so that items are like this: "Font Title = Font, Fallback Font"
      // vs. this: "Font, Fallback Font" 4/6/12

      var itemParts = item.split("=");
      var fontTitle = itemParts[0];
      var fontNameWithFallback = itemParts[1];

      var anchorId = options.inputElementId + "a" + i;

      // S Swett added "id" attribute below 4/6/12
      $(ul).append('<li><a href="#" style="font-family: ' + fontNameWithFallback + '" id="' + anchorId + '" data-item="' + i + '" >' + fontTitle + '</a></li>');

      // Prevent real select from working
      $(sel).focus(function (ev) {

        ev.preventDefault();

        // Show font list
        $(ul).show();

        // Position font list
        $(ul).css({
          top: $(sel).offset().top + $(sel).height() + 4,
          left: $(sel).offset().left
        });

        // Blur field
        $(this).blur();
        return false;
      });

      // S Swett fixed selector below; was running way too often 4/6/12
      // $(ul).find('a').click(function() {
      $(ul).find('#' + anchorId).click(function () {
        var font = options.fontsArray[$(this).data('item')];

        // S Swett split font into fontTitle
        var fontTitle = font.split("=")[0];
        $(sel).find('option').text(fontTitle).css('font-family', font.split("=")[1]);
        $(ul).hide();
        options.onChangeCallbackFunction(); // S Swett added this callback function 4/6/12
        return false;
      });
    });

  });

}