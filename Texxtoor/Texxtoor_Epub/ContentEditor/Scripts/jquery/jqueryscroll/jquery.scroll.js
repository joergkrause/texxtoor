
(function ($) {

    // *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
    $.fn.JScroll = function () {

        var JScroll = function (object) {
            this.object = $(object)
        };
        JScroll.prototype = {
            // Variables
            jsonData: null,
            RedrawExecutor: null,
            scrollPanel: null,
            scrollContainer: null,
            scrollTrack: null,
            scrollBar: null,
            arrowUp: null,
            arrowDown: null,
            percentScrolled: 0,
            scrollPanelHeight: 0,
            scrollPanelMaxHeight: 0,
            scrollPosition: 0,
            scrollMaxPosition: 0,
            scrollMinPosition: 0,
            contentLength: 0,
            startTime: null,
            index: 0,
            isPgDwn: false,
            pgDwn: 0,
            isPgUp: false,
            pgUp: 0,
            isLoaded: false,
            isFirst: true,
            locations: null,
            currentItem: null,
            currentIndex: 0,
            isScrollVisible: false,
            // Functions
            init: function (settings) {
                var $this = this;
                if (settings) $.extend($.fn.JScroll.defaults, settings);
                var options = $.fn.JScroll.defaults;
                var classes = $.fn.JScroll.classes;
                // Init objects
                $this.RedrawExecutor = new RedrawExecutor({});
                $this.scrollPanel = $('<div/>', { class: classes.sp }).append($this.object.children());
                $this.scrollBar = $('<div/>', { class: classes.sb }).height(options.scrollHeight);
                $this.scrollTrack = $('<div/>', { class: classes.st }).append($this.scrollBar);
                $this.scrollContainer = $('<div/>', { class: classes.sc })
                                        .append($this.scrollPanel)
                                        .append($this.scrollTrack)
                                        .appendTo($this.object);

                $this.scrollPanelMaxHeight = $this.scrollContainer.height();
                if (options.showArrows) {
                    $this.arrowUp = $('<div/>', { class: classes.au }).bind('mousedown.JScroll', function () { $this.arrowScroll(-1, this); return false; });
                    $this.arrowDown = $('<div/>', { class: classes.ad }).bind('mousedown.JScroll', function () { $this.arrowScroll(1, this); return false; });
                    $this.scrollTrack.append($this.arrowUp).append($this.arrowDown);
                    $this.scrollTrack.css({
                        top: $this.arrowUp.height() + 'px',
                        bottom: $this.arrowDown.height() + 'px'
                    });

                }
                $this.scrollMaxPosition = $this.scrollTrack.innerHeight() - options.scrollHeight;
                // Init events
                $this.scrollTrack.bind('click.JScroll', function (e) {
                    var position = e.pageY;
                    if (options.showArrows) {
                        if (position - $(this).offset().top < 0) return false;
                        if (position - $(this).offset().top > $this.scrollMaxPosition + options.scrollHeight) return false;
                    }

                    $this.setScrollPosition(position - $(this).offset().top - options.scrollHeight / 2);
                });
                // $this.scrollTrack.hide();
                $(window).bind('resize.JScroll', function (e) {
                    $this.resize();
                });
                $this.scrollBar.bind('mousedown.JScroll', function (e) {
                    $('html').bind('dragstart.JScroll selectstart.JScroll', function () { return false; });
                    var startPosition = e.pageY - $this.scrollBar.position().top;
                    $('html').bind('mousemove.JScroll', function (e) {
                        $this.setScrollPosition(e.pageY - startPosition);
                    }).bind('mouseup.JScroll mouseleave.JScroll', function () {
                        $('html').unbind('dragstart.JScroll selectstart.JScroll mousemove.JScroll mouseup.JScroll mouseleave.JScroll');
                    });
                });
                $this.initMousewheel();
                $this.loadData();
                return $this;
            },
            arrowScroll: function (direction, arrow) {
                arrow = $(arrow).addClass('active').blur();
                var $this = this;
                var scrollTimeout;
                var options = $.fn.JScroll.defaults;
                var scroll = function () {
                    $this.scrollBy(direction * options.arrowButtonSpeed);
                    scrollTimeout = setTimeout(scroll, options.arrowRepeatFreq);
                };
                $('html').bind('mouseup.JScroll', function () {
                    arrow.removeClass('active');
                    scrollTimeout && clearTimeout(scrollTimeout);
                    scrollTimeout = null;
                    $('html').unbind('mouseup.JScroll');
                });
                scroll();
            },
            resize: function () {
                var $this = this;
                var options = $.fn.JScroll.defaults;
                var maxPos = $this.scrollMaxPosition;
                $this.scrollPanelMaxHeight = $this.scrollContainer.height();
                if (options.showArrows) {
                    $this.scrollTrack.css({
                        top: $this.arrowUp.height() + 'px',
                        bottom: $this.arrowDown.height() + 'px'
                    });
                }
                $this.scrollMaxPosition = $this.scrollTrack.innerHeight() - options.scrollHeight;
                $this.setScrollPosition($this.scrollMaxPosition * $this.percentScrolled);
                if ($(".fr-container").is(":visible"))
                    author.setHighlightStyle();
            },
            loadData: function (settings) {
                var $this = this;
                $.extend($.fn.JScroll.defaults, settings || {});
                var options = $.fn.JScroll.defaults;
                var p = 0;
                var l = 0;
                $this.locations = [];
                $this.jsonData = [];
                $this.currentIndex = 0;
                $this.contentLength = 0;
                for (var i = 0; i < options.json.length; i++) {
                    var item = {
                        id: options.json[i].Id,
                        length: options.json[i].Length
                    };
                    $this.jsonData.push(item);
                }
                $.each($this.jsonData, function (key, value) {
                    var len = 100;
                    if (this.length)
                        len = this.length;
                    $this.contentLength += len;

                });
                $.each($this.jsonData, function (key, value) {
                    var len = 100;
                    if (this.length)
                        len = this.length;
                    l = l + len / $this.contentLength;
                    $this.locations.push({
                        id: this.id,
                        position: p * 100,
                        percent: (l - p) * 100,
                        length: len
                    });
                    p += len / $this.contentLength;
                    l = p;

                });
                $this.currentItem = $this.locations[$this.currentIndex];
            },
            loadItems: function () {
                var $this = this;
                $this.startTime = new Date().getTime();
                $this.scrollPanel.empty();
                author.authorUI.resetScrollPosition();
            },
            loadSnippet: function (id, callback, time) {
                var $this = this;
                $.ajax({
                    url: author.serviceUrl.getWidget,
                    data: {
                        id: author.documentId,
                        chapterId: author.chapterId,
                        snippetId: id
                    },
                    type: "POST",
                    cache: false,
                    dataType: "html",
                    success: function (html) {
                        callback(html, time);
                    },
                    error: function (html, time) {
                    }
                });
            },
            scrollToSnippet: function (id) {
                var $this = this;
                $.each($this.locations, function () {
                    if (id == this.id) {
                        if ($this.isPgDwn) {
                            $this.isPgDwn = false;
                            $this.setScrollPosition(((this.position + (this.percent / $this.pgDwn)) * $this.scrollMaxPosition / 100) + 0.00001);

                        } else if ($this.isPgUp) {
                            $this.isPgUp = false;
                            $this.setScrollPosition(((this.position +  (this.percent * $this.pgUp)) * $this.scrollMaxPosition / 100) + 0.00001);
                        } else {
                            $this.setScrollPosition((this.position * $this.scrollMaxPosition / 100) + 0.00001);
                        }
                    }
                });
            },
            showLoader: function (msg) {
                var $this = this;
                $(".loader-msg").text(msg);
                $(".loader-layout").css({
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0
                }).fadeIn(200, "linear");
            },
            setScrollPosition: function (position) {
                var $this = this;
                $this.isLoaded = false;
                if (position < 0) { position = 0; }
                else if (position > $this.scrollMaxPosition) { position = $this.scrollMaxPosition; }
                $this.startTime = new Date().getTime();
                $this.scrollPosition = position;
                $this.scrollBar.css({ top: position + 'px' });
                //$this.setContentPosition();
                var method = function () { $this.setContentPosition(); };
                $this.RedrawExecutor.execute(method);
            },
            setContentPosition: function () {
                var $this = this;
                $this.percentScrolled = $this.scrollPosition / $this.scrollMaxPosition;
                $.each($this.locations, function (index) {
                    if ($this.percentScrolled * 100 >= this.position) {
                        $this.currentItem = this;
                        $this.currentIndex = index;
                    }
                });
                $this.scrollPanel.empty();
                $this.scrollPanelHeight = 0;
                var id = $this.currentItem.id;
               
                if (author.contentData[author.chapterId] != null && author.contentData[author.chapterId][id] != null) {
                    $this.appendFirstElement(author.contentData[author.chapterId][id]);
                } else {
                    author.setStatusBar("Loading content");
                    var t = new Date().getTime();
                    $this.loadSnippet(id, function (html, time) {
                        if (author.contentData[author.chapterId] == null)
                            author.contentData[author.chapterId] = new Array();
                        author.contentData[author.chapterId][id] = html;
                        if (time < $this.startTime) return;
                        $this.appendFirstElement(html);
                    }, t);
                }
                author.authorUI.saveScroll();
            },
            setElementPositionNext: function (element) {
                var $this = this;
                var top = $(".snippet-block:last").position().top + $(".snippet-block:last").outerHeight(true);
                element.css({
                    top: top + 'px'
                });
                return element.appendTo($this.scrollPanel);
            },
            setElementPositionPrev: function (element) {
                var $this = this;
                var top = $(".snippet-block:last").prev().position().top - element.outerHeight(true);
                element.css({
                    top: top + 'px'
                });
            },
            appendElement: function (index) {
                var $this = this;
                if ($this.isPgUp) {
                    if (index < 0) {
                        author.prepareEditor();
                        author.highlightSnippet();
                        if ($(".fr-container").is(":visible"))
                            $this.setSearchOption();
                        author.setStatusBar("&nbsp");
                        $this.setEditorCaret();
                        $this.isLoaded = true;
                        $this.isPgUp = false;
                        $this.scrollToSnippet(parseInt($(".snippet-block:last").find(".editableByTexxtoor").attr("data-item")));
                        return;
                    }
                    var id = $this.jsonData[index].id;
                    if ($this.scrollPanelHeight >= 0) {
                        if (author.contentData[author.chapterId] != null && author.contentData[author.chapterId][id] != null) {
                            var element = $(author.contentData[author.chapterId][id]).appendTo($this.scrollPanel).css({ opacity: 0 });
                            var editable = element.find(".editableByTexxtoor");
                            if (!$("#leftbar").is(":visible")) {
                                $(".flowButton").hide();
                            }
                            if (!$("#rightbar").is(":visible")) {
                                $(".naviButton").hide();
                            }
                            author.activateEditor(editable);
                            $this.setElementPositionPrev(element);
                            $this.scrollPanelHeight = $(".snippet-block:last").position().top;
                            $this.appendElement(index - 1);
                        } else {
                            if ($this.scrollPanelHeight >= 0)
                                author.setStatusBar("Loading content");
                            var t = new Date().getTime();
                            $this.loadSnippet(id, function (html, time) {
                                author.contentData[author.chapterId][id] = html;
                                if (time < $this.startTime) return;
                                var element = $(author.contentData[author.chapterId][id]).appendTo($this.scrollPanel).css({ opacity: 0 });
                                var editable = element.find(".editableByTexxtoor");
                                if (!$("#leftbar").is(":visible")) {
                                    $(".flowButton").hide();
                                }
                                if (!$("#rightbar").is(":visible")) {
                                    $(".naviButton").hide();
                                }
                                author.activateEditor(editable);
                                $this.setElementPositionPrev(element);
                                $this.scrollPanelHeight = $(".snippet-block:last").position().top;
                                $this.appendElement(index - 1);
                            }, t);
                        }
                    } else {
                        $this.pgUp = -$(".snippet-block:last").position().top / $(".snippet-block:last").outerHeight(true);
                        $this.scrollToSnippet(parseInt($(".snippet-block:last").find(".editableByTexxtoor").attr("data-item")));
                    }
                } else {
                    if (index > $this.jsonData.length - 1) {
                        if (-$(".snippet-block:last").position().top > $(".snippet-block:last").height() - $(".snippet-block:last").height() / 3) {
                            $(".snippet-block:last").css({
                                top: -$(".snippet-block:last").height() + $(".snippet-block:last").height() / 2 + 'px'
                            });
                        }
                        author.prepareEditor();
                        author.highlightSnippet();
                        if ($(".fr-container").is(":visible"))
                            $this.setSearchOption();
                        author.setStatusBar("&nbsp");
                        $this.setEditorCaret();
                        $this.isLoaded = true;
                        return;
                    }
                    var id = $this.jsonData[index].id;
                    if ($this.scrollPanelHeight < $this.scrollPanelMaxHeight) {
                        if (author.contentData[author.chapterId] != null && author.contentData[author.chapterId][id] != null) {
                            var element = $this.setElementPositionNext($(author.contentData[author.chapterId][id]));
                            var editable = element.find(".editableByTexxtoor");
                            if (!$("#leftbar").is(":visible")) {
                                $(".flowButton").hide();
                            }
                            if (!$("#rightbar").is(":visible")) {
                                $(".naviButton").hide();
                            }
                            author.activateEditor(editable);
                            $this.scrollPanelHeight = $(".snippet-block:last").position().top + $(".snippet-block:last").outerHeight(true);
                            $this.appendElement(index + 1);
                        } else {
                            if ($this.scrollPanelHeight < $this.scrollPanelMaxHeight)
                                author.setStatusBar("Loading content");
                            var t = new Date().getTime();
                            $this.loadSnippet(id, function (html, time) {
                                if (author.contentData[author.chapterId] == null)
                                    author.contentData[author.chapterId] = new Array();
                                author.contentData[author.chapterId][id] = html;
                                if (time < $this.startTime) return;
                                var element = $this.setElementPositionNext($(html));
                                var editable = element.find(".editableByTexxtoor");
                                if (!$("#leftbar").is(":visible")) {
                                    $(".flowButton").hide();
                                }
                                if (!$("#rightbar").is(":visible")) {
                                    $(".naviButton").hide();
                                }
                                author.activateEditor(editable);
                                $this.scrollPanelHeight = $(".snippet-block:last").position().top + $(".snippet-block:last").outerHeight(true);
                                $this.isScrollVisible = false;
                                $this.appendElement(index + 1);
                            }, t);
                        }
                    } else {
                        author.prepareEditor();
                        author.highlightSnippet();
                        if ($(".fr-container").is(":visible"))
                            $this.setSearchOption();
                        author.setStatusBar("&nbsp");
                        $this.setEditorCaret();
                    }
                }
                
            },
            setPgUpElements: function (element, id) {
                var $this = this;
                var index = 0;
                $.each($this.locations, function (i) {
                    if (id == this.id) {
                        index = i;
                    }
                });
                if ($this.locations[index - 1] == null) {
                    $this.isPgUp = false;
                    return;
                }
                $this.scrollPanel.empty();
                $this.scrollPanelHeight = 0;

                var currentElement = $(author.contentData[author.chapterId][id]).appendTo($this.scrollPanel);
                var editable = currentElement.find(".editableByTexxtoor");
                if (!$("#leftbar").is(":visible")) {
                    $(".flowButton").hide();
                }
                if (!$("#rightbar").is(":visible")) {
                    $(".naviButton").hide();
                }
                author.activateEditor(editable);
                currentElement.css({
                    top: $this.scrollContainer.height() + $this.pgUp - currentElement.height() + 'px'
                });
                $this.appendElement(index - 1);
            },
            setEditorCaret: function () {
                author.findCaretTarget($(".editableByTexxtoor[data-item='" + author.snippetId + "']"), author.isSetCaretUp, author.isSetCaretDown);
                author.isSetCaretDown = false;
                author.isSetCaretUp = false;
            },
            setSearchOption: function () {
                author.highlights = author.search($("#find input"));
                if (author.direction == -1)
                    author.activeSearchElement.position = author.hIndex = $('#sn_block-' + author.activeSearchElement.snippetId).find("span.highlight").length - 1;
                else if (author.direction == 1)
                    author.activeSearchElement.position = author.hIndex = 0;
                author.setHighlightStyle();
                this.isLoaded = true;
            },
            setContentPositionAfterResize: function () {
                var $this = this;
                var top = $("#sn_block-" + author.snippetId).position().top + $("#sn_block-" + author.snippetId).outerHeight(true);
               
                $("#sn_block-" + author.snippetId).nextAll(".snippet-block").each(function () {
                    $(this).css({
                        top: top + 'px'
                    });
                    top += $(this).outerHeight(true);
                });
                author.highlightSnippet();
            },
            appendFirstElement: function (e) {
                var $this = this;
                $this.scrollTrack.removeClass('active').blur();
                var currentElement = $(e).appendTo($this.scrollPanel);
                var editable = currentElement.find(".editableByTexxtoor");
                if (!$("#leftbar").is(":visible")) {
                    $(".flowButton").hide();
                }
                if (!$("#rightbar").is(":visible")) {
                    $(".naviButton").hide();
                }
                author.activateEditor(editable);
                var top = -(($this.percentScrolled * 100 - $this.currentItem.position) /
                                $this.currentItem.percent * currentElement.outerHeight(true));
                currentElement.css({
                    top: top + 'px'
                });
                $this.scrollPanelHeight = $(".snippet-block:last").position().top + $(".snippet-block:last").outerHeight(true);
                $this.appendElement($this.currentIndex + 1);
            },
            pageUp: function () {
                this.isPgUp = true;
                var id = parseInt($(".snippet-block:first").find(".editableByTexxtoor").attr("data-item"));
                this.pgUp = -$(".snippet-block:first").position().top;
                this.setPgUpElements($(".snippet-block:first"), id);
            },
            pageDown: function () {
                this.isPgDwn = true;
                var id = parseInt($(".snippet-block:last").find(".editableByTexxtoor").attr("data-item"));
                this.pgDwn = $(".snippet-block:last").outerHeight(true) / (this.scrollContainer.height() - $(".snippet-block:last").position().top);
                this.scrollToSnippet(id);
            },
            initMousewheel: function () {
                var $this = this;
                var options = $.fn.JScroll.defaults;

                $($this.scrollContainer).unbind("mousewheel.JScroll").bind("mousewheel.JScroll", function (event, delta, deltaX, deltaY) {
                    $this.scrollTrack.addClass('active').blur();
                    $this.scrollBy(-deltaY * options.mousewheelSpeed);
                    
                    return false;
                });

            },
            scrollBy: function (deltaY) {
                var $this = this;
                if ($this.contentLength == 0) return;
                var snippetHeight = $('#sn_block-' + $this.currentItem.id).height();
                if (snippetHeight == undefined) snippetHeight = $(".snippet-block:first").height();
                if (isNaN($this.percentScrolled)) $this.percentScrolled = 0;
                var destY = $this.percentScrolled * 100 + (deltaY) * (20.0 / snippetHeight) * $this.locations[$this.currentIndex].percent;
                if ($this.locations[$this.currentIndex].position > destY)
                    destY = $this.locations[$this.currentIndex].position - 0.0001;
                else if ($this.locations[$this.currentIndex].position + $this.locations[$this.currentIndex].percent < destY)
                    destY = $this.locations[$this.currentIndex].position + $this.locations[$this.currentIndex].percent + 0.0001;
                $this.setScrollPosition(destY * $this.scrollMaxPosition / 100);
            },
            setSettings: function (settings) {
                if (settings) $.extend($.fn.JScroll.defaults, settings);
            }
        };
        return new JScroll(this);
    };
    // *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
    $.fn.JScroll.defaults = {
        scrollHeight: 50,
        mousewheelSpeed: 5,
        arrowButtonSpeed: 5,
        arrowRepeatFreq: 1,
        showArrows: true,
        json: null
    };
    // *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
    $.fn.JScroll.classes = {
        sp: "scrollPanel",
        sc: "scrollContainer",
        st: "scrollTrack",
        sb: "scrollBar",
        au: "arrowUp",
        ad: "arrowDown"
    };
    // *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*
})(jQuery);