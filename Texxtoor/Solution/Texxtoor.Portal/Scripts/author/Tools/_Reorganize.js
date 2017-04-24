function ReorganizeDlg(data) {
  this.options = data.options;
}

ReorganizeDlg.prototype = new BaseDlg();
ReorganizeDlg.prototype.constructor = ReorganizeDlg;
ReorganizeDlg.prototype.options = { linkTree: {}, serviceUrl: '' };

// Drag functions
ReorganizeDlg.prototype.stack = new Array();
ReorganizeDlg.prototype.temp = null;
  //takes an element and saves it's position in the sitemap.
  //note: doesn't commit the save until commit() is called!
  //this is because we might decide to cancel the move
ReorganizeDlg.prototype.saveState = function(item) {
  this.temp = { item: $(item), itemParent: $(item).parent(), itemAfter: $(item).prev() };
};
ReorganizeDlg.prototype.commit = function() {
  if (this.temp != null) {
    this.stack.push(this.temp);
    // TODO: Save to server
  }
};
//restores the state of the last moved item.
ReorganizeDlg.prototype.restoreState = function() {
  var h = this.stack.pop();
  if (h == null) return;
  if (h.itemAfter.length > 0) {
    h.itemAfter.after(h.item);
  } else {
    h.itemParent.prepend(h.item);
  }
  //checks the classes on the lists
  $('#sitemap li.sm2_liOpen').not(':has(li)').removeClass('sm2_liOpen');
  $('#sitemap li:has(ul li):not(.sm2_liClosed)').addClass('sm2_liOpen');
};

ReorganizeDlg.prototype.initializeData = function(id) {
  var $this = this;
  var chapterId = id;
  // we get a hierarchical structure from server, very similar to those used with jstree
  // we create a dynamic ul/li/dl/dt/dd tree from this and hook draggable code into this

  var init = function(linkTree) {
    $('#reorg-tree li').prepend('<div class="dropzone"></div>');
    $('#reorg-tree dl, #reorg-tree .dropzone').droppable({
      accept: '#reorg-tree li',
      tolerance: 'pointer',
      drop: function(e, ui) {
        var li = $(this).parent();
        var child = !$(this).hasClass('dropzone');
        if (child && li.children('ul').length == 0) {
          li.append('<ul/>');
        }
        if (child) {
          li.addClass('sm2_liOpen').removeClass('sm2_liClosed').children('ul').append(ui.draggable);
        } else {
          li.before(ui.draggable);
        }
        $('#reorg-tree li.sm2_liOpen').not(':has(li:not(.ui-draggable-dragging))').removeClass('sm2_liOpen');
        li.find('dl,.dropzone').css({ backgroundColor: '', borderColor: '' });
        this.commit();
      },
      over: function() {
        $(this).filter('dl').css({ backgroundColor: '#ccc' });
        $(this).filter('.dropzone').css({ borderColor: '#aaa' });
      },
      out: function() {
        $(this).filter('dl').css({ backgroundColor: '' });
        $(this).filter('.dropzone').css({ borderColor: '' });
      }
    });
    $('#reorg-tree li').draggable({
      handle: ' > dl',
      opacity: .8,
      addClasses: false,
      helper: 'clone',
      zIndex: 100,
      start: function(e, ui) {
        this.saveState(this);
      }
    });
    $('.sitemap_undo').click(this.restoreState);
    $(document).bind('keypress', function(e) {
      if (e.ctrlKey && (e.which == 122 || e.which == 26))
        this.restoreState();
    });
    $('.sm2_expander').on('click', function() {
      $(this).parent().parent().toggleClass('sm2_liOpen').toggleClass('sm2_liClosed');
      return false;
    });
    /*
     * { "options":
     *    { "linkTree ":
     *       [ 
     *         { "data":"Empfehlungen und Sicherheitshinweise",
     *           "attr": 
     *            { 
     *               "id" :"il-1062",
     *               "rel":"Section",
     *               "dataitem":"1062",
     *               "datatext":"Empfehlungen und Sicherheitshinweise"
     *            },
     *           "children": 
     *              [ 
     *                 { "data": "<p>Unsere Produkte sind FULL HTML</p>\\n\",
     *                   "attr": 
     *                    {
     *                       "id\":\"il-1063\",\"rel\":\"Text\",\"dataitem\":\"1063\",\"datatext\":\"Unsere Produkte sind  ...\"},
     *                   "children\":null
     *                    }
     *              ]
     *            },
     *          { "data ": "Zweites Kapitel\",\"attr\":{\"id\":\"il-63\",\"rel\":\"Section\",\"dataitem\":\"63\",\"datatext\":\"Zweites Kapitel\"},\"children\":[{\"data\":\"<p>Dies ist der zweite Text<\/p>\\n\",\"attr\":{\"id\":\"il-1064\",\"rel\":\"Text\",\"dataitem\":\"1064\",\"datatext\":\"Dies ist der zweite Text\\n\"},\"children\":null}]}]}}"
     * 
     * 
     */
    for (var i = 0; i < linkTree.length; i++) {
      var data = linkTree[i];
      var li = $("<li>").text('[' + data.attr.rel + '] ' + data.attr.datatext).data('item', data.attr.dataitem);
      if (data.children) {
        li.append($('<ul>').append($('<li>').text(data.children.length)));
      }
      $('#reorg-tree').append(li);
    }

  };

  init(this.options.linkTree);

};

ReorganizeDlg.prototype.getDialogHtml = function () {
  var $this = this;
  return $this.localize('' +
    '<div style="width:650px !important">' +
    '<p data-lc="Widgets" data-p="Reorganize_Text"></p>' +
    '<ul id="reorg-tree"></ul>' + 
    '<div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Reorganize_Tools_Btn_Undo" value="Undo" id="undoReorganize" />' +
    '<input type="button" data-lc="Widgets" data-p="Reorganize_Tools_Btn_Cancel" value="Cancel" id="closeReorganize" />' +    
    '</div></div>');
};