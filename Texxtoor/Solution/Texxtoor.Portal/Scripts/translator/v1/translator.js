// Author: Joerg Krause, joerg@augmentedcontent.de

var TRANSLATOR = (function (my) {

  my.documentId = 0;
  my.chapterId = 0;
  my.snippetId = 0;
  my.serviceUrl = {};
  my.locationUrl = {};

  my.isSaved = false;
  my.isLoaded = false;

  /************************************************************\
  |******************* Preperation & Init *********************|
  \************************************************************/
  my.initSnippetContainer = function () {
    var $this = this;
    $this.isLoaded = true;
    $this._attachEvents();
    $this._initMenus();
  }; 

  
  return my;
}(TRANSLATOR || {}));