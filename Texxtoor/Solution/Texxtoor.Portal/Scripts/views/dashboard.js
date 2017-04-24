function Dashboard(action) {
  this.action = action;
}

Dashboard.prototype = {
  action: {
    addUrl: '',
    removeUrl: '',
    getUrl: ''
  },

  assignJumpTargets: function() {
    $(document).on('click', '[data-jump]', function() {
      jumpAbsolute($(this).data('jump'));
    });
  },

  addKeyword: function () {
    var $this = this;
    if (!$('#keyword').val()) {
      $('div.introform[data-item="reader"] div.error').text("Provide a search value");
    } else {
      $.ajax({
        url: this.action.addUrl,
        data: {
          Keyword: $('#keyword').val(),
          Stage: $('select[name=level]').val(),
          Target: $('select[name=target]').val()
        },
        type: 'POST',
        cache: false,
        success: function() {
          $this.getKeywords();
        },
        error: function() {

        }
      });
      $('div.introform[data-item="reader"] div.error').text("");
    }
    return false;
  },

  removeKeyword: function (e) {
    var $this = this;
    var id = $(e.srcElement).data('id') || $(e.srcElement).parents('[data-id]').data('id');
    $.ajax({
      url: $this.action.removeUrl,
      data: {
        id: id
      },
      type: 'POST',
      dataType: 'json',
      cache: false,
      success: function() {
        $this.getKeywords();
      },
      error: function () {
        toastr.error('');
      }
    });
    return false;
  },

  getKeywords: function () {
    var $this = this;
    $.ajax({
      url: this.action.getUrl,
      type: 'GET',
      dataType: 'json',
      cache: false,
      success: function(data) {  
        if (data.data == null || data.data.length == 0) {
          $('#keywordListEmpty').show();
          $('#keywordList').hide();
        } else {
          $('#keywordListEmpty').hide();
          $('#keywordList').show();
          var d = $('#keywordList');
          d.empty();
          for (var i in data.data) {
            var pm = data.data[i];
            $('input[name=keywords]').val($('input[name=keywords]').val() + "," + pm.Keyword);
            $('input[name=stage]').val($('input[name=stage]').val() + "," + pm.Stage);
            $('input[name=target]').val($('input[name=target]').val() + "," + pm.Target);
            $('<a>')
              .attr('href', '#')
              .attr('data-id', pm.Id)
              .addClass('matrix-keywords')
              .click(function(e) { return $this.removeKeyword(e); })
              .html(pm.Keyword + "(<i>Level</i>: " + pm.Stage + "; <i>Usage</i>:" + pm.Target + ")")
              .appendTo(d)
              .append('<br>');
          }
        }
      },
      error: function() {

      }
    });
  }
}