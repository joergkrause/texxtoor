// Author: Joerg Krause, joerg@augmentedcontent.de

var DESIGNER = (function (my) {

  my.documentId = 0;
  my.snippetId = 0;
  my.serviceUrl = null;
  my.locationUrl = null;

  my.isSaved = false;
  my.isLoaded = false;

  /********************** Tools Mngmt **************************/
  my.showLoader = function (msg) {
    var $this = this;
    $(".loader-msg").text(msg);
    $(".loader-layout").css({
      top: 0,
      left: 0,
      right: 0,
      bottom: 0
    }).fadeIn(200, "linear");
  };
  my.hideLoader = function () {
    $(".loader-layout").fadeOut(200, "linear");
  };

  
  return my;
}(DESIGNER || {}));