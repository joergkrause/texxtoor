function AuthorUI() {
    this.isInitialized = false;
}

AuthorUI.prototype = {
    isInitialized: null,
    init: function () {
        if (this.isInitialized) return;
        this.closeAllPanes();
        // Res/Image   
        this.isInitialized = true;
    },
    createLinkDialogs: function () {
        $(".close-popup").click(function () {
            $(this).closest('.dialog-popup').hide();
            $(this).closest('.dialog-popup').find('a').unbind('click');
            $('.popup-layout').hide();
        });
        
        $('.popup-layout').hide();
        $('.internalLinkDialog').hide();
        $('.externalLinkDialog').hide();
        $('.imageCropDialog').hide(); 
        $('.imageColorsDialog').hide();
        $("#il-tree").jstree({ "plugins": ["themes", "html_data", "ui"],
            "themes": {
                "theme": "texxtoor-author",
                "dots": false,
                "icons": true
            }
        });
    },
    closeAllPanes: function () {
        // placeholder function
    },
    saveScroll: function () {
        $.cookie("ScrollPosition", author.jscroll.percentScrolled);
        $.cookie("Chapter", author.chapterId);
    },

    resetScrollPosition: function () {
        var scrollPosition = $.cookie("ScrollPosition");
        var chapterId = $.cookie("Chapter");
        if (typeof scrollPosition != "undefined" && chapterId == author.chapterId) {
            author.jscroll.setScrollPosition(author.jscroll.scrollMaxPosition * scrollPosition);
        } else {
            author.jscroll.setScrollPosition(0);
        }
    },
    destroyScrollPosition: function () {
        $.cookie("ScrollPosition", null);
    },

    /************************************************************\
    |***************** Internal UI functions ********************|
    \************************************************************/

    // Comments
    hideCommentDialogs: function () {
        $('.commentDialog').each(function () {
            $(this).html('');
            $(this).hide();
        });
    },
    activateCommentBtn: function (txt) {
        if ($(txt).val().length > 2) {
            $(txt).next('input').removeAttr('disabled');
        } else {
            $(txt).next('input').attr('disabled', 'disabled');
        }
    }

}