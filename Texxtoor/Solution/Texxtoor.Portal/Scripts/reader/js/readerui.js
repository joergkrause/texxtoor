function ReaderUI(reader, jQueryObj) {
  this.reader = reader;
  this.jqueryObj = jQueryObj;
}

ReaderUI.prototype = {
  reader: null,
  jqueryObj: null,
  currentBook: null,
  popupContentBlock: null,
  showCommentsDialog: null,
  addCommentDialog: null,
  bookReader: null,
  viewContainer: null,
  contentBody: null,
  verticalBar: null,
  track: null,
  scroll: null,
  prevPageY: null,
  arrowUp: null,
  arrowDown: null,
  lineHeight: 17,
  initEvent: function () {
    var $this = this;
    $(window).resize(function () {
      var body = $this.contentBody;
      $('.navigation-section').remove();
      $this.setPoint();
      //$this.renderBookmark();
      if ($this.currentBook.parentCommentsCache) {
        $this.renderComments($this.currentBook.parentCommentsCache);
      }
      var currentFragment = $this.contentBody.children('.book-fragment').first();
      var newScrollPos = $this.track.height() * $this.scroll.data('position');
      $this.scroll.data('setPosition')(newScrollPos);
      //body.data('setBodyFromSlider')(newScrollPos + Math.round($this.scroll.height() / 2));
    });
    this.scroll.click(function () { return false; });
    $(document).keydown(function (ev) {
      if (ev.keyCode == 38)
        $this.arrowUp.trigger('click');
      else if (ev.keyCode == 40)
        $this.arrowDown.trigger('click');
    });
    this.viewContainer.on('scroll', function () {
      var y = $(this).scrollTop();
      var t = $this.contentBody.height();
      var p = $this.track.height();
      $this.scroll.animate({ top: y / t * p });
      $("#comments-dialog-menu").hide();
      $("p").css('background-color', 'White');

      return false;
    });

    this.popupContentBlock.find('.btn-close').click(function () {
      $(this).closest('.content-block').animate({ top: '-100%' });
    });
    // global comment event handlers
    $("#comments-dialog-menu-add").click(function () {
      // forward dynamic data to the dialog
      $this.addCommentDialog.data('item', $("#comments-dialog-menu").data('item'));
      $this.addCommentDialog.data('cfi', $("#comments-dialog-menu").data('cfi'));
      $this.addCommentDialog.dialog('open');
    });
    $("#comments-dialog-menu-all").click(function () {
      $this.showCommentsFor($("#comments-dialog-menu").data('item'));
    });
    
    // prepare showCommentsDialog
    this.showCommentsDialog = $('#comments-dialog-show');
    
    this.showCommentsDialog.dialog({
      autoOpen: false,
      resizable: true,
      draggable: true,
      width: '655',
      height: 'auto',
      modal: false,
      show: 'scale',
      hide: 'scale',
      buttons: [
        {
            text: 'Add Comment',
            click: function () {
                $(this).dialog("close");
                $('#comments-dialog-add').dialog('open');
            }
        },
        { text: 'Close', click: function () { $(this).dialog("close"); } }
      ]
    });
    
    this.addCommentDialog = $('#comments-dialog-add');
    this.addCommentDialog.dialog({
      autoOpen: false,
      resizable: false,
      draggable: true,
      width: '450',
      height: 'auto',
      modal: false,
      show: 'scale',
      hide: 'scale',
      buttons: [
        { text: 'Add Comment', click: function () { $this.addComment(this); } },
        { text: 'Close', click: function () { $(this).dialog("close"); } }
      ],
      create: function (e, ui) {
        // attach internal handlers
        $('.comment-color-box').css('visibility', 'hidden');
        $('[name=comment-type]').on('change', function () {
          if ($(this).val() == "NOTE") {
            // PostIt is always private
            $('.comment-color-box').css('visibility', 'visible');
            $('#comment-keep-group').attr('disabled', 'disabled');
            $('#comment-keep-private').removeAttr('disabled');
            $('#comment-keep-author').attr('disabled', 'disabled');
          } else {
            $('.comment-color-box').css('visibility', 'hidden');
            $('#comment-keep-group').removeAttr('disabled');
            $('#comment-keep-private').removeAttr('disabled');
            $('#comment-keep-author').removeAttr('disabled');
          }
        });
        $('.comment-color-box-color').click(function () {
          $('.comment-color-box-color').css('border', '2px solid transparent');
          $(this).css('border', '2px outset silver');
          $('.comment-color-box').data('color', $(this).data('color'));
        });
      }
    });
    $("body").on('keydown', function (ev) {
      if (ev.keyCode == 27) {
        $("#comments-dialog-menu").hide();
        $("p").css('background-color', 'White');
      }
    });
  },
  initContentBody: function () {
    var $this = this;
    this.contentBody.data('loading', false);
    this.contentBody.data('setFragmentCall', false);
    this.contentBody.data('setPosition', function (position, success) {
      var TOP = 0;
      var BOTTOM = -($this.contentBody.height() - $this.viewContainer.height());
      var body = $this.contentBody;
      if (body.data('loading')) return;
      var viewContainer = $this.viewContainer;
      if (position > TOP && position != BOTTOM) {
        if ($.isFunction(body.data('onPrevFragment'))) {
          body.data('loading', true);
          body.css({ top: TOP });
          body.data('onPrevFragment')(body, success);
        }
        return;
      } else if (position < BOTTOM && position != TOP) {
        if ($.isFunction(body.data('onNextFragment'))) {
          body.data('loading', true);
          body.css({ top: BOTTOM });
          body.data('onNextFragment')(body, success);
        }
        return;
      }
      var prevTop = body.position().top;
      body.css({ top: position });
      var buff = 10;
      if (position > prevTop) {
        var lastFragment = body.find('.book-fragment').last();
        if (lastFragment.height() + buff < (body.height() - viewContainer.height()) + body.position().top)
          lastFragment.remove();
      } else {
        var firstFragment = body.find('.book-fragment').first();
        if (firstFragment.height() + buff < -body.position().top) {
          var tmpPosition = body.height() + body.position().top;
          firstFragment.remove();
          body.css({ top: -(body.height() - tmpPosition) });
        }
      }
      body.data('setSliderFromBody')();
    });

    this.contentBody.data('onPrevFragment', function (body, success) {
      var navItem = body.find('.book-fragment').first().data('NavigationItem');
      var prevItem = navItem.book.navigation.prevItem(navItem);
      if (prevItem) {
        var tmpPosition = body.height() + body.position().top;
        var lineHeight = $this.lineHeight * 3;
        tmpPosition += lineHeight;
        prevItem.getFragment(function (fragment) {
          $this.initContextMenu(fragment);
          $this.initSideMenu(fragment);
          $this.initFragmentReferences(fragment);
          $this.contentBody.prepend(fragment);
          $this.contentBody.css({ top: -($this.contentBody.height() - tmpPosition) });
          $this.contentBody.data('loading', false);
          if ($.isFunction(success))
            success(body, fragment);
        });
      } else {
        $this.contentBody.data('loading', false);
        if ($.isFunction(success))
          success(body, null);
      }
    });
    this.contentBody.data('onNextFragment', function (body, success) {
      var lineHeight = $this.lineHeight * 3;
      var navItem = body.find('.book-fragment').last().data('NavigationItem');
      var nextItem = navItem.book.navigation.nextItem(navItem);
      if (nextItem) {
        nextItem.getFragment(function (fragment) {
          $this.initSideMenu(fragment);
          $this.initFragmentReferences(fragment);
          $this.contentBody.append(fragment);
          $this.contentBody.css({ top: $this.contentBody.position().top - lineHeight });
          $this.contentBody.data('loading', false);
          if ($.isFunction(success))
            success(body, fragment);
        });
      } else {
        $this.contentBody.data('loading', false);
        if ($.isFunction(success))
          success(body, null);
      }
    });

    this.contentBody.data('setFragment', function (fragment, success) {
      var body = $this.contentBody;
      var viewContainer = $this.viewContainer;
      $this.initSideMenu(fragment);
      $this.initFragmentReferences(fragment);

      if (!body.data('setFragmentCall')) body.html(fragment);
      else body.append(fragment);

      body.data('setFragmentCall', true);
      if (body.height() <= viewContainer.height()) {
        var navItem = fragment.data('NavigationItem');
        var nextItem = navItem.book.navigation.nextItem(navItem);
        if (nextItem) {
          nextItem.getFragment(function (frag) {
            body.data('setFragment')(frag, success);
          });
        } else {
          body.data('setFragmentCall', false).css({ top: 0 });
          if ($.isFunction(success)) {
            success(body);
          }
        }
      } else {
        body.data('setFragmentCall', false).css({ top: 0 });
        if ($.isFunction(success)) {
          success(body);
        }
      }

      return body;
    });
    this.contentBody.data('setSliderFromBody', function () {
      var body = $this.contentBody;
      var fragments = $('.book-fragment', body);
      var bodyTop = body.position().top;

      for (var i = 0; i < fragments.length; ++i) {
        if ($(fragments[i]).height() > -bodyTop) {
          var fragment = $(fragments[i]);
          var scrollPoint = $('.play-order-' + fragment.data('NavigationItem').playOrder);
          var coeff = (-body.position().top - fragment.position().top) / fragment.height();
          $this.scroll.data('setPosition')((scrollPoint.position().top + coeff * scrollPoint.data('size')) - Math.round($this.scroll.height() / 2));
        }
      }
    });
  },
  initSideMenu: function (fragment) {
    var $this = this;
    $(fragment).find("div[id], table[id]").off('click');
    // each active fragment can invoke context functions
    $(fragment).find("div[id], table[id]").on('click', function (e) {
      var target = $(e.srcElement).closest("div[id],table[id]");
      //var xPosition = $(target).offset().left - $("#comments-dialog-menu").width() - 2;
      var xPosition = e.pageX;
      var yPosition = e.pageY;
      $("div[id],table[id]").css('border', '1px solid transparent');
      $(target).css('border', '1px solid #3c6e2d');
      $("#comments-dialog-menu").offset({ left: xPosition, top: yPosition });
      $("#comments-dialog-menu").data('item', target.attr('id'));
      // calculate the cfi
      var text = "";
      var cfi = "";
      if (window.getSelection) {
        text = window.getSelection().toString();
      } else if (document.selection && document.selection.type != "Control") {
        text = document.selection.createRange().text;
      }
      if (text) {
        cfi = target.text().indexOf(text) + ":" + text.length;
      }
      $("#comments-dialog-menu").data('cfi', cfi);
      $("#comments-dialog-menu").show();
      e.stopPropagation();
    });
    $(fragment).find("img.builder").click(function () {
      $("#comments-dialog-menu").hide();
      if ($(this).width() == 100) {
        var maxWidth = $(".content-body").width() + 90;
        $(this).css("width", "auto").css("height", "auto");
        $(this).css('right', '5px');
        $(this).css('max-width', maxWidth);
        $(this).css('overflow', 'scroll');
        $(this).css('position', 'relative');
        $(this).css('border', 'none');

        $(this).parent().next(".drawLine").width($(this).parent().next(".drawLine").width() - 140);
      } else {
        $(this).width(100).height(100);
        $(this).css('right', '-140px');
        $(this).css('position', 'absolute');
        $(this).css('border', 'solid 1px #4472C4');

        $(this).parent().next(".drawLine").width($(this).parent().next(".drawLine").width() + 140);
      }
    });
    // top menu bindings 
    $('#topmenu-bookmark-add').unbind('click');
    $('#topmenu-bookmark-add').click(function () {
      var navItem = $(fragment).data('NavigationItem');
      $this.createBookmark(new Bookmark(navItem, navItem.fragmentHref));
    });
    $('#topmenu-bookmark-del').unbind('click');
    $('#topmenu-bookmark-del').click(function () {
      var id = $(fragment).data('bookmarkId');
      $this.deleteBookmark(id);
    });
    $('#signOutMenu').unbind('click');
    $('#signOutMenu').click(function () {
      var loader = new Loader();
      loader.showFor($('body'));
      $this.reader.signOff(function () {
        loader.hideFor($('body'));
        location.reload(true);
      });
    });
  },
  initScroll: function () {
    var $this = this;
    this.scroll.data('setPosition', function (position, animate) {
      $this.scroll.data('position', position / $this.track.height());
      if (animate)
        $this.scroll.animate({ top: position });
      else
        $this.scroll.css({ top: position });
    });
  },
  initFragmentReferences: function (fragment) {
    var $this = this;
    var references = fragment.find('a');
    references.each(function () {
      var href = $(this).attr('href');
      if (href && href.search('http://') < 0) {
        $(this).click(function () {
          $this.scrollToFragmentHesh($(this).attr('href'));
          return false;
        });
      }
    });
  },
  move: function (event) {
    var scroll = event.data.scroll;
    var pos = scroll.position();
    var delta = event.data.prevPageY - event.pageY;
    event.data.prevPageY = event.pageY;
    var newTopPos = pos.top - delta;
    if (newTopPos >= 0 - scroll.height() / 2 && newTopPos <= event.data.track.height() - Math.round(scroll.height() / 2) + 1) {
      scroll.data('setPosition')(newTopPos);
    }
  },
  moveBody: function (event) {
    var body = event.data.contentBody;
    var delta = event.data.prevPageY - event.pageY;
    event.data.prevPageY = event.pageY;
    body.data('setPosition')(body.position().top - delta);
  },
  setPoint: function () {
    var $this = this;
    var navItems = this.currentBook.navigation.allNavigationItems;
    var allSize = 0;
    navItems.each(function (item) { allSize += item.size; });
    var position = 0;
    navItems.each(function (item) {
      var section = $('<div class="navigation-section" title="' + item.labelText + '"></div>');
      section.css({ top: position });
      section.data('NavigationItem', item);
      var size = Math.round($this.track.height() * (item.size / allSize));
      section.data('size', size);
      section.addClass('play-order-' + item.playOrder);
      position += size
      section.click(item, function (ev) {
        var pos = $(this).position();
        var coef = Math.round(($('.scroll').height() - $(this).height()) / 2);
        $('.scroll').data('setPosition')(pos.top - coef, 'animate');
        ev.data.getFragment(function (fragment) {
          $('.content-body').data('setFragment')(fragment).css({ top: 0 });
        });
        return false;
      });
      $this.track.append(section);
    });
  },
  remove: function () {
    this.viewContainer.remove();
  },
  buildContent: function () {
    var $this = this;
    var data = $this.buildContentJson();
    $("#divTree").jstree({
      'plugins': ['themes', 'ui', 'search', 'json_data', 'types'],
      'core': { animation: 0 },
      'themes': {
        'theme': 'texxtoor',
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
      "json_data": data
    }).bind("select_node.jstree", function (e, data) {
      var navItem = data.rslt.obj.data();
      $this.scrollToFragmentHesh(navItem.fragmentHref + navItem.hrefHash);
      data.rslt.obj.closest('.content-block').find('.btn-close').click();
    });
  },
  buildContentJson: function (navigationItem) {
    var $this = this;
    if (navigationItem) {
      if (navigationItem.children) {
        return $.map(navigationItem.children, function (item) {
          return {
            data: item.playOrder + '. ' + item.labelText,
            children: $this.buildContentJson(item),
            metadata: 'item'
          };
        });
      } else {
        return null;
      }
    } else {
      return {
        data: $.map(this.currentBook.navigation.children, function (item) {
          return {
            data: item.playOrder + '. ' + item.labelText,
            children: $this.buildContentJson(item),
            metadata: 'item'
          };
        })
      };
    }
  },
  scrollToFragment: function (playOrder) {
    var $this = this;
    var navItem = this.currentBook.navigation.getFragmentFromPlayOrder(playOrder);
    if (navItem) {
      navItem.getFragment(function (fragment) {
        $this.contentBody.data('setFragment')(fragment, function (body) {
          body.data('setSliderFromBody')();
        });
      });
    }
  },
  scrollToFragmentHesh: function (fragmentHref) {
    var $this = this;
    var hash;
    if (fragmentHref.indexOf('#') >= 0) {
      hash = fragmentHref.substring(fragmentHref.indexOf('#'), fragmentHref.length);
      fragmentHref = fragmentHref.substring(0, fragmentHref.indexOf('#'));
    }
    var navItem = this.currentBook.navigation.getFragmentFromHref(fragmentHref);
    navItem.getFragment(function (fragment) {
      var body = $this.contentBody;
      var element = fragment.find(hash).first();
      if (element.length > 0) {
        body.html(fragment);
        body.data('scrollTo')(element);
        body.data('setSliderFromBody')();
      } else {
        body.data('setFragment')(fragment);
        body.data('setSliderFromBody')();
      }
    });
  },
  renderBookmark: function () {
    var $this = this;
    $this.currentBook.getBookmarks(function (_bookmarks) {
      $('.fragment-bookmark').remove();
      var trackPoints = $('.navigation-section', $this.track);
      trackPoints.each(function () {
        var point = $(this);
        var bookmarks = _bookmarks.where(function (item) { return item.navigationItem.playOrder == point.data('NavigationItem').playOrder; });
        if (!bookmarks.isEmpty()) {
          bookmarks.each(function (bookmark) {
            var marker = $('<div class="fragment-bookmark"></div>');
            var markerLarge = $('<div class="fragment-bookmarkLarge"></div>');
            marker.data('href', bookmark.fragmentCfi);
            marker.click(function () {
              $this.scrollToFragmentHesh($(this).data('href'));
              return false;
            });
            marker.css({ top: point.position().top });
            markerLarge.css({ top: point.position().top });
            $this.track.append(marker);
            $this.contentBody.before(markerLarge);
          });
        }
      });
    });
  },
  createBookmark: function (bookmark) {
    this.currentBook.addBookmark(bookmark);
    this.renderBookmark();
  },
  deleteBookmark: function (bookmarkId) {
    this.currentBook.deleteBookmark(bookmarkId);
    this.renderBookmark();
  },
  showCommentsFor: function (navItemId) {
    var $this = this;
    this.showCommentsDialog.find('.comments-block-body').empty();
    var loader = new Loader();
    loader.showFor(this.showCommentsDialog);
    $this.getComments(navItemId, function (comments) {
      loader.hideFor($this.showCommentsDialog);
      var renderComments = function (parent, comments) {
        var navItemRef = this;
        comments.each(function (comment) {
          // handle specific comment functions
          var subject = comment.Subject;
          var color = "#EEEEEE";
          var confident = "";
          if (subject.match(/^COMM:/)) {
            color: '#fff';
            confident = subject.substr(5);
          }
          if (subject.match(/^NOTE:/)) {
            color = subject.substr(5);
            confident = "Private";
          }
          // create UI from template, hide template, then
          var commentHtml = $('.comment-body').clone();
          commentHtml.removeClass('comment-body');
          $('.comment-body').hide();
          // store original data
          commentHtml.data('comment', comment);
          // call children recursively
          $(commentHtml).find('#comment-id').val(comment.Id);
          $(commentHtml).find('#comment-body-owner').text(comment.Owner);
          $(commentHtml).find('#comment-body-color').css({ "background-color": color });
          $(commentHtml).find('#comment-body-confidelity').text(confident);
          $(commentHtml).find('#comment-body-text').text(comment.Content);
          $(commentHtml).css({ display: 'block' });
          parent.append(commentHtml);
          // here we need to append child elements
          if (!comment.Children.isEmpty()) {
              renderComments.call(navItemRef, commentHtml.find('.sub-comments'), comment.Children);
          }
        });
      };
      var parent = $this.showCommentsDialog.find('.comments-block-body');
      renderComments.call($this, parent, comments);
      parent.find('.reply-comment-button').click(function () {
          $(this).find('.create-comment-field').show();
          $(this).find('.create-comment-field').find(".PostReply").click(function () {
              $this.addComment(parent, $(this).parents().find("#comment-id").val());
          });
          $(this).find('.create-comment-field').find(".CancelReply").click(function () {
              $(this).parents().find('.create-comment-field').hide(); // QMM
          });
        // TODO: Add comment with defaults from parent and add as child comment
      });
      var dialogHeight = 120;
      if (comments.length > 0 && comments.length < 4) {
          dialogHeight = (comments.length * 100) + 190;
      } else {
          if (comments.length > 3) {
              dialogHeight = 430;
          }
          else {
              $this.showCommentsDialog.find('.comments-block').hide();
              $this.showCommentsDialog.find('.comment-body-reply-arrow').hide();
          }
      }
      $this.showCommentsDialog.dialog("option", "height", dialogHeight);
      $this.showCommentsDialog.dialog("open");
    });
  },
  addComment: function (ui, parentId) {
    var $this = this;
    var subject = $('[name=comment-type]:checked').val() + ":";
    if (subject == "NOTE:") {
      subject += $('.comment-color-box').data('color');
    } else {
      subject += $('[name=comment-keep]:checked').val();
    }
    var comment = new Comment();
    comment.Id = null;
    comment.BookId = $this.currentBook.bookId;
    comment.ParentId = parentId;
    comment.Subject = subject;
    if(parentId==null)
        comment.Content = $('#comment-value1').val();
    else
        comment.Content = $('#comment-value2').val();
    comment.FragmentCfi = $(ui).data('cfi');
    comment.NavigationItem = $(ui).data('item');
    $this.currentBook.addComment(comment, function () {
        $('.comments-block').show();
        $('.comment-body-reply-arrow').show();
        $(ui).dialog('close');
    });
  },
  getComments: function (navItemId, success) {
    var $this = this;
    $this.currentBook.getComments(navItemId, success);
  },
  renderComments: function (parentComments) {
    var $this = this;
    $('.fragment-comment').remove();
    var trackPoints = $('.navigation-section', $this.track);
    trackPoints.each(function () {
      var point = $(this);
      var comments = parentComments.where(function (item) { return item.FragmentCfi == point.data('NavigationItem').fragmentHref; });
      if (!comments.isEmpty()) {
        comments.each(function (comment) {
          var marker = $('<div class="fragment-comment" title="' + comment.Subject + '"></div>');
          marker.data('href', comment.FragmentCfi);
          marker.data('NavigationItem', point.data('NavigationItem'));
          marker.click(function () {
            $this.scrollToFragmentHesh($(this).data('href'));
            $this.showCommentsFor($(this).data('NavigationItem'));
            return false;
          });
          marker.css({ top: point.position().top });
          $this.track.append(marker);
        });
      }
    });
  },
  readBook: function (book) {
    var $this = this;
    this.currentBook = book;
    this.jqueryObj.children().remove();
    var viewer = $('.book-reader');
    this.bookReader = viewer;
    this.viewContainer = viewer.find('.view-container');
    this.contentBody = viewer.find('.content-body');
    this.verticalBar = viewer.find('.vertical-bar');
    this.track = viewer.find('.track');
    this.scroll = viewer.find('.scroll');
    this.popupContentBlock = viewer.find('.content-block');
    this.jqueryObj.append(viewer);
    if (book.navigation.children.length > 0) {
      book.navigation.children.first().getFragment(function (fragment) {
        $this.contentBody.data('setFragment')(fragment);
        // This code embeds expander at the end of div to expand it fully. QMM
        $(fragment).find("div.listing pre").each(function (index, value) {
          var html_org = $(this).html();
          var html_calc = '<span>' + html_org + '</span>'
          $(this).html(html_calc);
          var width = $(this).find('span:first').width();
          $(this).html(html_org);

          if ($(".content-body").width() < width) {
            $(this).closest("div").before("<span class='rightCircle'></span>").before("<span class='leftCircle'></span>");
          }
        });
        $(".rightCircle").click(function () {
          $(this).next().next("div").animate({ width: $(".content-body").width() + 229 }, 300).css("border", "solid 1px gray").css("box-shadow", "0 3px 3px #333").css("z-index", "2000");
          $(this).next().next("div").find("pre").css("overflow-x", "auto");
          $(this).hide();
          $(this).next().show(300);
        });
        $(".leftCircle").click(function () {
          $(this).next("div").css("width", $(".content-body").width()).css("border", "none").css("box-shadow", "none");
          $(this).hide();
          $(this).prev().show(300);
        });
        $(".larger").click(function () {
          var increaseWidth = $(".content-body").css("font-size").replace("px", "");
          increaseWidth = parseInt(increaseWidth) + 2;
          $(".content-body").css("font-size", increaseWidth + "px");
        });
        $(".smaller").click(function () {
          var decreaseWidth = $(".content-body").css("font-size").replace("px", "");
          decreaseWidth = parseInt(decreaseWidth) - 2;
          $(".content-body").css("font-size", decreaseWidth + "px");
        });
        // This code will link line with image...
        // Pass Scroll Parameters to angle for rotating....
        $(fragment).find(".figure").each(function () {
          $(this).append("<div class='drawLine'></div>");
          var widthLine = $(".content-body").width() + 120;
          $(this).find(".drawLine").css({ width: widthLine }); //.css("transform-origin", "12px 0px").css("transform", "rotate(2deg)");
        });
      });
    }
    this.setPoint();
    this.initEvent();
    this.initContentBody();
    this.initScroll();
    this.buildContent();
    this.renderBookmark();
    
    // QMM
    this.track.on("click", function () {
      $("#comments-dialog-menu").hide();
      $("p").css('background-color', 'White');
    });
    $(".navigation-section").click(function () {
      $("#comments-dialog-menu").hide();
      $("p").css('background-color', 'White');
      return false;
    });
  }
};
