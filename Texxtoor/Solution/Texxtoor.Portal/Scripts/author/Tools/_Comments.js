function CommentsDlg(data) {  
  this.options = data.options;
}

CommentsDlg.prototype = new BaseDlg();
CommentsDlg.prototype.constructor = CommentsDlg;
CommentsDlg.prototype.options = {};  
CommentsDlg.prototype.getDialogHtml = function () {
  var $this = this;
  return this.localize('' +
    '<div title="" style="min-width:650px !important">' +
    '<p data-lc="Widgets" data-p="Comments_Document_Text"></p>' +
    '<div id="commentAccordion">' +
    '<ul>' +
    '<li><a href="#comment-me" data-lc="Widgets" data-p="Comments_Document_Me"></a></li>' +
    '<li><a href="#comment-team" data-lc="Widgets" data-p="Comments_Document_Team"></a></li>' +
    '<li><a href="#comment-reader" data-lc="Widgets" data-p="Comments_Document_Reader"></a></li>' +
    '<li><a href="#comment-metadata" data-lc="Widgets" data-p="Comments_MetaData"></a></li>' +
    '</ul>' +
    
    '<div data-item="comment-me" id="comment-me">' +
    '<h3 data-lc="Widgets" data-p="Comments_Document_Me"></h3>' +
    '<label data-lc="Widgets" data-p="Comments_Me_Comments_Add"></label><br />' +
    '<input type="text" class="newComment" data-item="me" /><br>' +
    '<textarea class="newComment" data-item="me" rows="3" cols="55"></textarea><br>' +
    'Close Thread: <input type="checkbox" class="newComment" data-item="me" />' +
    '<input type="button" data-lc="Widgets" data-p="Comments_Tools_Btn_Add" value="Add" class="btnAddComment" data-item="me" style="float:right" />' +
    '<div data-lc="Widgets" data-p="Comments_Me_Comments_Existing"></div>' +
    '<p class="existingComments" data-item="me"></p>' +
    '</div>' +

    '<div data-item="team" id="comment-team">' +
    '<h3 data-lc="Widgets" data-p="Comments_Document_Team"></h3>' +
    '<label data-lc="Widgets" data-p="Comments_Team_Comments_Add"></label><br />' +
    '<input type="text" class="newComment" data-item="team" /><br>' +
    '<textarea class="newComment" data-item="team" rows="3" cols="55"></textarea><br>' +
    'Close Thread: <input type="checkbox" class="newComment" data-item="team" />' +
    '<input type="button" data-lc="Widgets" data-p="Comments_Tools_Btn_Add" value="Add" class="btnAddComment" data-item="team" style="float:right" />' +
    '<div data-lc="Widgets" data-p="Comments_Team_Comments_Existing"></div>' +
    '<p class="existingComments" data-item="team"></p>' +
    '</div>' +

    '<div data-item="reader" id="comment-reader">' +
    '<h3 data-lc="Widgets" data-p="Comments_Document_Reader"></h3>' +    
    '<label data-lc="Widgets" data-p="Comments_Reader_Comments_Add"></label><br />' +
    '<input type="text" class="newComment" data-item="reader" /><br>' +
    '<textarea class="newComment" data-item="reader" rows="3" cols="55"></textarea><br>' +
    '<span data-lc="Widgets" data-p="Comments_Reader_Close_Thread"></span>: <input type="checkbox" class="newComment" data-item="reader" />' +
    '<input type="button" data-lc="Widgets" data-p="Comments_Tools_Btn_Add" value="Add" class="btnAddComment" data-item="reader" style="float:right" />' +
    '<div data-lc="Widgets" data-p="Comments_Reader_Comments_Existing"></div>' +
    '<p class="existingComments" data-item="reader"></p>' +
    '</div>' +

    '<div data-item="metadata" id="comment-metadata">' +
    '<h3 data-lc="Widgets" data-p="Comments_MetaData"></h3>' +
    '<label data-lc="Widgets" data-p="Comments_MetaData_Hint"></label><br />' +
    '</div>' +


    '</div>' +
    '<div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Comments_Tools_Btn_Cancel" value="Cancel" id="btnCloseComments"/>' +
    '</div></div>');
};

