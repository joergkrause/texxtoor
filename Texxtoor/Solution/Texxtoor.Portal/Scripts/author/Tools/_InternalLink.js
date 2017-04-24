function InternalLinkDlg(data) {
  this.options = data.options;
}

InternalLinkDlg.prototype = new BaseDlg();
InternalLinkDlg.prototype.constructor = InternalLinkDlg;
InternalLinkDlg.prototype.options = { linkTree: {} };
InternalLinkDlg.prototype.selectNode = function (id) {};
InternalLinkDlg.prototype.initializeData = function () {
  var $this = this;
  $('#il-tree').jstree({
    'plugins': ['themes', 'ui', 'search', 'json_data', 'types'],
    'core': { animation: 0 },
    'themes': {
      'theme': 'texxtoor-author',
      'dots': false,
      'icons': true
    },
    'types': {
      'valid_children': ['none'],
      'types': {
        'Section': {
          'icon': { 'image': '/App_Sprites/Editor/newspaper_16.png' },
          'valid_children': ['Section', 'Text', 'Table', 'Figure', 'Listing', 'Sidebar']
        },
        'Text': {
          'icon': { 'image': '/App_Sprites/Editor/text_16.png' },
          'valid_children': ['none']
        },
        'Listing': {
          'icon': { 'image': '/App_Sprites/Editor/code_16.png' },
          'valid_children': ['none']
        },
        'Image': {
          'icon': { 'image': '/App_Sprites/Editor/photo_landscape_16.png' },
          'valid_children': ['none']
        },
        'Sidebar': {
          'icon': { 'image': '/App_Sprites/Editor/toc_16.png' },
          'valid_children': ['none']
        },
        'Table': {
          'icon': { 'image': '/App_Sprites/Editor/table2_16.png' },
          'valid_children': ['none']
        }
      }
    },
    'search': {
      'case_insensitive': true
    },
    'json_data': {
      'data': $this.options.linkTree
    }
  }).bind('select_node.jstree', function(e, data) {
    var node = $(data.rslt.obj);
    var r = node.attr('rel');
    var t = node.attr('datatext');
    var i = node.attr('dataitem');
    $this.selectNode(i, t);
    $('input[name=captionvalue]').attr('data-item', i).attr('data-type', r).attr('value', t);
    $('#foundtype').text(r);
  }).bind('search.jstree', function (e, data) {
    $('#searchhits').text(data.rslt.nodes.length);
    if (data.rslt.nodes.length > 0) {
      var node = $(data.rslt.nodes[0]);
      $(node).get(0).scrollIntoView();
    }
  }); 
};
InternalLinkDlg.prototype.getDialogHtml = function () {
  var $this = this;
  return $this.localize('' +
    '<div>' +
    '<span class="dialog_error" data-error="noLink">Links need a text selection to insert properly. Select appropriate text, and insert again.</span>' +
    '<span class="dialog_error" data-error="noInsert">Cannot insert link. Select text for link.</span>' +
    '<div id="internalLinkForm">' +
    '<p data-lc="Widgets" data-p="InternalLink_Tool_Text"></p>' +
    '<span class="caption" data-lc="Widgets" data-p="InternalLink_Tool_Caption">: </span>' +
    '<input type="text" name="captionvalue" data-item="" data-type="" style="width:400px" />&nbsp;(<span id="foundtype">n/a</span>)' +
    '<span class="caption" data-lc="Widgets" data-p="InternalLink_Tool_Search">: </span>' +
    '<input type="text" name="searchvalue" style="width:200px" />&nbsp;(<span id="searchhits">?</span>)' +
    '<div id="internalLinkList">' +
    '<div id="il-tree"></div>' +
    '</div></div>' +
    '<div class="buttons">' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Ok" value="Ok" id="btnAddlink" />' +
    '<input type="button" data-lc="Widgets" data-p="Image_Tools_Btn_Cancel" value="Cancel" id="btnCloselink"/>' +
    '</div></div>');
};