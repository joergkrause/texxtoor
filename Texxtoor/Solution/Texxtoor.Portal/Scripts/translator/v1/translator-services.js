// Author: Joerg Krause, joerg@augmentedcontent.de

var TRANSLATOR = (function (my) {

  my.loadDocument = function () {
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.loadDialog,
      data: {
        id: $this.documentId,
        dialog: 'internal'
      },
      cache: false,
      dataType: 'json',
      contentType: 'application/json',
      success: function (data) {
        var treeData = JSON.parse(data);
        loadTree(treeData.options.linkTree);
      }
    });
  }

  my.getTranslation = function (id) {
    var from = "de";
    var to = "en";
    var engine = "Google";
    var $this = this;
    $.ajax({
      url: $this.serviceUrl.getTranslation,
      data: {
        id: id,
        fromLanguage: from,
        toLanguage: to,
        engine: engine
      },
      cache: false,
      dataType: 'json',
      contentType: 'application/json',
      success: function (d) {
        $('#translatedText').html(d);
      },
      error: function(d) {
        alert('Something went wrong: ' + d.statusText);
      }
    });
  }

  var loadTree = function (data) {

    var selectNode = function (i, t, r) {
      // show appropriate editor
      $('.widget').hide();
      $('[data-type="' + r.toLowerCase() + '"]').show();
      $('#foundtype').text(r);
      $('#currentText').text('Loading ' + t);
      // Load Current Text in both 
      $.ajax({
        url: my.serviceUrl.getSnippet,
        data: { id: i },
        dataType: 'json',
        contentType: 'application/json',
        success: function (data) {
          var d = JSON.parse(data);
          switch (r.toLowerCase()) {
            case "text":
              $('#currentText').html(d.content);
              $('#translatedText').html(d.content);
              $('#currentCounter').text(d.content.length);
              $('#translatedCounter').text(d.content.length);
              break;
            case "section":
              $('#currentSection').val(d.content);
              $('#translatedText').text(d.content);
              $('#currentCounter').text(d.content.length);
              $('#translatedCounter').text(d.content.length);
              break;
            case "image":
              $('#currentCaption').val(d.title);
              $('#translatedText').text(d.title);
              $('#currentCounter').text(d.title.length);
              $('#translatedCounter').text(d.title.length);
              break;
            case "sidebar":
              $('#currentText').text(d.content);
              $('#translatedText').text(d.content);
              $('#currentCounter').text(d.content.length);
              $('#translatedCounter').text(d.content.length);
              break;
            case "table":
              $('#currentText').text(d.content);
              $('#translatedText').text(d.content);
              $('#currentCounter').text(d.content.length);
              $('#translatedCounter').text(d.content.length);
              break;
            default:
              alert('Snippet Not supoported');
              break;
          }
          // TODO: Only if autotrans and not translated
          my.getTranslation(i);
        }
      });
    }

    $('#opus-tree').jstree({
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
        'data': data
      }
    }).on('select_node.jstree', function (e, d) {
      var node = $(d.rslt.obj);
      var r = node.attr('rel');
      var t = node.attr('datatext');
      var i = node.attr('dataitem');
      selectNode(i, t, r);      
    }).on('search.jstree', function (e, data) {
      $('#searchhits').text(data.rslt.nodes.length);
      if (data.rslt.nodes.length > 0) {
        var node = $(data.rslt.nodes[0]);
        $(node).get(0).scrollIntoView();
      }
    }).on('loaded.jstree', function () {
      $('#opus-tree').jstree('open_all');
    });
  }

  return my;
}(TRANSLATOR || {}));